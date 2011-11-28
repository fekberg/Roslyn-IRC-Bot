using System;
using System.IO;
using System.Net.Sockets;
using System.Reflection;
using System.Security;
using System.Security.Policy;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Linq;
namespace RoslynIrcBot
{
    public class Bot
    {
        public static StreamWriter WriteLine;
        private TcpClient _listener;
        private NetworkStream _netStream;

        private string _readLine;
        private StreamReader _reader;
        public Bot()
        {
            Start();
        }
        public static void Add(string logMessage)
        {
            Console.WriteLine(logMessage);
            //TextWriter writer = new StreamWriter("W:\\Bot.log", true);

            //var write = string.Format("{0}\r\n\r\n", logMessage);

            //writer.Write(write);

            //writer.Close();
        }

        public void End()
        {
            _reader.Close();
            WriteLine.Close();
            _listener.Close();
        }

        public void Start()
        {
            try
            {
                Add("Inside Startup");

                _listener = new TcpClient(BotSettings.Default.Server, BotSettings.Default.Port);
                _netStream = _listener.GetStream();
                _reader = new StreamReader(_netStream, Encoding.GetEncoding(1252));
                WriteLine = new StreamWriter(_netStream, Encoding.GetEncoding(1252));

                var ping = new Pinger();
                ping.Start();

                Write(BotSettings.Default.User);

                Add(_reader.ReadLine());

                Write("NICK " + BotSettings.Default.Name);

                Add(_reader.ReadLine());

                Thread.Sleep(2500);

                foreach (var channel in BotSettings.Default.StartUpChannels)
                {
                    Write("JOIN " + channel);

                    Add(_reader.ReadLine());
                }

                Add("Start Listen");
                Listen();
            }
            catch (SocketException ex)
            {
            }
        }

        public static void Write(string Text)
        {
            WriteLine.WriteLine(Text);
            WriteLine.Flush();

            Add("Sending: " + Text);
        }

        private void Response(string message)
        {
            var channel = _readLine.Split(' ')[2];

            if (channel == BotSettings.Default.Name)
            {
                channel = _readLine.Split('!')[0];
                channel = channel.Remove(0, 1);
            }

            Write("PRIVMSG " + channel + " :" + message);
        }

        private ScriptExecuter _executer = new ScriptExecuter();
        private void RunInSandbox(string code)
        {
            Evidence ev = new Evidence();
            ev.AddHostEvidence(new Zone(SecurityZone.Internet));
            PermissionSet pset = SecurityManager.GetStandardSandbox(ev);

            AppDomainSetup ads = new AppDomainSetup();
            ads.ApplicationBase = @"W:\Filip\Work\Programming\Projects\Avoid-The-Monsters\RoslynIrcBot\RoslynIrcBot\bin\Debug";

            // Create the sandboxed domain.
            AppDomain sandbox = AppDomain.CreateDomain(
                "Sandboxed Domain", ev, ads, pset);
               //ev,
               //ads,
               //pset,
               //null);
            var remoteWorker = (ScriptExecuter)sandbox.CreateInstanceAndUnwrap(
            "RoslynIrcBot", "RoslynIrcBot.ScriptExecuter");
            try
            {
                var result = remoteWorker.Execute(code);
                if (result != null)
                    Response(result.Replace(System.Environment.NewLine, " ").Replace("\n", " ").Replace("\r", " "));
            }
            catch (Exception ex)
            {
                Response("not supported");
            }
        }
        private void Listen()
        {
            while (true)
            {
                while ((_readLine = _reader.ReadLine()) != null)
                {
                    _readLine = _readLine.Replace(System.Environment.NewLine, " ").Replace("\n", " ").Replace("\r", " ");
                    // -- Listen for commands
                    Add(_readLine);
                    var raw = _readLine.Split(' ');
                    raw = raw.Skip(3).ToArray();
                    var command = string.Join(" ", raw);
                    if (command.StartsWith(string.Format(":{0}", BotSettings.Default.Name)))
                    {
                        RunInSandbox(command.Substring(string.Format(":{0}", BotSettings.Default.Name).Length + 1));
                    }
                }

                _reader.Close();
                WriteLine.Close();
                _listener.Close();
            }
        }
    }
}