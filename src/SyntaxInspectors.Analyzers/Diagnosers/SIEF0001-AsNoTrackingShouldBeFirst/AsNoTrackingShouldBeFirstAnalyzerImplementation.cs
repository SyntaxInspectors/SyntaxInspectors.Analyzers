using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using SyntaxInspectors.Analyzers.Support;

namespace SyntaxInspectors.Analyzers.Diagnosers.AsNoTrackingShouldBeFirst;

internal sealed class AsNoTrackingShouldBeFirstAnalyzerImplementation : SyntaxNodeAnalyzerImplementationBase<AsNoTrackingShouldBeFirstAnalyzer>
{
    public AsNoTrackingShouldBeFirstAnalyzerImplementation(in SyntaxNodeAnalysisContext context) : base(context)
    {
    }

    public void AnalyzeInvocation()
    {
        var invocation = (InvocationExpressionSyntax)Context.Node;
        if (invocation.Expression is not MemberAccessExpressionSyntax memberAccess)
        {
            return;
        }

        if (!string.Equals(memberAccess.Name.Identifier.Text, "AsNoTracking", StringComparison.Ordinal))
        {
            return;
        }

        var symbol = Context.SemanticModel.GetSymbolInfo(memberAccess, cancellationToken: Context.CancellationToken).Symbol;
        if (symbol is null)
        {
            return;
        }

        if (!string.Equals(symbol.Name, "AsNoTracking", StringComparison.Ordinal))
        {
            return;
        }

        string? containingType = symbol.ContainingType?.ToDisplayString();
        if (!string.Equals(containingType, "Microsoft.EntityFrameworkCore.EntityFrameworkQueryableExtensions", StringComparison.Ordinal))
        {
            return;
        }

        var expr = memberAccess.Expression;
        while (expr is InvocationExpressionSyntax prevInvocation)
        {
            expr = (prevInvocation.Expression as MemberAccessExpressionSyntax)?.Expression;
        }

        if (expr is null)
        {
            return;
        }

        var typeInfo = Context.SemanticModel.GetTypeInfo(expr, cancellationToken: Context.CancellationToken).Type;

        if (typeInfo?.Name.Contains("DbSet", StringComparison.Ordinal) != true)
        {
            return;
        }

        if (!(memberAccess.Expression is IdentifierNameSyntax
            || (memberAccess.Expression is MemberAccessExpressionSyntax rootMember && (Context.SemanticModel.GetTypeInfo(rootMember, cancellationToken: Context.CancellationToken).Type?.Name.Contains("DbSet", StringComparison.Ordinal) ?? false))))
        {
            Context.ReportDiagnostic(Diagnostic.Create(DiagnosticRules.Default.Rule, memberAccess.Name.GetLocation()));
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
            private const string Category = "Usage";
            public const string DiagnosticId = "SIEF0001";
            public static readonly string HelpLinkUri = HelpLinkFactory.CreateForDiagnosticId(DiagnosticId);
            public static readonly LocalizableString Title = "AsNoTracking() should be first in the chain";
            public static readonly LocalizableString MessageFormat = "AsNoTracking() must be the first call in the expression chain";
            public static readonly LocalizableString Description = MessageFormat;
            public static readonly DiagnosticDescriptor Rule = new(DiagnosticId, Title, MessageFormat, Category, DiagnosticSeverity.Warning, true, Description, HelpLinkUri);
        }
    }
}
