using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.InteropServices;
using SmtpWindowsService.Interfaces;
using SmtpWindowsService.Methods;

namespace SmtpWindowsService
{
    public partial class SmtpService : ServiceBase
    {
        //private readonly IServerLogin _serverLogin;
        //public SmtpService(ServerLogin serverLogin) { _serverLogin = serverLogin; }

        IServerLogin _serverLogin = new ServerLogin();

        public SmtpService()
        {
            InitializeComponent();
            eventLog1 = new System.Diagnostics.EventLog();
            if (!System.Diagnostics.EventLog.SourceExists("MySource"))
            {
                System.Diagnostics.EventLog.CreateEventSource(
                    "MySource", "MyNewLog");
            }
            eventLog1.Source = "MySource";
            eventLog1.Log = "";
        }

        protected override void OnStart(string[] args)
        {
            ServiceStatus serviceStatus = new ServiceStatus();
            serviceStatus.dwCurrentState = ServiceState.SERVICE_START_PENDING;
            serviceStatus.dwWaitHint = 100000;
            serviceStatus.dwCurrentState = ServiceState.SERVICE_RUNNING;


            eventLog1.WriteEntry("Service started ..");
            SetUpTimer();
            
        }

        protected override void OnStop()
        {
            eventLog1.WriteEntry("Service stopped ..");
        }

        public void SetUpTimer()
        {
            var timer = new System.Timers.Timer {Interval = 60000};
            // 60 seconds
            timer.Elapsed += new System.Timers.ElapsedEventHandler(this.OnTimer);
            timer.Start();
        }

        public void OnTimer(object sender, System.Timers.ElapsedEventArgs args)
        {
            _serverLogin.ScanForSmtp();
            eventLog1.WriteEntry("Monitoring the System", EventLogEntryType.Information);
        }
        public enum ServiceState
        {
            SERVICE_STOPPED = 0x00000001,
            SERVICE_START_PENDING = 0x00000002,
            SERVICE_STOP_PENDING = 0x00000003,
            SERVICE_RUNNING = 0x00000004,
            SERVICE_CONTINUE_PENDING = 0x00000005,
            SERVICE_PAUSE_PENDING = 0x00000006,
            SERVICE_PAUSED = 0x00000007,
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct ServiceStatus
        {
            public long dwServiceType;
            public ServiceState dwCurrentState;
            public long dwControlsAccepted;
            public long dwWin32ExitCode;
            public long dwServiceSpecificExitCode;
            public long dwCheckPoint;
            public long dwWaitHint;
        };
    }
}
