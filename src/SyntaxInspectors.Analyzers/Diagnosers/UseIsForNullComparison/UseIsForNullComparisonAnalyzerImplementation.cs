#pragma warning disable

using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using SyntaxInspectors.Analyzers.Configuration;
using SyntaxInspectors.Analyzers.Extensions;
using SyntaxInspectors.Analyzers.Support;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace SyntaxInspectors.Analyzers.Diagnosers.UseIsForNullComparison;

[SuppressMessage("ReSharper", "UseCollectionExpression", Justification = "Not supported in lower versions of Roslyn")]
internal sealed class UseIsForNullComparisonAnalyzerImplementation : SyntaxNodeAnalyzerImplementationBase<UseIsForNullComparisonAnalyzer>
{
    private readonly IAnalyzerConfiguration _configuration;

    public UseIsForNullComparisonAnalyzerImplementation(in SyntaxNodeAnalysisContext context) : base(context)
    {
        _configuration = GenericConfigurationProvider.GetConfiguration(context, DiagnosticRules.Default.DiagnosticId);
    }

    public void AnalyzeBinaryExpression()
    {
        if (!_configuration.IsEnabled)
        {
            return;
        }

        var equalsExpression = (BinaryExpressionSyntax)Context.Node;
        if (!equalsExpression.IsKind(SyntaxKind.EqualsExpression) && !equalsExpression.IsKind(SyntaxKind.NotEqualsExpression))
        {
            return;
        }

        if (!equalsExpression.Right.IsKind(SyntaxKind.NullLiteralExpression))
        {
            return;
        }

        var typeInfo = Context.SemanticModel.GetTypeInfo(equalsExpression.Left).Type;
        if (typeInfo is null || !typeInfo.IsReferenceType)
        {
            return;
        }

        if (IsWithinQueryableWhere(equalsExpression))
        {
            return; // we allow the usage of '==' or '!=' within Queryable.Where calls
        }

        if (IsWithinQueryableLinq(equalsExpression))
        {
            return; // we allow the usage of '==' or '!=' within LINQ queries
        }

        var isOrIsNot = equalsExpression.IsKind(SyntaxKind.EqualsExpression) ? "is" : "is not";
        var intention = equalsExpression.IsKind(SyntaxKind.EqualsExpression) ? "null" : "not null";

        Context.ReportDiagnostic(Diagnostic.Create(DiagnosticRules.Default.Rule, equalsExpression.OperatorToken.GetLocation(), isOrIsNot, intention, equalsExpression.OperatorToken.Text));
    }

    private bool IsWithinQueryableLinq(BinaryExpressionSyntax binaryExpression)
    {
        if (!binaryExpression.IsKind(SyntaxKind.EqualsExpression) && !binaryExpression.IsKind(SyntaxKind.NotEqualsExpression))
        {
            return false;
        }

        var queryExpression = binaryExpression.Ancestors().OfType<QueryExpressionSyntax>().FirstOrDefault();
        if (queryExpression is null)
        {
            return false;
        }

        if (binaryExpression.Left is not IdentifierNameSyntax identifier)
        {
            return false;
        }

        var sourceDefinition = LinqIdentifierResolver.Resolve(queryExpression, identifier.Identifier.ValueText);
        if (sourceDefinition is null)
        {
            return false;
        }

        var sourceType = Context.SemanticModel.GetTypeInfo(sourceDefinition).Type;
        return sourceType is not null
               && sourceType.ToDisplayString().StartsWith("System.Linq.IQueryable");
    }

    private bool IsWithinQueryableWhere(BinaryExpressionSyntax binaryExpression)
    {
        return binaryExpression
              .Ancestors()
              .OfType<InvocationExpressionSyntax>()
              .Any(IsQueryableWhere);

        bool IsQueryableWhere(InvocationExpressionSyntax invocationExpression)
        {
            if (Context.SemanticModel.GetSymbolInfo(invocationExpression).Symbol is not IMethodSymbol methodSymbol)
            {
                return false;
            }

            return methodSymbol.Name.EqualsOrdinal("Where") && methodSymbol.ContainingType.ToDisplayString().EqualsOrdinal("System.Linq.Queryable");
        }
    }

    internal static class DiagnosticRules
    {
        internal static ImmutableArray<DiagnosticDescriptor> Rules { get; }
            = CommonRules.AllCommonRules
                         .Append(Default.Rule)
                         .ToImmutableArray();

        internal static class Default
        {
            private const string Category = "Intention";
            public const string DiagnosticId = "SI0011";
            public static readonly string HelpLinkUri = HelpLinkFactory.CreateForDiagnosticId(DiagnosticId);
            public static readonly LocalizableString Title = "Use `is` or `is not` for null-comparison";
            public static readonly LocalizableString MessageFormat = "Use `{0}` to compare for `{1}` instead of `{2}`";
            public static readonly LocalizableString Description = MessageFormat;
            public static readonly DiagnosticDescriptor Rule = new(DiagnosticId, Title, MessageFormat, Category, DiagnosticSeverity.Warning, true, Description, HelpLinkUri);
        }
    }
}
