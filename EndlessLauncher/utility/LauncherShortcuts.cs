using System.Diagnostics;

namespace EndlessLauncher.utility
{
    class LauncherShortcuts
    {
        private static readonly string FIREWALL_KOLIBRI_RULE_NAME = "Kolibri";

        public static Process OpenKiwix()
        {
            var kiwixExePath = System.IO.Path.Combine(
                GetExecutableDirectory(),
                ".kiwix-windows",
                "kiwix-desktop.exe"
            );

            var encyclopediaZimPath = System.IO.Path.Combine(new string[] {
                GetExecutableDirectory(),
                ".kiwix", "flatpak",
                "com.endlessm.encyclopedia.en-openzim-subscription",
                "1.zim"
            });

            return Utils.OpenUrl(kiwixExePath, encyclopediaZimPath);
        }

        public static Process OpenKolibri()
        {
            if (Utils.ActivateWindow("wxWindowNR", "Kolibri"))
            {
                return null;
            }

            var kolibriExePath = System.IO.Path.Combine(
                GetExecutableDirectory(),
                ".kolibri-windows",
                "Kolibri.exe"
            );

            return Utils.OpenUrl(kolibriExePath, "");
        }

        public static Process OpenReadme()
        {
            var readmePath = System.IO.Path.Combine(
                GetExecutableDirectory(),
                "Endless Key Quick Start.pdf"
            );

            return Utils.OpenUrl(readmePath, "");
        }
        private static string GetExecutableDirectory()
        {
            return System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);
        }
    }
}
