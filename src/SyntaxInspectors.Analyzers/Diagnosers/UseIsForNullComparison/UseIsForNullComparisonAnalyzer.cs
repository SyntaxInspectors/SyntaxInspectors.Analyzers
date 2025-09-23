using System.Collections.Immutable;
using SyntaxInspectors.Analyzers.Extensions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Diagnostics;

namespace SyntaxInspectors.Analyzers.Diagnosers.UseIsForNullComparison;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class UseIsForNullComparisonAnalyzer : DiagnosticAnalyzer
{
    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics
        => UseIsForNullComparisonAnalyzerImplementation.DiagnosticRules.Rules;

    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.Analyze);
        context.EnableConcurrentExecutionInReleaseMode();

        context.RegisterSyntaxNodeActionAndAnalyze<UseIsForNullComparisonAnalyzerImplementation>(a => a.AnalyzeBinaryExpression, SyntaxKind.EqualsExpression, SyntaxKind.NotEqualsExpression);
    }
}
