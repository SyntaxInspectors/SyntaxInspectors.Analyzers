
Set-StrictMode -Version "Latest"
$PSNativeCommandUseErrorActionPreference = $true
$ErrorActionPreference = "Stop"

function CleanUp
{
    Get-ChildItem -Recurse -Filter "obj" -Directory | Remove-Item -Recurse
    Get-ChildItem -Recurse -Filter "bin" -Directory | Remove-Item -Recurse
}

function CompileForSpecificRoslynVersion([string] $RoslynVersion) {
    Write-Host "Compiling 'SyntaxInspectors.Analyzers' for roslyn$RoslynVersion" -ForegroundColor Cyan
    dotnet.exe build SyntaxInspectors.Analyzers/SyntaxInspectors.Analyzers.csproj --configuration Release /p:RoslynVersion=roslyn$RoslynVersion /p:TargetFramework=netstandard2.0
    Write-Host "Compiling 'SyntaxInspectors.Analyzers.CodeFixers' for roslyn$RoslynVersion" -ForegroundColor Cyan
    dotnet.exe build SyntaxInspectors.Analyzers.CodeFixers/SyntaxInspectors.Analyzers.CodeFixers.csproj --configuration Release /p:RoslynVersion=roslyn$RoslynVersion /p:TargetFramework=netstandard2.0
}

function CompileAll {

    CompileForSpecificRoslynVersion "4.8"
    CompileForSpecificRoslynVersion "4.9.2"
    CompileForSpecificRoslynVersion "4.10"
    CompileForSpecificRoslynVersion "4.11"
    CompileForSpecificRoslynVersion "4.12"
    CompileForSpecificRoslynVersion "4.13"
    CompileForSpecificRoslynVersion "4.14"

    dotnet restore SyntaxInspectors.Analyzers.Pack/SyntaxInspectors.Analyzers.Pack.csproj -p:TargetFramework=netstandard2.0
    dotnet pack SyntaxInspectors.Analyzers.Pack/SyntaxInspectors.Analyzers.Pack.csproj --configuration Release --no-build -p:TargetFramework=netstandard2.0
}

Push-Location $PSScriptRoot

try {
    CleanUp
    CompileAll
}
finally {
    Pop-Location
}
