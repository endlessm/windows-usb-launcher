using Microsoft.Win32.SafeHandles;
using System;
using System.Runtime.InteropServices;

namespace EndlessLauncher
{
    public class NativeAPI
    {
        #region Windows File
        public const UInt32 GENERIC_EXECUTE = 0x01U << 29;
        public const UInt32 GENERIC_WRITE = 0x01U << 30;
        public const UInt32 GENERIC_READ = 0x01U << 31;
        public const UInt32 FILE_SHARE_READ = 0x00000001U;
        public const UInt32 FILE_SHARE_WRITE = 0x00000002U;
        public const UInt32 OPEN_EXISTING = 3U;
        public const UInt32 FILE_ATTRIBUTE_READONLY = 0x00000001U;
        #endregion

        #region IOCTL
        public const UInt32 IOCTL_VOLUME_GET_VOLUME_DISK_EXTENTS = 0x00560000;
        public const UInt32 IOCTL_DISK_GET_PARTITION_INFO_EX = (0x00000007 << 16) | (0x0012 << 2) | 0 | (0 << 14);
        public const UInt32 IOCTL_STORAGE_QUERY_PROPERTY = 0x2D1400;
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
        #endregion


        [Flags]
        public enum ExitWindows : UInt32
        {
            EwxReboot = 0x02,
        }

        [Flags]
        public enum ShutdownReason : UInt32
        {
            MajorOther = 0x00000000,
            MinorOther = 0x00000000,
            FlagPlanned = 0x80000000
        }

        [StructLayout(LayoutKind.Sequential)]
        internal class DISK_EXTENT
        {
            public UInt32 DiskNumber;
            public Int64 StartingOffset;
            public Int64 ExtentLength;
        }

        [StructLayout(LayoutKind.Sequential)]
        internal class VOLUME_DISK_EXTENTS
        {
            public UInt32 NumberOfDiskExtents;
            public DISK_EXTENT Extents;
        }

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


        [DllImport("kernel32.dll", CharSet = CharSet.Auto, CallingConvention = CallingConvention.StdCall, SetLastError = true)]
        public static extern SafeFileHandle CreateFile(string lpFileName,
            UInt32 dwDesiredAccess,
            UInt32 dwShareMode,
            IntPtr SecurityAttributes,
            UInt32 dwCreationDisposition,
            UInt32 dwFlagsAndAttributes,
            IntPtr hTemplateFile);

        public enum FirmwareType : UInt32
        {
            FirmwareTypeUnknown = 0,
            FirmwareTypeBios = 1,
            FirmwareTypeUefi = 2,
            FirmwareTypeMax = 3
        };

        [DllImport("advapi32.dll", ExactSpelling = true, SetLastError = true)]
        internal static extern bool OpenProcessToken(IntPtr h, Int32 acc, ref IntPtr phtok);

        [DllImport("advapi32.dll", SetLastError = true)]
        internal static extern bool LookupPrivilegeValue(string host, string name, ref long pluid);

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        public struct TokenPrivelege
        {
            public Int32 Count;
            public Int64 Luid;
            public Int32 Attr;
        }

        [DllImport("advapi32.dll", ExactSpelling = true, SetLastError = true)]
        internal static extern bool AdjustTokenPrivileges(IntPtr htok, bool disall, ref TokenPrivelege newst, Int32 len, IntPtr prev, IntPtr relen);

        [DllImport("kernel32.dll", ExactSpelling = true, SetLastError = true)]
        internal static extern IntPtr GetCurrentProcess();

        [DllImport("kernel32.dll", SetLastError=true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool CloseHandle(IntPtr hObject);

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
            [FieldOffset(0)]
            public PARTITION_INFORMATION_MBR Mbr;
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


        [StructLayout(LayoutKind.Explicit, CharSet = CharSet.Unicode)]
        public struct PARTITION_INFORMATION_GPT
        {
            [FieldOffset(0)]
            public Guid PartitionType;
            [FieldOffset(16)]
            public Guid PartitionId;
            [FieldOffset(32)]
            public UInt64 Attributes;
            [FieldOffset(40)]
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

        [DllImport("user32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool ExitWindowsEx(ExitWindows uFlags, ShutdownReason dwReason);


        [DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Auto, CallingConvention = CallingConvention.StdCall)]
        public static extern bool SetFirmwareEnvironmentVariable([MarshalAs(UnmanagedType.LPWStr)] string lpName, [MarshalAs(UnmanagedType.LPWStr)] string lpGuid, byte[] pValue, UInt32 nSize);

        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern bool GetFirmwareType(ref FirmwareType FirmwareType);

        [DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Auto, CallingConvention = CallingConvention.StdCall)]
        public static extern UInt32 GetFirmwareEnvironmentVariable([MarshalAs(UnmanagedType.LPWStr)] string lpName, [MarshalAs(UnmanagedType.LPWStr)] string lpGuid, byte[] pBuffer, UInt32 nSize);
    }
}
