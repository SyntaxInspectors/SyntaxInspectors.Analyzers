namespace SyntaxInspectors.Analyzers.Configuration.Si0002;

internal sealed class Si0002Configuration : IAnalyzerConfiguration
{
    public static Si0002Configuration Default { get; } = new(true, Mode.Strict);
    public static Si0002Configuration Disabled { get; } = new(false, Mode.Strict);
    public static Si0002Configuration Strict { get; } = new(true, Mode.Strict);
    public static Si0002Configuration Relaxed { get; } = new(true, Mode.Relaxed);

    public bool IsEnabled { get; }
    public Mode Mode { get; }
    public ConfigurationError? ConfigurationError { get; }

    public Si0002Configuration(bool isEnabled, Mode mode)
    {
        IsEnabled = isEnabled;
        Mode = mode;
    }

    public Si0002Configuration(ConfigurationError configurationError)
    {
        ConfigurationError = configurationError;
        IsEnabled = false;
    }

    public static class KeyNames
    {
        public const string IsEnabled = "SI0002.is_enabled";
        public const string ParameterOrderingFlat = "SI0002.mode";
    }
}
