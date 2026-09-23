-- StarX: أمان دوال الترخيص السبعة + RLS + rate limit
-- شغّل في Supabase SQL Editor بعد مراجعة أي نسخة سابقة.
-- ملاحظة: هذا النص افتراضي لم يكن متاحاً وقت التدقيق الأول — راجعه
-- وعِدل منطق المفاتيح/الجداول بما يطابق قاعدة بياناتك الحالية.

-- ========== 0) جداول أساسية (إن لم تكن موجودة) ==========
create table if not exists public.licenses (
  key         text primary key,
  plan        text not null default 'pro' check (plan in ('pro', 'owner')),
  is_active   boolean not null default true,
  max_devices int not null default 1,
  devices     int not null default 0,
  expires_at  timestamptz,
  note        text,
  created_at  timestamptz not null default default timezone('utc', now()),
  created_by  text
);

create table if not exists public.devices (
  hwid        text primary key,
  key         text references public.licenses(key) on delete set null,
  device_name text,
  plan        text not null default 'free',
  last_seen_at timestamptz not null default default timezone('utc', now()),
  is_online   boolean not null default false
);

create index if not exists devices_key_idx on public.devices (key);
create index if not exists devices_seen_idx on public.devices (last_seen_at desc);

-- ========== 1) RLS: لا قراءة/كتابة مباشرة للـ anon ==========
alter table public.licenses enable row level security;
alter table public.devices enable row level security;

-- لا سياسات قراءة/كتابة مباشرة — كل الوصول عبر SECURITY DEFINER functions
revoke all on public.licenses from anon, authenticated;
revoke all on public.devices from anon, authenticated;
grant usage on schema public to anon, authenticated;

-- ========== 2) rate limit بسيط في قاعدة البيانات ==========
create table if not exists public.rpc_rate_limits (
  bucket_key text primary key,
  count      int not null default 0,
  window_start timestamptz not null default default timezone('utc', now())
);

alter table public.rpc_rate_limits enable row level security;
revoke all on public.rpc_rate_limits from anon, authenticated;

create or replace function public._rate_limit(p_key text, p_max int, p_window_seconds int)
returns boolean
language plpgsql
security definer
set search_path = public
as $$
declare
  w timestamptz := default timezone('utc', now()) - make_interval(secs => p_window_seconds);
  c int;
begin
  insert into public.rpc_rate_limits (bucket_key, count, window_start)
  values (p_key, 1, default timezone('utc', now()))
  on conflict (bucket_key) do update set
    count = case when public.rpc_rate_limits.window_start < w then 1
                 else public.rpc_rate_limits.count + 1 end,
    window_start = case when public.rpc_rate_limits.window_start < w
                        then default timezone('utc', now())
                        else public.rpc_rate_limits.window_start end
  returning count into c;
  return c <= p_max;
exception when others then
  return true; -- لا نحجب العملية بسبب فشل عدّاد
end;
$$;

revoke execute on function public._rate_limit(text, int, int) from public, anon, authenticated;
grant execute on function public._rate_limit(text, int, int) to postgres, service_role;

-- ========== 3) دوال الترخيص ==========

-- 3.1 validate_license
create or replace function public.validate_license(p_key text, p_hwid text)
returns json
language plpgsql
security definer
set search_path = public
as $$
declare
  lk public.licenses%rowtype;
  dev_count int;
begin
  if p_key is null or length(trim(p_key)) < 8 then
    return json_build_object('ok', false, 'reason', 'invalid');
  end if;
  if not public._rate_limit('validate:' || left(coalesce(p_hwid, 'x'), 64), 30, 60) then
    return json_build_object('ok', false, 'reason', 'network');
  end if;

  select * into lk from public.licenses where key = trim(p_key) for update;
  if not found then
    return json_build_object('ok', false, 'reason', 'invalid');
  end if;
  if not lk.is_active then
    return json_build_object('ok', false, 'reason', 'revoked');
  end if;
  if lk.expires_at is not null and lk.expires_at < default timezone('utc', now()) then
    return json_build_object('ok', false, 'reason', 'expired');
  end if;

  -- حد الأجهزة
  select count(*) into dev_count from public.devices
    where key = trim(p_key) and hwid <> coalesce(p_hwid, '');
  if coalesce(p_hwid, '') <> '' then
    if not exists (select 1 from public.devices where hwid = p_hwid) and dev_count >= lk.max_devices then
      return json_build_object('ok', false, 'reason', 'device_limit');
    end if;
  end if;

  -- تحديث/إدراج الجهاز
  insert into public.devices (hwid, key, device_name, plan, last_seen_at, is_online)
  values (left(p_hwid, 64), trim(p_key), null, lk.plan, default timezone('utc', now()), true)
  on conflict (hwid) do update set
    key = excluded.key,
    plan = excluded.plan,
    last_seen_at = excluded.last_seen_at,
    is_online = true;

  update public.licenses set devices = (
    select count(*) from public.devices where key = trim(p_key)
  ) where key = trim(p_key);

  return json_build_object(
    'ok', true,
    'reason', 'active',
    'plan', lk.plan,
    'expires_at', case when lk.expires_at is null then null
                       else to_char(lk.expires_at at time zone 'utc', 'YYYY-MM-DD"T"HH24:MI:SS"Z"') end
  );
exception when others then
  return json_build_object('ok', false, 'reason', 'invalid');
end;
$$;

-- 3.2 release_license
create or replace function public.release_license(p_key text, p_hwid text)
returns json
language plpgsql
security definer
set search_path = public
as $$
begin
  if not public._rate_limit('release:' || left(coalesce(p_hwid, 'x'), 64), 30, 60) then
    return json_build_object('ok', false);
  end if;
  -- لا نحرر إلا إن كان الجهاز مربوطاً بهذا المفتاح
  update public.devices set key = null, plan = 'free', is_online = false
    where hwid = left(coalesce(p_hwid, ''), 64) and key = trim(coalesce(p_key, ''));
  update public.licenses set devices = (
    select count(*) from public.devices where key = trim(p_key)
  ) where key = trim(p_key);
  return json_build_object('ok', true);
exception when others then
  return json_build_object('ok', false);
end;
$$;

-- 3.3 ping
create or replace function public.ping(p_key text, p_hwid text)
returns json
language plpgsql
security definer
set search_path = public
as $$
begin
  if not public._rate_limit('ping:' || left(coalesce(p_hwid, 'x'), 64), 60, 60) then
    return json_build_object('ok', false);
  end if;
  update public.devices
    set last_seen_at = default timezone('utc', now()), is_online = true,
        key = case when key is null then trim(coalesce(p_key, '')) else key end
    where hwid = left(coalesce(p_hwid, ''), 64);
  return json_build_object('ok', true);
exception when others then
  return json_build_object('ok', false);
end;
$$;

-- 3.4 device_ping
create or replace function public.device_ping(
  p_hwid text, p_device_name text default null,
  p_plan text default 'free', p_key text default null)
returns json
language plpgsql
security definer
set search_path = public
as $$
begin
  if p_hwid is null or length(trim(p_hwid)) = 0 then
    return json_build_object('ok', false);
  end if;
  if not public._rate_limit('dp:' || left(p_hwid, 64), 60, 60) then
    return json_build_object('ok', false);
  end if;
  insert into public.devices (hwid, key, device_name, plan, last_seen_at, is_online)
  values (left(p_hwid, 64),
          case when p_key is not null and length(trim(p_key)) >= 8 then trim(p_key) else null end,
          left(coalesce(p_device_name, ''), 120),
          left(coalesce(p_plan, 'free'), 16),
          default timezone('utc', now()), true)
  on conflict (hwid) do update set
    key = case when excluded.key is not null then excluded.key else devices.key end,
    device_name = coalesce(excluded.device_name, devices.device_name),
    plan = excluded.plan,
    last_seen_at = excluded.last_seen_at,
    is_online = true;
  return json_build_object('ok', true);
exception when others then
  return json_build_object('ok', false);
end;
$$;

-- 3.5 create_key (مالك فقط)
create or replace function public.create_key(
  p_owner_key text, p_days int default 0,
  p_note text default '', p_max_devices int default 1)
returns json
language plpgsql
security definer
set search_path = public
as $$
declare
  ok_owner boolean;
  nk text;
  exp timestamptz;
  tries int := 0;
begin
  if p_owner_key is null or length(trim(p_owner_key)) < 8 then
    return json_build_object('ok', false, 'error', 'forbidden');
  end if;
  if not public._rate_limit('owner:' || left(p_owner_key, 16), 20, 60) then
    return json_build_object('ok', false, 'error', 'forbidden');
  end if;

  select exists(
    select 1 from public.licenses
    where key = trim(p_owner_key) and plan = 'owner' and is_active
  ) into ok_owner;
  if not ok_owner then
    return json_build_object('ok', false, 'error', 'forbidden');
  end if;

  exp := case when coalesce(p_days, 0) <= 0 then null
              else default timezone('utc', now()) + make_interval(days => p_days) end;

  -- مفتاح عشوائي عالي التشتت
  loop
    nk := encode(gen_random_bytes(16), 'hex');
    tries := tries + 1;
    exit when not exists (select 1 from public.licenses where key = nk) or tries > 20;
  end loop;
  if tries > 20 then
    return json_build_object('ok', false, 'error', 'failed');
  end if;

  insert into public.licenses (key, plan, is_active, max_devices, devices, expires_at, note, created_by)
  values (nk, 'pro', true, greatest(coalesce(p_max_devices, 1), 1), 0, exp,
          left(coalesce(p_note, ''), 200), left(p_owner_key, 64));

  return json_build_object(
    'ok', true,
    'key', nk,
    'expires_at', case when exp is null then null
                       else to_char(exp at time zone 'utc', 'YYYY-MM-DD"T"HH24:MI:SS"Z"') end
  );
exception when others then
  return json_build_object('ok', false, 'error', 'failed');
end;
$$;

-- 3.6 owner_overview
create or replace function public.owner_overview(p_owner_key text)
returns json
language plpgsql
security definer
set search_path = public
as $$
declare
  ok_owner boolean;
  online_json json;
  lic_json json;
begin
  if p_owner_key is null or length(trim(p_owner_key)) < 8 then
    return json_build_object('ok', false);
  end if;
  if not public._rate_limit('ovw:' || left(p_owner_key, 16), 30, 60) then
    return json_build_object('ok', false);
  end if;

  select exists(
    select 1 from public.licenses
    where key = trim(p_owner_key) and plan = 'owner' and is_active
  ) into ok_owner;
  if not ok_owner then
    return json_build_object('ok', false);
  end if;

  select coalesce(json_agg(row_to_json(t)), '[]'::json) into online_json from (
    select coalesce(nullif(device_name, ''), hwid) as device_name,
           hwid,
           case when key is null then '••••'
                else left(key, 4) || '••••' || right(key, 2) end as key_masked,
           plan,
           to_char(last_seen_at at time zone 'utc', 'YYYY-MM-DD"T"HH24:MI:SS"Z"') as last_seen_at,
           (last_seen_at > default timezone('utc', now()) - interval '90 seconds') as is_online
    from public.devices
    order by last_seen_at desc
    limit 100
  ) t;

  select coalesce(json_agg(row_to_json(t)), '[]'::json) into lic_json from (
    select key, plan, is_active, devices,
           case when expires_at is null then null
                else to_char(expires_at at time zone 'utc', 'YYYY-MM-DD"T"HH24:MI:SS"Z"') end as expires_at,
           note,
           to_char(created_at at time zone 'utc', 'YYYY-MM-DD"T"HH24:MI:SS"Z"') as created_at
    from public.licenses
    order by created_at desc
    limit 500
  ) t;

  return json_build_object('ok', true, 'online', online_json, 'licenses', lic_json);
exception when others then
  return json_build_object('ok', false);
end;
$$;

-- 3.7 delete_key (مالك فقط — لا يحذف مفتاح المالك)
create or replace function public.delete_key(p_owner_key text, p_key text)
returns json
language plpgsql
security definer
set search_path = public
as $$
declare
  ok_owner boolean;
  target_plan text;
begin
  if p_owner_key is null or p_key is null then
    return json_build_object('ok', false, 'error', 'forbidden');
  end if;
  if not public._rate_limit('del:' || left(p_owner_key, 16), 20, 60) then
    return json_build_object('ok', false, 'error', 'forbidden');
  end if;

  select exists(
    select 1 from public.licenses
    where key = trim(p_owner_key) and plan = 'owner' and is_active
  ) into ok_owner;
  if not ok_owner then
    return json_build_object('ok', false, 'error', 'forbidden');
  end if;

  select plan into target_plan from public.licenses where key = trim(p_key);
  if target_plan is null then
    return json_build_object('ok', false, 'error', 'not_found');
  end if;
  if target_plan = 'owner' or trim(p_key) = trim(p_owner_key) then
    return json_build_object('ok', false, 'error', 'protected');
  end if;

  delete from public.devices where key = trim(p_key);
  delete from public.licenses where key = trim(p_key);
  return json_build_object('ok', true);
exception when others then
  return json_build_object('ok', false, 'error', 'failed');
end;
$$;

-- ========== 4) الصلاحيات: anon على الدوال فقط ==========
do $$
declare f text;
begin
  foreach f in array array[
    'validate_license(text, text)',
    'release_license(text, text)',
    'ping(text, text)',
    'device_ping(text, text, text, text)',
    'create_key(text, int, text, int)',
    'owner_overview(text)',
    'delete_key(text, text)'
  ]
  loop
    execute format('revoke execute on function %s from public, anon, authenticated', f);
    execute format('grant execute on function %s to anon, authenticated', f);
  end loop;
end $$;

-- ملاحظة: rate limit عبر أنفس الدوال لا يمنع إعادة الإرسال من نفس IP
-- خارج قاعدة البيانات — فعّل أيضاً Supabase/PostgREST rate limiting إن توفر.
