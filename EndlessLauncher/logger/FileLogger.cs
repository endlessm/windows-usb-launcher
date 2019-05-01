using EndlessLauncher.utility;
using System;
using System.IO;
using System.Text;

namespace EndlessLauncher.logger
{
    class FileLogger : LoggerBase
    {
        private readonly string logFilePath = "";
        private StringBuilder delayedLog = new StringBuilder();

        public FileLogger(string logFilePath)
        {
            this.logFilePath = logFilePath;
        }

        private void WriteToFile(string message)
        {
            lock (lockObject)
            {
                try
                {
                    using (StreamWriter streamWriter = new StreamWriter(logFilePath, true))
                    {
                        streamWriter.Write(message);
                    }
                }
                catch (Exception) { }
            }
        }

        public override void Log(string message)
        {
            if (!Debug.ImmediateFileLogging)
            {
                delayedLog.Append(message).Append("\r\n");
            } else
            {
                WriteToFile(message + "\r\n");
            }
        }

        public override void Flush()
        {
            if (!Debug.ImmediateFileLogging)
            {
                WriteToFile(delayedLog.ToString());
                delayedLog.Clear();
            }
        }
    }
}
