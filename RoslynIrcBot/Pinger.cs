using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace RoslynIrcBot
{
    class Pinger
    {
        #region Member Variables
        // -- Private Members
        private static string PING = "PING :";
        private Thread m_pinger;
        #endregion

        public Pinger()
        {
            m_pinger = new Thread(new ThreadStart(this.Run));
        }

        public void Start()
        {
            m_pinger.Start();
        }

        private void Run()
        {
            while (true)
            {
                Bot.Write(PING + BotSettings.Default.Server);

                Thread.Sleep(15000);
            }
        }
    }
}
