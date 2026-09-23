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
        "no-signature" => Lang.T("upd_no_signature"),
        "bad-signature" => Lang.T("upd_bad_signature"),
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
        // نفس حماية المحدّث: مجلد التطوير (فيه .pdb/.csproj) لا يُحدَّث —
        // ونرجع رسالة بدل ما نخرج من التطبيق على الفاضي.
        try
        {
            foreach (var f in Directory.EnumerateFiles(
                AppContext.BaseDirectory, "*", SearchOption.TopDirectoryOnly))
            {
                string ext = Path.GetExtension(f);
                if (ext.Equals(".pdb", StringComparison.OrdinalIgnoreCase) ||
                    ext.Equals(".csproj", StringComparison.OrdinalIgnoreCase))
                    return Lang.T("upd_dev_folder");
            }
        }
        catch { /* ignore → proceed */ }

        string? updater = FindUpdater();
        if (updater == null)
            return Lang.T("upd_no_updater") + "\n" +
                   Path.Combine(AppContext.BaseDirectory, "StarXUpdater.exe");

        var (sha, shaErr) = await GitHubManager.GetAssetSha256Async(rel, ct).ConfigureAwait(false);
        if (string.IsNullOrEmpty(sha))
            return Lang.T("upd_no_checksum") +
                   (shaErr == "network" ? " (" + Lang.T("upd_failed") + ")" : "");

        // توقيع ECDSA — خط الدفاع ضد اختطاع حساب GitHub (المفتاح العام مضمّن)
        var (sig, sigErr) = await GitHubManager.GetAssetSignatureAsync(rel, ct).ConfigureAwait(false);
#if !DEBUG
        if (string.IsNullOrEmpty(sig))
            return Lang.T("upd_no_signature") +
                   (sigErr == "network" ? " (" + Lang.T("upd_failed") + ")" : "");
#endif
        if (!string.IsNullOrEmpty(sig) &&
            !StarXShared.UpdateSecurity.VerifySha256Hex(sha, sig))
            return Lang.T("upd_bad_signature");
#if DEBUG
        // في التطوير: السماح بغياب التوقيع للتجربة المحلية فقط
        if (string.IsNullOrEmpty(sig))
            sig = "";
#endif

        string args = $"--pid {Environment.ProcessId} " +
                      $"--install-dir \"{AppContext.BaseDirectory.TrimEnd(Path.DirectorySeparatorChar)}\" " +
                      $"--download-url \"{rel.DownloadUrl}\" " +
                      $"--version \"{rel.Version}\" " +
                      $"--sha256 \"{sha}\"" +
                      (string.IsNullOrEmpty(sig) ? "" : $" --sig \"{sig}\"");
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
