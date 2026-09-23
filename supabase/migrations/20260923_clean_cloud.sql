-- StarX: تنظيف بالإنترنت (إحصاءات + قواعد تقييم سحابية)
-- شغّل هذا الملف في Supabase SQL Editor أو عبر supabase db push

-- ========== 1) جدول إحصاءات التنظيف ==========
create table if not exists public.clean_runs (
  id          bigint generated always as identity primary key,
  hwid        text not null,
  device_name text,
  plan        text not null default 'free',
  deleted     int  not null default 0,
  freed       bigint not null default 0,
  failed      int  not null default 0,
  created_at  timestamptz not null default now()
);

create index if not exists clean_runs_hwid_idx on public.clean_runs (hwid, created_at desc);

alter table public.clean_runs enable row level security;

-- لا قراءة مباشرة للـ anon — فقط عبر دوال المالك لاحقاً
drop policy if exists "clean_runs_insert" on public.clean_runs;
create policy "clean_runs_insert"
  on public.clean_runs for insert
  to anon, authenticated
  with check (true);

-- ========== 2) جدول قواعد التقييم السحابية ==========
create table if not exists public.clean_rules (
  category_id text primary key,
  score       int check (score between 0 and 100),
  advice      text,
  enabled     boolean not null default true,
  updated_at  timestamptz not null default now()
);

alter table public.clean_rules enable row level security;

drop policy if exists "clean_rules_read" on public.clean_rules;
create policy "clean_rules_read"
  on public.clean_rules for select
  to anon, authenticated
  using (enabled);

-- قيم افتراضية مطابقة للمنطق المحلي (قابلة للتعديل من لوحة المالك/SQL)
insert into public.clean_rules (category_id, score, advice) values
  ('wintemp',   86, 'adv_system'),
  ('usertemp',  86, 'adv_system'),
  ('prefetch',  60, 'adv_review_sys'),
  ('softdist',  60, 'adv_review_sys'),
  ('cbs',       92, 'adv_logs'),
  ('thumb',     90, 'adv_cache'),
  ('chrome',    76, 'adv_cache'),
  ('edge',      76, 'adv_cache'),
  ('firefox',   76, 'adv_cache'),
  ('inetcache', 80, 'adv_cache'),
  ('dxcache',   62, 'adv_shader'),
  ('recycle',   40, 'adv_bin')
on conflict (category_id) do nothing;

-- ========== 3) RPC: رفع إحصاء تنظيف ==========
create or replace function public.report_clean_run(
  p_hwid text,
  p_device_name text default null,
  p_plan text default 'free',
  p_deleted int default 0,
  p_freed bigint default 0,
  p_failed int default 0
) returns json
language plpgsql
security definer
set search_path = public
as $$
begin
  if p_hwid is null or length(trim(p_hwid)) = 0 then
    return json_build_object('ok', false, 'error', 'bad_hwid');
  end if;

  insert into public.clean_runs (hwid, device_name, plan, deleted, freed, failed)
  values (
    left(trim(p_hwid), 64),
    left(coalesce(p_device_name, ''), 120),
    left(coalesce(p_plan, 'free'), 16),
    greatest(p_deleted, 0),
    greatest(p_freed, 0),
    greatest(p_failed, 0)
  );

  return json_build_object('ok', true);
exception when others then
  return json_build_object('ok', false, 'error', 'failed');
end;
$$;

revoke execute on function public.report_clean_run(text, text, text, int, bigint, int)
  from public, anon, authenticated;
grant execute on function public.report_clean_run(text, text, text, int, bigint, int)
  to anon, authenticated;

-- ========== 4) RPC: جلب قواعد التقييم ==========
create or replace function public.get_clean_rules()
returns json
language sql
security definer
stable
set search_path = public
as $$
  select coalesce(
    json_build_object(
      'ok', true,
      'scores', coalesce(
        (select json_object_agg(category_id, score)
         from public.clean_rules
         where enabled and score is not null),
        '{}'::json
      ),
      'advice', coalesce(
        (select json_object_agg(category_id, advice)
         from public.clean_rules
         where enabled and advice is not null),
        '{}'::json
      )
    ),
    json_build_object('ok', false)
  );
$$;

revoke execute on function public.get_clean_rules() from public, anon, authenticated;
grant execute on function public.get_clean_rules() to anon, authenticated;
