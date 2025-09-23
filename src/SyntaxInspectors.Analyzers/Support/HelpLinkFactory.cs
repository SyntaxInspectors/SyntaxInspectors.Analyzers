namespace SyntaxInspectors.Analyzers.Support;

public static class HelpLinkFactory
{
    public static string CreateForDiagnosticId(string diagnosticId)
        => $"https://github.com/SyntaxInspectors/SyntaxInspectors.Analyzers/blob/main/docs/Rules/{diagnosticId}.md";
}
