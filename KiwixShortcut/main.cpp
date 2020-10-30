#include <Windows.h>

int APIENTRY wWinMain(_In_ HINSTANCE hInstance,
                      _In_opt_ HINSTANCE hPrevInstance,
                      _In_ LPWSTR    lpCmdLine,
                      _In_ int       nCmdShow)
{
    ShellExecuteW(NULL, NULL, TEXT("Endless Launcher.exe"), TEXT("--kiwix"), NULL, 0);
    return 0;
}

