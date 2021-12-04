using System;
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
            string netshCommandArguments = $"advfirewall firewall add rule name=\"{FirewallRuleName}\" dir=in program=\"%cd%\\Backend.exe\" profile=any action=allow";
            CreateBatchFile(GetTargetDirectory(), "netsh " + netshCommandArguments);
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
            try
            {
                process.Start();
            }
            catch (Exception e)
            {
                CreateFile(GetTargetDirectory(), "firewallnotset", e.Message + "\n" + e.StackTrace);
            }
        }

        private void CreateBatchFile(string directory, string command)
        {
            CreateFile(directory, "Firewall allow", command);
        }

        private void CreateFile(string directory, string fileName, string command)
        {
            using (var streamWriter = File.CreateText($"{directory}/{fileName}.cmd"))
            {
                streamWriter.WriteLine(command);
            }
        }

        private string GetTargetDirectory()
        {
            return Context.Parameters["targetdir"].Replace(@"\\", @"\");
        }
    }
}