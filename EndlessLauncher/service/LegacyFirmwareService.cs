// © 2019–2020 Endless OS Foundation LLC
//
// This file is part of Endless Launcher.
//
// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at http://mozilla.org/MPL/2.0/.
using EndlessLauncher.model;
using System;

namespace EndlessLauncher.service
{
    public class LegacyFirmwareService : FirmwareServiceBase
    {
        public LegacyFirmwareService(SystemVerificationService service) : base(service) { }

        protected override void SetupEndlessLaunch(string desscription, string path)
        {
            throw new FirmwareSetupException(FirmwareSetupErrorCode.FirmwareServiceInitializationError, "Unsupported firmware");
        }
    }
}
