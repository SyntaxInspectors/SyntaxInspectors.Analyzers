using System.Diagnostics.CodeAnalysis;
using SyntaxInspectors.Analyzers.Tests.Helpers;
using Microsoft.CodeAnalysis.Diagnostics;
using Xunit.Abstractions;

namespace SyntaxInspectors.Analyzers.Tests;

[SuppressMessage("Maintainability", "CA1515:Because an application's API isn't typically referenced from outside the assembly, types can be made internal", Justification = "This is the base class for our unit tests")]
public abstract class TestBase<TAnalyzer>
    where TAnalyzer : DiagnosticAnalyzer, new()
{
    protected ITestOutputHelper TestOutputHelper { get; }

    protected TestBase(ITestOutputHelper testOutputHelper)
    {
        TestOutputHelper = testOutputHelper;
    }

    private protected CSharpAnalyzerTestBuilder<TAnalyzer> CreateTesterBuilder() => CSharpAnalyzerTestBuilder.Create<TAnalyzer>(TestOutputHelper);
}
