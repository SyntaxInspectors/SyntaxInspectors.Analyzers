using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using SyntaxInspectors.Analyzers.Extensions;

namespace Sofia.Analyzers;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class BanSynchronousEntityFrameworkAnalyzer : DiagnosticAnalyzer
{
    private const string DiagnosticId = "SIEF1002";
    private static readonly LocalizableString Title = "Synchronous EF Core call detected";
    private static readonly LocalizableString MessageFormat = "Avoid using synchronous '{0}' on IQueryable or DbSet. Use the async alternative instead.";
    private static readonly LocalizableString Description = "Use Async EF Core APIs instead of blocking synchronous calls.";
    private const string Category = "Performance";

    private static readonly DiagnosticDescriptor Rule = new(
        DiagnosticId, Title, MessageFormat, Category,
        DiagnosticSeverity.Warning, isEnabledByDefault: true, description: Description
    );

    private static readonly string[] ForbiddenMethods =
    [
        "All", "Any", "AsEnumerable", "Average", "Contains",
        "Count", "ElementAt", "ElementAtOrDefault", "Find",
        "First", "FirstOrDefault", "Last", "LastOrDefault",
        "LongCount", "Max", "MaxBy", "Min", "MinBy",
        "Single", "SingleOrDefault", "Sum", "ToArray",
        "ToDictionary", "ToHashSet", "ToList", "ToLookup"
    ];

#pragma warning disable IDE0303 // Cannot simplify collection initialization because of netstandard2.0
    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics
        => ImmutableArray.Create(Rule);
#pragma warning restore IDE0303

    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecutionInReleaseMode();

#pragma warning disable RS0030 // Do not use banned APIs
        context.RegisterSyntaxNodeAction(AnalyzeInvocation, SyntaxKind.InvocationExpression);
#pragma warning restore RS0030 // Do not use banned APIs
    }

    private static void AnalyzeInvocation(SyntaxNodeAnalysisContext context)
    {
        var invocation = (InvocationExpressionSyntax)context.Node;

        if (context.SemanticModel.GetSymbolInfo(invocation, cancellationToken: context.CancellationToken).Symbol is not IMethodSymbol symbol)
        {
            return;
        }

        if (!ForbiddenMethods.Contains(symbol.Name, StringComparer.Ordinal))
        {
            return;
        }

        var receiverType = GetReceiverType(invocation, context.SemanticModel, context.CancellationToken);
        if (receiverType is null)
        {
            return;
        }

        if (!DerivesFromQueryableOrDbSet(receiverType))
        {
            return;
        }

        // Check if this invocation is inside an expression tree lambda (Expression<Func<T,...>>)
        if (IsPartOfExpressionTree(invocation, context.SemanticModel, context.CancellationToken))
        {
            return; // safe, do not report
        }

        var diagnostic = Diagnostic.Create(Rule, invocation.GetLocation(), symbol.Name);
        context.ReportDiagnostic(diagnostic);
    }

    private static bool IsPartOfExpressionTree(SyntaxNode node, SemanticModel model, CancellationToken cancellationToken)
    {
        // Walk up parent syntax nodes to see if we're inside a lambda passed where EF expects Expression<>
        var lambdaParent = node.Ancestors().OfType<LambdaExpressionSyntax>().FirstOrDefault();
        if (lambdaParent is null)
        {
            return false;
        }

        // Get type of the lambda itself
        var typeInfo = model.GetTypeInfo(lambdaParent, cancellationToken);
        var type = typeInfo.ConvertedType;

        if (type is null)
        {
            return false;
        }

        // Expression<Func<...>> case
        return string.Equals(type.ContainingNamespace.ToDisplayString(), "System.Linq.Expressions", StringComparison.Ordinal)
            && string.Equals(type.Name, "Expression", StringComparison.Ordinal);
    }

    private static ITypeSymbol? GetReceiverType(InvocationExpressionSyntax invocation, SemanticModel model, CancellationToken cancellationToken)
    {
        // Handle simple calls: dbContext.Users.ToList()
        if (invocation.Expression is MemberAccessExpressionSyntax memberAccess)
        {
            return model.GetTypeInfo(memberAccess.Expression, cancellationToken: cancellationToken).Type;
        }

        // Handle extension methods: Queryable.Count(users)
        if (invocation.Expression is IdentifierNameSyntax && invocation.ArgumentList.Arguments.Count > 0)
        {
            return model.GetTypeInfo(invocation.ArgumentList.Arguments[0].Expression, cancellationToken: cancellationToken).Type;
        }

        return null;
    }

    private static bool DerivesFromQueryableOrDbSet(ITypeSymbol type)
    {
        // Check IQueryable<> inheritance
        if (type.AllInterfaces.Any(i => string.Equals(i.Name, "IQueryable", StringComparison.Ordinal)))
        {
            return true;
        }

        // Check DbSet<>
        string fullName = type.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);
        return fullName.StartsWith("Microsoft.EntityFrameworkCore.DbSet<", StringComparison.Ordinal);
    }
}
