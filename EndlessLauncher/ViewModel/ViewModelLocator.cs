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
            #endregion
        }

        public MainViewModel MainViewModel
        {
            get
            {
                return ServiceLocator.Current.GetInstance<MainViewModel>();
            }
        }
        
        public static void Cleanup()
        {
            // TODO Clear the ViewModels
        }


    }
}