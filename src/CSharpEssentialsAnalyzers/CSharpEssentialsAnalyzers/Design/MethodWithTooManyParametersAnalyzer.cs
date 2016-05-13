namespace CSharpEssentialsAnalyzers.Design
{
    using System.Collections.Immutable;

    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CSharp;
    using Microsoft.CodeAnalysis.CSharp.Syntax;
    using Microsoft.CodeAnalysis.Diagnostics;

    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class MethodWithTooManyParametersAnalyzer : DiagnosticAnalyzer
    {
        /// <summary>
        /// The diagnostic id.
        /// </summary>
        public static readonly string DiagnosticId = DiagnosticIds.MethodWithTooManyParameters.ToDiagnosticId();

        /// <summary>
        /// The title.
        /// </summary>
        internal static readonly LocalizableString Title = new LocalizableResourceString(nameof(Resources.MethodWithTooManyParametersTitle), Resources.ResourceManager, typeof(Resources));

        /// <summary>
        /// The message format.
        /// </summary>
        public static readonly LocalizableString MessageFormat = new LocalizableResourceString(nameof(Resources.MethodWithTooManyParametersMessageFormat), Resources.ResourceManager, typeof(Resources));

        /// <summary>
        /// The description.
        /// </summary>
        internal static readonly LocalizableString Description = new LocalizableResourceString(nameof(Resources.MethodWithTooManyParametersDescription), Resources.ResourceManager, typeof(Resources));

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
            context.RegisterSyntaxNodeAction(this.CheckMethodWithTooManyParameters, SyntaxKind.ParameterList);
        }

        private void CheckMethodWithTooManyParameters(SyntaxNodeAnalysisContext syntaxNodeAnalysisContext)
        {
            var parameterListSyntax = syntaxNodeAnalysisContext.Node as ParameterListSyntax;
            if (!(parameterListSyntax?.Parameters.Count >= 5))
            {
                return;
            }
            var diagnostic = Diagnostic.Create(Rule, parameterListSyntax.GetLocation(), Description);
            syntaxNodeAnalysisContext.ReportDiagnostic(diagnostic);
        }

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(Rule);

        
    }
}