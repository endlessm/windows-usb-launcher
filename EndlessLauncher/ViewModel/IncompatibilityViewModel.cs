using EndlessLauncher.logger;
using EndlessLauncher.Resources;
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
        private int errorCode;

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
                        Utils.OpenUrl("https://support.hack-computer.com/hc/en-us", null);
                    }));
            }
        }

        public int ErrorCode
        {
            get
            {
                return errorCode;
            }
            set
            {
                Set(ref errorCode, value);
                RaisePropertyChanged("ErrorMessage");
            }
        }

        public string ErrorMessage
        {
            get
            {
                return string.Format("{0} {1}", Literals.error_code_message, errorCode.ToString("000"));
            }
        }
    }
}
