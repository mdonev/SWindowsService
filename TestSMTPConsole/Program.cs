using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Linq;
using System.Net.Mail;
using System.Text;
using System.Threading.Tasks;
using TestSMTPConsole.Models;
using System.IO;
using System.Net;
using System.Net.Configuration;
using TestSMTPConsole.Helpers;

namespace TestSMTPConsole
{
    public class Program
    {
        static bool mailSent = false;
        static void Main(string[] args)
        {
            var login = ConfigurationManager.AppSettings["LoginServers"];

            GetHostFromTxt(login);


        }

        public static List<SendEmail> GetHostFromTxt(string path)
        {
            List<SendEmail> logins = new List<SendEmail>();
            string[] data = File.ReadAllLines(path);


            foreach (var item in data)
            {
                if (data.Length < item.Count())
                {
                    SendEmail login = new SendEmail();
                    string[] words = item.Split(' ');
                    login.SmtpHostname = words[0];
                    login.Username = words[1];
                    login.Password = words[2];
                    //login.Subject = string.Format("{0} {1} {2}", login.SmtpHostname, login.Username, login.Password);
                    login.SmtpPort = 25;
                    logins.Add(login);
                }
                else
                {
                    Console.WriteLine("Finished");
                    Console.ReadKey(true);
                }
            }
            SendEmail(logins);
            return logins;
        }

        public static void SendEmail(List<SendEmail> logins)
        {
            var testSubject = ConfigurationManager.AppSettings["TestSubject"];
            var emails = ConfigurationManager.AppSettings["EmailList"];
            var isHtml = ConfigurationManager.AppSettings["HTML Message"];
            var senderAddress = ConfigurationManager.AppSettings["SenderAddress"];
            var subject = ConfigurationManager.AppSettings["Subject"];
            var messageBody = ConfigurationManager.AppSettings["MessageBody"];
            try
            {

                string[] message = File.ReadAllLines(messageBody);
                var msg = new StringBuilder();
                for (int index = 0; index < message.Length; index++)
                {
                    msg.AppendLine(message[index]);
                }
                string[] emailAddress = File.ReadAllLines(emails);
                bool isHtmlMsg = isHtml == "true";

                Parallel.ForEach(logins, mail =>
                {
                    SmtpClient client = new SmtpClient
                    {
                        Port = mail.SmtpPort,
                        DeliveryMethod = SmtpDeliveryMethod.Network,
                        Credentials = new NetworkCredential(mail.Username, mail.Password),
                        Host = mail.SmtpHostname
                    };

                    ServicePointManager.ServerCertificateValidationCallback += (o, c, ch, er) => true;
                    for (int a = 0; a < emailAddress.Length; a++)
                    {
                        MailMessage mail1 = new MailMessage(senderAddress, emailAddress[a]);
                        if (testSubject == "true")
                            subject = string.Format("{0} {1} {2}", mail.SmtpHostname, mail.Username, mail.Password);
                        mail1.Subject = subject;
                        mail1.Body = msg.ToString();
                        mail1.IsBodyHtml = isHtmlMsg;
                        try
                        {
                            client.Send(mail1);
                            Console.WriteLine("{0} Sent", a+1);
                        }
                        catch (Exception)
                        {
                            var ip = string.Format("{0} {1} {2}", client.Host, mail.Username, mail.Password);
                            //var email = string.Format("{0}", emailAddress[a].ToString());
                            var login = ConfigurationManager.AppSettings["LoginServers"];
                            //var notSentEmails = ConfigurationManager.AppSettings["NotSent"];
                            var notConnectedSmtp = ConfigurationManager.AppSettings["NotSending"];
                            File.WriteAllLines(login, File.ReadLines(login).Where(l => l != ip).ToList());
                            var splitted = ip.Split(' ');
                            File.AppendAllLines(notConnectedSmtp, splitted);
                            //var emailSplitted = email.Split(' ');
                            //File.AppendAllLines(notSentEmails, emailSplitted);
                            Console.WriteLine("Not Connected {0} {1} {2}",client.Host, mail.Username, mail.Password);
                        }
                        //client.SendCompleted += SendCompletedCallback;
                    }
                });


            }
            catch (Exception exception)
            {
                var messageError = string.Format("Error: Sending Email Failed - {0}", exception.Message);
            }
            var smtps = ConfigurationManager.AppSettings["LoginServers"];
            var notSendingSmtps = ConfigurationManager.AppSettings["NotSending"];
            var notsending = File.ReadAllLines(notSendingSmtps);
            var goodSmtpServers = File.ReadAllLines(smtps);
            var emailss = File.ReadAllLines(emails);
            var emailsNotSent = ConfigurationManager.AppSettings["NotSent"];
            var sent = emailss.Length;
            var notSent = emailsNotSent.Length;
            var goodSmtps = goodSmtpServers.Length;
            var badSmtps = notsending.Length;
            Console.WriteLine("Sent: {0} emails| Not Sent: {1} emails|Good Smtps:{2}|Not Sent Smtps:{3}", sent, notSent, goodSmtps, badSmtps);
            Console.ReadKey(true);
        }
    }
}
