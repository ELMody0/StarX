using System.Runtime.InteropServices;

namespace StarX_Client;

/// <summary>فئة تنظيف: جذور متعددة + أنماط ملفات.</summary>
internal sealed record JunkCategory(
    string Id, string TitleKey, Func<string[]> Roots, string[] Patterns,
    bool IsRecycle, string? DisplayPath = null);

/// <summary>نتيجة فحص: صف واحد في القائمة (يقيّمها الوكيل الذكي لاحقاً).</summary>
internal sealed record JunkResult(
    string CategoryId, string Title, string Path, string[] Roots, string[] Patterns,
    long Bytes, int Files, string VerdictKey, bool Recommended,
    int Score = 80, string Advice = "adv_system", DateTime? NewestWrite = null);

/// <summary>محرك الفحص الذكي — مجلدات مؤقتة معروفة + سلة المحذوفات فقط.</summary>
internal static class CleanerEngine
{
    private const uint SHERB_NOCONFIRMATION = 0x00000001;
    private const uint SHERB_NOPROGRESSUI = 0x00000002;
    private const uint SHERB_NOSOUND = 0x00000004;

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode, Pack = 4)]
    private struct SHQUERYRBINFO
    {
        public int cbSize;
        public long i64Size;
        public long i64NumItems;
    }

    [DllImport("Shell32.dll", CharSet = CharSet.Unicode)]
    private static extern int SHQueryRecycleBin(string? pszRootPath, ref SHQUERYRBINFO info);

    [DllImport("Shell32.dll", CharSet = CharSet.Unicode)]
    private static extern int SHEmptyRecycleBin(IntPtr hwnd, string? pszRootPath, uint dwFlags);

    private static string WinDir =>
        Environment.GetEnvironmentVariable("windir") ?? @"C:\Windows";

    private static string LocalApp =>
        Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);

    private static string AppData =>
        Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);

    private static string[] FirefoxCaches()
    {
        var list = new List<string>();
        try
        {
            string prof = Path.Combine(AppData, @"Mozilla\Firefox\Profiles");
            if (!Directory.Exists(prof)) return [];
            foreach (var d in Directory.EnumerateDirectories(prof))
            {
                string c = Path.Combine(d, "cache2");
                if (Directory.Exists(c)) list.Add(c);
            }
        }
        catch { /* ignore */ }
        return [.. list];
    }

    public static IReadOnlyList<JunkCategory> Categories() =>
    [
        new("wintemp", "clean_wintemp",
            () => [Path.Combine(WinDir, "Temp")], ["*"], false),
        new("usertemp", "clean_usertemp",
            () => [Path.GetTempPath().TrimEnd(Path.DirectorySeparatorChar)], ["*"], false),
        new("prefetch", "clean_prefetch",
            () => [Path.Combine(WinDir, "Prefetch")], ["*"], false),
        new("softdist", "clean_softdist",
            () => [Path.Combine(WinDir, "SoftwareDistribution", "Download")], ["*"], false),
        new("cbs", "clean_cbs",
            () => [Path.Combine(WinDir, "Logs", "CBS")], ["*"], false),
        new("thumb", "clean_thumb",
            () => [Path.Combine(LocalApp, @"Microsoft\Windows\Explorer")],
            ["thumbcache_*.db", "iconcache_*.db"], false),
        new("chrome", "clean_chrome",
            () => [Path.Combine(LocalApp, @"Google\Chrome\User Data\Default\Cache"),
                   Path.Combine(LocalApp, @"Google\Chrome\User Data\Default\Code Cache")],
            ["*"], false),
        new("edge", "clean_edge",
            () => [Path.Combine(LocalApp, @"Microsoft\Edge\User Data\Default\Cache"),
                   Path.Combine(LocalApp, @"Microsoft\Edge\User Data\Default\Code Cache")],
            ["*"], false),
        new("firefox", "clean_firefox", FirefoxCaches, ["*"], false,
            Path.Combine(AppData, @"Mozilla\Firefox\Profiles")),
        new("inetcache", "clean_inetcache",
            () => [Path.Combine(LocalApp, @"Microsoft\Windows\INetCache")], ["*"], false),
        new("dxcache", "clean_dxcache",
            () => [Path.Combine(LocalApp, "D3DSCache")], ["*"], false),
        new("recycle", "clean_recycle", () => [], ["*"], true),
    ];

    public static string TitleOf(JunkCategory c) => Lang.T(c.TitleKey);
    public static string VerdictOf(bool safe) => Lang.T(safe ? "clean_safe" : "clean_review");

    // ---------- أماكن مستثناة من الفحص والمسح ----------

    public static List<string> GetExcludes()
    {
        var list = new List<string>();
        try
        {
            string raw = AppSettings.GetString("cleanExcludes", "[]");
            using var doc = System.Text.Json.JsonDocument.Parse(raw);
            if (doc.RootElement.ValueKind != System.Text.Json.JsonValueKind.Array) return list;
            foreach (var e in doc.RootElement.EnumerateArray())
            {
                if (e.ValueKind != System.Text.Json.JsonValueKind.String) continue;
                string? p = NormalizePath(e.GetString());
                if (p != null && !list.Contains(p, StringComparer.OrdinalIgnoreCase)) list.Add(p);
            }
        }
        catch { /* ignore */ }
        return list;
    }

    public static void SetExcludes(IEnumerable<string> paths)
    {
        var list = new List<string>();
        foreach (var p in paths)
        {
            string? n = NormalizePath(p);
            if (n != null && !list.Contains(n, StringComparer.OrdinalIgnoreCase)) list.Add(n);
        }
        AppSettings.Set("cleanExcludes", System.Text.Json.JsonSerializer.Serialize(list));
    }

    private static string? NormalizePath(string? p)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(p)) return null;
            return Path.GetFullPath(p.Trim()).TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
        }
        catch { return null; }
    }

    private static bool IsExcluded(string path, List<string> excludes)
    {
        if (excludes.Count == 0) return false;
        string? full = NormalizePath(path);
        if (full == null) return false;
        foreach (var ex in excludes)
        {
            if (full.Equals(ex, StringComparison.OrdinalIgnoreCase)) return true;
            if (full.StartsWith(ex + Path.DirectorySeparatorChar, StringComparison.OrdinalIgnoreCase)) return true;
        }
        return false;
    }

    public static async Task<List<JunkResult>> ScanAsync(
        IProgress<string>? progress, CancellationToken ct)
    {
        var rows = new List<JunkResult>();
        foreach (var cat in Categories())
        {
            ct.ThrowIfCancellationRequested();
            progress?.Report(TitleOf(cat));
            if (cat.IsRecycle)
            {
                rows.Add(ScanRecycle());
                continue;
            }
            var r = await Task.Run(() => ScanRoots(cat, progress, ct), ct).ConfigureAwait(false);
            if (r.Bytes > 0 || r.Files > 0)
                rows.Add(r);
        }
        rows.Sort((a, b) =>
        {
            int s = b.Recommended.CompareTo(a.Recommended);
            return s != 0 ? s : b.Bytes.CompareTo(a.Bytes);
        });
        return rows;
    }

    private static JunkResult ScanRoots(
        JunkCategory cat, IProgress<string>? progress, CancellationToken ct)
    {
        long bytes = 0;
        int files = 0;
        int seen = 0;
        DateTime? newest = null;
        string[] roots;
        try { roots = cat.Roots(); }
        catch { roots = []; }
        var live = roots.Where(Directory.Exists).ToArray();
        string display = cat.DisplayPath ?? (live.FirstOrDefault() ?? roots.FirstOrDefault() ?? "");
        var excludes = GetExcludes();

        foreach (string root in live)
        {
            if (ct.IsCancellationRequested) break;
            if (IsExcluded(root, excludes)) continue;
            foreach (string pattern in cat.Patterns)
            {
                if (ct.IsCancellationRequested) break;
                var stack = new Stack<string>();
                stack.Push(root);
                while (stack.Count > 0)
                {
                    if (ct.IsCancellationRequested) break;
                    string dir = stack.Pop();
                    if (IsExcluded(dir, excludes)) continue;
                    string[] sub;
                    try { sub = Directory.GetDirectories(dir); }
                    catch { continue; }
                    foreach (var d in sub) stack.Push(d);
                    string[] names;
                    try { names = Directory.GetFiles(dir, pattern); }
                    catch { continue; }
                    foreach (var f in names)
                    {
                        if (IsExcluded(f, excludes)) continue;
                        try
                        {
                            var fi = new FileInfo(f);
                            bytes += fi.Length;
                            DateTime w = fi.LastWriteTimeUtc;
                            if (newest == null || w > newest) newest = w;
                            files++;
                            if (++seen % 800 == 0)
                                progress?.Report(TitleOf(cat) + $" — {files}");
                        }
                        catch { /* مستخدم أو بلا صلاحية */ }
                    }
                }
            }
        }
        return new JunkResult(cat.Id, TitleOf(cat), display, live, cat.Patterns,
            bytes, files, "clean_safe", true, NewestWrite: newest);
    }

    private static JunkResult ScanRecycle()
    {
        try
        {
            var info = new SHQUERYRBINFO { cbSize = Marshal.SizeOf<SHQUERYRBINFO>() };
            if (SHQueryRecycleBin(null, ref info) == 0)
                return new JunkResult("recycle", Lang.T("clean_recycle"), Lang.T("clean_recycle_path"),
                    [], ["*"], info.i64Size, (int)Math.Min(info.i64NumItems, int.MaxValue),
                    "clean_review", false);
        }
        catch { /* ignore */ }
        return new JunkResult("recycle", Lang.T("clean_recycle"), Lang.T("clean_recycle_path"),
            [], ["*"], 0, 0, "clean_review", false);
    }

    public static async Task<(int Deleted, long Freed, int Failed)> DeleteAsync(
        IEnumerable<JunkResult> selected, IProgress<double>? progress, CancellationToken ct,
        List<CleanedFile>? deletedFiles = null)
    {
        var list = selected.ToList();
        // كل الشغل في خيط خلفية عشان الواجهة ماتقفش — التقدم يوصل عبر IProgress فقط
        return await Task.Run(() =>
        {
            int deleted = 0, failed = 0;
            long freed = 0;
            long totalFiles = 0;
            foreach (var r in list)
                if (r.CategoryId != "recycle") totalFiles += r.Files;
            long done = 0;
            int lastPct = -1;

            foreach (var r in list)
            {
                ct.ThrowIfCancellationRequested();
                if (r.CategoryId == "recycle")
                {
                    try
                    {
                        int hr = SHEmptyRecycleBin(IntPtr.Zero, null,
                            SHERB_NOCONFIRMATION | SHERB_NOPROGRESSUI | SHERB_NOSOUND);
                        if (hr == 0) { deleted += r.Files; freed += r.Bytes; }
                        else failed += Math.Max(r.Files, 1);
                    }
                    catch { failed += Math.Max(r.Files, 1); }
                    continue;
                }
                try
                {
                    foreach (string f in EnumerateAllFiles(r, ct))
                    {
                        if (ct.IsCancellationRequested) break;
                        try
                        {
                            long len = new FileInfo(f).Length;
                            File.Delete(f);
                            deleted++;
                            freed += len;
                            if (deletedFiles != null && deletedFiles.Count < 1000)
                                deletedFiles.Add(new CleanedFile(f, len));
                        }
                        catch { failed++; }
                        done++;
                        // بلّغ عند تغيّر النسبة فقط عشان ماتغرقش الواجهة
                        if (totalFiles > 0)
                        {
                            int pct = (int)(done * 100 / totalFiles);
                            if (pct != lastPct) { lastPct = pct; progress?.Report(pct / 100.0); }
                        }
                    }
                    foreach (string root in r.Roots)
                        RemoveEmptyDirs(root);
                }
                catch { failed++; }
            }
            progress?.Report(1.0);
            return (deleted, freed, failed);
        }, ct).ConfigureAwait(false);
    }

    private static IEnumerable<string> EnumerateAllFiles(JunkResult r, CancellationToken ct)
    {
        var excludes = GetExcludes();
        foreach (string root in r.Roots)
        {
            if (!Directory.Exists(root)) continue;
            if (IsExcluded(root, excludes)) continue;
            foreach (string pattern in r.Patterns)
            {
                var stack = new Stack<string>();
                stack.Push(root);
                while (stack.Count > 0)
                {
                    if (ct.IsCancellationRequested) yield break;
                    string dir = stack.Pop();
                    if (IsExcluded(dir, excludes)) continue;
                    string[] sub, names;
                    try { sub = Directory.GetDirectories(dir); }
                    catch { continue; }
                    foreach (var d in sub) stack.Push(d);
                    try { names = Directory.GetFiles(dir, pattern); }
                    catch { continue; }
                    foreach (var f in names)
                    {
                        if (IsExcluded(f, excludes)) continue;
                        yield return f;
                    }
                }
            }
        }
    }

    private static void RemoveEmptyDirs(string root)
    {
        try
        {
            var dirs = Directory.EnumerateDirectories(root, "*",
                    new EnumerationOptions { RecurseSubdirectories = true })
                .OrderByDescending(d => d.Length).ToList();
            foreach (var d in dirs)
            {
                try
                {
                    if (!Directory.EnumerateFileSystemEntries(d).Any())
                        Directory.Delete(d);
                }
                catch { /* ignore */ }
            }
        }
        catch { /* ignore */ }
    }

    public static string FormatSize(long bytes)
    {
        string[] units = ["B", "KB", "MB", "GB", "TB"];
        double v = bytes;
        int u = 0;
        while (v >= 1024 && u < units.Length - 1) { v /= 1024; u++; }
        return u == 0 ? $"{bytes} {units[u]}" : $"{v:F1} {units[u]}";
    }
}

/// <summary>سجل عمليات التنظيف + الإجماليات (آخر 30 عملية) + أحدث الملفات المنظفة.</summary>
internal sealed record CleanRecord(DateTime At, int Deleted, long Freed, int Failed);

/// <summary>ملف منظف مع حجمه وقت الحذف (للترتيب بالأهم: الأكبر أولاً).</summary>
internal sealed record CleanedFile(string Path, long Size);

internal static class CleanHistory
{
    public const int MaxRecentFiles = 200;

    // للاختبارات فقط
#pragma warning disable CS0649
    internal static string? FileOverride;
#pragma warning restore CS0649

    private static string FilePath =>
        FileOverride ?? Path.Combine(AppSettings.Dir, "clean_history.json");

    public static void Add(int deleted, long freed, int failed, IEnumerable<CleanedFile>? files = null)
    {
        try
        {
            var all = LoadAll();
            all.Insert(0, new CleanRecord(DateTime.UtcNow, deleted, freed, failed));
            while (all.Count > 30) all.RemoveAt(all.Count - 1);
            var recent = LoadRecentFiles();
            if (files != null)
            {
                foreach (var f in files)
                {
                    if (string.IsNullOrWhiteSpace(f.Path)) continue;
                    recent.RemoveAll(x => x.Path.Equals(f.Path, StringComparison.OrdinalIgnoreCase));
                    recent.Insert(0, f);
                }
                recent.Sort((a, b) => b.Size.CompareTo(a.Size));
                while (recent.Count > MaxRecentFiles) recent.RemoveAt(recent.Count - 1);
            }
            Directory.CreateDirectory(Path.GetDirectoryName(FilePath)!);
            using var ms = new MemoryStream();
            using (var w = new System.Text.Json.Utf8JsonWriter(ms))
            {
                w.WriteStartObject();
                w.WriteStartArray("runs");
                foreach (var r in all)
                {
                    w.WriteStartObject();
                    w.WriteString("at", r.At.ToString("o"));
                    w.WriteNumber("deleted", r.Deleted);
                    w.WriteNumber("freed", r.Freed);
                    w.WriteNumber("failed", r.Failed);
                    w.WriteEndObject();
                }
                w.WriteEndArray();
                w.WriteStartArray("recentFiles");
                foreach (var f in recent)
                {
                    w.WriteStartObject();
                    w.WriteString("p", f.Path);
                    w.WriteNumber("s", f.Size);
                    w.WriteEndObject();
                }
                w.WriteEndArray();
                w.WriteEndObject();
            }
            File.WriteAllBytes(FilePath, ms.ToArray());
        }
        catch { /* ignore */ }
    }

    public static IReadOnlyList<CleanedFile> RecentFiles()
    {
        var list = LoadRecentFiles();
        list.Sort((a, b) => b.Size.CompareTo(a.Size));
        return list;
    }

    public static (int Runs, long Freed) Totals()
    {
        try
        {
            var all = LoadAll();
            long f = 0;
            foreach (var r in all) f += r.Freed;
            return (all.Count, f);
        }
        catch { return (0, 0); }
    }

    private static List<CleanedFile> LoadRecentFiles()
    {
        var list = new List<CleanedFile>();
        try
        {
            if (!File.Exists(FilePath)) return list;
            using var doc = System.Text.Json.JsonDocument.Parse(File.ReadAllText(FilePath));
            if (!doc.RootElement.TryGetProperty("recentFiles", out var arr) ||
                arr.ValueKind != System.Text.Json.JsonValueKind.Array) return list;
            foreach (var e in arr.EnumerateArray())
            {
                if (e.ValueKind == System.Text.Json.JsonValueKind.String)
                {
                    // صيغة قديمة: مسار فقط
                    string? s = e.GetString();
                    if (!string.IsNullOrWhiteSpace(s)) list.Add(new CleanedFile(s, 0));
                }
                else if (e.ValueKind == System.Text.Json.JsonValueKind.Object)
                {
                    string p = e.TryGetProperty("p", out var pe) &&
                        pe.ValueKind == System.Text.Json.JsonValueKind.String
                        ? pe.GetString() ?? "" : "";
                    long s = e.TryGetProperty("s", out var se) &&
                        se.ValueKind == System.Text.Json.JsonValueKind.Number
                        ? se.GetInt64() : 0;
                    if (!string.IsNullOrWhiteSpace(p) &&
                        !list.Exists(x => x.Path.Equals(p, StringComparison.OrdinalIgnoreCase)))
                        list.Add(new CleanedFile(p, Math.Max(0, s)));
                }
                if (list.Count >= MaxRecentFiles) break;
            }
        }
        catch { /* ignore */ }
        return list;
    }

    private static List<CleanRecord> LoadAll()
    {
        var list = new List<CleanRecord>();
        try
        {
            if (!File.Exists(FilePath)) return list;
            using var doc = System.Text.Json.JsonDocument.Parse(File.ReadAllText(FilePath));
            if (!doc.RootElement.TryGetProperty("runs", out var arr) ||
                arr.ValueKind != System.Text.Json.JsonValueKind.Array) return list;
            foreach (var e in arr.EnumerateArray())
            {
                DateTime at = e.TryGetProperty("at", out var a) &&
                    a.ValueKind == System.Text.Json.JsonValueKind.String &&
                    DateTime.TryParse(a.GetString(), null,
                        System.Globalization.DateTimeStyles.RoundtripKind, out var dt)
                    ? dt : DateTime.MinValue;
                int del = e.TryGetProperty("deleted", out var d) && d.ValueKind == System.Text.Json.JsonValueKind.Number
                    ? d.GetInt32() : 0;
                long fr = e.TryGetProperty("freed", out var fr2) && fr2.ValueKind == System.Text.Json.JsonValueKind.Number
                    ? fr2.GetInt64() : 0;
                int fa = e.TryGetProperty("failed", out var fa2) && fa2.ValueKind == System.Text.Json.JsonValueKind.Number
                    ? fa2.GetInt32() : 0;
                list.Add(new CleanRecord(at, del, fr, fa));
            }
        }
        catch { /* ignore */ }
        return list;
    }
}
