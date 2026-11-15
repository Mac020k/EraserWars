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
| PlayMode | 3 | 3 | 0 | Knockoff + HostCommandGate flick settle + sway |

Artifacts: `TestResults/EditMode-results.xml`, `TestResults/PlayMode-results.xml`
(gitignored).

## Core smoke

`tools/run-core-smoke.ps1` → 3/3 passed (unchanged).

## Fish-Net spike (same day)

- Embed 4.7.4 under `Assets/FishNet` (Yak/Demos removed). Compile Exit 0.
- `FishNetSpikeSceneBuilder.Build` → `Assets/Scenes/FishNet_Spike.unity` with
  Tugboat HUD + `HostCommandGate` canned flick + settle pose RPC.
- Dual-instance pose delta: pending operator.
- PlayMode `HostCommandGateFlickSettlePlayModeTests` covers gate + host PhysX settle without runtime NetworkObjects.
