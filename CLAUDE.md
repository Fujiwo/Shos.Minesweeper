# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## 目的
Web ブラウザーで遊べるマインスイーパーを作る。
- 難易度: 初級 / 中級 / 上級 / カスタム
- 公開: 静的サイトとして配置（Blazor WebAssembly スタンドアロン）
- 次の版（1.1.0）: WPF 版・コンソール版の前に、Web 版の UI を洗練し、効果音を入れる（ユーザーの決定、2026-09-26）。効果音は WPF 版でも使い、コンソール版では使わない。効果音の部品（どの出来事でどの音を鳴らすか、音そのもの）は `Shos.Minesweeper.Presentation` に置いて WPF 版と共有する。ただし、Presentation は効果音の実装（音を鳴らす仕組み。ブラウザーの Web Audio、WPF の再生の方法など）に依存しない。鳴らすのは各アプリである。共通のゲームの進め方（Presentation の `GameSession`）は効果音をサポートし、鳴らす効果音を、外から受け取った「音の出口」に渡す。出口を渡さない版（コンソール版）では鳴らないだけで、共通の側はどの版かに依存しない（ユーザーの決定、2026-09-26。docs/04-architecture.md の 4 章）
- 前提: Web 版 1.1.0 を公開したら、次に、同じゲームのデスクトップ版とコンソール版を、新しい一巡（仕様書から公開まで）として作る。そのため、UI の技術（Blazor、Avalonia、コンソール）に依存しない部品は、アプリで共有できるプロジェクトに置く。ゲームのルールとベストタイムの保存の形式は `Shos.Minesweeper.GameLogic`、表示の文言と押し方からの操作の割り当ては `Shos.Minesweeper.Presentation` である。どの UI でも形が変わらないと言えるものだけを共有し、形が UI の設計に左右されるもの（盤面の置き方、キーの受け方など）は、デスクトップ版・コンソール版の一巡で要るものが見えてから移す
- デスクトップ版の技術: 当初は WPF 版の予定だったが、Linux の上でビルド・テスト・公開ができ、Windows のデスクトップ アプリとして動くように、WPF の代わりに Avalonia UI で作る（ユーザーの決定、2026-09-27。docs/desktop-console/01-research.md の 1.1）。Avalonia の有料の製品は使わない。上の 1.1.0 の決定と、1.1.0 までの文書にある「WPF 版」は、デスクトップ版と読み替える

## 動作環境
スマートフォン、タブレット、PC の各ブラウザーで動作すること。
- 対象ブラウザー: 各 OS の最新版の Chrome / Edge / Safari / Firefox。iOS ではどのブラウザーも Safari と同じエンジン（WebKit）で動くので、iOS の実機確認は Safari を中心に行えばよい
- 入力: マウス（PC）とタッチ（スマホ、タブレット）の両方に対応する
- 画面: 縦向きと横向きの両方で、画面サイズに合わせたレイアウトにする（レスポンシブ）
- この節は Web 版の動作環境である。デスクトップ版・コンソール版の動作環境は、docs/desktop-console/02-spec.md の 4.1、5.1 にある（デスクトップ版は Windows 11、コンソール版は Windows 11 と Linux）

## 開発手順
現在の工程: デスクトップ版・コンソール版の一巡の工程 12（リファクタリング）。対象の一覧（docs/desktop-console/reviews/code-review.md の「工程 12」）の承認を得て、R1〜R4 を反映した。工程 12 の完了の承認を待っている（2026-09-28）（工程が承認されたら、Claude がこの行を次の工程に更新する）

Web 版 1.0.0 は工程 1〜16 をすべて終えた（2026-09-26 に公開）。1.1.0（UI の洗練と効果音）も同じ工程をたどり、すべて終えた（2026-09-27 に公開）。1.1.0 では、次のように進めた。
- 成果物は新しいファイルにせず、既存の文書に「改訂（1.1.0）」の節を足すか、該当する箇所を書き換えて、冒頭の状態に改訂したことを書く。レビューは、既存のレビューのファイルに「1.1.0 のレビュー」の節を追記する。コードレビュー・リファクタリング・結合テストは docs/reviews/code-review.md に 1.1.0 の節を足す。リリースノートは docs/release-notes.md の先頭に 1.1.0 を足す
- 変える部分に絞る。調査書は効果音の技術的な制約（ブラウザーの自動再生の制限、iOS の消音スイッチ、WPF での再生の方法など）の追補だけにする

デスクトップ版・コンソール版は、二つの版を一つの一巡（工程 1〜16）で作る（2026-09-27 に始めた）。
- 成果物は、Web 版の文書に足さずに、`docs/desktop-console/` に下の表と同じ名前で作る（例: docs/desktop-console/01-research.md）。レビューは `docs/desktop-console/reviews/` に、コードレビュー・リファクタリング・結合テストは docs/desktop-console/reviews/code-review.md に書く。Web 版と同じ内容（ゲームのルールなど）は繰り返さず、Web 版の文書を指す
- 共有の部品（GameLogic、Presentation）を変えたときは、この一巡の設計書に書き、Web 版の設計書（docs/04-architecture.md、docs/05-class-design.md）の該当する箇所に、変えたことと参照先を書き足す

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
| 12 | リファクタリング | 整理したソースコード。対象の一覧と結果は docs/reviews/code-review.md に追記する |
| 13 | 結合テストとデバッグ | 修正済みのソースコード |
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
  - 振る舞いは変えない。不具合を見つけたら直さずに記録し、工程 13（結合テストとデバッグ）で直す。直さないとリファクタリングを進められない場合だけ、再現するテストを先に足し、リファクタリングとは手順を分けて直す。
  - クラス名や責務を変えたら、docs/05-class-design.md（必要なら docs/04-architecture.md）も更新する。
  - 最後に全テストが Green であることを確かめる（ブラウザーでの確認は、続く工程 13 で行う）。

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
- デスクトップ版: Avalonia UI 12.1（MIT。パッケージは Avalonia、Avalonia.Desktop、Avalonia.Themes.Fluent）。効果音は NAudio 3.1（MIT。NAudio.Core、NAudio.WinMM。ユーザーの決定、2026-09-28）。NAudio.WinMM が Windows 向けの対象にしか入らないので、デスクトップ版とそのテストの対象は `net10.0-windows` で、`EnableWindowsTargeting` により Linux の上でもビルド・テスト・発行ができる（ユーザーの決定、2026-09-28）。画面のテスト（Avalonia.Headless.XUnit）は、xUnit v3 の 4 系で動かないので使わない。判断はビューモデルに置き、xUnit で確かめる（docs/desktop-console/reviews/code-review.md の区切り 1）
- コンソール版: `System.Console` と VT のシーケンスだけで作る（ライブラリなし）
- Avalonia のビルドの利用情報の送信は、リポジトリ直下の `Directory.Build.targets` で止めている（ユーザーの決定、2026-09-27）
- Linux の上のビルド・テスト・発行は、WSL の Ubuntu の `~/.dotnet` に入れた .NET 10 の SDK で確かめる（`~/.dotnet/dotnet`）。Windows と中間ファイルが混ざらないように、リポジトリを bin と obj を除いて WSL のホームに写してから行う（Git Bash から `wsl.exe` に `/mnt/c/...` のパスを渡すときは、`MSYS_NO_PATHCONV=1` を付ける）

## 設計方針
- ゲームロジックは UI に依存しない C# クラスとして分け、単体テストできるようにする
- 利用者が入力した文字列は、解釈する前に `Trim().Normalize(NormalizationForm.FormKC)` で整える（ユーザーの指示、2026-09-27）。変換は Presentation の `InputText.Normalize` の 1 か所に置く。ただし、ブラウザーの .NET は FormKC に対応していない（`PlatformNotSupportedException` になる）ので、Web 版は同じ変換を `browser.js` の `normalizeInput`（`trim().normalize("NFKC")`）で行う（docs/04-architecture.md の 9.1）

## コーディング規範（sustainable-code スキル）
設計・実装・テスト・レビューの判断基準には、`.claude/skills/` にあるプロジェクトのスキルを使う。このプロジェクトの会話は日本語なので、**`sustainable-code-jp` を使う**（`sustainable-code` は英語版で、このプロジェクトでは使わない。評価と改良の対象外で、日本語版を改良した後に、それに合わせる。ユーザーの決定、2026-09-27）。どの reference を読むかはスキルの SKILL.md にある表に従い、ここでは工程との対応だけを定める。

| 工程 | スキルでの作業の種類 | 補足 |
|------|----------------------|------|
| 1〜6 調査書・仕様書・UI デザインの作成とレビュー | 適用しない | コードに触れない作業のため |
| 7, 9 アーキテクチャー設計書・クラス設計書の作成 | 設計の相談 | クラス名・メソッド名を決めるときは「命名」も適用する |
| 8, 10 アーキテクチャー設計書・クラス設計書のレビュー | 設計の相談 | 観点は下の「レビューの観点」のとおり |
| 11 実装 | 新機能の実装 | テストファーストで進め、一手ごとに Green を保つ |
| 11 実装したコードのレビュー | レビュー | 観点は下の「レビューの観点」のとおり。指摘はスキルの書式で docs/reviews/code-review.md に記録する。区切りのレビューの前に、refactoring.md の臭いの表と書式、quality-gates.md の七箇条の節を読み直す |
| 12 リファクタリング | リファクタリング | 対象の一覧を承認してもらってから始め、一手ごとに Green を保つ |
| 13 結合テストとデバッグ | バグ修正 | 不具合を再現するテストを先に足してから直す。直す前に quality-gates.md の「バグ修正」を、性能を判断する前に「技術的負債とパフォーマンス」を読む |
| 14〜16 リリース | 適用しない | ビルドの設定と手順書の作成のため |

- **レビューの観点**（工程 8、10、11）: 七箇条、引き算（Think Simple）、整合（仕様書・UI デザイン・設計書と食い違わないか、書かれた方式で要求を実現できるか）、正しさ（例外の境界、後始末、実行環境の差）。七箇条の外の指摘は、箇条に当てはめずに「整合」「正しさ」と書く（docs/reviews/skill-evaluation.md の P1）。
- **実行環境での確認**: 区切りごとに、テストの Green に加えて、本物の実行環境で動かして確かめる（Web 版はブラウザー、デスクトップ版は Windows のアプリ、コンソール版は端末）。実行環境で振る舞いが変わるもの（ブラウザーの .NET の API、Linux の上で発行した Windows 向けのアプリ、発行の設定による正規化など）は、その区切りのうちに確かめる（1.1.0 の B2。P2）。
- **計画と引き算**: この CLAUDE.md の「目的」に書かれた計画（デスクトップ版・コンソール版など）は、スキルの判断ルール 1 の「具体的な根拠」に当たる。引き算で見送るときは、その計画に照らした理由を書く（P4）。
- 優先順位はスキルの定めどおり、ユーザーの指示、この CLAUDE.md、スキルの順とする。
- テストフレームワークの導入はスキルではユーザー確認が必要な事項だが、このプロジェクトでは「開発環境」の xUnit と bUnit でユーザーと合意済みなので、確認なしで導入してよい。

### スキルの評価の記録
デスクトップ版・コンソール版の一巡を終えた後に、日本語版のスキルの改善案を作る（ユーザーの指示、2026-09-27）。そのために、一巡の間、スキルの効果と使われ方を記録し続ける。

- 記録は docs/reviews/skill-log.md に追記する。書くきっかけ、書式、判定の区分は、そのファイルの冒頭のとおりである。後から判定しようとすると根拠が残らないので、出来事が起きたその場で書き、判定もその場で決める。
- 比べる基準は、1.0.0・1.1.0 の評価（docs/reviews/skill-evaluation.md）である。
- 一巡の間は、スキルのファイルを変えない（`.claude/settings.json` の `deny` で編集を禁じている）。改善したくなったら、案を記録に書くだけにする。この「コーディング規範」の節を変えたときは、日付と内容を記録の「条件の変更」に書く。
- 工程 16 の後に、記録から数を取り、docs/reviews/skill-evaluation.md に次の一巡の評価の章を足して、日本語版の改善案を作る。英語版は、日本語版を改良した後に合わせる。

## コードの現状

Web 版 1.1.0（UI の洗練と効果音）を `https://fujiwo.github.io/Shos.Minesweeper/` に公開した（2026-09-27、タグ `v1.1.0`。その前の版は 1.0.0、タグ `v1.0.0`）。公開の手順と確認項目は docs/06-release.md（1.1.0 は 9 章）、公開後の確認の結果は docs/reviews/06-release-review.md、変更の一覧は docs/release-notes.md にある。

- 実装（工程 11）は、クラス設計書（docs/05-class-design.md）の 8 章の区切り 1〜7 で行った。区切りごとのレビュー、リファクタリング（工程 12。R1〜R5 と、R5 の後のやり直しの RR1〜RR4）、結合テスト（工程 13）の記録は docs/reviews/code-review.md にある。
- 1.1.0 の実装は、クラス設計書の 12.10 の区切り 1〜7 で行った。区切りごとのレビュー、リファクタリング（R1〜R3）、結合テスト（見つけた不具合 B1、B2）の記録は、docs/reviews/code-review.md の「1.1.0」の節にある。
- デスクトップ版・コンソール版は、工程 11 の区切り 7 まで作った（工程 11 の区切りはこれですべて。2026-09-28 に承認された）。区切り 1 で骨組み（空のウィンドウと、端末の準備と後始末）を作り、区切り 2 で共有の部品（カーソル、演出、読み上げの名前、画面の文言、盤面の寸法、ベストタイムのファイル `BestTimesFile`）を GameLogic と Presentation に移した・足した。区切り 3、4 で、コンソール版はすべての難易度で遊べるようになった（ヘルプ、難易度の選択、カスタムの入力を含む）。区切り 5、6 で、デスクトップ版はすべての難易度で遊べるようになった（難易度ダイアログ、勝利カード、難易度に合わせたウィンドウの大きさを含む）。区切り 7 で、デスクトップ版に効果音と演出を加えた（OS のアニメーション効果に従う）。実装の区切りはクラス設計書（docs/desktop-console/05-class-design.md）の 8 章、レビューの記録は docs/desktop-console/reviews/code-review.md にある。
- デスクトップ版・コンソール版と共有する部品は、`Shos.Minesweeper.GameLogic`（ゲームのルールとベストタイムの保存の形式。1.1.0 で操作の結果 `MoveResult`）と `Shos.Minesweeper.Presentation`（表示の文言、押し方からの操作の割り当て。1.1.0 で 1 回のゲームの進め方 `GameSession`、効果音の部品、入力された文字列の整え方 `InputText`）に切り出してある（1.0.0 の工程 12 の R5、1.1.0 のアーキテクチャー設計）。

## コマンド
ソリューションは新しい XML 形式の `Shos.Minesweeper.slnx` で、次の 11 のプロジェクトがある。

| プロジェクト | 内容 |
|--------------|------|
| `Shos.Minesweeper.GameLogic` | ゲームのルールと、ベストタイムの保存の形式とファイルの読み書き（`BestTimesFile`。デスクトップ版・コンソール版が使う）。UI に依存しない。クラスライブラリ |
| `Shos.Minesweeper.Presentation` | UI の技術に依存しない、アプリで共有する表示と入力の部品（表示の文言と読み上げの名前、押し方からの操作の割り当て、入力された文字列の整え方 `InputText`、1 回のゲームの進め方 `GameSession`、効果音の種類・鳴らす音を決める表・波形の合成、キーボードのカーソル `BoardCursor`、演出の順序 `BoardAnimation`、盤面の寸法の決まり `BoardDimensions`）。音を鳴らす仕組みには依存しない。クラスライブラリ |
| `Shos.Minesweeper` | Blazor WebAssembly アプリ |
| `Shos.Minesweeper.GameLogic.Tests` | GameLogic のテスト（xUnit v3）。Web アプリに依存しない |
| `Shos.Minesweeper.Presentation.Tests` | Presentation のテスト（xUnit v3）。Web アプリに依存しない。`GameSession` のテストで盤面を決めるため、TestSupport を参照する |
| `Shos.Minesweeper.Tests` | Web アプリのテスト（xUnit v3。コンポーネントのテストには bUnit を使う） |
| `Shos.Minesweeper.TestSupport` | テストの共通の補助（盤面を文字の絵で書く `TestGames`）。クラスライブラリ |
| `Shos.Minesweeper.Desktop` | デスクトップ版（Avalonia のアプリ。Windows 11 向け） |
| `Shos.Minesweeper.ConsoleApp` | コンソール版（Windows 11 と Linux 向け）。`Console` という名前にしないのは、名前空間が `System.Console` を隠すため |
| `Shos.Minesweeper.ConsoleApp.Tests` | コンソール版のテスト（xUnit v3） |
| `Shos.Minesweeper.Desktop.Tests` | デスクトップ版のテスト（xUnit v3）。ビューモデルと計算を、Avalonia を起動せずに確かめる |

```bash
dotnet build Shos.Minesweeper.slnx
dotnet run --project Shos.Minesweeper                        # http://localhost:5268
dotnet run --project Shos.Minesweeper --launch-profile https  # https://localhost:7163
dotnet watch --project Shos.Minesweeper                      # ホットリロード
dotnet publish Shos.Minesweeper -c Release                   # 静的ファイルとして bin/Release/net10.0/publish/wwwroot に出力
dotnet run --project Shos.Minesweeper.Desktop                # デスクトップ版
dotnet run --project Shos.Minesweeper.ConsoleApp             # コンソール版（端末の中で動かす。入力をリダイレクトすると起動できない）
dotnet publish Shos.Minesweeper.Desktop -c Release -r win-x64 --self-contained -p:PublishSingleFile=true -p:IncludeNativeLibrariesForSelfExtract=true -o <出力先>
dotnet publish Shos.Minesweeper.ConsoleApp -c Release -r linux-x64 --self-contained -p:PublishSingleFile=true -o <出力先>   # win-x64 も同じ形

dotnet test                                                                                          # 全テスト（リポジトリ直下で。global.json と slnx を見つける）
dotnet test --project Shos.Minesweeper.GameLogic.Tests                                               # GameLogic のテストだけ（Web アプリをビルドしない）
dotnet test --project Shos.Minesweeper.Presentation.Tests                                            # Presentation のテストだけ（Web アプリをビルドしない）
dotnet test --project Shos.Minesweeper.ConsoleApp.Tests                                              # コンソール版のテストだけ
dotnet test --project Shos.Minesweeper.Desktop.Tests                                                 # デスクトップ版のテストだけ
dotnet test --project Shos.Minesweeper.GameLogic.Tests --filter-class "Shos.Minesweeper.GameLogic.Tests.BoardTests"  # 1 つのテストクラス
dotnet test --project Shos.Minesweeper.Tests --filter-method "*ClickingACellOpensIt"                 # 1 件（ワイルドカード可）
```

- Visual Studio でアプリをデバッグ実行している間や、MSBuild の常駐プロセスが残っている間や、VS Code の C# Dev Kit がファイルの変更を受けてビルドしている間は、コマンドラインのビルドが `obj/Debug/net10.0/tmp-webcil` を消せずに失敗する（MSB4018）ことがある。デバッグ実行を止め、`dotnet build-server shutdown` を実行してからビルドし直す。C# Dev Kit のビルドと重なったときは、30 秒ほど待ってからやり直すと通る。`dotnet test` の中のビルドがロックに当たったときは、`dotnet build` が通った後に `dotnet test --no-build` でテストだけを流せる。環境変数 `MSBUILDDISABLENODEREUSE=1` を設定しておくと、常駐プロセスが残りにくい。
- C# Dev Kit のビルドと重なると、デスクトップ版の中間の DLL が壊れて、`CreateAppHost` が `BadImageFormatException: Image is too small` で失敗することがある。`dotnet build-server shutdown` の後に、`Shos.Minesweeper.Desktop` の `bin` と `obj` を消してビルドし直す。
- テストは Microsoft.Testing.Platform で動かす。リポジトリ直下の `global.json` でこのモードを選んでいる（.NET 10 の SDK では、xUnit v3 のテストを従来の VSTest のモードで `dotnet test` できないため）。そのため、テストの絞り込みは `--filter` ではなく、`--filter-class`・`--filter-method` などの xUnit のオプションで行う。絞り込みは `--project` でテストプロジェクトを指定して行う（ソリューション全体に絞り込みを付けると、当てはまるテストのないプロジェクトが失敗扱いになりうる）。

## 構成とポイント
- `Program.cs` で `App` を `#app` に、`HeadOutlet` を `head::after` にマウントする。サーバー側はなく、すべてブラウザー内で動く。
- `App.razor` は Router である。既定のレイアウトは `Layout/MainLayout.razor`（`@Body` だけを描画）で、未一致のルートは `Pages/NotFound.razor` に回される。
- `wwwroot/index.html` はホストページである。csproj で `OverrideHtmlAssetPlaceholders` が有効なので、`<script type="importmap">`、`<link rel="preload" id="webassembly">`、`blazor.webassembly#[.{fingerprint}].js` は .NET 10 のビルド時に書き換えられるプレースホルダーである。削除や変更はしないこと。
- コンポーネントの見た目は CSS 分離（`*.razor.css`）で書き、`index.html` で `Shos.Minesweeper.styles.css` を読み込んでいる。配色のトークン（UI デザイン 4.1）とページ全体のスタイルは `wwwroot/css/app.css` にある。
- アプリの C# は、フォルダーごとの名前空間（`Components`、`Display`、`Browser` など）に置き、`_Imports.razor` で取り込んでいる。フォルダーを追加したときは、ここに `@using` を追記すること。
- 公開は `.github/workflows/deploy.yml`（GitHub Actions）で行う。push では動かず、手動で起動したときだけ、テスト、発行、`<base href>` の書き換えをして GitHub Pages に置く。手順は docs/06-release.md にある。
- JavaScript は `wwwroot/js/browser.js` だけで、C# から呼ぶのは `Browser/BrowserFeatures.cs` だけである（アーキテクチャー設計書 9 章）。
- `Nullable` と `ImplicitUsings` が有効である。
