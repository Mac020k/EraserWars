# プレイテスト: Fish-Net dual-instance (pending)

## セッション

- 日付: 2026-11-15
- フェーズ: P1 net spike
- 状態: プレイヤービルド未作成のため dual 実測は保留

## ブロッカー

- `Builds/FishNetSpike/EraserWars.exe` が無い。
- `tools/run-fishnet-dual-smoke.ps1 -Build` は batchmode のため、開いている Editor を閉じてから実行する必要がある。

## 次の手順

1. Unity Editor を閉じる
2. `tools/run-fishnet-dual-smoke.ps1 -Build` でクライアントをビルド＆起動
3. Editor で `FishNet_Spike` を Play → Start Host → Fire canned flick
4. Client で Start Client し、両 HUD の `Local vs host delta` を記録

## 関連

- [ADR-0008](../decisions/ADR-0008-fishnet-spike.md)
- `Assets/_Project/Net/FishNetSpikeHud.cs`
