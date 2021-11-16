using System.Collections;
using System.ComponentModel;
using System.Configuration.Install;
using System.Diagnostics;

namespace Peernet.Browser.Setup.AddFirewallRule
{
    [RunInstaller(true)]
    public partial class Installer : System.Configuration.Install.Installer
    {
        private const string FirewallRuleName = "Peernet Cmd";

        public Installer()
            : base()
        {
            Committed += new InstallEventHandler(MyInstaller_Committed);
        }

        private void MyInstaller_Committed(object sender, InstallEventArgs e)
        {
            var targetDirectory = Context.Parameters["targetdir"];
            targetDirectory = targetDirectory.Replace(@"\\", @"\");
            string netshCommand = $"netsh advfirewall firewall add rule name=\"{FirewallRuleName}\" dir=in program=\"{targetDirectory}Backend.exe\" profile=any action=allow";
            ExecuteWithArgs("CMD.exe", "/c " + netshCommand);
        }

        private void ExecuteWithArgs(string processName, string args)
        {
            var process = new Process();
            var processStartInfo = new ProcessStartInfo();
            processStartInfo.WindowStyle = ProcessWindowStyle.Hidden;
            processStartInfo.FileName = processName;
            processStartInfo.Arguments = args;
            processStartInfo.Verb = "runas";

            process.StartInfo = processStartInfo;
            process.Start();
        }
    }
}