using EndlessLauncher.logger;
using Microsoft.Win32.SafeHandles;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Management;
using System.Runtime.InteropServices;
using System.Text;
using static EndlessLauncher.utility.Utils;
using static EndlessLauncher.NativeAPI;
using EndlessLauncher.model;
using static EndlessLauncher.model.FirmwareSetupException;

namespace EndlessLauncher.service
{
    public class EFIFirmwareService : FirmwareServiceBase
    {
        private const int HARD_DRIVE_DISKPATH_SIZE = 42;
        private const int SIGNATURE_TYPE_OFFSET = 41;
        private const int SIGNATURE_OFFSET = 24;
        private const int PARTITION_NUMBER_OFFSET = 4;

        private List<UefiGPTEntry> entries;

        public EFIFirmwareService(SystemVerificationService service) : base(service) { }

        private class UefiGPTEntry
        {
            public UInt16 number;
            public Guid guid;
            public string path;
            public string description;
            public int partitionNumber;
        }

        protected override void SetupEndlessLaunch()
        {
            LogHelper.Log("EFIFirmwareService:SetupEndlessLaunch:");

            ObtainPrivileges(SE_SYSTEM_ENVIRONMENT_NAME);
            ObtainPrivileges(SE_SHUTDOWN_NAME);

            KeyValuePair<string, long> volumeInfo = GetESPVolume();

            if (volumeInfo.Key == null)
            {
                throw new FirmwareSetupException(FirmwareSetupErrorCode.EspVolumeNotFoundError, "Could not Found USB ESP volume");
            }

            string volume = "\\\\.\\" + volumeInfo.Key;

            LogHelper.Log("EFIFirmwareService:SetupEndlessLaunch: volume: {0}", volume);

            //We assume there is at least the Windows UEFI entry
            entries = GetGPTUefiEntries();
            if (entries == null || entries.Count == 0)
            {
                throw new FirmwareSetupException(FirmwareSetupErrorCode.NoExistingUefiEntriesError, "Found 0 UEFI entries");
            }

            PrintUefiEntries();

            //TODO Use configuration for Entry name and entry path
            UefiGPTEntry entry = CreateUEfiEntry(volume, "Hack OS", "\\EFI\\BOOT\\BOOTX64.EFI", volumeInfo.Value);

            if (entry == null)
            {
                throw new FirmwareSetupException(FirmwareSetupErrorCode.CreateNewUefiEntryError, "Create new Uefi entry failed");
            }

            if (!entries.Contains(entry))
            {
                entries.Add(entry);
            }

            PrintUefiEntries();

            bool inBootOrder = AddToBootOrder(entry);
            LogHelper.Log("EFIFirmwareService:SetupEndlessLaunch: In Boot order: {0}", inBootOrder);

            if (!inBootOrder)
            {
                throw new FirmwareSetupException(FirmwareSetupErrorCode.AddToBootOrderError, "Add new entry to BootOrder failed");
            }
            else
            {
                if (!SetBootNext(entry))
                {
                    throw new FirmwareSetupException(FirmwareSetupErrorCode.SetBootNextError, "Set BootNext failed");
                }
            }
        }

        private bool SetBootNext(UefiGPTEntry entry)
        {
            LogHelper.Log("EFIFirmwareService:SetBootNext: Boot{0}", entry.number.ToString("X4"));
            byte[] bootNextBytes = BitConverter.GetBytes(entry.number);

            bool success = SetFirmwareEnvironmentVariable("BootNext", EFI_GLOBAL_VARIABLE, bootNextBytes, (uint)bootNextBytes.Length);
            if (success)
            {
                LogHelper.Log("EFIFirmwareService:SetBootNext: Success");
            }
            else
            {
                LogHelper.Log("EFIFirmwareService:SetBootNext: Error: {0}", Marshal.GetLastWin32Error());
            }
            return success;
        }

        private void PrintUefiEntries()
        {
            if (entries == null)
            {
                LogHelper.Log("EFIFirmwareService:PrintUefiEntries: No UEFI entries" );
                return;
            }

            LogHelper.Log("EFIFirmwareService:PrintUefiEntries UEFI Entries: {0}", entries.Count);

            foreach (UefiGPTEntry entry in entries)
            {
                LogHelper.Log("EFIFirmwareService:PrintUefiEntries:----------------------------");
                LogHelper.Log("EFIFirmwareService:PrintUefiEntries:Variable: Boot{0}", entry.number.ToString("X4"));
                LogHelper.Log("EFIFirmwareService:PrintUefiEntries:Partition Number: {0}", entry.partitionNumber);
                LogHelper.Log("EFIFirmwareService:PrintUefiEntries:Description: {0}", entry.description);
                LogHelper.Log("EFIFirmwareService:PrintUefiEntries:Device GUID: {0}", entry.guid);
                LogHelper.Log("EFIFirmwareService:PrintUefiEntries:FilePath: {0}", entry.path);
            }

            LogHelper.Log("EFIFirmwareService:PrintUefiEntries:----------------------------");
        }

        private UefiGPTEntry CreateUEfiEntry(string volume, string description, string path, long blockSize)
        {
            LogHelper.Log("EFIFirmwareService:CreateUEfiEntry: {0}", volume);
            LogHelper.Log("EFIFirmwareService:CreateUEfiEntry:Description: {0}", description);
            LogHelper.Log("EFIFirmwareService:CreateUEfiEntry:Path: {0}", path);
            LogHelper.Log("EFIFirmwareService:CreateUEfiEntry:BlockSize: {0}", blockSize);


            SafeFileHandle hndl = CreateFile(volume,
                                               GENERIC_READ | GENERIC_WRITE,
                                               FILE_SHARE_READ | FILE_SHARE_WRITE,
                                               IntPtr.Zero,
                                               OPEN_EXISTING,
                                               FILE_ATTRIBUTE_READONLY,
                                               IntPtr.Zero);
            if (hndl.IsInvalid)
            {
                LogHelper.Log("EFIFirmwareService:CreateUEfiEntry:Invalid handle:Error: {0}", Marshal.GetLastWin32Error());
                hndl.Close();
                throw new FirmwareSetupException(FirmwareSetupErrorCode.GetPartitionEspInfoError, "Get ESP partition information failed");
            }

            PARTITION_INFORMATION_EX partition = new PARTITION_INFORMATION_EX();

            UInt32 outBufferSize = (UInt32)Marshal.SizeOf(partition);
            IntPtr outBuffer = Marshal.AllocHGlobal((int)outBufferSize);
            UInt32 bytesReturned = 0;



            if (DeviceIoControl(hndl,
                             IOCTL_DISK_GET_PARTITION_INFO_EX,
                             IntPtr.Zero,
                             0,
                             outBuffer,
                             outBufferSize,
                             out bytesReturned,
                             IntPtr.Zero))
            {
                partition = (PARTITION_INFORMATION_EX)Marshal.PtrToStructure(outBuffer, typeof(PARTITION_INFORMATION_EX));
            }
            else
            {
                LogHelper.Log("EFIFirmwareService:CreateUEfiEntry:IOCTL_DISK_GET_PARTITION_INFO_EX Failed: Error: {0}", Marshal.GetLastWin32Error());
                Marshal.FreeHGlobal(outBuffer);
                hndl.Close();
                throw new FirmwareSetupException(FirmwareSetupErrorCode.GetPartitionEspInfoError, "Get ESP partition information failed");
            }

            PARTITION_INFORMATION_GPT gptPartition = partition.DriveLayoutInformaiton.Gpt;

            foreach (UefiGPTEntry entry in entries)
            {
                if (entry.guid.Equals(gptPartition.PartitionId))
                {
                    LogHelper.Log("EFIFirmwareService:CreateUEfiEntry:Entry for {0} already exists. Return the existing entry", gptPartition.PartitionId);
                    hndl.Close();
                    Marshal.FreeHGlobal(outBuffer);
                    return entry;
                }
            }

            EFI_HARD_DRIVE_PATH hdPath = new EFI_HARD_DRIVE_PATH();
            LogHelper.Log("EFIFirmwareService:CreateUEfiEntry: Partition StartingOffset: {0}", (UInt64)(partition.StartingOffset / blockSize));
            LogHelper.Log("EFIFirmwareService:CreateUEfiEntry: PartitionLength: {0}", (UInt64)(partition.PartitionLength / blockSize));


            //EFI_HARD_DRIVE_PATH
            hdPath.type = 0x4;
            hdPath.subtype = 0x1;
            hdPath.signature = new byte[16];
            hdPath.length = (ushort)Marshal.SizeOf(hdPath);
            hdPath.part_num = (uint)partition.PartitionNumber;
            hdPath.start = (UInt64)(partition.StartingOffset / blockSize);
            hdPath.size = (UInt64)(partition.PartitionLength / blockSize);
            hdPath.signature = gptPartition.PartitionId.ToByteArray();
            hdPath.mbr_type = 2; /* GPT */
            hdPath.signature_type = 2; /* GPT partition UUID */

            //EFI_END_DEVICE_PATH
            EFI_END_DEVICE_PATH endPath = new EFI_END_DEVICE_PATH();
            endPath.type = 0x7f; /* Descriptor end */
            endPath.subtype = 0xff; /* Full path end */
            endPath.length = 4;

            // EFI_FILE_PATH
            EFI_FILE_PATH filePath = new EFI_FILE_PATH();

            byte[] pathBytes = Encoding.Unicode.GetBytes(path);
            byte[] descBytes = Encoding.Unicode.GetBytes(description);

            filePath.length = (ushort)(Marshal.SizeOf(filePath) + pathBytes.Length + 2);
            filePath.type = 4; /* Media device */
            filePath.subtype = 4; /* File path */

            // EFI_LOAD_OPTION
            EFI_LOAD_OPTION loadOption = new EFI_LOAD_OPTION();
            loadOption.attributes = 1; //Mark as active
            loadOption.file_path_list_length = (ushort)(hdPath.length + filePath.length + endPath.length);

            int totalSize = Marshal.SizeOf(loadOption) + hdPath.length + filePath.length + endPath.length + descBytes.Length + 2;

            byte[] entryBytes = new byte[totalSize];
            int offset = 0;

            //Copy EFI_LOAD_OPTION
            offset += CopyStructureToArray<EFI_LOAD_OPTION>(loadOption, offset, entryBytes);

            //Copy Description
            Array.Copy(descBytes, 0, entryBytes, offset, descBytes.Length);

            offset += descBytes.Length;
            offset += 2;

            //Copy EFI_HARD_DRIVE_PATH
            offset += CopyStructureToArray<EFI_HARD_DRIVE_PATH>(hdPath, offset, entryBytes);

            //Copy EFI_FILE_PATH without the actual path string /EFI/BOOT/<loader>.efi
            offset += CopyStructureToArray<EFI_FILE_PATH>(filePath, offset, entryBytes);

            //Copy path i.e: /EFI/BOOT/<loader>.efi
            Array.Copy(pathBytes, 0, entryBytes, offset, pathBytes.Length);
            offset += pathBytes.Length;
            offset += 2;

            //Copy EFI_END_DEVICE_PATH
            offset += CopyStructureToArray<EFI_END_DEVICE_PATH>(endPath, offset, entryBytes);

            int firstFreeEntry = GetFirstFreeUefiEntryNumber();
            if (firstFreeEntry == -1)
            {
                LogHelper.Log("EFIFirmwareService:CreateUEfiEntry: Could not find a free Uefi entry:");
                Marshal.FreeHGlobal(outBuffer);
                hndl.Close();
                throw new FirmwareSetupException(FirmwareSetupErrorCode.FindFreeUefiEntryError, "Could not find a free Uefi entry");
            }

            bool success = SetFirmwareEnvironmentVariable("Boot" + firstFreeEntry.ToString("X4"), UEFI_BOOT_NAMESPACE, entryBytes, (uint)entryBytes.Length);

            UefiGPTEntry newEntry = null;
            if (success)
            {
                LogHelper.Log("EFIFirmwareService:CreateUEfiEntry: Entry Created:");

                newEntry = new UefiGPTEntry()
                {
                    description = description,
                    path = path,
                    partitionNumber = (int)hdPath.part_num,
                    guid = gptPartition.PartitionId,
                    number = (ushort)firstFreeEntry
                };
            }

            hndl.Close();
            Marshal.FreeHGlobal(outBuffer);

            return newEntry;
        }

        private bool AddToBootOrder(UefiGPTEntry entry)
        {
            LogHelper.Log("EFIFirmwareService:AddToBootOrder: Boot{0}", entry.number.ToString("X4"));
            UInt16[] bootOrder = GetBootOrder();
            LogHelper.Log("EFIFirmwareService:AddToBootOrder: Current BootOrder:");
            PrintBootOrder(bootOrder);

            foreach (UInt16 bootNumber in bootOrder)
            {
                if (entry.number == bootNumber)
                {
                    LogHelper.Log("EFIFirmwareService:AddToBootOrder: Already in BootOrder");
                    return true;
                }
            }

            byte[] newBootOrderBytes = new byte[(bootOrder.Length + 1) * sizeof(UInt16)];


            //Copy existing order bytes
            for (int i = 0; i < bootOrder.Length; i++)
            {
                Array.Copy(BitConverter.GetBytes(bootOrder[i]), 0, newBootOrderBytes, i * sizeof(UInt16), sizeof(UInt16));
            }

            Array.Copy(BitConverter.GetBytes(entry.number), 0, newBootOrderBytes, bootOrder.Length * sizeof(UInt16), sizeof(UInt16));

            bool success = SetFirmwareEnvironmentVariable("BootOrder", UEFI_BOOT_NAMESPACE, newBootOrderBytes, (uint)newBootOrderBytes.Length);

            if (!success)
            {
                LogHelper.Log("EFIFirmwareService:AddToBootOrder Failed: Error: {0}", Marshal.GetLastWin32Error());
            }
            LogHelper.Log("EFIFirmwareService:AddToBootOrder: New BootOrder: ");
            PrintBootOrder(GetBootOrder());

            return success;
        }

        private void PrintBootOrder(UInt16[] bootOrderArray)
        {
            if (bootOrderArray == null  || bootOrderArray.Length == 0)
            {
                LogHelper.Log("EFIFirmwareService:PrintBootOrder: BootOrder is null or empty");
                return;
            }


            StringBuilder bootOrderSb = new StringBuilder();

            for (int i = 0; i <= bootOrderArray.Length / 2; i++)
            {
                bootOrderSb.AppendFormat("Boot{0} ", bootOrderArray[i].ToString("X4"));
            }

            LogHelper.Log("EFIFirmwareService:PrintBootOrder: {0}: {1}", bootOrderArray.Length / 2 + 1, bootOrderSb.ToString());
        }

        private UInt16[] GetBootOrder()
        {
            UInt16[] bootOrderArray;

            byte[] bootOrderBytes = new byte[1000];
            uint size = GetFirmwareEnvironmentVariable("BootOrder", UEFI_BOOT_NAMESPACE, bootOrderBytes, (uint)bootOrderBytes.Length);

            if (size <= 0)
            {
                LogHelper.Log("EFIFirmwareService:GetBootOrder: Could not read BootOrder: Error: {0}", Marshal.GetLastWin32Error());
                return null;
            }

            bootOrderArray = new UInt16[size / 2];

            for (int i = 0; i < size / 2; i++)
            {
                bootOrderArray[i] = BitConverter.ToUInt16(bootOrderBytes, i * 2);
            }

            return bootOrderArray;
        }

        private int GetFirstFreeUefiEntryNumber()
        {
            for (int i = 0; i <= 0xffff; i++)
            {
                byte[] vardata = new byte[10000];
                uint size = GetFirmwareEnvironmentVariable(string.Format(LOAD_OPTION_FORMAT, i), EFI_GLOBAL_VARIABLE, vardata, (uint)vardata.Length);

                if (size == 0 && Marshal.GetLastWin32Error() == ERROR_ENVVAR_NOT_FOUND)
                {
                    LogHelper.Log("EFIFirmwareService:GetFreeUefiEntryNumber: Found: {0}", i);
                    return i;
                }
            }

            return -1;
        }

        private List<UefiGPTEntry> GetGPTUefiEntries()
        {
            List<UefiGPTEntry> entries = new List<UefiGPTEntry>();
            int countEmpty = 0;

            for (int i = 0; i <= 0xffff && countEmpty <= 5; i++)
            {
                byte[] vardata = new byte[10000];
                int signature_type = -1;
                uint size = GetFirmwareEnvironmentVariable(string.Format(LOAD_OPTION_FORMAT, i), EFI_GLOBAL_VARIABLE, vardata, (uint)vardata.Length);
                if (size > 0)
                {
                    UefiGPTEntry entry = new UefiGPTEntry();
                    entry.number = (UInt16)i;
                    entry.description = new string(Encoding.Unicode.GetString(vardata, 6, (int)size).TakeWhile(x => x != 0).ToArray());
                    var descBytes = Encoding.Unicode.GetBytes(entry.description);

                    //Device type
                    int devPathTypeStart = 6 + entry.description.Length * 2 + 2;
                    int iteration = 0;


                    //TODO Use EFI_HARD_DRIVE_PATH structure instead of parsing the entire byte array "manually"
                    while (devPathTypeStart + HARD_DRIVE_DISKPATH_SIZE < (int)size)
                    {
                        iteration++;
                        byte devPathType = vardata[devPathTypeStart];

                        //Device subtype
                        int devPathSubTypeStart = devPathTypeStart + 1;
                        byte devPathSubType = vardata[devPathSubTypeStart];

                        switch (devPathType)
                        {
                            case 0x4: //Device type switch 
                                switch (devPathSubType) //Device subtype switch
                                {
                                    case 0x1:
                                        int signature_type_index = devPathTypeStart + SIGNATURE_TYPE_OFFSET;
                                        signature_type = vardata[signature_type_index];
                                        switch (signature_type) //Device signature type
                                        {
                                            case 0x1: //MBR  We're interested only in GPT entries, as our Drive use a GPT partition table
                                                break;
                                            case 0x2: //GPT We're interested only in GPT entries, as our Drive use a GPT partition table
                                                int partNumOffset = devPathTypeStart + PARTITION_NUMBER_OFFSET;
                                                entry.partitionNumber = (int)BitConverter.ToUInt32(vardata, partNumOffset);

                                                int signatureOffet = devPathTypeStart + SIGNATURE_OFFSET;
                                                entry.guid = new Guid(vardata.Skip(signatureOffet).Take(16).ToArray());

                                                break;
                                        }
                                        break;

                                    case 0x4:
                                        {
                                            var filepath = new string(Encoding.Unicode.GetString(vardata, devPathTypeStart + 4, (int)size).TakeWhile(x => x != 0).ToArray());
                                            entry.path = filepath;
                                        }

                                        break;
                                }
                                break;
                            case 0x7f: // End of device path
                                break;
                        }

                        devPathTypeStart = devPathTypeStart + HARD_DRIVE_DISKPATH_SIZE;
                    }
                    entries.Add(entry);
                    countEmpty = 0;
                }
                else
                {
                    countEmpty++;
                }
            }
            return entries;
        }

        //TODO create a class/struct? It's not clear what's the purpose of KeyValuePair
        private KeyValuePair<string, long> GetESPVolume()
        {
            LogHelper.Log("EFIFirmwareService:GetESPVolume:");

            //Assume it's 512 bytes
            long blockSize = 512;

            //Get the physical disk for the application partition
            int physicalDriveNumber = systemVerificationService.CurrentPhysicalDiskIndex;
            LogHelper.Log("EFIFirmwareService:GetESPVolume: Current physical drive: {0}", physicalDriveNumber);

            if (physicalDriveNumber == -1)
                return new KeyValuePair<string, long>(null, blockSize);

            //Get the blockSize of the ESP partition
            //TODO query only the necessary fields
            ObjectQuery query = new ObjectQuery("SELECT * FROM Win32_DiskPartition where DiskIndex like " + physicalDriveNumber);
            ManagementObjectSearcher moSearcher = new ManagementObjectSearcher(query);

            foreach (ManagementObject mo in moSearcher.Get())
            {
                if ((bool)mo["Bootable"])
                {
                    blockSize = (long.Parse)(mo["BlockSize"].ToString());
                    break;
                }
            }

            //Query for the Volume{GUID}
            //TODO query only the necessary fields
            query = new ObjectQuery("SELECT * FROM Win32_Volume");
            moSearcher = new ManagementObjectSearcher(query);

            foreach (ManagementObject mo in moSearcher.Get())
            {
                //TODO Use regex
                string deviceId = mo["DeviceID"].ToString().Replace("\\", "").Replace("?", "");
                int disk = GetDiskForVolume(deviceId);

                //We assume the USB stick has exactly 2 partition
                //TODO Maybe add an extra check
                if (disk == physicalDriveNumber &&
                    (mo["DriveLetter"] == null || !mo["DriveLetter"].ToString().Equals(systemVerificationService.CurrentDriveLetter)))
                {
                    LogHelper.Log("EFIFirmwareService:GetESPVolume: ESP Volume: {0}", deviceId);
                    LogHelper.Log("EFIFirmwareService:GetESPVolume: ESP Name: {0}", mo["name"]);
                    LogHelper.Log("EFIFirmwareService:GetESPVolume: ESP FileSystem: {0}", mo["FileSystem"]);
                    LogHelper.Log("EFIFirmwareService:GetESPVolume: ESP Label: {0}", mo["Label"] == null ? "Null" : mo["Label"].ToString());

                    return new KeyValuePair<string, long>(deviceId, blockSize);
                }
            }

            return new KeyValuePair<string, long>(null, blockSize);
        }

        private int GetDiskForVolume(string volume)
        {
            int disk = -1;

            // CreateFile method accepts "\\\\.\\Volume{4b3d5e28-4b1a-11e9-b640-54bf64435496}"
            SafeFileHandle hndl = CreateFile(string.Format("\\\\.\\{0}", volume),
                                                           GENERIC_READ | GENERIC_WRITE,
                                                           FILE_SHARE_READ | FILE_SHARE_WRITE,
                                                           IntPtr.Zero,
                                                           OPEN_EXISTING,
                                                           FILE_ATTRIBUTE_READONLY,
                                                           IntPtr.Zero);
            if (hndl.IsInvalid)
            {
                LogHelper.Log("EFIFirmwareService:GetDiskForVolume: Invalid handle:");
            } else
            {
                VOLUME_DISK_EXTENTS vde = new VOLUME_DISK_EXTENTS();
                UInt32 outBufferSize = (UInt32)Marshal.SizeOf(vde);
                IntPtr outBuffer = Marshal.AllocHGlobal((int)outBufferSize);
                UInt32 bytesReturned = 0;

                if (DeviceIoControl(hndl,
                                             IOCTL_VOLUME_GET_VOLUME_DISK_EXTENTS,
                                             IntPtr.Zero,
                                             0,
                                             outBuffer,
                                             outBufferSize,
                                             out bytesReturned,
                                             IntPtr.Zero))
                {
                    Marshal.PtrToStructure(outBuffer, vde);
                    disk = Convert.ToInt32(vde.Extents.DiskNumber);
                }

                Marshal.FreeHGlobal(outBuffer);
            }

            hndl.Close();
            return disk;
        }

        private int GetDiskForMountedDrive(string driveLetter)
        {
            int disk = -1;

            //TODO query only the necessary fields
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

                //TODO query only the necessary fields
                queryResults = new ManagementObjectSearcher(query);

                ManagementObjectCollection drives = queryResults.Get();

                foreach (var drive in drives)
                    disk = int.Parse(drive["DeviceID"].ToString().Replace("\\\\.\\PHYSICALDRIVE", ""));
            }

            return disk;
        }

        private void ObtainPrivileges(string privilege)
        {
            LogHelper.Log("EFIFirmwareService:ObtainPrivileges: {0}", privilege);
            IntPtr hToken = IntPtr.Zero;
            IntPtr processhandle = IntPtr.Zero;

            processhandle = GetCurrentProcess();

            if (!OpenProcessToken(processhandle, TOKEN_ADJUST_PRIVILEGES | TOKEN_QUERY, ref hToken))
            {
                throw new FirmwareSetupException(FirmwareSetupErrorCode.OpenProcessTokenError, "ObtainPrivileges: OpenProcessToken failed: " + privilege);
            }

            TokenPrivelege tp;
            tp.Count = 1;
            tp.Luid = 0;
            tp.Attr = SE_PRIVILEGE_ENABLED;

            if (!LookupPrivilegeValue(null, privilege, ref tp.Luid))
                throw new FirmwareSetupException(FirmwareSetupErrorCode.LookupPrivilegeError, "ObtainPrivileges: LookupPrivilegeValue failed: " + privilege);

            if (!AdjustTokenPrivileges(hToken, false, ref tp, 0, IntPtr.Zero, IntPtr.Zero))
                throw new FirmwareSetupException(FirmwareSetupErrorCode.AdjustTokenPrivilegeError, "ObtainPrivileges: AdjustTokenPrivileges failed:" + privilege);
        }

    }
}
