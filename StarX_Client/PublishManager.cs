using System.IO.Compression;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace StarX_Client;

/// <summary>نشر إصدار جديد إلى GitHub Releases من لوحة المالك (يتطلب رمزاً بصلاحية repo).</summary>
internal static class PublishManager
{
    private static readonly HttpClient _http = new();

    private static readonly string[] ExcludedExtensions = [".pdb"];
    private static readonly string[] ExcludedDirs = ["logs", "backup"];

    public static bool TryParseVersionInput(string? s, out string normalized)
    {
        normalized = string.Empty;
        if (string.IsNullOrWhiteSpace(s)) return false;
        s = s.Trim();
        if (!UpdateVersion.TryParse(s, out var v)) return false;
        normalized = v.ToString();
        return true;
    }

    public static async Task<(bool Ok, string Message)> PublishAsync(
        string version, string token, string notes,
        IProgress<string>? progress, CancellationToken ct = default)
    {
        if (!TryParseVersionInput(version, out string ver))
            return (false, Lang.T("pub_bad_version"));
        if (string.IsNullOrWhiteSpace(token))
            return (false, Lang.T("pub_no_token"));

        string work = Path.Combine(Path.GetTempPath(), "StarXPublish", ver);
        string zipPath = Path.Combine(work, $"StarX-v{ver}.zip");
        try
        {
            progress?.Report(Lang.T("pub_stage"));
            if (Directory.Exists(work)) Directory.Delete(work, recursive: true);
            string stage = Directory.CreateDirectory(Path.Combine(work, "stage")).FullName;
            StageInstallDir(AppContext.BaseDirectory, stage, ver);

            progress?.Report(Lang.T("pub_zip"));
            if (File.Exists(zipPath)) File.Delete(zipPath);
            ZipDirectory(stage, zipPath);

            progress?.Report(Lang.T("pub_hash"));
            string sha = Sha256File(zipPath);

            progress?.Report(Lang.T("pub_release"));
            var (relId, uploadUrl, relErr) = await CreateReleaseAsync(ver, token.Trim(), notes ?? "", ct)
                .ConfigureAwait(false);
            if (relId < 0)
                return (false, relErr);

            progress?.Report(Lang.T("pub_upload"));
            string? upErr = await UploadAssetAsync(uploadUrl, zipPath,
                $"StarX-v{ver}.zip", "application/zip", token.Trim(), ct).ConfigureAwait(false);
            if (upErr != null)
                return (false, upErr);

            string shaFile = Path.Combine(work, $"StarX-v{ver}.zip.sha256");
            await File.WriteAllTextAsync(shaFile, $"{sha}  StarX-v{ver}.zip\n", ct).ConfigureAwait(false);
            upErr = await UploadAssetAsync(uploadUrl, shaFile,
                $"StarX-v{ver}.zip.sha256", "text/plain", token.Trim(), ct).ConfigureAwait(false);
            if (upErr != null)
                return (false, upErr);

            try { Directory.Delete(work, recursive: true); } catch { /* ignore */ }
            return (true, $"{Lang.T("pub_done")} — v{ver}");
        }
        catch (OperationCanceledException)
        {
            return (false, Lang.T("upd_failed"));
        }
        catch (Exception ex)
        {
            return (false, ex.Message);
        }
    }

    private static void StageInstallDir(string src, string stage, string version)
    {
        foreach (string file in Directory.EnumerateFiles(src, "*", SearchOption.AllDirectories))
        {
            string rel = Path.GetRelativePath(src, file);
            string[] parts = rel.Split(Path.DirectorySeparatorChar);
            if (parts.Any(p => ExcludedDirs.Contains(p, StringComparer.OrdinalIgnoreCase)))
                continue;
            if (ExcludedExtensions.Contains(Path.GetExtension(file), StringComparer.OrdinalIgnoreCase))
                continue;
            if (Path.GetFileName(file).EndsWith(".bak", StringComparison.OrdinalIgnoreCase))
                continue;
            string dest = Path.Combine(stage, rel);
            Directory.CreateDirectory(Path.GetDirectoryName(dest)!);
            File.Copy(file, dest, overwrite: true);
        }
        File.WriteAllText(Path.Combine(stage, "VERSION"), version + "\n");
        if (!File.Exists(Path.Combine(stage, "StarX.exe")))
            throw new InvalidOperationException("StarX.exe");
    }

    private static void ZipDirectory(string stage, string zipPath)
    {
        using var zip = ZipFile.Open(zipPath, ZipArchiveMode.Create);
        foreach (string file in Directory.EnumerateFiles(stage, "*", SearchOption.AllDirectories))
        {
            string entry = Path.GetRelativePath(stage, file).Replace(Path.DirectorySeparatorChar, '/');
            zip.CreateEntryFromFile(file, entry, CompressionLevel.Optimal);
        }
    }

    private static string Sha256File(string path)
    {
        using var sha = SHA256.Create();
        using var fs = File.OpenRead(path);
        return Convert.ToHexString(sha.ComputeHash(fs)).ToLowerInvariant();
    }

    private static async Task<(long RelId, string UploadUrl, string Error)> CreateReleaseAsync(
        string ver, string token, string notes, CancellationToken ct)
    {
        using var req = new HttpRequestMessage(HttpMethod.Post,
            $"https://api.github.com/repos/{UpdateConfig.RepoOwner}/{UpdateConfig.RepoName}/releases");
        req.Headers.UserAgent.ParseAdd("StarX-OwnerPanel");
        req.Headers.Accept.ParseAdd("application/vnd.github+json");
        req.Headers.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
        req.Content = new StringContent(JsonSerializer.Serialize(new
        {
            tag_name = "v" + ver,
            name = "StarX v" + ver,
            body = notes,
            draft = false,
            prerelease = false,
        }), Encoding.UTF8, "application/json");

        using var res = await _http.SendAsync(req, ct).ConfigureAwait(false);
        string text = await res.Content.ReadAsStringAsync(ct).ConfigureAwait(false);
        if ((int)res.StatusCode == 401 || (int)res.StatusCode == 403)
            return (-1, "", Lang.T("pub_unauth"));
        if ((int)res.StatusCode == 422)
            return (-1, "", Lang.T("pub_exists"));
        if (!res.IsSuccessStatusCode)
            return (-1, "", $"GitHub {(int)res.StatusCode}");
        try
        {
            using var doc = JsonDocument.Parse(text);
            long id = doc.RootElement.GetProperty("id").GetInt64();
            string up = doc.RootElement.GetProperty("upload_url").GetString() ?? "";
            int q = up.IndexOf('{');
            if (q >= 0) up = up[..q];
            return (id, up, "");
        }
        catch
        {
            return (-1, "", Lang.T("upd_invalid"));
        }
    }

    private static async Task<string?> UploadAssetAsync(
        string uploadUrl, string filePath, string assetName, string contentType,
        string token, CancellationToken ct)
    {
        try
        {
            string url = uploadUrl + "?name=" + Uri.EscapeDataString(assetName);
            using var req = new HttpRequestMessage(HttpMethod.Post, url);
            req.Headers.UserAgent.ParseAdd("StarX-OwnerPanel");
            req.Headers.Accept.ParseAdd("application/vnd.github+json");
            req.Headers.Authorization =
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
            using var fs = File.OpenRead(filePath);
            req.Content = new StreamContent(fs);
            req.Content.Headers.ContentType =
                new System.Net.Http.Headers.MediaTypeHeaderValue(contentType);
            req.Content.Headers.ContentLength = fs.Length;
            using var res = await _http.SendAsync(req, ct).ConfigureAwait(false);
            if ((int)res.StatusCode is 201 or 200) return null;
            string text = await res.Content.ReadAsStringAsync(ct).ConfigureAwait(false);
            return $"GitHub {(int)res.StatusCode}: {(text.Trim().Length > 160 ? text.Trim()[..160] : text.Trim())}";
        }
        catch (Exception ex) when (ex is TimeoutException or HttpRequestException or TaskCanceledException)
        {
            return Lang.T("upd_failed");
        }
        catch (Exception ex)
        {
            return ex.Message;
        }
    }
}
