using System;
using System.Collections.Generic;
using Roslyn.Compilers.CSharp;
using Roslyn.Scripting.CSharp;
using Roslyn.Scripting;

namespace RoslynIrcBot
{
    public class ScriptExecuter : MarshalByRefObject
    {
        static readonly ScriptExecuter Host = new ScriptExecuter();

        public string Execute(string code)
        {
            var engine = new ScriptEngine();
            
            var session = Session.Create(Host);

            try
            {
                if (!Validate(code)) return "Not implemeted";

                var result = engine.Execute(code, session);
                if(result != null)
                    return result.ToString();
            }
            catch (Exception ex)
            {
                return ex.ToString();
            }

            return null;
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

                if (syntaxNode is InvocationExpressionSyntax && ((InvocationExpressionSyntax)syntaxNode).Expression is MemberAccessExpressionSyntax) return false;
            }

            return true;
        }
    }
}
