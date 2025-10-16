using System.Diagnostics.CodeAnalysis;
using SyntaxInspectors.Analyzers.Diagnosers.AsNoTrackingShouldBeFirst;

namespace SyntaxInspectors.Analyzers.Tests.Diagnosers;

[SuppressMessage("Code Smell", "S2699:Tests should include assertions", Justification = "This is done internally by AnalyzerTest.RunAsync()")]
public sealed class AsNoTrackingShouldBeFirstAnalyzerTests(ITestOutputHelper testOutputHelper)
    : TestBase<AsNoTrackingShouldBeFirstAnalyzer>(testOutputHelper)
{
    [Fact]
    public async Task AsNoTracking_WhenFirstInChain_ThenOk()
    {
        const string code = """
                            using var dbContext = new TestContext();
                            dbContext.Entities.AsNoTracking().Where(e => e.Id > 0).ToList();
                            """;

        await RunTestAsync(code);
    }

    [Fact]
    public async Task AsNoTracking_WhenNotFirstInChain_ThenDiagnose()
    {
        const string code = """
                            using var dbContext = new TestContext();
                            dbContext.Entities.Where(e => e.Id > 0).{|SIEF0001:AsNoTracking|}().ToList();
                            """;

        await RunTestAsync(code);
    }

    [Fact]
    public async Task AsNoTracking_WhenAfterMultipleMethods_ThenDiagnose()
    {
        const string code = """
                            using var dbContext = new TestContext();
                            dbContext.Entities.Where(e => e.Id > 0).OrderBy(e => e.Name).{|SIEF0001:AsNoTracking|}().ToList();
                            """;

        await RunTestAsync(code);
    }

    [Fact]
    public async Task AsNoTracking_WhenDirectlyOnDbSet_ThenOk()
    {
        const string code = """
                            using var dbContext = new TestContext();
                            dbContext.Entities.AsNoTracking().ToList();
                            """;

        await RunTestAsync(code);
    }

    [Fact]
    public async Task AsNoTracking_WhenOnPropertyAccess_ThenOk()
    {
        const string code = """
                            using var dbContext = new TestContext();
                            var entities = dbContext.Entities.AsNoTracking();
                            """;

        await RunTestAsync(code);
    }

    [Fact]
    public async Task AsNoTracking_WhenInMiddleOfLongChain_ThenDiagnose()
    {
        const string code = """
                            using var dbContext = new TestContext();
                            dbContext.Entities
                                .Where(e => e.Id > 0)
                                .{|SIEF0001:AsNoTracking|}()
                                .Select(e => e.Name)
                                .ToList();
                            """;

        await RunTestAsync(code);
    }

    [Fact]
    public async Task NoAsNoTracking_WhenNotCalled_ThenOk()
    {
        const string code = """
                            using var dbContext = new TestContext();
                            dbContext.Entities.Where(e => e.Id > 0).ToList();
                            """;

        await RunTestAsync(code);
    }

    [Fact]
    public async Task AsNoTracking_WhenOnNonDbSetType_ThenOk()
    {
        const string code = """
                            var list = new List<Entity> { new Entity { Id = 1, Name = "Test" } };
                            var query = list.AsQueryable().Where(e => e.Id > 0).ToList();
                            """;

        await RunTestAsync(code);
    }

    private static string CreateTestCode(string insertionCode)
    {
        return $$"""
                 #nullable enable

                 using System;
                 using System.Collections.Generic;
                 using System.Linq;
                 using Microsoft.EntityFrameworkCore;

                 namespace Tests;

                 public sealed class Entity
                 {
                     public int    Id   { get; set; }
                     public string Name { get; set; } = null!;
                 }

                 public sealed class TestContext : DbContext
                 {
                     public DbSet<Entity> Entities { get; set; } = null!;
                 }

                 public static class Test
                 {
                     public static void TestMethod()
                     {
                         {{insertionCode}}
                     }
                 }
                 """;
    }

    private async Task RunTestAsync(string insertionCode)
    {
        var code = CreateTestCode(insertionCode);

        await CreateTesterBuilder()
             .WithTestCode(code)
             .WithNugetPackage("Microsoft.EntityFrameworkCore", "9.0.8")
             .Build()
             .RunAsync();
    }
}
