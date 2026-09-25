# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## 目的
Web ブラウザーで遊べるマインスイーパーを作る。
- 難易度: 初級 / 中級 / 上級 / カスタム
- 公開: 静的サイトとして配置（Blazor WebAssembly スタンドアロン）

## 動作環境
スマートフォン、タブレット、PC の各ブラウザーで動作すること。
- 対象ブラウザー: 各 OS の最新版の Chrome / Edge / Safari / Firefox
- 入力: マウス（PC）とタッチ（スマホ、タブレット）の両方に対応する
- 画面: 縦向きと横向きの両方で、画面サイズに合わせたレイアウトにする（レスポンシブ）

## 開発手順
各工程の成果物は `docs/` に Markdown で作る（図は Mermaid）。
**各工程が終わったらユーザーの承認を得て、承認されてから次の工程に進むこと。**

| # | 工程 | 成果物 |
|---|------|--------|
| 1 | 調査書作成 | docs/01-research.md |
| 2 | 調査書レビュー | docs/reviews/01-research-review.md |
| 3 | 仕様書作成 | docs/02-spec.md |
| 4 | 仕様書レビュー | docs/reviews/02-spec-review.md |
| 5 | アーキテクチャー設計書作成 | docs/03-architecture.md |
| 6 | アーキテクチャー設計書レビュー | docs/reviews/03-architecture-review.md |
| 7 | クラス設計書作成 | docs/04-class-design.md |
| 8 | クラス設計書レビュー | docs/reviews/04-class-design-review.md |
| 9 | 実装（単体テストも一緒に書く） | ソースコード、テストプロジェクト |
| 10 | 結合テストとデバッグ | 修正済みのソースコード |

レビューでの指摘はレビュー用のファイルに記録し、反映したら元の成果物を更新する。

仕様書では、少なくとも次の論点を決めること。
- タッチでの旗の立て方（長押しにするか、「開く」と「旗」を切り替えるモードにするか）
- 盤面が画面に入らない場合の扱い（上級 30×16 をスマホの縦画面でどう表示するか。縮小表示／スクロール／ピンチズーム）
- 誤操作対策（長押しの判定時間、ダブルタップによるズームやスクロールとの競合）
- マス目の最小サイズ（タップしやすい大きさ。目安は 44px 前後）

現在の工程: 1. 調査書作成

## 開発環境
- .NET 10 (`net10.0`)、Blazor WebAssembly（スタンドアロン）
- 言語: C#, Razor, HTML/CSS。JavaScript interop は必要なときに限る
- テスト: xUnit（ロジック）、bUnit（コンポーネント）
- ゲームロジックは UI に依存しない C# クラスとして分け、単体テストできるようにする

## 現状
プロジェクトは空の `blazorwasm` テンプレート（`Pages/Home.razor` は "Hello, world!"）のままで、ゲームロジックはまだ実装されていません。テストプロジェクトもまだありません。

## コマンド

ソリューションは新しい XML 形式の `Shos.Minesweeper.slnx` で、プロジェクトは `Shos.Minesweeper/Shos.Minesweeper.csproj` の 1 つだけです。

```bash
dotnet build Shos.Minesweeper.slnx
dotnet run --project Shos.Minesweeper                        # http://localhost:5268
dotnet run --project Shos.Minesweeper --launch-profile https  # https://localhost:7163
dotnet watch --project Shos.Minesweeper                      # ホットリロード
dotnet publish Shos.Minesweeper -c Release                   # 静的ファイルとして bin/Release/net10.0/publish/wwwroot に出力
```

## 構成とポイント

- `Program.cs` で `App` を `#app` に、`HeadOutlet` を `head::after` にマウントします。サーバー側はなく、すべてブラウザー内で動きます。
- `App.razor` は Router です。既定のレイアウトは `Layout/MainLayout.razor`（`@Body` だけを描画）で、未一致のルートは `Pages/NotFound.razor` に回されます。
- `wwwroot/index.html` はホストページです。csproj で `OverrideHtmlAssetPlaceholders` が有効なので、`<script type="importmap">`、`<link rel="preload" id="webassembly">`、`blazor.webassembly#[.{fingerprint}].js` は .NET 10 のビルド時に書き換えられるプレースホルダーです。削除や変更はしないでください。
- CSS 分離 (`*.razor.css`) を使う場合は、`index.html` 内でコメントアウトされている `<link href="Shos.Minesweeper.styles.css" rel="stylesheet" />` を有効にしてください。グローバルスタイルは `wwwroot/css/app.css` にあります。
- 名前空間は `_Imports.razor` で `Shos.Minesweeper` / `Shos.Minesweeper.Layout` を取り込んでいます。フォルダーを追加したときは、必要に応じてここに `@using` を追記してください。
- `Nullable` と `ImplicitUsings` が有効です。
