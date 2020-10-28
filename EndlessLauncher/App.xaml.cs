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
        private delegate System.Diagnostics.Process StartShortcut();

        private static Mutex mutex = null;

        protected override void OnStartup(StartupEventArgs e)
        {
            string errorCode = null;
            bool fullLogging = false;
            StartShortcut startShortcut = null;

            for (int i = 0; i < e.Args.Length; i += 1)
            {
                switch (e.Args[i].ToLower())
                {
                    case "-e":
                    case "--errorcode":
                        if (i + 1 < e.Args.Length)
                            errorCode = e.Args[i + 1];
                        break;
                    case "-fl":
                    case "--fulllog":
                        fullLogging = true;
                        break;
                    case "--kiwix":
                        startShortcut = LauncherShortcuts.OpenKiwix;
                        break;
                    case "--kolibri":
                        startShortcut = LauncherShortcuts.OpenKolibri;
                        break;
                    case "--readme":
                        startShortcut = LauncherShortcuts.OpenReadme;
                        break;
                }
            }

            Debug.ImmediateFileLogging = fullLogging;

            LogHelper.Log("AppVersion:{0}", Assembly.GetExecutingAssembly().GetName().Version);
            if (errorCode != null)
            {
                LogHelper.Log("OnStartup:Simulate Error:{0}", errorCode);
                Debug.SetDebugSimulatedError(errorCode);
            }

            if (startShortcut != null)
            {
                startShortcut();
                Shutdown();
            }
            else
            {
                mutex = new Mutex(true, "{5E80890D-A4E6-4B8C-B123-FFD89450547F}", out bool isNew);

                if (!isNew)
                {
                    Utils.ActivateWindow(null, "MainWindow");
                    Shutdown();
                }
            }
        }
    }
}
