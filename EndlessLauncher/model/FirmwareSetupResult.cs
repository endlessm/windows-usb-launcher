namespace EndlessLauncher.model
{
    public class FirmwareSetupResult
    {
        public static FirmwareSetupResult CreateSuccess()
        {
            return new FirmwareSetupResult(true, null);
        }

        public static FirmwareSetupResult CreateFailed(FirmwareSetupException error)
        {
            return new FirmwareSetupResult(false, error);
        }

        public static FirmwareSetupResult CreateFailed(FirmwareSetupException.ErrorCode errorCode, string message)
        {
            return new FirmwareSetupResult(false, new FirmwareSetupException(errorCode, message));
        }

        private FirmwareSetupResult(bool success, FirmwareSetupException error)
        {
            Success = success;
            Error = error;
        }

        public FirmwareSetupException Error
        {
            get;
            private set;
        }

        public bool Success
        {
            get;
            private set;
        }
    }
}
