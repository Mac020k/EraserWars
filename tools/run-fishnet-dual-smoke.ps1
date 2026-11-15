# Launch Fish-Net spike dual-instance smoke (editor already Host, this is Client player build).
# Prerequisites: Unity closed for -batchmode build, or use an existing Windows player at $PlayerPath.
param(
    [string]$Unity = "C:\Program Files\Unity\Hub\Editor\6000.6.9f1\Editor\Unity.exe",
    [string]$PlayerPath = "Builds\FishNetSpike\EraserWars.exe",
    [switch]$Build
)

$ErrorActionPreference = "Stop"
$root = Resolve-Path (Join-Path $PSScriptRoot "..")
Set-Location $root

if ($Build -or -not (Test-Path $PlayerPath)) {
    New-Item -ItemType Directory -Force -Path (Join-Path $root "TestResults") | Out-Null
    Write-Host "Building FishNet_Spike Windows player via EraserWars.Editor.FishNetSpikePlayerBuild ..."
    & $Unity `
        -batchmode -nographics `
        -projectPath $root.Path `
        -executeMethod EraserWars.Editor.FishNetSpikePlayerBuild.BuildWindowsPlayer `
        -logFile (Join-Path $root "TestResults\FishNet-spike-player-build.log") `
        -quit
    if ($LASTEXITCODE -ne 0) {
        throw "Unity player build failed (exit $LASTEXITCODE). See TestResults\FishNet-spike-player-build.log"
    }
}

if (-not (Test-Path $PlayerPath)) {
    throw "Player not found at $PlayerPath. Re-run with -Build."
}

Write-Host "Starting client player. In the Editor: open FishNet_Spike, Play, Start Host, Fire canned flick."
Write-Host "In the player: Start Client, then compare Local vs host delta on both HUDs."
Start-Process -FilePath (Resolve-Path $PlayerPath).Path -WorkingDirectory (Split-Path (Resolve-Path $PlayerPath).Path)
