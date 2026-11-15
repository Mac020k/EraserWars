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
| Dual-instance flick pose match | Pending operator (needs two Play instances; HUD shows local vs host delta) |

## 手順 (残作業)

1. Play → Start Host → **Fire canned flick**。別プロセスまたはビルドで Start Client し、HUD の `Local vs host delta` を見る。
2. 許容差を決め、必要なら自動照合テストを足す。

## 失敗時メモ

Yak を戻すと ClientSocket の PRO スタブで CS0103 / CS1061 になる。
NGO への切り替え判断は、Tugboat ホスト権威が製品要件を満たせないときだけ行う。
