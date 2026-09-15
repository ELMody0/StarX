using System.Diagnostics;
using System.IO.Compression;
using System.Net.Http;
using System.Security.Cryptography;

namespace StarXUpdater;

/// <summary>
/// نافذة المحدّث: تحميل ← تحقق ← انتظار الخروج ← نسخ احتياطي ← تثبيت ← تحقق ← إعادة تشغيل.
/// لا يعرض أي Dialogs — كل الأخطاء في النافذة + ملف السجل + كود الخروج.
/// </summary>
internal sealed class UpdaterForm : Form
{
    private readonly UpdaterOptions? _opts;
    private readonly string? _argError;
    private readonly Label _lblStatus = new();
    private readonly ProgressBar _bar = new();
    private readonly Label _lblDetail = new();
    private readonly Button _btnClose = new();
    private static readonly HttpClient _http = new()
    {
        Timeout = TimeSpan.FromMinutes(30),
    };

    public int ExitCode { get; private set; } = 1;
    public bool Succeeded { get; private set; }

    private string TempDir => Path.Combine(Path.GetTempPath(), "StarXUpdate");
    private string LogPath => _opts != null
        ? Path.Combine(_opts.InstallDir, "logs", "updater.log")
        : Path.Combine(TempDir, "updater.log");

    public UpdaterForm(UpdaterOptions? opts, string? argError)
    {
        _opts = opts;
        _argError = argError;
        Text = "StarX Update";
        Size = new Size(470, 230);
        MinimumSize = new Size(470, 230);
        MaximumSize = new Size(470, 230);
        StartPosition = FormStartPosition.CenterScreen;
        FormBorderStyle = FormBorderStyle.FixedDialog;
        MaximizeBox = false;
        MinimizeBox = false;
        ControlBox = false;
        BackColor = Color.Black;
        ForeColor = Color.White;

        _lblStatus.AutoSize = false;
        _lblStatus.Bounds = new Rectangle(20, 18, 410, 28);
        _lblStatus.Font = new Font("Segoe UI", 12F, FontStyle.Bold);
        _lblStatus.ForeColor = Color.White;
        _lblStatus.Text = "StarX Update";
        Controls.Add(_lblStatus);

        _bar.Bounds = new Rectangle(20, 56, 410, 26);
        _bar.Minimum = 0;
        _bar.Maximum = 100;
        _bar.Style = ProgressBarStyle.Blocks;
        Controls.Add(_bar);

        _lblDetail.AutoSize = false;
        _lblDetail.Bounds = new Rectangle(20, 90, 410, 40);
        _lblDetail.Font = new Font("Segoe UI", 9F);
        _lblDetail.ForeColor = Color.FromArgb(170, 170, 170);
        Controls.Add(_lblDetail);

        _btnClose.Text = "Close";
        _btnClose.Bounds = new Rectangle(345, 145, 85, 32);
        _btnClose.FlatStyle = FlatStyle.Flat;
        _btnClose.FlatAppearance.BorderColor = Color.White;
        _btnClose.ForeColor = Color.White;
        _btnClose.BackColor = Color.Transparent;
        _btnClose.Visible = false;
        _btnClose.Click += (s, e) => Close();
        Controls.Add(_btnClose);

        Shown += async (s, e) => await RunAsync();
    }

    private void Log(string m)
    {
        try
        {
            string? dir = Path.GetDirectoryName(LogPath);
            if (!string.IsNullOrEmpty(dir)) Directory.CreateDirectory(dir);
            File.AppendAllText(LogPath, $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] {m}\n");
        }
        catch { /* ignore */ }
    }

    private void Report(string status, string? detail = null, int pct = -1, bool marquee = false)
    {
        _lblStatus.Text = status;
        if (detail != null) _lblDetail.Text = detail;
        if (marquee) _bar.Style = ProgressBarStyle.Marquee;
        else
        {
            _bar.Style = ProgressBarStyle.Blocks;
            if (pct >= 0) _bar.Value = Math.Clamp(pct, 0, 100);
        }
    }

    private void Fail(string message)
    {
        Log("ERROR: " + message);
        _lblStatus.ForeColor = Color.FromArgb(255, 123, 123);
        Report(message, "");
        _btnClose.Visible = true;
        ControlBox = true;
        ExitCode = 1;
    }

    private async Task RunAsync()
    {
        if (_opts == null)
        {
            Fail(_argError ?? "Invalid arguments.");
            ExitCode = 2;
            return;
        }
        var o = _opts;
        Log($"Update started: current pid={o.Pid} install={o.InstallDir} version={o.Version} url={o.DownloadUrl}");
        try
        {
            // 1) انتظار خروج StarX (حدثي — بدون Sleep ثابت)
            try
            {
                var p = Process.GetProcessById(o.Pid);
                Report("Waiting for StarX to exit…", "", marquee: true);
                Log($"Waiting for pid {o.Pid}…");
                if (!p.WaitForExit(30000))
                {
                    Fail("StarX is still running. Please close StarX and try again.");
                    return;
                }
                Log("StarX exited.");
            }
            catch (ArgumentException)
            {
                Log("StarX already exited.");
            }

            // 2) التحميل إلى ملف مؤقت
            Directory.CreateDirectory(TempDir);
            string zipPath = Path.Combine(TempDir, $"StarX-v{o.Version}.zip");
            if (File.Exists(zipPath)) File.Delete(zipPath);
            Report("Downloading update…", "", 0);
            Log("Download started.");
            await DownloadAsync(o.DownloadUrl, zipPath);
            Log("Download completed: " + new FileInfo(zipPath).Length + " bytes.");

            // 3) التحقق من البصمة — ممنوع التثبيت قبلها
            Report("Verifying update…", "", marquee: true);
            string actual = Sha256File(zipPath);
            if (!actual.Equals(o.Sha256, StringComparison.OrdinalIgnoreCase))
            {
                try { File.Delete(zipPath); } catch { /* ignore */ }
                Fail("Update cancelled.\nThe downloaded update failed integrity verification.\nYour current StarX installation was not modified.");
                return;
            }
            Log("Checksum OK.");

            // 4) نسخة احتياطية
            Report("Preparing update…", "", marquee: true);
            var selfNames = SelfFileNames();
            string backup = Path.Combine(TempDir, "backup-" + DateTime.Now.ToString("yyyyMMdd-HHmmss"));
            CopyTree(o.InstallDir, backup, selfNames, skipLogs: true);
            Log("Backup at: " + backup);

            // 5) الاستبدال الآمن
            try
            {
                Report("Installing update…", "", marquee: true);
                WipeInstall(o.InstallDir, selfNames);
                ExtractZip(zipPath, o.InstallDir, selfNames);
            }
            catch (Exception ex)
            {
                Log("Install failed: " + ex.Message + " — restoring backup.");
                try
                {
                    WipeInstall(o.InstallDir, selfNames);
                    CopyTree(backup, o.InstallDir, selfNames, skipLogs: false);
                }
                catch (Exception rex)
                {
                    Fail("Installation failed and rollback failed: " + rex.Message +
                         "\nBackup kept at: " + backup);
                    return;
                }
                Fail("Installation failed — previous version restored.\n" + ex.Message);
                return;
            }

            // 6) التحقق من التثبيت
            if (!VerifyInstall(o.InstallDir, o.Version, out string why))
            {
                Log("Verify failed: " + why + " — restoring backup.");
                try
                {
                    WipeInstall(o.InstallDir, selfNames);
                    CopyTree(backup, o.InstallDir, selfNames, skipLogs: false);
                }
                catch { /* ignore */ }
                Fail("Installation verification failed — previous version restored.");
                return;
            }
            Log("Installation verified.");

            // 7) تنظيف + إعادة التشغيل
            try { File.Delete(zipPath); } catch { /* ignore */ }
            try { Directory.Delete(backup, recursive: true); } catch { /* ignore */ }
            string exe = Path.Combine(o.InstallDir, "StarX.exe");
            Report("Restarting StarX…", "", 100);
            var started = Process.Start(new ProcessStartInfo
            {
                FileName = exe,
                UseShellExecute = true,
                WorkingDirectory = o.InstallDir,
            });
            Log("StarX started (pid " + started?.Id + ").");
            Succeeded = true;
            ExitCode = 0;
            Close();
        }
        catch (Exception ex)
        {
            Fail("Update failed: " + ex.Message);
        }
    }

    private HashSet<string> SelfFileNames()
    {
        var set = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        try
        {
            string self = Process.GetCurrentProcess().MainModule?.FileName ?? "StarXUpdater.exe";
            string base_ = Path.GetFileNameWithoutExtension(self);
            set.Add(Path.GetFileName(self));
            set.Add(base_ + ".dll");
            set.Add(base_ + ".pdb");
            set.Add(base_ + ".exe");
        }
        catch { set.Add("StarXUpdater.exe"); }
        return set;
    }

    private async Task DownloadAsync(string url, string dest)
    {
        if (url.StartsWith("file:", StringComparison.OrdinalIgnoreCase))
        {
            string src = new Uri(url).LocalPath;
            using var fin = File.OpenRead(src);
            using var fout = File.Create(dest);
            await CopyWithProgressAsync(fin, fout, fin.Length);
            return;
        }
        using var res = await _http.GetAsync(url, HttpCompletionOption.ResponseHeadersRead);
        res.EnsureSuccessStatusCode();
        long? total = res.Content.Headers.ContentLength;
        using var net = await res.Content.ReadAsStreamAsync();
        using var fout2 = File.Create(dest);
        await CopyWithProgressAsync(net, fout2, total ?? -1);
    }

    private async Task CopyWithProgressAsync(Stream src, Stream dst, long total)
    {
        byte[] buf = new byte[1024 * 64];
        long done = 0;
        int n;
        while ((n = await src.ReadAsync(buf, 0, buf.Length)) > 0)
        {
            await dst.WriteAsync(buf, 0, n);
            done += n;
            if (total > 0)
            {
                int pct = (int)(done * 100 / total);
                Report("Downloading update…",
                    $"{done / 1048576.0:F1} MB / {total / 1048576.0:F1} MB", pct);
            }
            else
            {
                Report("Downloading update…", $"{done / 1048576.0:F1} MB", -1, marquee: true);
            }
        }
    }

    private static string Sha256File(string path)
    {
        using var sha = SHA256.Create();
        using var fs = File.OpenRead(path);
        return Convert.ToHexString(sha.ComputeHash(fs)).ToLowerInvariant();
    }

    private static void CopyTree(string src, string dst, HashSet<string> skipFiles, bool skipLogs)
    {
        foreach (string file in Directory.EnumerateFiles(src, "*", SearchOption.AllDirectories))
        {
            string rel = Path.GetRelativePath(src, file);
            string[] parts = rel.Split(Path.DirectorySeparatorChar);
            if (skipLogs && parts.Length > 0 &&
                parts[0].Equals("logs", StringComparison.OrdinalIgnoreCase))
                continue;
            if (skipFiles.Contains(Path.GetFileName(file)))
                continue;
            string d = Path.Combine(dst, rel);
            Directory.CreateDirectory(Path.GetDirectoryName(d)!);
            File.Copy(file, d, overwrite: true);
        }
    }

    private static void WipeInstall(string install, HashSet<string> keepFiles)
    {
        foreach (string file in Directory.EnumerateFiles(install, "*", SearchOption.AllDirectories))
        {
            string rel = Path.GetRelativePath(install, file);
            string[] parts = rel.Split(Path.DirectorySeparatorChar);
            if (parts.Length > 0 && parts[0].Equals("logs", StringComparison.OrdinalIgnoreCase))
                continue;
            if (keepFiles.Contains(Path.GetFileName(file)))
                continue;
            File.Delete(file);
        }
        foreach (string dir in Directory.EnumerateDirectories(install))
        {
            if (Path.GetFileName(dir).Equals("logs", StringComparison.OrdinalIgnoreCase))
                continue;
            if (!Directory.EnumerateFileSystemEntries(dir).Any())
                Directory.Delete(dir);
        }
    }

    private static void ExtractZip(string zipPath, string install, HashSet<string> skipFiles)
    {
        string root = Path.GetFullPath(install) + Path.DirectorySeparatorChar;
        using var zip = ZipFile.OpenRead(zipPath);
        foreach (var entry in zip.Entries)
        {
            if (string.IsNullOrEmpty(entry.Name))
                continue; // مجلد
            if (skipFiles.Contains(entry.Name))
                continue; // لا نستبدل المحدّث أثناء تشغيله
            string dest = Path.GetFullPath(Path.Combine(install,
                entry.FullName.Replace('/', Path.DirectorySeparatorChar)));
            if (!dest.StartsWith(root, StringComparison.OrdinalIgnoreCase))
                throw new InvalidOperationException("Unsafe package entry: " + entry.FullName);
            Directory.CreateDirectory(Path.GetDirectoryName(dest)!);
            entry.ExtractToFile(dest, overwrite: true);
        }
    }

    private static bool VerifyInstall(string install, string expectedVersion, out string why)
    {
        why = "";
        if (!File.Exists(Path.Combine(install, "StarX.exe")))
        {
            why = "StarX.exe missing";
            return false;
        }
        try
        {
            string vPath = Path.Combine(install, "VERSION");
            if (!File.Exists(vPath))
            {
                why = "VERSION missing";
                return false;
            }
            string v = File.ReadAllText(vPath).Trim().TrimStart('v', 'V');
            if (!v.Equals(expectedVersion.Trim().TrimStart('v', 'V'), StringComparison.OrdinalIgnoreCase))
            {
                why = $"VERSION mismatch: {v}";
                return false;
            }
        }
        catch (Exception ex)
        {
            why = ex.Message;
            return false;
        }
        return true;
    }
}
