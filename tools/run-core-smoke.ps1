$ErrorActionPreference = "Stop"
$env:Path = [System.Environment]::GetEnvironmentVariable("Path","Machine") + ";" + [System.Environment]::GetEnvironmentVariable("Path","User")
$proj = Join-Path $PSScriptRoot "CoreSmoke\Keshipin.Core.Smoke.csproj"
dotnet test $proj --nologo
exit $LASTEXITCODE
