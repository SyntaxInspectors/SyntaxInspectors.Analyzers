using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Diagnostics;
using SyntaxInspectors.Analyzers.Extensions;

namespace SyntaxInspectors.Analyzers.Diagnosers.ExtensionClassName;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class ExtensionClassNameAnalyzer : DiagnosticAnalyzer
{
    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics
        => ExtensionClassNameAnalyzerImplementation.DiagnosticRules.Rules;

    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.Analyze);
        context.EnableConcurrentExecutionInReleaseMode();

        context.RegisterSyntaxNodeActionAndAnalyze<ExtensionClassNameAnalyzerImplementation>(a => a.AnalyzeClassDeclaration, SyntaxKind.ClassDeclaration);
    }
}
