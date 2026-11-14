# 消しピンウォーズ 知識庫

『消しピンウォーズ』の設計、調査、決定、進捗をここに集める。
Markdown は GitHub 上でそのまま読めるので、表示用の仕組みは作らない。

## 現在のフェーズ

P0 (土台) の仕上げと、P1 (物理の縦切り、カードなし) の手触り検証中。

2026-11-14 時点で、次のものがリポにある。

- Unity 標準の `.gitignore` と、LFS 用の `.gitattributes`。`git-lfs/3.8.0` 導入済み。
- `Assets/_Project` の asmdef と、依存の向き。
- `Keshipin.Core` の reducer と EditMode テスト。
- `tools/run-tests.ps1`、`tools/build.ps1`、`tools/run-core-smoke.ps1`。
- 教室シーン `Assets/Scenes/Classroom_Casual.unity` (メニュー `Keshipin > Build Classroom Casual Scene`)。ホットシートの Pass まで手触り確認済み。
- UI モック2案 (`prototypes/` の `.jpg`)。

次のものは、まだ終わっていない。

- Localization / ProBuilder / Multiplayer Play Mode は Unity `6000.6.9f1` でパッケージ側が CS0619 になるため、いったん `manifest.json` から外している。互換版が出るか、自前グレーボックスに寄せるかは未決。
- Fish-Net の導入スパイクは手順だけで、未実施 ([ADR-0008](decisions/ADR-0008-fishnet-spike.md))。
- 物理スケールは仮決め ([ADR-0007](decisions/ADR-0007-physics-scale.md))。
- P0 の完了条件「別フォルダに clone して開き、`run-tests.ps1` が緑」を、まだ誰も確かめていない。
- UI 方向はオペレータの選択待ち ([ADR-0006](decisions/ADR-0006-ui-direction.md))。

## 次の3手

1. Unity を閉じて `.\tools\run-tests.ps1 -Platform All` を回し、結果を記録する。別フォルダへの clone でも同じ結果になるか確かめる。
2. `Classroom_Casual` で2人と4人を3試合ずつ遊び、結果を [playtests/](playtests/TEMPLATE.md) に書く。
3. UI モック2案から1つを選び、[ADR-0006](decisions/ADR-0006-ui-direction.md) を Accepted にする。

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

### 決定 (ADR)

- [ADR-0001 Fish-Net の listen host](decisions/ADR-0001-fishnet.md)
- [ADR-0002 ホストだけが物理を計算する](decisions/ADR-0002-host-physics.md)
- [ADR-0003 無料+DLC、有料ガチャなし](decisions/ADR-0003-monetization.md)
- [ADR-0004 .gitignore と LFS](decisions/ADR-0004-gitignore-lfs.md)
- [ADR-0005 reducer で試合を進める](decisions/ADR-0005-reducer.md)
- [ADR-0006 UI の方向 (選択待ち)](decisions/ADR-0006-ui-direction.md)
- [ADR-0007 物理スケール (仮)](decisions/ADR-0007-physics-scale.md)
- [ADR-0008 Fish-Net 導入スパイク (未実施)](decisions/ADR-0008-fishnet-spike.md)

### 計画と記録

- [マイルストーン](roadmap/milestones.md)
- [Steam 出荷のオペレータ作業](roadmap/steam-operator-checklist.md)
- [プレイテスト記録テンプレート](playtests/TEMPLATE.md)
- [試作と UI モック](prototypes/README.md)

## 書き方

- 決定は ADR に書く。GDD は ADR と回答アーカイブを参照し、同じことを二重に書かない。
- 推測には「推測」、自分で測った値には「実測」と書く。
- 設計文書は日本語、ルートの `README.md` は英語で書く。
