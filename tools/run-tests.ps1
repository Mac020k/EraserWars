param(
    [ValidateSet("EditMode", "PlayMode", "All")]
    [string]$Platform = "EditMode",
    [string]$UnityExe = "C:\Program Files\Unity\Hub\Editor\6000.6.9f1\Editor\Unity.exe"
)

$ErrorActionPreference = "Stop"
$ProjectPath = Resolve-Path (Join-Path $PSScriptRoot "..")
$ResultsDir = Join-Path $ProjectPath "TestResults"
New-Item -ItemType Directory -Force -Path $ResultsDir | Out-Null

function Invoke-UnityTests {
    param([string]$TestPlatform)
    $out = Join-Path $ResultsDir "$TestPlatform-results.xml"
    $log = Join-Path $ResultsDir "$TestPlatform-unity.log"
    if (Test-Path $out) { Remove-Item $out -Force }
    Write-Host "Running $TestPlatform tests..."
    # Start-Process -Wait keeps the script on the editor process until batchmode exits.
    # Do not pass -quit. With -runTests it can shut down before the runner starts.
    $proc = Start-Process -FilePath $UnityExe -ArgumentList @(
        "-batchmode",
        "-nographics",
        "-projectPath", "$ProjectPath",
        "-runTests",
        "-testPlatform", $TestPlatform,
        "-testResults", $out,
        "-logFile", $log
    ) -PassThru -Wait -NoNewWindow
    $code = $proc.ExitCode
    if (-not (Test-Path $out)) {
        Write-Error "No results XML at $out. See $log. Exit=$code"
    }
    [xml]$xml = Get-Content $out
    $total = [int]$xml."test-run".total
    $failed = [int]$xml."test-run".failed
    $passed = [int]$xml."test-run".passed
    Write-Host "$TestPlatform total=$total passed=$passed failed=$failed exit=$code"
    return $failed
}

$failures = 0
if ($Platform -eq "EditMode" -or $Platform -eq "All") {
    $failures += Invoke-UnityTests -TestPlatform "EditMode"
}
if ($Platform -eq "PlayMode" -or $Platform -eq "All") {
    $failures += Invoke-UnityTests -TestPlatform "PlayMode"
}

if ($failures -gt 0) {
    exit 1
}
exit 0
