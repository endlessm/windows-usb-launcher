using EndlessLauncher.logger;
using EndlessLauncher.model;
using EndlessLauncher.utility;
using GalaSoft.MvvmLight.Ioc;
using Microsoft.Win32.SafeHandles;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Management;
using System.Runtime.InteropServices;
using System.Security.Principal;
using System.Text;
using System.Threading.Tasks;
using static EndlessLauncher.NativeAPI;
using static EndlessLauncher.NativeMethods;

namespace EndlessLauncher.service
{
    public class SystemVerificationService
    {
        private const int MINIMUM_RAM = 2 * 1024 * 1024; //KB
        private const int SUPPORTED_WINDOWS_VERSION = 10;
        private const int MINIMUM_CPU_CORES = 2;

        private int currentPhysicalDiskIndex = -1;
        private string currentDriveLetter = null;

        public event EventHandler<EndlessErrorEventArgs<SystemVerificationErrorCode>> VerificationFailed;
        public event EventHandler VerificationPassed;

        private enum SupportedOS
        {
            Windows10 = 10
        }

        private class USBDevice
        {
            public string DriverKeyName
            {
                get;
                set;
            }

            public string DevicePath
            {
                get;
                set;
            }
            public string Description
            {
                get;
                set;
            }

            public override string ToString()
            {
                StringBuilder sb = new StringBuilder();
                sb.Append("\nUSBDevice:---------------------------\n");
                sb.AppendFormat("Path: {0}\n", DevicePath);
                sb.AppendFormat("Description: {0}\n", Description);
                sb.AppendFormat("DriverKeyName: {0}\n", DriverKeyName);
                sb.Append("USBDevice:---------------------------");

                return sb.ToString();
            }
        }

        private class UsbHub
        {
            public string DevicePath
            {
                get;
                set;
            }
            public int PortCount
            {
                get;
                set;
            }

            public List<UsbPort> Ports { get; } = new List<UsbPort>();

            public override string ToString()
            {
                StringBuilder sb = new StringBuilder();
                sb.Append("\nUSBHub:---------------------------\n");
                sb.AppendFormat("Path: {0}\n", DevicePath);
                sb.AppendFormat("PortCount: {0}:\n", PortCount);
                foreach (UsbPort port in Ports)
                {
                    sb.Append(port.ToString());
                }
                sb.Append("USBHub:---------------------------");

                return sb.ToString();
            }
        }

        private class UsbPort
        {
            public string DriverKeyName
            {
                get;
                set;
            }

            public int PortNumber
            {
                get;
                set;
            }

            public bool IsUSB30
            {
                get;
                set;
            }

            public override string ToString()
            {
                StringBuilder sb = new StringBuilder();
                sb.Append("USBPort:---------------------------\n");
                sb.AppendFormat("DriverKeyName: {0}\n", DriverKeyName);
                sb.AppendFormat("IsUSB30: {0}\n", IsUSB30);

                return sb.ToString();
            }
        }

        private static readonly List<FirmwareType> supportedFirmwares = new List<FirmwareType>
        {
            FirmwareType.FirmwareTypeUefi
        };

        public async void VerifyRequirements()
        {
            LogHelper.Log("SystemVerificationService:VerifyRequirements:");

            if (Debug.SimulatedVerificationError != SystemVerificationErrorCode.NoError)
            {
                VerificationFailed?.Invoke(this, new EndlessErrorEventArgs<SystemVerificationErrorCode>
                {
                    ErrorCode = Debug.SimulatedVerificationError
                });

                return;
            }

            //Check 64-bit OS
            if (!Environment.Is64BitOperatingSystem)
            {
                VerificationFailed?.Invoke(this, new EndlessErrorEventArgs<SystemVerificationErrorCode>
                {
                    ErrorCode = SystemVerificationErrorCode.Not64BitSystem
                });
                return;
            }

            //Check admin rights
            if (!RunningAsAdministrator())
            {
                VerificationFailed?.Invoke(this, new EndlessErrorEventArgs<SystemVerificationErrorCode>
                {
                    ErrorCode = SystemVerificationErrorCode.NoAdminRights
                });
                return;
            }

            //Check Windows version
            if (!VerifyWindowsVersion())
            {
                VerificationFailed?.Invoke(this, new EndlessErrorEventArgs<SystemVerificationErrorCode>
                {
                    ErrorCode = SystemVerificationErrorCode.UnsupportedOS
                });
                return;
            }

            //Check RAM
            if (!VerifyRAM())
            {
                VerificationFailed?.Invoke(this, new EndlessErrorEventArgs<SystemVerificationErrorCode>
                {
                    ErrorCode = SystemVerificationErrorCode.InsufficientRAM
                });
                return;
            }

            //Check if UEFI firmware
            if (!InitializeFrameworkService())
            {
                VerificationFailed?.Invoke(this, new EndlessErrorEventArgs<SystemVerificationErrorCode>
                {
                    ErrorCode = SystemVerificationErrorCode.UnsupportedFirmware
                });
                return;
            }

            try
            {
                if (!await Task.Run(() => CheckCPUCoresCount()))
                {
                    VerificationFailed?.Invoke(this, new EndlessErrorEventArgs<SystemVerificationErrorCode>
                    {
                        ErrorCode = SystemVerificationErrorCode.SingleCoreProcessor
                    });
                    return;
                }

                await Task.Run(() => VerifyUSB30());
                LogHelper.Log("SystemVerificationService:VerifyRequirements: Success");
                VerificationPassed?.Invoke(this, null);

            }
            catch (SystemVerificationException ex)
            {
                VerificationFailed?.Invoke(this, new EndlessErrorEventArgs<SystemVerificationErrorCode>
                {
                    ErrorCode = ex.Code
                });
            }
            catch (Exception ex)
            {
                LogHelper.Log("SystemVerificationService:VerifyRequirements: Message: {0}", ex.Message);
                LogHelper.Log("SystemVerificationService:VerifyRequirements: StackTrace: {0}", ex.StackTrace);

                VerificationFailed?.Invoke(this, new EndlessErrorEventArgs<SystemVerificationErrorCode>
                {
                    ErrorCode = SystemVerificationErrorCode.GenericVerificationError
                });
            }
        }

        private bool InitializeFrameworkService()
        {
            FirmwareType firmwareType = FirmwareType.FirmwareTypeUnknown;

            try
            {
                GetFirmwareType(ref firmwareType);
            }
            catch (Exception ex)
            {
                LogHelper.Log("SystemVerificationService:InitializeFrameworkService: Failed: StackTrace: {0}", ex.Message);
            }

            switch (firmwareType)
            {
                case FirmwareType.FirmwareTypeUefi:
                    SimpleIoc.Default.Register<FirmwareServiceBase, EFIFirmwareService>();
                    break;

                case FirmwareType.FirmwareTypeUnknown:
                case FirmwareType.FirmwareTypeMax:
                case FirmwareType.FirmwareTypeBios:
                    SimpleIoc.Default.Register<FirmwareServiceBase, LegacyFirmwareService>();
                    break;
            }

            return supportedFirmwares.Contains(firmwareType);
        }

        private bool RunningAsAdministrator()
        {
            WindowsIdentity windowsIdentity = WindowsIdentity.GetCurrent();
            WindowsPrincipal windowsPrincipal = new WindowsPrincipal(windowsIdentity);

            return windowsPrincipal.IsInRole(WindowsBuiltInRole.Administrator);
        }

        private bool VerifyWindowsVersion()
        {
            LogHelper.Log("SystemVerificationService:VerifyWindowsVersion:");

            var values = Enum.GetValues(typeof(SupportedOS));

            foreach (SupportedOS value in values)
            {
                if ((int)value == Environment.OSVersion.Version.Major)
                {
                    return true;
                }
            }

            return false;
        }

        private bool VerifyRAM()
        {
            GetPhysicallyInstalledSystemMemory(out long totalInstalledRam);

            LogHelper.Log("SystemVerificationService:VerifyRAM: Installed: {0} GB", totalInstalledRam / 1024 / 1024);

            return MINIMUM_RAM <= totalInstalledRam;
        }

        //Assuming physical cores
        private bool CheckCPUCoresCount()
        {
            int coreCount = 0;
            foreach (var item in new System.Management.ManagementObjectSearcher("Select NumberOfCores from Win32_Processor").Get())
            {
                coreCount += int.Parse(item["NumberOfCores"].ToString());
            }

            LogHelper.Log("CheckCPUCoresCount:NumberOfCores: {0}", coreCount);

            return coreCount >= MINIMUM_CPU_CORES;
        }

        public string CurrentDriveLetter
        {
            get
            {
                if (currentDriveLetter == null)
                {
                    currentDriveLetter = Path.GetPathRoot(System.Reflection.Assembly.GetEntryAssembly().Location).Replace("\\", "");
                }
                return currentDriveLetter;
            }
        }

        public int CurrentPhysicalDiskIndex
        {
            get
            {
                if (currentPhysicalDiskIndex == -1)
                {
                    LogHelper.Log("CurrentPhysicalDiskIndex: Current drive: {0}", CurrentDriveLetter);

                    currentPhysicalDiskIndex = GetDiskForMountedDrive(CurrentDriveLetter);
                }

                return currentPhysicalDiskIndex;
            }
        }

        private int GetDiskForMountedDrive(string driveLetter)
        {
            int disk = -1;

            string query = "ASSOCIATORS OF {Win32_LogicalDisk.DeviceID='" +
                driveLetter + "'} " +
                "WHERE AssocClass = Win32_LogicalDiskToPartition";

            ManagementObjectSearcher queryResults = new ManagementObjectSearcher(query);
            ManagementObjectCollection partitions = queryResults.Get();

            foreach (var partition in partitions)
            {
                query = "ASSOCIATORS OF {Win32_DiskPartition.DeviceID='" +
                    partition["DeviceID"] + "'}" +
                    " WHERE AssocClass = Win32_DiskDriveToDiskPartition";

                queryResults = new ManagementObjectSearcher(query);

                ManagementObjectCollection drives = queryResults.Get();

                foreach (var drive in drives)
                    disk = int.Parse(drive["DeviceID"].ToString().Replace("\\\\.\\PHYSICALDRIVE", ""));
            }

            return disk;
        }

        private void VerifyUSB30()
        {
            LogHelper.Log("VerifyUSB30:");
            string query = string.Format("select PNPDeviceID from Win32_DiskDrive where InterfaceType='USB' and index='{0}'", CurrentPhysicalDiskIndex);

            ManagementObjectSearcher searcher = new ManagementObjectSearcher(query);
            string pnpDeviceID = string.Empty;
            foreach (ManagementObject mo in searcher.Get())
            {
                pnpDeviceID = mo["PNPDeviceID"].ToString();
            }

            if (string.IsNullOrEmpty(pnpDeviceID))
            {
                throw new SystemVerificationException(SystemVerificationErrorCode.UsbDeviceNotFound, "Could not find usb pnp device id");
            }

            pnpDeviceID = pnpDeviceID.Substring(pnpDeviceID.LastIndexOf("\\") + 1, pnpDeviceID.Length - pnpDeviceID.LastIndexOf("\\") - 3);
            LogHelper.Log("VerifyUSB30: PNPDeviceID: {0}", pnpDeviceID);

            USBDevice usbDevice = GetCurrentUSBDevice(pnpDeviceID);

            if (usbDevice == null)
            {
                LogHelper.Log("VerifyUSB30: Invalid usb device:");
                throw new SystemVerificationException(SystemVerificationErrorCode.UsbDeviceNotFound, "Could not find usb device");
            }

            LogHelper.Log("VerifyUSB30: Found the USB device: {0}", usbDevice);

            List<UsbHub> hubs = GetAvailableUSBPorts();

            //foreach (UsbHub hub in hubs)
            //{
            //    LogHelper.Log("VerifyUSB30: " + hub);
            //}

            UsbPort connectedPort = null;

            bool availableUsb30Ports = false;

            foreach (UsbHub hub in hubs)
            {
                foreach (UsbPort port in hub.Ports)
                {
                    availableUsb30Ports |= port.IsUSB30;
                    if (!string.IsNullOrEmpty(port.DriverKeyName) && port.DriverKeyName.Equals(usbDevice.DriverKeyName))
                    {
                        connectedPort = port;
                    }
                }
            }

            if (connectedPort == null)
            {
                throw new SystemVerificationException(SystemVerificationErrorCode.UsbPortNotFound, "Could not find the usb port");
            }

            if (connectedPort.IsUSB30)
            {
                //The USB device is connected to a USB3.0 Port
                //Returning means the method is terminated succesfully
                return;
            }
            else
            {
                if (availableUsb30Ports)
                {
                    throw new SystemVerificationException(SystemVerificationErrorCode.NotUSB30Port, "The USB is not inserted into a USB3.0 port");
                }
                else
                {
                    throw new SystemVerificationException(SystemVerificationErrorCode.NoUSBPortsFound, "The machine doesn't have USB3.0 Ports");
                }
            }



        }

        private USBDevice GetCurrentUSBDevice(string pnpDeviceId)
        {
            LogHelper.Log("GetCurrentUSBDevice: PNPDeviceID: {0}", pnpDeviceId);

            USBDevice usbDevice = null;

            SP_DEVINFO_DATA devInfodata = new SP_DEVINFO_DATA();
            devInfodata.cbSize = (UInt32)Marshal.SizeOf(devInfodata);

            Guid usbHubGuid = new Guid(DEVINTERFACE_USB_DEVICE);
            IntPtr deviceEnumHandle = SetupDiGetClassDevs(ref usbHubGuid
                                                        , IntPtr.Zero
                                                        , IntPtr.Zero
                                                        , (int)(DiGetClassFlags.DIGCF_DEVICEINTERFACE | DiGetClassFlags.DIGCF_PRESENT));


            // We only support 64-bits system. IntPtr size will always be 8, it's safe to convert it to Int64 
            if (deviceEnumHandle.ToInt64() != INVALID_HANDLE_VALUE)
            {
                UInt32 memberIndex = 0;
                SP_DEVICE_INTERFACE_DATA devInterfaceData = new SP_DEVICE_INTERFACE_DATA();
                devInterfaceData.cbSize = Marshal.SizeOf(devInterfaceData);

                //enumerate all connected usb devices
                bool hubEnumSucces = SetupDiEnumDeviceInterfaces(deviceEnumHandle, IntPtr.Zero, ref usbHubGuid, memberIndex, ref devInterfaceData);
                LogHelper.Log("GetCurrentUSBDevice: Found at least one usb device: {0}", hubEnumSucces);

                while (hubEnumSucces)
                {
                    string devicePath = GetDevicePath(deviceEnumHandle, ref devInterfaceData, ref devInfodata);
                    if (devicePath.ToLower().Contains(pnpDeviceId.ToLower()))
                    {
                        LogHelper.Log("GetCurrentUSBDevice: devicePath: {0}", devicePath);
                        usbDevice = new USBDevice()
                        {
                            DevicePath = devicePath,
                            DriverKeyName = GetDeviceProperty(deviceEnumHandle, SetupDiGetDeviceRegistryPropertyEnum.SPDRP_DRIVER, devInfodata),
                            Description = GetDeviceProperty(deviceEnumHandle, SetupDiGetDeviceRegistryPropertyEnum.SPDRP_DEVICEDESC, devInfodata),
                        };
                    }

                    hubEnumSucces = SetupDiEnumDeviceInterfaces(deviceEnumHandle, IntPtr.Zero, ref usbHubGuid, ++memberIndex, ref devInterfaceData);
                }

                SetupDiDestroyDeviceInfoList(deviceEnumHandle);
            }
            else
            {
                LogHelper.Log("GetCurrentUSBDevice:Invalid Handle:");
            }
            return usbDevice;
        }

        private string GetDeviceProperty(IntPtr deviceEnumHandle, SetupDiGetDeviceRegistryPropertyEnum property, SP_DEVINFO_DATA devInfodata)
        {
            string devicePropertyValue = null;
            byte[] ptrBuf = new byte[256];

            if (!SetupDiGetDeviceRegistryProperty(deviceEnumHandle, ref devInfodata, (UInt32)property, out UInt32 RegType, ptrBuf, 256, out UInt32 RequiredSize))
            {
                LogHelper.Log("GetDeviceProperty: Failed: Error: {0}", Marshal.GetLastWin32Error());

            }
            else
            {
                devicePropertyValue = new string(Encoding.Unicode.GetString(ptrBuf, 0, (int)RequiredSize).TakeWhile(x => x != 0).ToArray());
            }

            return devicePropertyValue;
        }

        private string GetDevicePath(IntPtr deviceEnumHandle, ref SP_DEVICE_INTERFACE_DATA devInterfaceData, ref SP_DEVINFO_DATA devInfodata)
        {
            string devicePathName = null;
            UInt32 dtSize = 0;

            SetupDiGetDeviceInterfaceDetailW(deviceEnumHandle, ref devInterfaceData, IntPtr.Zero, 0, ref dtSize, ref devInfodata);
            int error = Marshal.GetLastWin32Error();
            if (error == ERROR_INSUFFICIENT_BUFFER)
            {
                IntPtr devInfoDetailData = Marshal.AllocHGlobal((int)dtSize);

                // We only support 64-bits system. IntPtr size will always be 8, it's safe to convert it to Int64 
                Marshal.WriteInt32(devInfoDetailData, IntPtr.Size);

                //Get the DevicePath


                if (!SetupDiGetDeviceInterfaceDetailW(deviceEnumHandle, ref devInterfaceData, devInfoDetailData, (UInt32)(dtSize), ref dtSize, ref devInfodata))
                {
                    LogHelper.Log("GetDevicePath: Failed to get devicePath: {0}", Marshal.GetLastWin32Error());
                }
                else
                {
                    devicePathName = Marshal.PtrToStringUni(devInfoDetailData + 4);
                }

                Marshal.FreeHGlobal(devInfoDetailData);
            }
            else
            {
                LogHelper.Log("GetDevicePath: Failed to get size: {0}", error);
            }

            return devicePathName;
        }

        private List<UsbHub> GetAvailableUSBPorts()
        {
            LogHelper.Log("GetAvailableUSBPorts:");
            List<UsbHub> hubList = new List<UsbHub>();

            SP_DEVINFO_DATA devInfodata = new SP_DEVINFO_DATA();
            devInfodata.cbSize = (UInt32)Marshal.SizeOf(devInfodata);

            Guid usbHubGuid = new Guid(DEVINTERFACE_USB_HUB);
            IntPtr hubHandle = SetupDiGetClassDevs(ref usbHubGuid
                                                        , IntPtr.Zero
                                                        , IntPtr.Zero
                                                        , (int)(DiGetClassFlags.DIGCF_DEVICEINTERFACE | DiGetClassFlags.DIGCF_PRESENT));

            // We only support 64-bits system. IntPtr size will always be 8, it's safe to convert it to Int64 
            if (hubHandle.ToInt64() != INVALID_HANDLE_VALUE)
            {
                UInt32 memberIndex = 0;
                SP_DEVICE_INTERFACE_DATA devInterfaceData = new SP_DEVICE_INTERFACE_DATA();
                devInterfaceData.cbSize = Marshal.SizeOf(devInterfaceData);

                bool hubEnumSucces = SetupDiEnumDeviceInterfaces(hubHandle, IntPtr.Zero, ref usbHubGuid, memberIndex, ref devInterfaceData);

                while (hubEnumSucces)
                {
                    string devicePath = GetDevicePath(hubHandle, ref devInterfaceData, ref devInfodata);

                    if (!string.IsNullOrEmpty(devicePath))
                    {
                        UsbHub hub = new UsbHub
                        {
                            DevicePath = devicePath
                        };

                        hubList.Add(hub);
                        CheckHubPorts(hub);
                    }
                    else
                    {
                        LogHelper.Log("GetAvailableUSBPorts: Could not get Hub Device path");
                    }

                    hubEnumSucces = SetupDiEnumDeviceInterfaces(hubHandle, IntPtr.Zero, ref usbHubGuid, ++memberIndex, ref devInterfaceData);
                }

                SetupDiDestroyDeviceInfoList(hubHandle);
            }
            else
            {
                LogHelper.Log("GetAvailableUSBPorts: Invalid handle: Error: {0}", Marshal.GetLastWin32Error());
            }

            return hubList;
        }

        private void CheckHubPorts(UsbHub hub)
        {
            LogHelper.Log("CheckHubPorts:");

            SafeFileHandle hndl = CreateFile(hub.DevicePath,
                                                           GENERIC_READ | GENERIC_WRITE,
                                                           FILE_SHARE_READ | FILE_SHARE_WRITE,
                                                           IntPtr.Zero,
                                                           OPEN_EXISTING,
                                                           FILE_ATTRIBUTE_READONLY,
                                                           IntPtr.Zero);
            if (!hndl.IsInvalid)
            {
                USB_NODE_INFORMATION usbNodeInfo = new USB_NODE_INFORMATION();
                usbNodeInfo.NodeType = (int)USB_HUB_NODE.UsbHub;
                UInt32 usbNodeInfoSize = (UInt32)Marshal.SizeOf(usbNodeInfo);
                IntPtr ptrNodeInfo = Marshal.AllocHGlobal((int)usbNodeInfoSize);
                Marshal.StructureToPtr(usbNodeInfo, ptrNodeInfo, true);

                if (!DeviceIoControl(hndl, IOCTL_USB_GET_NODE_INFORMATION, ptrNodeInfo, usbNodeInfoSize, ptrNodeInfo, usbNodeInfoSize, out UInt32 returnedSize, IntPtr.Zero))
                {
                    LogHelper.Log("CheckHubPorts: Failed to get usb hub port count: {0}", Marshal.GetLastWin32Error());
                }
                else
                {
                    usbNodeInfo = (USB_NODE_INFORMATION)Marshal.PtrToStructure(ptrNodeInfo, typeof(USB_NODE_INFORMATION));
                    LogHelper.Log("CheckHubPorts:Total number of ports: {0}", usbNodeInfo.u.HubInformation.HubDescriptor.bNumberOfPorts);
                    hub.PortCount = usbNodeInfo.u.HubInformation.HubDescriptor.bNumberOfPorts;

                    for (int index = 1; index <= hub.PortCount; index++)
                    {
                        UsbPort port = new UsbPort
                        {
                            DriverKeyName = GetDriverKeyNameForPort(hndl, index),
                            IsUSB30 = IsUSB30Port(hndl, index),
                            PortNumber = index
                        };

                        hub.Ports.Add(port);
                    }
                }
                Marshal.FreeHGlobal(ptrNodeInfo);

            }
            else
            {
                LogHelper.Log("CheckHubPorts: CreateFile: Invalid handle");
            }

            hndl.Close();
        }

        private bool IsUSB30Port(SafeFileHandle hndl, int index)
        {
            bool isUSB30 = false;
            USB_NODE_CONNECTION_INFORMATION_EX_V2 nodeInfoExV2 = new USB_NODE_CONNECTION_INFORMATION_EX_V2();
            UInt32 size = (UInt32)Marshal.SizeOf(nodeInfoExV2);
            nodeInfoExV2.ConnectionIndex = (UInt32)index;
            nodeInfoExV2.Length = (UInt32)size;
            nodeInfoExV2.SupportedUsbProtocols = USB_PROTOCOLS.Usb300;

            IntPtr ptrNodeInfoExV2 = Marshal.AllocHGlobal((int)size);
            Marshal.StructureToPtr(nodeInfoExV2, ptrNodeInfoExV2, true);

            if (!DeviceIoControl(hndl, IOCTL_USB_GET_NODE_CONNECTION_INFORMATION_EX_V2, ptrNodeInfoExV2, (UInt32)size, ptrNodeInfoExV2, size, out size, IntPtr.Zero))
            {
                LogHelper.Log("CheckHubPorts: IsUSB30Port: Failed: Error: {0}", Marshal.GetLastWin32Error());
            }
            else
            {
                nodeInfoExV2 = (USB_NODE_CONNECTION_INFORMATION_EX_V2)Marshal.PtrToStructure(ptrNodeInfoExV2, typeof(USB_NODE_CONNECTION_INFORMATION_EX_V2));
                isUSB30 = nodeInfoExV2.SupportedUsbProtocols.HasFlag(USB_PROTOCOLS.Usb300);
            }

            Marshal.FreeHGlobal(ptrNodeInfoExV2);

            return isUSB30;
        }

        private string GetDriverKeyNameForPort(SafeFileHandle hndl, int index)
        {
            string deviceDriverKeyName = null;
            USB_NODE_CONNECTION_DRIVERKEY_NAME driverKeyName = new USB_NODE_CONNECTION_DRIVERKEY_NAME();
            driverKeyName.ConnectionIndex = (UInt32)index;
            UInt32 driverKeyNameSize = (UInt32)Marshal.SizeOf(driverKeyName);

            IntPtr ptrDriverKey = Marshal.AllocHGlobal((int)driverKeyNameSize);
            Marshal.StructureToPtr(driverKeyName, ptrDriverKey, true);
            UInt32 nBytesReturned;

            // Use an IOCTL call to request the Driver Key Name
            // In case it fails, it means the port has no connected device
            if (DeviceIoControl(hndl, IOCTL_USB_GET_NODE_CONNECTION_DRIVERKEY_NAME, ptrDriverKey, driverKeyNameSize, ptrDriverKey, driverKeyNameSize, out nBytesReturned, IntPtr.Zero))
            {
                driverKeyName = (USB_NODE_CONNECTION_DRIVERKEY_NAME)Marshal.PtrToStructure(ptrDriverKey, typeof(USB_NODE_CONNECTION_DRIVERKEY_NAME));
                deviceDriverKeyName = driverKeyName.DriverKeyName;
            }
            Marshal.FreeHGlobal(ptrDriverKey);
            return deviceDriverKeyName;
        }

    }
}
