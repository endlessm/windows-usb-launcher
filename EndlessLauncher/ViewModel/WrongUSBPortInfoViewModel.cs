using EndlessLauncher.logger;
using EndlessLauncher.service;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;

namespace EndlessLauncher.ViewModel
{
    public class WrongUSBPortInfoViewModel : ViewModelBase
    {
        public RelayCommand backRelayCommand;
        private IFrameNavigationService navigationService;

        public WrongUSBPortInfoViewModel(IFrameNavigationService frameNavigationService)
        {
            this.navigationService = frameNavigationService;
        }

        public RelayCommand BackRelayCommand
        {
            get
            {
                return backRelayCommand
                    ?? (backRelayCommand = new RelayCommand(
                    () =>
                    {
                        LogHelper.Log("BackRelayCommand: ");
                        navigationService.GoBack();
                    }));
            }
        }
    }
}
