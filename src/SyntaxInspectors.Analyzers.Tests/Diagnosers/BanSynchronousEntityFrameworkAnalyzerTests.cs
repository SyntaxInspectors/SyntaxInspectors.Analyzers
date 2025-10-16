using System.Diagnostics.CodeAnalysis;
using SyntaxInspectors.Analyzers.Diagnosers.BanSynchronousEntityFramework;

namespace SyntaxInspectors.Analyzers.Tests.Diagnosers;

[SuppressMessage("Code Smell", "S2699:Tests should include assertions", Justification = "This is done internally by AnalyzerTest.RunAsync()")]
public sealed class BanSynchronousEntityFrameworkAnalyzerTests(ITestOutputHelper testOutputHelper)
    : TestBase<BanSynchronousEntityFrameworkAnalyzer>(testOutputHelper)
{
    [Theory]
    [InlineData("ToList", "")]
    [InlineData("ToArray", "")]
    [InlineData("First", "")]
    [InlineData("FirstOrDefault", "")]
    [InlineData("Single", "")]
    [InlineData("SingleOrDefault", "")]
    [InlineData("Count", "")]
    [InlineData("Any", "")]
    [InlineData("All", "e => true")]
    [InlineData("Last", "")]
    [InlineData("LastOrDefault", "")]
    [InlineData("Max", "")]
    [InlineData("Min", "")]
    [InlineData("Sum", "e => e.Id")]
    [InlineData("Average", "e => e.Id")]
    [InlineData("ElementAt", "0")]
    [InlineData("ElementAtOrDefault", "0")]
    [InlineData("Contains", "new Entity()")]
    [InlineData("LongCount", "")]
    public async Task SynchronousMethod_OnDbSet_ThenDiagnose(string methodName, string arguments)
    {
        var code = $$"""
                     using var dbContext = new TestContext();
                     dbContext.Entities.{|SIEF0002:{{methodName}}|}({{arguments}});
                     """;

        await RunTestAsync(code);
    }

    [Theory]
    [InlineData("ToListAsync", "")]
    [InlineData("ToArrayAsync", "")]
    [InlineData("FirstAsync", "")]
    [InlineData("FirstOrDefaultAsync", "")]
    [InlineData("SingleAsync", "")]
    [InlineData("SingleOrDefaultAsync", "")]
    [InlineData("CountAsync", "")]
    [InlineData("AnyAsync", "")]
    [InlineData("LastAsync", "")]
    [InlineData("LastOrDefaultAsync", "")]
    [InlineData("MaxAsync", "")]
    [InlineData("MinAsync", "")]
    [InlineData("SumAsync", "e => e.Id")]
    [InlineData("AverageAsync", "e => e.Id")]
    [InlineData("ContainsAsync", "new Entity()")]
    [InlineData("LongCountAsync", "")]
    public async Task AsyncMethod_OnDbSet_ThenOk(string methodName, string arguments)
    {
        var code = $$"""
                     using var dbContext = new TestContext();
                     await dbContext.Entities.{{methodName}}({{arguments}});
                     """;

        await RunTestAsync(code);
    }

    [Fact]
    public async Task SynchronousMethod_OnQueryableChain_ThenDiagnose()
    {
        const string code = """
                            using var dbContext = new TestContext();
                            dbContext.Entities.Where(e => e.Id > 0).{|SIEF0002:ToList|}();
                            """;

        await RunTestAsync(code);
    }

    [Fact]
    public async Task SynchronousMethod_OnInMemoryList_ThenOk()
    {
        const string code = """
                            var list = new List<Entity> { new Entity { Id = 1, Name = "Test" } };
                            list.ToList();
                            """;

        await RunTestAsync(code);
    }

    [Fact]
    public async Task SynchronousMethod_InsideExpressionTree_ThenOk()
    {
        const string code = """
                            using var dbContext = new TestContext();
                            dbContext.Entities.Where(e => dbContext.ProjectionEntities.Any(p => p.Id == e.Id)).{|SIEF0002:ToList|}();
                            """;

        await RunTestAsync(code);
    }

    [Fact]
    public async Task ToDictionary_OnDbSet_ThenDiagnose()
    {
        const string code = """
                            using var dbContext = new TestContext();
                            dbContext.Entities.{|SIEF0002:ToDictionary|}(e => e.Id, e => e.Name);
                            """;

        await RunTestAsync(code);
    }

    [Fact]
    public async Task ToHashSet_OnDbSet_ThenDiagnose()
    {
        const string code = """
                            using var dbContext = new TestContext();
                            dbContext.Entities.{|SIEF0002:ToHashSet|}();
                            """;

        await RunTestAsync(code);
    }

    [Fact]
    public async Task AsEnumerable_OnDbSet_ThenDiagnose()
    {
        const string code = """
                            using var dbContext = new TestContext();
                            dbContext.Entities.{|SIEF0002:AsEnumerable|}();
                            """;

        await RunTestAsync(code);
    }

    [Fact]
    public async Task Find_OnDbSet_ThenDiagnose()
    {
        const string code = """
                            using var dbContext = new TestContext();
                            dbContext.Entities.{|SIEF0002:Find|}(1);
                            """;

        await RunTestAsync(code);
    }

    [Fact]
    public async Task Theory_IsEnabled_WhenEnabled_ThenDiagnose()
    {
        const string code = """
                            using var dbContext = new TestContext();
                            dbContext.Entities.{|SIEF0002:ToList|}();
                            """;

        await RunTestAsync(code);
    }

    [Fact]
    public async Task SynchronousMethod_WithAsNoTracking_ThenStillDiagnose()
    {
        const string code = """
                            using var dbContext = new TestContext();
                            dbContext.Entities.AsNoTracking().{|SIEF0002:ToList|}();
                            """;

        await RunTestAsync(code);
    }

    [Fact]
    public async Task MaxBy_OnDbSet_ThenDiagnose()
    {
        const string code = """
                            using var dbContext = new TestContext();
                            dbContext.Entities.{|SIEF0002:MaxBy|}(e => e.Id);
                            """;

        await RunTestAsync(code);
    }

    [Fact]
    public async Task MinBy_OnDbSet_ThenDiagnose()
    {
        const string code = """
                            using var dbContext = new TestContext();
                            dbContext.Entities.{|SIEF0002:MinBy|}(e => e.Id);
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
                 using System.Threading.Tasks;
                 using Microsoft.EntityFrameworkCore;

                 namespace Tests;

                 public sealed class Entity
                 {
                     public int    Id   { get; set; }
                     public string Name { get; set; } = null!;
                 }

                 public sealed class ProjectionEntity
                 {
                     public int    Id   { get; set; }
                     public string Name { get; set; } = null!;
                 }

                 public sealed class TestContext : DbContext
                 {
                     public DbSet<Entity>           Entities           { get; set; } = null!;
                     public DbSet<ProjectionEntity> ProjectionEntities { get; set; } = null!;
                 }

                 public static class Test
                 {
                     public static async Task TestMethod()
                     {
                         {{insertionCode}}
                     }
                 }
                 """;
    }

    private Task RunTestAsync(string insertionCode)
        => RunTestAsync(insertionCode, true);

    private async Task RunTestAsync(string insertionCode, bool isEnabled)
    {
        var code = CreateTestCode(insertionCode);

        await CreateTesterBuilder()
             .WithTestCode(code)
             .SetEnabled(isEnabled, "SIEF0002")
             .WithNugetPackage("Microsoft.EntityFrameworkCore", "9.0.8")
             .Build()
             .RunAsync();
    }
}
