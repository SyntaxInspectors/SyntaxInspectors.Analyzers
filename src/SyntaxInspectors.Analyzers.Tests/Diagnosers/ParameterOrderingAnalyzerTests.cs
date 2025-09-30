using System.Diagnostics.CodeAnalysis;
using SyntaxInspectors.Analyzers.Diagnosers.ParameterOrdering;
using Microsoft.CodeAnalysis.CSharp.Testing;
using Microsoft.CodeAnalysis.Testing;
using SyntaxInspectors.Analyzers.Configuration.Si0007;

namespace SyntaxInspectors.Analyzers.Tests.Diagnosers;

[SuppressMessage("Code Smell", "S2699:Tests should include assertions", Justification = "This is done internally by AnalyzerTest.RunAsync()")]
public sealed class ParameterOrderingAnalyzerTests(ITestOutputHelper testOutputHelper)
    : TestBase<ParameterOrderingAnalyzer>(testOutputHelper)
{
    [Theory]
    [InlineData("(string value)")]
    [InlineData("{|SI0007:(ILogger logger, string value)|}")]
    [InlineData("{|SI0007:(ILogger<TestClass> logger, string value, CancellationToken cancellationToken)|}")]
    [InlineData("{|SI0007:(CancellationToken cancellationToken, ILogger logger, string value)|}")]
    [InlineData("(string value, ILogger logger, CancellationToken cancellationToken, params string[] values)")]
    public Task Theory_OnMethod(string parameters)
    {
        var code = $$"""
                     using System.Threading;
                     using Microsoft.Extensions.Logging;

                     public class TestClass
                     {
                         public TestClass{{parameters}} // constructor
                         {
                         }

                         public void Test{{parameters}} // method
                         {
                         }
                     }
                     """;

        return CreateTester(code, true).RunAsync(TestContext.Current.CancellationToken);
    }

    [Theory]
    [InlineData(true, "{|SI0007:(ILogger<TestClass> logger, string value)|}")]
    [InlineData(false, "(ILogger<TestClass> logger, string value)")]
    public Task Theory_IsEnabled(bool isEnabled, string parameterCode)
    {
        var code = $$"""
                     using System.Threading;
                     using Microsoft.Extensions.Logging;

                     public class TestClass
                     {
                         public TestClass{{parameterCode}}
                         {
                         }
                     }
                     """;

        return CreateTester(code, isEnabled).RunAsync(TestContext.Current.CancellationToken);
    }

    private CSharpAnalyzerTest<ParameterOrderingAnalyzer, DefaultVerifier> CreateTester(string code, bool isEnabled, string? configValueForLoggerParameterPlacement = null)
        => CreateTesterBuilder()
          .WithTestCode(code)
          .WithNugetPackage("Microsoft.Extensions.Logging.Abstractions", "9.0.8")
          .SetEnabled(isEnabled, "SI0007")
          .WithEditorConfigLine($"{Si0007Configuration.KeyNames.ParameterOrderingFlat} = {configValueForLoggerParameterPlacement ?? string.Empty}")
          .Build();
}
