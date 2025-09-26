namespace SyntaxInspectors.Analyzers.Support;

public static class HelpLinkFactory
{
    public static string CreateForDiagnosticId(string diagnosticId)
        => $"https://github.com/SyntaxInspectors/SyntaxInspectors.Analyzers/tree/main/docs/Rules/SI0001.md{diagnosticId}.md";
}
