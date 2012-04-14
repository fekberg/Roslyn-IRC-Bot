using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security.Permissions;
using System.Security.Policy;
using System.Text;
using Roslyn.Compilers;
using Roslyn.Compilers.CSharp;
using System.Security;
using System.Threading;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Net;
using Roslyn.Scripting.CSharp;
using Rossbot.Data;
using System.Linq;

namespace Rossbot.Api
{
    public class ByteCodeLoader : MarshalByRefObject
    {
        public ByteCodeLoader()
        {
        }
        public object Run(byte[] compiledAssembly)
        {
            var assembly = Assembly.Load(compiledAssembly);
            assembly.EntryPoint.Invoke(null, new object[] { });
            var result = assembly.GetType("EntryPoint").GetProperty("Result").GetValue(null, null);

            return result;
        }
    }

    public class ScriptExecuter
    {
        private static string FormatResult(object input)
        {
            try
            {
                var formatter = new ObjectFormatter(maxLineLength: 350);
                var result = formatter.FormatObject(input);

                if (String.IsNullOrEmpty(result)) return "null";

                result = result.Replace(Environment.NewLine, " ").Replace("\n", " ").Replace("\r", " ");

                if (result.Length > 350) result = result.Substring(0, 350);

                return result;
            }
            catch (Exception ex)
            {
                return ex.ToString();
            }
        }

        private static AppDomain CreateSandbox()
        {
            Thread.CurrentThread.CurrentCulture = new CultureInfo("en");
            Thread.CurrentThread.CurrentUICulture = new CultureInfo("en");
            var e = new Evidence();
            e.AddHostEvidence(new Zone(SecurityZone.Internet));

            var ps = SecurityManager.GetStandardSandbox(e);
            var security = new SecurityPermission(SecurityPermissionFlag.Execution);

            ps.AddPermission(security);

            var setup = new AppDomainSetup { ApplicationBase = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) };
            return AppDomain.CreateDomain("Sandbox", null, setup, ps);
        }
        public object Execute(string code)
        {
            Thread.CurrentThread.CurrentCulture = new CultureInfo("en");
            Thread.CurrentThread.CurrentUICulture = new CultureInfo("en");
            // if (!Validate(code)) return "Not implemented";
            const string entryPoint = "using System; public class EntryPoint { public static object Result {get;set;} public static void Main() { try { Result = Script.Eval(); } catch(Exception ex) { Result = ex.ToString(); } } }"; // new StreamReader("EntryPoint.txt").ReadToEnd();
            var script = "public static object Eval() {" + code + "}";
            
            var core = Assembly.Load("System.Core, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089");
            var system = Assembly.Load("System, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089");

            string[] namespaces;
            using (var context = new DataContext())
            {
                var query = from ns in context.Namespaces
                            select ns.Namespace;
                namespaces = query.ToArray();
            }

            var compilation = Compilation.Create("foo",
                options: new CompilationOptions(assemblyKind: AssemblyKind.ConsoleApplication, usings: ReadOnlyArray<string>.CreateFrom(namespaces)),
                syntaxTrees: new[]
            {
                SyntaxTree.ParseCompilationUnit(entryPoint),
                SyntaxTree.ParseCompilationUnit(script, options: new ParseOptions(kind: SourceCodeKind.Interactive))
            },
            references: new MetadataReference[] { 
                    new AssemblyFileReference(typeof(object).Assembly.Location),
                    new AssemblyFileReference(core.Location), 
                    new AssemblyFileReference(system.Location)
                });

            byte[] compiledAssembly;
            using (var output = new MemoryStream())
            {
                EmitResult emitResult = compilation.Emit(output, null, null, null, xmlNameResolver: null);

                if (!emitResult.Success)
                {
                    var errors = emitResult.Diagnostics.Select(x => x.Info.GetMessage().Replace("Eval()", "<Factory>()").ToString()).ToArray();

                    return String.Join(", ", errors);
                }
                // report errors if emitResult.Diagnostics has any

                compiledAssembly = output.ToArray();
            }

            if (compiledAssembly.Length == 0) return "Incorrect data";

            AppDomain sandbox = CreateSandbox();
            var loader = (ByteCodeLoader)Activator.CreateInstance(sandbox, typeof(ByteCodeLoader).Assembly.FullName, typeof(ByteCodeLoader).FullName).Unwrap();
            object result = null;
            try
            {
                var scriptThread = new Thread(() =>
                {
                    Thread.CurrentThread.CurrentCulture = new CultureInfo("en");
                    Thread.CurrentThread.CurrentUICulture = new CultureInfo("en");
                    try
                    {
                        result = loader.Run(compiledAssembly);
                    }
                    catch(Exception ex)
                    {
                        result = ex.Message;
                    }
                });
                scriptThread.Start();
                if (!scriptThread.Join(6000))
                {
                    scriptThread.Abort();
                    AppDomain.Unload(sandbox);
                    return "Timeout";
                }
            }
            catch(Exception ex)
            {
                result = ex.InnerException != null ? ex.InnerException.ToString() : ex.ToString();
            }

            AppDomain.Unload(sandbox);

            if (result == null || String.IsNullOrEmpty(result.ToString())) result = "null";

            return FormatResult(result);
        }
        public bool Validate(string code)
        {
            var syntax = Syntax.ParseStatement(code);

            return ScanSyntax(syntax.ChildNodes());
        }

        public bool ScanSyntax(IEnumerable<SyntaxNode> syntaxNodes)
        {
            foreach (var syntaxNode in syntaxNodes)
            {
                if (syntaxNode.HasChildren)
                {
                    if (!ScanSyntax(syntaxNode.ChildNodes())) return false;
                }
                Debug.WriteLine(syntaxNode.GetType());
                // if (syntaxNode is QualifiedNameSyntax || syntaxNode is InvocationExpressionSyntax && ((InvocationExpressionSyntax)syntaxNode).Expression is MemberAccessExpressionSyntax) return false;
            }

            return true;
        }
    }
}
