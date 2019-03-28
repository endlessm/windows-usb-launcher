namespace EndlessLauncher.logger
{
    public abstract class LoggerBase
    {
        protected object lockObject = new object();
        public abstract void Log(string message);
    }
}
