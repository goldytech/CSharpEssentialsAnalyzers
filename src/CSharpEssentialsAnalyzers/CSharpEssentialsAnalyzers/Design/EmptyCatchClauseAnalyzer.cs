namespace CSharpEssentialsAnalyzers.Design
{
    using System.Collections.Immutable;

    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CSharp;
    using Microsoft.CodeAnalysis.CSharp.Syntax;
    using Microsoft.CodeAnalysis.Diagnostics;
    
    /// <summary>
    /// The empty catch clause analyzer.
    /// </summary>
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class EmptyCatchClauseAnalyzer : DiagnosticAnalyzer
    {
        /// <summary>
        /// The diagnostic id.
        /// </summary>
        public static readonly string DiagnosticId = DiagnosticIds.EmptyCatchClause.ToDiagnosticId();
        internal static readonly LocalizableString Title = new LocalizableResourceString(nameof(Resources.EmptyCatchClauseTitle), Resources.ResourceManager, typeof(Resources));
        public static readonly LocalizableString MessageFormat = new LocalizableResourceString(nameof(Resources.EmptyCatchClauseMessageFormat), Resources.ResourceManager, typeof(Resources));
        internal static readonly LocalizableString Description = new LocalizableResourceString(nameof(Resources.EmptyCatchClauseDescription), Resources.ResourceManager, typeof(Resources));

        internal static DiagnosticDescriptor Rule = new DiagnosticDescriptor(
                                                     DiagnosticId,
                                                     Title,
                                                     MessageFormat,
                                                     AnalyzerCategories.Design,
                                                     DiagnosticSeverity.Warning,
                                                     isEnabledByDefault: true);
        public override void Initialize(AnalysisContext context)
        {
             context.RegisterSyntaxNodeAction(this.CheckCatchClause,SyntaxKind.CatchClause);
        }

        private void CheckCatchClause(SyntaxNodeAnalysisContext syntaxNodeAnalysisContext)
        {
            var catchStatement = syntaxNodeAnalysisContext.Node as CatchClauseSyntax;
            if (catchStatement?.Block?.Statements.Count != 0)
            {
                return;
            }
            var diagnostic = Diagnostic.Create(Rule, catchStatement.GetLocation(), Description);
            syntaxNodeAnalysisContext.ReportDiagnostic(diagnostic);
        }

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(Rule);
    }
}