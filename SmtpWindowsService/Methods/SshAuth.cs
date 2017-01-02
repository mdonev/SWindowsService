using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using NLog;
using NLog.Fluent;
using Renci.SshNet;
using SmtpWindowsService.Interfaces;
using SmtpWindowsService.Models;

namespace SmtpWindowsService.Methods
{

    public class SshAuth : ISshAuth
    {
        private static object locker = new object();
        private static Logger logger = LogManager.GetCurrentClassLogger();
        IEmailService _emailService = new EmailService();
        public override void AuthAndStart(string hostnameOrIp, string username, string password, ScanIp item)
        {
            //SmtpScanCommands(hostnameOrIp, username, password, item);
        }

        private void SmtpScanCommands(string hostnameOrIp, string username, string password, ScanIp item)
        {
            string command1 = "cd" + " " + ConfigurationManager.AppSettings["remoteSmtpDirectory"];
            string command2 = string.Format("nohup ./start {0}.{1} >> /dev/null &", item.AIp, item.BIp);
            string command3 = "history -c";
            SshClient sshclient = new SshClient(hostnameOrIp, username, password);
            sshclient.Connect();



            var shell = sshclient.CreateShellStream("cmd.exe", 80, 24, 800, 600, 1024);
            var writer = new StreamWriter(shell);
            writer.AutoFlush = true;

            while (!shell.DataAvailable)
                Thread.Sleep(1000); //This wait period seems required
            writer.WriteLine(command1);
            while (!shell.DataAvailable)
                Thread.Sleep(1000); //This wait period seems required
            writer.WriteLine(command2);
            while (!shell.DataAvailable)
                Thread.Sleep(1000); //This wait period seems required
            writer.WriteLine(command3);
            while (!shell.DataAvailable)
                Thread.Sleep(1000); //This wait period seems required


            sshclient.Disconnect();
        }

        public override void GetResults(string hostnameOrIp, string username, string password)
        {
            string reader = "";
            string command1 = ConfigurationManager.AppSettings["remoteSmtpDirectory"];
            string command4 = "vuln.txt";
            var localFile = ConfigurationManager.AppSettings["vulnLocal"];

            var remoteFile = command1 + "/" + command4;
            var tempLocalFile = ConfigurationManager.AppSettings["localDestinatonForTempVuln"];

            using (var sftp = new SftpClient(hostnameOrIp, 22, username, password))
            {
                sftp.Connect();
                lock (locker)
                {
                    using (var file = File.OpenWrite(tempLocalFile))
                    {
                        sftp.DownloadFile(remoteFile, file);
                    }
                }

                sftp.Disconnect();
            }
            var res = File.ReadAllLines(tempLocalFile);
            File.AppendAllLines(localFile, res);
        }

    }
}
