using System.Security.Cryptography;
using System.Text;

namespace StarXShared;

/// <summary>
/// توقيع/تحقق ECDSA P-256 على بصمة SHA-256 لحزم التحديث.
/// المفتاح العام مضمّن في الكود؛ المفتاح الخاص في متغير البيئة STARX_SIGNING_KEY
/// أو ملف signing/starx_update_private.key (غير مُلتزم في git).
/// </summary>
internal static class UpdateSecurity
{
    /// <summary>المفتاح العام (SPKI base64) — يُتحقق به في العميل والمحدّث.</summary>
    public const string PublicKeyBase64 =
        "MFkwEwYHKoZIzj0CAQYIKoZIzj0DAQcDQgAExT4yuHsvOTBoReqVW5xrCJJiZ4OrrLjE5bVZYzZVJod1HzRZPbDVwuQ0hNUGsLUY9ZK/+dg39oF5pB2z1ZaJCg==";

    /// <summary>توقيع بصمة الـ hex (lowercase) — يرجع base64 أو null.</summary>
    public static string? SignSha256Hex(string sha256Hex, string privateKeyBase64)
    {
        if (string.IsNullOrWhiteSpace(sha256Hex) || string.IsNullOrWhiteSpace(privateKeyBase64))
            return null;
        try
        {
            using var ec = ECDsa.Create();
            ec.ImportECPrivateKey(Convert.FromBase64String(privateKeyBase64.Trim()), out _);
            byte[] sig = ec.SignData(
                Encoding.UTF8.GetBytes(sha256Hex.Trim().ToLowerInvariant()),
                HashAlgorithmName.SHA256);
            return Convert.ToBase64String(sig);
        }
        catch
        {
            return null;
        }
    }

    /// <summary>التحقق من توقيع بصمة hex بمفتاح عام base64.</summary>
    public static bool VerifySha256Hex(string sha256Hex, string signatureBase64,
        string? publicKeyBase64 = null)
    {
        if (string.IsNullOrWhiteSpace(sha256Hex) ||
            string.IsNullOrWhiteSpace(signatureBase64))
            return false;
        try
        {
            using var ec = ECDsa.Create();
            ec.ImportSubjectPublicKeyInfo(
                Convert.FromBase64String((publicKeyBase64 ?? PublicKeyBase64).Trim()), out _);
            return ec.VerifyData(
                Encoding.UTF8.GetBytes(sha256Hex.Trim().ToLowerInvariant()),
                Convert.FromBase64String(signatureBase64.Trim()),
                HashAlgorithmName.SHA256);
        }
        catch
        {
            return false;
        }
    }

    /// <summary>المفتاح الخاص من البيئة أو ملف المشروع (للنشر المحلي).</summary>
    public static string? LoadPrivateKey()
    {
        string? env = Environment.GetEnvironmentVariable("STARX_SIGNING_KEY");
        if (!string.IsNullOrWhiteSpace(env)) return env.Trim();
        try
        {
            string p = Path.Combine(AppContext.BaseDirectory, "signing", "starx_update_private.key");
            if (File.Exists(p))
            {
                string t = File.ReadAllText(p).Trim();
                if (t.Length > 0) return t;
            }
        }
        catch { /* ignore */ }
        return null;
    }
}
