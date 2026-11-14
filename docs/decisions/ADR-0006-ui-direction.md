# ADR-0006 UI の方向

- 状態: Proposed (モック提示済み、オペレータの選択待ち)
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

未決。オペレータが選ぶ。

選ぶまでは、HUD を太字案の余白と文字サイズで仮に作る。

## 結果

- 選んだら、この ADR を Accepted にし、選んだ案と理由を書く。
- USS の変数 (文字サイズ、余白、色) を確定する。
