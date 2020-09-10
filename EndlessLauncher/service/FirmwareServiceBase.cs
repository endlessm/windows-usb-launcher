// © 2019–2020 Endless OS Foundation LLC
//
// This file is part of Endless Launcher.
//
// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at http://mozilla.org/MPL/2.0/.
using EndlessLauncher.logger;
using EndlessLauncher.model;
using System;
using System.Threading.Tasks;
using static EndlessLauncher.NativeMethods;
using static EndlessLauncher.NativeAPI;
using EndlessLauncher.utility;

namespace EndlessLauncher.service
{
    public abstract class FirmwareServiceBase
    {
        public event EventHandler SetupCompleted;
        public event EventHandler<EndlessErrorEventArgs<FirmwareSetupErrorCode>> SetupFailed;
        protected SystemVerificationService systemVerificationService;

        public FirmwareServiceBase(SystemVerificationService service)
        {
            this.systemVerificationService = service;
        }

        public async void SetupEndlessLaunchAsync(string description, string path)
        {
            LogHelper.Log("SetupEndlessLaunchAsync:");

            if (Debug.SimulatedFirmwareError != FirmwareSetupErrorCode.NoError)
            {
                SetupFailed?.Invoke(this, new EndlessErrorEventArgs<FirmwareSetupErrorCode>
                {
                    ErrorCode = Debug.SimulatedFirmwareError
                });

                return;
            }

            try
            {
                await Task.Run(() => SetupEndlessLaunch(description, path));
                SetupCompleted?.Invoke(this, null);
            }
            catch (FirmwareSetupException ex)
            {
                LogHelper.Log("SetupEndlessLaunchAsync:FirmwareSetupException: Error:{0} Message:{1}", ex.Code, ex.Message);
                LogHelper.Flush();
                SetupFailed?.Invoke(this, new EndlessErrorEventArgs<FirmwareSetupErrorCode>
                {
                    ErrorCode = ex.Code
                });

            }
            catch (Exception ex)
            {
                LogHelper.Log("SetupEndlessLaunchAsync:Exception: Message: {0}", ex.Message);
                LogHelper.Log("SetupEndlessLaunchAsync:Exception: StackTrace: {0}", ex.StackTrace);
                LogHelper.Flush();
                SetupFailed?.Invoke(this, new EndlessErrorEventArgs<FirmwareSetupErrorCode>
                {
                    ErrorCode = FirmwareSetupErrorCode.GenericFirmwareError
                });
            }

        }

        public void Reboot()
        {
            if (!ExitWindowsEx(ExitWindows.EwxReboot,
                ShutdownReason.MajorOther |
                ShutdownReason.MinorOther |
                ShutdownReason.FlagPlanned))
            {
                LogHelper.Log("FirmwareServiceBase: Reboot Failed");
            }
        }

        protected abstract void SetupEndlessLaunch(string description, string path);
    }

}
