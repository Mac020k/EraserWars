# ADR-0006 UI の方向

- 状態: Accepted (default under autonomy; operator may override)
- 日付: 2026-11-14
- 関連: Q88、[11_art-ui-audio.md](../gdd/11_art-ui-audio.md)

## 文脈

オペレータはイメージがわかないので両方の図を見たいと答えた (Q88)。
発売時の UI は UI Toolkit で作る。
配信者向けのパーティゲームなので、配信画面で読めることが要る (Q3)。

## 選択肢

1. 実況映えする太字。`docs/prototypes/ui-mock-broadcast.jpg`
2. モバイル風のコンパクト。`docs/prototypes/ui-mock-compact.jpg`

同じ画像を `Assets/Art/UI/` にも置いた。

## 決定

**1. 実況映えする太字 (broadcast)** を既定とする。

理由。配信で読めることが製品要件 (Q3) で、ADR の仮置きも太字案だった。
ホットシート縦切りの Debug OnGUI は別物のままにし、本番 HUD は broadcast の余白と文字サイズから作る。

オペレータが compact に変えたい場合は、この ADR を更新してから USS を切り替える。

## 結果

- HUD / USS の変数は broadcast モックを基準にする。
- compact 案の画像は比較用に残す。
