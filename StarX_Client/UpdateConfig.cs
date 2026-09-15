namespace StarX_Client;

/// <summary>إعدادات نظام التحديث المركزية.</summary>
internal static class UpdateConfig
{
    public const string RepoOwner = "ELMody0";
    public const string RepoName = "StarX";
    public const string AssetPrefix = "StarX-v";
    public const string AssetExtension = ".zip";

    public const int InitialDelaySeconds = 15;
    public const int CheckIntervalMinutes = 30;
    public const int SuppressionHours = 6;
    public const int RequestTimeoutSeconds = 20;

    public static string ApiLatestUrl =>
        $"https://api.github.com/repos/{RepoOwner}/{RepoName}/releases/latest";
}
