using EndlessLauncher.logger;
using EndlessLauncher.model;
using System;
using System.Threading.Tasks;
using static EndlessLauncher.NativeAPI;

namespace EndlessLauncher.service
{
    public abstract class FirmwareServiceBase
    {
        public event EventHandler<FirmwareSetupResult> SetupCompleted;

        public async void SetupEndlessLaunchAsync()
        {
            LogHelper.Log("SetupEndlessLaunchAsync:Start:");
            FirmwareSetupResult result;

            try
            {
                result = await Task.Run(() => SetupEndlessLaunch());
            } catch(FirmwareSetupException ex)
            {
                LogHelper.Log("SetupEndlessLaunchAsync:FirmwareSetupException: " + ex.Message);
                result = FirmwareSetupResult.CreateFailed(ex);

            } catch(Exception ex)
            {
                LogHelper.Log("SetupEndlessLaunchAsync:Exception: " + ex.Message);
                result = FirmwareSetupResult.CreateFailed(new FirmwareSetupException(FirmwareSetupException.ErrorCode.GenericError, "Unknown Error"));
            }

            LogHelper.Log("SetupEndlessLaunchAsync:End:");

            SetupCompleted?.Invoke(this, result);
        }

        public void Reboot()
        {
            if (!ExitWindowsEx(ExitWindows.EwxReboot,
                ShutdownReason.MajorOther |
                ShutdownReason.MinorOther |
                ShutdownReason.FlagPlanned))
            {
                LogHelper.Log("FirmwareServiceBase: EWX_REBOOT: Failed");
            }
        }

        protected abstract FirmwareSetupResult SetupEndlessLaunch();
    }

}
