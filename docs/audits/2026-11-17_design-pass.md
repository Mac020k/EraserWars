# Design pass 2026-11-17

Source audits (read-only) against `main` around `838e39b` / `7bdf4cc`.

| Lens | Focus |
| --- | --- |
| Sakurai | Risk/return, first minutes, clarity, feedback, skip waits, visible progress |
| Soft / cozy craft | One classroom fantasy, pleasant progress, ritual, low pressure |
| HCI first session | Nielsen + shared-desk hotseat |
| Round-2 compliance | Prior UI instructions |

## Verdict on Round-2 instructions

About **92%** reflected after stills `08–19` landed in `7bdf4cc`. Remaining partial: destination pages keep a **4%** margin (near-full, not edge-to-edge). Doc files still mention magazine names; UI chrome does not.

## Shared P0 (fix first)

1. **Debug chrome defaults ON** — English bar + left panel cover the real HUD, steal flicks, and can restart the match. Defaults live in `StartMenuController.debugModeEnabled`, `MatchDirector.showDebug`, and `ClassroomSceneBuilder` writing `showDebug = true`.
2. **Win condition not stated** — Home/pause tip is procedure-only (“play a card, then flick”), not “knock off the desk / last eraser wins”.
3. **Aim not labeled after the hand hides** — Existing `ねらえ!` copy exists in `UiText` but is not painted on the Aim phase.
4. **Face crop merge regression** — `998f974` restored `-unity-background-scale-mode: scale-and-crop` on `.char-tile-art`, which overrides the 780% close crop. Fix: keep 780% / top 2% only (no scale-and-crop).

## Shared P1 (next)

| Issue | Notes |
| --- | --- |
| Unlocks / XP disconnected | CSV promises wins, packs, duties; match end only adds XP total |
| Open hands (GDD) | Only the active seat sees cards in CardSelect |
| Points mode UI | Hides stocks; round is not `n/8`; self-KO scoring gap vs GDD |
| Cut-in not skippable | 0.8–1.2s hard wait |
| Pause does not freeze settle physics | Falls continue under a thin veil |
| Hit feedback undersized | Small kick / confetti; self-KO silent |
| Min launch on soft contact | Nudges become launches |
| Still style split | Cel 00–01, kept 07 (older uniform), watercolor 08–19 without erasers; borrowed file-name vibes |
| Hotseat handoff copy | `このきを` is unclear; should be mouse/device handoff |

## Soft-craft priorities (art / tone)

- One age, one drawing language, eraser in hand for every still.
- Rename display/file identities away from borrowed surnames where they read as other IPs.
- Collection leads with school/class/bio before physics numbers.
- Replace ROUND / P1 / English debug with classroom ritual language once chrome is clean.
- Hide “準備中” / secret-room net stubs from the first path.

## Already aligned

Fast start into a real match, stock hearts, rematch, seat colors + colorblind option, reduce-motion, binder deck + five presets, notebook options, thin scrollbars, large UI scale only.

## Merge regressions (UI defect hunt)

| Severity | Issue |
| --- | --- |
| High | Face crop: scale-and-crop vs 780% |
| Med | Deck shows locked character name/stats; empty unsaved presets vs starter on character change |
| Med | Pause leaves hand visible under thin veil; yield timer / physics keep running |
| Med | Character filter can leave stale detail when strip empties |
| Low | Hidden hand title; idle timer ring looks full white; UiText orphans |

## Fix queue

1. Debug OFF by default (+ scene builder).
2. Face crop: drop scale-and-crop; unify 780%.
3. Win-condition tip + clearer pass hint + Aim cue.
4. Deck lock masking; preset empty/starter consistency; pause hides hand + freezes settle/yield.
5. Wire unlock grants from match results; show the next unlock, not a naked XP total.
6. Points-mode honesty; skippable cut-in.
7. Still roster pass (age / eraser / names).
8. Open-hand HUD if GDD stays public hands.
