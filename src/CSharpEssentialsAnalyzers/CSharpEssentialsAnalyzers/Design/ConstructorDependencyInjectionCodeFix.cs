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
            var classdeclarationNode = constructorDeclarationSyntax.Parent as ClassDeclarationSyntax;
            ConstructorDeclarationSyntax newCtorWithParameter;
           // if (classdeclarationNode != null)
          //  {
           //     foreach (var fieldDeclaration in from syntaxNode in classdeclarationNode.ChildNodes() where syntaxNode.Kind() == SyntaxKind.FieldDeclaration select syntaxNode as FieldDeclarationSyntax)
              //  {
            //        var semanticModel = await document.GetSemanticModelAsync(cancellationToken).ConfigureAwait(false);
             //       if (this.AnalyzeField(fieldDeclaration,semanticModel, ConstructorDependencyInjectionAnalyzer.ClassTypeData.Interfaces.FirstOrDefault().MetadataName))
             //       {
                       
                 //   }
               // }
           // }
            if (node != null)
            {
                var nodetobeRemoved = root.RemoveNode(node, SyntaxRemoveOptions.KeepNoTrivia);

                // remove existing assignment



                // newCtorWithParameter = constructorDeclarationSyntax.Body.Statements.Insert(0,SyntaxFactory.ExpressionStatement())
                newCtorWithParameter = constructorDeclarationSyntax.AddParameterListParameters(
                     SyntaxFactory.Parameter(
                         SyntaxFactory.Identifier(ConstructorDependencyInjectionAnalyzer.ClassTypeData.Name.ToLower()))
                         .WithType(SyntaxFactory.ParseTypeName(ConstructorDependencyInjectionAnalyzer.ClassTypeData.Interfaces.FirstOrDefault().Name)));
                //var doc = document.WithSyntaxRoot(nodetobeRemoved);
                //doc = await
                //  CodeFixHelpers.ReplaceNode(document, constructorDeclarationSyntax, newCtorWithParameter, cancellationToken)
                //       .ConfigureAwait(false);

                // return doc;

               // return await
                //        CodeFixHelpers.ReplaceNode(document, node, nodetobeRemoved, cancellationToken)
                  //          .ConfigureAwait(false);
            }
            return null;
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