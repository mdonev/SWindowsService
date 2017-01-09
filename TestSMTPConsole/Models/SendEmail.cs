using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TestSMTPConsole.Models
{
    public class SendEmail
    {
        public string FromEmail { get; set; }
        public string ToEmail { get; set; }
        public string SmtpHostname { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
        public string Subject { get; set; }
        public string Message { get; set; }
        public int SmtpPort { get; set; }
    }
}
