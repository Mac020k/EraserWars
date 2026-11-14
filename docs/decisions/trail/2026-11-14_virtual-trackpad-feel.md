# Decision trail: virtual trackpad + readable flick

Date: 2026-11-14

## Goal

Operator can follow eraser flight by eye, and can flick by dragging anywhere
(Monster Strike / Shironeko style), without clicking the mesh first.

## How (subsystem)

- Input: `HotseatFlickInput` → `FlickCommand` → `MatchDirector.SubmitFlick` →
  `FlickApplier` → `EraserBody.ApplyFlick` (PhysX impulse).
- Strength: `J = power * jMax * L^1.5` in `FlickMath`.
- Slide: `linearDamping ≈ 1.5 / speed` on the rigidbody.

## Architect pick

| Option | Pros | Cons | Verdict |
| --- | --- | --- | --- |
| A. Keep mesh hit + lower jMax only | Smallest diff | Misses still feel bad | Reject |
| B. Screen-space trackpad → same FlickCommand | Familiar mobile feel; keeps net/reducer shape | Spin from contact point deferred | **Accept** |
| C. New world-space invisible pad collider | Easy raycast | Extra scene object; camera-dependent | Reject |

Principles applied: Experience First (delight over mesh-hit convenience),
Model the Domain (gesture maps into existing FlickCommand, not a parallel API).

## Defaults chosen

- First pass: `jMax` 0.09, `speed` 0.45, `maxSpeed` 0.85 (coast ~0.25 m, too short)
- After Play arithmetic + verify: `jMax` 0.12, `speed` 0.65, `maxSpeed` 1.8
- `maxDragPixels` ≈ 180
- Contact offset (0,0) for v1 trackpad

## Verify (Computer Use)

Empty-desk LMB drag → `Phase=Simulating` → settle → `Seat=1`. Pass.

## Throughput checkpoint

- Blocking: keep FlickCommand contract; do not break MatchReduce.
- Streams: one owner for Gameplay input + feel (shared MatchDirector).
- Shared state: director current eraser only; no parallel writers.
- Decomposition: one worker (coupled files).

## Next after verify

Commit feel unit, then P1 playtest log + continue milestones (Fish-Net spike still pending Unity install).
