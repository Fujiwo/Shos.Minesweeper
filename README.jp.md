# Shos.Minesweeper

[English](README.md)

.NET 10 と C# で作ったマインスイーパーです。同じゲームのロジックを共有する、次の三つの版があります。

| 版 | 動く環境 | 技術 | 最新の版 |
|----|----------|------|----------|
| **Web 版** | スマートフォン、タブレット、PC のブラウザー | Blazor WebAssembly（スタンドアロン、静的サイト） | 1.1.0 |
| **デスクトップ版** | Windows 11（x64） | Avalonia UI 12 | 1.0.0 |
| **コンソール版** | Windows 11（x64）と Linux（x64）の端末 | `System.Console` と VT のシーケンス | 1.0.0 |

どの版も、画面の言語は日本語だけです。

## 遊び方

- **Web 版**: <https://fujiwo.github.io/Shos.Minesweeper/> を開きます。インストールは要りません。ページを読み込むときに、インターネットにつながっている必要があります。
- **デスクトップ版・コンソール版**: [GitHub Releases](https://github.com/Fujiwo/Shos.Minesweeper/releases)（タグ `desktop-v1.0.0`、`console-v1.0.0`）からまとめたファイルをダウンロードし、展開して実行ファイルを開きます。1 つのファイルに必要なものをすべて含めてあるので、.NET を入れる必要はありません。
  - 実行ファイルには署名をしていません。「Windows によって PC が保護されました」と出たら、「詳細情報」を押してから「実行」を押します。
  - Linux（コンソール版）: `tar -xzf Shos.Minesweeper.ConsoleApp-1.0.0-linux-x64.tar.gz` で展開し、`./Shos.Minesweeper.ConsoleApp` で実行します（「許可がありません」と出たら、先に `chmod +x Shos.Minesweeper.ConsoleApp` をします）。起動できないときは、ICU のライブラリ（Ubuntu・Debian では `libicu` で始まるパッケージ）を入れてください。
  - コンソール版: 端末の大きさは、80 列 × 24 行以上にしてください。

## 機能

- 難易度: 初級（9×9、地雷 10）、中級（16×16、地雷 40）、上級（30×16、地雷 99）、カスタム（幅 5〜30、高さ 5〜24、地雷 1〜「幅×高さ − 9」）。
- 最初に開いたマスとその周りには、地雷がありません。
- コード: 開いた数字のマスを開くと、周りの旗の数が数字と同じときに、残りのマスをまとめて開きます。
- 初級・中級・上級のベストタイムを、手元に保存します。インターネットには何も送りません。
- Web 版とデスクトップ版: 効果音と動き（広がるように開く、負けたときに地雷が現れる、勝ったときに旗が跳ねる）。どちらも OS の動きを減らす設定に従い、ツールバーで音を消せます。
- Web 版: 縦向き・横向きの画面に合わせたレイアウト、マウスとタッチの操作、ライトとダークの配色、スクリーンリーダーでの読み上げ。

## 操作

### Web 版

| 操作 | マウス | タッチ | キーボード |
|------|--------|--------|------------|
| マスを開く | 左クリック | タップ | 矢印キーでマスを選び、Space か Enter |
| 旗を立てる・外す | 右クリック | 長押し（400 ミリ秒） | F |
| コード | 開いた数字のマスを左クリック | 開いた数字のマスをタップ | 開いた数字のマスで Space か Enter |

ツールバーの旗モードのボタンを押すと、タップ（左クリック）と長押しが入れ替わり、タップで旗を立てられます。顔のボタンで新しいゲームを始め、難易度のボタンで難易度を変えます。

### デスクトップ版

| 操作 | マウス | キーボード |
|------|--------|------------|
| マスを開く（数字のマスならコード） | 左クリック | 矢印キーでマスを選び、Space か Enter |
| 旗を立てる・外す | 右クリック | F |
| 新しいゲーム | 顔のボタン | F2 |
| 難易度を変える | 難易度のボタン（左上） | Tab でボタンに移り、Enter |
| 効果音を消す・戻す | スピーカーのボタン（右上） | Tab でボタンに移り、Space |

### コンソール版

| キー | 動作 |
|------|------|
| 矢印キー、または H・J・K・L | マスを選ぶ（H 左、J 下、K 上、L 右） |
| Space または Enter | 選んだマスを開く（数字のマスならコード） |
| F | 旗を立てる・外す |
| N | 新しいゲーム |
| D | 難易度を選ぶ（1〜4 のキー、または矢印と Enter） |
| ? | キーの一覧 |
| Q、Ctrl+C | 終わる |

盤面は記号で表します（`#` 開いていない、`F` 旗、数字、`*` 地雷、`@` 踏んだ地雷、`X` 誤った旗）。環境変数 `NO_COLOR` を設定すると、色を付けずに表示します。

## 記録の保存先

| 版 | 保存先 |
|----|--------|
| Web 版 | ブラウザーのローカルストレージ（別のブラウザーや端末とは共有しません） |
| デスクトップ版 | `%LOCALAPPDATA%\Shos.Minesweeper\Desktop` |
| コンソール版（Windows） | `%LOCALAPPDATA%\Shos.Minesweeper\ConsoleApp` |
| コンソール版（Linux） | `~/.local/share/Shos.Minesweeper/ConsoleApp`（または `$XDG_DATA_HOME/Shos.Minesweeper/ConsoleApp`） |

## ソースからのビルド

[.NET 10 SDK](https://dotnet.microsoft.com/download/dotnet/10.0) が要ります。デスクトップ版のプロジェクトは `EnableWindowsTargeting` を付けて `net10.0-windows` を対象にしているので、ソリューション全体を Linux の上でもビルド・テストできます。

```bash
dotnet build Shos.Minesweeper.slnx
dotnet test                                                # 全テスト（リポジトリ直下で実行する）

dotnet run --project Shos.Minesweeper                      # Web 版: http://localhost:5268
dotnet run --project Shos.Minesweeper.Desktop              # デスクトップ版
dotnet run --project Shos.Minesweeper.ConsoleApp           # コンソール版（端末の中で実行する）

dotnet publish Shos.Minesweeper -c Release                 # 静的ファイルを bin/Release/net10.0/publish/wwwroot に出力
```

テストは Microsoft.Testing.Platform で動かします（`global.json` で選んでいます）。そのため、絞り込みは `--filter` ではなく、`--project` と一緒に `--filter-class`、`--filter-method` などの xUnit のオプションで行います。

## プロジェクトの構成

| プロジェクト | 内容 |
|--------------|------|
| `Shos.Minesweeper.GameLogic` | ゲームのルールと、ベストタイムの保存の形式。UI に依存しない |
| `Shos.Minesweeper.Presentation` | アプリで共有する、UI に依存しない部品（表示の文言、入力の割り当て、ゲームの進め方、効果音の定義と合成） |
| `Shos.Minesweeper` | Web 版（Blazor WebAssembly） |
| `Shos.Minesweeper.Desktop` | デスクトップ版（Avalonia UI。効果音は NAudio） |
| `Shos.Minesweeper.ConsoleApp` | コンソール版 |
| `Shos.Minesweeper.TestSupport` | テストの共通の補助 |
| `*.Tests` | 各プロジェクトのテスト（xUnit v3。Razor コンポーネントには bUnit） |

## 文書

このプロジェクトは、調査、仕様、UI デザイン、アーキテクチャー設計、クラス設計、実装、リファクタリング、結合テスト、リリースの工程を、工程ごとにレビューしながら文書に残して作りました。文書は日本語です。

- Web 版: [`docs/`](docs/) — [リリースノート](docs/release-notes.md)
- デスクトップ版・コンソール版: [`docs/desktop-console/`](docs/desktop-console/) — [リリースノート](docs/desktop-console/release-notes.md)
- レビュー: [`docs/reviews/`](docs/reviews/)、[`docs/desktop-console/reviews/`](docs/desktop-console/reviews/)
