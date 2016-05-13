namespace CSharpEssentialsAnalyzers.Test.CustomVerifiers
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Resources;

    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CSharp;
    using Microsoft.CodeAnalysis.Formatting;

    using TestHelper;

    public abstract class CustomCodeFixVerifier : CodeFixVerifier
    {
        protected abstract string GetDiagnosticId();
        protected abstract DiagnosticSeverity GetSeverity();
        protected abstract bool TokenShouldTrigger(SyntaxToken token);
        protected abstract string CreateMessage(SyntaxToken token);
        public virtual int IndentSpaces { get; } = 0;

        public virtual Location GetSquiggleLocation(SyntaxToken token) => token.GetLocation();

        protected void VerifyNoDiagnosticForFile(string fileName)
        {

            string filePath = Path.Combine(System.AppDomain.CurrentDomain.BaseDirectory, "TestData", "EmptyCatchClause", "NonTriggering", fileName);

           
            var test = File.ReadAllText(filePath);

           this.VerifyCSharpDiagnostic(test);
        }

        protected void VerifyDiagnosticsForFile(string fileName,string analyzerFolder)
        {
            string filePath = Path.Combine(System.AppDomain.CurrentDomain.BaseDirectory,"TestData",analyzerFolder,"Triggering",fileName);

           
            var test = File.ReadAllText(filePath);
            var tree = SyntaxFactory.ParseCompilationUnit(test);
            var syntaxTokens = tree.DescendantTokens().Where(x => TokenShouldTrigger(x));
            var expected = GetExpectedResults(syntaxTokens);

            VerifyCSharpDiagnostic(test, expected.ToArray());
        }

        private IEnumerable<DiagnosticResult> GetExpectedResults(IEnumerable<SyntaxToken> syntaxTokens)
        {
            var expected = new List<DiagnosticResult>();
            foreach (var syntaxToken in syntaxTokens)
            {
                var location = GetSquiggleLocation(syntaxToken).GetLineSpan().StartLinePosition;

                var result = new DiagnosticResult
                {
                    Id = GetDiagnosticId(),
                    Message = CreateMessage(syntaxToken),
                    Severity = GetSeverity(),
                    Locations =
                        new[] {                            new DiagnosticResultLocation(
                       "Test0.cs",  location.Line + 1, location.Character+1)                        }
                };
                expected.Add(result);
            }
            return expected;
        }

        protected void VerifyCodeFixForFile(string beforeFullFileName, string afterFullFileName, int ordinal, bool multipleFixes)
        {
            var test = File.ReadAllText(beforeFullFileName);
            var fixCode = File.ReadAllText(afterFullFileName);
            

            CustomHelpers.MakeCommonNamespaces(ref test, ref fixCode);

            if (multipleFixes)
            {
                VerifyCSharpFix(test, fixCode, ordinal,  true);
            }
            else
            {
                VerifyCSharpFix(test, fixCode,  0, true);
            }
        }

        protected virtual Action<Workspace> AddWorkspaceOptions()
        {
            return IndentSpaces == 4 || IndentSpaces == 0
                                            ? (Action<Workspace>)null
                                            : x => x.Options = x.Options.WithChangedOption(
                                                      FormattingOptions.IndentationSize,
                                                      LanguageNames.CSharp,
                                                      IndentSpaces);
        }
    }
}