//using System;
//using System.Collections.Generic;
//using System.Configuration;
//using System.Globalization;
//using System.IO;
//using System.IO.Pipes;
//using System.Net.Sockets;
//using System.Reflection;
//using System.Security;
//using System.Security.Permissions;
//using System.Security.Policy;
//using System.Security.Principal;
//using System.ServiceModel;
//using System.ServiceProcess;
//using System.Text;
//using System.Text.RegularExpressions;
//using System.Threading;
//using System.Linq;
//using Roslyn.Scripting;
//using Roslyn.Scripting.CSharp;
//using Rossbot.Api;
//using System.Collections;

//namespace RoslynIrcBot
//{
//    [SecuritySafeCritical]
//    public class Bot
//    {
//        public static StreamWriter WriteLine;
//        private TcpClient _listener;
//        private NetworkStream _netStream;

//        private string _readLine;
//        private StreamReader _reader;
//        public Bot()
//        {
//            Start();
//        }
//        public static void Add(string logMessage)
//        {
//            Console.WriteLine(logMessage);
//            //TextWriter writer = new StreamWriter("W:\\Bot.log", true);

//            //var write = string.Format("{0}\r\n\r\n", logMessage);

//            //writer.Write(write);

//            //writer.Close();
//        }

//        public void End()
//        {
//            _reader.Close();
//            WriteLine.Close();
//            _listener.Close();
//        }

//        public void Start()
//        {
//            try
//            {
//                Add("Inside Startup");

//                _listener = new TcpClient(BotSettings.Default.Server, BotSettings.Default.Port);
//                _netStream = _listener.GetStream();
//                _reader = new StreamReader(_netStream, Encoding.GetEncoding(1252));
//                WriteLine = new StreamWriter(_netStream, Encoding.GetEncoding(1252));

//                var ping = new Pinger();
//                ping.Start();

//                Write(BotSettings.Default.User);

//                Add(_reader.ReadLine());

//                Write("NICK " + BotSettings.Default.Name);

//                Add(_reader.ReadLine());

//                Thread.Sleep(2500);

//                foreach (var channel in BotSettings.Default.StartUpChannels)
//                {
//                    Write("JOIN " + channel);

//                    Add(_reader.ReadLine());
//                }

//                Add("Start Listen");
//                Listen();
//            }
//            catch (SocketException ex)
//            {
//            }
//        }

//        public static void Write(string Text)
//        {
//            WriteLine.WriteLine(Text);
//            WriteLine.Flush();

//            Add("Sending: " + Text);
//        }

//        private void Response(string message)
//        {
//            var channel = _readLine.Split(' ')[2];

//            if (channel == BotSettings.Default.Name)
//            {
//                channel = _readLine.Split('!')[0];
//                channel = channel.Remove(0, 1);
//            }

//            Write("PRIVMSG " + channel + " :" + message);
//        }

//        private static readonly Uri ServiceUri = new Uri("net.pipe://localhost/Pipe");
//        private const string PipeName = "RoslynCodeExecution";
//        private static readonly EndpointAddress ServiceAddress = new EndpointAddress(string.Format(CultureInfo.InvariantCulture, "{0}/{1}", ServiceUri.OriginalString, PipeName));
//        private static ICommandService _serviceProxy;

//        private static void StartCodeService()
//        {
//            var service = new ServiceController("RoslynCodeService");
//            if (service.Status != ServiceControllerStatus.Running)
//            {
//                service.Start();

//                service.WaitForStatus(ServiceControllerStatus.Running);
//            }
//            _serviceProxy = ChannelFactory<ICommandService>.CreateChannel(new NetNamedPipeBinding(), ServiceAddress);
//        }

//        private void RunInSandbox(string code)
//        {
//            StartCodeService();

//            var doRestart = false;
//            var serviceResult = "timeout";
//            var invocationThread = new Thread(() =>
//            {
//                Thread.CurrentThread.CurrentCulture = new CultureInfo("en");
//                Thread.CurrentThread.CurrentUICulture = new CultureInfo("en");
//                try
//                {
//                    serviceResult = _serviceProxy.Execute(code);
//                }
//                catch (EndpointNotFoundException ex)
//                {
//                    doRestart = true;
//                }
//                catch (Exception ex)
//                {
//                    _serviceProxy = null;
//                }
//            });

//            invocationThread.Start();

//            invocationThread.Join(6000);

//            if (doRestart)
//            {
//                _serviceProxy = null;
//                RunInSandbox(code);
//            }
//            else
//            {
//                if (string.IsNullOrEmpty(serviceResult)) serviceResult = "null";
//                Response(serviceResult);
//            }
//        }

//        private void Listen()
//        {
//            while (true)
//            {
//                while ((_readLine = _reader.ReadLine()) != null)
//                {
//                    _readLine = _readLine.Replace(System.Environment.NewLine, " ").Replace("\n", " ").Replace("\r", " ");
//                    // -- Listen for commands
//                    Add(_readLine);
//                    var raw = _readLine.Split(' ');
//                    raw = raw.Skip(3).ToArray();
//                    var command = string.Join(" ", raw);
//                    if (command.StartsWith(string.Format(":{0}", BotSettings.Default.Name)))
//                    {
//                        try
//                        {
//                            var commandText = command.Substring(string.Format(":{0}", BotSettings.Default.Name).Length + 1).Trim();
//                            if (commandText.ToLower().Contains("!raw") && _readLine.StartsWith(":frW!~frW@smartit.se"))
//                            {
//                                if (commandText.ToLower().Contains("nick"))
//                                {
//                                    BotSettings.Default.Name = commandText.Remove(0, 5).Remove(0, 5);
//                                    BotSettings.Default.Save();
//                                }
//                                Write(commandText.Remove(0, 5));
//                            }
//                            else
//                                RunInSandbox(commandText);
//                        }
//                        catch(Exception ex)
//                        {
//                            Response(ex);
//                        }
//                    }
//                }
//            }
//        }
//    }
//}