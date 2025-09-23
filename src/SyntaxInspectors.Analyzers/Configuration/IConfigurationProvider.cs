using Microsoft.CodeAnalysis.Diagnostics;

namespace SyntaxInspectors.Analyzers.Configuration;

internal interface IConfigurationProvider<out T>
    where T : class, IAnalyzerConfiguration
{
    T GetConfiguration(AnalyzerConfigOptions options);
}
