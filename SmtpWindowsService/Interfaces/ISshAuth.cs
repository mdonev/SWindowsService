using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SmtpWindowsService.Models;
using Renci.SshNet;
using System.IO;

namespace SmtpWindowsService.Interfaces
{
    public abstract class ISshAuth
    {
        public abstract void AuthAndStart(string hostnameOrIp, string username, string password, ScanIp item);
        public abstract void GetResults(string hostnameOrIp, string username, string password);

        public abstract SshClient SshClientConnect(string hostnameOrIp, string username, string password, out ShellStream shell,
            out StreamWriter writer,string appName);

        public abstract void ExecuteCommand(ShellStream shell, string command, StreamWriter writer);
    }
}
