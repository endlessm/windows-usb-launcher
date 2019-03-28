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
            //if (!Debugger.IsAttached)
            //{
                loggerList.Add(new FileLogger());
            //}
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
    }
}
