using EndlessLauncher.logger;
using EndlessLauncher.model;
using EndlessLauncher.service;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;

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

        private bool launchEnabled;
        private bool rebootEnabled;

        private RelayCommand launchRelayCommand;
        private RelayCommand closeRelayCommand;
        private RelayCommand rebootRelayCommand;

        private string firmwareSetupStatus = "Status:";
        private IFrameNavigationService navigationService;

        public MainViewModel(FirmwareServiceBase firmwareService, SystemVerificationService sysInfoService, IFrameNavigationService frameNavigationService)
        {
            this.navigationService = frameNavigationService;

            this.firmwareService = firmwareService;
            this.sysInfoService = sysInfoService;

            //Subscribe for firmware setup events
            this.firmwareService.SetupCompleted += FirmwareService_SetupCompleted;
            this.firmwareService.SetupFailed += FirmwareService_SetupFailed;

            //Subscribe for verification events
            this.sysInfoService.VerificationFailed += SysInfoService_VerificationFailed;
            this.sysInfoService.VerificationPassed += SysInfoService_VerificationPassed;

            sysInfoService.VerifyRequirements();
        }

        private void SysInfoService_VerificationPassed(object sender, System.EventArgs e)
        {
            LaunchEnabled = true;
        }

        private void SysInfoService_VerificationFailed(object sender, SystemVerificationErrorCode e)
        {
            FirmwareSetupStatus = "Status: ErrorCode: " + (int)e + " " + e;
        }

        private void FirmwareService_SetupFailed(object sender, FirmwareSetupErrorCode e)
        {
            FirmwareSetupStatus = "Status: ErrorCode: " + (int)e + " " + e;
        }

        private void FirmwareService_SetupCompleted(object sender, System.EventArgs e)
        {
            FirmwareSetupStatus = "Status: Firmware Setup: Success";
            RebootEnabled = true;
        }

        public RelayCommand LaunchRelayCommand
        {
            get
            {
                if (launchRelayCommand == null)
                {
                    launchRelayCommand = new RelayCommand(() =>
                    {
                        LaunchEnabled = false;
                        firmwareService.SetupEndlessLaunchAsync();
                    });
                }

                return launchRelayCommand;
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
                        navigationService.NavigateTo("IncompatibilityPage");
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
                        LogHelper.Log("CloseRelayCommand: ");
                        System.Windows.Application.Current.Shutdown();
                    }));
            }
        }

        public RelayCommand RebootRelayCommand
        {
            get
            {
                return rebootRelayCommand
                    ?? (rebootRelayCommand = new RelayCommand(
                    () =>
                    {
                        LogHelper.Log("rebootRelayCommand: ");
                        firmwareService.Reboot();
                    }));
            }
        }

        public bool LaunchEnabled
        {
            get
            {
                return launchEnabled;
            }
            set
            {
                Set(ref launchEnabled, value);
            }
        }

        public bool RebootEnabled
        {
            get
            {
                return rebootEnabled;
            }
            set
            {
                Set(ref rebootEnabled, value);
            }
        }

        public string FirmwareSetupStatus
        {
            get
            {
                return firmwareSetupStatus;
            }
            set
            {
                Set(ref firmwareSetupStatus, value);
            }
        }

    }
}