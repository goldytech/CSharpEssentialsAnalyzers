namespace CSharpEssentialsAnalyzers.Helpers
{
    using System;
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;

    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CodeFixes;
    using Microsoft.CodeAnalysis.CSharp;
    using Microsoft.CodeAnalysis.CSharp.Syntax;
    using Microsoft.CodeAnalysis.Formatting;

    public static class CodeFixHelpers
    {
        public static async Task<T> FirstAncestorNodeFromCodeFixContext<T>(
                  CodeFixContext context, Diagnostic diagnostic)
            where T : SyntaxNode
        {
            var root = await context
                              .Document
                              .GetSyntaxRootAsync(context.CancellationToken)
                              .ConfigureAwait(false);
            var diagnosticSpan = diagnostic.Location.SourceSpan;
            var node = root
                              .FindToken(diagnosticSpan.Start)
                              .Parent
                              .FirstAncestorOrSelf<T>();
            return node;
        }

        public static async Task<Document> ReplaceNode(Document document,
                 SyntaxNode oldNode, SyntaxNode newNode,
                 CancellationToken cancellationToken,
                 bool moveTrivia = false)
        {
            var actions = new List<Func<CompilationUnitSyntax, CompilationUnitSyntax>>()
         {
               x => ReplaceNode(x, oldNode, newNode, moveTrivia)
           
         };
            return await UpdateDocument(document, actions, cancellationToken);
        }

        public static async Task<Document> UpdateDocument(Document document,
                  IEnumerable<Func<CompilationUnitSyntax, CompilationUnitSyntax>> actions,
                  CancellationToken cancellationToken)
        {
            var rootNode = await document.GetSyntaxRootAsync(cancellationToken);
            var root = rootNode as CompilationUnitSyntax;
            var newRoot = root;
            foreach (var action in actions)
            {
                newRoot = action(newRoot);
            }
            var newDocument = document.WithSyntaxRoot(newRoot);
            return newDocument;
        }

        public static CompilationUnitSyntax ReplaceNode(CompilationUnitSyntax root,
                 SyntaxNode oldNode, SyntaxNode newNode, bool moveTrivia = false)
        {
            if (moveTrivia)
            {
                newNode = newNode
                         .WithLeadingTrivia(oldNode
                                            .GetLeadingTrivia()
                                            .AddRange(newNode.GetLeadingTrivia()))
                         .WithTrailingTrivia(oldNode
                                            .GetTrailingTrivia()
                                            .AddRange(newNode.GetTrailingTrivia()));
            }
            newNode = newNode
                     .WithAdditionalAnnotations(Formatter.Annotation);
            var newRoot = root.ReplaceNode(oldNode, newNode);
            return newRoot;
        }

        public static CompilationUnitSyntax AddUsing(CompilationUnitSyntax root,
                 string usingName)
        {
            var newUsing = SyntaxFactory.UsingDirective(SyntaxFactory.IdentifierName(usingName))
                           .WithAdditionalAnnotations(Formatter.Annotation);
            root = root.AddUsings(newUsing);
            return root;
        }

        public static T UpdateNode<T>(T node, Func<T, T> update)
           where T : SyntaxNode
        {
            var trivia = node.GetLeadingTrivia();
            var newNode = node.WithoutLeadingTrivia();
            newNode = update(newNode);
            return newNode.WithLeadingTrivia(trivia);
        }

    }
}