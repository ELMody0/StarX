using System.Text.Json;
using System.Text.Json.Nodes;

namespace StarX_Client;

/// <summary>إعدادات التطبيق المحلية: ملف JSON واحد يحافظ على كل المفاتيح.</summary>
internal static class AppSettings
{
    // للاختبارات فقط
#pragma warning disable CS0649
    internal static string? DirOverride;
#pragma warning restore CS0649

    private static string Dir =>
        DirOverride ?? Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "StarX");

    private static string FilePath => Path.Combine(Dir, "settings.json");

    private static readonly object _lock = new();

    public static string GetString(string key, string fallback = "")
    {
        lock (_lock)
        {
            try
            {
                if (!File.Exists(FilePath)) return fallback;
                using var doc = JsonDocument.Parse(File.ReadAllText(FilePath));
                if (doc.RootElement.TryGetProperty(key, out var e) &&
                    e.ValueKind == JsonValueKind.String)
                    return e.GetString() ?? fallback;
            }
            catch { /* ignore */ }
            return fallback;
        }
    }

    public static void Set(string key, string? value)
    {
        lock (_lock)
        {
            try
            {
                Directory.CreateDirectory(Dir);
                JsonObject obj;
                if (File.Exists(FilePath))
                {
                    try
                    {
                        obj = JsonNode.Parse(File.ReadAllText(FilePath))?.AsObject() ?? new JsonObject();
                    }
                    catch { obj = new JsonObject(); }
                }
                else obj = new JsonObject();
                if (value == null) obj.Remove(key);
                else obj[key] = value;
                File.WriteAllText(FilePath, obj.ToJsonString());
            }
            catch { /* ignore */ }
        }
    }
}
