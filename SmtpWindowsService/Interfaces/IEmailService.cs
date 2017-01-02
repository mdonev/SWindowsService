using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmtpWindowsService.Interfaces
{
    public interface IEmailService
    {
        void SendEmail(string message, string subject, string from, string to, string smtpServer, string username, string password, string port);
        void SendSmtpResults();
    }
}
