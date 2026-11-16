# Short 2P hotseat smoke. Drops Temp flags that OpenSceneFlagWatcher already consumes.
# Live mode drives the one editor already open on -Project.
# Usage:
#   tools/run-hotseat-e2e.ps1
#   tools/run-hotseat-e2e.ps1 -DryRun
#   tools/run-hotseat-e2e.ps1 -SelfTest
# -DryRun writes those same flag files. Run it only when that editor is closed.
param(
    [string]$Project = (Resolve-Path (Join-Path $PSScriptRoot "..")).Path,
    [string]$AuditPath = "",
    [int]$StepTimeoutSeconds = 60,
    [switch]$DryRun,
    [switch]$SelfTest
)

$ErrorActionPreference = "Stop"

$script:AuditHeader = "ts`tmode`tsteps`tmissed`tevidence`tresult"

function Get-HotseatSteps {
    $steps = @(
        @{ Name = "enter-play"; Flag = "enter-play.flag"; Log = "" },
        @{ Name = "start-hotseat-2"; Flag = "start-hotseat-2.flag"; Log = "start-hotseat-2: BeginMatch(2)" },
        @{ Name = "flick-current"; Flag = "flick-current.flag"; Log = "flick-current: P" },
        @{ Name = "dump-ui"; Flag = "dump-ui.flag"; Log = "dump-ui: match phase=" }
    )
    return ,$steps
}

function Get-FileLength([string]$Path) {
    if (-not (Test-Path -LiteralPath $Path)) {
        return [int64]0
    }

    $stream = [System.IO.File]::Open(
        $Path,
        [System.IO.FileMode]::Open,
        [System.IO.FileAccess]::Read,
        [System.IO.FileShare]::ReadWrite)
    try {
        return $stream.Length
    }
    finally {
        $stream.Dispose()
    }
}

function Read-FileFrom([string]$Path, [int64]$Start) {
    if (-not (Test-Path -LiteralPath $Path)) {
        return ""
    }

    $stream = [System.IO.File]::Open(
        $Path,
        [System.IO.FileMode]::Open,
        [System.IO.FileAccess]::Read,
        [System.IO.FileShare]::ReadWrite)
    try {
        if ($stream.Length -lt $Start) {
            $Start = 0
        }

        $count = [int]($stream.Length - $Start)
        if ($count -le 0) {
            return ""
        }

        $buffer = New-Object byte[] $count
        [void]$stream.Seek($Start, [System.IO.SeekOrigin]::Begin)
        [void]$stream.Read($buffer, 0, $count)
        return [System.Text.Encoding]::UTF8.GetString($buffer)
    }
    finally {
        $stream.Dispose()
    }
}

function Wait-Until([scriptblock]$Done, [datetime]$Deadline) {
    while ((Get-Date) -lt $Deadline) {
        if (& $Done) {
            return $true
        }

        Start-Sleep -Milliseconds 200
    }

    return [bool](& $Done)
}

function Add-HotseatAuditRow([string]$Path, [string]$Row) {
    $parent = Split-Path -Parent $Path
    if ($parent) {
        New-Item -ItemType Directory -Force -Path $parent | Out-Null
    }

    $utf8 = New-Object System.Text.UTF8Encoding $false
    $needsHeader = $true
    if (Test-Path -LiteralPath $Path) {
        $needsHeader = ([System.IO.File]::ReadAllText($Path)).Length -eq 0
    }

    if ($needsHeader) {
        [System.IO.File]::WriteAllText($Path, $script:AuditHeader + [Environment]::NewLine, $utf8)
    }

    [System.IO.File]::AppendAllText($Path, $Row + [Environment]::NewLine, $utf8)
    return $Row
}

function Invoke-HotseatE2E {
    param(
        [string]$Project,
        [string]$AuditPath,
        [switch]$DryRun,
        [int]$StepTimeoutSeconds
    )

    New-Item -ItemType Directory -Force -Path $Project | Out-Null
    $temp = Join-Path $Project "Temp"
    New-Item -ItemType Directory -Force -Path $temp | Out-Null
    $logPath = Join-Path $Project "Logs\Editor.log"
    $utf8 = New-Object System.Text.UTF8Encoding $false
    $steps = Get-HotseatSteps
    $passed = @()
    $missed = "-"
    $evidence = "Logs/Editor.log"
    if ($DryRun) {
        $flags = ($steps | ForEach-Object { "Temp/$($_.Flag)" }) -join ","
        $logs = ($steps | Where-Object { $_.Log } | ForEach-Object { $_.Log }) -join ","
        $evidence = "$flags;$logs"
    }

    foreach ($step in $steps) {
        $flagPath = Join-Path $temp $step.Flag
        $deadline = (Get-Date).AddSeconds($StepTimeoutSeconds)
        $logStart = Get-FileLength $logPath
        [System.IO.File]::WriteAllText($flagPath, "", $utf8)

        $consumed = $false
        if ($DryRun) {
            Remove-Item -LiteralPath $flagPath -Force
            $consumed = -not (Test-Path -LiteralPath $flagPath)
        }
        else {
            $consumed = Wait-Until { -not (Test-Path -LiteralPath $flagPath) } $deadline
            if (-not $consumed) {
                Remove-Item -LiteralPath $flagPath -Force -ErrorAction SilentlyContinue
            }
        }

        if (-not $consumed) {
            $missed = $step.Name
            $evidence = "flag-timeout"
            break
        }

        if (-not $DryRun -and $step.Log) {
            $needle = $step.Log
            $hit = Wait-Until { (Read-FileFrom $logPath $logStart).Contains($needle) } $deadline
            if (-not $hit) {
                $missed = $step.Name
                $evidence = "Logs/Editor.log"
                break
            }
        }

        $passed += $step.Name
    }

    $stepField = "-"
    if ($passed.Count -gt 0) {
        $stepField = $passed -join ","
    }

    $result = "fail"
    if ($missed -eq "-") {
        $result = "pass"
    }

    $mode = "live"
    if ($DryRun) {
        $mode = "dry-run"
    }

    $ts = [DateTime]::UtcNow.ToString("yyyy-MM-ddTHH:mm:ss") + "Z"
    $row = Add-HotseatAuditRow -Path $AuditPath -Row "$ts`t$mode`t$stepField`t$missed`t$evidence`t$result"
    $code = 1
    if ($result -eq "pass") {
        $code = 0
    }

    return [pscustomobject]@{
        ExitCode = $code
        Row = $row
        AuditPath = $AuditPath
    }
}

function Assert-HotseatRow {
    param(
        [string]$Row,
        [string]$Mode,
        [string]$Steps,
        [string]$Missed,
        [string]$Evidence,
        [string]$Result
    )

    $parts = $Row.Split("`t")
    if ($parts.Count -ne 6) {
        throw "audit row has $($parts.Count) columns: $Row"
    }

    if ($parts[0] -notmatch '^\d{4}-\d{2}-\d{2}T\d{2}:\d{2}:\d{2}Z$') {
        throw "audit timestamp '$($parts[0])'"
    }

    if ($parts[1] -ne $Mode) { throw "mode '$($parts[1])'" }
    if ($parts[2] -ne $Steps) { throw "steps '$($parts[2])'" }
    if ($parts[3] -ne $Missed) { throw "missed '$($parts[3])'" }
    if ($parts[4] -ne $Evidence) { throw "evidence '$($parts[4])'" }
    if ($parts[5] -ne $Result) { throw "result '$($parts[5])'" }
}

function Assert-NoFlags([string]$Project) {
    $temp = Join-Path $Project "Temp"
    $left = @(Get-ChildItem -LiteralPath $temp -Filter *.flag -ErrorAction SilentlyContinue)
    if ($left.Count -ne 0) {
        throw "flag files left behind: $($left.Name -join ',')"
    }
}

function Invoke-HotseatSelfTest {
    $repoRoot = (Resolve-Path (Join-Path $PSScriptRoot "..")).Path
    $watcherPath = Join-Path $repoRoot "Assets\_Project\Editor\OpenSceneFlagWatcher.cs"
    $watcherText = [System.IO.File]::ReadAllText($watcherPath)
    $flagEvidence = "Temp/enter-play.flag,Temp/start-hotseat-2.flag,Temp/flick-current.flag,Temp/dump-ui.flag"
    $logEvidence = "start-hotseat-2: BeginMatch(2),flick-current: P,dump-ui: match phase="
    foreach ($needle in ($flagEvidence.Split(",") + $logEvidence.Split(","))) {
        if (-not $watcherText.Contains($needle)) {
            throw "OpenSceneFlagWatcher.cs is missing $needle"
        }
    }

    $root = Join-Path ([System.IO.Path]::GetTempPath()) ("hotseat-e2e-" + [guid]::NewGuid().ToString("n"))
    New-Item -ItemType Directory -Force -Path $root | Out-Null
    try {
        $passAudit = Join-Path $root "dry-run.tsv"
        $pass = Invoke-HotseatE2E -Project $root -AuditPath $passAudit -DryRun -StepTimeoutSeconds 5
        if ($pass.ExitCode -ne 0) {
            throw "dry-run exit $($pass.ExitCode): $($pass.Row)"
        }

        $passLines = @(Get-Content -LiteralPath $passAudit)
        if ($passLines.Count -ne 2) {
            throw "dry-run audit has $($passLines.Count) lines"
        }

        if ($passLines[0] -ne "ts`tmode`tsteps`tmissed`tevidence`tresult") {
            throw "dry-run header '$($passLines[0])'"
        }

        Assert-HotseatRow -Row $pass.Row `
            -Mode "dry-run" `
            -Steps "enter-play,start-hotseat-2,flick-current,dump-ui" `
            -Missed "-" `
            -Evidence "$flagEvidence;$logEvidence" `
            -Result "pass"
        Assert-NoFlags $root
        Write-Output $pass.Row

        $failRoot = Join-Path $root "live-timeout"
        $failAudit = Join-Path $failRoot "live.tsv"
        $fail = Invoke-HotseatE2E -Project $failRoot -AuditPath $failAudit -StepTimeoutSeconds 1
        if ($fail.ExitCode -eq 0) {
            throw "live timeout returned pass: $($fail.Row)"
        }

        $failLines = @(Get-Content -LiteralPath $failAudit)
        if ($failLines.Count -ne 2) {
            throw "live audit has $($failLines.Count) lines"
        }

        if ($failLines[0] -ne "ts`tmode`tsteps`tmissed`tevidence`tresult") {
            throw "live header '$($failLines[0])'"
        }

        Assert-HotseatRow -Row $fail.Row -Mode "live" -Steps "-" -Missed "enter-play" -Evidence "flag-timeout" -Result "fail"
        Assert-NoFlags $failRoot
        Write-Output $fail.Row
        Write-Output "self-test pass"
    }
    finally {
        Remove-Item -LiteralPath $root -Recurse -Force -ErrorAction SilentlyContinue
    }
}

if ($SelfTest) {
    Invoke-HotseatSelfTest
    exit 0
}

if (-not $AuditPath) {
    if ($DryRun) {
        $AuditPath = Join-Path $Project "Temp\hotseat-e2e-dry-run.tsv"
    }
    else {
        $AuditPath = Join-Path $Project ".audit\hotseat-e2e.tsv"
    }
}

$run = Invoke-HotseatE2E -Project $Project -AuditPath $AuditPath -DryRun:$DryRun -StepTimeoutSeconds $StepTimeoutSeconds
Write-Output $run.Row
exit $run.ExitCode
