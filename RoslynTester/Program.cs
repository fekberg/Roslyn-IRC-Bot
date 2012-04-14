using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace RoslynTester
{
    static class Program
    {
        public static Process RoslynService;
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            //var task = Task.Factory.StartNew(() =>
            //{
            //    while (true)
            //    {
            //        RoslynService = new Process
            //        {
            //            StartInfo =
            //            {
            //                FileName = "Rossbot.Service.exe",
            //                UseShellExecute = false,
            //                WindowStyle = ProcessWindowStyle.Hidden
            //            }
            //        };

            //        RoslynService.Start();

            //        RoslynService.WaitForExit();
            //    }
            //});
            try
            {
                Application.EnableVisualStyles();
                Application.SetCompatibleTextRenderingDefault(false);
                Application.Run(new RoslynTester());
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
        }
    }
}
