using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net.Mail;
using System.Text;
using System.Threading.Tasks;
using SmtpWindowsService.Interfaces;
using System.IO;

namespace SmtpWindowsService.Methods
{
    public class EmailService : IEmailService
    {
        public void SendEmail(string message,string subject,string from,string to,string smtpServer,string username,string password,string port)
        {
            MailMessage mail = new MailMessage(from, to);
            SmtpClient client = new SmtpClient();
            client.EnableSsl = true;
            client.Port = 25;
            client.DeliveryMethod = SmtpDeliveryMethod.Network;
            client.Credentials = new System.Net.NetworkCredential(username, password);
            client.Host = smtpServer;
            mail.Subject = subject;
            mail.Body = message;
            client.Send(mail);
        }

        public void SendSmtpResults()
        {
            var vulnFile = ConfigurationManager.AppSettings["vulnLocal"];
            var res = File.ReadAllText(vulnFile);

            var message = res;
            var subject = "Results" + " - " + DateTime.Now;
            var from = ConfigurationManager.AppSettings["EmailFrom"];
            var to = ConfigurationManager.AppSettings["EmailTo"];
            var smtpServer = ConfigurationManager.AppSettings["SmtpServer"];
            var username = ConfigurationManager.AppSettings["SmtpUsername"];
            var password = ConfigurationManager.AppSettings["SmtpPassword"];
            var port = ConfigurationManager.AppSettings["SmtpPort"];

            SendEmail(message, subject,from,to,smtpServer,username,password,port);
        }

    }
}
