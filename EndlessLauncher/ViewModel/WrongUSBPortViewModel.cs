using EndlessLauncher.service;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;

namespace EndlessLauncher.ViewModel
{
    public class WrongUSBPortViewModel : ViewModelBase
    {
        public RelayCommand moreInfoRelayCommand;
        private IFrameNavigationService navigationService;

        public WrongUSBPortViewModel(IFrameNavigationService frameNavigationService)
        {
            this.navigationService = frameNavigationService;
        }

        public RelayCommand MoreInfoRelayCommand
        {
            get
            {
                return moreInfoRelayCommand
                    ?? (moreInfoRelayCommand = new RelayCommand(
                    () =>
                    {
                        this.navigationService.NavigateTo("WrongUSBPortInfoPage");
                    }));
            }
        }
    }
}
