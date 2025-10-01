using System.Collections.Immutable;

namespace SyntaxInspectors.Analyzers.Configuration.Si0007;

internal sealed class Si0007Configuration : IAnalyzerConfiguration
{
    public static Si0007Configuration Default { get; } = new(true, Defaults.ParameterOrderFlat, Defaults.ParameterDescriptions);
    public static Si0007Configuration Disabled { get; } = new(false, Defaults.ParameterOrderFlat, ImmutableArray<ParameterDescription>.Empty);

    public bool IsEnabled { get; }
    public ConfigurationError? ConfigurationError { get; }
    public string ParameterOrderFlat { get; }
    public ImmutableArray<ParameterDescription> ParameterDescriptions { get; }

    public Si0007Configuration(bool isEnabled, string parameterOrderFlat, in ImmutableArray<ParameterDescription> parameterDescriptions)
    {
        IsEnabled = isEnabled;
        ParameterOrderFlat = parameterOrderFlat;
        ParameterDescriptions = parameterDescriptions;
    }

    public Si0007Configuration(ConfigurationError configurationError)
    {
        ConfigurationError = configurationError;
        ParameterOrderFlat = string.Empty;
        IsEnabled = false;
        ParameterDescriptions = ImmutableArray<ParameterDescription>.Empty;
    }

    public static class KeyNames
    {
        public const string IsEnabled = "SI0007.is_enabled";
        public const string ParameterOrderingFlat = "SI0007.parameter_ordering";
    }

    internal static class Placeholders
    {
        public const string Other = "{other}";
    }

    internal static class Defaults
    {
        public const string ParameterOrderFlat = $"{Placeholders.Other}|Microsoft.Extensions.Logging*|System.Threading.CancellationToken";
        public static ImmutableArray<ParameterDescription> ParameterDescriptions { get; } = ParameterOrderParser.Parse(ParameterOrderFlat);
    }
}
