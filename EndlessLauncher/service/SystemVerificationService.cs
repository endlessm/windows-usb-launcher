using EndlessLauncher.logger;
using EndlessLauncher.model;
using System;
using System.Collections.Generic;
using System.Security.Principal;
using static EndlessLauncher.NativeAPI;

namespace EndlessLauncher.service
{
    //TODO implement async query
    public class SystemVerificationService
    {
        private enum SupportedOS
        {
            Windows10 = 10
        }

        private static readonly List<FirmwareType> supportedFirmwares = new List<FirmwareType>
        {
            FirmwareType.FirmwareTypeUefi
        };

        private const int MINIMUM_RAM = 2 * 1024 * 1024; //KB
        private const int SUPPORTED_WINDOWS_VERSION = 10;
        private const int MINIMUM_CPU_CORES = 2;

        private const string REQUIREMENT_KEY_ADMINISTRATOR = "AdministratorPriveleges";
        private const string REQUIREMENT_KEY_WINDOWS_VERSION = "WindowsVersion";
        private const string REQUIREMENT_KEY_MINIMUM_RAM = "MinimumRAM";
        private const string REQUIREMENT_KEY_ARCHITECTURE = "64-BitsOperatingSystem";
        private const string REQUIREMENT_KEY_MULTI_CORE_CPU = "MultiCoreCPU";
        private const string REQUIREMENT_KEY_SUPPORTED_FIRMWARE = "SupportedFirmware";

        private static readonly FirmwareType firmwareType;

        public static FirmwareType FirmwareType
        {
            get
            {
                return firmwareType;
            }
        }

        static SystemVerificationService()
        {
            GetFirmwareType(ref firmwareType);
        }

        private Dictionary<string, bool> requirements = new Dictionary<string, bool>()
        {
            { REQUIREMENT_KEY_ADMINISTRATOR, false },
            { REQUIREMENT_KEY_WINDOWS_VERSION, false },
            { REQUIREMENT_KEY_MINIMUM_RAM, false },
            { REQUIREMENT_KEY_ARCHITECTURE, Environment.Is64BitOperatingSystem },
            { REQUIREMENT_KEY_MULTI_CORE_CPU, false },
            { REQUIREMENT_KEY_SUPPORTED_FIRMWARE, false },
        };

        //TODO Don't check for everything if one fails? i.e. Supported Firmware != UEFI
        public Dictionary<string, bool> VerifyRequirements()
        {
            LogHelper.Log("SystemVerificationService:VerifyRequirements: ");

            requirements[REQUIREMENT_KEY_SUPPORTED_FIRMWARE] = IsFirwareSupported();
            requirements[REQUIREMENT_KEY_MULTI_CORE_CPU] = CheckCPUCoresCount();
            requirements[REQUIREMENT_KEY_ADMINISTRATOR] = RunningAsAdministrator();
            requirements[REQUIREMENT_KEY_WINDOWS_VERSION] = VerifyWindowsVersion();
            requirements[REQUIREMENT_KEY_MINIMUM_RAM] = VerifyRAM();
            requirements[REQUIREMENT_KEY_MULTI_CORE_CPU] = CheckCPUCoresCount();


            return requirements;
        }

        private bool IsFirwareSupported()
        {
            return supportedFirmwares.Contains(FirmwareType);
        }

        private bool RunningAsAdministrator()
        {
            WindowsIdentity windowsIdentity = WindowsIdentity.GetCurrent();
            WindowsPrincipal windowsPrincipal = new WindowsPrincipal(windowsIdentity);

            return windowsPrincipal.IsInRole(WindowsBuiltInRole.Administrator);
        }

        private bool VerifyWindowsVersion()
        {
            LogHelper.Log("SystemVerificationService:VerifyWindowsVersion: ");

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
            long totalInstalledRam;

            NativeAPI.GetPhysicallyInstalledSystemMemory(out totalInstalledRam);

            LogHelper.Log("SystemVerificationService:VerifyRAM: Installed: " + totalInstalledRam / 1024 / 1024 + " Gb");

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

            LogHelper.Log("CheckCPUCoresCount:NumberOfCores: " + coreCount);

            return coreCount >= MINIMUM_CPU_CORES;
        }

    }
}
