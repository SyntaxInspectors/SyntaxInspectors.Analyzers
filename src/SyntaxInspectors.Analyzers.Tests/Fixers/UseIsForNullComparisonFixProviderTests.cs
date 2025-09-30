using SyntaxInspectors.Analyzers.CodeFixers;
using SyntaxInspectors.Analyzers.Diagnosers.UseIsForNullComparison;

namespace SyntaxInspectors.Analyzers.Tests.Fixers;

public sealed class UseIsForNullComparisonFixProviderTests : CodeFixTestBase<UseIsForNullComparisonAnalyzer, UseIsForNullComparisonFixProvider>
{
    public UseIsForNullComparisonFixProviderTests(ITestOutputHelper testOutputHelper) : base(testOutputHelper)
    {
    }

    [Fact]
    public Task WhenEqualNullComparison_ThenReplace()
    {
        const string code = """
                            public static class Test
                            {
                                public static bool IsNull(object? obj)
                                    => obj {|SI0011:==|} null;
                            }
                            """;

        const string expectedFixedCode = """
                                         public static class Test
                                         {
                                             public static bool IsNull(object? obj)
                                                 => obj is null;
                                         }
                                         """;

        return ValidateAsync(code, expectedFixedCode);
    }

    [Fact]
    public Task WhenNotEqualNullComparison_ThenReplace()
    {
        const string code = """
                            public static class Test
                            {
                                public static bool IsNotNull(object? obj)
                                    => obj {|SI0011:!=|} null;
                            }
                            """;

        const string expectedFixedCode = """
                                         public static class Test
                                         {
                                             public static bool IsNotNull(object? obj)
                                                 => obj is not null;
                                         }
                                         """;

        return ValidateAsync(code, expectedFixedCode);
    }

    private Task ValidateAsync(string code, string expectedFixedCode)
        => CreateTestBuilder()
          .WithTestCode(code)
          .WithFixedCode(expectedFixedCode)
          .Build()
          .RunAsync();
}
