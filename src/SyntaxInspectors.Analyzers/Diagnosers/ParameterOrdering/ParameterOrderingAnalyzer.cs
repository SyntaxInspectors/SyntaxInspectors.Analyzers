using System.Collections.Immutable;
using SyntaxInspectors.Analyzers.Extensions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Diagnostics;

namespace SyntaxInspectors.Analyzers.Diagnosers.ParameterOrdering;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class ParameterOrderingAnalyzer : DiagnosticAnalyzer
{
    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics
        => ParameterOrderingAnalyzerImplementation.DiagnosticRules.Rules;

    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.Analyze);
        context.EnableConcurrentExecutionInReleaseMode();
        context.RegisterSyntaxNodeActionAndAnalyze<ParameterOrderingAnalyzerImplementation>(a => a.AnalyzeParameterList, SyntaxKind.ParameterList);
    }
}
