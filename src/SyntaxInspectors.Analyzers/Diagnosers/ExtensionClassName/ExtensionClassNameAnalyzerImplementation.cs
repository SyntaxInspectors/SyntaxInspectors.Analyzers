using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using SyntaxInspectors.Analyzers.Configuration;
using SyntaxInspectors.Analyzers.Support;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace SyntaxInspectors.Analyzers.Diagnosers.ExtensionClassName;

[SuppressMessage("ReSharper", "UseCollectionExpression", Justification = "Not supported in lower versions of Roslyn")]
internal sealed class ExtensionClassNameAnalyzerImplementation : SyntaxNodeAnalyzerImplementationBase<ExtensionClassNameAnalyzer>
{
    private readonly IAnalyzerConfiguration _configuration;

    public ExtensionClassNameAnalyzerImplementation(in SyntaxNodeAnalysisContext context) : base(context)
    {
        _configuration = GenericConfigurationProvider.GetConfiguration(context, DiagnosticRules.Default.DiagnosticId);
    }

    public void AnalyzeClassDeclaration()
    {
        if (!_configuration.IsEnabled)
        {
            return;
        }

        var classDeclaration = (ClassDeclarationSyntax)Context.Node;
        var className = classDeclaration.Identifier.Text;
        var endsWithExtensions = className.EndsWith("Extensions", StringComparison.Ordinal);
        if (endsWithExtensions)
        {
            return;
        }

        if (!ContainsOldStyleExtensionMethods(classDeclaration))
        {
            return;
        }

        var suggestedClassName = $"{className}Extensions";

        Context.ReportDiagnostic(Diagnostic.Create(DiagnosticRules.Default.Rule, classDeclaration.Identifier.GetLocation(), className, suggestedClassName));
    }

    private bool ContainsOldStyleExtensionMethods(ClassDeclarationSyntax classDeclaration)
        => classDeclaration
          .Members.OfType<MethodDeclarationSyntax>()
          .Any(a => Context.SemanticModel.GetDeclaredSymbol(a) is IMethodSymbol { IsExtensionMethod: true });

    internal static class DiagnosticRules
    {
        internal static ImmutableArray<DiagnosticDescriptor> Rules { get; }
            = CommonRules.AllCommonRules
                         .Append(Default.Rule)
                         .ToImmutableArray();

        internal static class Default
        {
            private const string Category = "Intention";
            public const string DiagnosticId = "SI0006";
            public static readonly string HelpLinkUri = HelpLinkFactory.CreateForDiagnosticId(DiagnosticId);
            public static readonly LocalizableString Title = "Classes containing extension methods should have an `Extensions` suffix";
            public static readonly LocalizableString MessageFormat = "Rename the class `{0}` to `{1}` to indicate it contains extension methods";
            public static readonly LocalizableString Description = MessageFormat;
            public static readonly DiagnosticDescriptor Rule = new(DiagnosticId, Title, MessageFormat, Category, DiagnosticSeverity.Warning, true, Description, HelpLinkUri);
        }
    }
}
