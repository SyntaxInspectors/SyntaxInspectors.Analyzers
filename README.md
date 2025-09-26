# SyntaxInspectors.Analyzers

C# code analyzers for .NET 8.0 and higher. Lower .NET versions are not supported.

Source: https://github.com/SyntaxInspectors/SyntaxInspectors.Analyzers

# Rules

| Id                                                                                                      | Category                | Technology       | Severity | Has Code fix | Title                                                                                 |
|:--------------------------------------------------------------------------------------------------------|:------------------------|:-----------------|:--------:|:------------:|:--------------------------------------------------------------------------------------|
| [SI0001](https://github.com/SyntaxInspectors/SyntaxInspectors.Analyzers/tree/main/docs/Rules/SI0001.md) | Predictability          | General          |    ⚠️    |      ❌       | Provide an equality comparer argument                                                 | 
| [SI0002](https://github.com/SyntaxInspectors/SyntaxInspectors.Analyzers/tree/main/docs/Rules/SI0002.md) | Intention / Performance | Entity Framework |    ⚠️    |      ❌       | Always specify the tracking type when using Entity Framework                          | 
| [SI0003](https://github.com/SyntaxInspectors/SyntaxInspectors.Analyzers/tree/main/docs/Rules/SI0003.md) | Performance             | General          |    ⚠️    |      ❌       | Do not return materialized collection as enumerable                                   | 
| [SI0004](https://github.com/SyntaxInspectors/SyntaxInspectors.Analyzers/tree/main/docs/Rules/SI0004.md) | Performance             | General          |    ⚠️    |      ❌       | Do not create tasks of enumerable type containing a materialized collection           | 
| [SI0005](https://github.com/SyntaxInspectors/SyntaxInspectors.Analyzers/tree/main/docs/Rules/SI0005.md) | Style                   | General          |    ⚠️    |      ❌       | Do not use general warning suppression                                                | 
| [SI0006](https://github.com/SyntaxInspectors/SyntaxInspectors.Analyzers/tree/main/docs/Rules/SI0006.md) | Style                   | General          |    ⚠️    |      ❌       | Classes containing extension methods should have an `Extensions` suffix               | 
| [SI0007](https://github.com/SyntaxInspectors/SyntaxInspectors.Analyzers/tree/main/docs/Rules/SI0007.md) | Style                   | General          |    ⚠️    |      ❌       | Non-compliant parameter order                                                         | 
| [SI0008](https://github.com/SyntaxInspectors/SyntaxInspectors.Analyzers/tree/main/docs/Rules/SI0008.md) | Performance             | General          |    ⚠️    |      ❌       | Do not await Task.FromResult()                                                        | 
| [SI0009](https://github.com/SyntaxInspectors/SyntaxInspectors.Analyzers/tree/main/docs/Rules/SI0009.md) | Readability / Clarity   | General          |    ⚠️    |      ❌       | Lambda variable declaration hides outer lambda variable that share the same name      | 
| [SI0010](https://github.com/SyntaxInspectors/SyntaxInspectors.Analyzers/tree/main/docs/Rules/SI0010.md) | Readability             | General          |    ⚠️    |      ❌       | Declare constants at the top of the method                                            | 
| [SI0011](https://github.com/SyntaxInspectors/SyntaxInspectors.Analyzers/tree/main/docs/Rules/SI0010.md) | Intention               | General          |    ⚠️    |      ✅       | Use is or is not for null-comparison                                                  | 
| [SI9999](https://github.com/SyntaxInspectors/SyntaxInspectors.Analyzers/tree/main/docs/Rules/SI9999.md) | Analyzer Error          | General          |    ⚠️    |      ❌       | Unexpected error in SyntaxInspectors.Analyzers                                        | 

# Logging

To enable logging, set the following property to true in the `.editorconfig` file:

```
[*.cs]
SyntaxInspectors_Analyzers.is_logging_enabled = None | Duration | Full
```

| Value    | Description                                                       |
|:---------|:------------------------------------------------------------------|
| None     | Logging is disabled                                               |  
| Duration | Only log the duration it took for each analyzer to analyze a node |
| Full     | Full logging                                                      |

Please be aware that logging will slow down the analysis by several factors. It should only be used for debugging
purposes.
