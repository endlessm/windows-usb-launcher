// © 2019–2020 Endless OS Foundation LLC
//
// This file is part of Endless Launcher.
//
// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at http://mozilla.org/MPL/2.0/.
using EndlessLauncher.logger;
using EndlessLauncher.utility;
using System.Reflection;
using System.Threading;
using System.Windows;

namespace EndlessLauncher
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private static Mutex mutex = null;
        protected override void OnStartup(StartupEventArgs e)
        {
            string errorCode = null;
            bool fullLogging = false;

            if (e.Args != null && e.Args.Length > 0)
            {
                for (int i = 0; i < e.Args.Length; i += 1)
                {
                    string arg = e.Args[i].ToLower();
                    if ( i + 1 < e.Args.Length && (arg.Equals("-e") || arg.Equals("--errorcode")))
                        errorCode = e.Args[i + 1];
                    if (arg.Equals("--fulllog") || arg.Equals("-fl"))
                    {
                        fullLogging = true;
                    }
                }
            }

            Debug.ImmediateFileLogging = fullLogging;

            LogHelper.Log("AppVersion:{0}", Assembly.GetExecutingAssembly().GetName().Version);
            if (errorCode != null)
            {
                LogHelper.Log("OnStartup:Simulate Error:{0}", errorCode);
                Debug.SetDebugSimulatedError(errorCode);
            }

            mutex = new Mutex(true, "{5E80890D-A4E6-4B8C-B123-FFD89450547F}", out bool isNew);
            if (!isNew)
            {
                Utils.ActivateWindow(null, "MainWindow");
                Shutdown();
            }
        }
    }
}
