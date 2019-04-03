using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace EndlessLauncher.logger
{
    public static class LogHelper
    {
        private static List<LoggerBase> loggerList = new List<LoggerBase>();

        static LogHelper()
        {
            try
            {
                string logFolder = AppDomain.CurrentDomain.BaseDirectory;
                LogFilePath += logFolder
                    + "EndlessLauncher_"
                    + DateTime.Now.ToString("dd_MM_yyyy_hh_mm")
                    + ".log";
            }
            catch (Exception e)
            {
                Debug.WriteLine("Error: " + e.Message);
            }

            Debug.WriteLine(LogFilePath);

            if (!string.IsNullOrEmpty(LogFilePath))
            {
                loggerList.Add(new FileLogger(LogFilePath));
            }
        }

        public static string LogFilePath
        {
            get;
            private set;
        }

        private static string Format(ref string message)
        {
            message = DateTime.Now.ToString("hh:mm:ss.fff ") + message;
            return message;
        }

        public static void Log(string message)
        {
            Format(ref message);
            if (Debugger.IsAttached)
            {
                Debug.WriteLine(message);
            }

            foreach (var logger in loggerList)
            {
                logger.Log(message);
            }
        }

        public static void Log(string format, params object[] objects)
        {
            Log(string.Format(format, objects));
        }
    }
}
