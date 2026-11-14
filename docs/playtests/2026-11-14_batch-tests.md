# Playtest / CI 2026-11-14 batch Unity tests

Operator: agent (batchmode after editor closed)

## Command

```powershell
.\tools\run-tests.ps1 -Platform EditMode   # earlier in session
.\tools\run-tests.ps1 -Platform PlayMode
```

`tools/run-tests.ps1` was fixed to use `Start-Process -Wait` and to omit `-quit`
(so `-runTests` is not aborted before the runner starts).

## Results

| Platform | total | passed | failed | notes |
| --- | --- | --- | --- | --- |
| EditMode | 13 | 13 | 0 | Includes MatchReduce + FlickMath sway/impulse |
| PlayMode | 2 | 2 | 0 | Includes ClassroomKnockoffPlayModeTests |

Artifacts: `TestResults/EditMode-results.xml`, `TestResults/PlayMode-results.xml`
(gitignored).

## Core smoke

`tools/run-core-smoke.ps1` → 3/3 passed (unchanged).
