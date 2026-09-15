using System.Diagnostics;

namespace StarX_Client;

/// <summary>نتيجة فحص التحديث.</summary>
internal sealed record UpdateCheckResult(bool Available, ReleaseInfo? Release, string? Message);

/// <summary>تنسيق فحص التحديثات + الإشعار + تشغيل المحدّث.</summary>
internal static class UpdateManager
{
    public static async Task<UpdateCheckResult> CheckAsync(bool silent, CancellationToken ct = default)
    {
        var (rel, err) = await GitHubManager.GetLatestAsync(ct).ConfigureAwait(false);
        if (rel == null)
            return new UpdateCheckResult(false, null, silent ? null : FriendlyCheckError(err));

        if (!UpdateVersion.TryParse(AppVersion.Current, out var cur) ||
            !UpdateVersion.TryParse(rel.Version, out var lat) ||
            lat.CompareTo(cur) <= 0)
        {
            return new UpdateCheckResult(false, rel,
                silent ? null : $"{Lang.T("upd_uptodate")}\n{Lang.T("upd_current")}: v{AppVersion.Current}");
        }

        if (silent && IsSuppressed(rel.Version))
            return new UpdateCheckResult(false, rel, null);

        return new UpdateCheckResult(true, rel, null);
    }

    public static string FriendlyCheckError(string? err) => err switch
    {
        "network" => Lang.T("upd_failed"),
        "rate" => Lang.T("upd_rate"),
        "none" => Lang.T("upd_none"),
        "invalid" or "no-checksum" => Lang.T("upd_invalid"),
        _ when (err ?? "").StartsWith("http:", StringComparison.OrdinalIgnoreCase) => Lang.T("upd_failed"),
        _ => Lang.T("upd_failed"),
    };

    public static bool IsSuppressed(string version)
    {
        try
        {
            if (AppSettings.GetString("updateSkippedVersion") != version)
                return false;
            string until = AppSettings.GetString("skipUntilUtc");
            return DateTime.TryParse(until, null,
                System.Globalization.DateTimeStyles.RoundtripKind, out var dt) && DateTime.UtcNow < dt;
        }
        catch { return false; }
    }

    public static void MarkSuppressed(string version)
    {
        AppSettings.Set("updateSkippedVersion", version);
        AppSettings.Set("skipUntilUtc",
            DateTime.UtcNow.AddHours(UpdateConfig.SuppressionHours).ToString("o"));
    }

    public static void ClearSuppression()
    {
        AppSettings.Set("updateSkippedVersion", null);
        AppSettings.Set("skipUntilUtc", null);
    }

    public static string? FindUpdater()
    {
        try
        {
            string dir = AppContext.BaseDirectory;
            foreach (var n in new[] { "StarXUpdater.exe", Path.Combine("StarXUpdater", "StarXUpdater.exe") })
            {
                string p = Path.Combine(dir, n);
                if (File.Exists(p)) return p;
            }
        }
        catch { /* ignore */ }
        return null;
    }

    /// <summary>تشغيل المحدّث ثم إغلاق التطبيق. ترجع رسالة خطأ أو null عند النجاح.</summary>
    public static async Task<string?> LaunchUpdaterAsync(ReleaseInfo rel, CancellationToken ct = default)
    {
        string? updater = FindUpdater();
        if (updater == null)
            return Lang.T("upd_no_updater");

        var (sha, shaErr) = await GitHubManager.GetAssetSha256Async(rel, ct).ConfigureAwait(false);
        if (string.IsNullOrEmpty(sha))
            return Lang.T("upd_no_checksum") +
                   (shaErr == "network" ? " (" + Lang.T("upd_failed") + ")" : "");

        string args = $"--pid {Environment.ProcessId} " +
                      $"--install-dir \"{AppContext.BaseDirectory.TrimEnd(Path.DirectorySeparatorChar)}\" " +
                      $"--download-url \"{rel.DownloadUrl}\" " +
                      $"--version \"{rel.Version}\" " +
                      $"--sha256 \"{sha}\"";
        try
        {
            Process.Start(new ProcessStartInfo
            {
                FileName = updater,
                Arguments = args,
                UseShellExecute = true,
                WorkingDirectory = Path.GetDirectoryName(updater) ?? AppContext.BaseDirectory,
            });
            return null;
        }
        catch (Exception ex)
        {
            return ex.Message;
        }
    }
}
