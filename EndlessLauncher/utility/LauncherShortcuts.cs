using System.Diagnostics;
using System.Linq;
using Microsoft.Win32;
using WindowsFirewallHelper;
using WindowsFirewallHelper.FirewallAPIv2.Rules;

namespace EndlessLauncher.utility
{
    class LauncherShortcuts
    {
        private static readonly string FIREWALL_KOLIBRI_RULE_NAME = "Kolibri";

        public static Process OpenKiwix()
        {
            var vcRuntimeInstallerPath = System.IO.Path.Combine(
                GetExecutableDirectory(),
                ".kiwix-windows",
                "vc_redist.x64.exe"
            );

            // TODO: Wait asynchronously while disabling the Kiwix button
            Utils.OpenUrl(vcRuntimeInstallerPath, "/install /quiet /norestart").WaitForExit();

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

            var existingFwRule = FirewallManager.Instance.Rules.SingleOrDefault(fwRule => {
                if (!(fwRule is StandardRuleWin8))
                {
                    return false;
                }

                var win8Rule = (StandardRuleWin8)fwRule;
                return win8Rule.Name == FIREWALL_KOLIBRI_RULE_NAME && win8Rule.ApplicationName == kolibriExePath;
            });

            if (existingFwRule == null)
            {
                var fwRule = FirewallManager.Instance.CreateApplicationRule(
                    FirewallManager.Instance.GetProfile().Type,
                    FIREWALL_KOLIBRI_RULE_NAME,
                    FirewallAction.Allow,
                    kolibriExePath
                );
                fwRule.Direction = FirewallDirection.Inbound;
                FirewallManager.Instance.Rules.Add(fwRule);
            }

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
