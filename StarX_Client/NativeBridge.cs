using System.Runtime.InteropServices;

namespace StarX_Client;

/// <summary>
/// P/Invoke bridge for NativeBridge.dll (x64) built from ..\NativeBridge.cpp.
/// Exports: Add, ShowNativeMessage, GetNativeVersion.
/// </summary>
internal static partial class NativeBridge
{
    [LibraryImport("NativeBridge.dll")]
    internal static partial int Add(int a, int b);

    [LibraryImport("NativeBridge.dll", StringMarshalling = StringMarshalling.Utf16)]
    internal static partial void ShowNativeMessage(string msg);

    [LibraryImport("NativeBridge.dll")]
    internal static partial int GetNativeVersion();

    internal static string FormatVersion(int raw)
    {
        // Convention: 100 => v1.0.0
        return $"v{raw / 100}.{raw / 10 % 10}.{raw % 10} ({raw})";
    }
}
