﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SmtpWindowsService.Models;

namespace SmtpWindowsService.Interfaces
{
    public interface IServerLogin
    {
        void ScanForSmtp();
        List<Login> GetHostFromTxt(string path);
    }
}
