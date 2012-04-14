using System;
using System.Diagnostics;
using System.Globalization;
using System.ServiceModel;
using System.Text;
using System.Threading;
using System.Reflection;
using Roslyn.Scripting.CSharp;
using Rossbot.Api;

namespace Rossbot.Windows.Service.Test
{
    [ServiceBehavior(InstanceContextMode = InstanceContextMode.Single)]
    class CommandService : ICommandService
    {

        public string Execute(string code)
        {
            EventLog.WriteEntry("RoslynCodeService", "Running code execution", EventLogEntryType.Information);
            var engine = new ScriptExecuter();
            var result = engine.Execute(code);

            EventLog.WriteEntry("RoslynCodeService", "Running code execution - Finished", EventLogEntryType.Information);
            return result.ToString();
        }

        //static string FormatResult(object input)
        //{
        //    try
        //    {
        //        var formatter = new ObjectFormatter();
        //        var result = formatter.FormatObject(input);

        //        if (string.IsNullOrEmpty(result)) return "null";

        //        result = result.Replace(Environment.NewLine, " ").Replace("\n", " ").Replace("\r", " ");

        //        if (result.Length > 350) result = result.Substring(0, 350);

        //        return result;
        //    }
        //    catch (Exception ex)
        //    {
        //        return ex.ToString();
        //    }
        //}
    }
}
