# プレイテスト: Fish-Net dual-instance smoke

## セッション

- 日付: 2026-11-15
- フェーズ: P1 net spike
- ビルド: `Builds/FishNetSpike/EraserWars.exe` (FishNetSpikePlayerBuild, exit 0)
- Editor: Unity 6000.6.9f1 / `FishNet_Spike`
- 自動化: `FishNetSpikeAutoSmoke` (Editor=Host+canned flick, Player=Client)

## 実施

1. `FishNetSpikeSceneBuilder` で SceneId を再割当、NetworkManager を root 化、AutoSmoke を独立 GO に分離。
2. Windows プレイヤーを再ビルド。
3. Editor Play (Host) + `Builds/FishNetSpike/EraserWars.exe` (Client) を同時起動。
4. `Temp/fishnet-dual-smoke-host.txt` / `Temp/fishnet-dual-smoke-client.txt` に結果を記録。

## 結果

| 項目 | 結果 |
| --- | --- |
| Player Windows build | 成功 |
| Host settle pose | `(0.220, 0.055, 1.540)` |
| Local vs host delta (Client) | **0.0000 m** |
| Local vs host delta (Host) | **0.0000 m** |

## 判定

Host/Client 2インスタンスの canned flick 後ポーズは一致。Fish-Net スパイクの dual-smoke は完了。

## 関連

- [ADR-0008](../decisions/ADR-0008-fishnet-spike.md)
