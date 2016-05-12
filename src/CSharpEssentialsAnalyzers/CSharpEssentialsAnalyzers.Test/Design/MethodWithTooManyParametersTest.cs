namespace CSharpEssentialsAnalyzers.Test.Design
{
    using System.IO;

    using CSharpEssentialsAnalyzers.Design;
    using CSharpEssentialsAnalyzers.Test.CustomVerifiers;

    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CodeFixes;
    using Microsoft.CodeAnalysis.CSharp;
    using Microsoft.CodeAnalysis.CSharp.Syntax;
    using Microsoft.CodeAnalysis.Diagnostics;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public class MethodWithTooManyParametersTest  : CustomCodeFixVerifier
    {
        protected override string GetDiagnosticId()
        {
            return MethodWithTooManyParametersAnalyzer.DiagnosticId;
        }

        protected override DiagnosticSeverity GetSeverity()
        {
            return DiagnosticSeverity.Warning;
        }

        protected override bool TokenShouldTrigger(SyntaxToken token)
        {
            var parameterListSyntax = token.Parent as ParameterListSyntax;
            return parameterListSyntax != null & !token.IsKind(SyntaxKind.IdentifierToken)
                   && parameterListSyntax.Parameters.Count >= 5;
        }

        protected override string CreateMessage(SyntaxToken token)
        {
            return string.Format(MethodWithTooManyParametersAnalyzer.MessageFormat.ToString());
        }

        protected override CodeFixProvider GetCSharpCodeFixProvider()
       => new MethodWithTooManyParametersCodeFix();
        protected override DiagnosticAnalyzer GetCSharpDiagnosticAnalyzer()
       => new MethodWithTooManyParametersAnalyzer();

        [TestMethod]
        public void Method_With_Five_Or_More_Parameters_Should_Trigger_Diagnostic()
        {
           VerifyDiagnosticsForFile("MethodWithMoreThanFiveParameters.csx", "MethodWithTooManyParameters");
        }

        [TestMethod]
        public void Method_With_Five_Or_More_Parameters_Should_Apply_RefactoringFix()
        {
            var fileBeforeFix = Path.Combine(System.AppDomain.CurrentDomain.BaseDirectory, "TestData", "MethodWithTooManyParameters", "Triggering", "MethodWithMoreThanFiveParameters.csx");
            var fileAfterFix = Path.Combine(System.AppDomain.CurrentDomain.BaseDirectory, "TestData", "MethodWithTooManyParameters", "AfterFix", "TodoRefactoringComment.csx");
            VerifyCodeFixForFile(fileBeforeFix, fileAfterFix, 0, false);
        }
    }
}