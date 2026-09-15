namespace StarX_Client;

/// <summary>مصدر الإصدار المفرد: ملف VERSION بجانب التطبيق.</summary>
internal static class AppVersion
{
    public static string Current { get; } = Load();

    private static string Load()
    {
        try
        {
            string p = Path.Combine(AppContext.BaseDirectory, "VERSION");
            if (File.Exists(p))
            {
                string v = File.ReadAllText(p).Trim();
                if (UpdateVersion.TryParse(v, out _))
                    return UpdateVersion.Normalize(v);
            }
        }
        catch { /* fallback below */ }
        return "1.0.0";
    }
}
