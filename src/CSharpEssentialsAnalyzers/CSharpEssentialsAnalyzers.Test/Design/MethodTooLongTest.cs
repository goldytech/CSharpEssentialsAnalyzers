namespace CSharpEssentialsAnalyzers.Test.Design
{
    using CSharpEssentialsAnalyzers.Design;
    using CustomVerifiers;

    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CSharp.Syntax;
    using Microsoft.CodeAnalysis.Diagnostics;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public class MethodTooLongTest : CustomCodeFixVerifier
    {
        protected override string GetDiagnosticId()
        {
            return MethodTooLongAnalyzer.DiagnosticId;
        }

        protected override DiagnosticSeverity GetSeverity()
        {
            return DiagnosticSeverity.Warning;
        }

        protected override bool TokenShouldTrigger(SyntaxToken token)
        {
            var methodDeclarationSyntax = token.Parent as MethodDeclarationSyntax;

            if (methodDeclarationSyntax != null)
            {
                var location = methodDeclarationSyntax.GetLocation();
                var startline = location.GetLineSpan().StartLinePosition.Line;
                var endline = location.GetLineSpan().EndLinePosition.Line;
                if (endline - startline > 25)
                {
                    return true;
                }
                return false;
            }

            //return methodDeclarationSyntax != null
            //      && (methodDeclarationSyntax.Body.Statements.Count > 25);
            return false;
        }

        protected override string CreateMessage(SyntaxToken token)
        {
            return string.Format(MethodTooLongAnalyzer.MessageFormat.ToString());
        }

        protected override DiagnosticAnalyzer GetCSharpDiagnosticAnalyzer()
       => new MethodTooLongAnalyzer();

        [TestMethod]
        public void Method_With_More_Than_25_Lines_Should_Trigger_Diagnostic()
        {
            VerifyDiagnosticsForFile("MethodTooLongClass.csx", "MethodTooLong");
        }
    }
}