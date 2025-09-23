namespace SyntaxInspectors.Analyzers.Configuration;

public static class GenericConfigurationKeyNames
{
    public static string CreateIsEnabledKeyName(string diagnosticId) => $"{diagnosticId}.is_enabled";
}
