# プレイテスト: 至近距離ノック

## セッション

- 日付: 2026-11-15
- フェーズ: P1 / P2 juiciness
- ビルド: editor (planar Stay normal fix)
- 人数: 4 (録画確認) / 2 (数学+PlayMode)
- 遊んだ人: オペレータ (agent + computer-use)
- ステージ: Classroom_Casual
- ルール: カジュアル ストック3
- カード: なし (ホットシート)

## 観察

1. 初回調整 (`minClosing` 1.25 / `minLaunch` 1.4、Enter のみ) では、密着状態で弾いてもターンだけ進み、位置がほぼ変わらないことが多かった。
2. 原因仮説: 既接触ペアは `OnCollisionEnter` が再発火せず、PhysX の弱い押し合いだけが残る。
3. Stay + floor 後の録画再確認 (4P、Seat1 青→赤):
   - 青 (打撃側) は机上を明確に左へ移動した。
   - 赤 (当てられた側) の画面位置はほぼ不変だった。
   - ユーザー指摘どおり、ゲーム性が担保されていない状態を再確認。
4. 追加原因: 密着 `OnCollisionStay` の接触法線がほぼ垂直になり、水平飛び出しを計算した直後に `linearVelocity.y` の保存処理で打ち消されていた。
5. 対策: 接触法線を XZ に投影 (退化時は中心差)。分離量を 0.02m に。PlayMode `AlreadyTouchingFlickMovesHitteeOnDeskPlane` で机上移動を固定。

## 手応え

- 至近距離でも当てられた側が机上を大きく動く: はい (XZ 投影後。密着から弾くと赤が机右端・青が中央左まで離れ、打撃側と被弾側の両方が机スケールで動いた)
- ヒットストップは短く感じるが邪魔ではない
- カメラ追従が寄ると確認しづらいので、検証時は Tab で俯瞰に戻すとよい

## 次に直すこと

1. Fish-Net Host/Client 2インスタンスで終端姿勢 delta を実測する
2. 2人・4人オペレータ playtest を3試合ずつ記録する
3. PlayMode `AlreadyTouchingFlickMovesHitteeOnDeskPlane` を Editor 閉じてバッチ実行する

## 関連

- `EraserCollisionRelay` / `CollisionMath`
- `docs/gdd/04_flick.md`
