# ADR-0008 Fish-Net 導入スパイク

- 状態: Pending (手順整備済み、パッケージ未導入)
- 日付: 2026-11-14
- 手順書: [FishNetInstall.md](../../Assets/_Project/Net/FishNetInstall.md)

## 手順

1. Unity を閉じる (`Temp/UnityLockfile` を消す)。
2. Fish-Net 4.x を Packages または Plugins に入れる。
3. `FishNet_Spike` シーンで Tugboat Host / Client を起動する。
4. Host だけが PhysX を回し、`HostCommandGate` 経由の1フリックで両端の終端姿勢を比べる。

Multiplayer Play Mode は 6000.6.9f1 で CS0619 のため保留。スパイクは2つのエディタインスタンスかビルド+エディタでよい。

## 失敗時

NGO 2.14 + コミュニティ Steam transport を再評価し、この ADR を更新する。
