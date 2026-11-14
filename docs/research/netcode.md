# ネットコード調査

2026-11-14 に調査エージェントがまとめた内容。
ラベルの意味は次のとおり。

- 実測。この PC のファイルか、リンク先の公式文書で確かめた。
- 推測。当てはめで、まだ動かしていない。

## 結論

Fish-Net の listen host にする。決定は [ADR-0001](../decisions/ADR-0001-fishnet.md)。

- ホストが机の Rigidbody をすべてシミュレートする。
- クライアントは弾きのコマンドとカードのコマンドを送る。
- 同じ PC と LAN では Tugboat を使う。
- Steam の招待が要る段階で、FishySteamworks と Steamworks.NET を足す。
- Rigidbody の予測は、遊んで弾きが遅れて感じるまで入れない。

プレイヤーから見ると、全員が同じ静止姿勢を見る。LAN の試合に Photon のアカウントも Steam クライアントも要らない。
次の開発者から見ると、カードのルールをネットなしでテストでき、輸送を替えても弾きのコードを書き直さずに済む。

## この PC の状態

実測。`ProjectSettings/ProjectVersion.txt` と `Packages/manifest.json` から読んだ。

- エディタは 6000.6.9f1。
- URP は 17.6.0、Input System は 1.20.1。
- 組み込みの物理モジュールは有効。
- ネットコードと Steam のパッケージはまだ入っていない。

## 選択肢の比較

| 選択肢 | この作品で得るもの | コスト | 机の上の物理 | Steam への道 |
| --- | --- | --- | --- | --- |
| Fish-Net | 今は listen host、後で同じオブジェクトに予測を足せる | ゲームでの利用は無料。ラグ補償は有料の Pro | ホストが計算する。`PredictionRigidbody` で力を再生できる | 文書化された輸送は FishySteamworks (Steamworks.NET) |
| NGO 2.14 | このエディタでホスト権威の `NetworkRigidbody` | 無料。6000.6 では 2.14.0 がプレリリース扱い | 権威側の Rigidbody だけが動的で、他のコピーは kinematic | FAQ が Steam 輸送を案内している |
| Mirror 96 | 単純なホストと RPC。2026年11月7日まで更新が続く | 無料、オープンソース | 調べた範囲では公式の Rigidbody ロールバックはない | FizzySteamworks の最終リリースが 2024年12月23日 |
| Photon Fusion 2 | 予測付きの PhysX はこの中で最も強い | 1アプリ100 CCU まで無料。500 CCU は月125ドル | 物理アドオンが再シミュレーションする。Photon 自身が CPU 負荷が高いと書いている | Steam とは別。シングル以外はクラウド接続が要る |
| Photon Quantum | ビット単位で決定論的なロールバック | Fusion と同じ CCU 課金 | 独自の物理ライブラリで、PhysX ではない | Photon Cloud |
| 自前リレー | 自分で運用するリレー | NAT 越え、ホスティング、複製ライブラリの選定が別に要る | なし | Steam Datagram Relay と役割が重なる |

## 物理の同期

採用するのはホスト権威。

- クライアント権威は机を割る。実測。NGO 2.14.0 の物理マニュアルでは、権威側の Rigidbody だけが動的で、他のコピーは kinematic になる。
- 推測。プレイヤーごとに持ち主が違う消しゴムは、1つの衝突解決を共有できない。消しゴムが重なる場面がまさにそれに当たる。
- ホスト権威なら PhysX のシーンは1つ。クライアントは弾きを送り、全員がホストの姿勢を描く。1つのシミュレーションしかないので結果がそろう (推測)。代わりに、ホストはチートでき、ホストが抜けると試合が終わる。
- 決定論的ロールバックは、この作品には合わない。実測。Unity の GhostObject 物理マニュアルは、Enhanced Determinism がロールバック用のビット単位の決定論を保証しないと書いている。その予測の道は NGO 3.2 と `com.unity.netcode` で、最低 Unity 6000.7 が要る。このエディタで入る NGO は 2.14.0 のプレリリースだけ。

## MVP の手順

机と Steam で同じ listen host のコードを使う。

1. LAN で Tugboat を使う。シーンは1つ。弾きのコマンドは消しゴムの ID、力積、スピンを運ぶ。ホストが大きさを制限してから力をかける。カードは別のメッセージにし、ホストが同じシミュレーションの中で解決する。
2. ホストから姿勢を配る。止まったら、信頼性ありで静止姿勢を1回だけ送る。送信頻度は LAN ビルドで調整する。今決める数値はない。
3. Steam 版では輸送を FishySteamworks に替える。招待は Steam ロビーで行う。ゲームのやりとりはコマンドのまま。
4. オンラインで弾きが遅れて感じたら、弾いた消しゴムだけ `PredictionRigidbody` にする。他は補間したホストの姿勢のまま。山全体を予測すると再シミュレーションが重い (推測)。

## Steam の通信

- 実測。Steam Networking Sockets の P2P は `CreateListenSocketP2P` と `ConnectP2P` で始める。Valve の SDR ページによれば、Steam が P2P の経路とリレーを受け持ち、アドレスは隠れる。リレーはこれで足りる。
- 実測。FishySteamworks の文書は Steamworks.NET を指定している。Facepunch.Steamworks も `SteamNetworkingSockets` を出している。
- 推測。ラッパーは1つだけ入れる。どちらも Steam を初期化するので、両方入れると衝突する。
- 実測。Unity Relay は月平均50 CCU まで無料で、超えると1 CCU あたり0.16ドル。Steam だけで出すなら要らない。

## リスク

- Fish-Net 4.7.4 はこのプロジェクトで動かしていない。Asset Store の対応表記は 6000.3.17f1 で、6000.6.9f1 との相性は推測。手順は [ADR-0008](../decisions/ADR-0008-fishnet-spike.md)。通らなければ P6 の前に NGO 2.14 への切り替えを判断する。
- listen host はホストがチートでき、ホストが抜けると試合が終わる。UI で先に伝える。試合中にホストを移すのは後の仕事にする。
- ホスト自身の弾きには遅延がない。不公平に感じたら、ホストの消しゴムもクライアントと同じ姿勢バッファから描く (推測)。
- 滑っている最中に質量や摩擦を変えたり、物を出したりするカードは、ホストで実行する。クライアントでも実行すると、次の姿勢が届くまで食い違う。
- Steam 輸送のテストには Steam クライアントが要る。FishyFacepunch の README によれば、Steam 輸送は自分自身に接続できない。同じ PC のテストは Tugboat で行う。
- Fish-Net のライセンスは、ゲームでの利用を認め、別のネットワーク製品に組み込むことを禁じている。

## 出典

- https://docs.unity.com/en-us/engine/6000.6/manual/packages-list/packages-all/pack-preview/com-unity-netcode-gameobjects
- https://docs.unity3d.com/Packages/com.unity.netcode.gameobjects@2.14/manual/advanced-topics/physics.html
- https://docs.unity3d.com/Packages/com.unity.netcode.gameobjects@3.2/manual/ghostobjects.html
- https://docs.unity3d.com/Packages/com.unity.netcode@7.2/manual/ghostobject/ghostobject-physics.html
- https://assetstore.unity.com/packages/tools/network/fishnet-networking-evolved-207815
- https://github.com/FirstGearGames/FishNet/blob/main/LICENSE.md
- https://fish-networking.gitbook.io/docs/overview/readme/features
- https://fish-networking.gitbook.io/docs/guides/features/prediction/predictionrigidbody
- https://fish-networking.gitbook.io/docs/fishnet-building-blocks/transports/fishysteamworks
- https://github.com/FirstGearGames/FishyFacepunch
- https://github.com/MirrorNetworking/Mirror/releases
- https://github.com/Chykary/FizzySteamworks/releases
- https://doc.photonengine.com/fusion/v2/addons/physics-addon-2.1
- https://www.photonengine.com/fusion/pricing
- https://partner.steamgames.com/doc/api/ISteamNetworkingSockets
- https://partner.steamgames.com/doc/features/multiplayer/steamdatagramrelay
- https://wiki.facepunch.com/steamworks/SteamNetworkingSockets
- https://unity.com/products/gaming-services/pricing
