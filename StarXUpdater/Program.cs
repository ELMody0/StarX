namespace StarXUpdater;

/// <summary>خيارات سطر الأوامر للمحدّث (يمررها StarX.exe).</summary>
internal sealed record UpdaterOptions(
    int Pid, string InstallDir, string DownloadUrl, string Version, string Sha256);

internal static class UpdaterArgs
{
    public static bool TryParse(string[] args, out UpdaterOptions? opts, out string error)
    {
        opts = null;
        error = "";
        var map = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        for (int i = 0; i < args.Length; i++)
        {
            string a = args[i];
            if (!a.StartsWith("--")) { error = $"Unknown argument: {a}"; return false; }
            string key = a[2..];
            if (i + 1 >= args.Length || args[i + 1].StartsWith("--"))
            { error = $"Missing value for: {a}"; return false; }
            map[key] = args[++i];
        }

        if (!map.TryGetValue("pid", out string? pidS) || !int.TryParse(pidS, out int pid) || pid <= 0)
        { error = "Missing or invalid: --pid <process-id>"; return false; }
        if (!map.TryGetValue("install-dir", out string? dir) || string.IsNullOrWhiteSpace(dir) ||
            !Directory.Exists(dir))
        { error = "Missing or invalid: --install-dir <path>"; return false; }
        if (!map.TryGetValue("download-url", out string? url) || !IsAllowedUrl(url))
        { error = "Missing or invalid: --download-url <https|file url>"; return false; }
        if (!map.TryGetValue("version", out string? ver) || !IsSemVer(ver))
        { error = "Missing or invalid: --version <x.y.z>"; return false; }
        if (!map.TryGetValue("sha256", out string? sha) || sha.Length != 64 || !sha.All(Uri.IsHexDigit))
        { error = "Missing or invalid: --sha256 <64 hex chars>"; return false; }

        opts = new UpdaterOptions(pid, Path.GetFullPath(dir.Trim()), url.Trim(),
            ver.Trim().TrimStart('v', 'V'), sha.ToLowerInvariant());
        return true;
    }

    private static bool IsAllowedUrl(string? url)
    {
        if (!Uri.TryCreate(url?.Trim(), UriKind.Absolute, out var u))
            return false;
        if (u.Scheme == Uri.UriSchemeHttps || u.Scheme == Uri.UriSchemeFile)
            return true;
        // http للمختبرات المحلية فقط
        return u.Scheme == Uri.UriSchemeHttp && u.IsLoopback;
    }

    private static bool IsSemVer(string? v)
    {
        if (string.IsNullOrWhiteSpace(v)) return false;
        v = v.Trim().TrimStart('v', 'V');
        var parts = v.Split('.');
        return parts.Length == 3 && parts.All(p => int.TryParse(p, out int n) && n >= 0);
    }
}

internal static class Program
{
    [STAThread]
    static int Main(string[] args)
    {
        ApplicationConfiguration.Initialize();
        if (!UpdaterArgs.TryParse(args, out var opts, out string error))
        {
            using var bad = new UpdaterForm(null, "Invalid arguments.\n" + error);
            Application.Run(bad);
            return 2;
        }
        using var form = new UpdaterForm(opts, null);
        Application.Run(form);
        return form.ExitCode;
    }
}
