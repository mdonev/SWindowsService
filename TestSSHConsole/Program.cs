using System;
using System.Collections.Generic;
using System.IO;
using System.Configuration;
using System.Linq;
using System.Threading;
using TestSSHConsole.Models;
using Renci.SshNet;

namespace TestSSHConsole
{
    public class Program
    {
        private static object locker = new object();
        public static void Main(string[] args)
        {
            LoginSSH();
        }

        public static void LoginSSH()
        {
            string path = ConfigurationManager.AppSettings["Servers"];
            var listOfServers = GetHostFromTxt(path);
            for (int index = 0; index < listOfServers.Count; index++)
            {
                var server = listOfServers[index];
                var hostnameOrIp = server.IP;
                var username = server.Username;
                var password = server.Password;
                ShellStream shell;
                StreamWriter writer;
                var connectedClients = SshClientConnect(hostnameOrIp, username, password, out shell, out writer,
                    index);
            }
            //_emailService.SendSmtpResults();
            //var localFile = ConfigurationManager.AppSettings["vulnLocal"];
            //string[] aa = DateTime.Now.ToString("HH:mm").Split(' ');
            //File.WriteAllLines(localFile, aa);
        }
        public static List<Login> GetHostFromTxt(string path)
        {
            List<Login> logins = new List<Login>();
            string[] data = File.ReadAllLines(path);
            foreach (var item in data)
            {
                Login login = new Login();
                string[] words = item.Split(' ');
                login.IP = words[2];
                login.Username = words[1];
                login.Password = words[0];
                logins.Add(login);
            }
            return logins;
        }
        public static SshClient SshClientConnect(string hostnameOrIp, string username, string password, out ShellStream shell, out StreamWriter writer,int lineCount)
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
                if (sshclient.IsConnected)
                {
                    var command1 = "cp /proc/cpuinfo /tmp/file.txt";
                    var command2 = "uname -m > /tmp/a.txt";
                    ExecuteCommand(shell, command1, writer);
                    ExecuteCommand(shell, command2, writer);
                }
                HandleFileOutputs(hostnameOrIp, username, password,lineCount);
            }
            catch (Exception exception)
            {
                exception.Message.ToString();
                var messageError = string.Format("error connecting to host - {0} {1} {2}", hostnameOrIp, username,
                    password);
                //_emailService.LogNotConnected(messageError);
                shell = null;
                writer = null;
            }
            return sshclient;

        }
        public static void ExecuteCommand(ShellStream shell, string command, StreamWriter writer)
        {
            writer.WriteLine(command);
            while (!shell.DataAvailable)
                Thread.Sleep(1000); //This wait period seems required
        }

        public static void HandleFileOutputs(string hostnameOrIp,string username,string password,int lineCount)
        {
            var remoteFile = "/usr/lib/php";
            var remoteCpuInfoFile = "/tmp/file.txt";
            var remoteBitInfoFile = "/tmp/a.txt";
            using (var sftp = new SftpClient(hostnameOrIp, 22, username, password))
            {
                sftp.Connect();
                lock (locker)
                {
                    bool isPHP;
                    var tempPath = ConfigurationManager.AppSettings["TempCpuInfo"];
                    var tempUnameInfo = ConfigurationManager.AppSettings["TempUnameInfo"];
                    using (var file = File.OpenWrite(tempUnameInfo))
                    {
                        sftp.DownloadFile(remoteBitInfoFile, file);
                    }
                    var input = "i686";
                    var model = SearchLineContents(tempUnameInfo, input);
                    if (model == "i686")
                    {
                        isPHP = true;
                        using (var file = File.OpenWrite(tempPath))
                        {
                            sftp.DownloadFile(remoteCpuInfoFile, file);
                            sftp.Disconnect();
                        }
                    }
                    else
                    {
                        isPHP = false;
                        using (var file = File.OpenWrite(tempPath))
                        {
                            sftp.DownloadFile(remoteCpuInfoFile, file);
                            sftp.Disconnect();
                            
                        }
                    }
                    if (isPHP) { 
                        string path = ConfigurationManager.AppSettings["PHPServers"];
                        SaveToFile(hostnameOrIp, username, password, path, tempPath, lineCount);
                    }
                    else
                    {
                        string path = ConfigurationManager.AppSettings["cpuinfo"];
                        SaveToFile(hostnameOrIp, username, password, path, tempPath, lineCount);
                    }
                }

                sftp.Disconnect();
            }
        }
        private static void SaveToFile(string hostnameOrIp, string username, string password,string path,string tempPath, int lineCount)
        {

            string lineFilePath = ConfigurationManager.AppSettings["servers"];
            var lines = File.ReadAllLines(lineFilePath);

            string input = "model name";
            var model = SearchLineContents(tempPath, input);
            if (lines.Count() > lineCount) { 
                string[] aa = string.Format("{0}" + " " + "{1}" + " " + "{2}" + " " + "{3}", hostnameOrIp, username, password, model).Split(' ');
            File.AppendAllLines(path, aa);
            }
            else
            {
                Console.WriteLine("Finished");
            }
        }

        private static string SearchLineContents(string tempPath, string input)
        {
            FileStream inFile = new FileStream(tempPath, FileMode.Open, FileAccess.Read);
            StreamReader reader = new StreamReader(inFile);
            string record;
            record = reader.ReadLine();
            var model = String.Empty;
            while (record != null)
            {
                if (record.Contains(input))
                {
                    Console.WriteLine(record);
                    model = record;
                }
                record = reader.ReadLine();
            }
            return model;
        }
    }
}
