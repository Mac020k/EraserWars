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
`Gameplay` references `Core` and `Cards`.
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
2. Enter Play Mode. A full-screen **Start Menu** (UI Toolkit, CoroCoro-energy CTA, five icon tiles) appears first. Pick 2P/4P, then **マッチ開始**. Escape returns to the menu. The start UIDocument deactivates for the match so the HUD can take the panel.
3. Match HUD is broadcast-style (player pills with stocks, turn clock, phase text). It only shows after the match starts. Left-drag in the Game view (outside HUD) to flick. Space or Pass skips the turn. The turn clock auto-passes on timeout. MatchEnd shows a winner banner (`P# のかち!` / `ひきわけ!`); **再戦** calls `Rematch`.
4. Agent one-shot flags under `Temp/` (consumed by `OpenSceneFlagWatcher`) drive Play without fragile toolbar clicks: `enter-play.flag`, `start-hotseat-2.flag`, `flick-current.flag`, `knock-out.flag`, `rematch.flag`, `dump-ui.flag`, `heartbeat.flag`. Decision log: `.audit/playable-hotseat.tsv`.
5. Right-drag orbits the camera. Tab toggles top-down. Wheel zoom is proportional and snappy. Default framing fills the view with the play desks. Prefer Game view **Free Aspect** at Scale **1x** (`EraserWars > Use Game View Free Aspect`, auto-applied); fixed Full HD/4K entries crop the editor panel but built players scale correctly. Tone is **コロコロコミック** (magazine), not “ころころ” as onomatopoeia (Q8 clarification). After a flick settles, orbit framing from the previous turn is kept (chase focus only is cleared).
6. When your eraser hits another during Simulating, physics freezes for 0.08 seconds. Desk and floor hits do not freeze. Another eraser hit inside 0.15 seconds is ignored. Close-range Stay contacts project the hit normal onto XZ so the hittee still launches on the desk (vertical PhysX normals would otherwise be wiped).
7. During Simulating, an eraser within 0.05 m of the outer desk edge, or dropping through the 0.05 m slice under the desk marker, slows time to 0.4 for 0.22 seconds. An eraser-hit freeze cancels that slow, and the next one waits until every eraser has left the zone and 0.6 seconds from the start have passed.

`EraserWars > Import Cards CSV` reads `Assets/Data/Csv/cards.csv` and creates or updates `Assets/Data/Cards/Card_<id>.asset` for each row.

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
