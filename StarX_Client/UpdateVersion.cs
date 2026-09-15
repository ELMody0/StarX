namespace StarX_Client;

/// <summary>مقارنة إصدارات دلالية (1.10.0 أكبر من 1.9.0) — بدون مقارنة نصية.</summary>
internal readonly record struct UpdateVersion(int Major, int Minor, int Patch) : IComparable<UpdateVersion>
{
    public static string Normalize(string? v) =>
        TryParse(v, out var p) ? p.ToString() : (v ?? string.Empty).Trim();

    public static bool TryParse(string? s, out UpdateVersion v)
    {
        v = default;
        if (string.IsNullOrWhiteSpace(s)) return false;
        s = s.Trim();
        if (s.StartsWith("v", StringComparison.OrdinalIgnoreCase)) s = s[1..];
        int cut = s.IndexOfAny(['-', '+']);
        if (cut >= 0) s = s[..cut];
        var parts = s.Split('.');
        if (parts.Length == 0 || parts.Length > 3) return false;
        int[] n = [0, 0, 0];
        for (int i = 0; i < parts.Length; i++)
        {
            if (!int.TryParse(parts[i].Trim(), out n[i]) || n[i] < 0) return false;
        }
        v = new UpdateVersion(n[0], n[1], n[2]);
        return true;
    }

    public int CompareTo(UpdateVersion o)
    {
        int c = Major.CompareTo(o.Major);
        if (c != 0) return c;
        c = Minor.CompareTo(o.Minor);
        if (c != 0) return c;
        return Patch.CompareTo(o.Patch);
    }

    public override string ToString() => $"{Major}.{Minor}.{Patch}";
}
