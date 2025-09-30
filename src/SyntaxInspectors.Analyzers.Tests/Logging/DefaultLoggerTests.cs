using System.Diagnostics.CodeAnalysis;
using SyntaxInspectors.Analyzers.Logging;
using Shouldly;

namespace SyntaxInspectors.Analyzers.Tests.Logging;

public sealed class DefaultLoggerTests
{
    [Fact]
    public void WriteLine_ShouldContainAllInformation()
    {
        // arrange
        var sut = new DefaultLogger<DefaultLoggerTests>(LogLevel.Full);
        EnsureLogFileIsDeleted();

        // act
        sut.WriteLine(LogLevel.Full, "test1");

        // assert
        var logFileContent = GetLogFileContent();
        logFileContent.ShouldNotBeEmpty();
        logFileContent.ShouldContain("Context=DefaultLoggerTests");
        logFileContent.ShouldContain($"Method={nameof(WriteLine_ShouldContainAllInformation)}");
        logFileContent.ShouldContain("Message=test1");
        logFileContent.ShouldContain($"PID={Environment.ProcessId}");
        logFileContent.ShouldContain($"TID={Environment.CurrentManagedThreadId}");
    }

    [SuppressMessage("Dunno", "MA0045:Do not use blocking calls in a sync method (need to make calling method async)", Justification = "We're in non-async context here")]
    private static string GetLogFileContent() =>
        File.ReadAllText(DefaultLogger.FilePath);

    private static void EnsureLogFileIsDeleted()
    {
        if (File.Exists(DefaultLogger.FilePath))
        {
            File.Delete(DefaultLogger.FilePath);
        }
    }
}
