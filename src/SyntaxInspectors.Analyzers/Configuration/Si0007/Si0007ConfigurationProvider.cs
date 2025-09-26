using Microsoft.CodeAnalysis.Diagnostics;
using SyntaxInspectors.Analyzers.Extensions;

namespace SyntaxInspectors.Analyzers.Configuration.Si0007;

internal sealed class Si0007ConfigurationProvider : ConfigurationProvider<Si0007Configuration>
{
    public static Si0007ConfigurationProvider Instance { get; } = new();

    private Si0007ConfigurationProvider()
    {
    }

    protected override Si0007Configuration GetConfigurationCore(AnalyzerConfigOptions options)
    {
        if (!options.IsDiagnosticEnabled("SI0007"))
        {
            return Si0007Configuration.Disabled;
        }

        var (parameterOrder, parameterOrderFlat) = GetParameterOrdering(options);
        if (parameterOrder.Count == 0)
        {
            return Si0007Configuration.Default;
        }

        var firstDuplicate = parameterOrder
                            .GroupBy(a => a, StringComparer.OrdinalIgnoreCase)
                            .Where(a => a.Count() > 1)
                            .Select(a => a.Key)
                            .FirstOrDefault();

        if (firstDuplicate is not null)
        {
            var error = new ConfigurationError(Si0007Configuration.KeyNames.ParameterOrderingFlat, ".editorconfig", $"Duplicate value: {firstDuplicate}");
            return new Si0007Configuration(error);
        }

        return new Si0007Configuration(true, parameterOrderFlat, ParameterOrderParser.Parse(parameterOrder));
    }

    private static (IReadOnlyList<string> ParameterOrder, string ParameterOrderFlat) GetParameterOrdering(AnalyzerConfigOptions options)
    {
        var value = options.GetOptionsValueOrDefault(Si0007Configuration.KeyNames.ParameterOrderingFlat);
        return value.IsNullOrWhiteSpace()
            ? ([], string.Empty)
            : (ParameterOrderParser.SplitConfigurationParameterOrder(value), value);
    }
}
