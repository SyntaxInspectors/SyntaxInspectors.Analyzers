using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using SyntaxInspectors.Analyzers.Configuration;
using SyntaxInspectors.Analyzers.Logging;
using SyntaxInspectors.Analyzers.Support;

namespace SyntaxInspectors.Analyzers.Diagnosers.WarningSuppression;

[SuppressMessage("ReSharper", "UseCollectionExpression", Justification = "Not supported in lower versions of Roslyn")]
internal sealed class GeneralWarningSuppressionAnalyzerImplementation : SyntaxNodeAnalyzerImplementationBase<GeneralWarningSuppressionAnalyzerImplementation>
{
    private readonly IAnalyzerConfiguration _configuration;

    public GeneralWarningSuppressionAnalyzerImplementation(in SyntaxNodeAnalysisContext context) : base(context)
    {
        _configuration = GenericConfigurationProvider.GetConfiguration(context, DiagnosticRules.Default.DiagnosticId);
    }

    public void AnalyzePragma()
    {
        if (!_configuration.IsEnabled)
        {
            return;
        }

        var directive = (PragmaWarningDirectiveTriviaSyntax)Context.Node;
        if (directive.ErrorCodes.Count == 0)
        {
            Logger.ReportDiagnostic(DiagnosticRules.Default.Rule, directive.GetLocation());
            Context.ReportDiagnostic(Diagnostic.Create(DiagnosticRules.Default.Rule, directive.GetLocation()));
        }
    }

    internal static class DiagnosticRules
    {
        internal static ImmutableArray<DiagnosticDescriptor> Rules { get; }
            = CommonRules.AllCommonRules
                         .Append(Default.Rule)
                         .ToImmutableArray();

        internal static class Default
        {
            private const string Category = "Code Smell";
            public const string DiagnosticId = "SI0005";
            public static readonly string HelpLinkUri = HelpLinkFactory.CreateForDiagnosticId(DiagnosticId);
            public static readonly LocalizableString Title = "Do not use general warning suppression";
            public static readonly LocalizableString MessageFormat = Title;
            public static readonly LocalizableString Description = MessageFormat;
            public static readonly DiagnosticDescriptor Rule = new(DiagnosticId, Title, MessageFormat, Category, DiagnosticSeverity.Error, true, Description, HelpLinkUri);
        }
    }
}
