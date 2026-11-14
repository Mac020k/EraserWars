# 消しピンウォーズ 知識庫

『消しピンウォーズ』の設計、調査、決定、進捗をここに集める。
Markdown は GitHub 上でそのまま読めるので、表示用の仕組みは作らない。

## 現在のフェーズ

P1 物理縦切りの手触りを固めつつ、P2 弾きの juiciness を載せている (2026-11-14)。

リポにあるもの。

- Unity 標準の `.gitignore` と LFS。`Classroom_Casual` ホットシート。
- 仮想トラックパッド弾き、狙い線ブレ、衝突ヒットストップ、縁際スロー。
- Core smoke 緑。PlayMode knockoff ゲート追加 (Unity を閉じて `run-tests.ps1` で実行)。
- UI 方向は broadcast を既定 ([ADR-0006](decisions/ADR-0006-ui-direction.md))。

まだ終わっていないもの。

- Fish-Net 導入スパイクは未実施 ([ADR-0008](decisions/ADR-0008-fishnet-spike.md)、手順は `Assets/_Project/Net/FishNetInstall.md`)。
- PlayMode / EditMode の Unity バッチ実行は、開いているエディタのロックで未完。
- 2人・4人のオペレータ playtests を3試合ずつ記録すること。
- Localization / ProBuilder / MPM は CS0619 のため保留。

## 次の3手

1. Unity を閉じて `.\tools\run-tests.ps1 -Platform All` を回し、結果を `TestResults/` と playtests に残す。
2. Fish-Net スパイクを `FishNetInstall.md` どおりに1日で実施し、ADR-0008 を更新する。
3. `Classroom_Casual` で2人と4人を3試合ずつ遊び、[playtests/](playtests/TEMPLATE.md) に書く。

## 文書

### 回答

- [2026-11-14 回答アーカイブ (Q1〜Q120)](questions/2026-11-14_answers.md)

### 調査

- [桜井政博『ゲームを作るには』](research/sakurai.md)
- [競合 (keshipin.jp、ボクらの消しゴム落とし)](research/competitors.md)
- [ネットコード](research/netcode.md)
- [Steam](research/steam.md)

### GDD

1. [ビジョンと立ち位置](gdd/01_vision.md)
2. [コアループ](gdd/02_core-loop.md)
3. [卓のルール](gdd/03_rules.md)
4. [弾き](gdd/04_flick.md)
5. [消しゴム](gdd/05_erasers.md)
6. [キャラクター](gdd/06_characters.md)
7. [カードとデッキ](gdd/07_cards.md)
8. [モード](gdd/08_modes.md)
9. [ステージ](gdd/09_stages.md)
10. [オンライン](gdd/10_online.md)
11. [アート、UI、サウンド](gdd/11_art-ui-audio.md)
12. [収益化](gdd/12_monetization.md)

### 決定

- [ADR-0001 Fish-Net](decisions/ADR-0001-fishnet.md)
- [ADR-0002 Host physics](decisions/ADR-0002-host-physics.md)
- [ADR-0003 Monetization](decisions/ADR-0003-monetization.md)
- [ADR-0004 gitignore / LFS](decisions/ADR-0004-gitignore-lfs.md)
- [ADR-0005 Reducer](decisions/ADR-0005-reducer.md)
- [ADR-0006 UI direction](decisions/ADR-0006-ui-direction.md)
- [ADR-0007 Physics scale](decisions/ADR-0007-physics-scale.md)
- [ADR-0008 Fish-Net spike](decisions/ADR-0008-fishnet-spike.md)

### その他

- [マイルストーン](roadmap/milestones.md)
- [Steam オペレータ手順](roadmap/steam-operator-checklist.md)
- [プレイテスト](playtests/TEMPLATE.md)
- [プロトタイプ](prototypes/README.md)
