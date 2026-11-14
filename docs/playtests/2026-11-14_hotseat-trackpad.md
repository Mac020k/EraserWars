# Playtest 2026-11-14 Hotseat trackpad feel

Operator: agent (Computer Use)
Scene: `Classroom_Casual`
Players: 2 hotseat

## Checks

| Check | Result |
| --- | --- |
| Erasers visible (red / blue) | Pass |
| LMB drag on empty desk starts flick | Pass (Phase → Simulating) |
| Hint text shows drag anywhere | Pass |
| Turn advances after settle | Pass (Seat 0 → 1) |
| Flight readable (not instant teleport) | Pass (inferred from Simulating duration before Seat flip) |

## Tuning after first agent pass

Coast with `maxSpeed=0.85` and `speed=0.45` was ~0.25 m (too short for desk play).
Raised to `jMax=0.12`, `speed=0.65`, `maxSpeed=1.8` before this verify.

## Notes

Aim line visibility not isolated in the recording (drag was quick).
Right-orbit not re-tested this pass.
Error Pause preference still sticky in the editor; prefer `EditorApplication.isPlaying` toggles for automation.
