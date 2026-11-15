param(
    [string]$UnityExe = "C:\Program Files\Unity\Hub\Editor\6000.6.9f1\Editor\Unity.exe",
    [string]$BuildTarget = "Win64"
)

$ErrorActionPreference = "Stop"
$ProjectPath = Resolve-Path (Join-Path $PSScriptRoot "..")
$BuildDir = Join-Path $ProjectPath "Build\$BuildTarget"
$Log = Join-Path $ProjectPath "TestResults\build-$BuildTarget.log"
New-Item -ItemType Directory -Force -Path $BuildDir | Out-Null
New-Item -ItemType Directory -Force -Path (Split-Path $Log) | Out-Null

$OutExe = Join-Path $BuildDir "Eraser Wars.exe"
Write-Host "Building $BuildTarget to $OutExe"

& $UnityExe `
    -batchmode `
    -nographics `
    -projectPath $ProjectPath `
    -buildWindows64Player $OutExe `
    -logFile $Log `
    -quit

if ($LASTEXITCODE -ne 0) {
    Write-Error "Build failed. See $Log"
    exit $LASTEXITCODE
}
Write-Host "Build ok: $OutExe"
exit 0
