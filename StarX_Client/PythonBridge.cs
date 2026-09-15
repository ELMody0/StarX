using System.Diagnostics;
using System.Text;
using System.Text.Json;

namespace StarX_Client;

/// <summary>نتيجة أمر Python (بروتوكول JSON).</summary>
internal sealed record PythonResult(bool Ok, string Raw, JsonDocument? Doc, string Error)
{
    public int GetInt(string name, int fallback = 0)
    {
        try
        {
            if (Doc != null && Doc.RootElement.TryGetProperty(name, out var e) && e.TryGetInt32(out int v))
                return v;
        }
        catch { /* fallback */ }
        return fallback;
    }

    public string GetString(string name, string fallback = "")
    {
        try
        {
            if (Doc != null && Doc.RootElement.TryGetProperty(name, out var e) &&
                e.ValueKind == JsonValueKind.String)
                return e.GetString() ?? fallback;
        }
        catch { /* fallback */ }
        return fallback;
    }
}

/// <summary>
/// جسر C# → Python: يشغّل محرك scripts/starx_tools.py كعملية خارجية
/// ببروتوكول JSON، مع اكتشاف المفسر ومهلة زمنية وإلغاء.
/// </summary>
internal static class PythonBridge
{
    private static string[]? _interpreter; // [exe, ...prefixArgs]
    private static readonly Lock _lock = new();
    private static string? _scriptPath;

    public const int DefaultTimeoutMs = 15000;

    public static string ScriptPath
    {
        get
        {
            if (_scriptPath != null) return _scriptPath;
            string[] probes =
            [
                Path.Combine(AppContext.BaseDirectory, "scripts", "starx_tools.py"),
                Path.Combine(Environment.CurrentDirectory, "scripts", "starx_tools.py"),
                Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "..", "scripts", "starx_tools.py")),
            ];
            _scriptPath = probes.FirstOrDefault(File.Exists)
                ?? throw new FileNotFoundException("لم يتم العثور على starx_tools.py بجانب التطبيق.");
            return _scriptPath;
        }
    }

    public static async Task<(bool Available, string Detail, string? Interpreter)> CheckHealthAsync(
        int timeoutMs = 8000, CancellationToken ct = default)
    {
        try
        {
            var r = await RunAsync("version", timeoutMs, ct).ConfigureAwait(false);
            if (!r.Ok) return (false, r.Error, null);
            string py = r.GetString("python", "?");
            string v = r.GetString("version", "?");
            string interp;
            lock (_lock) interp = _interpreter == null ? "?" : string.Join(" ", _interpreter);
            return (true, $"Python {py} • starx-python {v}", interp);
        }
        catch (Exception ex)
        {
            return (false, ex.Message, null);
        }
    }

    public static Task<PythonResult> RunAsync(string command, CancellationToken ct, params string[] args)
        => RunAsync(command, DefaultTimeoutMs, ct, args);

    public static async Task<PythonResult> RunAsync(
        string command, int timeoutMs, CancellationToken ct, params string[] args)
    {
        string script;
        try { script = ScriptPath; }
        catch (Exception ex) { return new PythonResult(false, string.Empty, null, ex.Message); }

        string[]? interp = await FindInterpreterAsync(timeoutMs, ct).ConfigureAwait(false);
        if (interp == null)
            return new PythonResult(false, string.Empty, null,
                "لا يوجد مفسر Python (جرّب تثبيت Python 3 أو ضبط STARX_PYTHON).");

        var (exit, stdout, stderr) = await ExecAsync(interp, [script, command, .. args], timeoutMs, ct)
            .ConfigureAwait(false);

        JsonDocument? doc = null;
        bool okFlag = false;
        string error = string.Empty;
        try
        {
            doc = JsonDocument.Parse(stdout.Trim());
            if (doc.RootElement.TryGetProperty("ok", out var okEl) &&
                okEl.ValueKind == JsonValueKind.True)
                okFlag = true;
            else if (doc.RootElement.TryGetProperty("error", out var errEl) &&
                     errEl.ValueKind == JsonValueKind.String)
                error = errEl.GetString() ?? string.Empty;
        }
        catch
        {
            error = "مخرجات Python غير صالحة (ليست JSON).";
        }

        bool ok = exit == 0 && okFlag;
        if (!ok && string.IsNullOrEmpty(error))
            error = string.IsNullOrWhiteSpace(stderr)
                ? $"خرج Python بكود {exit}."
                : stderr.Trim().Length > 300 ? stderr.Trim()[..300] + "…" : stderr.Trim();

        return new PythonResult(ok, stdout, ok ? doc : null, ok ? string.Empty : error);
    }

    private static async Task<string[]?> FindInterpreterAsync(int timeoutMs, CancellationToken ct)
    {
        lock (_lock)
        {
            if (_interpreter != null) return _interpreter;
        }

        var candidates = new List<string[]>();
        string? env = Environment.GetEnvironmentVariable("STARX_PYTHON");
        if (!string.IsNullOrWhiteSpace(env)) candidates.Add([env.Trim()]);
        candidates.Add(["python3"]);
        candidates.Add(["python"]);
        candidates.Add(["py", "-3"]);

        using var cts = CancellationTokenSource.CreateLinkedTokenSource(ct);
        cts.CancelAfter(Math.Min(timeoutMs, 6000));
        foreach (var c in candidates)
        {
            try
            {
                var (exit, stdout, _) = await ExecAsync(c, ["--version"], 5000, cts.Token)
                    .ConfigureAwait(false);
                if (exit == 0 && stdout.Trim().StartsWith("Python ", StringComparison.OrdinalIgnoreCase))
                {
                    lock (_lock) _interpreter = c;
                    return c;
                }
            }
            catch { /* جرّب التالي */ }
        }
        return null;
    }

    private static async Task<(int Exit, string Out, string Err)> ExecAsync(
        string[] exe, string[] args, int timeoutMs, CancellationToken ct)
    {
        using var proc = new Process();
        proc.StartInfo.FileName = exe[0];
        foreach (var a in exe.Skip(1)) proc.StartInfo.ArgumentList.Add(a);
        foreach (var a in args) proc.StartInfo.ArgumentList.Add(a);
        proc.StartInfo.UseShellExecute = false;
        proc.StartInfo.CreateNoWindow = true;
        proc.StartInfo.RedirectStandardOutput = true;
        proc.StartInfo.RedirectStandardError = true;
        proc.StartInfo.StandardOutputEncoding = Encoding.UTF8;
        proc.StartInfo.StandardErrorEncoding = Encoding.UTF8;

        var sbOut = new StringBuilder();
        var sbErr = new StringBuilder();
        proc.OutputDataReceived += (s, e) => { if (e.Data != null) sbOut.AppendLine(e.Data); };
        proc.ErrorDataReceived += (s, e) => { if (e.Data != null) sbErr.AppendLine(e.Data); };

        if (!proc.Start())
            throw new InvalidOperationException($"تعذر تشغيل {exe[0]}.");
        proc.BeginOutputReadLine();
        proc.BeginErrorReadLine();

        using var cts = CancellationTokenSource.CreateLinkedTokenSource(ct);
        cts.CancelAfter(timeoutMs);
        try
        {
            await proc.WaitForExitAsync(cts.Token).ConfigureAwait(false);
        }
        catch (OperationCanceledException)
        {
            try { if (!proc.HasExited) proc.Kill(entireProcessTree: true); } catch { /* ignore */ }
            throw new TimeoutException($"انتهت مهلة Python ({timeoutMs}ms).");
        }
        return (proc.ExitCode, sbOut.ToString(), sbErr.ToString());
    }
}
