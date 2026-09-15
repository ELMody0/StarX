namespace StarX_Client;

/// <summary>
/// إعدادات Supabase — املأ القيم هنا أو عبر متغيرات البيئة:
/// STARX_SUPABASE_URL / STARX_SUPABASE_ANON_KEY
/// </summary>
internal static class SupabaseConfig
{
    public static string Url =>
        Environment.GetEnvironmentVariable("STARX_SUPABASE_URL")
        ?? "https://jkzattwaeugkkbjfgfbi.supabase.co";

    public static string AnonKey =>
        Environment.GetEnvironmentVariable("STARX_SUPABASE_ANON_KEY")
        ?? "sb_publishable_tysmzoUc_gIuaiKIonKEFg_BmlwMBmX";

    public static bool IsConfigured
    {
        get
        {
            if (Url.Contains("YOUR-PROJECT", StringComparison.OrdinalIgnoreCase) ||
                AnonKey.StartsWith("YOUR-", StringComparison.OrdinalIgnoreCase))
                return false;
            if (!Uri.TryCreate(Url, UriKind.Absolute, out var u))
                return false;
            if (u.Scheme == Uri.UriSchemeHttps)
                return true;
            // http مسموح للتطوير المحلي (loopback) فقط
            return u.Scheme == Uri.UriSchemeHttp && u.IsLoopback;
        }
    }

    public static string RpcUrl(string function) =>
        Url.TrimEnd('/') + "/rest/v1/rpc/" + function;
}
