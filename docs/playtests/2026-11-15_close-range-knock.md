# プレイテスト: 至近距離ノック

## セッション

- 日付: 2026-11-15
- フェーズ: P1 / P2 juiciness
- ビルド: editor (post Stay+launch floor bump; commit pending)
- 人数: 2
- 遊んだ人: オペレータ (agent + computer-use)
- ステージ: Classroom_Casual
- ルール: カジュアル ストック3
- カード: なし (ホットシート)

## 観察

1. 初回調整 (`minClosing` 1.25 / `minLaunch` 1.4、Enter のみ) では、密着状態で弾いてもターンだけ進み、位置がほぼ変わらないことが多かった。
2. 原因仮説: 既接触ペアは `OnCollisionEnter` が再発火せず、PhysX の弱い押し合いだけが残る。
3. 対策後 (`OnCollisionStay` + `minClosing` 2.0 / `minLaunch` 2.4 / 分離 0.012):
   - ほぼ密着から Seat0 が強めに弾くと、赤が机の右奥、青が中央寄りまで離れ、机スケールで明確に移動した。
   - ストックは消耗なし (落ちず)。手応えとして「当たった側が動く」は確認できた。

## 手応え

- 至近距離でも当てられた側が机上を大きく動く: はい (対策後)
- ヒットストップは短く感じるが邪魔ではない
- カメラ追従が寄ると確認しづらいので、検証時は Tab で俯瞰に戻すとよい

## 次に直すこと

1. Fish-Net Host/Client 2インスタンスで終端姿勢 delta を実測する
2. 2人・4人オペレータ playtest を3試合ずつ記録する
3. 密着時のヒットをログ/デバッグ色で可視化できると録画確認が楽

## 関連

- `EraserCollisionRelay` / `CollisionMath`
- `docs/gdd/04_flick.md`
