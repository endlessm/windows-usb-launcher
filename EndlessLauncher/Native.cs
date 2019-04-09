using Microsoft.Win32.SafeHandles;
using System;
using System.Runtime.InteropServices;
using static EndlessLauncher.NativeAPI;

namespace EndlessLauncher
{
    public class NativeAPI
    {
        #region Windows File
        public const UInt32 GENERIC_EXECUTE = 0x20000000U;
        public const UInt32 GENERIC_WRITE = 0x40000000U;
        public const UInt32 GENERIC_READ = 0x80000000U;
        public const UInt32 FILE_SHARE_READ = 0x00000001U;
        public const UInt32 FILE_SHARE_WRITE = 0x00000002U;
        public const UInt32 OPEN_EXISTING = 0x00000003U;
        public const UInt32 FILE_ATTRIBUTE_READONLY = 0x00000001U;

        public const int ERROR_INSUFFICIENT_BUFFER = 122;
        public const int INVALID_HANDLE_VALUE = -1;
        #endregion

        #region IOCTL
        public const UInt32 IOCTL_VOLUME_GET_VOLUME_DISK_EXTENTS = 0x00560000U;
        public const UInt32 IOCTL_DISK_GET_PARTITION_INFO_EX = 0x00070048U;
        public const UInt32 IOCTL_STORAGE_QUERY_PROPERTY = 0x2D1400U;

        public const UInt32 IOCTL_USB_GET_NODE_CONNECTION_INFORMATION_EX_V2 = 0x22045cU;
        public const UInt32 IOCTL_USB_GET_NODE_INFORMATION = 0x220408U;
        public const UInt32 IOCTL_USB_GET_PORT_CONNECTOR_PROPERTIES = 0x00220458U;
        public const UInt32 IOCTL_USB_GET_NODE_CONNECTION_DRIVERKEY_NAME = 0x220420U;
        #endregion

        #region Privilege
        public const string SE_SYSTEM_ENVIRONMENT_NAME = "SeSystemEnvironmentPrivilege";
        public const string SE_SHUTDOWN_NAME = "SeShutdownPrivilege";
        public const Int32 SE_PRIVILEGE_ENABLED = 0x00000002;
        public const Int32 TOKEN_QUERY = 0x00000008;
        public const Int32 TOKEN_ADJUST_PRIVILEGES = 0x00000020;
        #endregion

        #region EFI Fields
        public const string UEFI_BOOT_NAMESPACE = "{8BE4DF61-93CA-11d2-AA0D-00E098032B8C}";
        public const string EFI_BOOT_ORDER = "BootOrder";
        public const string EFI_GLOBAL_VARIABLE = "{8BE4DF61-93CA-11D2-AA0D-00E098032B8C}";
        public const string LOAD_OPTION_FORMAT = "Boot{0:X4}";
        public const Int64 ERROR_ENVVAR_NOT_FOUND = 203L;
        public static readonly Guid ESP_GUID = new Guid("C12A7328-F81F-11D2-BA4B-00A0C93EC93B");
        #endregion

        public const string DEVINTERFACE_USB_HUB = "{F18A0E88-C30C-11D0-8815-00A0C906BED8}";
        public const string DEVINTERFACE_USB_DEVICE = "{A5DCBF10-6530-11D2-901F-00C04FB951ED}";

        public enum SetupDiGetDeviceRegistryPropertyEnum : uint
        {
            SPDRP_DEVICEDESC = 0x00000000, // DeviceDesc (R/W)
            SPDRP_DRIVER = 0x00000009, // Driver (R/W)
        }

        public struct USB_NODE_CONNECTION_INFORMATION_EX_V2
        {
            public UInt32 ConnectionIndex;
            public UInt32 Length;
            public USB_PROTOCOLS SupportedUsbProtocols;
            public USB_NODE_CONNECTION_INFORMATION_EX_V2_FLAGS Flags;
        }

        [Flags]
        public enum USB_PROTOCOLS : uint
        {
            Usb110 = 0x01,
            Usb200 = 0x02,
            Usb300 = 0x04,
        }

        [Flags]
        public enum USB_NODE_CONNECTION_INFORMATION_EX_V2_FLAGS : UInt32
        {
            DeviceIsOperatingAtSuperSpeedOrHigher = 0x01,
            DeviceIsSuperSpeedCapableOrHigher = 0x02,
        }

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto, Pack = 1)]
        public struct USB_NODE_CONNECTION_DRIVERKEY_NAME
        {
            public UInt32 ConnectionIndex;
            public UInt32 ActualLength;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 256)]
            public string DriverKeyName;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct USB_NODE_INFORMATION
        {
            public USB_HUB_NODE NodeType;
            public UsbNodeUnion u;
        }

        [StructLayout(LayoutKind.Explicit)]
        public struct UsbNodeUnion
        {
            [FieldOffset(0)]
            public USB_HUB_INFORMATION HubInformation;
            [FieldOffset(0)]
            public USB_MI_PARENT_INFORMATION MiParentInformation;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct USB_HUB_INFORMATION
        {
            public USB_HUB_DESCRIPTOR HubDescriptor;
            public byte HubIsBusPowered;
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        public struct USB_HUB_DESCRIPTOR
        {
            public byte bDescriptorLength;
            public byte bDescriptorType;
            public byte bNumberOfPorts;
            public UInt16 wHubCharacteristics;
            public byte bPowerOnToPowerGood;
            public byte bHubControlCurrent;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 64)]
            public byte[] bRemoveAndPowerMask;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct USB_MI_PARENT_INFORMATION
        {
            public UInt32 NumberOfInterfaces;
        };

        public enum USB_HUB_NODE : uint
        {
            UsbHub,
            UsbMIParent
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        public struct SP_DEVICE_INTERFACE_DATA
        {
            public Int32 cbSize;
            public Guid interfaceClassGuid;
            public Int32 flags;
            private UIntPtr reserved;
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        public struct SP_DEVINFO_DATA
        {
            public UInt32 cbSize;
            public Guid ClassGuid;
            public UInt32 DevInst;
            private UIntPtr Reserved;
        }

        [Flags]
        public enum DiGetClassFlags : UInt32
        {
            DIGCF_DEFAULT = 0x00000001U,
            DIGCF_PRESENT = 0x00000002U,
            DIGCF_ALLCLASSES = 0x00000004U,
            DIGCF_PROFILE = 0x00000008U,
            DIGCF_DEVICEINTERFACE = 0x00000010U,
        }

        [Flags]
        public enum ExitWindows : UInt32
        {
            EwxReboot = 0x02U,
        }

        [Flags]
        public enum ShutdownReason : UInt32
        {
            MajorOther = 0x00000000U,
            MinorOther = 0x00000000U,
            FlagPlanned = 0x80000000U
        }

        [StructLayout(LayoutKind.Sequential)]
        public class DISK_EXTENT
        {
            public UInt32 DiskNumber;
            public Int64 StartingOffset;
            public Int64 ExtentLength;
        }

        [StructLayout(LayoutKind.Sequential)]
        public class VOLUME_DISK_EXTENTS
        {
            public UInt32 NumberOfDiskExtents;
            public DISK_EXTENT Extents;
        }

        public enum FirmwareType : UInt32
        {
            FirmwareTypeUnknown = 0U,
            FirmwareTypeBios = 1U,
            FirmwareTypeUefi = 2U,
            FirmwareTypeMax = 3U
        };

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        public struct TokenPrivelege
        {
            public Int32 Count;
            public Int64 Luid;
            public Int32 Attr;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct PARTITION_INFORMATION_EX
        {
            [MarshalAs(UnmanagedType.U4)]
            public PARTITION_STYLE PartitionStyle;
            public Int64 StartingOffset;
            public Int64 PartitionLength;
            public Int32 PartitionNumber;
            public bool RewritePartition;
            public PARTITION_INFORMATION_UNION DriveLayoutInformaiton;
        }

        public enum PARTITION_STYLE : Int32
        {
            MasterBootRecord = 0,
            GuidPartitionTable = 1,
            Raw = 2
        }

        [StructLayout(LayoutKind.Explicit)]
        public struct PARTITION_INFORMATION_UNION
        {
            [FieldOffset(0)]
            public PARTITION_INFORMATION_GPT Gpt;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct PARTITION_INFORMATION_MBR
        {
            public byte PartitionType;
            [MarshalAs(UnmanagedType.U1)]
            public bool BootIndicator;
            [MarshalAs(UnmanagedType.U1)]
            public bool RecognizedPartition;
            public UInt32 HiddenSectors;
        }

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
        public struct PARTITION_INFORMATION_GPT
        {
            public Guid PartitionType;
            public Guid PartitionId;
            public UInt64 Attributes;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 36)]
            public string Name;
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        public struct EFI_HARD_DRIVE_PATH
        {
            public byte type;
            public byte subtype;
            public UInt16 length;
            public UInt32 part_num;
            public UInt64 start;
            public UInt64 size;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 16)]
            public byte[] signature;
            public byte mbr_type;
            public byte signature_type;
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        public struct EFI_END_DEVICE_PATH
        {
            public byte type;
            public byte subtype;
            public UInt16 length;
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        public struct EFI_FILE_PATH
        {
            public byte type;
            public byte subtype;
            public UInt16 length;
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        public struct EFI_LOAD_OPTION
        {
            public UInt32 attributes;
            public UInt16 file_path_list_length;
        }
    }

    internal class NativeMethods
    {
        [DllImport("setupapi.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        internal static extern bool SetupDiGetDeviceRegistryProperty(
            IntPtr deviceInfoSet,
            ref SP_DEVINFO_DATA deviceInfoData,
            UInt32 property,
            out UInt32 propertyRegDataType,
            byte[] propertyBuffer,
            UInt32 propertyBufferSize,
            out UInt32 requiredSize
        );

        [DllImport("kernel32", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static extern bool DeviceIoControl(SafeFileHandle hDevice,
            UInt32 ioControlCode,
            IntPtr inBuffer,
            UInt32 inBufferSize,
            IntPtr outBuffer,
            UInt32 outBufferSize,
            out UInt32 bytesReturned,
            IntPtr overlapped);

        [DllImport("kernel32.dll", CharSet = CharSet.Unicode, CallingConvention = CallingConvention.StdCall, SetLastError = true)]
        internal static extern SafeFileHandle CreateFile(string lpFileName,
            UInt32 dwDesiredAccess,
            UInt32 dwShareMode,
            IntPtr SecurityAttributes,
            UInt32 dwCreationDisposition,
            UInt32 dwFlagsAndAttributes,
            IntPtr hTemplateFile);

        [DllImport("advapi32.dll", ExactSpelling = true, SetLastError = true)]
        internal static extern bool OpenProcessToken(IntPtr h, Int32 acc, ref IntPtr phtok);

        [DllImport("advapi32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
        internal static extern bool LookupPrivilegeValue(string host, string name, ref long pluid);

        [DllImport("advapi32.dll", ExactSpelling = true, SetLastError = true)]
        internal static extern bool AdjustTokenPrivileges(
            IntPtr htok,
            bool disall,
            ref TokenPrivelege newst,
            Int32 len,
            IntPtr prev,
            IntPtr relen);

        [DllImport("kernel32.dll", ExactSpelling = true, SetLastError = true)]
        internal static extern IntPtr GetCurrentProcess();

        [DllImport("kernel32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static extern bool CloseHandle(IntPtr hObject);

        [DllImport("setupapi.dll", SetLastError = true, ExactSpelling = true)]
        internal static extern bool SetupDiGetDeviceInterfaceDetailW(
            IntPtr hDevInfo,
            ref SP_DEVICE_INTERFACE_DATA deviceInterfaceData,
            IntPtr deviceInterfaceDetailData,
            UInt32 deviceInterfaceDetailDataSize,
            ref UInt32 requiredSize,
            ref SP_DEVINFO_DATA deviceInfoData);


        [DllImport(@"setupapi.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        internal static extern bool SetupDiEnumDeviceInterfaces(
            IntPtr hDevInfo,
            IntPtr devInfo,
            ref Guid interfaceClassGuid,
            UInt32 memberIndex,
            ref SP_DEVICE_INTERFACE_DATA deviceInterfaceData
        );

        [DllImport("setupapi.dll", CharSet = CharSet.Auto)]
        internal static extern IntPtr SetupDiGetClassDevs(
            ref Guid ClassGuid,
            IntPtr Enumerator,
            IntPtr hwndParent,
            int Flags
        );
        [DllImport("setupapi.dll", SetLastError = true)]
        internal static extern bool SetupDiDestroyDeviceInfoList(IntPtr DeviceInfoSet);


        [DllImport("user32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static extern bool ExitWindowsEx(ExitWindows uFlags, ShutdownReason dwReason);


        [DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Auto, CallingConvention = CallingConvention.StdCall)]
        internal static extern bool SetFirmwareEnvironmentVariable(
            [MarshalAs(UnmanagedType.LPWStr)]
            string lpName,
            [MarshalAs(UnmanagedType.LPWStr)]
            string lpGuid,
            byte[] pValue,
            UInt32 nSize
            );

        [DllImport("kernel32.dll", SetLastError = true)]
        internal static extern bool GetFirmwareType(ref FirmwareType FirmwareType);

        [DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Auto, CallingConvention = CallingConvention.StdCall)]
        internal static extern UInt32 GetFirmwareEnvironmentVariable(
            [MarshalAs(UnmanagedType.LPWStr)]
            string lpName,
            [MarshalAs(UnmanagedType.LPWStr)]
            string lpGuid,
            byte[] pBuffer,
            UInt32 nSize);

        [DllImport("kernel32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static extern bool GetPhysicallyInstalledSystemMemory(out long kiloBytes);

        [DllImport("user32", CharSet = CharSet.Unicode)]
        internal static extern IntPtr FindWindow(string cls, string win);

        [DllImport("user32")]
        internal static extern bool SetForegroundWindow(IntPtr hWnd);

        [DllImport("user32")]
        internal static extern bool IsIconic(IntPtr hWnd);

        [DllImport("user32")]
        internal static extern bool OpenIcon(IntPtr hWnd);
    }
}
