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

    [ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(ConstructorDependencyInjectionCodeFix)), Shared]
    public class ConstructorDependencyInjectionCodeFix :CodeFixProvider
    {
        public override async Task RegisterCodeFixesAsync(CodeFixContext context)
        {
            var diagnostic = context.Diagnostics.First();
            
            var constructorDeclarationSyntax = await CodeFixHelpers.FirstAncestorNodeFromCodeFixContext<ConstructorDeclarationSyntax>(context, diagnostic).ConfigureAwait(false);
           // context.RegisterCodeFix(CodeAction.Create("Msg", c => AddParameterToConstructorAsync(context.Document,parameterListSyntax,c),null ));

            
            context.RegisterCodeFix(CodeAction.Create(Resources.CtorCodeFix, ct => this.AddParameterToConstructorAsync(context.Document,diagnostic,constructorDeclarationSyntax,ct)),diagnostic);

        }

        private async Task<Document> AddParameterToConstructorAsync(Document document, Diagnostic declarationSyntax, ConstructorDeclarationSyntax constructorDeclarationSyntax, CancellationToken cancellationToken)
        {

            var root = await document.GetSyntaxRootAsync(cancellationToken).ConfigureAwait(false);
            var node = root.FindNode(declarationSyntax.Location.SourceSpan);
            var semanticModel = await document.GetSemanticModelAsync(cancellationToken).ConfigureAwait(false);
            var fieldName = this.GetFieldVariableName(constructorDeclarationSyntax.Parent,ConstructorDependencyInjectionAnalyzer.ClassTypeData.Name,semanticModel);
            if (fieldName == null)
            {
                return null;
            }
            var assignmentField = SyntaxFactory.ExpressionStatement(SyntaxFactory.AssignmentExpression(SyntaxKind.SimpleAssignmentExpression,
                SyntaxFactory.MemberAccessExpression(SyntaxKind.SimpleMemberAccessExpression, SyntaxFactory.ThisExpression(),
                    SyntaxFactory.IdentifierName(fieldName)), SyntaxFactory.IdentifierName(ConstructorDependencyInjectionAnalyzer.ClassTypeData.Name.ToLower().Substring(1, ConstructorDependencyInjectionAnalyzer.ClassTypeData.Name.Length - 1))));
            
                     
            var  newConstructor =
                constructorDeclarationSyntax.WithBody(constructorDeclarationSyntax.Body.AddStatements(assignmentField)).AddParameterListParameters(SyntaxFactory.Parameter(
                    SyntaxFactory.Identifier(ConstructorDependencyInjectionAnalyzer.ClassTypeData.Name.ToLower().Substring(1, ConstructorDependencyInjectionAnalyzer.ClassTypeData.Name.Length - 1)))
                    .WithType(SyntaxFactory.ParseTypeName(ConstructorDependencyInjectionAnalyzer.ClassTypeData.Name)));
            return await CodeFixHelpers.ReplaceNode(document, constructorDeclarationSyntax, newConstructor, cancellationToken).ConfigureAwait(false);
        }

        private string GetFieldVariableName(SyntaxNode rootNode, string variableType,SemanticModel semanticModel)
        {
            foreach (var syntaxNode in rootNode.ChildNodes())
            {
                if (syntaxNode.Kind() == SyntaxKind.FieldDeclaration)
                {
                    var fieldDeclaration = syntaxNode as FieldDeclarationSyntax;
                    if (fieldDeclaration == null)
                    {
                        continue;
                    }

                    foreach (var variableDeclaratorSyntax in fieldDeclaration.Declaration.Variables)
                    {
                        if (variableDeclaratorSyntax.Identifier.ValueText.Equals(variableType))
                        {
                            return variableDeclaratorSyntax.Identifier.ValueText;
                        }
                    }
                }
            }
            return string.Empty;
        }

        private bool AnalyzeField(FieldDeclarationSyntax fieldDeclaration, SemanticModel semanticModel,string typeName)
        {
            var typeSymbol = semanticModel.GetSymbolInfo(fieldDeclaration.Declaration.Type);
            return typeSymbol.Symbol.MetadataName != null && (typeSymbol.Symbol != null && typeSymbol.Symbol.MetadataName.Equals(typeName));
        }

        private void OnFirstTaskCompleted(Task obj)
        {
            throw new System.NotImplementedException();
        }

        public override ImmutableArray<string> FixableDiagnosticIds
            => ImmutableArray.Create(ConstructorDependencyInjectionAnalyzer.DiagnosticId);

        public override sealed FixAllProvider GetFixAllProvider()
        => WellKnownFixAllProviders.BatchFixer;

        
    }
}