
Set-StrictMode -Version "Latest"
$PSNativeCommandUseErrorActionPreference = $true
$ErrorActionPreference = "Stop"

function CleanUp
{
    Get-ChildItem -Recurse -Filter "obj" -Directory | Remove-Item -Recurse
    Get-ChildItem -Recurse -Filter "bin" -Directory | Remove-Item -Recurse
}

function CompileForSpecificRoslynVersion([string] $RoslynVersion, [string] $TargetFramework) {
    Write-Host "Compiling 'SyntaxInspectors.Analyzers' for roslyn$RoslynVersion and $TargetFramework" -ForegroundColor Cyan
    dotnet.exe build SyntaxInspectors.Analyzers/SyntaxInspectors.Analyzers.csproj --configuration Release /p:RoslynVersion=roslyn$RoslynVersion /p:TargetFramework=$TargetFramework
    Write-Host "Compiling 'SyntaxInspectors.Analyzers.CodeFixers' for roslyn$RoslynVersion and $TargetFramework" -ForegroundColor Cyan
    dotnet.exe build SyntaxInspectors.Analyzers.CodeFixers/SyntaxInspectors.Analyzers.CodeFixers.csproj --configuration Release /p:RoslynVersion=roslyn$RoslynVersion /p:TargetFramework=$TargetFramework
}

function CompileAll {
    $targets = @("netstandard2.0")
    $roslynVersions = @("4.8", "4.9", "4.10", "4.11", "4.12", "4.13", "4.14")

    foreach ($target in $targets) {
        foreach ($roslynVersion in $roslynVersions) {
            CompileForSpecificRoslynVersion -RoslynVersion $roslynVersion -TargetFramework $target
        }
    }

    dotnet restore SyntaxInspectors.Analyzers.Pack/SyntaxInspectors.Analyzers.Pack.csproj /p:TargetFramework=netstandard2.0
    dotnet pack SyntaxInspectors.Analyzers.Pack/SyntaxInspectors.Analyzers.Pack.csproj --configuration Release --no-build /p:TargetFramework=netstandard2.0
}

Push-Location $PSScriptRoot

try {
    CleanUp
    CompileForSpecificRoslynVersion -RoslynVersion 4.14 -TargetFramework "net9.0"

    CleanUp
    CompileAll
}
finally {
    Pop-Location
}
