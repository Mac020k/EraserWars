# ADR-0001 Fish-Net の listen host

- 状態: Accepted
- 日付: 2026-11-14
- 関連: Q75、Q102、[netcode.md](../research/netcode.md)

## 文脈

2〜4人がオンラインで同じ机を囲む。
専用サーバは最小限にしたい (Q75)。予算は10万円以内。
消しゴムは重なって押し合うので、全員の画面で同じ山の崩れ方が要る。
エディタは 6000.6.9f1 で、ネットコードのパッケージはまだ入っていない。

## 選択肢

1. Fish-Net の listen host。無料。予測を後から足せる。Steam 輸送 (FishySteamworks) の文書がある。
2. NGO 2.14。Unity 公式。ただし権威側以外の Rigidbody が kinematic になり、6000.6 ではプレリリース扱い。
3. Mirror。本体は活発だが、Steam 輸送の FizzySteamworks の最終リリースが2024年12月。
4. Photon Fusion 2。予測が最も強いが、クラウドの課金が前提。
5. 自前のリレー。Steam Datagram Relay と役割が重なる。

## 決定

Fish-Net の listen host にする。

- LAN と同じ PC では Tugboat を使う。
- Steam では FishySteamworks と Steamworks.NET を使う。
- クライアント予測は、遅延が体感で問題になってから入れる。

## 結果

- サーバ費用は0円。ランクの照合だけ Cloudflare の無料枠を使う ([10_online.md](../gdd/10_online.md))。
- ホストのチートとホストの離脱を受け入れる。カジュアルは UI で注意を出し、ランクは結果を照合する。
- Fish-Net と 6000.6.9f1 の相性は未検証。P1-5 の1日スパイクで確かめる ([ADR-0008](ADR-0008-fishnet-spike.md))。
- スパイクが通らなければ、P6 の前に NGO 2.14 への切り替えを判断し、この ADR を更新する。
- `Keshipin.Core` と `Keshipin.Cards` は Fish-Net を参照しない。切り替えても影響は `Net` と `Steam` に閉じる。
