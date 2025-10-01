using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Diagnostics;
using SyntaxInspectors.Analyzers.Extensions;

namespace SyntaxInspectors.Analyzers.Diagnosers.WarningSuppression;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class GeneralWarningSuppressionAnalyzer : DiagnosticAnalyzer
{
    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics
        => GeneralWarningSuppressionAnalyzerImplementation.DiagnosticRules.Rules;

    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.Analyze);
        context.EnableConcurrentExecutionInReleaseMode();
        context.RegisterSyntaxNodeActionAndAnalyze<GeneralWarningSuppressionAnalyzerImplementation>(a => a.AnalyzePragma, SyntaxKind.PragmaWarningDirectiveTrivia);
    }
}
