using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using Microsoft.CodeAnalysis;
using SyntaxInspectors.Analyzers.Support;

namespace SyntaxInspectors.Analyzers.Diagnosers;

[SuppressMessage("Security", "S1075:Refactor your code not to use hardcoded absolution paths or URIs", Justification = "Path to the documentation")]
[SuppressMessage("ReSharper", "UseCollectionExpression", Justification = "Not supported in lower versions of Roslyn")]
internal static class CommonRules
{
    public static ImmutableArray<DiagnosticDescriptor> AllCommonRules { get; } = new[]
    {
        UnhandledError.Rule,
        InvalidConfigurationValue.Rule
    }.ToImmutableArray();

    public static class UnhandledError
    {
        private const string Category = "Warning";
        public const string DiagnosticId = "SI9999";
        public static readonly string HelpLinkUri = HelpLinkFactory.CreateForDiagnosticId(DiagnosticId);
        public static readonly LocalizableString Title = "The SyntaxInspectors.Analyzers package encountered an error";
        public static readonly LocalizableString MessageFormat = "An error occurred in the SyntaxInspectors.Analyzers package. Check the log files in the 'SyntaxInspectors.Analyzers' subfolder of the temp folder.";
        public static readonly LocalizableString Description = Title;
        public static readonly DiagnosticDescriptor Rule = new(DiagnosticId, Title, MessageFormat, Category, DiagnosticSeverity.Warning, true, Description, HelpLinkUri);
    }

    public static class InvalidConfigurationValue
    {
        private const string Category = "Error";
        public const string DiagnosticId = "SI9998";
        public static readonly string HelpLinkUri = HelpLinkFactory.CreateForDiagnosticId(DiagnosticId);
        public static readonly LocalizableString Title = "Invalid SyntaxInspectors.Analyzers configuration value";
        public static readonly LocalizableString MessageFormat = "The configuration entry '{0}' in file '{1}' is invalid because: {2}";
        public static readonly LocalizableString Description = Title;
        public static readonly DiagnosticDescriptor Rule = new(DiagnosticId, Title, MessageFormat, Category, DiagnosticSeverity.Warning, true, Description, HelpLinkUri, "CompilationEnd");
    }
}
