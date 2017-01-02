using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SmtpWindowsService.Models;

namespace SmtpWindowsService.Interfaces
{
    public abstract class ISshAuth
    {
        public abstract void AuthAndStart(string hostnameOrIp, string username, string password, ScanIp item);
        public abstract void GetResults(string hostnameOrIp, string username, string password);
    }
}
