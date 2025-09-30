using System.Diagnostics.CodeAnalysis;
using SyntaxInspectors.Analyzers.Diagnosers.UseIsForNullComparison;
using Xunit.Abstractions;

namespace SyntaxInspectors.Analyzers.Tests.Diagnosers;

[SuppressMessage("Code Smell", "S2699:Tests should include assertions", Justification = "This is done internally by AnalyzerTest.RunAsync()")]
public sealed class UseIsForNullComparisonAnalyzerTests(ITestOutputHelper testOutputHelper)
    : TestBase<UseIsForNullComparisonAnalyzer>(testOutputHelper)
{
    [Theory]
    [InlineData(@"_ = value == ""a"";")]
    [InlineData(@"_ = value != ""a"";")]
    [InlineData("int? a = 303; _ = a == null;")]
    [InlineData("int? a = 303; _ = a != null;")]
    [InlineData("_ = value {|SI0011:==|} null;")]
    [InlineData("_ = value {|SI0011:!=|} null;")]
    public Task Theory(string insertionCode)
    {
        var code = $$"""
                     public static class Test
                     {
                         public static void DoSomething(string value)
                         {
                             {{insertionCode}}
                         }
                     }
                     """;

        return ValidateAsync(code);
    }

    [Fact]
    public Task WhenIsInQueryableWhere_ThenOk()
    {
        const string code = """
                            using System.Linq;

                            public static class Test
                            {
                                public static void DoSomething(IQueryable<string> values)
                                {
                                    var _ = values.Where(a => a == null);
                                }
                            }
                            """;

        return ValidateAsync(code);
    }

    [Theory]
    [InlineData(true, "var isNull = value {|SI0011:==|} null;")]
    [InlineData(false, "var isNull = value == null;")]
    public Task Theory_IsEnabled(bool isEnabled, string insertionCode)
    {
        var code = $$"""
                     public static class Test
                     {
                         public static void DoSomething(string value)
                         {
                            {{insertionCode}}
                         }
                     }
                     """;

        return ValidateAsync(code, isEnabled);
    }

    [Fact]
    public Task InsideLinq_WithEnumerable_ThenDiagnose()
    {
        const string code = """
                            using System.Collections.Generic;
                            using System.Linq;

                            public static class Test
                            {
                                public static void DoSomething(IEnumerable<string?> values)
                                {
                                    var _ =
                                        from v in values
                                        join v2 in values on v equals v2
                                        where v {|SI0011:==|} null
                                        select v;
                                }
                            }
                            """;

        return ValidateAsync(code);
    }

    [Fact]
    public Task InsideLinq_WithQueryable_ThenOk()
    {
        const string code = """
                            using System.Collections.Generic;
                            using System.Linq;

                            public static class Test
                            {
                                public static void DoSomething(IQueryable<string?> values)
                                {
                                    var _ =
                                        from v in values
                                        join v2 in values on v equals v2
                                        where v == null
                                        select v;
                                }
                            }
                            """;

        return ValidateAsync(code);
    }

    private Task ValidateAsync(string code)
        => ValidateAsync(code, true);

    private Task ValidateAsync(string code, bool isEnabled)
        => CreateTesterBuilder()
          .WithTestCode(code)
          .SetEnabled(isEnabled, "SI0011")
          .Build()
          .RunAsync();
}
