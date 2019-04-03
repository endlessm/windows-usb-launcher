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

        private RelayCommand launchRelayCommand;
        private RelayCommand closeRelayCommand;

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

        }

        private void SysInfoService_VerificationPassed(object sender, System.EventArgs e)
        {
            navigationService.NavigateTo("WelcomePage");
            LaunchEnabled = true;
        }

        private void SysInfoService_VerificationFailed(object sender, SystemVerificationErrorCode e)
        {

            LogHelper.Log("SystemVerificationFailed: " + e);
            switch (e)
            {
                case SystemVerificationErrorCode.NotUSB30Port:
                    navigationService.NavigateTo("WrongUSBPortPage");
                    break;

                default:
                    navigationService.NavigateTo("IncompatibilityPage");
                    break;
            }
        }

        private void FirmwareService_SetupFailed(object sender, FirmwareSetupErrorCode e)
        {
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
                        LogHelper.Log("CloseRelayCommand: ");
                        System.Windows.Application.Current.Shutdown();
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
    }
}