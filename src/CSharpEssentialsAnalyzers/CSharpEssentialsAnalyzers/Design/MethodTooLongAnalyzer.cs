namespace CSharpEssentialsAnalyzers.Design
{
    using System.Collections.Immutable;

    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CSharp;
    using Microsoft.CodeAnalysis.CSharp.Syntax;
    using Microsoft.CodeAnalysis.Diagnostics;

    /// <summary>
    /// The method too long analyzer.
    /// </summary>
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class MethodTooLongAnalyzer : DiagnosticAnalyzer
    {
        
         /// <summary>
        /// The diagnostic id.
        /// </summary>
        public static readonly string DiagnosticId = DiagnosticIds.MethodTooLong.ToDiagnosticId();

        /// <summary>
        /// The title.
        /// </summary>
        internal static readonly LocalizableString Title = new LocalizableResourceString(nameof(Resources.MethodTooLongTitle), Resources.ResourceManager, typeof(Resources));

        /// <summary>
        /// The message format.
        /// </summary>
        public static readonly LocalizableString MessageFormat = new LocalizableResourceString(nameof(Resources.MethodTooLongMessageFormat), Resources.ResourceManager, typeof(Resources));

        /// <summary>
        /// The description.
        /// </summary>
        internal static readonly LocalizableString Description = new LocalizableResourceString(nameof(Resources.MethodTooLongDescription), Resources.ResourceManager, typeof(Resources));

        internal static DiagnosticDescriptor Rule = new DiagnosticDescriptor(
                                                     DiagnosticId,
                                                     Title,
                                                     MessageFormat,
                                                     AnalyzerCategories.Design,
                                                     DiagnosticSeverity.Warning,
                                                     true,
                                                     Description);

        /// <summary>
        /// The maximum number of lines.
        /// </summary>
       // private const int MaximumNumberOfLines = 25;

        public override void Initialize(AnalysisContext context)
        {
            context.RegisterSyntaxNodeAction(this.CheckMethodTooLong, SyntaxKind.MethodDeclaration);
            
        }

        private void CheckMethodTooLong(SyntaxNodeAnalysisContext syntaxNodeAnalysisContext)
        {
            var methodDeclaration = syntaxNodeAnalysisContext.Node as MethodDeclarationSyntax;
            if (methodDeclaration == null || methodDeclaration.Body.Statements.Count <= 25)
            {
                return;
            }

            var diagnostic = Diagnostic.Create(Rule, methodDeclaration.GetLocation(), Description);
            syntaxNodeAnalysisContext.ReportDiagnostic(diagnostic);
        }

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; }
    }
}
