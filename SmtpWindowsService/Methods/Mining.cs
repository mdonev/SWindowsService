using System;
using System.Configuration;
using System.IO;
using System.Threading;
using Renci.SshNet;
using SmtpWindowsService.Interfaces;

namespace SmtpWindowsService.Methods
{
    public class Mining : IMining
    {
        IServerLogin _serverLogin = new ServerLogin();
        ISshAuth _sshAuth = new SshAuth();
        IEmailService _emailService = new EmailService();
        private static object locker = new object();

        public void CheckMiners()
        {
            var localPath = ConfigurationManager.AppSettings["miners"];
            var logins = _serverLogin.GetHostFromTxt(localPath);

            foreach (var login in logins)
            {
                HandleMiners(login.IP, login.Username, login.Password);
            }
        }

        public void HandleMiners(string hostnameOrIp, string username, string password)
        {
            var remoteDirectory = ConfigurationManager.AppSettings["remoteDefaultDirectory"];
            var remoteFile = string.Format("{0}/sandy/yam", remoteDirectory);
            var downloadHost = ConfigurationManager.AppSettings["downloadHost"];
            var appName = "MINER";
            try
            {
                using (var sftp = new SftpClient(hostnameOrIp, 22, username, password))
                {
                    sftp.Connect();
                    lock (locker)
                    {
                        if (sftp.Exists(remoteFile))
                        {
                            var command1 = string.Format("wget {0}/sc -P {1} && chmod +x {1}/sc && {1}/sc", downloadHost, remoteDirectory);
                            ShellStream shell;
                            StreamWriter writer;
                            var sshclient = _sshAuth.SshClientConnect(hostnameOrIp, username, password, out shell, out writer, appName);
                            _sshAuth.ExecuteCommand(shell, command1, writer);
                            Thread.Sleep(20000);
                            sshclient.Disconnect();
                        }
                        else
                        {
                            var command1 = string.Format("wget {0}/dl -P {1} && chmod +x {1}/dl && {1}/dl", downloadHost, remoteDirectory);
                            ShellStream shell;
                            StreamWriter writer;
                            var sshclient = _sshAuth.SshClientConnect(hostnameOrIp, username, password, out shell, out writer, appName);
                            _sshAuth.ExecuteCommand(shell, command1, writer);
                            Thread.Sleep(20000);
                            sshclient.Disconnect();
                        }
                    }

                    sftp.Disconnect();
                }
            }
            catch (Exception)
            {
                var messageError = string.Format("error executing scripts - {0} {1} {2}", hostnameOrIp,
                    username, password);
                _emailService.LogNotConnected(messageError);
            }
        }
    }
}
