namespace CSharpEssentialsAnalyzers.Test.Design
{
    using System;
    using System.IO;

    using CSharpEssentialsAnalyzers.Design;
    using CSharpEssentialsAnalyzers.Test.CustomVerifiers;

    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CodeFixes;
    using Microsoft.CodeAnalysis.CSharp;
    using Microsoft.CodeAnalysis.Diagnostics;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    using CSharpExtensions = Microsoft.CodeAnalysis.CSharp.CSharpExtensions;

    [TestClass]
    public class EmptyCatchClauseTest   :CustomCodeFixVerifier
    {
        protected override string GetDiagnosticId()
        {
            return EmptyCatchClauseAnalyzer.DiagnosticId;
        }

        protected override DiagnosticAnalyzer GetCSharpDiagnosticAnalyzer()
        => new EmptyCatchClauseAnalyzer();
        protected override DiagnosticSeverity GetSeverity() => DiagnosticSeverity.Warning;

        protected override bool TokenShouldTrigger(SyntaxToken token)
        {
            return token.Kind() == SyntaxKind.CatchKeyword;
        }

        protected override string CreateMessage(SyntaxToken token)
        {
            return string.Format(EmptyCatchClauseAnalyzer.MessageFormat.ToString());
        }

        protected override CodeFixProvider GetCSharpCodeFixProvider()
        => new EmptyCatchClauseCodeFix();

        [TestMethod]
        public void EmptyCatchClause_Empty_string_should_not_give_diagnostic()
        {
            VerifyCSharpDiagnostic("");
        }

        [TestMethod]
        public void EmptyCatchClause_NonTriggering_code_should_not_give_diagnostic()
        {
            //NonEmptyCatchClauseInAwait
            VerifyNoDiagnosticForFile("NonEmptyCatchClauseInAwait.csx");
        }
         [TestMethod]
        public void EmptyCatchClause_Triggering_code_should_give_diagnostics()
        {
            this.VerifyDiagnosticsForFile("EmptyCatchClauseInAwait.csx","EmptyCatchClause");
            
        }

        [TestMethod]
        public void EmptyCatchClause_Triggering_code_for_specified_files_should_give_code_fix()
        {
            var fileBeforeFix = Path.Combine(System.AppDomain.CurrentDomain.BaseDirectory, "TestData", "EmptyCatchClause", "Triggering", "EmptyCatchClauseInAwait.csx");
            var fileAfterFix = Path.Combine(System.AppDomain.CurrentDomain.BaseDirectory, "TestData", "EmptyCatchClause", "AfterFix", "RemoveTryCatchEmptyCatchClauseInAwait.csx");
            VerifyCodeFixForFile(fileBeforeFix,fileAfterFix,0,false);
        }
    }
}