# ADR-0005 reducer で試合を進める

- 状態: Accepted
- 日付: 2026-11-14
- 関連: Q18、Q81、[02_core-loop.md](../gdd/02_core-loop.md)

## 文脈

同じ試合のルールを、ホットシート、オンラインのホスト、リプレイ、テストの4か所で使う。
ルールを MonoBehaviour や Fish-Net のコールバックに散らすと、4か所で食い違う。
Q18 の同順位のように、細かい裁定が多い。

## 決定

試合の進行は純粋関数 `MatchReduce.Reduce(MatchState, MatchEvent) -> MatchState` だけが進める。

- `MatchState` と `PlayerState` は不変の record で、`System.Collections.Immutable` を使う。
- `MatchEvent` は抽象 record の階層。`TurnStarted`、`CardPlayed`、`Flicked`、`Passed`、`EraserFell`、`SimulationSettled`、`TimerExpired`、`PlayerDisconnected`、`PlayerReconnected`。
- フェーズは `TurnStart`、`CardSelect`、`Aim`、`Simulating`、`Resolve`、`RoundEnd`、`MatchEnd` の7つ。
- reducer は `Keshipin.Core` にあり、UnityEngine、Fish-Net、Steam を参照しない。

## 結果

- ホストが reducer を回し、クライアントは結果を受け取る。
- 試合ログはイベント列と各ターンの静止姿勢になる。リプレイはそれを畳み直す。
- ルールは Unity なしでテストできる。Unity 上の EditMode テスト (`Assets/Tests/EditMode`) と、`tools/run-core-smoke.ps1` の `dotnet test` の2か所で回す。
- Unity には `System.Collections.Immutable` が標準で入っていないので、`Assets/Plugins/System.Collections.Immutable.dll` を同梱している。`.gitattributes` は `*.dll` を LFS に載せる。
