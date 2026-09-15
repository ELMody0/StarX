using Microsoft.Win32;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace StarX_Client;

/// <summary>حالة الترخيص: free = بدون مفتاح، pro = مشترك، owner = المالك.</summary>
internal sealed record LicenseState(bool Active, string Plan, string Detail, string KeyMasked)
{
    public bool IsPro => Plan == "pro" || Plan == "owner";
    public bool IsOwner => Plan == "owner";
}

internal sealed record LicenseCache(string Key, string Hwid, string Plan, DateTime ValidatedAtUtc);

internal sealed record OwnerUser(string Hwid, string KeyMasked, string Plan, string LastSeen, bool Online);

internal sealed record OwnerLicense(string Key, string Plan, bool Active, int Devices, string? ExpiresAt, string? Note, string Created);

/// <summary>
/// نظام مفاتيح التفعيل عبر Supabase (PostgREST RPC — بدون مكتبات خارجية).
/// الدوال: validate_license, release_license, ping, create_key, owner_overview.
/// </summary>
internal static class LicenseManager
{
    public const int RequestTimeoutMs = 12000;
    public const int OfflineGraceDays = 7;

    private static readonly HttpClient _http = new();

    // للاختبارات فقط: تجاوز مسار ملف الكاش
#pragma warning disable CS0649
    internal static string? CachePathOverride;
#pragma warning restore CS0649

    private static string CachePath =>
        CachePathOverride ??
        Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "StarX", "license.json");

    // ---------- البصمة ----------

    public static string GetHwid()
    {
        try
        {
            using var k = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Microsoft\Cryptography");
            var g = k?.GetValue("MachineGuid")?.ToString();
            if (!string.IsNullOrWhiteSpace(g))
                return "mg-" + g!.Trim().ToLowerInvariant();
        }
        catch { /* fallback below */ }
        using var sha = SHA256.Create();
        byte[] hash = sha.ComputeHash(Encoding.UTF8.GetBytes(
            Environment.MachineName + "|" + Environment.UserName));
        return "pc-" + Convert.ToHexString(hash)[..16].ToLowerInvariant();
    }

    // ---------- الكاش المحلي ----------

    public static LicenseCache? LoadCache()
    {
        try
        {
            string p = CachePath;
            if (!File.Exists(p)) return null;
            using var doc = JsonDocument.Parse(File.ReadAllText(p));
            var r = doc.RootElement;
            return new LicenseCache(
                Str(r, "key"),
                Str(r, "hwid"),
                Str(r, "plan", "pro"),
                r.TryGetProperty("validatedAt", out var v) && v.ValueKind == JsonValueKind.String
                    ? v.GetDateTime().ToUniversalTime() : DateTime.MinValue);
        }
        catch { return null; }
    }

    public static void SaveCache(string key, string hwid, string plan)
    {
        string p = CachePath;
        Directory.CreateDirectory(Path.GetDirectoryName(p)!);
        string json = JsonSerializer.Serialize(new
        {
            key,
            hwid,
            plan,
            validatedAt = DateTime.UtcNow,
        });
        File.WriteAllText(p, json);
    }

    public static void ClearCache()
    {
        try { if (File.Exists(CachePath)) File.Delete(CachePath); } catch { /* ignore */ }
    }

    private static string Str(JsonElement r, string name, string fallback = "") =>
        r.TryGetProperty(name, out var e) && e.ValueKind == JsonValueKind.String
            ? e.GetString() ?? fallback : fallback;

    // ---------- Supabase RPC ----------

    public static async Task<(bool Ok, string Reason, string Plan, string? ExpiresAt)> ValidateOnlineAsync(
        string key, string hwid, CancellationToken ct = default)
    {
        if (!SupabaseConfig.IsConfigured)
            return (false, "config", "free", null);
        if (string.IsNullOrWhiteSpace(key))
            return (false, "invalid", "free", null);

        JsonDocument? doc;
        try
        {
            doc = await RpcAsync("validate_license",
                new { p_key = key.Trim(), p_hwid = hwid }, ct).ConfigureAwait(false);
        }
        catch (Exception ex) when (ex is TimeoutException or HttpRequestException or TaskCanceledException)
        {
            if (ex.Message.Contains("PGRST202", StringComparison.OrdinalIgnoreCase))
                return (false, "config", "free", null);
            return (false, "network", "free", null);
        }
        catch (Exception ex)
        {
            return (false, "network:" + ex.Message, "free", null);
        }

        using (doc)
        {
            try
            {
                var r = doc.RootElement;
                bool ok = r.TryGetProperty("ok", out var okEl) &&
                          okEl.ValueKind == JsonValueKind.True;
                string reason = Str(r, "reason", ok ? "active" : "invalid");
                string plan = ok ? Str(r, "plan", "pro") : "free";
                if (plan != "pro" && plan != "owner") plan = "pro";
                string? exp = r.TryGetProperty("expires_at", out var ee) &&
                              ee.ValueKind == JsonValueKind.String
                    ? ee.GetString() : null;
                return (ok, reason, plan, exp);
            }
            catch
            {
                return (false, "network:bad-response", "free", null);
            }
        }
    }

    public static async Task ReleaseOnlineAsync(string key, string hwid, CancellationToken ct = default)
    {
        if (!SupabaseConfig.IsConfigured) return;
        try
        {
            using var doc = await RpcAsync("release_license",
                new { p_key = key.Trim(), p_hwid = hwid }, ct).ConfigureAwait(false);
        }
        catch { /* best effort */ }
    }

    public static async Task PingAsync(string key, string hwid, CancellationToken ct = default)
    {
        if (!SupabaseConfig.IsConfigured) return;
        try
        {
            using var doc = await RpcAsync("ping",
                new { p_key = key.Trim(), p_hwid = hwid }, ct).ConfigureAwait(false);
        }
        catch { /* best effort */ }
    }

    public static async Task<(bool Ok, string NewKey, string? ExpiresAt, string Error)> CreateKeyAsync(
        string ownerKey, int days, string note, int maxDevices, CancellationToken ct = default)
    {
        JsonDocument doc;
        try
        {
            doc = await RpcAsync("create_key",
                new { p_owner_key = ownerKey, p_days = days, p_note = note ?? "", p_max_devices = maxDevices },
                ct).ConfigureAwait(false);
        }
        catch (Exception ex) when (ex is TimeoutException or HttpRequestException or TaskCanceledException)
        {
            return (false, string.Empty, null, Lang.T("r_network"));
        }
        catch (Exception ex)
        {
            return (false, string.Empty, null, Lang.T("r_network") + ": " + ex.Message.Left(120));
        }

        using (doc)
        {
            try
            {
                var r = doc.RootElement;
                bool ok = r.TryGetProperty("ok", out var okEl) &&
                          okEl.ValueKind == JsonValueKind.True;
                if (!ok)
                {
                    string err = Str(r, "error", "forbidden");
                    return (false, string.Empty, null,
                        err == "forbidden" ? Lang.T("r_forbidden") : Lang.T("r_create_fail"));
                }
                string? exp = r.TryGetProperty("expires_at", out var ee) &&
                              ee.ValueKind == JsonValueKind.String
                    ? ee.GetString() : null;
                return (true, Str(r, "key"), exp, string.Empty);
            }
            catch
            {
                return (false, string.Empty, null, Lang.T("r_bad_response"));
            }
        }
    }

    public static async Task<(bool Ok, string Error, List<OwnerUser> Online, List<OwnerLicense> Licenses)> OverviewAsync(
        string ownerKey, CancellationToken ct = default)
    {
        var online = new List<OwnerUser>();
        var licenses = new List<OwnerLicense>();
        JsonDocument doc;
        try
        {
            doc = await RpcAsync("owner_overview", new { p_owner_key = ownerKey }, ct)
                .ConfigureAwait(false);
        }
        catch (Exception ex) when (ex is TimeoutException or HttpRequestException or TaskCanceledException)
        {
            return (false, Lang.T("r_network"), online, licenses);
        }
        catch (Exception ex)
        {
            return (false, Lang.T("r_network") + ": " + ex.Message.Left(120), online, licenses);
        }

        using (doc)
        {
            try
            {
                var r = doc.RootElement;
                bool ok = r.TryGetProperty("ok", out var okEl) &&
                          okEl.ValueKind == JsonValueKind.True;
                if (!ok)
                    return (false, Lang.T("r_forbidden"), online, licenses);
                if (r.TryGetProperty("online", out var on) && on.ValueKind == JsonValueKind.Array)
                    foreach (var u in on.EnumerateArray())
                        online.Add(new OwnerUser(
                            Str(u, "hwid"), Str(u, "key_masked", "—"), Str(u, "plan", "free"),
                            Str(u, "last_seen_at").Left(16),
                            u.TryGetProperty("is_online", out var io) && io.ValueKind == JsonValueKind.True));
                if (r.TryGetProperty("licenses", out var li) && li.ValueKind == JsonValueKind.Array)
                    foreach (var l in li.EnumerateArray())
                        licenses.Add(new OwnerLicense(
                            Str(l, "key"), Str(l, "plan", "pro"),
                            !l.TryGetProperty("is_active", out var ia) || ia.ValueKind != JsonValueKind.False,
                            l.TryGetProperty("devices", out var dv) && dv.ValueKind == JsonValueKind.Number
                                ? dv.GetInt32() : 0,
                            l.TryGetProperty("expires_at", out var ee) && ee.ValueKind == JsonValueKind.String
                                ? ee.GetString() : null,
                            l.TryGetProperty("note", out var nt) && nt.ValueKind == JsonValueKind.String
                                ? nt.GetString() : null,
                            Str(l, "created_at").Left(16)));
                return (true, string.Empty, online, licenses);
            }
            catch
            {
                return (false, Lang.T("r_bad_response"), online, licenses);
            }
        }
    }

    public static async Task<(bool Ok, string Error)> DeleteKeyAsync(
        string ownerKey, string key, CancellationToken ct = default)
    {
        JsonDocument doc;
        try
        {
            doc = await RpcAsync("delete_key",
                new { p_owner_key = ownerKey, p_key = key.Trim() }, ct).ConfigureAwait(false);
        }
        catch (Exception ex) when (ex is TimeoutException or HttpRequestException or TaskCanceledException)
        {
            return (false, Lang.T("r_network"));
        }
        catch (Exception ex)
        {
            return (false, Lang.T("r_network") + ": " + ex.Message.Left(120));
        }

        using (doc)
        {
            try
            {
                var r = doc.RootElement;
                bool ok = r.TryGetProperty("ok", out var okEl) &&
                          okEl.ValueKind == JsonValueKind.True;
                if (ok) return (true, string.Empty);
                return (false, Str(r, "error", "forbidden") switch
                {
                    "forbidden" => Lang.T("r_forbidden"),
                    "not_found" => Lang.T("r_not_found"),
                    "protected" => Lang.T("r_protected"),
                    _ => Lang.T("r_delete_fail"),
                });
            }
            catch
            {
                return (false, Lang.T("r_bad_response"));
            }
        }
    }

    private static async Task<JsonDocument> RpcAsync<T>(string function, T body, CancellationToken ct)
    {
        using var cts = CancellationTokenSource.CreateLinkedTokenSource(ct);
        cts.CancelAfter(RequestTimeoutMs);
        using var req = new HttpRequestMessage(HttpMethod.Post, SupabaseConfig.RpcUrl(function));
        req.Headers.TryAddWithoutValidation("apikey", SupabaseConfig.AnonKey);
        req.Headers.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", SupabaseConfig.AnonKey);
        req.Content = new StringContent(JsonSerializer.Serialize(body), Encoding.UTF8, "application/json");

        HttpResponseMessage res;
        try
        {
            res = await _http.SendAsync(req, cts.Token).ConfigureAwait(false);
        }
        catch (OperationCanceledException) when (!ct.IsCancellationRequested)
        {
            throw new TimeoutException("انتهت مهلة الاتصال بـ Supabase.");
        }

        using (res)
        {
            string text = await res.Content.ReadAsStringAsync(cts.Token).ConfigureAwait(false);
            if (!res.IsSuccessStatusCode)
                throw new HttpRequestException($"Supabase {(int)res.StatusCode}: {text.Trim().Left(200)}");
            return JsonDocument.Parse(text);
        }
    }

    // ---------- الحالة (كاش + تحقق + سماح عدم الاتصال) ----------

    public static async Task<LicenseState> GetStatusAsync(CancellationToken ct = default)
    {
        if (!SupabaseConfig.IsConfigured)
            return new LicenseState(false, "free", Lang.T("st_no_supabase"), string.Empty);

        var cache = LoadCache();
        if (cache == null || string.IsNullOrWhiteSpace(cache.Key))
            return new LicenseState(false, "free", Lang.T("st_inactive"), string.Empty);

        var (ok, reason, plan, _) = await ValidateOnlineAsync(cache.Key, cache.Hwid, ct).ConfigureAwait(false);
        if (ok)
        {
            SaveCache(cache.Key, cache.Hwid, plan); // تجديد تاريخ التحقق
            return new LicenseState(true, plan, string.Empty, string.Empty);
        }
        if (reason.StartsWith("network", StringComparison.OrdinalIgnoreCase))
        {
            if ((DateTime.UtcNow - cache.ValidatedAtUtc).TotalDays <= OfflineGraceDays)
                return new LicenseState(true, cache.Plan, Lang.T("st_offline"), string.Empty);
            return new LicenseState(false, "free", Lang.T("st_no_grace"), string.Empty);
        }
        ClearCache(); // مرفوض من السيرفر
        return new LicenseState(false, "free", ReasonText(reason), string.Empty);
    }

    public static string Mask(string key)
    {
        if (string.IsNullOrEmpty(key)) return string.Empty;
        key = key.Trim();
        return key.Length <= 8 ? "••••" : key[..4] + "••••" + key[^2..];
    }

    public static string ReasonText(string reason) => reason switch
    {
        "invalid" => Lang.T("r_invalid"),
        "revoked" => Lang.T("r_revoked"),
        "expired" => Lang.T("r_expired"),
        "device_limit" => Lang.T("r_device_limit"),
        "config" => Lang.T("r_config"),
        "forbidden" => Lang.T("r_forbidden"),
        "not_found" => Lang.T("r_not_found"),
        "protected" => Lang.T("r_protected"),
        _ when reason.StartsWith("network", StringComparison.OrdinalIgnoreCase) => Lang.T("r_network"),
        _ => Lang.T("r_unknown"),
    };
}

file static class StringExt
{
    public static string Left(this string s, int n) =>
        string.IsNullOrEmpty(s) ? s : (s.Length <= n ? s : s[..n]);
}
