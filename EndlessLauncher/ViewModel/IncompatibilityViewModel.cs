// © 2019–2020 Endless OS Foundation LLC
//
// This file is part of Endless Launcher.
//
// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at http://mozilla.org/MPL/2.0/.
using EndlessLauncher.logger;
using EndlessLauncher.model;
using EndlessLauncher.Resources;
using EndlessLauncher.service;
using EndlessLauncher.utility;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using System.IO;

namespace EndlessLauncher.ViewModel
{
    public class IncompatibilityViewModel : ViewModelBase
    {
        private RelayCommand wrongUSBPortInfoRelayCommand;
        private RelayCommand backRelayCommand;
        private RelayCommand showLogRelayCommand;
        private RelayCommand supportRelayCommand;
        private IFrameNavigationService navigationService;
        private SystemVerificationErrorCode systemVerificationErrorCode = SystemVerificationErrorCode.NoError;
        private FirmwareSetupErrorCode firmwareSetupErrorCode = FirmwareSetupErrorCode.NoError;

        public IncompatibilityViewModel(IFrameNavigationService frameNavigationService)
        {
            this.navigationService = frameNavigationService;
        }

        public RelayCommand WrongUSBPortInfoRelayCommand
        {
            get
            {
                return wrongUSBPortInfoRelayCommand
                    ?? (wrongUSBPortInfoRelayCommand = new RelayCommand(
                    () =>
                    {
                        navigationService.NavigateTo("WrongUSBPortInfoPage");
                    }));
            }
        }

        public RelayCommand BackRelayCommand
        {
            get
            {
                return backRelayCommand
                    ?? (backRelayCommand = new RelayCommand(
                    () =>
                    {
                        navigationService.NavigateTo("WelcomePage");
                    }));
            }
        }

        public RelayCommand ShowLogRelayCommand
        {
            get
            {
                return showLogRelayCommand
                    ?? (showLogRelayCommand = new RelayCommand(
                    () =>
                    {
                        string filePath = LogHelper.LogFilePath;
                        if (!File.Exists(filePath))
                        {
                            return;
                        }

                        Utils.OpenUrl("explorer.exe", string.Format("/select, \"{0}\"", filePath));
                    }));
            }
        }

        public RelayCommand SupportRelayCommand
        {
            get
            {
                return supportRelayCommand
                    ?? (supportRelayCommand = new RelayCommand(
                    () =>
                    {
                        Utils.OpenUrl("https://support.endlessm.com/hc/en-us", null);
                    }));
            }
        }

        public SystemVerificationErrorCode SystemVerificationErrorCode
        {
            get
            {
                return systemVerificationErrorCode;
            }
            set
            {
                Set(ref systemVerificationErrorCode, value);
                RaisePropertyChanged("ErrorMessage");
            }
        }

         public FirmwareSetupErrorCode FirmwareSetupErrorCode
        {
            get
            {
                return firmwareSetupErrorCode;
            }
            set
            {
                Set(ref firmwareSetupErrorCode, value);
                RaisePropertyChanged("ErrorMessage");
            }
        }

        public bool IsFirmwareError
        {
            get
            {
                return this.FirmwareSetupErrorCode != FirmwareSetupErrorCode.NoError;
            }
        }

        public bool IsWrongUSBPort
        {
            get
            {
                return this.SystemVerificationErrorCode == SystemVerificationErrorCode.NotUSB30Port;
            }
        }

        public string ErrorTitle
        {
            get
            {
                return IsFirmwareError ? Literals.launch_failed : Literals.launch_not_supported;
            }
        }

        public string ErrorMessage
        {
            get
            {
                var errorStringId = "error_unknown";

                if (this.FirmwareSetupErrorCode != FirmwareSetupErrorCode.NoError)
                {
                    errorStringId = string.Format("error_firmware_{0}", this.FirmwareSetupErrorCode.ToString());
                }
                else if (this.SystemVerificationErrorCode != SystemVerificationErrorCode.NoError)
                {
                    errorStringId = string.Format("error_system_{0}", this.SystemVerificationErrorCode.ToString());
                }

                return string.Format(
                    "{0}: {1}",
                    Literals.error_details,
                    Literals.ResourceManager.GetString(errorStringId, Literals.Culture) ?? errorStringId
                );
            }
        }
    }
}
