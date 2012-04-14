using System;
using System.Globalization;
using System.Linq;
using System.ServiceModel;
using System.ServiceProcess;
using System.Threading;
using Meebey.SmartIrc4net;
using Rossbot.Data;
using System.Net;

namespace RoslynIrcBot
{
    public class Bot
    {
        private static readonly Uri ServiceUri = new Uri("net.pipe://localhost/Pipe");
        private const string PipeName = "RoslynCodeExecution";
        private static readonly EndpointAddress ServiceAddress = new EndpointAddress(string.Format(CultureInfo.InvariantCulture, "{0}/{1}", ServiceUri.OriginalString, PipeName));
        private static ICommandService _serviceProxy;
        static readonly IrcClient Client = new IrcClient();

        public Bot()
        {
            Client.SendDelay = 200;
            Client.AutoReconnect = true;
            Client.AutoRetry = true;
            Client.ActiveChannelSyncing = true;
            Client.OnQueryMessage += Client_OnQueryMessage;
            Client.OnChannelMessage += Client_OnQueryMessage;
            Client.OnJoin += Client_OnQueryMessage;
        }

        public void Start()
        {
            Client.Connect(BotSettings.Default.Server, BotSettings.Default.Port);
            Client.Login(BotSettings.Default.Name, "Roslyn IRC Bot by Filip Ekberg");

            foreach (var channel in BotSettings.Default.StartUpChannels)
                Client.RfcJoin(channel);

            Client.Listen();
            Client.Disconnect();

        }

        private static void StartCodeService()
        {
            var service = new ServiceController("RoslynCodeService");
            if (service.Status != ServiceControllerStatus.Running)
            {
                service.Start();

                service.WaitForStatus(ServiceControllerStatus.Running);
            }
            _serviceProxy = ChannelFactory<ICommandService>.CreateChannel(new NetNamedPipeBinding(), ServiceAddress);
        }

        private void RunInSandbox(string code, IrcMessageData data)
        {
            StartCodeService();

            var doRestart = false;
            var serviceResult = "timeout";
            var invocationThread = new Thread(() =>
            {
                Thread.CurrentThread.CurrentCulture = new CultureInfo("en");
                Thread.CurrentThread.CurrentUICulture = new CultureInfo("en");
                try
                {
                    serviceResult = _serviceProxy.Execute(code);
                }
                catch (EndpointNotFoundException ex)
                {
                    doRestart = true;
                }
                catch (Exception ex)
                {
                    _serviceProxy = null;
                }
            });

            invocationThread.Start();

            invocationThread.Join(6000);

            if (doRestart)
            {
                _serviceProxy = null;
                RunInSandbox(code, data);
            }
            else
            {
                if (string.IsNullOrEmpty(serviceResult)) serviceResult = "null";

                string receiver = data.Type == ReceiveType.ChannelMessage ? data.Channel : data.Nick;

                if (serviceResult.StartsWith("\"")) serviceResult = serviceResult.Substring(1, serviceResult.Length-1);
                if(serviceResult.EndsWith("\"")) serviceResult = serviceResult.Substring(0, serviceResult.Length-1);

                Client.SendMessage(SendType.Message, receiver, ((data.Type == ReceiveType.ChannelMessage) ? data.Nick + ": " : "") + serviceResult);

            }
        }
        void HandlePastedCode(IrcEventArgs e, DataContext context, string commandText)
        {
            var host = commandText.Split(' ')[0];
            var client = new WebClient();

            string pasteId = string.Empty;
            string toExecute = string.Empty;

            if (host.Contains("http://pastebin.com"))
            {
                pasteId = commandText.Split(' ')[0].Replace("http://pastebin.com/", "");
                toExecute = client.DownloadString(string.Format("http://pastebin.com/raw.php?i={0}", pasteId));
            }
            else if (host.Contains("http://pastie.org"))
            {
                pasteId = commandText.Split(' ')[0].Replace("http://pastie.org/", "");
                toExecute = client.DownloadString(string.Format("http://pastie.org/pastes/{0}/text", pasteId));
            }

            Execute(e, context, toExecute);

        }
        bool IsPasteUrl(string text)
        {
            // http://pastie.org/pastes/{0}/text
            // http://pastebin.com/raw.php?i={0}
            if (text.Contains("http://pastebin.com") /*|| text.Contains("http://pastie.org")*/)
                return true;

            return false;
        }
        void ProcessCommand(IrcEventArgs e, DataContext context, string commandText)
        {
            if (context.Users.Any(x => x.Ident == e.Data.Ident && x.UserLevel == -1)) return;

            if (IsPasteUrl(commandText.Split(' ')[0]))
            {
                HandlePastedCode(e, context, commandText);
            }
            else if (commandText.StartsWith(">>"))
            {
                QueueCommand(e, context, commandText);
            }
            else
            {
                Execute(e, context, commandText);
            }
        }
        void QueueCommand(IrcEventArgs e, DataContext context, string commandText)
        {
            commandText = commandText.Substring(2).Trim();
            var command = new Commands { Id = Guid.NewGuid(), Command = commandText.Trim(), Posted = DateTime.Now, Username = e.Data.Nick };

            context.Commands.AddObject(command);
            context.SaveChanges();
        }
        void Execute(IrcEventArgs e, DataContext context, string commandText)
        {
            if (context.Commands.Any(x => x.Username == e.Data.Nick))
            {
                var commands = context.Commands.Where(x => x.Username == e.Data.Nick).OrderBy(x => x.Posted).Select(x => x.Command).ToArray();
                var toExecute = string.Join(Environment.NewLine, commands);
                toExecute += commandText;
                RunInSandbox(toExecute, e.Data);

                foreach (var command in context.Commands.Where(x => x.Username == e.Data.Nick))
                    context.DeleteObject(command);

                context.SaveChanges();
            }
            else
            {
                RunInSandbox(commandText, e.Data);
            }
        }
        void Client_OnQueryMessage(object sender, IrcEventArgs e)
        {
            using (var context = new DataContext())
            {
                switch (e.Data.Type)
                {
                    case ReceiveType.Join:
                        Client.RfcNotice(e.Data.Nick, "Hi, I am a IRC bot written in C# that executes code with Roslyn.");
                        Client.RfcNotice(e.Data.Nick, "To try me out, send me a private message with the following: var hello = \"hello world\"; return hello;");
                        Client.RfcNotice(e.Data.Nick, "You can also execute multi-line commands by starting the line with \">>\" remember to have a return statement in your last command");
                        break;
                    case ReceiveType.ChannelMessage:
                        if ((e.Data.Type == ReceiveType.ChannelMessage && e.Data.Message.StartsWith(BotSettings.Default.Name) && e.Data.Message.Length > BotSettings.Default.Name.Length) || (e.Data.Message.StartsWith(">>") && e.Data.Message.Length > 4))
                        {
                            var queryCommand = e.Data.Message.Substring(string.Format(BotSettings.Default.Name).Length + 1).Trim();
                            ProcessCommand(e, context, queryCommand);

                        }
                        break;
                    case ReceiveType.QueryMessage:
                        var channelCommand = e.Data.Message;
                        ProcessCommand(e, context, channelCommand);
                        break;
                }
            }
        }
    }
}
