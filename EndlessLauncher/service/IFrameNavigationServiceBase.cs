// © 2019–2020 Endless OS Foundation LLC
//
// This file is part of Endless Launcher.
//
// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at http://mozilla.org/MPL/2.0/.
using GalaSoft.MvvmLight.Views;

namespace EndlessLauncher.service
{
    public interface IFrameNavigationService : INavigationService
    {
        object Parameter { get; }
    }
}
