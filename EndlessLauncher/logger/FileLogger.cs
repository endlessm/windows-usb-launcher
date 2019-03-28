using System;
using System.Diagnostics;
using System.IO;

namespace EndlessLauncher.logger
{
    class FileLogger : LoggerBase
    {
        private string logFilePath = "";
        public FileLogger()
        {
            try
            {
                string tempFolder = System.IO.Path.GetTempPath();
                logFilePath += tempFolder 
                    + "EndlessLauncher_" 
                    + DateTime.Now.ToString("dd_MM_yyyy_hh_mm") 
                    + ".log";

            }
            catch (Exception e)
            {
                Debug.WriteLine("Error: " + e.Message);
            }

            Debug.WriteLine(logFilePath);
            
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
