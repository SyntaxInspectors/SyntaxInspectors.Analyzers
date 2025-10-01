using Microsoft.CodeAnalysis.Diagnostics;
using SyntaxInspectors.Analyzers.Extensions;
using SyntaxInspectors.Analyzers.Logging;

namespace SyntaxInspectors.Analyzers.Diagnosers;

internal abstract class SyntaxNodeAnalyzerImplementationBase<TImplementation>
    where TImplementation : class
{
    protected ILogger<TImplementation>? Logger { get; }
    protected SyntaxNodeAnalysisContext Context { get; }

    protected SyntaxNodeAnalyzerImplementationBase(in SyntaxNodeAnalysisContext context)
    {
        Context = context;
        Logger = context.CreateLogger<TImplementation>();
    }
}
