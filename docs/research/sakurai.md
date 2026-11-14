# 桜井政博『ゲームを作るには』と関連作の調査

2026-11-14 に調査エージェントがまとめた内容を、消しピンウォーズに写したもの。
ラベルの意味は次のとおり。

- 出典あり。リンク先にそう書いてある。
- 推測。この作品への当てはめで、まだ試していない。

この作品はまだ誰もプレイテストしていない。だから作品についての「実測」はない。

## 桜井政博の原則

動画の書き起こしは、リスクとリターン、最初の3分、遅さは罪の3つ。
残りは senkohome と note の要約 (第三者のまとめ) から取った。

### リスクとリターン

- 出典あり。桜井はゲーム性を駆け引き、つまりリスクとリターンと定義する。スペースインベーダーで砲台を敵に近づけると、撃たれる危険と撃ち落とす機会が同時に上がる。[動画](https://www.youtube.com/watch?v=cTSMohV3TgQ)、[要約](https://senkohome.com/sakurai-game-dev-gameplay/)
- 出典あり。リスクとリターンは近くに、適切な大きさで置き、逆転を混ぜる。例はマリオの跳ね返ってくる甲羅。同じ出典。
- 出典あり。調整のてこは2つある。リスクを決める要素 (砲台の幅) と、リスクを変える仕組み (トーチカ)。[note](https://note.com/milli_era/n/n4ba77c4403c4)
- 出典あり。「ギュッとしてパッと解消」。緊張をためてから一気に解く。緊張をなくすと面白さも消える。[要約](https://senkohome.com/sakurai-game-dev-gameplay/)
- 出典あり。ゲーム性を上げると一般性 (とっつきやすさ) が下がる。初代カービィはわざとリスクを下げた。同じ出典。
- 出典あり。ズルをある程度許す。ご褒美は「どれぐらい良いことをしたか」に合わせる。同じ出典。
- 出典あり。スマブラが蓄積ダメージと場外を選んだのは、コンボが駆け引きにならないと桜井が判断したから。コンボ中、やられる側は待つだけになる。[要約](https://senkohome.com/sakurai-game-dev-concept/)

### 最初の数分とテンポ

- 出典あり。面白さは遊び始めて3分で伝わらないといけない。プロローグは飛ばす。チュートリアルは空っぽの練習場ではなく本番に近づける。[動画](https://www.youtube.com/watch?v=PlbwQ_rntCM)、[要約](https://senkohome.com/sakurai-game-dev-planning-design/)
- 出典あり。遅さは罪。削れる待ちはすべて削り、画面遷移を速くする。[動画](https://www.youtube.com/watch?v=yu7BDkENnNM)、[要約](https://senkohome.com/sakurai-game-dev-specification/)
- 出典あり。すぐ再挑戦させ、冷める前に操作を返す。負けは短く派手な演出ではっきり見せる。復帰直後に攻撃させない。[要約](https://senkohome.com/sakurai-game-dev-gameplay/)

### 手応え

- 出典あり。スマブラのヒットストップには8つの決まりがある。[note](https://note.com/rr_r612/n/n7773b6ebf77e)
  - やられ側を攻撃側より大きく揺らす。
  - 当たり判定は揺らさない。動かすのは見た目だけ。
  - 地上では横にだけ揺らす。
  - 揺れ幅はヒットストップの間に減衰させる。
  - 長さはダメージ×技ごとの係数。
  - やられポーズは約4フレームで混ぜる。
  - 攻撃側は1フレーム未満の速さで前に進み続ける。
  - カメラが引くほど揺れ幅を大きくする。
- 出典あり。ヒットストップには上限がある。乱戦で固まった2人が、3人目にただで殴られるから。[Source Gaming](https://sourcegaming.info/2015/11/11/thoughts-on-hitstop-sakurais-famitsu-column-vol-490-1/)
- 出典あり。画面の揺れは強く始めて減衰させる。フラッシュを足す。ヒットマークは明るい背景でも暗い背景でも読めるようにする。[要約](https://senkohome.com/sakurai-game-dev-effect/)
- 出典あり。同じボタンでも、画面上で動く物の慣性で重くも軽くも感じる。はじき入力では、スティックを倒す速さが技の強さを決める。[要約](https://senkohome.com/sakurai-game-dev-specification/)
- 出典あり。ほめてやれ。スマブラの撃墜演出の紙吹雪は、撃墜した側をほめている。入力に反応がない場面をなくす。[要約](https://senkohome.com/sakurai-game-dev-planning-design/)

### 乱数とバランス

- 出典あり。乱数は新鮮さを生むが、不公平にも感じる。スマブラはファイターから乱数を外し、アイテムに残した。腕で扱える乱数なら受け入れられる。[要約](https://senkohome.com/sakurai-game-dev-specification/)
- 出典あり。平均と平凡は同じ。ファイターの長所を削らない。スマブラ SPECIAL のオンライン勝率は87体で47.1%〜51.4%に収まっている。同じ出典。
- 出典あり。逆転の助けは軽くする。スマッシュボールは先頭の人ほど取りにくく、最後の切りふだのゲージは負けている人ほど早くたまる。同じ出典。

## 関連作の教訓

- 出典あり。Slay the Spire の初期版は、敵の行動がランダムで、防ぐか攻めるかを決められなかった。意図アイコンで直した。[Ars Technica 書き起こし](https://cdn.arstechnica.net/wp-content/uploads/2019/05/0e5cc753-f874-4f8d-945c-708c5bfad685cc.txt)
- 出典あり。Slay the Spire は数値に騙されることがあった。Madness が強く見えたのは、負けるデッキが消えた終盤に出るカードだったから。[PCGamesN](https://www.pcgamesn.com/slay-the-spire/slay-the-spire-balance)
- 出典あり。Peglin は物理とデッキ構築を混ぜたローグライク。ターン制の物理とデッキの乱数が配信と相性がよく、配信でコミュニティが育った。[GameMaker](https://gamemaker.io/en/blog/peglin-developers-interview)
- 出典あり。Peggle は最初の6面ほどで、跳ね方を数度だけ曲げて運を足している。気づいたプレイヤーは子ども扱いされたと感じる。[Game Developer](https://www.gamedeveloper.com/design/how-and-why-game-devs-manipulate-luck-in-games-like-i-peggle-i-)
- 出典あり。Peggle の Extreme Fever は、操作できない最後の一打をスローの祝福に変える。[PC Gamer](https://www.pcgamer.com/the-making-of-peggle/2/)
- 出典あり。Marvel Snap は1試合の平均が約3分。両者が同時に手番を進めるので待ちがない。[GamesBeat](https://gamesbeat.com/ben-brode-bets-super-speed-will-make-marvel-snap-stand-out/)
- 出典あり。Jackbox は1度に1つの作業だけをさせる。「なぜ面白いかの説明に1段落かかるなら削る」。[Built In Chicago](https://www.builtinchicago.org/articles/jackbox-games-design-party-pack)
- 出典あり。Human Fall Flat は物理がぐらぐらでも、いくつかの操作が必ず効くので遊べる。[Hardcore Gamer](https://hardcoregamer.com/previews/pax-east-human-fall-flat-is-essentially-gang-beasts-the-puzzle-game/204551/)
- 出典あり。Unity の PhysX は、同じマシン、同じビルド、Enhanced Determinism オンのときだけ決定論的になる。CPU ベンダーや x86 と ARM の間では結果が変わる。[Unity Discussions](https://discussions.unity.com/t/cross-platform-determinism-in-unity-using-webassembly/1495296)
- 出典あり。ターン制の Unity ゲーム Artillery Royale は、物理の非決定論に当たり、ターンの再生ストリームを作った。[開発記](https://jerome.gangneux.net/weheartprojects.com/2020/08/technologies/)

## 消しピンウォーズへの当てはめ

### 弾きのリスクとリターン

- 推測。カードがなくても、弾き自体にリスクとリターンがあるべき。強く弾けば相手を落とせるが、自分も縁へ運ばれる。
- 採用。ドラッグが長いほどブレが大きく速くなる ([04_flick.md](../gdd/04_flick.md))。これが砲台の幅にあたるリスクの要素になる。
- 採用。消しゴムの大きさ、質量、摩擦がリスクを決め、カードと机の物がリスクを変える。桜井の2つのてこに対応する。
- 推測。反動で自分が飛ぶ跳ね返りが、マリオの甲羅の役になる。

### 最初の3分

- 採用。起動から最初の落下まで3分以内を P1 の完了条件にする ([milestones.md](../roadmap/milestones.md))。
- 採用。文字のチュートリアルから始めない。カードは数手後に1枚ずつ見せる。
- 採用。負けたら1ボタンで即再戦 (Q112)。
- 推測。最初のソロ戦だけ、Peggle 型の見えない補助を入れる手がある。対人戦では切る。気づかれると反感を買う点に注意する。

### 手応え

- 採用。衝突でヒットストップを入れる。長さは衝撃の大きさに比例させ、上限を設ける。揺らすのは見た目の子メッシュだけで、Rigidbody は動かさない。スマブラの「当たり判定は揺らさない」に当たる。
- 採用。止めるときは `Physics.simulationMode = Script` で物理の歩みを止め、演出と音は動かし続ける。
- 採用。1ショットのヒットストップの合計に上限を置く。4人卓の連鎖衝突で固まりが積み上がるのを防ぐ。
- 採用。縁際でぐらつく場面と落下はスローにする。Peggle の Extreme Fever に近い。

### カードと物理の正直さ

- 採用。カードは弾く前の条件だけを変え、飛んでいる最中の軌道は変えない。結果を自分の狙いのおかげだと感じられる。
- 採用。予測線は最初の衝突点まで。先を見せるのはカード (予言ノート) の役にする。
- 採用。乱数はデッキのドローに置き、衝突の結果には入れない。スマブラがファイターから乱数を外した判断と同じ向き。
- 推測。手札は公開 (Q59) なので、Slay the Spire の意図アイコンほどの読みやすさは最初からある。

### オンライン

- 採用。PhysX は CPU ごとにずれるので、ホストが1回だけ計算して結果を配る ([ADR-0002](../decisions/ADR-0002-host-physics.md))。
- 採用。固定 dt で物理を回し、描画のフレームレートと切り離す。
- 採用。各ショットを試合ログに残す。Peglin のように、切り抜きと配信の素材になる。

## オペレータが決めること (調査時点)

調査の時点で残っていた問いと、その後の回答。

- 勝ち方は場外だけか、HP と場外か。回答は場外だけ (Q16、Q39)。
- デッキは Peglin 型か、試合前構築か。回答は試合前構築 (Q52)。
- 相手の弾きの後に出すリアクションカードを認めるか。回答は認めない (Q55)。
