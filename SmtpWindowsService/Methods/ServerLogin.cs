using SmtpWindowsService.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using SmtpWindowsService.Models;
using System.Configuration;

namespace SmtpWindowsService.Methods
{
    public class ServerLogin : IServerLogin
    {
        ISshAuth sshAuth = new SshAuth();
        IEmailService _emailService = new EmailService();
        public void ScanForSmtp()
        {
            string path = ConfigurationManager.AppSettings["Servers"];
            var listOfServers = GetHostFromTxt(path);
            foreach (var server in listOfServers)
            {
                var hostnameOrIp = server.IP;
                var username = server.Username;
                var password = server.Password;
                sshAuth.GetResults(hostnameOrIp,username,password);
                List<ScanIp> scanList = new List<ScanIp>();
                var scanIp = GetIpForScan();
                scanList = scanIp;
                foreach (var item in scanList)
                {
                    sshAuth.AuthAndStart(hostnameOrIp, username, password, item);
                    SaveIP(item);
                }
            }
            _emailService.SendSmtpResults();
            var localFile = ConfigurationManager.AppSettings["vulnLocal"];
            string[] aa = DateTime.Now.ToString("HH:mm").Split(' ');
            File.WriteAllLines(localFile, aa);

        }

        public List<Login> GetHostFromTxt(string path)
        {
            List<Login> logins = new List<Login>();
            string[] data = File.ReadAllLines(path);
            foreach (var item in data)
            {
                Login login = new Login();
                string[] words = item.Split(' ');
                login.IP = words[0];
                login.Username = words[1];
                login.Password = words[2];
                logins.Add(login);
            }
            return logins;
        }

        public List<ScanIp> GetIpForScan()
        {
            var scanList = new List<ScanIp>();
            for (int i = 0; i < 4; i++)
            {
                ScanIp scan = new ScanIp();
                string lineFilePath = ConfigurationManager.AppSettings["LineCount"];
                var lines = File.ReadAllLines(lineFilePath);
                var line = lines.FirstOrDefault();

                string path = ConfigurationManager.AppSettings["IPs"];
                string data = File.ReadAllLines(path).ElementAtOrDefault(int.Parse(line));

                string[] words = data.Split(' ');
                scan.AIp = int.Parse(words[0]);
                scan.BIp = int.Parse(words[1]) + i;
                scan.LineCount = int.Parse(line);
                var ipLinesLength = File.ReadAllLines(path).Length;
                if (scan.BIp > 255)
                {
                    scan.LineCount += 1;
                    if (ipLinesLength <= scan.LineCount)
                    {
                        scan.LineCount = 0;
                        var data1 = SaveLineText(path, scan, lineFilePath);
                        string[] words1 = data1.Split(' ');
                        scan.AIp = int.Parse(words1[0]);
                        scan.BIp = 0;
                    }
                    else
                    {
                        var data2 = SaveLineText(path, scan, lineFilePath);
                        string[] words2 = data2.Split(' ');
                        scan.AIp = int.Parse(words2[0]);
                        scan.BIp = 0;
                    }
                }
                scanList.Add(scan);
            }
            return scanList;
        }

        private static string SaveLineText(string path, ScanIp scan, string lineFilePath)
        {
            string data1 = File.ReadAllLines(path).ElementAtOrDefault(scan.LineCount);
            var text = File.ReadAllLines(lineFilePath);
            text[0] = string.Format("{0}", scan.LineCount);
            File.WriteAllLines(lineFilePath, text);
            return data1;
        }

        private static void SaveIP(ScanIp item)
        {
            string path = ConfigurationManager.AppSettings["IPs"];
            string lineFilePath = ConfigurationManager.AppSettings["LineCount"];
            var lines = File.ReadAllLines(lineFilePath);
            var line = lines.FirstOrDefault();
            var text = File.ReadAllLines(path);
            text[int.Parse(line)] = string.Format("{0}" + " " + "{1}", item.AIp, item.BIp);
            File.WriteAllLines(path, text);
        }

    }
}
