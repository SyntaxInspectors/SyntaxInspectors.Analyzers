using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.Diagnostics;

namespace SyntaxInspectors.Analyzers.Configuration;

internal abstract class ConfigurationProvider<T> : IConfigurationProvider<T>
    where T : class, IAnalyzerConfiguration
{
    public async Task<T?> GetConfigurationAsync(CodeFixContext context)
    {
        var syntaxTree = await context.Document.GetSyntaxTreeAsync().ConfigureAwait(false);
        if (syntaxTree is null)
        {
            return null;
        }

        var options = context.Document.Project.AnalyzerOptions.AnalyzerConfigOptionsProvider.GetOptions(syntaxTree);
        return GetConfiguration(options);
    }

    public T GetConfiguration(in SyntaxNodeAnalysisContext context)
    {
        var options = context.Options.AnalyzerConfigOptionsProvider.GetOptions(context.Node.SyntaxTree);
        return GetConfiguration(options);
    }

    public T GetConfiguration(AnalyzerConfigOptions options)
    {
        var configuration = GetConfigurationCore(options);
        return configuration.ConfigurationError is null ? configuration : null;
    }

    protected abstract T GetConfigurationCore(AnalyzerConfigOptions options);
}
