// © 2019–2020 Endless OS Foundation LLC
//
// This file is part of Endless Launcher.
//
// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at http://mozilla.org/MPL/2.0/.
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
                        navigationService.GoBack();
                    }));
            }
        }
    }
}
