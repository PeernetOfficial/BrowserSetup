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
        private const string BatchFileName = "Firewall allow.cmd";

        public Installer()
            : base()
        {
            Committed += new InstallEventHandler(MyInstaller_Committed);
        }

        private static void CreateFile(string directory, string fileName, string command)
        {
            using (var streamWriter = File.CreateText($"{directory}/{fileName}"))
            {
                streamWriter.WriteLine(command);
            }
        }

        private void CreateBatchFile(string directory, string command)
        {
            CreateFile(directory, BatchFileName, command);
        }

        private void Execute(string processName)
        {
            var process = new Process();
            var processStartInfo = new ProcessStartInfo();
            processStartInfo.WindowStyle = ProcessWindowStyle.Normal;
            processStartInfo.FileName = processName;
            processStartInfo.Verb = "runas";
            processStartInfo.WorkingDirectory = GetTargetDirectory();
            process.StartInfo = processStartInfo;

            try
            {
                process.Start();
            }
            catch (Exception)
            {
                CreateFile(GetTargetDirectory(), "firewallnotset", string.Empty);
            }
        }

        private string GetTargetDirectory()
        {
            return Context.Parameters["targetdir"].Replace(@"\\", @"\");
        }

        private void MyInstaller_Committed(object sender, InstallEventArgs e)
        {
            string netshCommand = $"netsh advfirewall firewall add rule name=\"{FirewallRuleName}\" dir=in program=\"%~dp0\\Backend.exe\" profile=any action=allow";
            CreateBatchFile(GetTargetDirectory(), netshCommand);
            Execute(BatchFileName);
        }
    }
}