# Eraser Wars (消しピンウォーズ)

Eraser Wars is a 2 to 4 player party game for Steam (Windows).
Each player owns an eraser tied to a character.
On your turn you may play one card, then flick your eraser to knock rivals off a classroom desk.
A fall costs one stock, and the last player with stock wins.
The target release is August 2027, free to play with paid DLC.

Design documents live in [`docs/`](docs/00_index.md) and are written in Japanese.

## Requirements

- Unity Editor `6000.6.9f1`. The scripts in `tools/` expect it at `C:\Program Files\Unity\Hub\Editor\6000.6.9f1`.
- Git LFS. `.gitattributes` routes `*.fbx`, `*.blend`, `*.png`, `*.jpg`, `*.psd`, `*.wav`, `*.ogg` and other binaries through LFS.
- Windows 10 or 11.
- Optional: the .NET 8 SDK, for `tools/run-core-smoke.ps1`.

## Setup

1. Install Git LFS and run `git lfs install` once per machine.
2. Clone the repository.

   ```powershell
   git clone https://github.com/Mac020k/EraserWars.git
   ```

3. Open the `EraserWars` folder in Unity Hub with `6000.6.9f1`.
4. Wait for the first package resolve. `Packages/` and `ProjectSettings/` are tracked, so a fresh clone opens without manual setup.

The English product name is `Eraser Wars` and the company name is `Mac020k`. Both are set in `ProjectSettings/ProjectSettings.asset`.

## Folder layout

```text
Assets/
  _Project/        Game code, one assembly definition per folder
    Core/          Match state, the reducer, rule sets, flick math. No UnityEngine reference.
    Cards/         Card, eraser, character, stage and rule set definitions, card resolver
    Gameplay/      Desk rig, eraser bodies, flick, fall and settle detection, hitstop, MatchDirector
    Input/         Input System actions to FlickCommand
    CameraRig/     Orbit, overhead and close-up cameras
    View/          VFX, shake, fall cutscene
    UI/            HUD, hand, deck builder (UI Toolkit)
    AI/            CPU shot planner
    Audio/         BGM loop, UI click and flick cues, volume prefs
    Net/           Network boundary: command validation, disconnect policy, Fish-Net spike HUD
    Steam/         Lobby and Steam transport
    Persistence/   Save data, match log
    Editor/        CSV import, scene builder, Fish-Net spike builder, debug menus
  Data/            ScriptableObjects and Csv/cards.csv
  FishNet/         Embedded Fish-Net 4.7.4 (Tugboat only; Yak/Demos removed)
  Plugins/         System.Collections.Immutable.dll, used by Core
  Art/ Audio/ Prefabs/ Scenes/
  Tests/
    EditMode/      Reducer tests
    PlayMode/      Physics and flick tests
docs/              Design vault (GDD, ADRs, research, roadmap, playtests)
tools/             Batch scripts for tests and builds
```

Assembly references point one way.
`Core` references nothing.
`Cards` references `Core`.
`Gameplay` references `Core`, `Cards`, and `Audio`.
`View`, `AI` and `Net` reference `Gameplay`.
`UI` references `Core`, `Cards`, `Gameplay` and `View`. `Steam` references `Net`.
`Core` uses `noEngineReferences` and never references Fish-Net or Steam, so card and rule logic runs in tests with no network.
`Assets/csc.rsp` sets `-langVersion:10` so Core can use `record struct`. `IsExternalInit.cs` polyfills init-only setters for netstandard.

## Match core

`EraserWars.Core` holds the match rules with no UnityEngine reference.
`MatchReduce.Reduce` folds one `MatchEvent` into an immutable `MatchState`.
A fall spends one stock for the victim whether the last touch was theirs or someone else's.
Stock that hits zero stamps `EliminatedRound` with the current round, so falls in one round share a rank.
The round continues while more than one player still has stock, or a later seat in `TurnOrder` has not acted.
After the last seat, one remaining survivor ends the match. Otherwise the next round opens on the next player who can act.
`TurnTimeSeconds` of 0 ignores `TimerExpired`. A disconnected player is skipped when their turn opens.
`RuleSet.StockCount` is the starting stock. `MatchFactory.CreateHotseat` uses that count, empty hands, and turn order `0..n-1`.
The start menu segment `mode-stock` / `mode-points` stores `eraserwars.winMode` in PlayerPrefs. `0` is stock and `1` is points.
`MatchDirector.StartHotseat` reads that key for both 2-player and 4-player hotseat, then applies the saved turn time.
Points loads `RuleSet.PointsTrial`, which ends after 8 rounds. Stock loads `RuleSet.CasualStock`. Rematch and the debug Hotseat buttons read the same key.

## Match log

`MatchLogWriter` writes one file when a hotseat match reaches `Phase.MatchEnd`.
`MatchDirector` writes that file from `Update`. `StartHotseat` writes the same match if that write has not succeeded. `Rematch` and the debug Hotseat buttons call `StartHotseat`, so the finished match is stored before the next one starts.
In the editor the directory is `playtests/match-logs/`. A player build uses `Application.persistentDataPath/MatchLogs`.
Each file records the match id, start and end in UTC, `WinMode`, stock count, point-round cap, ending round, winner seat or a draw, and each seat's stocks, points, and eliminated round.
P3 can measure match length from the two timestamps and group matches by `WinMode`. Comeback counts and one-punch counts are not in this file.

## Card effects

`Assets/Data/Csv/cards.csv` lists each card's family, build cost and an effect string that `EffectSpecParser` turns into an `EffectSpec`.
`CardResolver.Apply` records each effect as an `ActiveEffect` whose `SpecKind` string encodes its parameters, and `TickEffects` ages them after each settle.
`MatchEffectRules` holds the pure match-state rules for the families that act on turns and stocks.
- `ModifyRule:OpponentTurnTime:x:1` scales the next rival's turn clock once at handover, then removes itself.
- `ModifyRule:SelfTurnTime:x:1` scales the current clock as soon as the card is played.
- `GrantShield:absorb:hits` lets the owner survive that many falls. The saved eraser respawns at a clear spawn point and the shield lasts a full round of rival turns.
- `SwapPositions` swaps the owner's eraser with the next living rival in turn order.
- `RestoreStock:n` gives back up to `n` stocks, capped at `RuleSet.StockCount`.

## Audio

`AudioBus` starts a generated BGM loop as soon as play mode begins. A UI button click plays `ui-click`. An accepted flick plays `flick`. An accepted eraser hit plays `eraser-hit`. A floor contact plays `floor-hit`. A card cut-in plays `card-play`. A signature cut-in plays the brighter `card-play-signature`. Volumes are percents in `PlayerPrefs`, owned by `EraserWars.Audio.GameSettings`. The keys match the options sliders `opt-bgm` and `opt-se`. See [`Assets/_Project/Audio/README.md`](Assets/_Project/Audio/README.md).

## Card faces

`CardGlance` maps the first effect on a card to a `CardSymbol`.
`CardFaceBuilder.Paint` draws that symbol on a collection tile, on a match hand card, and on the card cut-in.
Each common-card row in the deck builder uses the collection face, so the cost gem, family color, and effect icon match the collection.
The deck cost meter above the lists is unchanged.
Family color, the inner frame, the cost gem, and the effect badge live in `Assets/UI/CardFace.uss`.
Glyph vectors live in `Assets/_Project/UI/CardIcons/`.
`tools/generate_card_icons.py` writes those files and `CardIconSvg.cs`.
The running game rasterizes the same markup. It does not depend on an SVG importer.
A contact sheet of the six families is in `docs/prototypes/card-tcg/contact-sheet.png`.

## Card cut-in

Playing a card shows a tilted banner over the match and plays a slash. A signature card uses a brighter slash at the same SE volume.
The banner names the seat and the card, and paints the card face with `CardFaceBuilder` on `CardSurface.CutIn`.
A signature card holds for 1.2 seconds. A common card holds for 0.8 seconds. Each of those times includes a 0.2 second slash in and a 0.2 second slash out.
While the banner is up, the turn callout, the turn announce, the hand, and the pass button hide, and the turn clock does not move. The announce's one second waits until the banner ends.
Flicks and passes from the player wait until the banner ends. `SubmitFlick` still accepts a command, so a test can play a card and flick in the same call.
The match is already in Aim under the banner. When the banner finishes, the player can aim.

## Tests

Close the Unity Editor first. Batch mode cannot open a project that an editor already has open.

```powershell
.\tools\run-tests.ps1                    # EditMode only (default)
.\tools\run-tests.ps1 -Platform PlayMode
.\tools\run-tests.ps1 -Platform All
.\tools\run-tests.ps1 -UnityExe "D:\Unity\6000.6.9f1\Editor\Unity.exe"
```

The script runs `Unity.exe -batchmode -runTests` once per platform.
It writes `TestResults/<Platform>-results.xml` and `TestResults/<Platform>-unity.log`, prints the total, passed and failed counts, and exits with code 1 if any test failed.
It also fails if Unity produced no results XML, and the log file says why.

`tools/run-core-smoke.ps1` runs the reducer tests in `tools/CoreSmoke` with `dotnet test`, without Unity.
The project compiles the `Core` sources directly, so it catches rule regressions in seconds.

## Classroom scene

1. In Unity, run `EraserWars > Build Classroom Casual Scene` (also builds UI icons). It writes `Assets/Scenes/Classroom_Casual.unity` as a Heisei ordinary classroom at about **7 m × 9 m × 3 m** (short chalkboard wall, deeper room), with a **2×2 four-desk arena** (0.6×0.4 m each), yard-side windows (4 bays / 5 pillars), corridor 引き違い戸 (~1.8 m bays) + high 連窓, one rear cleaning locker plus a long bayed storage shelf, chalkboard wall, paired prop desks (2-desk units along depth), and **full hide** of occluding walls/ceilings plus mounted props (chalk, corkboard, locker, fluorescents, curtains). Looking in through open window/door bays also hides that wall via separation planes ([ADR-0009](docs/decisions/ADR-0009-start-screen-and-classroom-look.md)). UITK uses `ScaleWithScreenSize` + **Expand** and %-based USS so Full HD / 4K / free aspect stay readable.
2. Enter Play Mode. A full-screen **Start Menu** (UI Toolkit, CoroCoro-energy CTA, five icon tiles) appears first. Pick 2P or 4P. The seat strip under that choice shows the default roster. Click a seat, then an unlocked face tile, to change who sits there. **マッチ開始** without that click keeps the default roster. Escape closes the face list when it is open. Otherwise Escape suspends the match and shows **つづける** (resume) plus **あたらしいマッチ**. The start UIDocument deactivates for the match so the HUD can take the panel. The **コレクション** page stays a fixed 900x700 box.
3. Match HUD is broadcast-style. Under the top bar, a seat callout names the current player at 52px in that seat's color (pink, blue, green, orange for P1 to P4). When a seat receives the turn, a centered 72px line shows `P# のばん` in that color for 1 second. The line hides while a card cut-in or the finish card is up, and that second waits while the cut-in is up. During TurnStart, CardSelect, and Aim the callout adds `このきを P# にわたして`. The clock in that callout shows seconds at 64px and turns red at 5 seconds or less. The matching player pill shows `いま`. The callout hides at MatchEnd so the finish card stays clear. The HUD only shows after the match starts. Left-drag in the Game view (outside the HUD) to flick. A press that hits the current eraser sets the contact point, and an off-center hit spins harder. A press that misses the eraser stays at the center. `FlickTuning.SpinGain` scales that yaw (1 keeps the geometric lever). Space or the Pass button skips the turn.

On a gamepad, the left stick aims on the camera's horizontal plane. Up flies away from the camera, and how far you tilt the stick is the power. Hold the right trigger, the left trigger, or the south button (A on Xbox, Cross on PlayStation), then release to flick. The aim line shows while you hold, and the contact stays at the center, so a pad flick does not spin from the edge. East (B on Xbox, Circle on PlayStation) passes, and it clears the aim line first. The right stick does not orbit the camera. The turn clock auto-passes on timeout. MatchEnd covers the classroom with a light dim and a finish card. The winner line is 88px, larger than the turn callout, in that seat's color, under a 44px seat-color bar. A draw stays neutral and reads `ひきわけ!`. The line under the winner shows each seat's stock or points. **再戦** is the filled button and calls `Rematch`. **メニューへ** is the outline button.
4. Agent one-shot flags under `Temp/` (consumed by `OpenSceneFlagWatcher`) drive Play without fragile toolbar clicks: `enter-play.flag`, `start-hotseat-2.flag`, `flick-current.flag`, `knock-out.flag`, `rematch.flag`, `dump-ui.flag`, `heartbeat.flag`. Decision log: `.audit/playable-hotseat.tsv`.
5. Right-drag orbits the camera. Tab shows the desks from above. C shows the active eraser up close. Press the same key to return to the orbit, or the other key to switch view. The wheel zooms the orbit only. Default framing fills the view with the play desks. Prefer Game view **Free Aspect** at Scale **1x** (`EraserWars > Use Game View Free Aspect`, auto-applied); fixed Full HD/4K entries crop the editor panel but built players scale correctly. Tone is **コロコロコミック** (magazine), not “ころころ” as onomatopoeia (Q8 clarification). After a flick settles, orbit framing from the previous turn is kept (chase focus only is cleared).
6. When your eraser hits another during Simulating, physics freezes for 0.08 seconds. Desk and floor hits do not freeze. Another eraser hit inside 0.15 seconds is ignored. Close-range Stay contacts project the hit normal onto XZ so the hittee still launches on the desk (vertical PhysX normals would otherwise be wiped).
7. During Simulating, an eraser within 0.05 m of the outer desk edge, or dropping through the 0.05 m slice under the desk marker, slows time to 0.4 for 0.22 seconds. An eraser-hit freeze cancels that slow, and the next one waits until every eraser has left the zone and 0.6 seconds from the start have passed.

`EraserWars > Import All Content CSV` imports cards (`Assets/Data/Csv/cards.csv` → `Assets/Data/Cards/`), erasers / characters (`erasers.csv`, `characters.csv` → `Assets/Data/Erasers/`, `Assets/Data/Characters/`), and refreshes `Assets/Resources/EraserWars/ContentCatalog.asset`. The Start Menu **コレクション** panel browses that catalog (20 characters, 50 cards, school-motif filters).

## Fish-Net spike

Fish-Net **4.7.4** is embedded under `Assets/FishNet` (not UPM). See [`Assets/_Project/Net/FishNetInstall.md`](Assets/_Project/Net/FishNetInstall.md) and [ADR-0008](docs/decisions/ADR-0008-fishnet-spike.md).

1. `EraserWars > Build Fish-Net Spike Scene` writes `Assets/Scenes/FishNet_Spike.unity` (SceneIds assigned, NetworkManager at root).
2. `EraserWars > Build Fish-Net Spike Windows Player` (or `tools/run-fishnet-dual-smoke.ps1 -Build`) writes `Builds/FishNetSpike/EraserWars.exe`.
3. Play the Editor scene (auto Host + canned flick via `FishNetSpikeAutoSmoke`) and launch the player (auto Client). Results land in `Temp/fishnet-dual-smoke-{host,client}.txt`.
4. Manual HUD path still works: **Start Host** / **Start Client** / **Fire canned flick**. Dual-smoke on 2026-11-15 recorded **Local vs host delta = 0.0000 m**.

## Build

```powershell
.\tools\build.ps1
```

The script writes `Build/Win64/Eraser Wars.exe` and logs to `TestResults/build-Win64.log`.
`Build/` and `TestResults/` are ignored by git.

## Documentation

Start at [`docs/00_index.md`](docs/00_index.md).
It lists the current phase, the next steps, and links to every design document.
