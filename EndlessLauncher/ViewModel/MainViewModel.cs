using EndlessLauncher.logger;
using EndlessLauncher.model;
using EndlessLauncher.service;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using System.Collections.Generic;
using System.Threading;

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

        private Dictionary<string, Requirement> requirements;

        private RelayCommand launchRelayCommand;
        private RelayCommand closeRelayCommand;
        private RelayCommand rebootRelayCommand;
        private string firmwareSetupStatus = "Status:";

        public MainViewModel(FirmwareServiceBase firmwareService, SystemVerificationService sysInfoService)
        {
            this.firmwareService = firmwareService;
            this.sysInfoService = sysInfoService;
            this.firmwareService.SetupCompleted += FirmwareService_SetupCompleted ;

            requirements = sysInfoService.Verify();
            CheckRequirements();
        }

        //TODO handle Errors
        private void FirmwareService_SetupCompleted(object sender, FirmwareSetupResult e)
        {
            LogHelper.Log("FirmwareService_SetupCompleted:");
            if (e.Success)
            {
                FirmwareSetupStatus = "Status: Success";
            } else
            {
                FirmwareSetupStatus = "Status: ErrorCode: " + (int)e.Error.Code;
            }
            RebootEnabled = true;
        }

        private void CheckRequirements()
        {
            bool allRequirementsMet = true;
            foreach(KeyValuePair<string, Requirement> pair in requirements)
            {
                //TODO handle individual errors
                allRequirementsMet &= pair.Value.IsMet;
            }

            LaunchEnabled = allRequirementsMet;
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

        public RelayCommand CloseRelayCommand
        {
            get
            {
                if (closeRelayCommand == null)
                {
                    closeRelayCommand = new RelayCommand(() =>
                    {
                        LogHelper.Log("CloseRelayCommand: ");
                        System.Windows.Application.Current.Shutdown();
                    });
                }

                return closeRelayCommand;
            }
        }

        public RelayCommand RebootRelayCommand
        {
            get
            {
                if (rebootRelayCommand == null)
                {
                    rebootRelayCommand = new RelayCommand(() =>
                    {
                        LogHelper.Log("rebootRelayCommand: ");
                        firmwareService.Reboot();
                        
                    });
                }

                return rebootRelayCommand;
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