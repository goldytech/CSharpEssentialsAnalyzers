namespace CSharpEssentialsAnalyzers.Design
{
    using System.Collections.Immutable;
    using System.Composition;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;

    using Helpers;

    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CodeActions;
    using Microsoft.CodeAnalysis.CodeFixes;
    using Microsoft.CodeAnalysis.CSharp;
    using Microsoft.CodeAnalysis.CSharp.Syntax;

    [ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(MethodWithTooManyParametersCodeFix)), Shared]
    public class MethodWithTooManyParametersCodeFix :CodeFixProvider
    {
        public override async Task RegisterCodeFixesAsync(CodeFixContext context)
        {
            var diagnostic = context.Diagnostics.First();
            var parameterListSyntax = await CodeFixHelpers.FirstAncestorNodeFromCodeFixContext<ParameterListSyntax>(context, diagnostic).ConfigureAwait(false);
           // context.RegisterCodeFix(CodeAction.Create("Msg", c => AddToDoCommentsOnTopOfTheMethodAsync(context.Document,parameterListSyntax,c),null ));

            
            context.RegisterCodeFix(CodeAction.Create(Resources.MethodWithTooManyParametersCodeFix, ct => this.AddToDoCommentsOnTopOfTheMethodAsync(context.Document,parameterListSyntax,ct)),diagnostic);

        }

        private async Task<Document> AddToDoCommentsOnTopOfTheMethodAsync(Document document, ParameterListSyntax parameterListSyntax, CancellationToken cancellationToken)
        {
            var todoComment = SyntaxFactory.Comment("// TODO Refactor it to reduce the number of parameters. For help refer --> https://refactoring.guru/smells/long-parameter-list ");
            var endOfLine = SyntaxFactory.EndOfLine("\r\n");

            var methodDeclarationSyntax = parameterListSyntax.Parent as MethodDeclarationSyntax;
            if (methodDeclarationSyntax == null)
            {
                return null;
            }
            var existingCode = methodDeclarationSyntax.Body;

            var newCode = existingCode.WithLeadingTrivia(parameterListSyntax.GetLeadingTrivia().Add(todoComment).Add(endOfLine));
            return await CodeFixHelpers.ReplaceNode(document, existingCode, newCode, cancellationToken).ConfigureAwait(false);
        }

        public override ImmutableArray<string> FixableDiagnosticIds
            => ImmutableArray.Create(MethodWithTooManyParametersAnalyzer.DiagnosticId);

        public override sealed FixAllProvider GetFixAllProvider()
        => WellKnownFixAllProviders.BatchFixer;

        
    }
}