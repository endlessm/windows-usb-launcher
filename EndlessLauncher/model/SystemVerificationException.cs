namespace EndlessLauncher.model
{
    public class SystemVerificationException : EndlessExceptionBase<SystemVerificationErrorCode>
    {
        public SystemVerificationException(SystemVerificationErrorCode errorCode, string message) : base(errorCode, message)
        {
        }
    }
}
