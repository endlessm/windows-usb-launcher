using System;
using System.Diagnostics;
using System.IO;

namespace EndlessLauncher.logger
{
    class FileLogger : LoggerBase
    {
        private readonly string logFilePath = "";
        public FileLogger(string logFilePath)
        {
            this.logFilePath = logFilePath;
        }

        public override void Log(string message)
        {
            lock (lockObject)
            {
                try
                {
                    using (StreamWriter streamWriter = new StreamWriter(logFilePath, true))
                    {
                        streamWriter.WriteLine(message);
                        streamWriter.Close();
                    }
                }
                catch (Exception) { }
            }
        }
    }
}
