using System;
using System.Threading;
using System.Windows;
using static EndlessLauncher.NativeAPI;

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
                    OpenIcon(other);
            }
        }
    }
}
