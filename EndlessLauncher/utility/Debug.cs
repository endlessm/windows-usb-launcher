// © 2019–2020 Endless OS Foundation LLC
//
// This file is part of Endless Launcher.
//
// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at http://mozilla.org/MPL/2.0/.
using EndlessLauncher.logger;
using EndlessLauncher.model;
using System;

namespace EndlessLauncher.utility
{
    public class Debug
    {
        public static FirmwareSetupErrorCode SimulatedFirmwareError { get; private set; } = FirmwareSetupErrorCode.NoError;
        public static SystemVerificationErrorCode SimulatedVerificationError { get; private set; } = SystemVerificationErrorCode.NoError;

        public static void SetDebugSimulatedError(string error)
        {
            if (int.TryParse(error, out int errorCode))
            {
                SetDebugSimulatedError(errorCode);
            }
            else if (Enum.TryParse(error, out FirmwareSetupErrorCode firmwareErrorCode))
            {
                SimulatedFirmwareError = firmwareErrorCode;
            }
            else if (Enum.TryParse(error, out SystemVerificationErrorCode verificationErrorCode))
            {
                SimulatedVerificationError = verificationErrorCode;
            }
        }

        private static void SetDebugSimulatedError(int errorCode)
        {
            if (Enum.IsDefined(typeof(SystemVerificationErrorCode), errorCode))
            {
                SimulatedVerificationError = (SystemVerificationErrorCode)errorCode;
            } else if (Enum.IsDefined(typeof(FirmwareSetupErrorCode), errorCode))
            {
                SimulatedFirmwareError = (FirmwareSetupErrorCode)errorCode;
            }
        }

        public static bool ImmediateFileLogging
        {
            get;
            set;
        }

    }
}
