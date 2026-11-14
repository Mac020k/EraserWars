# ADR-0008 Fish-Net 導入スパイク

- 状態: Pending
- 日付: 2026-11-14

## 手順

1. Unity を閉じる (batchmode と競合するため)。
2. Asset Store / OpenUPM から Fish-Net 4.x を入れる。
3. Multiplayer Play Mode で Player 2つを起動。
4. `HostCommandGate` 経由で Flick を送り、姿勢が揃うことを確認。

## 失敗時

NGO 2.14 + コミュニティ Steam transport を再評価し、この ADR を更新する。
