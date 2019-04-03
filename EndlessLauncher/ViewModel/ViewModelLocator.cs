/*
  In App.xaml:
  <Application.Resources>
      <vm:ViewModelLocator xmlns:vm="clr-namespace:EndlessLauncher"
                           x:Key="Locator" />
  </Application.Resources>
  
  In the View:
  DataContext="{Binding Source={StaticResource Locator}, Path=ViewModelName}"

  You can also use Blend to do all this with the tool's support.
  See http://www.galasoft.ch/mvvm
*/

using CommonServiceLocator;
using EndlessLauncher.service;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Ioc;
using System;
using static EndlessLauncher.NativeAPI;

namespace EndlessLauncher.ViewModel
{
    /// <summary>
    /// This class contains static references to all the view models in the
    /// application and provides an entry point for the bindings.
    /// </summary>
    public class ViewModelLocator
    {
        /// <summary>
        /// Initializes a new instance of the ViewModelLocator class.
        /// </summary>
        public ViewModelLocator()
        {
            ServiceLocator.SetLocatorProvider(() => SimpleIoc.Default);

            SetupNavigation();

            #region Services
            //Register the system requirement verification service.
            SimpleIoc.Default.Register<SystemVerificationService, SystemVerificationService>();

            //Register the firmware service
            switch (SystemVerificationService.FirmwareType)
            {
                case FirmwareType.FirmwareTypeUefi:
                    SimpleIoc.Default.Register<FirmwareServiceBase, EFIFirmwareService>();
                    break;

                case FirmwareType.FirmwareTypeUnknown:
                case FirmwareType.FirmwareTypeMax:
                case FirmwareType.FirmwareTypeBios:
                    SimpleIoc.Default.Register<FirmwareServiceBase, LegacyFirmwareService>();
                    break;
            }
            #endregion

            #region ViewModels
            SimpleIoc.Default.Register<MainViewModel>();
            SimpleIoc.Default.Register<IncompatibilityViewModel>();
            #endregion
        }

        public MainViewModel MainViewModel
        {
            get
            {
                return ServiceLocator.Current.GetInstance<MainViewModel>();
            }
        }

        public IncompatibilityViewModel IncompatibilityViewModel
        {
            get
            {
                return ServiceLocator.Current.GetInstance<IncompatibilityViewModel>();
            }
        }

        public static void Cleanup()
        {
        }

        private static void SetupNavigation()
        {
            var navigationService = new FrameNavigationService();
            navigationService.Configure("WelcomePage", new Uri("../Views/WelcomePage.xaml", UriKind.Relative));
            navigationService.Configure("IncompatibilityPage", new Uri("../Views/IncompatibilityPage.xaml", UriKind.Relative));
            navigationService.Configure("WrongPortInfoPage", new Uri("../Views/WrongPortInfoPage.xaml", UriKind.Relative));
            navigationService.Configure("WrongUSBPortPage", new Uri("../Views/WrongUSBPortPage.xaml", UriKind.Relative));
            SimpleIoc.Default.Register<IFrameNavigationService>(() => navigationService);
        }


    }
}