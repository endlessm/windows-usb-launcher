using System;

namespace EndlessLauncher.model
{
    public enum FirmwareSetupErrorCode : int
    {
        GenericError = -1,
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
    };

    public enum SystemVerificationErrorCode : int
    {
        GenericError = 100,
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
    };

    public class EndlessErrorEventArgs<T> : EventArgs
    {
        public T ErrorCode
        {
            get;
            set;
        }
    }
}
