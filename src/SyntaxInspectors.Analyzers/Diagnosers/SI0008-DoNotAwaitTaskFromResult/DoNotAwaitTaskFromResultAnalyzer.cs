using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Diagnostics;
using SyntaxInspectors.Analyzers.Extensions;

namespace SyntaxInspectors.Analyzers.Diagnosers.DoNotAwaitTaskFromResult;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class DoNotAwaitTaskFromResultAnalyzer : DiagnosticAnalyzer
{
    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics
        => DoNotAwaitTaskFromResultAnalyzerImplementation.DiagnosticRules.Rules;

    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.Analyze);
        context.EnableConcurrentExecutionInReleaseMode();
        context.RegisterSyntaxNodeActionAndAnalyze<DoNotAwaitTaskFromResultAnalyzerImplementation>(a => a.AnalyzeAwait, SyntaxKind.AwaitExpression);
    }
}
