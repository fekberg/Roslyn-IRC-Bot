﻿using System;
using System.ServiceModel;

namespace Rossbot.Windows.Service.Test
{
    static class CommandServer
    {

        private static readonly Uri ServiceUri = new Uri("net.pipe://localhost/Pipe");
        private const string PipeName = "RoslynCodeExecution";

        private static readonly CommandService Service = new CommandService();
        private static ServiceHost _host;

        public static void Start()
        {
            _host = new ServiceHost(Service, ServiceUri);
            _host.AddServiceEndpoint(typeof(ICommandService), new NetNamedPipeBinding(), PipeName);
            _host.Open();
        }

        public static void Stop()
        {
            if ((_host == null) || (_host.State == CommunicationState.Closed)) return;

            _host.Close();
            _host = null;
        }
    }
}
