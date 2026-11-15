# 10 オンライン

## 方針

- めっちゃカメレオンと同じく、P2P のホスト方式にする。専用サーバは最小限 (Q75)。
- まず Steam (PC)。スマホは後 (Q74)。
- マッチメイキングは発売時に要るが、最初のビルドには要らない (Q79)。
- ボイスチャットは内蔵しない (Q80)。
- テキストチャットもない。表示名は Steam の名前だけ (Q115)。

## 構成

| 層 | 選択 | 決定 |
| --- | --- | --- |
| ネットコード | Fish-Net の listen host | [ADR-0001](../decisions/ADR-0001-fishnet.md) |
| 物理 | ホストだけが計算する | [ADR-0002](../decisions/ADR-0002-host-physics.md) |
| 試合の進行 | ホストが reducer を回す | [ADR-0005](../decisions/ADR-0005-reducer.md) |
| 輸送 (LAN、同じ PC) | Tugboat | ADR-0001 |
| 輸送 (Steam) | FishySteamworks と Steamworks.NET | ADR-0001 |
| ランクの照合 | Cloudflare Workers と D1 (無料枠) | この章 |

比較は [netcode.md](../research/netcode.md) にある。

## コマンドの流れ

ホットシートと同じコマンドの経路を使う。

1. クライアントは `FlickCommand`、`CardPlay`、`Pass` を送る。
2. ホストが手番、手札、力の上限を検証してから reducer に入れる。境界は `EraserWars.Net` の `HostCommandGate`。
3. ホストが物理を回す。姿勢はホストの NetworkTransform で配る。
4. 止まったら、信頼性ありで静止姿勢を1回だけ送る。

乱数は使わない。ブレは時間で決まる周期関数なので、`ReleaseTime` を送れば同じ角度になる。

## 切断

- 切断したら30秒待つ。その後は毎ターン強制スキップする (Q78)。
- 戻ってきたら、次の手番から続けられる (`PlayerReconnected`)。
- ホストが抜けたら試合は終わる。そのことを UI で先に伝えておく。

## チート対策

- カーネル型のアンチチートは入れない ([steam.md](../research/steam.md))。
- listen host には、ホストのチートとホストの離脱がある。カジュアルでは UI で注意を出す。
- ランクだけ結果照合で守る (Q76)。
  - 両方のクライアントが同じ結果ハッシュを Workers に送る。
  - Steam 認証チケットを照合できたときだけ、D1 のレートを更新する。
  - サーバの仕事はこれだけにする。

## 遅延

- ターン制なので、弾きの入力からホストの反映まで、体感で遅れないことを目標にする (Q77)。
- 数値の目標は P6 で実測してから決める。
- オンラインで弾きが遅れて感じたら、弾いた消しゴムだけ Fish-Net の `PredictionRigidbody` にする。

## 試合ログ

- 最初から記録する (Q81)。`EraserWars.Persistence` の `MatchLogWriter`。
- 中身はイベント列と、各ターンの静止姿勢。
- リプレイ再生とネットの一致確認に使う。

## 確かめること

- P1-5。Fish-Net 4.x を 6000.6.9f1 に入れ、Multiplayer Play Mode で2つの Player をつなぐ ([ADR-0008](../decisions/ADR-0008-fishnet-spike.md))。通らなければ P6 の前に NGO 2.14 への切り替えを判断する。
- P6 の完了条件。2台の PC (または2つの Steam アカウント) で4人戦を1試合やりきり、両方の試合ログの最終状態ハッシュが一致する。
