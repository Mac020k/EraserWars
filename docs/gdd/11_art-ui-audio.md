# 11 アート、UI、サウンド

## アート

- ローポリのセル調 (Q86)。
- 教室はそこそこリアル (Q87)。
- アセットは自作する。Blender、OpenCV、AI 補助を使う (Q91)。
- 実在の消しゴムはオマージュとして似せてよい (Q116)。

### 作り方

| 対象 | 作り方 | 時期 |
| --- | --- | --- |
| 消しゴム | Blender のローポリ。最初は直方体 | P4 |
| 立ち絵 | 2Dのセル調。AI 補助。Content Survey で開示 | P4 |
| カードイラスト | AI 補助 (Q63) | P4〜P5 |
| 教室と他のステージ | ProBuilder でグレーボックス、後で Blender | P1、P5 |
| シェーダ | URP の ShaderGraph。ランプのトゥーンと、背面法線のアウトライン | P5 |
| テクスチャの前処理 | OpenCV で減色と線抽出 | P5 |

実測。Blender は PATH と Program Files に見当たらない。P5 で導入する。

### 演出

- 指や手は描かない (Q35 の既定)。
- カードを出すときはスマブラ風のカットイン。
- 落下は短く、0.8秒以内。落ちる途中で消しゴム同士が当たったら撃墜演出 (Q21)。
- 撃墜した側をほめる紙吹雪 ([sakurai.md](../research/sakurai.md) の「ほめてやれ」)。
- ヒットストップと揺れの決まりは [04_flick.md](04_flick.md)。

## UI

- UI Toolkit で作る。
- 文字列はすべて Unity Localization のキーで管理する (ja/en)。
- フォントは M PLUS Rounded 1c (OFL) を候補にする。
- 試合中 HUD は実況映え太字 ([ADR-0006](../decisions/ADR-0006-ui-direction.md))。
- 試合前は専用フルスクリーン開始画面 ([ADR-0009](../decisions/ADR-0009-start-screen-and-classroom-look.md)、モック `docs/prototypes/ui-mock-start-fullscreen-6b.jpg`)。
- トーンはコロコロコミック調の熱量・アイコン先行 (Q8 訂正)。文字だけの Debug OnGUI に依存しない。
- 画面の大きさは大。UiScale は 1.2 固定。小と標準は置かない。

| 案 | モック | 状態 |
| --- | --- | --- |
| 実況映えする太字 | `docs/prototypes/ui-mock-broadcast.jpg` | HUD 既定 |
| モバイル風のコンパクト | `docs/prototypes/ui-mock-compact.jpg` | 比較用 |
| フルスクリーン開始 | `docs/prototypes/ui-mock-start-fullscreen-6b.jpg` | 開始画面 採用 |
| オーバーレイ開始 | `docs/prototypes/ui-mock-start-overlay-6a.jpg` | 不採用 |

### HUD に出すもの

- 手番の人と残り時間。
- 全員のストック。
- 全員の手札 (公開、Q59)。
- 予測線とブレの矢印。
- デバッグ表示 (質量、速度、接触点、フェーズ)。

## サウンド

- ボイスはない (Q50)。
- BGM はコロコロコミック調の明るいバトル／ホビー感。購入素材で賄う (Q89 の既定。Q8 訂正後)。
- いちばん大事な効果音は、消しゴム同士の衝突と、落ちて床に当たる音 (Q90 の既定)。
- SE は本物の消しゴムを机で弾いた音を録る。
- 素材の予算は BGM と SE で約3万円。商用利用と再配布の条件を確かめてから買う ([steam.md](../research/steam.md))。
