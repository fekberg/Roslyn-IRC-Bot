using System;
using System.Diagnostics;
using System.Globalization;
using System.ServiceModel;
using System.Threading;
using Roslyn.Scripting.CSharp;
using Rossbot.Api;

namespace Rossbot.Windows.Service
{
    [ServiceBehavior(InstanceContextMode = InstanceContextMode.Single)]
    internal class CommandService : ICommandService
    {

        public string Execute(string code)
        {
            Thread.CurrentThread.CurrentCulture = new CultureInfo("en");
            Thread.CurrentThread.CurrentUICulture = new CultureInfo("en");
            EventLog.WriteEntry("RoslynCodeService", "Initializing code execution", EventLogEntryType.Information);
            var engine = new ScriptExecuter();
            try
            {
                EventLog.WriteEntry("RoslynCodeService", "Running code execution", EventLogEntryType.Information);
                var unformatted = engine.Execute(code);

                EventLog.WriteEntry("RoslynCodeService", "Running code execution - Finished", EventLogEntryType.Information);

                return FormatResult(unformatted);
            }
            catch (Exception ex)
            {
                EventLog.WriteEntry("RoslynCodeService", ex.ToString(), EventLogEntryType.Error);    
            }

            return string.Empty;
        }

        private static string FormatResult(object input)
        {
            try
            {
                var formatter = new ObjectFormatter(maxLineLength: 350);
                var result = formatter.FormatObject(input);

                if (string.IsNullOrEmpty(result)) return "null";

                result = result.Replace(Environment.NewLine, " ").Replace("\n", " ").Replace("\r", " ");

                if (result.Length > 350) result = result.Substring(0, 350);

                return result;
            }
            catch (Exception ex)
            {
                return ex.ToString();
            }
        }
    }
}
