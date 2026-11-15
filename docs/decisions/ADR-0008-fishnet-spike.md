# ADR-0008 Fish-Net 導入スパイク

- 状態: Accepted (compile + Tugboat scene path verified 2026-11-14)
- 日付: 2026-11-14
- 手順書: [FishNetInstall.md](../../Assets/_Project/Net/FishNetInstall.md)

## 決定

Fish-Net **4.7.4** (`a362752`) を `Assets/FishNet` に埋め込む。
UPM git URL (`#4.7.4`) は無料版の **Yak** スタブが CS エラーになるため不採用。
Yak フォルダは削除済み。LAN は **Tugboat** のみ使う。

## 実施結果 (実測)

| 項目 | 結果 |
| --- | --- |
| Unity 6000.6.9f1 batchmode compile | Exit 0 after Yak removal |
| EditMode tests | 18/18 passed (includes HostCommandGate) |
| PlayMode tests | 3/3 passed (includes HostCommandGate settle) |
| Spike scene builder | batch `-executeMethod EraserWars.Editor.FishNetSpikeSceneBuilder.Build` → `Assets/Scenes/FishNet_Spike.unity` (Exit 0) |
| HostCommandGate canned flick path | Wired in `FishNetSpikeSession` (server PhysX + ObserversRpc settle pose) |
| Dual-instance flick pose match | **PASS 2026-11-15** — Local vs host delta **0.0000 m** ([playtest](../playtests/2026-11-15_fishnet-dual-smoke.md)) |
| Windows player path | `Builds/FishNetSpike/EraserWars.exe` via `FishNetSpikePlayerBuild` |
| Auto smoke | `FishNetSpikeAutoSmoke` writes `Temp/fishnet-dual-smoke-{host,client}.txt` |

## 手順 (任意の再測)

1. Play `FishNet_Spike` (auto Host) と `Builds/FishNetSpike/EraserWars.exe` (auto Client)。
2. `Temp/fishnet-dual-smoke-*.txt` の delta を確認。許容差を決め、必要なら EditMode/PlayMode 自動照合を足す。

## 失敗時メモ

Yak を戻すと ClientSocket の PRO スタブで CS0103 / CS1061 になる。
NGO への切り替え判断は、Tugboat ホスト権威が製品要件を満たせないときだけ行う。
