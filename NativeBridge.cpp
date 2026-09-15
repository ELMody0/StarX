#include <windows.h>

extern "C" __declspec(dllexport) int Add(int a, int b) {
    return a + b;
}

extern "C" __declspec(dllexport) void ShowNativeMessage(const wchar_t* msg) {
    MessageBoxW(NULL, msg, L"StarX Native Bridge", MB_OK | MB_ICONINFORMATION);
}

extern "C" __declspec(dllexport) int GetNativeVersion() {
    return 100; // v1.0.0
}