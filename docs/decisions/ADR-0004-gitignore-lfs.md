# ADR-0004 .gitignore と LFS

- 状態: Accepted
- 日付: 2026-11-14
- 関連: Q106

## 文脈

2026-11-14 の実測で、元の `.gitignore` は `/Packages` と `/ProjectSettings` を除外していた。
追跡ファイルは39個で、すべて `Assets` の下にあった。
そのため GitHub (`Mac020k/EraserWars`) からプロジェクトを復元できなかった。
オペレータは GitHub の容量を心配している (Q106)。

## 決定

- `.gitignore` を Unity の標準に直す。`Library/`、`Temp/`、`Obj/`、`Build/`、`Builds/`、`Logs/`、`UserSettings/`、`*.csproj`、`*.sln` を除外する。
- `Packages/` と `ProjectSettings/` を追跡する。
- `tools/` が作る `TestResults/` と、`tools/**/bin/`、`tools/**/obj/` も除外する。
- 大きなバイナリは Git LFS に載せる。`.gitattributes` で `*.fbx`、`*.blend`、`*.png`、`*.jpg`、`*.psd`、`*.wav`、`*.ogg` などを LFS にする。
- Unity の YAML (`*.unity`、`*.prefab`、`*.asset`、`*.meta`) はテキストとして扱う。

## 結果

- 別のフォルダに clone して Unity で開ける。これが P0 の完了条件で、まだ確かめていない。
- 実測 (2026-11-14 追記)。この PC に `git-lfs/3.8.0` を入れ、リポで `git lfs install` 済み。最初の LFS 対象をコミットする前に、`git lfs ls-files` で追跡を確認する。
- GitHub LFS の無料枠は小さい (具体値は要確認)。超えそうになったら、`.blend` の原本を別のストレージに置く。
