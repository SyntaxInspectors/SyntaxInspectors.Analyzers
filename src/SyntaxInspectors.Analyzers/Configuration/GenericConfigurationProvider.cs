using SyntaxInspectors.Analyzers.Extensions;
using Microsoft.CodeAnalysis.Diagnostics;

namespace SyntaxInspectors.Analyzers.Configuration;

internal static class GenericConfigurationProvider
{
    public static IAnalyzerConfiguration GetConfiguration(in SyntaxNodeAnalysisContext context, string diagnosticId)
    {
        var isEnabled = IsDiagnosticEnabled(context, diagnosticId);
        return isEnabled
            ? GenericConfiguration.Enabled
            : GenericConfiguration.Disabled;
    }

    private static bool IsDiagnosticEnabled(in SyntaxNodeAnalysisContext context, string diagnosticId)
    {
        var keyName = GenericConfigurationKeyNames.CreateIsEnabledKeyName(diagnosticId);
        return context.GetOptionsBooleanValue(keyName, defaultValue: true);
    }
}
