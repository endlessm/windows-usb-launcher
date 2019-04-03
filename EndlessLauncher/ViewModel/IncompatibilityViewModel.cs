using EndlessLauncher.logger;
using EndlessLauncher.utility;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using System.IO;

namespace EndlessLauncher.ViewModel
{
    public class IncompatibilityViewModel : ViewModelBase
    {
        private RelayCommand showLogRelayCommand;
        private RelayCommand supportRelayCommand;

        public RelayCommand ShowLogRelayCommand
        {
            get
            {
                return showLogRelayCommand
                    ?? (showLogRelayCommand = new RelayCommand(
                    () =>
                    {
                        LogHelper.Log("ShowLogRelayCommand: {0}", LogHelper.LogFilePath);

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
                        LogHelper.Log("SupportRelayCommand:");
                        Utils.OpenUrl("https://support.endlessm.com/hc/en-us", null);
                    }));
            }
        }


    }
}
