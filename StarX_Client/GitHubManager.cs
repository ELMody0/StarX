using System.Text.Json;

namespace StarX_Client;

/// <summary>معلومات إصدار GitHub + أصوله.</summary>
internal sealed record ReleaseAsset(string Name, string DownloadUrl);

internal sealed record ReleaseInfo(
    string Version, string Tag, string Name, string Notes,
    string AssetName, string DownloadUrl,
    IReadOnlyList<ReleaseAsset> Assets)
{
    public string? Sha256Url =>
        Assets.FirstOrDefault(a => a.Name.Equals(
            AssetName + ".sha256", StringComparison.OrdinalIgnoreCase))?.DownloadUrl;

    public string? SigUrl =>
        Assets.FirstOrDefault(a => a.Name.Equals(
            AssetName + ".sig", StringComparison.OrdinalIgnoreCase))?.DownloadUrl;
}

/// <summary>تكامل GitHub Releases (قراءة عامة — بدون أسرار).</summary>
internal static class GitHubManager
{
    private static readonly HttpClient _http = new();

    // للاختبارات فقط: استبدال رابط الـ API بالكامل
#pragma warning disable CS0649
    internal static string? ApiBaseOverride;
#pragma warning restore CS0649

    public static async Task<(ReleaseInfo? Release, string? Error)> GetLatestAsync(
        CancellationToken ct = default)
    {
        string url = ApiBaseOverride ?? UpdateConfig.ApiLatestUrl;
        using var cts = CancellationTokenSource.CreateLinkedTokenSource(ct);
        cts.CancelAfter(TimeSpan.FromSeconds(UpdateConfig.RequestTimeoutSeconds));

        using var req = new HttpRequestMessage(HttpMethod.Get, url);
        req.Headers.UserAgent.ParseAdd("StarX-Updater");
        req.Headers.Accept.ParseAdd("application/vnd.github+json");

        HttpResponseMessage res;
        try
        {
            res = await _http.SendAsync(req, cts.Token).ConfigureAwait(false);
        }
        catch (Exception ex) when (ex is TimeoutException or HttpRequestException or TaskCanceledException)
        {
            return (null, "network");
        }
        catch (Exception)
        {
            return (null, "network");
        }

        using (res)
        {
            string text;
            try
            {
                text = await res.Content.ReadAsStringAsync(cts.Token).ConfigureAwait(false);
            }
            catch { return (null, "network"); }

            if (!res.IsSuccessStatusCode)
            {
                int code = (int)res.StatusCode;
                if (code == 403 || code == 429) return (null, "rate");
                if (code == 404) return (null, "none");
                return (null, "http:" + code);
            }

            try
            {
                using var doc = JsonDocument.Parse(text);
                var r = doc.RootElement;
                string tag = r.TryGetProperty("tag_name", out var t) &&
                             t.ValueKind == JsonValueKind.String ? t.GetString() ?? "" : "";
                if (!UpdateVersion.TryParse(tag, out _))
                    return (null, "invalid");
                string version = UpdateVersion.Normalize(tag);

                var assets = new List<ReleaseAsset>();
                if (r.TryGetProperty("assets", out var arr) && arr.ValueKind == JsonValueKind.Array)
                {
                    foreach (var a in arr.EnumerateArray())
                    {
                        string n = a.TryGetProperty("name", out var an) &&
                                   an.ValueKind == JsonValueKind.String ? an.GetString() ?? "" : "";
                        string u = a.TryGetProperty("browser_download_url", out var au) &&
                                   au.ValueKind == JsonValueKind.String ? au.GetString() ?? "" : "";
                        if (!string.IsNullOrEmpty(n) && !string.IsNullOrEmpty(u))
                            assets.Add(new ReleaseAsset(n, u));
                    }
                }

                var pkg = assets.FirstOrDefault(a =>
                    a.Name.StartsWith(UpdateConfig.AssetPrefix, StringComparison.OrdinalIgnoreCase) &&
                    a.Name.EndsWith(UpdateConfig.AssetExtension, StringComparison.OrdinalIgnoreCase));
                if (pkg == null)
                    return (null, "invalid");

                string name = r.TryGetProperty("name", out var rn) &&
                              rn.ValueKind == JsonValueKind.String ? rn.GetString() ?? "" : "";
                string body = r.TryGetProperty("body", out var rb) &&
                              rb.ValueKind == JsonValueKind.String ? rb.GetString() ?? "" : "";
                return (new ReleaseInfo(version, tag, name, body, pkg.Name, pkg.DownloadUrl, assets), null);
            }
            catch
            {
                return (null, "invalid");
            }
        }
    }

    /// <summary>تحميل نص ملف الـ SHA-256 المرافق للحزمة.</summary>
    public static async Task<(string? Sha256, string? Error)> GetAssetSha256Async(
        ReleaseInfo rel, CancellationToken ct = default)
    {
        string? url = rel.Sha256Url;
        if (string.IsNullOrEmpty(url))
            return (null, "no-checksum");

        using var cts = CancellationTokenSource.CreateLinkedTokenSource(ct);
        cts.CancelAfter(TimeSpan.FromSeconds(UpdateConfig.RequestTimeoutSeconds));
        try
        {
            using var res = await _http.GetAsync(url, cts.Token).ConfigureAwait(false);
            if (!res.IsSuccessStatusCode)
                return (null, "no-checksum");
            string text = await res.Content.ReadAsStringAsync(cts.Token).ConfigureAwait(false);
            string hex = text.Split((char[])[' ', '\t', '\r', '\n'],
                StringSplitOptions.RemoveEmptyEntries).FirstOrDefault() ?? "";
            if (hex.Length != 64 || !hex.All(Uri.IsHexDigit))
                return (null, "no-checksum");
            return (hex, null);
        }
        catch (Exception ex) when (ex is TimeoutException or HttpRequestException or TaskCanceledException)
        {
            return (null, "network");
        }
        catch (Exception)
        {
            return (null, "network");
        }
    }

    /// <summary>تحميل توقيع ECDSA المرفق للحزمة (ملف .sig).</summary>
    public static async Task<(string? Sig, string? Error)> GetAssetSignatureAsync(
        ReleaseInfo rel, CancellationToken ct = default)
    {
        string? url = rel.SigUrl;
        if (string.IsNullOrEmpty(url))
            return (null, "no-signature");

        using var cts = CancellationTokenSource.CreateLinkedTokenSource(ct);
        cts.CancelAfter(TimeSpan.FromSeconds(UpdateConfig.RequestTimeoutSeconds));
        try
        {
            using var res = await _http.GetAsync(url, cts.Token).ConfigureAwait(false);
            if (!res.IsSuccessStatusCode)
                return (null, "no-signature");
            string text = (await res.Content.ReadAsStringAsync(cts.Token).ConfigureAwait(false)).Trim();
            if (string.IsNullOrEmpty(text))
                return (null, "no-signature");
            // يقبل base64 خام أو سطر نصي يحويه
            string b64 = text.Split((char[])[' ', '\t', '\r', '\n'],
                StringSplitOptions.RemoveEmptyEntries).FirstOrDefault() ?? text;
            return (b64, null);
        }
        catch (Exception ex) when (ex is TimeoutException or HttpRequestException or TaskCanceledException)
        {
            return (null, "network");
        }
        catch (Exception)
        {
            return (null, "network");
        }
    }
}
