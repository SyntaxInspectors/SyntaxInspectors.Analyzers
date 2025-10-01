using Microsoft.CodeAnalysis.Diagnostics;
using SyntaxInspectors.Analyzers.Extensions;

namespace SyntaxInspectors.Analyzers.Configuration.Si0002;

internal sealed class Si0002ConfigurationProvider : ConfigurationProvider<Si0002Configuration>
{
    public static Si0002ConfigurationProvider Instance { get; } = new();

    private Si0002ConfigurationProvider()
    {
    }

    protected override Si0002Configuration GetConfigurationCore(AnalyzerConfigOptions options)
    {
        if (!options.IsDiagnosticEnabled("SI0002"))
        {
            return Si0002Configuration.Disabled;
        }

        try
        {
            var mode = GetMode(options);
            return mode switch
            {
                Mode.Strict => Si0002Configuration.Strict,
                Mode.Relaxed => Si0002Configuration.Relaxed,
                _ => throw new InvalidOperationException($"Unknown mode: {mode}")
            };
        }
        catch (Exception ex)
        {
            var error = ConfigurationError.CreateWithDefaultEditorConfigFile(Si0002Configuration.KeyNames.ParameterOrderingFlat, ex.Message);
            return new Si0002Configuration(error);
        }
    }

    private static Mode GetMode(AnalyzerConfigOptions options)
    {
        var value = options.GetOptionsValueOrDefault(Si0002Configuration.KeyNames.ParameterOrderingFlat);
        if (value.IsNullOrWhiteSpace())
        {
            return Mode.Strict;
        }

        return Enum.TryParse<Mode>(value, ignoreCase: true, out var mode)
            ? mode
            : throw new InvalidOperationException($"Unknown mode: {value}");
    }
}
