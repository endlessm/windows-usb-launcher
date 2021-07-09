#include <Windows.h>

int APIENTRY wWinMain(_In_ HINSTANCE hInstance,
                      _In_opt_ HINSTANCE hPrevInstance,
                      _In_ LPWSTR    lpCmdLine,
                      _In_ int       nCmdShow)
{
    ShellExecuteW(NULL, NULL, TEXT("kolibri-electron.exe"), NULL, TEXT(".kolibri-windows"), 0);
    return 0;
}

