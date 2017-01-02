using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmtpWindowsService.Models
{
    public class Login
    {
        public string IP { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }

        public int CountTimes { get; set; }
    }
}
