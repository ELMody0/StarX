namespace StarX_Client;

/// <summary>سياق سريع عن الجهاز (بدون أي انتظار أو صلاحيات خاصة).</summary>
internal sealed record DeviceContext(double FreePercent, long FreeBytes, string Os, long RamBytes);

/// <summary>
/// وكيل محلي ذكي: يفهم حالة الجهاز (المساحة/الرام/النظام) ويقيّم كل نتيجة
/// فحص بدرجة ثقة وسبب واضح، ويعلّم على الآمن تلقائياً — والقرار الأخير لك.
/// يعمل بالكامل على الجهاز بدون إنترنت.
/// </summary>
internal static class SmartAdvisor
{
    public const int RecommendThreshold = 70;

    public static DeviceContext GetContext()
    {
        double pct = 100;
        long free = 0;
        try
        {
            string? root = Path.GetPathRoot(Environment.SystemDirectory);
            var d = new DriveInfo(string.IsNullOrEmpty(root) ? "C:\\" : root);
            if (d.IsReady && d.TotalSize > 0)
            {
                free = d.AvailableFreeSpace;
                pct = 100.0 * d.AvailableFreeSpace / d.TotalSize;
            }
        }
        catch { /* ignore */ }
        long ram = 0;
        try { ram = (long)GC.GetGCMemoryInfo().TotalAvailableMemoryBytes; } catch { /* ignore */ }
        string os;
        try { os = Environment.OSVersion.VersionString; } catch { os = ""; }
        return new DeviceContext(pct, free, os, ram);
    }

    public static List<JunkResult> Analyze(List<JunkResult> rows, CloudRules? cloud = null)
    {
        var ctx = GetContext();
        var result = new List<JunkResult>(rows.Count);
        foreach (var r in rows)
        {
            var (score, advice) = Score(r, ctx, cloud);
            bool rec = score >= RecommendThreshold && r.Bytes > 0;
            result.Add(r with
            {
                Recommended = rec,
                VerdictKey = rec ? "clean_safe" : "clean_review",
                Score = score,
                Advice = advice,
            });
        }
        return result;
    }

    public static string Describe(JunkResult r) =>
        string.IsNullOrEmpty(r.Advice) ? "" : Lang.T(r.Advice);

    public static string Summarize(IReadOnlyList<JunkResult> rows, DeviceContext? ctx = null)
    {
        ctx ??= GetContext();
        int nSafe = 0, nRev = 0;
        long bSafe = 0, bRev = 0;
        foreach (var r in rows)
        {
            if (r.Recommended) { nSafe++; bSafe += r.Bytes; }
            else { nRev++; bRev += r.Bytes; }
        }
        string t = $"{Lang.T("ai_title")}: {nSafe} {Lang.T("ai_safe")} ({CleanerEngine.FormatSize(bSafe)})";
        if (nRev > 0)
            t += $" • {nRev} {Lang.T("ai_review")} ({CleanerEngine.FormatSize(bRev)})";
        if (ctx.FreePercent < 10)
            t += $" • {Lang.T("ai_lowdisk")} ({ctx.FreePercent:F0}%)";
        return t;
    }

    private static (int Score, string Advice) Score(JunkResult r, DeviceContext ctx, CloudRules? cloud)
    {
        int s;
        string adv;
        int cloudScoreVal = 0;
        bool cloudScore = cloud != null && cloud.Scores.TryGetValue(r.CategoryId, out cloudScoreVal);
        if (cloudScore)
        {
            s = cloudScoreVal;
            adv = "adv_system";
        }
        else
        {
            switch (r.CategoryId)
            {
                case "recycle": s = 40; adv = "adv_bin"; break;
                case "dxcache": s = 62; adv = "adv_shader"; break;
                case "chrome":
                case "edge":
                case "firefox": s = 76; adv = "adv_cache"; break;
                case "inetcache": s = 80; adv = "adv_cache"; break;
                case "softdist":
                case "prefetch": s = 60; adv = "adv_review_sys"; break;
                case "usertemp":
                case "wintemp": s = 86; adv = "adv_system"; break;
                case "thumb": s = 90; adv = "adv_cache"; break;
                case "cbs": s = 92; adv = "adv_logs"; break;
                default: s = 70; adv = "adv_system"; break;
            }
        }

        if (cloud != null && cloud.Advice.TryGetValue(r.CategoryId, out string? cloudAdv) &&
            !string.IsNullOrWhiteSpace(cloudAdv))
            adv = cloudAdv;

        bool sensitive = adv is "adv_bin" or "adv_shader" or "adv_review_sys";
        if (r.NewestWrite is DateTime nw)
        {
            DateTime utc;
            try { utc = nw.Kind == DateTimeKind.Utc ? nw : nw.ToUniversalTime(); }
            catch { utc = DateTime.UtcNow; }
            var age = DateTime.UtcNow - utc;
            if (age < TimeSpan.FromDays(1))
            {
                s -= 30;
                if (!sensitive) adv = "adv_fresh";
            }
            else if (age < TimeSpan.FromDays(7))
            {
                s -= 12;
                if (!sensitive) adv = "adv_fresh";
            }
            else if (age > TimeSpan.FromDays(30))
            {
                s += 4;
                adv = r.Bytes > 100L * 1024 * 1024 ? "adv_old_big" : "adv_old";
            }
        }

        if (r.Bytes < 256L * 1024 && s >= RecommendThreshold && !sensitive && adv != "adv_fresh")
            adv = "adv_tiny";

        if (ctx.FreePercent < 10 && r.Bytes > 50L * 1024 * 1024)
            s += 8;

        return (Math.Clamp(s, 0, 100), adv);
    }
}
