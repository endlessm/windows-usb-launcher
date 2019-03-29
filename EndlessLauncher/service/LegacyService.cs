using EndlessLauncher.model;
using System;

namespace EndlessLauncher.service
{
    public class LegacyFirmwareService : FirmwareServiceBase
    {
        protected override FirmwareSetupResult SetupEndlessLaunch()
        {
            throw new FirmwareSetupException(FirmwareSetupException.ErrorCode.BiosModeLegacy, "Legacy Bios mode");
        }
    }
}
