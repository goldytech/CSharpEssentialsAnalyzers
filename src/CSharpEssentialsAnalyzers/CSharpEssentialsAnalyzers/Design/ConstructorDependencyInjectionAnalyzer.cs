namespace CSharpEssentialsAnalyzers.Design
{
    using System;
    using System.Collections.Immutable;
    using System.Linq;

    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CSharp;
    using Microsoft.CodeAnalysis.CSharp.Syntax;
    using Microsoft.CodeAnalysis.Diagnostics;

    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class ConstructorDependencyInjectionAnalyzer : DiagnosticAnalyzer
    {
        public const int MaximumLinesOfCode = 25;

        /// <summary>
        /// The diagnostic id.
        /// </summary>
        public static readonly string DiagnosticId = DiagnosticIds.Ctor.ToDiagnosticId();

        /// <summary>
        /// The title.
        /// </summary>
        internal static readonly LocalizableString Title = new LocalizableResourceString(nameof(Resources.CtorTitle), Resources.ResourceManager, typeof(Resources));

        /// <summary>
        /// The message format.
        /// </summary>
        public static readonly LocalizableString MessageFormat = new LocalizableResourceString(nameof(Resources.CtorMessageFormat), Resources.ResourceManager, typeof(Resources));

        /// <summary>
        /// The description.
        /// </summary>
        internal static readonly LocalizableString Description = new LocalizableResourceString(nameof(Resources.CtorDescription), Resources.ResourceManager, typeof(Resources));

        internal static DiagnosticDescriptor Rule = new DiagnosticDescriptor(
                                                     DiagnosticId,
                                                     Title,
                                                     MessageFormat,
                                                     AnalyzerCategories.Design,
                                                     DiagnosticSeverity.Hidden,
                                                     true,
                                                     Description);

        internal static ITypeSymbol ClassTypeData;

        

        public override void Initialize(AnalysisContext context)
        {
            ClassTypeData = null;
            context.RegisterSyntaxNodeAction(this.CheckDependencyInjectionInConstructor, SyntaxKind.ConstructorDeclaration);
        }

        private void CheckDependencyInjectionInConstructor(SyntaxNodeAnalysisContext syntaxNodeAnalysisContext)
        {
            var constructorDeclarationSyntax = syntaxNodeAnalysisContext.Node as ConstructorDeclarationSyntax;

            if (constructorDeclarationSyntax == null)
            {
                return;
            }

            var classdeclarationNode = constructorDeclarationSyntax.Parent as ClassDeclarationSyntax;
            var semanticModel = syntaxNodeAnalysisContext.SemanticModel;


            if (this.CheckIfGivenParameterAlreadyExists(constructorDeclarationSyntax.ParameterList))
            {
                return;
            }

            foreach (var statementSyntax in constructorDeclarationSyntax.Body.Statements)
                {
                    if (statementSyntax.Kind() != SyntaxKind.ExpressionStatement)
                    {
                        // we are only intrested in Expression statement else we continue
                        continue;
                    }
                    if (!this.AnalyzeStatement(statementSyntax, semanticModel))
                    {
                        continue;
                    }
                    if (!this.DoesFieldDeclarationExists(classdeclarationNode,ClassTypeData.MetadataName,semanticModel))
                    {
                        continue;
                    }
                    var diagnostic = Diagnostic.Create(Rule, statementSyntax.GetLocation(), Description);
                    syntaxNodeAnalysisContext.ReportDiagnostic(diagnostic);
                    return;
                }
            }
        

        private bool CheckIfGivenParameterAlreadyExists(ParameterListSyntax parameterList)
        {
            if (ClassTypeData == null)
            {
                return false;
            }

            return parameterList.Parameters.Any(parameter => parameter.Identifier.Text == ClassTypeData.Name.ToLower().Substring(1, ClassTypeData.Name.Length - 1));
        }

        private bool AnalyzeStatement(StatementSyntax statementSyntax, SemanticModel semanticModel)
        {
            var assignmentExpression = statementSyntax.ChildNodes().FirstOrDefault(x => x.Kind() == SyntaxKind.SimpleAssignmentExpression);
            var objectCreationExpression = assignmentExpression.ChildNodes().FirstOrDefault(x => x.Kind() == SyntaxKind.ObjectCreationExpression);
           var typeSymbol = semanticModel.GetTypeInfo(objectCreationExpression).ConvertedType;
            var typeInfo = semanticModel.GetTypeInfo(objectCreationExpression);
            ClassTypeData = typeSymbol;
            
            return typeSymbol.OriginalDefinition.SpecialType == SpecialType.None  // should be custom user defined type and not any built-in type
                              && typeInfo.Type.TypeKind == TypeKind.Class  // should be of Class Type
                              && typeSymbol.IsReferenceType   // should be reference type
                              && typeInfo.Type.Interfaces.Any(); // should have atleast one interface available which it implements directly
        }

        private bool DoesFieldDeclarationExists(ClassDeclarationSyntax classDeclarationSyntax, string typeName, SemanticModel semanticModel)
        {
            foreach (var fieldDeclaration in from syntaxNode in classDeclarationSyntax.ChildNodes()
                                             where syntaxNode.Kind() == SyntaxKind.FieldDeclaration
                                             select syntaxNode as FieldDeclarationSyntax)
            {
                var typeSymbol = semanticModel.GetSymbolInfo(fieldDeclaration.Declaration.Type);
                return typeSymbol.Symbol.MetadataName != null && (typeSymbol.Symbol != null && typeSymbol.Symbol.MetadataName.Equals(typeName));
            }

            return false;
        }

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(Rule);
    }
}