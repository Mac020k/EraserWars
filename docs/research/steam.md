# Steam 調査

2026-11-14 に調査エージェントがまとめた内容。
「公式」は本文を読んだ Steamworks の文書、「二次情報」はブログや記事である。
オペレータの手作業は [steam-operator-checklist.md](../roadmap/steam-operator-checklist.md) にまとめた。

## Steamworks の登録

- 登録にはデジタル書類、銀行情報、税務情報、本人確認が要る。終わるまで15〜30日かかる。[公式 Steam Direct](https://partner.steamgames.com/steamdirect)
- アプリ料金は100ドルで返金されない。調整後の総収益が1,000ドルに達すると、支払いに含めて戻る。同じ出典。
- 料金を払ってから21日たたないと、最初のタイトルは公開できない。同じ出典。
- 「近日登場」ページを最低2週間出してから公開する。[公式 Release Process](https://partner.steamgames.com/doc/store/releasing)
- 審査は2段階で、ストアページ、ビルドの順。それぞれ3〜5営業日。7営業日以上の余裕を見る。[公式 Review Process](https://partner.steamgames.com/doc/store/review_process)
- 公開は自分で「Release App」を押す。自動では出ない。
- 税務は日米租税条約の W-8BEN で米国の源泉税が免除される。TIN が要る。確認に2〜7営業日。[公式 登録 FAQ](https://partner.steamgames.com/doc/gettingstarted/faq?l=japanese)
- 売上は翌月30日までに米ドルの SWIFT 送金で届く。100ドルに達するまで保留される。[公式 支払い FAQ](https://partner.steamgames.com/doc/finance/payments_salesreporting/faq?l=japanese)
- 取り分は最初の1,000万ドルまで開発側70%。二次情報 ([The Verge](https://www.theverge.com/2018/11/30/18120577/valve-steam-game-marketplace-revenue-split-new-rules-competition))。

## ストア素材

公式一覧は [Graphical Assets](https://partner.steamgames.com/doc/store/assets)。寸法は2024年8月に更新された。

| 素材 | サイズ |
| --- | --- |
| Header Capsule | 920×430 |
| Small Capsule | 462×174 |
| Main Capsule | 1232×706 |
| Vertical Capsule | 748×896 |
| Library Capsule | 600×900 |
| Library Hero | 3840×1240 (png) |
| Library Logo | 幅1280 または高さ720 (png) |
| Screenshots | 1920×1080 以上、16:9 |
| App Icon | 184×184 (jpg) |
| Shortcut Icon | 256×256 |
| Event Cover | 800×450 |

- カプセルには読めるタイトルかロゴを入れる。
- スクリーンショットはゲーム画面だけ。コンセプトアートや宣伝文句入りの画像は通らない。
- 公開時にない機能は載せない。載せるなら未実装と書く。[公式 Review Process](https://partner.steamgames.com/doc/store/review_process)
- 日本語と英語それぞれに、説明文とカプセルを用意する。英語は必須のフォールバック。[公式 Languages](https://partner.steamgames.com/doc/store/localization/languages)
- ゲーム画面を映したトレーラーを用意する。

## 年齢レーティング

- Content Survey は必須。一般、成人向け、生成 AI の3部で、回答から各地域のレーティングが作られる。[公式](https://partner.steamgames.com/doc/gettingstarted/contentsurvey)
- この作品は AI 補助の立ち絵とカードイラストを使うので、生成 AI の部で開示する (Q63)。
- PC のダウンロード専売なら CERO は要らない。二次情報だが複数の記事が一致する。[note](https://note.com/tango_lab/n/ne73e09f3f321)
- 推測。スマホや家庭用へ移すときは、その時点で各社の要件を確かめ直す。

## Steam Deck

Deck は必須にしない (Q97)。将来の参考として残す。[公式 Compatibility Review](https://partner.steamgames.com/doc/steamdeck/compat)

- 検証結果は販売に影響しない。バッジが変わるだけ。
- Verified にはコントローラで全コンテンツに届くこと、入力デバイスに合ったボタン表示が要る。
- 1280×800 で30fps 以上。文字の高さは9px 以上、推奨12px。
- コントローラ対応 (Q34) を入れておけば、後で申請しやすい (推測)。

## チート対策

- カーネル型のアンチチート (EAC、BattlEye) は入れない。Deck 非対応のおそれがあり、費用に見合わない (推奨)。[公式 Anti-cheat](https://partner.steamgames.com/doc/features/anticheat)
- 公式の指針は、状態をサーバが権威的に持つこと。P2P なら互いに検証し、ホストに全権を持たせない。[公式 Multiplayer](https://partner.steamgames.com/doc/features/multiplayer)
- この作品は、物理をホストだけが計算し、クライアントは入力を送って姿勢を受け取る ([ADR-0002](../decisions/ADR-0002-host-physics.md))。
- ランクの結果は、両クライアントの結果ハッシュと Steam 認証チケットで照合する ([10_online.md](../gdd/10_online.md))。
- 通信は Steam Networking (Datagram Relay) を使う。IP が隠れ、ポート開放が要らない。[公式](https://partner.steamgames.com/doc/features/multiplayer/networking)
- Game Ban が P2P だけの構成でどこまで効くかは確かめていない。

調査エージェントは NGO と Steam 輸送の組み合わせを挙げたが、この作品は Fish-Net を選んだ ([ADR-0001](../decisions/ADR-0001-fishnet.md))。

## 言語と価格

- 日本語ユーザーは2026年10月の調査で2.47%、言語別7位。英語は39.04%、簡体字は21.98%。[Valve ハードウェア調査](https://store.steampowered.com/hwsurvey/Steam-Hardware-Software-Survey/)
- 推測。日英同時 (Q6) の次に足すなら簡体字が候補。
- 円建ての価格を必ず入れる。入れないと日本のユーザーが買えない。DLC にも当てはまる。[公式 Pricing](https://partner.steamgames.com/doc/store/pricing)
- 日本のインディー対戦ゲームの相場はおおむね500〜980円。二次情報 ([Game*Spark](https://www.gamespark.jp/article/2025/09/20/157427.html))。DLC の値付けの参考にする。
- 値上げすると30日は割引できない。公開後30日は価格を変えない。
- 広告モデルとペイウォールは認められていない。

## Early Access を使わない理由

使わないと決めた (Q96)。公式の条件は次のとおり。[公式 Early Access](https://partner.steamgames.com/doc/store/earlyaccess)

- EA で一度出すと「近日登場」に戻せない。
- 1.0 公開時の露出がない。
- 将来の約束を売りにできない。

代わりに [Steam Playtest](https://partner.steamgames.com/doc/features/playtest) を回す。無料で、ストアページを出す前でもキーで配れる。評価とウィッシュリストに影響しない。

## Next Fest とデモ

- 各タイトル1回だけ出られる。デモの審査は開始の3週間前までに出す。ストアページの公開が要る。[公式 Next Fest](https://partner.steamgames.com/doc/marketing/upcoming_events/nextfest)
- 調査時の公式ページでは、次回は2027年2月の予定。ストアページ公開が3月なので間に合わない。計画は6月回を狙うが、日程は通例からの推測で、要確認。
- デモは別の App ID。ウィッシュリストへの通知は初回公開から2週間以内に1回だけ送れる。[公式 Demos](https://partner.steamgames.com/doc/store/application/demos)
- 二次情報。公開時のウィッシュリストは5,000前後が目標で、8,000が強い水準 ([How To Market A Game](https://howtomarketagame.com/benchmarks/))。2026年6月に Valve が露出のルールを変えたとされ、基準は動いている。

## オペレータが決めること

- 事業主体。個人で登録する前提 (計画)。契約の名義、銀行口座、本人確認を一致させる。
- 税務。売上は輸出免税として扱われるが、課税事業者の判定には合算される。インボイス登録と確定申告は税理士に確認する。
- 大学の兼業規定。大学の設備や時間を使うなら、知財の帰属を確かめる。
- 商標。「消しピンウォーズ」を J-PlatPat で調べる ([competitors.md](competitors.md))。
- 素材のライセンス。BGM、SE、フォントが商用利用と再配布に対応しているか。
- DLC の円建て価格と地域別価格。

## 確かめていないこと

- ストア素材と審査の細部は、読んだ公式ページの範囲に限る。
- P2P 構成での Game Ban の効き方。
- Valve の露出ルール変更は二次情報だけで、公式では確かめていない。
