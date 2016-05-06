namespace CSharpEssentialsAnalyzers.Design
{
    using System;
    using System.Collections.Generic;
    using System.Collections.Immutable;
    using System.Composition;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;

    using CSharpEssentialsAnalyzers.Helpers;

    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CodeActions;
    using Microsoft.CodeAnalysis.CodeFixes;
    using Microsoft.CodeAnalysis.CSharp;
    using Microsoft.CodeAnalysis.CSharp.Syntax;

    [ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(EmptyCatchClauseCodeFix)), Shared]
    public class EmptyCatchClauseCodeFix : CodeFixProvider
    {

        internal static readonly LocalizableString FixMessage00 =
        new LocalizableResourceString(nameof(Resources.EmptyCatchClause00CodeFixMessage),
                                      Resources.ResourceManager, typeof(Resources));
        internal static readonly LocalizableString FixMessage01 =
           new LocalizableResourceString(nameof(Resources.EmptyCatchClause01CodeFixMessage),
                                         Resources.ResourceManager, typeof(Resources));
        internal static readonly LocalizableString FixMessage02 =
           new LocalizableResourceString(nameof(Resources.EmptyCatchClause02CodeFixMessage),
                                         Resources.ResourceManager, typeof(Resources));
        internal static readonly LocalizableString FixMessage03 =
           new LocalizableResourceString(nameof(Resources.EmptyCatchClause03CodeFixMessage),
                                         Resources.ResourceManager, typeof(Resources));

        public override async Task RegisterCodeFixesAsync(CodeFixContext context)
        {
            var diagnostic = context.Diagnostics.First();
            var catchClause = await CodeFixHelpers.FirstAncestorNodeFromCodeFixContext<CatchClauseSyntax>(context, diagnostic)
                    .ConfigureAwait(false);
            context.RegisterCodeFix(CodeAction.Create(FixMessage00.ToString(),
                c => RemoveEmptyCatchBlockAsync(context.Document, catchClause, c)), diagnostic);

            context.RegisterCodeFix(
    CodeAction.Create(FixMessage01.ToString(),
         c => this.RemoveEmptyCatchBlockPutCommentAsync(context.Document, catchClause, c)),
    diagnostic);
            context.RegisterCodeFix(
                CodeAction.Create(FixMessage02.ToString(),
                     c => RemoveEmptyCatchBlockAndBracesAsync(context.Document, catchClause, c)),
                diagnostic);
            context.RegisterCodeFix(
                CodeAction.Create(FixMessage03.ToString(),
                     c => InsertExceptionAsync(context.Document, catchClause, c)),
                diagnostic);


        }

        private async Task<Document> InsertExceptionAsync(
            Document document,
            CatchClauseSyntax catchClause,
            CancellationToken cancellationToken)
        {
            {
                var catchDeclaration =
                    SyntaxFactory.CatchDeclaration(SyntaxFactory.IdentifierName("Exception"))
                        .WithIdentifier(SyntaxFactory.Identifier("ex"));

                // Preserve the block because it may have comments
                var newBlock = catchClause.Block.AddStatements(SyntaxFactory.ThrowStatement());

                var newCatch = catchClause.WithDeclaration(catchDeclaration).WithBlock(newBlock);

                var actions = new List<Func<CompilationUnitSyntax, CompilationUnitSyntax>>()
                                  {
                                      x =>
                                      CodeFixHelpers
                                          .ReplaceNode(
                                              x,
                                              catchClause,
                                              newCatch),
                                      x =>
                                      CodeFixHelpers.AddUsing
                                          (x, "System")
                                  };

                return await CodeFixHelpers.UpdateDocument(document, actions, cancellationToken).ConfigureAwait(false);
            }
        }

        private async Task<Document> RemoveEmptyCatchBlockAndBracesAsync(Document document, CatchClauseSyntax catchClause, CancellationToken cancellationToken)
        {
            var tryStatement = catchClause.Parent as TryStatementSyntax;
            SyntaxNode newNode;
            SyntaxNode oldNode;
            var parentSyntax = tryStatement.Parent;
            var blockSyntax = parentSyntax as BlockSyntax;
            if (blockSyntax != null)
            {
                newNode = this.RemoveBraces(tryStatement, blockSyntax);
                oldNode = blockSyntax;
            }
            else
            {
                newNode = tryStatement.Block;
                oldNode = tryStatement;
            }
            return await CodeFixHelpers.ReplaceNode(document,
               oldNode, newNode, cancellationToken).ConfigureAwait(false);
        }

        private async Task<Document> RemoveEmptyCatchBlockPutCommentAsync(Document document, CatchClauseSyntax catchClause, CancellationToken cancellationToken)
        {
            var tryStatement = catchClause.Parent as TryStatementSyntax;
            var statementBlock = tryStatement.Block
                        .WithTrailingTrivia(tryStatement.GetTrailingTrivia()
                           .Add(SyntaxFactory.Comment("//TODO: Consider reading MSDN Documentation about how to use Try...Catch => http://msdn.microsoft.com/en-us/library/0yd65esw.aspx"))
                           .Add(SyntaxFactory.EndOfLine("\r\n")));

            return await CodeFixHelpers.ReplaceNode(document, tryStatement,
               statementBlock, cancellationToken).ConfigureAwait(false);
        }

        private async Task<Document> RemoveEmptyCatchBlockAsync(Document document, CatchClauseSyntax catchClause, CancellationToken cancellationToken)
        {
            {
                var tryStatement = catchClause.Parent as TryStatementSyntax;
                return await CodeFixHelpers.ReplaceNode(document, tryStatement,
                   tryStatement.Block, cancellationToken).ConfigureAwait(false);
            }
        }

        public override ImmutableArray<string> FixableDiagnosticIds => ImmutableArray.Create(EmptyCatchClauseAnalyzer.DiagnosticId);

        public override FixAllProvider GetFixAllProvider() => WellKnownFixAllProviders.BatchFixer;

        private SyntaxNode RemoveBraces(TryStatementSyntax tryStatement,
               BlockSyntax blockSyntax)
        {
            var blockStatements = blockSyntax.Statements;
            var newStatements = blockStatements.ReplaceRange(
                              tryStatement, tryStatement.Block.Statements);
            var newBlock = blockSyntax
                              .WithStatements(newStatements);
            return newBlock;
        }
    }
}