using System.Collections.Concurrent;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using System.Text.RegularExpressions;
using SyntaxInspectors.Analyzers.Extensions;

namespace SyntaxInspectors.Analyzers.Configuration.Si0007;

[SuppressMessage("ReSharper", "UseCollectionExpression", Justification = "Not supported in lower versions of Roslyn")]
internal static class ParameterOrderParser
{
    private static readonly char[] ParameterSplittingChars = ['|'];
    private static readonly Regex AlwaysFalseRegex = new(Regex.Escape(@"\A\\?"), RegexOptions.Compiled, TimeSpan.FromSeconds(1));
    private static readonly ConcurrentDictionary<string, Regex> RegexCache = new(StringComparer.Ordinal);

    public static IReadOnlyList<string> SplitConfigurationParameterOrder(string parameterOrder)
        => parameterOrder.Split(ParameterSplittingChars, StringSplitOptions.RemoveEmptyEntries);

    public static ImmutableArray<ParameterDescription> Parse(string parameterOrder)
        => Parse(SplitConfigurationParameterOrder(parameterOrder));

    public static ImmutableArray<ParameterDescription> Parse(IEnumerable<string> parameters)
        => parameters
          .Select((a, index)
               => a.EqualsOrdinal(Si0007Configuration.Placeholders.Other)
                   ? new ParameterDescription(AlwaysFalseRegex, index, true)
                   : new ParameterDescription(TransformToRegex(a), index, false))
          .ToImmutableArray();

    private static Regex TransformToRegex(string expression)
        => RegexCache.GetOrAdd(expression, TransformToRegexCore);

    private static Regex TransformToRegexCore(string expression)
    {
        var pattern = @$"\A{Regex.Escape(expression).Replace(@"\*", ".*", StringComparison.Ordinal)}\z";
        return new Regex(pattern, RegexOptions.Compiled, TimeSpan.FromSeconds(1));
    }
}
