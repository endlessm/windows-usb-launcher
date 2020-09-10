// © 2019–2020 Endless OS Foundation LLC
//
// This file is part of Endless Launcher.
//
// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at http://mozilla.org/MPL/2.0/.
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
