namespace EndlessLauncher.model
{
    public class FirmwareSetupException : EndlessExceptionBase<FirmwareSetupErrorCode>
    {
        public FirmwareSetupException(FirmwareSetupErrorCode errorCode, string message) : base(errorCode, message)
        {
        }
    }
}
