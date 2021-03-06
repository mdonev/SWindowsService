﻿using System;
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
        public void SendEmail(string message, string subject, string from, string to, string toAdd, string smtpServer, string username, string password, string port)
        {
            try
            {
                MailMessage mail = new MailMessage(from, to);
                mail.To.Add(toAdd);
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
            catch (Exception)
            {
                var messageError = string.Format("Error: Sending Email Failed");
                LogNotConnected(messageError);
            }
        }

        public void SendSmtpResults()
        {
            var vulnFile = ConfigurationManager.AppSettings["vulnLocal"];
            var res = File.ReadAllLines(vulnFile);
            StringBuilder builder = new StringBuilder();
            for (int i = 0; i < res.Length; i++)
            {
                builder.AppendLine(res[i]);
            }

            var message = builder.ToString();
            var subject = "Results" + " - " + DateTime.Now;
            var from = ConfigurationManager.AppSettings["EmailFrom"];
            var to = ConfigurationManager.AppSettings["EmailTo"];
            var toAdd = ConfigurationManager.AppSettings["EmailToAdd"];
            var smtpServer = ConfigurationManager.AppSettings["SmtpServer"];
            var username = ConfigurationManager.AppSettings["SmtpUsername"];
            var password = ConfigurationManager.AppSettings["SmtpPassword"];
            var port = ConfigurationManager.AppSettings["SmtpPort"];

            SendEmail(message, subject, from, to, toAdd, smtpServer, username, password, port);
        }

        public void LogNotConnected(string error)
        {

            var message = error;
            var subject = "error connecting to host" + " - " + DateTime.Now;
            var from = ConfigurationManager.AppSettings["EmailFrom"];
            var to = ConfigurationManager.AppSettings["EmailTo"];
            var toAdd = ConfigurationManager.AppSettings["EmailTo"];
            var smtpServer = ConfigurationManager.AppSettings["SmtpServer"];
            var username = ConfigurationManager.AppSettings["SmtpUsername"];
            var password = ConfigurationManager.AppSettings["SmtpPassword"];
            var port = ConfigurationManager.AppSettings["SmtpPort"];

            SendEmail(message, subject, from, to, toAdd, smtpServer, username, password, port);
        }

    }
}
