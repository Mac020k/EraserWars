# Playtest 2026-11-14 Hitstop (pending operator eye check)

Status: code landed in `27ec314` follow-up commit; visible freeze not yet confirmed in Play this session.

## Expected

1. Play `Classroom_Casual`.
2. Drag anywhere and release so your eraser hits the other.
3. Both should freeze ~0.08s, then continue.
4. Desk / floor hits do not freeze.
5. A second eraser hit inside 0.15s does not freeze again.

## Evidence so far

- Assembly rebuild included `EraserCollisionRelay` and `NotifyEraserCollision` (agent report).
- Core smoke still green after related work.
