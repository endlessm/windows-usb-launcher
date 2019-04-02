using EndlessLauncher.model;
using System;

namespace EndlessLauncher.service
{
    public class LegacyFirmwareService : FirmwareServiceBase
    {
        public LegacyFirmwareService(SystemVerificationService service) : base(service) { }

        protected override void SetupEndlessLaunch()
        {
            throw new FirmwareSetupException(FirmwareSetupErrorCode.FirmwareServiceInitializationError, "Unsupported firmware");
        }
    }
}
