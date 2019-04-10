# windows-usb-launcher
USB Launcher app for Windows

## Build instructions
#### Tools
- [Visual Studio build tools 2017/2019 (msbuild)](https://visualstudio.microsoft.com/downloads/)
- [nuget](https://www.nuget.org/downloads)
- [.net development tools 4.6](https://dotnet.microsoft.com/download/visual-studio-sdks)
#### Commands
1. Restore nuget dependencies
```
nuget.exe restore
```
2. Build the solution
```
MSBuild.exe EndlessLauncher.sln /p:Configuration="<Debug/Release>";outdir="<release_folder>"
```
## Other info
It is possbile to "force" a system verification or a firmware setup error by running the application with "-e <errorCode>", 
"--errorCode <errorCode>" command arguments
```
EndlessLauncher.exe -e GenericVerificationError
EndlessLauncher.exe --errorCode 100
```
Firmware setup error codes:
```
NoError = int.MinValue,
GenericFirmwareError = -1,
OpenProcessTokenError = 1,
LookupPrivilegeError,
AdjustTokenPrivilegeError,
EspPartitionNotFoundError,
GetBootBorderError,
NoExistingUefiEntriesError,
GetPartitionEspInfoError,
FindFreeUefiEntryError,
CreateNewUefiEntryError,
AddToBootOrderError,
SetBootNextError,
FirmwareServiceInitializationError
```
System requirements verification error codes
```
NoError = int.MinValue,
GenericVerificationError = 100,
Not64BitSystem,
UnsupportedFirmware,
NoAdminRights,
UnsupportedOS,
InsufficientRAM,
SingleCoreProcessor,
UsbDeviceNotFound,
UsbPortNotFound,
NotUSB30Port,
NoUSBPortsFound
```
  
 This will redirect the application to the incompatible USB(error) screen.
