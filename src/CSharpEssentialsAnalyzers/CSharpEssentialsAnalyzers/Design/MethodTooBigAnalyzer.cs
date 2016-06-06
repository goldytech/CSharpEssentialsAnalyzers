namespace CSharpEssentialsAnalyzers.Design
{
    using System.Collections.Immutable;

    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CSharp;
    using Microsoft.CodeAnalysis.CSharp.Syntax;
    using Microsoft.CodeAnalysis.Diagnostics;

    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class MethodTooBigAnalyzer : DiagnosticAnalyzer
    {
        public const int MaximumLinesOfCode = 25;

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

        public override void Initialize(AnalysisContext context)
        {
            context.RegisterSyntaxNodeAction(this.CheckMethodLength, SyntaxKind.MethodDeclaration);
        }

        private void CheckMethodLength(SyntaxNodeAnalysisContext syntaxNodeAnalysisContext)
        {
            var method = syntaxNodeAnalysisContext.Node as MethodDeclarationSyntax;
            if (method?.Body != null && method.Body.Statements.Count > MaximumLinesOfCode)
            {
                var diagnostic = Diagnostic.Create(Rule, method.GetLocation(), Description);
                syntaxNodeAnalysisContext.ReportDiagnostic(diagnostic); 
            }
        }

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(Rule);
    }
}