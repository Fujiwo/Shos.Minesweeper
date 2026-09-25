# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## 目的
Web ブラウザーで遊べるマインスイーパーを作る。
- 難易度: 初級 / 中級 / 上級 / カスタム
- 公開: 静的サイトとして配置（Blazor WebAssembly スタンドアロン）

## 動作環境
スマートフォン、タブレット、PC の各ブラウザーで動作すること。
- 対象ブラウザー: 各 OS の最新版の Chrome / Edge / Safari / Firefox。iOS ではどのブラウザーも Safari と同じエンジン（WebKit）で動くので、iOS の実機確認は Safari を中心に行えばよい
- 入力: マウス（PC）とタッチ（スマホ、タブレット）の両方に対応する
- 画面: 縦向きと横向きの両方で、画面サイズに合わせたレイアウトにする（レスポンシブ）

## 開発手順
現在の工程: 10. クラス設計書レビュー（工程が承認されたら、Claude がこの行を次の工程に更新する）

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
| 11 | 実装（単体テストも一緒に書く） | ソースコード、テストプロジェクト。コードレビューの結果は docs/reviews/code-review.md に追記する |
| 12 | 結合テストとデバッグ | 修正済みのソースコード |
| 13 | リファクタリング | 整理したソースコード。対象の一覧と結果は docs/reviews/code-review.md に追記する |
| 14 | リリース準備 | docs/06-release.md（手順書・確認項目）、docs/release-notes.md |
| 15 | リリースレビュー | docs/reviews/06-release-review.md |
| 16 | 公開と公開後の確認 | 公開された URL。確認結果は docs/reviews/06-release-review.md に追記する |

### レビュー
- レビューは Claude が行い、その結果をユーザーが承認する。ユーザーからの指摘も同じレビューのファイルに記録する。
- 指摘はレビューのファイルに記録し、反映したら元の成果物を更新する。
- 実装（工程 11）は、機能のまとまりごとに区切って進め、一区切りごとにコードレビューを行って docs/reviews/code-review.md に追記する。

### 各工程で扱う内容
- **仕様書**: 調査書（docs/01-research.md）の 9 章に挙げた論点をすべて決めること。論点の一覧は調査書だけに置き、この CLAUDE.md には重ねて書かない。
- **UI デザイン**: 仕様書で決めた操作と表示を画面に落とし込む。少なくとも次を含めること。
  - 画面構成と各要素（盤面、残り地雷数、経過時間、リセット、難易度の選択など）
  - スマートフォン（縦・横）、タブレット、PC の画面サイズごとのレイアウト（ワイヤーフレームは Markdown 内のテキスト図か SVG で描く）
  - マスの各状態（未開放、開放済み、数字 1〜8、旗、地雷、誤った旗など）の見た目と配色
  - 操作に対するフィードバック（押下中、長押しの進行、勝利、敗北）
  - アクセシビリティ（色だけに頼らない表現、コントラスト。仕様書でキーボード操作に対応すると決めた場合は、フォーカスの表示）
- **リファクタリング**: 機能のまとまりをまたぐ見直しを行う（まとまりの中の整理は工程 11 で済ませる）。
  - 先に Claude がコード全体を見直し、直す対象を臭いの名前で一覧にしてユーザーの承認を得る。工程 11 のコードレビューで後回しにした指摘もこの一覧に含める。
  - 振る舞いは変えない。不具合を見つけたら、工程 12 と同じく再現するテストを先に足し、リファクタリングとは手順を分けて直す。
  - クラス名や責務を変えたら、docs/05-class-design.md（必要なら docs/04-architecture.md）も更新する。
  - 最後に全テストが Green であることと、主要な操作をブラウザーで一通り確かめる。

### リリース
- リリースレビューでは、少なくとも次を確認する: 全テストが Green、仕様書の要求を満たしている、動作環境の各端末・ブラウザーでの実機確認（ユーザーが行う）、発行物の設定（`<base href>`、公開先に必要なファイル）。
- **公開の操作（デプロイ、push、タグ付けなど）は、リリースレビューが承認され、ユーザーが明示的に指示したときだけ行う。**

### git
- リポジトリは `https://github.com/Fujiwo/Shos.Minesweeper`（remote は `origin`）。
- 改行コードは `.gitattributes` の `* text=auto` で統一している。Windows 側の作業ツリーは CRLF になるが、WSL の git からも差分として扱われない。
- コミットは、ユーザーの指示があるときだけ行う。

## 開発環境
- .NET 10 (`net10.0`)、Blazor WebAssembly（スタンドアロン）
- 言語: C#, Razor, HTML/CSS。JavaScript interop は必要なときに限る
- テスト: .NET 10 の Blazor アプリで最も一般的な構成として、ロジックは xUnit、Razor コンポーネントは bUnit を使う

## 設計方針
- ゲームロジックは UI に依存しない C# クラスとして分け、単体テストできるようにする

## コーディング規範（sustainable-code スキル）
設計・実装・テスト・レビューの判断基準には、`.claude/skills/` にあるプロジェクトのスキルを使う。このプロジェクトの会話は日本語なので、**`sustainable-code-jp` を使う**（`sustainable-code` は同じ内容の英語版）。どの reference を読むかはスキルの SKILL.md にある表に従い、ここでは工程との対応だけを定める。

| 工程 | スキルでの作業の種類 | 補足 |
|------|----------------------|------|
| 1〜6 調査書・仕様書・UI デザインの作成とレビュー | 適用しない | コードに触れない作業のため |
| 7, 9 アーキテクチャー設計書・クラス設計書の作成 | 設計の相談 | クラス名・メソッド名を決めるときは「命名」も適用する |
| 8, 10 アーキテクチャー設計書・クラス設計書のレビュー | 設計の相談 | 七箇条と Think Simple（引き算の設計）をレビューの観点にする |
| 11 実装 | 新機能の実装 | テストファーストで進め、一手ごとに Green を保つ |
| 11 実装したコードのレビュー | レビュー | 指摘はスキルの書式で docs/reviews/code-review.md に記録する |
| 12 結合テストとデバッグ | バグ修正 | 不具合を再現するテストを先に足してから直す |
| 13 リファクタリング | リファクタリング | 対象の一覧を承認してもらってから始め、一手ごとに Green を保つ |
| 14〜16 リリース | 適用しない | ビルドの設定と手順書の作成のため |

- 優先順位はスキルの定めどおり、ユーザーの指示、この CLAUDE.md、スキルの順とする。
- テストフレームワークの導入はスキルではユーザー確認が必要な事項だが、このプロジェクトでは「開発環境」の xUnit と bUnit でユーザーと合意済みなので、確認なしで導入してよい。

## コードの現状
コードは空の `blazorwasm` テンプレート（`Pages/Home.razor` は "Hello, world!"）のままで、ゲームロジックもテストプロジェクトもまだない。文書の進み具合は「開発手順」の「現在の工程」を見ること。

## コマンド
ソリューションは新しい XML 形式の `Shos.Minesweeper.slnx` で、プロジェクトは `Shos.Minesweeper/Shos.Minesweeper.csproj` の 1 つだけである。

```bash
dotnet build Shos.Minesweeper.slnx
dotnet run --project Shos.Minesweeper                        # http://localhost:5268
dotnet run --project Shos.Minesweeper --launch-profile https  # https://localhost:7163
dotnet watch --project Shos.Minesweeper                      # ホットリロード
dotnet publish Shos.Minesweeper -c Release                   # 静的ファイルとして bin/Release/net10.0/publish/wwwroot に出力
```

テストプロジェクトを作ったら、`dotnet test` と、1 件だけテストを実行する方法（`dotnet test --filter`）をここに追記すること。

## 構成とポイント
- `Program.cs` で `App` を `#app` に、`HeadOutlet` を `head::after` にマウントする。サーバー側はなく、すべてブラウザー内で動く。
- `App.razor` は Router である。既定のレイアウトは `Layout/MainLayout.razor`（`@Body` だけを描画）で、未一致のルートは `Pages/NotFound.razor` に回される。
- `wwwroot/index.html` はホストページである。csproj で `OverrideHtmlAssetPlaceholders` が有効なので、`<script type="importmap">`、`<link rel="preload" id="webassembly">`、`blazor.webassembly#[.{fingerprint}].js` は .NET 10 のビルド時に書き換えられるプレースホルダーである。削除や変更はしないこと。
- CSS 分離 (`*.razor.css`) を使う場合は、`index.html` 内でコメントアウトされている `<link href="Shos.Minesweeper.styles.css" rel="stylesheet" />` を有効にすること。グローバルスタイルは `wwwroot/css/app.css` にある。
- 名前空間は `_Imports.razor` で `Shos.Minesweeper` / `Shos.Minesweeper.Layout` を取り込んでいる。フォルダーを追加したときは、必要に応じてここに `@using` を追記すること。
- `Nullable` と `ImplicitUsings` が有効である。
