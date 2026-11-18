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
    Gameplay/      Desk rig, eraser bodies, flick, fall and settle detection, hitstop, MatchDirector, CPU seat turns
    Input/         Input System actions to FlickCommand
    CameraRig/     Orbit, overhead and close-up cameras
    View/          VFX, shake, fall cutscene
    UI/            HUD, hand, deck builder (UI Toolkit)
    AI/            Shot-planner stub. The seat that plays a turn lives in Gameplay.
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
The home strip `home-tip` reads "机から落としたらストックが1つ減る。最後まで残った人が勝ち。カードは出しても出さなくてもいい。出したらドラッグではじく。" until the player taps わかった. That tap writes `eraserwars.firstCoach` as `1`. Match start does not read `eraserwars.firstCoach`, and the start button stays enabled while the strip is up. Pause rules use the same line.
Options keeps the UI at large scale. `GameSettings.UiScale` is `1.2`, and `SnapUiScale` folds a saved `0.8` or `1` to `1.2` on load. The options screen shows 大. It has no small or standard control.
Options writes `eraserwars.reduceMotion` as `0` or `1`. The value `1` holds the edge glow at a dim constant. `EdgeSlowmoCue.ReduceMotionPreferred` reads that key.
Options writes `eraserwars.seatPalette` as `0` or `1`. The value `1` paints seats blue, magenta, orange, and gold on the erasers, the knock praise, and the match HUD. The HUD root gains the class `cb-seats`. The value `0` keeps red, blue, green, and yellow.

## CPU seats

`eraserwars.cpuSeats` is a PlayerPrefs bit mask read by `MatchDirector.StartHotseat`. Bit 0 is seat 0. The default is 0, so every hotseat seat stays human.
`MatchDirector.SetCpuSeats` pins a mask on that director and later matches on it ignore the pref.
A marked seat ignores aim and card play. The Pass button and the hand stay off for that seat only.
The seat waits out the turn callout, then waits `CpuTurn.ThinkSeconds` (0.4 seconds).
When `round + seat` is even, it flicks through `SubmitFlick` with drag `0.18`, direction `(0.15, 1)`, and contact Y `0.85`.
When that sum is odd, it passes. Round 1 seat 0 passes. Round 1 seat 1 flicks.
If the flick applier or the eraser body is missing, that visit passes instead.
`Assets/_Project/AI/CpuPlanner.cs` is unused by this path.

## Match log

`MatchLogWriter` writes one file when a hotseat match reaches `Phase.MatchEnd`.
`MatchDirector` writes that file from `Update`. `StartHotseat` writes the same match if that write has not succeeded. `Rematch` and the debug Hotseat buttons call `StartHotseat`, so the finished match is stored before the next one starts.
In the editor the directory is `playtests/match-logs/`. A player build uses `Application.persistentDataPath/MatchLogs`.
Each file records the match id, start and end in UTC, `WinMode`, stock count, point-round cap, ending round, winner seat or a draw, and each seat's stocks, points, and eliminated round.
P3 can measure match length from the two timestamps and group matches by `WinMode`. Comeback counts and one-punch counts are not in this file.

`ReplayStub` overwrites `last-flick.txt` when `MatchDirector.SubmitFlick` accepts a flick.
The file is `Application.temporaryCachePath/replay/last-flick.txt`. In the editor that folder is `Temp/replay`.
The text stores the seat, aim vector, drag power, and contact offset from that `FlickCommand`.
`SubmitFlick` reads the file back and prints it in the console.
A flick the reducer rejects leaves the previous file in place.
The file records that accepted command so a smoke read can print it.

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

`CardGlance.Primary` maps the first effect on a card to a coarse `CardSymbol` such as time, weapon, or desk.
`CardGlance.IconKey` is that effect string. Two cards share a pictogram only when the effect strings are the same.
`CardFaceBuilder.Paint` draws the pictogram on a collection tile, on a match hand card, on the card cut-in, and on the large collection showcase.
The card tab replaces the character still with that showcase. Under it are two placeholders labeled 付与演出 and プレイ演出.
Card family filter chips use the same frame colors as the faces. Kiai is red, homework is blue, seat is green, stationery is orange, cover is pink, and signature is gold.
Each common-card row in the deck builder uses the collection face, so the cost gem, family color, and effect icon match the collection.
The deck cost meter above the lists is unchanged by the face work.
The thick frame is the family color. Inside it are a white stroke, a black cost gem, a diagonal hatch, a large effect pictogram, and a white name plate. Those rules live in `Assets/UI/CardFace.uss`.
The hatch is a 128 pixel tile with point filtering. `UiImageUtil.ApplyPattern` repeats that tile at the texture's pixel size.
Glyph vectors live in `Assets/_Project/UI/CardIcons/`.
`tools/generate_card_icons.py` writes those files and `CardIconSvg.cs`.
The running game rasterizes the same markup. It does not depend on an SVG importer.
A contact sheet of the six families is in `docs/prototypes/card-tcg/contact-sheet.png`.

## Character cards

Character name cards (Collection and the seat picker) share `CharacterTileBuilder`.
A card is portrait (142x210) with a peach border, a round face with a black ring, the class line, then the name. The archetype diamond sits in the top-right corner.
Faces are 512 px square head-and-collar crops baked from the stills by `tools/bake_faces.py` into `Assets/Art/Characters/Faces/`.
`EraserWars > Import Character Stills` assigns them to `CharacterDefinition.FacePortrait`, and the UI draws them with `background-size: cover`.
The crop table and the card spec are in `docs/decisions/ADR-0010-character-card-face-crops.md`.

## Deck save

The deck screen is an open binder on a wood desk. The left page is the character. The right page is that character's common cards. The back button stays in the destination header.
The left page has a "player Pn" ribbon, a round face portrait, the name, an archetype pill, a stat table (mass, cost cap, strong and weak subjects), the bio, the two signature cards, and a horizontal strip of character face chips.
The right page has a headline with a status line, preset tabs 1 to 5, family filter chips, a two-column grid of common-card cells, and a footer with the cost bar and 保存.
Each cell shows the collection face with its cost gem, the name, a family badge, a one-line summary, minus and plus buttons with the copy count, and a check mark when the card is in the deck.
The family chips only filter the grid. They never change the deck. The grid keeps its scroll offset when a card is added or removed.
The layout rationale is in `docs/decisions/ADR-0011-deck-binder-layout.md`. The rules, presets, cost budget, and save path are unchanged.
`DeckRules.Evaluate` checks the working list before that list is stored.
保存 stays disabled while the check fails, and the cost meter shows the reason.
The check fails when the eraser is missing, when the list holds more than six commons, when one card passes its copy limit, or when the build cost passes that eraser's cap.
A legal edit is stored on the hotseat seat immediately.
The same edit is written to that character's active preset.
An illegal edit stays on the working list only.
`DeckPresets` keeps five named presets for each character id. The store is JSON in PlayerPrefs at `ew.deck.presets.v1`.
Tabs 1 to 5 select the active preset. A blank tab starts empty.
Choosing another character loads that character's active preset into the working list. When the check passes, `HotseatLoadout.Commons` for the seat becomes that preset.
Choosing a character who has no saved preset fills the working list with the starter commons, cards 1, 5, and 12.
Leaving the deck screen stores the working list only when the check passes.
The match reads `HotseatLoadout.Commons` for each seat.
Picking a character on the home strip copies that character's saved preset into the seat when the check passes.

## Card cut-in

Playing a card shows a tilted banner over the match and plays a slash. A signature card uses a brighter slash at the same SE volume.
The banner names the seat and the card, and paints the card face with `CardFaceBuilder` on `CardSurface.CutIn`.
A signature card holds for 1.2 seconds. A common card holds for 0.8 seconds. Each of those times includes a 0.2 second slash in and a 0.2 second slash out.
While the banner is up, the turn circle, the turn announce, the hand, and the pass button hide, and the turn clock does not move. The announce's one second waits until the banner ends. After the banner, the hand stays hidden because that turn already played its card. An empty hand stays hidden too.
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

## Language

`UiText` in `Assets/_Project/UI/UiText.cs` holds the start menu and match HUD lines.
Each `UiKey` has a Japanese line and an English line.
The game shows Japanese until a caller uses `UiLocale.Set`.
That call stores `ja` or `en` under the PlayerPrefs key `eraserwars.locale`.
The options page has no language row.
`StartMenuController` and `MatchHudController` call `UiLocale.Load` when they enable, then read `UiText` for the lines they draw.
Deck status, collection detail, and seat names stay in the Japanese written in those controllers.

## Classroom scene

The eraser prefab uses `Mat_EraserBase` with shader `EraserWars/EraserToon`.
Lit faces keep the seat color.
The shadow face is 58 percent of that color.
A 1.2 mm shell draws the ink edge.
`MatchDirector` still writes `_BaseColor` for each seat.
`ActiveEraserCue` still writes `_EmissionColor` and draws the ring.

1. In Unity, run `EraserWars > Build Classroom Casual Scene` (also builds UI icons). It writes `Assets/Scenes/Classroom_Casual.unity` as a Heisei ordinary classroom at about **7 m × 9 m × 3 m** (short chalkboard wall, deeper room), with a **2×2 four-desk arena** (0.6×0.4 m each), yard-side windows (4 bays / 5 pillars), corridor 引き違い戸 (~1.8 m bays) + high 連窓, one rear cleaning locker plus a long bayed storage shelf, chalkboard wall, paired prop desks (2-desk units along depth), and **full hide** of occluding walls/ceilings plus mounted props (chalk tray, posters, corkboard, locker, fluorescents, curtains). Looking in through open window/door bays also hides that wall via separation planes ([ADR-0009](docs/decisions/ADR-0009-start-screen-and-classroom-look.md)). UITK uses `ScaleWithScreenSize` + **Expand** and %-based USS so Full HD / 4K / free aspect stay readable.
2. Enter Play Mode. A full-screen **Start Menu** (UI Toolkit, high-energy CTA, five icon tiles) appears first. Pick 2P or 4P. The seat strip under that choice shows the default roster. Click a seat, then an unlocked face tile, to change who sits there. **マッチ開始** without that click keeps the default roster. Escape closes the face list when it is open. During a match, Escape or **メニュー** opens a pause card on the HUD. The card pauses the turn clock, lists the rules, each seat's stock or points, and a short options line, then offers **つづける** and **メインメニューへ**. **つづける** closes the card and runs the clock. **メインメニューへ** opens this start menu in the suspended state, where **つづける** resumes and **あたらしいマッチ** starts over. The start UIDocument deactivates for the match so the HUD can take the panel. Deck, collection, options, and the secret room fill the panel with a 4% margin. Each page scrolls inside its own ScrollView, so the classroom stays still. Character strips, the seat strip, and deck card rows scroll sideways when they overflow.
3. Match HUD keeps the classroom in the middle. A pink rail across the top holds a score pill per seat (pink, blue, green, orange for P1 to P4) and a center circle. The circle reads `RoundN` and the remaining seconds. Its outer border is the active seat's color, and an inner arc fills with the time left in that same color. The arc is empty while the clock is idle, including during a flick. At 5 seconds or less the digits turn red. A centered 72px line still shows `P# のばん` for 1 second when a seat receives the turn. That line hides while a card cut-in or the finish card is up, and that second waits while the cut-in is up. During TurnStart and CardSelect a caption reads `マウスを P# にわたして`. During Aim, once the hand is hidden, that caption reads `ねらえ!`. The matching pill shows `いま` and scales up. In points mode each pill shows that seat's score, for example `1点`, in the seat color, and hides the stock hearts. Stock mode keeps the hearts and hides the score. The circle hides at MatchEnd so the finish card stays clear. The hand fans above the bottom edge while the current human seat is in CardSelect and still holds cards. After the card is played, or when the hand is empty, or on a CPU seat, the cards use `display: none`. **メニュー** sits at the bottom left and **パス** at the bottom right. The HUD only shows after the match starts. During TurnStart, CardSelect, and Aim, that eraser eases to 1.14 times its size, pulses the same color, and draws a thin ring. Left-drag in the Game view (outside the HUD) to flick. A press that hits the current eraser sets the contact point, and an off-center hit spins harder. A press that misses the eraser stays at the center. `FlickTuning.SpinGain` scales that yaw (1 keeps the geometric lever). Space or the Pass button skips the turn.

On a gamepad, the left stick aims on the camera's horizontal plane. Up flies away from the camera, and how far you tilt the stick is the power. Hold the right trigger, the left trigger, or the south button (A on Xbox, Cross on PlayStation), then release to flick. The aim line shows while you hold, and the contact stays at the center, so a pad flick does not spin from the edge. East (B on Xbox, Circle on PlayStation) passes, and it clears the aim line first. The right stick does not orbit the camera. The turn clock auto-passes on timeout. MatchEnd covers the classroom with a light dim and a finish card. The winner line is 88px, larger than the turn callout, in that seat's color, under a 44px seat-color bar. A draw stays neutral and reads `ひきわけ!`. The line under the winner shows each seat's stock or points. The next line shows that match's XP as `けいけん +10  ·  ごうけい N`. `CareerXp` adds 10 to the PlayerPrefs int `eraserwars.xp` once per finish card. A rematch adds another 10. The card does not unlock a character. **再戦** is the filled button and calls `Rematch`. **メニューへ** is the outline button.
4. Agent one-shot flags under `Temp/` (consumed by `OpenSceneFlagWatcher`) drive Play without fragile toolbar clicks: `enter-play.flag`, `start-hotseat-2.flag`, `flick-current.flag`, `knock-out.flag`, `rematch.flag`, `dump-ui.flag`, `heartbeat.flag`. Decision log: `.audit/playable-hotseat.tsv`. With one editor already open on this project, run `tools/run-hotseat-e2e.ps1`. It drops `enter-play.flag`, `start-hotseat-2.flag`, `flick-current.flag`, and `dump-ui.flag` in that order, then appends one row to `.audit/hotseat-e2e.tsv`. `tools/run-hotseat-e2e.ps1 -SelfTest` checks that sequence without Unity.
5. Right-drag orbits the camera during a match and on the home menu. Tab shows the desks from above. C shows the active eraser up close. Press the same key to return to the orbit, or the other key to switch view. The wheel zooms the orbit during a match, and on the home menu except while the pointer is over a scroller. On the home menu the camera yaws 8 degrees per second around the desks. That yaw stops when reduce-motion is on, when a destination page is open, or when the match starts. Destination pages ignore right-drag and the wheel, so their ScrollViews do not move the classroom. Default framing fills the view with the play desks. Prefer Game view **Free Aspect** at Scale **1x** (`EraserWars > Use Game View Free Aspect`, auto-applied); fixed Full HD/4K entries crop the editor panel but built players scale correctly. Tone follows the hobby-magazine battle book named in Q8, not the onomatopoeia “ころころ”. After a flick settles, orbit framing from the previous turn is kept (chase focus only is cleared).
6. When your eraser hits another during Simulating, physics freezes for 0.08 seconds. Desk and floor hits do not freeze. Another eraser hit inside 0.15 seconds is ignored. Close-range Stay contacts project the hit normal onto XZ so the hittee still launches on the desk (vertical PhysX normals would otherwise be wiped).
7. During Simulating, an eraser within 0.05 m of the outer desk edge, or dropping through the 0.05 m slice under the desk marker, slows time to 0.4 for 0.22 seconds. While that slow runs, MatchHud paints a dark rim vignette that clears when the slow ends (reduce-motion prefers a flat edge). An eraser-hit freeze cancels that slow, and the next one waits until every eraser has left the zone and 0.6 seconds from the start have passed.

`EraserWars > Import All Content CSV` imports cards (`Assets/Data/Csv/cards.csv` → `Assets/Data/Cards/`), erasers / characters (`erasers.csv`, `characters.csv` → `Assets/Data/Erasers/`, `Assets/Data/Characters/`), and refreshes `Assets/Resources/EraserWars/ContentCatalog.asset`. The Start Menu **コレクション** panel browses that catalog (20 characters, 50 cards, school-motif filters).
`CharacterUnlocks.Grant` stores an extra character id in PlayerPrefs.
The next character strip draws that face unlocked and adds `unlock-flash` for 650 ms.
`CharacterAccess.IsUnlocked` is the check for the collection, the seat strip, and `HotseatLoadout.TryAssignUnlocked`.
A character is unlocked when `UnlockedByDefault` is set, or when `CharacterUnlocks.IsEarned` contains that id.
The start menu debug toggle starts off. Match start and resume copy that toggle into `MatchDirector.showDebug`, so the IMGUI debug chrome stays hidden until the toggle is on. The toggle calls `CharacterAccess.SetDebugUnlockAll`.
While that flag is on, every character is unlocked.
Turning the toggle off clears the flag, and `HotseatLoadout.ReleaseLockedSeats` puts a seat back on the default roster when that character is locked again.
An earned id stays available after the flag is off.
The collection shows eraser mass times 1000, so 0.02 is shown as 20.
`EraserDefinition.Mass` stays in kilograms.
`CharacterFlavor` holds the subject pair, the school, and the class for ids 0 through 19. `TileCaption` prints the school and class under the face. Teacher roles print the role instead, such as 留学生の先生役. `PassiveText` holds the passive line for that screen. The type mark sits on the top-right of the circle. Selecting a face updates that mark and does not add another tile.
The still frame uses the portrait's pixel aspect, clips with `overflow: hidden`, and switches among the still, a 3D model slot, and an eraser design slot.

## Secret room stub

The start menu button **ひみつのへや** opens a page on that menu.
The page has a join-code field, **コードをつくる**, and **コピー**.
**コードをつくる** writes a 6-character code from `ABCDEFGHJKLMNPQRSTUVWXYZ23456789`.
That set skips 0, 1, I, and O.
**コピー** copies the field when the text is one of those codes.
The page does not start a host, a client, or a match.

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
