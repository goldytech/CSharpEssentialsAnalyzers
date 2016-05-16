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
            return MethodTooBigAnalyzer.DiagnosticId;
        }

        protected override DiagnosticSeverity GetSeverity()
        {
            return DiagnosticSeverity.Warning;
        }

        protected override bool TokenShouldTrigger(SyntaxToken token)
        {
            var methodDeclarationSyntax = token.Parent as MethodDeclarationSyntax;
            return methodDeclarationSyntax != null && token.LeadingTrivia.Equals(token.TrailingTrivia)
              && (methodDeclarationSyntax.Body.Statements.Count > MethodTooBigAnalyzer.MaximumLinesOfCode);

        }

        protected override string CreateMessage(SyntaxToken token)
        {
            return string.Format(MethodTooBigAnalyzer.MessageFormat.ToString());
        }

        protected override DiagnosticAnalyzer GetCSharpDiagnosticAnalyzer()
       => new MethodTooBigAnalyzer();

        [TestMethod]
        public void Method_With_More_Than_25_Lines_Should_Trigger_Diagnostic()
        {
            VerifyDiagnosticsForFile("MethodTooLongClass.csx", "MethodTooLong");
        }
    }
}