using EndlessLauncher.model;
using System.Collections.Generic;
using System.Security.Principal;

namespace EndlessLauncher.service
{
    public class SystemVerificationService
    {
        private Dictionary<string, Requirement> requirements = new Dictionary<string, Requirement>()
        {
            { "AdministratorPriveleges", new Requirement()}
        };

        public Dictionary<string, Requirement> Verify()
        {
            bool runningAsAdmin = RunningAsAdministrator();

            requirements["AdministratorPriveleges"].Resolve(
                runningAsAdmin, 
                "App must run with administrator priveleges");
            

            return requirements;
        } 

        private bool RunningAsAdministrator()
        {
            WindowsIdentity windowsIdentity = WindowsIdentity.GetCurrent();
            WindowsPrincipal windowsPrincipal = new WindowsPrincipal(windowsIdentity);

            return windowsPrincipal.IsInRole(WindowsBuiltInRole.Administrator);
        }


    }
}
