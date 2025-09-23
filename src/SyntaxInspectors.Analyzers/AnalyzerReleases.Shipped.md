; Shipped analyzer releases
; https://github.com/dotnet/roslyn-analyzers/blob/main/src/Microsoft.CodeAnalysis.Analyzers/ReleaseTrackingAnalyzers.Help.md

Rule ID | Category | Severity | Notes
-------|----------------|-------------|-------
SI0001 | Predictability | Warning | Provide an equality comparer argument
SI0002 | Intention / Performance | Warning | Always specify the tracking type when using Entity Framework
SI0003 | Performance | Warning | Do not return materialized collection as enumerable
SI0004 | Performance | Warning | Do not create tasks of enumerable type containing a materialized collection
SI0005 | Style | Warning | Do not use general warning suppression
SI0006 | Style | Warning | Classes containing extension methods should have an `Extensions` suffix
SI0007 | Style | Warning | Non-compliant parameter order
SI0008 | Performance | Warning | Do not await Task.FromResult()
SI0009 | Readability / Clarity | Warning | Lambda variable declaration hides outer lambda variable that share the same name
SI0010 | Readability | Warning | Declare constants at the top of the method
SI0011 | Intention | Warning | Use is or is not for null-comparison
SI9999 | Analyzer Error | Warning | Unexpected error in SyntaxInspectors.Analyzers
