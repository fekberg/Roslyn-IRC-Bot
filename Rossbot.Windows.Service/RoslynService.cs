using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Globalization;
using System.IO.Pipes;
using System.Linq;
using System.ServiceModel;
using System.ServiceProcess;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Roslyn.Scripting.CSharp;
using Rossbot.Api;

namespace Rossbot.Windows.Service
{
    public partial class RoslynService : ServiceBase
    {
        public RoslynService()
        {
            InitializeComponent();
        }

        protected override void OnStart(string[] args)
        {
            Thread.CurrentThread.CurrentCulture = new CultureInfo("en");
            Thread.CurrentThread.CurrentUICulture = new CultureInfo("en");
            try
            {
                if (!EventLog.SourceExists("RoslynCodeService"))
                    EventLog.CreateEventSource("RoslynCodeService", "Application");
                
                EventLog.WriteEntry("RoslynCodeService", "Starting Command Server", EventLogEntryType.Information);
                var thread = new Thread(() =>
                {
                    Thread.CurrentThread.CurrentCulture = new CultureInfo("en");
                    Thread.CurrentThread.CurrentUICulture = new CultureInfo("en");
                    EventLog.WriteEntry("RoslynCodeService", "Execution thread started", EventLogEntryType.Information);
                    CommandServer.Start();
                });

                thread.Start();
            }
            catch (Exception ex)
            {
                EventLog.WriteEntry("RoslynCodeService", ex.ToString(), EventLogEntryType.Error);
            }
        }

        protected override void OnStop()
        {
            CommandServer.Stop();
        }
    }
}
