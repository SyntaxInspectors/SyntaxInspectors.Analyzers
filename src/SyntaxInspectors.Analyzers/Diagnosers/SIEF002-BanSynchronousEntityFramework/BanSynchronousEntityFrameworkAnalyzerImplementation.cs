using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using SyntaxInspectors.Analyzers.Support;

namespace SyntaxInspectors.Analyzers.Diagnosers.BanSynchronousEntityFramework;

[SuppressMessage("ReSharper", "UseCollectionExpression", Justification = "Not supported in lower versions of Roslyn")]
internal sealed class BanSynchronousEntityFrameworkAnalyzerImplementation : SyntaxNodeAnalyzerImplementationBase<BanSynchronousEntityFrameworkAnalyzer>
{
    private static readonly ImmutableHashSet<string> ForbiddenMethods = new[]
    {
        "All", "Any", "AsEnumerable", "Average", "Contains",
        "Count", "ElementAt", "ElementAtOrDefault", "Find",
        "First", "FirstOrDefault", "Last", "LastOrDefault",
        "LongCount", "Max", "MaxBy", "Min", "MinBy",
        "Single", "SingleOrDefault", "Sum", "ToArray",
        "ToDictionary", "ToHashSet", "ToList", "ToLookup"
    }.ToImmutableHashSet(StringComparer.Ordinal);

    public BanSynchronousEntityFrameworkAnalyzerImplementation(in SyntaxNodeAnalysisContext context) : base(context)
    {
    }

    public void AnalyzeInvocation()
    {
        var invocation = (InvocationExpressionSyntax)Context.Node;

        if (Context.SemanticModel.GetSymbolInfo(invocation, cancellationToken: Context.CancellationToken).Symbol is not IMethodSymbol symbol)
        {
            return;
        }

        if (!ForbiddenMethods.Contains(symbol.Name))
        {
            return;
        }

        var receiverType = GetReceiverType(invocation, Context.SemanticModel, Context.CancellationToken);
        if (receiverType is null)
        {
            return;
        }

        if (!DerivesFromQueryableOrDbSet(receiverType))
        {
            return;
        }

        if (IsPartOfExpressionTree(invocation, Context.SemanticModel, Context.CancellationToken))
        {
            return;
        }

        var location = GetDiagnosticLocation(invocation);
        var diagnostic = Diagnostic.Create(DiagnosticRules.Default.Rule, location, symbol.Name);
        Context.ReportDiagnostic(diagnostic);
    }

    private static Location GetDiagnosticLocation(InvocationExpressionSyntax invocation)
    {
        if (invocation.Expression is MemberAccessExpressionSyntax memberAccess)
        {
            return memberAccess.Name.GetLocation();
        }

        return invocation.GetLocation();
    }

    private static bool IsPartOfExpressionTree(SyntaxNode node, SemanticModel model, CancellationToken cancellationToken)
    {
        var lambdaParent = node.Ancestors().OfType<LambdaExpressionSyntax>().FirstOrDefault();
        if (lambdaParent is null)
        {
            return false;
        }

        var typeInfo = model.GetTypeInfo(lambdaParent, cancellationToken);
        var type = typeInfo.ConvertedType;

        if (type is null)
        {
            return false;
        }

        return string.Equals(type.ContainingNamespace.ToDisplayString(), "System.Linq.Expressions", StringComparison.Ordinal)
            && string.Equals(type.Name, "Expression", StringComparison.Ordinal);
    }

    private static ITypeSymbol? GetReceiverType(InvocationExpressionSyntax invocation, SemanticModel model, CancellationToken cancellationToken)
    {
        if (invocation.Expression is MemberAccessExpressionSyntax memberAccess)
        {
            return model.GetTypeInfo(memberAccess.Expression, cancellationToken: cancellationToken).Type;
        }

        if (invocation.Expression is IdentifierNameSyntax && invocation.ArgumentList.Arguments.Count > 0)
        {
            return model.GetTypeInfo(invocation.ArgumentList.Arguments[0].Expression, cancellationToken: cancellationToken).Type;
        }

        return null;
    }

    private static bool DerivesFromQueryableOrDbSet(ITypeSymbol type)
    {
        if (type.AllInterfaces.Any(i => string.Equals(i.Name, "IQueryable", StringComparison.Ordinal)))
        {
            return true;
        }

        string fullName = type.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);
        return fullName.StartsWith("Microsoft.EntityFrameworkCore.DbSet<", StringComparison.Ordinal);
    }

    internal static class DiagnosticRules
    {
        internal static ImmutableArray<DiagnosticDescriptor> Rules { get; }
            = CommonRules.AllCommonRules
                         .Append(Default.Rule)
                         .ToImmutableArray();

        internal static class Default
        {
            private const string Category = "Performance";
            public const string DiagnosticId = "SIEF0002";
            public static readonly string HelpLinkUri = HelpLinkFactory.CreateForDiagnosticId(DiagnosticId);
            public static readonly LocalizableString Title = "Synchronous EF Core call detected";
            public static readonly LocalizableString MessageFormat = "Avoid using synchronous '{0}' on IQueryable or DbSet. Use the async alternative instead.";
            public static readonly LocalizableString Description = "Use Async EF Core APIs instead of blocking synchronous calls.";
            public static readonly DiagnosticDescriptor Rule = new(DiagnosticId, Title, MessageFormat, Category, DiagnosticSeverity.Warning, true, Description, HelpLinkUri);
        }
    }
}
