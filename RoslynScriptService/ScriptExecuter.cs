using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security.Permissions;
using System.Security.Policy;
using System.Threading.Tasks;
using Roslyn.Compilers;
using Roslyn.Compilers.CSharp;
using Roslyn.Scripting;
using Roslyn.Scripting.CSharp;
using System.Security;

namespace RoslynScriptService
{
public class ScriptExecuter
{
    public static ScriptEngine Engine { get; set; }
    public static Session Session { get; set; }
    private Assembly _dynamicAssembly;

    [SecuritySafeCritical]
    public string Execute(string code)
    {
        var task = Task<string>.Factory.StartNew(() =>
        {
            try
            {
                if (!Validate(code)) return "Not implemeted";

                var e = new Evidence();
                e.AddHostEvidence(new Zone(SecurityZone.Untrusted));

                var ps = SecurityManager.GetStandardSandbox(e);
                var security = new SecurityPermission(SecurityPermissionFlag.NoFlags);

                ps.AddPermission(security);

                var setup = new AppDomainSetup { ApplicationBase = Environment.CurrentDirectory };
                var domain = AppDomain.CreateDomain("Sandbox", e, setup, ps);
                AppDomain.CurrentDomain.AssemblyResolve += DomainAssemblyResolve;
                using (var memoryStream = new MemoryStream())
                {
                    var defaultReferences = new[] {"System"};
                    var defaultNamespaces = new[] { "System" };
                    CommonScriptEngine engine = new ScriptEngine(defaultReferences, defaultNamespaces);
                    var options = new ParseOptions(kind: SourceCodeKind.Script, languageVersion: LanguageVersion.CSharp6);

                    foreach (var ns in defaultNamespaces) engine.Execute(string.Format("using {0};", ns), Session);
                    byte[] assembly = null;
                   
                    var resultCode = engine.CompileSubmission<object>(code, Session);
                    resultCode.Compilation.Emit(memoryStream);
                    assembly = memoryStream.ToArray();
                    
                    //var compilationSubmission = Engine.CompileSubmission<dynamic>(code, Session);
                    //compilationSubmission.Compilation.AddReferences(new AssemblyNameReference("System.IO, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a"));
                    //compilationSubmission.Compilation.Emit(memoryStream);

                    domain.Load("mscorlib");
                    domain.Load("System, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089");
                    domain.Load("Microsoft.Csharp, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a");
                    domain.Load("Roslyn.Compilers, Version=1.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35");
                    domain.Load("Roslyn.Compilers.Csharp, Version=1.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35");

                    _dynamicAssembly = Assembly.Load(assembly);
                    
                    var loaded = domain.Load(assembly);

                    var submission = loaded.GetTypes().Where(x => x.Name.Contains("Submission")).Last();
                    var methods = submission.GetMethods();
                    var result = methods.Where(x => x.Name.Contains("Factory")).First().Invoke(submission, new[] { Session });

                    if (result != null)
                        return result.ToString();

                    AppDomain.Unload(domain);
                }
            }
            catch (Exception ex)
            {
                return ex.ToString();
            }

            return null;
        });
        var finished = task.Wait(10000);

        if (finished) 
            return task.Result;

        return "Timeout";
    }

    Assembly DomainAssemblyResolve(object sender, ResolveEventArgs args)
    {
        return _dynamicAssembly;
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
            Console.WriteLine(syntaxNode.GetType());
            // if (syntaxNode is QualifiedNameSyntax || syntaxNode is InvocationExpressionSyntax && ((InvocationExpressionSyntax)syntaxNode).Expression is MemberAccessExpressionSyntax) return false;
        }

        return true;
    }
}
}
