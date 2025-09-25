As you might have seen, the Analyzer classes serve as entry point and the actual implementation is in the class with the "Implementation".
The reason for this is to have the context and logger (if needed) as field so we don't need to pass it around to methods. In short, to reduce pollution


# Register Syntax Node Action
```csharp
public override void Initialize(AnalysisContext context)
{
    context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.Analyze);
    context.EnableConcurrentExecutionInReleaseMode();
    context.RegisterSyntaxNodeActionAndAnalyze<DoNotAwaitTaskFromResultAnalyzerImplementation>(a => a.AnalyzeAwait, SyntaxKind.AwaitExpression);
}
```
Here, we don't call `context.EnableConcurrentExecution();` as when debugging, we don't want to other threads to interfere or disturb us. Therefore call `ontext.EnableConcurrentExecutionInReleaseMode();` which does what it says.

# Configuration
If your analyzer needs custom configuration, see `SyntaxInspectors.Analyzers.Configuration.Si0007.ConfigurationProvider` as reference.
The code to load configuration is called lots of time. Unfortunately, we cannot cache the configuration as there might be additional `.editorconfig` files in sub-folders. Users can make changes to them as well and then you're screwed :).

# Deactivation of Analyzers
There's a separate config which every analyzer should handle and that would be the `is_enabled` config flag.
Although you can mute analyzer diagnostics with `dotnet_diagnostic.xxx.severity = none`, deactivating an analyzer in case it causes problems might still be a good feature. With problems i mean crashes, high CPU usage in large files or whatever.

# Logging
Analyzers should not do any I/O at all. However, if we want to do use logging, there's no other way.
Since we should not use any external library in analyzers, we can't use any logging framework. Therefore, i wrote my one which has the focus of speed and simplicity. 
Instead of logging to a single file, which can be difficult since we have multiple threads and even multiple processes, synchronization is a huge pain. I tried named mutexes etc. but this was just introducing additional issues. Therefore, the log file name pattern contains the process id (PID) and thread id (TID). That will make them unique and we don't have any synchronization issues.
Currently, the logger is always opening the file, appending the string and closing the file. Sure we can keep the file open and just flush every time the string was written, but that would require additional code. I think we can keep it as it is for now. Logging should only be enabled in troubleshooting cases.

# Diagnostic IDs
- Use 4 digits prefixed by `SI` (e.g. `SI0123`)

# Tests
- To use custom `.editorconfig` file entries or `additional nuget packages` during tests, check out the method `CreateTester()` of the class `ParameterOrderingAnalyzerTests()`. Should be self explaining


# Checklist when implementing an analyzer
- Implement the Analyzer and the Implementation class in a minimalistic way. 
- Write unit tests and execute them. Check the test output window which will print out the syntax tree which is quite helpful when writing analyzers.
- Update `README.md` with the new diagnostic
- Update `AnalyzerReleases.Shipped.md` with the new diagnostics. Attention, do not use double-spaces here, otherwise the Roslyn analyzer for analyzers will complain. This is a well known bug
