using EndlessLauncher.logger;
using EndlessLauncher.model;
using System;
using System.Threading.Tasks;
using static EndlessLauncher.NativeAPI;

namespace EndlessLauncher.service
{
    public abstract class FirmwareServiceBase
    {
        public event EventHandler SetupCompleted;
        public event EventHandler<FirmwareSetupErrorCode> SetupFailed;
        protected SystemVerificationService systemVerificationService;

        public FirmwareServiceBase(SystemVerificationService service)
        {
            this.systemVerificationService = service;
        }

        public async void SetupEndlessLaunchAsync()
        {
            LogHelper.Log("SetupEndlessLaunchAsync:Start:");
            
            try
            {
                await Task.Run(() => SetupEndlessLaunch());
                SetupCompleted?.Invoke(this, null);
            } catch(FirmwareSetupException ex)
            {
                LogHelper.Log("SetupEndlessLaunchAsync:FirmwareSetupException: " + ex.Message);
                SetupFailed?.Invoke(this, ex.Code);

            } catch(Exception ex)
            {
                LogHelper.Log("SetupEndlessLaunchAsync:Exception: " + ex.Message);
                SetupFailed?.Invoke(this, FirmwareSetupErrorCode.GenericError);
            }

            LogHelper.Log("SetupEndlessLaunchAsync:End:");
        }

        public void Reboot()
        {
            if (!ExitWindowsEx(ExitWindows.EwxReboot,
                ShutdownReason.MajorOther |
                ShutdownReason.MinorOther |
                ShutdownReason.FlagPlanned))
            {
                LogHelper.Log("FirmwareServiceBase: ExitWindowsEx: Failed");
            }
        }

        protected abstract void SetupEndlessLaunch();
    }

}
