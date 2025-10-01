using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using System.Collections.Immutable;
using SyntaxInspectors.Analyzers.Extensions;

namespace SyntaxInspectors.Analyzers;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class AsNoTrackingShouldBeFirstAnalyzer : DiagnosticAnalyzer
{
    private const string DiagnosticIdAsNoTracking = "SI1001";
    private static readonly DiagnosticDescriptor Rule = new(
        DiagnosticIdAsNoTracking,
        "AsNoTracking() should be first in the chain",
        "AsNoTracking() must be the first call in the expression chain",
        "Usage",
        DiagnosticSeverity.Warning,
        isEnabledByDefault: true);

#pragma warning disable IDE0303 // Cannot simplify collection initialization because of netstandard2.0
    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(Rule);
#pragma warning restore IDE0303

    public override void Initialize(AnalysisContext context)
    {
        context.EnableConcurrentExecutionInReleaseMode();
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);

#pragma warning disable RS0030 // Do not use banned APIs
        context.RegisterSyntaxNodeAction(AnalyzeInvocation, SyntaxKind.InvocationExpression);
#pragma warning restore RS0030 // Do not use banned APIs
    }

    private static void AnalyzeInvocation(SyntaxNodeAnalysisContext context)
    {
        var invocation = (Microsoft.CodeAnalysis.CSharp.Syntax.InvocationExpressionSyntax)context.Node;
        if (invocation.Expression is not Microsoft.CodeAnalysis.CSharp.Syntax.MemberAccessExpressionSyntax memberAccess)
        {
            return;
        }

        if (!string.Equals(memberAccess.Name.Identifier.Text, "AsNoTracking", StringComparison.Ordinal))
        {
            return;
        }

        // Get symbol to ensure it's the EF Core AsNoTracking
        var symbol = context.SemanticModel.GetSymbolInfo(memberAccess, cancellationToken: context.CancellationToken).Symbol;
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

        // Traverse backwards to see if it's FIRST after DbSet  
        var expr = memberAccess.Expression;
        while (expr is Microsoft.CodeAnalysis.CSharp.Syntax.InvocationExpressionSyntax prevInvocation)
        {
            expr = (prevInvocation.Expression as Microsoft.CodeAnalysis.CSharp.Syntax.MemberAccessExpressionSyntax)?.Expression;
        }

        // Now 'expr' is the root, typically a DbSet property  
        if (expr is null)
        {
            return;
        }

        var typeInfo = context.SemanticModel.GetTypeInfo(expr, cancellationToken: context.CancellationToken).Type;

        // Ensure it's a DbSet or valid for AsNoTracking  
        if (typeInfo?.Name.Contains("DbSet", StringComparison.Ordinal) != true)
        {
            return;
        }

        // Check if the immediate parent of AsNoTracking is not a DbSet — this means it's not first  
        if (!(memberAccess.Expression is Microsoft.CodeAnalysis.CSharp.Syntax.IdentifierNameSyntax
            || (memberAccess.Expression is Microsoft.CodeAnalysis.CSharp.Syntax.MemberAccessExpressionSyntax rootMember && (context.SemanticModel.GetTypeInfo(rootMember, cancellationToken: context.CancellationToken).Type?.Name.Contains("DbSet", StringComparison.Ordinal) ?? false))))
        {
            context.ReportDiagnostic(Diagnostic.Create(Rule, memberAccess.Name.GetLocation()));
        }
    }
}
