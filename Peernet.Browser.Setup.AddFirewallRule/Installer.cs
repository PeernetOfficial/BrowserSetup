using System.ComponentModel;
using System.Configuration.Install;
using System.Diagnostics;
using System.IO;

namespace Peernet.Browser.Setup.AddFirewallRule
{
    [RunInstaller(true)]
    public partial class Installer : System.Configuration.Install.Installer
    {
        private const string FirewallRuleName = "Peernet Browser";

        public Installer()
            : base()
        {
            Committed += new InstallEventHandler(MyInstaller_Committed);
        }

        private void MyInstaller_Committed(object sender, InstallEventArgs e)
        {
            var targetDirectory = Context.Parameters["targetdir"];
            targetDirectory = targetDirectory.Replace(@"\\", @"\");
            string netshCommandArguments = $"advfirewall firewall add rule name=\"{FirewallRuleName}\" dir=in program=\"%cd%\\Backend.exe\" profile=any action=allow";
            CreateBatchFile(targetDirectory, "netsh " + netshCommandArguments);
            ExecuteWithArgs("netsh.exe", netshCommandArguments);
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

        private void CreateBatchFile(string directory, string command)
        {
            using (var streamWriter = File.CreateText($"{directory}/Firewall allow.cmd"))
            {
                streamWriter.WriteLine(command);
            }
        }
    }
}
