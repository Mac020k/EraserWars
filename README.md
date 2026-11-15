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

1. In Unity, run `EraserWars > Build Classroom Casual Scene`. It writes `Assets/Scenes/Classroom_Casual.unity` with one joined `DeskArena` (1.2×0.8), four corner legs sunk into the floor, a room shell (walls, chalkboard, baseboards), warm key + fill light, a kill zone, and four spawn points.
2. Enter Play Mode. `OrbitCamera.FrameDeskOverview` frames the full desk (distance 4.55, pitch 36, FOV 48) so legs and floor contact stay in view. `ClassroomBootstrap` starts Hotseat with its `players` field (2 by default).
3. Debug HUD is broadcast-style: top banner for round/seat/time, left STOCKS panel with accent rows. Left-drag in the Game view (outside the chrome) to flick. Space or Pass skips the turn.
4. Right-drag orbits the camera. Tab toggles top-down. Wheel zooms. Keep Game view Scale at **1x** — a near-zero Scale makes the desk look missing.
5. When your eraser hits another during Simulating, physics freezes for 0.08 seconds. Desk and floor hits do not freeze. Another eraser hit inside 0.15 seconds is ignored. Close-range Stay contacts project the hit normal onto XZ so the hittee still launches on the desk (vertical PhysX normals would otherwise be wiped).
6. During Simulating, an eraser within 0.05 m of the outer desk edge, or dropping through the 0.05 m slice under the desk marker, slows time to 0.4 for 0.22 seconds. An eraser-hit freeze cancels that slow, and the next one waits until every eraser has left the zone and 0.6 seconds from the start have passed.

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
