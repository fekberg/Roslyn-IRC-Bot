using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Globalization;
using System.IO.Pipes;
using System.Linq;
using System.Security.Principal;
using System.ServiceModel;
using System.ServiceProcess;
using System.Text;
using System.Windows.Forms;
using Roslyn.Scripting.CSharp;
using Rossbot.Api;
using System.Threading.Tasks;
using System.Threading;

namespace RoslynTester
{
    public partial class RoslynTester : Form
    {
        public RoslynTester()
        {
            InitializeComponent();
        }

        private static readonly Uri ServiceUri = new Uri("net.pipe://localhost/Pipe");
        private const string PipeName = "RoslynCodeExecution";
        private static readonly EndpointAddress ServiceAddress = new EndpointAddress(string.Format(CultureInfo.InvariantCulture, "{0}/{1}", ServiceUri.OriginalString, PipeName));
        private static ICommandService ServiceProxy;

        private static void Start()
        {
            var service = new ServiceController("RoslynCodeService");
            if (service.Status != ServiceControllerStatus.Running)
            {
                service.Start();

                service.WaitForStatus(ServiceControllerStatus.Running);
            }
            ServiceProxy = ChannelFactory<ICommandService>.CreateChannel(new NetNamedPipeBinding(), ServiceAddress);
        }
        private void RunCodeClick(object sender, EventArgs e)
        {
            if (ServiceProxy == null) Start();

            var doRestart = false;
            var serviceResult = "timeout";
            var invocationThread = new Thread(() =>
            {
                try
                { 
                    serviceResult = ServiceProxy.Execute(input.Text);
                }
                catch (EndpointNotFoundException ex)
                {
                    doRestart = true;
                }
                catch (Exception ex)
                {
                    ServiceProxy = null;
                    serviceResult = ex.ToString();
                }
            });

            invocationThread.Start();

            invocationThread.Join(3000);

            if (doRestart)
            {
                ServiceProxy = null;
                RunCodeClick(null,null);
            }

            //var invocationThread = new Thread(() => { 
            //    var pipeClient = new NamedPipeClientStream(".", "RoslynExecutionService", PipeDirection.InOut, PipeOptions.None, TokenImpersonationLevel.Impersonation);

            //    pipeClient.Connect();

            //    foreach (var b in Encoding.Default.GetBytes(input.Text))
            //        pipeClient.WriteByte(b);
            //    pipeClient.Flush();

            //    pipeClient.WriteByte((byte)'\0');
            //    pipeClient.Flush();

            //    int data;
            //    var result = new List<byte>();
            //    while ((data = pipeClient.ReadByte()) > 0)
            //    {
            //        result.Add(Convert.ToByte(data));
            //    }

            //    serviceResult = Encoding.Default.GetString(result.ToArray());
            //});

            //invocationThread.Start();

            //invocationThread.Join(10000);

            output.Text = serviceResult;
        }
    }
}
