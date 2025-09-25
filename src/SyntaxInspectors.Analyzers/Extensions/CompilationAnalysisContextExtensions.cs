using SyntaxInspectors.Analyzers.Configuration;
using SyntaxInspectors.Analyzers.Diagnosers;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Text;

namespace SyntaxInspectors.Analyzers.Extensions;

internal static class CompilationAnalysisContextExtensions
{
    private const string EditorConfigFileName = ".editorconfig";

    public static void ReportConfigurationValidationError(this in SyntaxNodeAnalysisContext context, ConfigurationError error)
    {
        var path = GetConfigFileName(context);
        var linePositionSpan = new LinePositionSpan(new LinePosition(0, 0), new LinePosition(0, 0));
        var location = Location.Create(path, TextSpan.FromBounds(0, 0), linePositionSpan);
        var rule = Diagnostic.Create(CommonRules.InvalidConfigurationValue.Rule, location, error.KeyName, error.FilePath, error.Reason);
        context.ReportDiagnostic(rule);
    }

    private static string GetConfigFileName(in SyntaxNodeAnalysisContext context)
    {
        var file = context.Options.AdditionalFiles.FirstOrDefault(a => a.Path.Contains(EditorConfigFileName, StringComparison.OrdinalIgnoreCase));
        return file?.Path ?? EditorConfigFileName;
    }
}
