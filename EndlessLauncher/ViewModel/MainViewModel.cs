using EndlessLauncher.model;
using EndlessLauncher.service;
using EndlessLauncher.utility;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using GalaSoft.MvvmLight.Ioc;
using HWND = System.IntPtr;
using Microsoft.Win32;
using System;
using System.Runtime.InteropServices;
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
        private static readonly string FIREWALL_KOLIBRI_RULE_NAME = "Kolibri";
        private static readonly int WIN32_SW_RESTORE = 9;

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

        [DllImport("USER32.DLL")]
        private static extern HWND FindWindowA(string lpClassName, string lpWindowName);

        [DllImport("USER32.DLL")]
        private static extern bool ShowWindow(HWND hWnd, int nCmdShow);

        [DllImport("USER32.DLL")]
        private static extern bool BringWindowToTop(HWND hWnd);

        private string GetExecutableDirectory()
        {
            return System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);
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
                    openKiwixRelayCommand = new RelayCommand(() =>
                    {
                        var vcRuntimeInstalled = Registry.GetValue(
                            "HKEY_LOCAL_MACHINE\\SOFTWARE\\WOW6432Node\\Microsoft\\VisualStudio\\14.0\\VC\\Runtimes\\X64",
                            "Installed", null
                        );

                        if (vcRuntimeInstalled == null || (int) vcRuntimeInstalled == 0)
                        {
                            var vcRuntimeInstallerPath = System.IO.Path.Combine(
                                GetExecutableDirectory(),
                                ".kiwix-windows",
                                "vc_redist.x64.exe"
                            );

                            // TODO: Wait asynchronously while disabling the Kiwix button
                            Utils.OpenUrl(vcRuntimeInstallerPath, "/install /quiet").WaitForExit();
                        }

                        var kiwixExePath = System.IO.Path.Combine(
                            GetExecutableDirectory(),
                            ".kiwix-windows",
                            "kiwix-desktop.exe"
                        );

                        var encyclopediaZimPath =  System.IO.Path.Combine(new string[] {
                            GetExecutableDirectory(),
                            ".kiwix", "flatpak",
                            "com.endlessm.encyclopedia.en-openzim-subscription",
                            "1.zim"
                        });

                        Utils.OpenUrl(kiwixExePath, encyclopediaZimPath);
                    });
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
                    openKolibriRelayCommand = new RelayCommand(() =>
                    {
                        // Show Kolibri window if already exists.
                        // Ideally, this should be done in the Kolibri app but for now we
                        // stick with this approach.
                        HWND hWnd = FindWindowA("wxWindowNR", "Kolibri");

                        if (hWnd != IntPtr.Zero)
                        {
                            ShowWindow(hWnd, WIN32_SW_RESTORE);
                            BringWindowToTop(hWnd);
                            return;
                        }

                        var kolibriExePath = System.IO.Path.Combine(
                            GetExecutableDirectory(),
                            ".kolibri-windows",
                            "Kolibri.exe"
                        );

                        var existingFwRule = FirewallManager.Instance.Rules.SingleOrDefault(fwRule => {
                            if (!(fwRule is StandardRuleWin8))
                            {
                                return false;
                            }

                            var win8Rule = (StandardRuleWin8)fwRule;
                            return win8Rule.Name == FIREWALL_KOLIBRI_RULE_NAME && win8Rule.ApplicationName == kolibriExePath;
                        });

                        if (existingFwRule == null)
                        {
                            var fwRule = FirewallManager.Instance.CreateApplicationRule(
                                FirewallManager.Instance.GetProfile().Type,
                                FIREWALL_KOLIBRI_RULE_NAME,
                                FirewallAction.Allow,
                                kolibriExePath
                            );
                            fwRule.Direction = FirewallDirection.Inbound;
                            FirewallManager.Instance.Rules.Add(fwRule);
                        }

                        Utils.OpenUrl(kolibriExePath, "");
                    });
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
                    openReadmeRelayCommand = new RelayCommand(() =>
                    {
                        var readmePath = System.IO.Path.Combine(
                            GetExecutableDirectory(),
                            "README.pdf"
                        );

                        Utils.OpenUrl(readmePath, "");
                    });
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
