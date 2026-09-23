using System.Text;
using System.Text.Json;

namespace StarX_Client;

/// <summary>قواعد سحابية: تجاوزات درجة/نصيحة للفئات من السيرفر.</summary>
internal sealed record CloudRules(
    IReadOnlyDictionary<string, int> Scores,
    IReadOnlyDictionary<string, string> Advice);

/// <summary>
/// ربط التنظيف بالإنترنت عبر Supabase RPC:
/// 1) جلب قواعد التقييم المحدّثة (get_clean_rules) أثناء الفحص
/// 2) رفع إحصاءات المسح (report_clean_run) بعد كل عملية
/// الفشل لا يوقف التنظيف المحلي أبداً.
/// </summary>
internal static class CleanerCloud
{
    public const int RequestTimeoutMs = 8000;

    private static readonly HttpClient _http = new();

    /// <summary>التنبيه/المزامنة السحابية مفعّلة؟ (افتراضي: مفعّلة)</summary>
    public static bool Enabled
    {
        get => AppSettings.GetString("cleanCloud", "1") == "1";
        set => AppSettings.Set("cleanCloud", value ? "1" : "0");
    }

    /// <summary>جلب قواعد التقييم من السيرفر — null عند أي فشل (يعمل محلياً فقط).</summary>
    public static async Task<CloudRules?> FetchRulesAsync(CancellationToken ct = default)
    {
        if (!Enabled || !SupabaseConfig.IsConfigured) return null;
        try
        {
            using var doc = await RpcAsync("get_clean_rules", new { }, ct).ConfigureAwait(false);
            var r = doc.RootElement;
            if (!(r.TryGetProperty("ok", out var ok) && ok.ValueKind == JsonValueKind.True))
                return null;

            var scores = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
            var advice = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

            if (r.TryGetProperty("scores", out var sc) && sc.ValueKind == JsonValueKind.Object)
                foreach (var p in sc.EnumerateObject())
                    if (p.Value.ValueKind == JsonValueKind.Number)
                    {
                        int v = p.Value.GetInt32();
                        if (v is >= 0 and <= 100) scores[p.Name] = v;
                    }

            if (r.TryGetProperty("advice", out var ad) && ad.ValueKind == JsonValueKind.Object)
                foreach (var p in ad.EnumerateObject())
                    if (p.Value.ValueKind == JsonValueKind.String)
                    {
                        string? v = p.Value.GetString();
                        if (!string.IsNullOrWhiteSpace(v)) advice[p.Name] = v!;
                    }

            if (scores.Count == 0 && advice.Count == 0) return null;
            return new CloudRules(scores, advice);
        }
        catch
        {
            return null; // سيرفر غير متاح — نكمل محلياً
        }
    }

    /// <summary>رفع إحصاء عملية تنظيف — best effort، يرجع false عند الفشل.</summary>
    public static async Task<bool> ReportRunAsync(
        int deleted, long freed, int failed, CancellationToken ct = default)
    {
        if (!Enabled || !SupabaseConfig.IsConfigured) return false;
        try
        {
            string plan = "free";
            try
            {
                var cache = LicenseManager.LoadCache();
                if (cache != null && !string.IsNullOrWhiteSpace(cache.Plan)) plan = cache.Plan;
            }
            catch { /* ignore */ }

            using var doc = await RpcAsync("report_clean_run", new
            {
                p_hwid = LicenseManager.GetHwid(),
                p_device_name = LicenseManager.GetDeviceName(),
                p_plan = plan,
                p_deleted = deleted,
                p_freed = freed,
                p_failed = failed,
            }, ct).ConfigureAwait(false);
            return doc.RootElement.TryGetProperty("ok", out var ok) && ok.ValueKind == JsonValueKind.True;
        }
        catch
        {
            return false;
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
                throw new HttpRequestException($"Supabase {(int)res.StatusCode}: {text.Trim()[..Math.Min(200, text.Trim().Length)]}");
            return JsonDocument.Parse(text);
        }
    }
}
