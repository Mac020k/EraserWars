# カード一覧 (50枚)

家族 (カテゴリ) は小学校モチーフ。

| 家族 | キー | 意味 |
| --- | --- | --- |
| 気合 | kiai | パワー・質量・もう一回 |
| 宿題 | homework | 予測・集中・読み |
| 席替え | seat | 位置入れ替え・ワープ |
| 文房具 | stationery | 定規・鉛・ホッチキス等の武器 |
| カバー | cover | のり・ワックス・盾・カス |
| 必殺 | signature | キャラ固有 |

`build_cost` はデッキ構築コスト。`uses_per_match` / `cooldown` / `duration` は試合内制約。

## コモン (1〜34)

| ID | 表示名 | 家族 | cost | effects |
| --- | --- | --- | --- | --- |
| 1 | 重い消しゴム | 気合 | 1 | ModifyStat:Mass:1.5:1 |
| 2 | のり付け | カバー | 2 | ModifyStat:Friction:2:2 |
| 3 | ワックス | カバー | 1 | ModifyStat:Friction:0.5:1 |
| 4 | 定規ビーム | 文房具 | 2 | PlaceDeskObject:RulerWall:2 |
| 5 | 集中 | 宿題 | 1 | ModifyFlick:1:0:1 |
| 6 | 全力 | 気合 | 2 | ModifyFlick:1.4:1:2 |
| 7 | 先生の目 | 宿題 | 2 | ModifyRule:OpponentTurnTime:0.5:1 |
| 8 | 席替え | 席替え | 2 | SwapPositions |
| 9 | シールド | カバー | 2 | GrantShield:0.5:1 |
| 10 | 芯の鉛 | 文房具 | 1 | GrantWeapon:0.15:1 |
| 11 | 消しカス | カバー | 1 | PlaceDeskObject:DustPatch:2 |
| 12 | 予言 | 宿題 | 1 | RevealTrajectory:2 |
| 13 | 気合入れ | 気合 | 1 | ModifyStat:Power:1.25:1 |
| 14 | もう一回 | 気合 | 3 | ExtraFlick |
| 15 | 体重乗せ | 気合 | 2 | ModifyStat:Mass:1.8:1 |
| 16 | 走り込み | 気合 | 1 | ModifyStat:Speed:1.3:1 |
| 17 | 宿題忘れ | 宿題 | 2 | ModifyRule:OpponentTurnTime:0.8:1 |
| 18 | 見直し | 宿題 | 1 | ModifyFlick:1:0.5:1 |
| 19 | カンニング防止 | 宿題 | 2 | ModifyRule:HideTrajectory:1:1 |
| 20 | 席取り | 席替え | 2 | Teleport:0.5:0.5 |
| 21 | 端っこへどうぞ | 席替え | 2 | ModifyRule:PushEdge:0.4:1 |
| 22 | 真ん中キープ | 席替え | 1 | ModifyRule:CenterBias:0.3:1 |
| 23 | 鉛筆の芯 | 文房具 | 1 | GrantWeapon:0.1:1 |
| 24 | 分度器 | 文房具 | 2 | GrantWeapon:0.18:1 |
| 25 | ホッチキス | 文房具 | 2 | GrantWeapon:0.2:1 |
| 26 | クリップ爆弾 | 文房具 | 2 | GrantWeapon:0.25:1 |
| 27 | カバー装着 | カバー | 1 | ModifyStat:Bounciness:1.4:1 |
| 28 | テープ止め | カバー | 2 | ModifyStat:Friction:2.5:1 |
| 29 | カスの山 | カバー | 2 | PlaceDeskObject:DustPile:2 |
| 30 | 安全第一 | カバー | 2 | GrantShield:1:2 |
| 31 | 休憩時間 | 気合 | 1 | ModifyRule:CooldownCut:0.5:1 |
| 32 | チャイム前 | 宿題 | 2 | ModifyRule:SelfTurnTime:1.2:1 |
| 33 | 掃除当番 | カバー | 1 | PlaceDeskObject:ClearDust:1 |
| 34 | 班長の合図 | 席替え | 2 | ModifyStat:Power:1.15:1 |

## 必殺 (35〜50) — キャラ0〜7 × 2

| ID | 表示名 | キャラ | cost | effects |
| --- | --- | --- | --- | --- |
| 35 | もう一押し | 赤星 | 3 | ExtraFlick |
| 36 | 燃える白 | 赤星 | 2 | ModifyStat:Power:1.8:1 |
| 37 | どすこい | 轟 | 3 | ModifyStat:Mass:2.2:1 |
| 38 | 押し出し | 轟 | 2 | ModifyRule:PushEdge:0.7:1 |
| 39 | 検算 | 水無月 | 2 | ModifyFlick:1:0:0.5 |
| 40 | 定規レーザー | 水無月 | 3 | PlaceDeskObject:RulerWall:2 |
| 41 | くっつきシール | 花咲 | 2 | ModifyStat:Friction:3:1 |
| 42 | 安全コーン | 花咲 | 2 | PlaceDeskObject:Cone:2 |
| 43 | ニューカマー | Alex | 2 | ModifyStat:Speed:1.5:1 |
| 44 | 席替えマスター | Alex | 3 | SwapPositions |
| 45 | 声かけ | 鳳 | 2 | ModifyStat:Power:1.35:1 |
| 46 | フォーメーション | 鳳 | 3 | GrantShield:0.5:1 |
| 47 | 試作カバー | くろえ | 2 | ModifyStat:Bounciness:1.8:1 |
| 48 | ホッチキス芯X | くろえ | 3 | GrantWeapon:0.3:1 |
| 49 | のり爆弾 | Mei | 3 | GrantWeapon:0.28:1 |
| 50 | ゆっくりでいい | Mei | 2 | ModifyRule:OpponentTurnTime:0.6:1 |

## CSV / インポート

- ソース: `Assets/Data/Csv/cards.csv`
- メニュー: `EraserWars/Import Cards CSV` または `EraserWars/Import All Content CSV`
- 出力: `Assets/Data/Cards/Card_<id>.asset`
- カタログ: `Assets/Resources/EraserWars/ContentCatalog.asset`
