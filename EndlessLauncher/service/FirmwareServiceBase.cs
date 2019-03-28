using EndlessLauncher.logger;
using System;
using System.Threading.Tasks;
using static EndlessLauncher.NativeAPI;

namespace EndlessLauncher.service
{
    public abstract class FirmwareServiceBase
    {
        public event EventHandler SetupCompleted;

        public async void SetupEndlessLaunchAsync()
        {
            LogHelper.Log("SetupEndlessLaunchAsync:Start:");

            await Task.Run(() => SetupEndlessLaunch());

            LogHelper.Log("SetupEndlessLaunchAsync:End:");

            SetupCompleted?.Invoke(this, null);
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

        protected abstract void SetupEndlessLaunch();
    }

}
