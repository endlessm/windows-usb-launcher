using EndlessLauncher.logger;
using EndlessLauncher.utility;
using System;
using System.Threading;
using System.Windows;
using static EndlessLauncher.NativeMethods;

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

            if (e.Args != null && e.Args.Length > 0)
            {
                for (int i = 0; i < e.Args.Length; i += 1)
                {
                    string arg = e.Args[i].ToLower();
                    if ( i + 1 < e.Args.Length && (arg.Equals("-e") || arg.Equals("--errorcode")))
                        errorCode = e.Args[i + 1];
                    if (arg.Equals("--fullLog") || arg.Equals("-fl"))
                    {
                        Debug.ImmediateFileLogging = true;
                    }
                }

                LogHelper.Log("OnStartup:Simulate Error:{0}", errorCode);
                Debug.SetDebugSimulatedError(errorCode);
            }

            mutex = new Mutex(true, "{5E80890D-A4E6-4B8C-B123-FFD89450547F}", out bool isNew);
            if (!isNew)
            {
                ActivateOtherWindow();
                Shutdown();
            }
        }

        private static void ActivateOtherWindow()
        {
            var other = FindWindow(null, "MainWindow");
            if (other != IntPtr.Zero)
            {
                SetForegroundWindow(other);
                if (IsIconic(other))
                {
                    OpenIcon(other);
                }
            }
        }
    }
}
