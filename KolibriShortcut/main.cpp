#include <Windows.h>
#include <stdio.h>


int APIENTRY wWinMain(_In_ HINSTANCE hInstance,
                      _In_opt_ HINSTANCE hPrevInstance,
                      _In_ LPWSTR    lpCmdLine,
                      _In_ int       nCmdShow)
{
    printf("Launching\n");
    AddDllDirectory(L"D:\\.kolibri-windows\\resources\\app\\src\\Kolibri");
    printf("After setting DLL\n");
    ShellExecuteW(NULL, NULL, TEXT("kolibri-electron.exe"), NULL, TEXT(".kolibri-windows"), 0);
    return 0;
}

