# Saves Game view PNGs of the Start, Collection, and Options screens from a running Unity editor.
# Needs the editor open on -Project; it enters Play through Temp/enter-play.flag when asked.
# Usage: tools/menu-shots.ps1 -Prefix docs/prototypes/ui-hig-after [-Aspect 16x9] [-EnterPlay]
param(
    [string]$Project = (Resolve-Path "$PSScriptRoot\..").Path,
    [string]$Prefix = "Temp/ui-hig/shot",
    [string]$Aspect = "",
    [int]$CharacterId = 0,
    [switch]$EnterPlay,
    [int]$TimeoutSeconds = 120
)

$ErrorActionPreference = "Stop"
$temp = Join-Path $Project "Temp"
$utf8 = New-Object System.Text.UTF8Encoding($false)

if ($EnterPlay) {
    New-Item -ItemType File -Force (Join-Path $temp "enter-play.flag") | Out-Null
    Start-Sleep -Seconds 8
}

$shots = @(
    @{ Screen = "home"; Extra = "" },
    @{ Screen = "collection"; Extra = "$CharacterId" },
    @{ Screen = "options"; Extra = "" }
)

$failed = 0
foreach ($shot in $shots) {
    $relative = "$Prefix-$($shot.Screen).png"
    $output = Join-Path $Project $relative
    $body = "$($shot.Screen) $relative $($shot.Extra) $Aspect".Trim()
    $flag = Join-Path $temp "menu-shot.flag"
    Remove-Item $output -ErrorAction SilentlyContinue
    [System.IO.File]::WriteAllText($flag, $body, $utf8)

    $deadline = (Get-Date).AddSeconds($TimeoutSeconds)
    while ((Test-Path $flag) -and (Get-Date) -lt $deadline) { Start-Sleep -Milliseconds 250 }
    while (-not (Test-Path $output) -and (Get-Date) -lt $deadline) { Start-Sleep -Milliseconds 250 }
    Start-Sleep -Milliseconds 750

    if (Test-Path $output) {
        Write-Output "saved $relative"
    } else {
        Write-Output "MISSING $relative (is the editor in Play Mode?)"
        Remove-Item $flag -ErrorAction SilentlyContinue
        $failed++
    }
}

exit $failed
