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
| 5 | UI デザイン作成 | docs/03-ui-design.md |
| 6 | UI デザインレビュー | docs/reviews/03-ui-design-review.md |
| 7 | アーキテクチャー設計書作成 | docs/04-architecture.md |
| 8 | アーキテクチャー設計書レビュー | docs/reviews/04-architecture-review.md |
| 9 | クラス設計書作成 | docs/05-class-design.md |
| 10 | クラス設計書レビュー | docs/reviews/05-class-design-review.md |
| 11 | 実装（単体テストも一緒に書く） | ソースコード、テストプロジェクト |
| 12 | 結合テストとデバッグ | 修正済みのソースコード |

レビューでの指摘はレビュー用のファイルに記録し、反映したら元の成果物を更新する。

仕様書では、少なくとも次の論点を決めること。
- タッチでの旗の立て方（長押しにするか、「開く」と「旗」を切り替えるモードにするか）
- 盤面が画面に入らない場合の扱い（上級 30×16 をスマホの縦画面でどう表示するか。縮小表示／スクロール／ピンチズーム）
- 誤操作対策（長押しの判定時間、ダブルタップによるズームやスクロールとの競合）
- マス目の最小サイズ（タップしやすい大きさ。目安は 44px 前後）

UI デザインでは、仕様書で決めた操作と表示を画面に落とし込む。少なくとも次を含めること。
- 画面構成と各要素（盤面、残り地雷数、経過時間、リセット、難易度の選択など）
- スマートフォン（縦・横）、タブレット、PC の画面サイズごとのレイアウト（ワイヤーフレームは Markdown 内のテキスト図か SVG で描く）
- マスの各状態（未開放、開放済み、数字 1〜8、旗、地雷、誤った旗など）の見た目と配色
- 操作に対するフィードバック（押下中、長押しの進行、勝利、敗北）
- アクセシビリティ（色だけに頼らない表現、コントラスト、キーボード操作の要否）

現在の工程: 2. 調査書レビュー

## 開発環境
- .NET 10 (`net10.0`)、Blazor WebAssembly（スタンドアロン）
- 言語: C#, Razor, HTML/CSS。JavaScript interop は必要なときに限る
- テスト: .NET 10 の Blazor アプリで最も一般的な構成を使う。ロジックは xUnit、Razor コンポーネントは bUnit（Microsoft の Blazor ドキュメントが紹介しているコンポーネントのテスト用ライブラリ。xUnit と組み合わせて使う）
- ゲームロジックは UI に依存しない C# クラスとして分け、単体テストできるようにする

## コーディング規範（sustainable-code スキル）
設計・実装・テスト・レビューの判断基準には、`.claude/skills/` にあるプロジェクトのスキルを使う。このプロジェクトの会話は日本語なので、**`sustainable-code-jp` を使う**（`sustainable-code` は同じ内容の英語版）。どの reference を読むかはスキルの SKILL.md にある表に従い、ここでは工程との対応だけを定める。

| 工程 | スキルでの作業の種類 | 補足 |
|------|----------------------|------|
| 1〜6 調査書・仕様書・UI デザインの作成とレビュー | 適用しない | コードに触れない作業のため |
| 7, 9 アーキテクチャー設計書・クラス設計書の作成 | 設計の相談 | クラス名・メソッド名を決めるときは「命名」も適用する |
| 8, 10 アーキテクチャー設計書・クラス設計書のレビュー | 設計の相談 | 七箇条と Think Simple（引き算の設計）をレビューの観点にする |
| 11 実装 | 新機能の実装 | テストファーストで進め、一手ごとに Green を保つ |
| 11 実装したコードのレビュー | レビュー | 指摘はスキルの書式で `docs/reviews/` に記録する |
| 12 結合テストとデバッグ | バグ修正 | 不具合を再現するテストを先に足してから直す |

- 優先順位はスキルの定めどおり、ユーザーの指示、この CLAUDE.md、スキルの順とする。
- テストフレームワークの導入はスキルではユーザー確認が必要な事項だが、このプロジェクトでは「開発環境」の xUnit と bUnit でユーザーと合意済みなので、確認なしで導入してよい。

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
