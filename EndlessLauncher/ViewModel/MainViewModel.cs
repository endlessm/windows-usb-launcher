// © 2019–2020 Endless OS Foundation LLC
//
// This file is part of Endless Launcher.
//
// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at http://mozilla.org/MPL/2.0/.
using EndlessLauncher.model;
using EndlessLauncher.service;
using EndlessLauncher.utility;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using GalaSoft.MvvmLight.Ioc;
using Microsoft.Win32;
using System.Linq;
using WindowsFirewallHelper;
using WindowsFirewallHelper.FirewallAPIv2.Rules;

namespace EndlessLauncher.ViewModel
{
    /// <summary>
    /// This class contains properties that the main View can data bind to.
    /// <para>
    /// Use the <strong>mvvminpc</strong> snippet to add bindable properties to this ViewModel.
    /// </para>
    /// <para>
    /// You can also use Blend to data bind with the tool's support.
    /// </para>
    /// <para>
    /// See http://www.galasoft.ch/mvvm
    /// </para>
    /// </summary>
    public class MainViewModel : ViewModelBase
    {
        private FirmwareServiceBase firmwareService;
        private SystemVerificationService sysInfoService;

        private bool launchSupported;

        private RelayCommand launchRelayCommand;
        private RelayCommand openKiwixRelayCommand;
        private RelayCommand openKolibriRelayCommand;
        private RelayCommand moreInformationRelayCommand;
        private RelayCommand openReadmeRelayCommand;
        private RelayCommand closeRelayCommand;

        private IFrameNavigationService navigationService;
        private static readonly string EFI_BOOTLOADER_PATH = "\\EFI\\BOOT\\BOOTX64.EFI";
        private static readonly string ENDLESS_ENTRY_DESCRIPTION = "Endless OS";

        public MainViewModel(SystemVerificationService sysInfoService, IFrameNavigationService frameNavigationService)
        {
            this.navigationService = frameNavigationService;
            this.sysInfoService = sysInfoService;

            //Subscribe for verification events
            this.sysInfoService.VerificationFailed += SysInfoService_VerificationFailed;
            this.sysInfoService.VerificationPassed += SysInfoService_VerificationPassed;

        }

        private void SysInfoService_VerificationPassed(object sender, System.EventArgs e)
        {
            navigationService.NavigateTo("WelcomePage");

            //Subscribe for firmware setup events
            firmwareService = SimpleIoc.Default.GetInstance<FirmwareServiceBase>();
            firmwareService.SetupCompleted += FirmwareService_SetupCompleted;
            firmwareService.SetupFailed += FirmwareService_SetupFailed;

            LaunchSupported = true;
        }

        private void SysInfoService_VerificationFailed(object sender, EndlessErrorEventArgs<SystemVerificationErrorCode> e)
        {
            SimpleIoc.Default.GetInstance<IncompatibilityViewModel>().SystemVerificationErrorCode = e.ErrorCode;
            navigationService.NavigateTo("WelcomePage");

            LaunchSupported = false;
        }

        private void FirmwareService_SetupFailed(object sender, EndlessErrorEventArgs<FirmwareSetupErrorCode> e)
        {
            SimpleIoc.Default.GetInstance<IncompatibilityViewModel>().FirmwareSetupErrorCode = e.ErrorCode;
            navigationService.NavigateTo("IncompatibilityPage");
        }

        private void FirmwareService_SetupCompleted(object sender, System.EventArgs e)
        {
            firmwareService.Reboot();
        }

        public RelayCommand LaunchRelayCommand
        {
            get
            {
                if (launchRelayCommand == null)
                {
                    launchRelayCommand = new RelayCommand(() =>
                    {
                        firmwareService.SetupEndlessLaunchAsync(ENDLESS_ENTRY_DESCRIPTION, EFI_BOOTLOADER_PATH);
                    });
                }

                return launchRelayCommand;
            }
        }

        public RelayCommand OpenKiwixRelayCommand
        {
            get
            {
                if (openKiwixRelayCommand == null)
                {
                    openKiwixRelayCommand = new RelayCommand(() => LauncherShortcuts.OpenKiwix());
                }

                return openKiwixRelayCommand;
            }
        }

        public RelayCommand OpenKolibriRelayCommand
        {
            get
            {
                if (openKolibriRelayCommand == null)
                {
                    openKolibriRelayCommand = new RelayCommand(() => LauncherShortcuts.OpenKolibri());
                }

                return openKolibriRelayCommand;
            }
        }

        public RelayCommand MoreInformationRelayCommand
         {
            get
            {
                if (moreInformationRelayCommand == null)
                {
                    moreInformationRelayCommand = new RelayCommand(() =>
                    {
                        navigationService.NavigateTo("IncompatibilityPage");
                    });
                }

                return moreInformationRelayCommand;
            }
        }

        public RelayCommand OpenReadmeRelayCommand
        {
            get
            {
                if (openReadmeRelayCommand == null)
                {
                    openReadmeRelayCommand = new RelayCommand(() => LauncherShortcuts.OpenReadme());
                }

                return openReadmeRelayCommand;
            }
        }

        private RelayCommand loadedCommand;
        public RelayCommand LoadedCommand
        {
            get
            {
                return loadedCommand
                    ?? (loadedCommand = new RelayCommand(
                    () =>
                    {
                        sysInfoService.VerifyRequirements();
                    }));
            }
        }

        public RelayCommand CloseRelayCommand
        {
            get
            {
                return closeRelayCommand
                    ?? (closeRelayCommand = new RelayCommand(
                    () =>
                    {
                        System.Windows.Application.Current.Shutdown();
                    }));
            }
        }

        public bool LaunchSupported
        {
            get
            {
                return launchSupported;
            }
            set
            {
                Set(ref launchSupported, value);
            }
        }

        public bool LaunchNotSupported
        {
            get
            {
                return !launchSupported;
            }
        }
    }
}
