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
                LogHelper.Log("SetDebugSimulatedError: " + SimulatedVerificationError);
            } else if (Enum.IsDefined(typeof(FirmwareSetupErrorCode), errorCode))
            {
                SimulatedFirmwareError = (FirmwareSetupErrorCode)errorCode;
                LogHelper.Log("SetDebugSimulatedError: " + SimulatedFirmwareError);
            }
        }
    }
}
