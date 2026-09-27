# コードレビュー（デスクトップ版・コンソール版）

| 項目 | 内容 |
|------|------|
| 工程 | 11. 実装（デスクトップ版・コンソール版の一巡）。工程 12 のリファクタリングと工程 13 の結合テストも、この文書に書く |
| 対象 | 区切りごとの差分（docs/desktop-console/05-class-design.md の 8 章） |
| レビュー者 | Claude（自己レビュー） |

## 観点と書式

- 観点は CLAUDE.md の「レビューの観点」（七箇条、引き算、整合、正しさ）。七箇条の外の指摘は、箇条に当てはめずに「整合」「正しさ」と書く。
- 区切りのレビューの前に、スキルの refactoring.md の臭いの表と書式、quality-gates.md の七箇条の節を読み直す（CLAUDE.md の「コーディング規範」）。
- 指摘は、スキルの書式（`<場所>: <臭いの名前>（<症状の根拠>）→ <技法> → <結果と検証>`）で書き、七箇条の箇条名か観点を添える。臭いの表に当てはまらないものは、名前を当てはめずに症状を書く。
- 各区切りで、テストの Green に加えて、実行環境で確かめたこと（アーキテクチャー設計書 11 章の #）を書く。

## 区切り 1: 骨組みと実行環境（2026-09-27）

### 作ったもの

| プロジェクト | 中身 |
|--------------|------|
| `Shos.Minesweeper.ConsoleApp` | `TerminalSession`（端末の準備と後始末、キーと大きさの読み取り）、`WindowsConsoleMode`（VT の解釈を有効にする）、`TerminalSize`、`VirtualTerminalSequences`（区切り 1 で使うシーケンスだけ）、`Program`（押したキーの中身を出す確認用。区切り 3 で置き換える） |
| `Shos.Minesweeper.ConsoleApp.Tests` | `TerminalSizeTests`（4 件）、`VirtualTerminalSequencesTests`（2 件） |
| `Shos.Minesweeper.Desktop` | `Program`、`App`（Fluent のテーマ、OS のライト・ダークに従う）、`MainWindow`（題名「マインスイーパー」、初級の中身の大きさ 400×382 の空のウィンドウ） |
| リポジトリ直下 | `Directory.Build.targets`（Avalonia のビルドの利用情報の送信を止める。下の「ユーザーの決定」） |

- テストファーストで進めた。コンソール版は、テストを先に書き、型がないためにビルドが失敗すること（Red）を確かめてから型を書いた。デスクトップ版は、ヘッドレスのテスト（`MainWindow` の題名）を先に書き、エントリ ポイントがないためにビルドが失敗することを確かめてから書いた。

### 実行環境の確認（アーキテクチャー設計書 11 章）

| # | 確かめたこと | 結果 |
|---|--------------|------|
| 1 | Avalonia.Headless.XUnit 12.1.3 が、xUnit v3 4.0.1 と Microsoft.Testing.Platform で動くか | **動かない。** テストを見つける段階で `MissingMethodException`（`Xunit.v3.TestIntrospectionHelper.GetTestCaseDetails`）になる。xUnit v3 の 4 系で拡張の API が変わったためで、調査書 4.9 のリスクが起きた。Avalonia.Headless.XUnit に新しい版はない（12.1.3 が最新）。アーキ 11 章 #1 の手（画面のテストは作らず、ビューモデルのテストと実機の確認で補う。ユーザーの確定、工程 8）に従った |
| 2 | Linux の上で `win-x64` の自己完結の単一ファイルを発行し、Windows で動くか | **動く。** WSL の Ubuntu で発行したデスクトップ版（約 97 MB）を Windows で起動し、題名「マインスイーパー」のウィンドウが出た。描画のネイティブのライブラリ（SkiaSharp）が読めないと起動の時点で終わるので、読めている。Windows で発行したものも同じく動いた。コンソール版の `win-x64` と `linux-x64` も、警告なしで発行できた |
| 5（一部） | 端末の VT、代替画面、キーの読み取り、キーを読んでいない間の打鍵 | WSL の疑似端末（`script`）で、Linux 向けのコンソール版を起動した。代替画面への出入り、カーソルの非表示・表示のシーケンスが出て、送ったキー（`a`）を読み、Q で終わった。キーを読んでいない間に送った文字は画面に出なかった（エコーなし）。**Windows Terminal、従来のコンソール ホスト、WSL の実際の端末での見え方と、Ctrl+C、IME の全角の数字の届き方は、ユーザーに実機で確かめてもらう**（下の「残ること」） |
| 7 | WSL の Linux に ICU が入っているか | **入っている**（libicu 74）。Linux 向けのコンソール版は、インバリアント モードにせずに起動した（ICU がなければ起動の時点で終わる） |

- Linux の上のビルドとテスト: WSL の Ubuntu に .NET 10 の SDK（10.0.401。Windows と同じ版）を `~/.dotnet` に入れ（ユーザーの決定）、リポジトリを bin と obj を除いてホームに写してから、ビルドと全テストを流した。**ビルドは警告なしで通り、全テスト 499 件が Green**（Windows でも 499 件が Green）。写したのは、Windows と Linux の中間ファイル（obj の復元の結果）が同じフォルダーで混ざらないようにするためである。

### ユーザーの決定

| 事項 | 決定 |
|------|------|
| WSL の .NET の SDK | `dotnet-install.sh` で WSL のホーム（`~/.dotnet`）に入れる。sudo も OS のパッケージも使わない |
| Avalonia のビルドの利用情報の送信 | 止める。Avalonia の `Avalonia.BuildServices` が、ビルドのたびに `AvaloniaStats` のタスクで匿名の利用情報を Avalonia 社に送る（ビルドの間に、このタスクのプロセスが動いているのを見つけた）。リポジトリ直下の `Directory.Build.targets` で、送る処理の条件の `UsedAvaloniaProducts` を空にした。この値を使うのは Avalonia と Avalonia.BuildServices のパッケージだけである。止まったことは、条件の値を戻して（送信は環境変数 `AVALONIA_TELEMETRY_OPTOUT=1` で止めたまま）ビルドするとログに `AvaloniaStats` が出て、戻さないと出ないことで確かめた |

### 設計からの変更

| 変更 | 理由 |
|------|------|
| `Shos.Minesweeper.Desktop.Tests` は、区切り 1 では作らず、区切り 5（最初のビューモデル）で作る | ヘッドレスのテストが動かない（#1）ので、区切り 1 のデスクトップ版には、テストで確かめられるものがない。テストが 0 件のプロジェクトは、Microsoft.Testing.Platform では失敗になる（終了コード 8）。0 件を許す設定を足すより、テストの対象ができたときに作るほうが単純である。いったん作ったヘッドレスのテストとプロジェクトは消した |
| Desktop.Tests に Avalonia.Headless.XUnit を入れない | #1 |

クラス設計書の 7.1 と 8 章、アーキテクチャー設計書の 11 章と 12 章に、この結果を書いた。

### 指摘

| # | 観点 | 指摘 | 対応 |
|---|------|------|------|
| 1 | 正しさ（後始末） | `TerminalSession` のコンストラクター: 途中で失敗したときの後始末がない（VT の解釈と出力の文字コードを変えた後に `Console.TreatControlCAsInput` を読んでいた。入力がリダイレクトされていると、ここで `IOException` になる。オブジェクトができる前に失敗するので `Dispose` が呼ばれず、変えた設定が呼び出し元のシェルに残る）→ 失敗しうる設定を最初に行う順に改め、理由をコメントに書いた → 入力をリダイレクトして起動すると同じ例外で終わり、コード ページ 932 のコマンド プロンプトのコード ページが 932 のままであることを確かめた。テスト 6 件 Green | 直した |
| 2 | 引き算 | `VirtualTerminalSequences`: 区切り 1 で使うシーケンス（代替画面、カーソル、行の残りの消去、色を戻す、カーソルの位置）だけを置いた。画面を消すシーケンスと色（`StyleOf`）は、使う区切り 3 で足す | 問題なし |
| 3 | 意図を表現 | `Program`（コンソール版）: 押したキーの中身を出す確認用の中身で、Ctrl+C の判定（`IsInterrupt`）を仮に書いている。区切り 3 で、`KeyboardMapping.IsInterrupt` と `GameLoop` に置き換える。置き換えることを冒頭のコメントに書いた | 区切り 3 で置き換える |
| 4 | 正しさ（後始末） | `TerminalSession.Output`（標準出力の `StreamWriter`）を閉じない。閉じると標準出力も閉じ、`Dispose` の後に例外の内容を出せなくなるため。理由をコメントに書いた | 問題なし |

七箇条で見て問題がなかった点: 型の名前は、クラス設計書 5.1、5.3 のとおり。`TerminalSize` は列と行の組に名前を付けた型（1.1 の「値の組に名前を付ける」）。`WindowsConsoleMode` は Windows でないとき・失敗したときに `null` を返し、呼ぶ側は `null` でなければ戻す、の 1 か所で扱っている。

### 残ること

- **ユーザーに確かめてもらうこと**（アーキ 11 章 #5。区切り 1 の完了の条件）: 確認用のコンソール版を、Windows Terminal、従来のコンソール ホスト、WSL の端末で動かす。
  - 代替画面に切り替わり、Q で元の画面に戻るか
  - Ctrl+C でも終わるか。そのとき出るキーの中身
  - 日本語の入力（IME）をオンにして「２」を打ったとき、`KeyChar=U+FF12` と出るか
  - 速く打っても、画面に余計な文字が残らないか
- デスクトップ版のウィンドウは、区切り 5 で中身を作ってから、見た目（題名の帯の明暗、書体、DPI）を確かめる（#8）。
- **上のユーザーの確認は、行われないまま区切り 2 に進んだ**（ユーザーの指示「区切り 2（共有の部品の移動）に進め」、2026-09-27）。#5 は、コンソール版の画面を作る区切り 3 で、同じ端末でまとめて確かめる。

## 区切り 2: 共有の部品の移動（2026-09-27）

### 手順

CLAUDE.md の「開発手順」とアーキテクチャー設計書 6.1 のとおり、移すだけの手順（A）と、機能を足す手順（B、C）を分け、手順ごとに全テストを Green にした。

| 手順 | 中身 | テスト |
|------|------|--------|
| A1 | `CellAnimationKind`、`CellAnimation`、`BoardAnimation` を、名前とメンバーを変えずに Presentation に移した（`git mv`）。テストも Presentation.Tests に移した | 499 件 Green |
| A2 | GameLogic の `Board.Contains` を公開した（テストを先に書いた）。Web 版の `BoardPlacement` に `ToBoard(Direction)` を加えた（テストを先に書いた）。`Direction` と `BoardCursor` を Presentation に移し、カーソルを盤面の座標で動かす形（`Move(Direction, Board)`）にした。`BoardView` は `Placement.ToBoard(direction)` で変えてから渡す。前の `BoardCursor` だけが使っていた `BoardPlacement.ToDisplay` と `Contains` を消した | 507 件 Green。縦と横を入れ替えた盤面での矢印キーの bUnit のテスト（`ArrowKeysFollowTheDisplayedDirectionOfATransposedBoard`）が通る |
| A3 | 読み上げの名前を `BoardNames`（`Of`、`CellOf`）に、盤面の寸法の定数を `BoardDimensions` に移した。`CellPresentation` から名前の部分を消した | 508 件 Green。盤面とマスの `aria-label` を確かめる bUnit のテストが通る |
| A4 | Razor に直接書いていたツールバー、難易度ダイアログ、勝利カードの文言を、`ToolbarTexts`、`DifficultyDialogTexts`、`CustomDifficultyTexts`、`WinCardTexts` に移した（テストを先に書いた）。地雷数の範囲の出し分けも `CustomDifficultyTexts.MineCountRangeOf` に移した | 527 件 Green。文言を確かめる bUnit のテスト（ツールバー、難易度ダイアログ、勝利カード）が通る |
| B | GameLogic に `BestTimesFile` を加えた（テストファースト。書いて読む、ない・壊れている・フォルダーがない・パスがフォルダー、フォルダーを作る、書けない） | 538 件 Green |
| C | `BoardCursor.MoveTo(CellPosition, Board)` を加えた（テストファースト。盤面の外は `ArgumentOutOfRangeException`） | 539 件 Green |

- 仕上げに、Presentation のプロジェクト ファイルのコメントを今の中身に合わせ、Web 版の設計書（docs/04-architecture.md、docs/05-class-design.md）の該当する箇所と状態の行に、移したことと参照先を書き足した。
- **Windows と WSL の Linux の両方で、ビルドは警告なしで通り、全テスト 539 件が Green**（Linux では、`BestTimesFile` のフォルダーを読むテストも通った。例外の種類の違いを `IOException` と `UnauthorizedAccessException` の両方で受けている）。
- 区切りの最後（テストのクラスを分け直した後）に、Windows の全体のビルドが、Web 版の `obj/Debug/net10.0/tmp-webcil` を消せずに何度も失敗した（MSB4018。CLAUDE.md の「コマンド」にある失敗）。ビルドのプロセスは残っておらず、フォルダーそのものを別のプロセス（Dropbox の同期か VS Code のファイルの監視の見込み。特定できなかった）が開いていた。コードの変更をすべて含むビルドは、分け直しの前に Windows と Linux で通っている。分け直しの後の状態は、Windows では Presentation.Tests のビルドとテスト（134 件）で、全体は WSL の Linux のビルドと全テスト（539 件 Green）で確かめた。

### 実行環境の確認

| 確かめたこと | 結果 |
|--------------|------|
| Web 版をブラウザーで動かし、キーボードの操作と読み上げの名前が変わらないこと（クラス設計書 8 章の区切り 2） | **確かめられていない。** 画面なしの Edge（`--headless`、`--dump-dom`）で描いた DOM を取り出そうとしたが、どの指定でも出力が空だった。深追いせず、ユーザーに確かめてもらう（下の「残ること」）。自動のテストでは、bUnit のテスト（HTML と `aria-label`、縦と横を入れ替えた盤面での矢印キー）がすべて通っている |

### 指摘

| # | 観点 | 指摘 | 対応 |
|---|------|------|------|
| 1 | ルールの統一 | `ScreenTextsTests`: 4 つの型のテストを 1 つのクラスにまとめていた（このリポジトリのテストは、型ごとに 1 つのテストクラス。クラス設計書 7.2 も 4 つのクラスとしている）→ 型ごとの 4 つのテストクラスに分けた → 14 件のまま、Presentation.Tests 134 件 Green | 直した |
| 2 | 引き算 | `BoardPlacement.ToDisplay`、`Contains`: 前の `BoardCursor` だけが使っていたので、カーソルが盤面の座標で動くようになって、テストからしか呼ばれなくなった → 消し、テストも `ToBoard(DisplayPosition)` の確かめだけを残した | 直した（手順 A2） |
| 3 | 引き算 | `CustomDifficultyTexts` の範囲の式の文: クラス設計書 3.5 では公開の定数にしていたが、`MineCountRangeOf` の中だけで使うので、非公開にした。クラス設計書を合わせた | 直した |
| 4 | Once And Only Once | 「ベスト {n} 秒」の形の文が、`DifficultyDialogTexts.BestTimeOf`（難易度の行）、`WinCardTexts.BestTimeOf`（更新しなかったときのこれまでのベスト）、既存の `Announcements.Won`（「ベスト {n} 秒。」）の 3 か所にある。どれも「ベストタイムが n 秒」の意図だが、画面の要素ごとに句点や置き方が違い、要素ごとに変わりうる。1 つにまとめると、要素の文言の型が互いに依存する | 直さない（クラス設計書 3.5 の「要素ごとに変わる理由が違う」に従う） |
| 5 | 正しさ | `BestTimesFile`: 受け止めるのは `IOException`（派生を含む）と `UnauthorizedAccessException` だけで、パスの誤り（`ArgumentException` など）はプログラムの誤りとして受け止めない（アーキ 10 章）。`Path.GetDirectoryName` がルートで null を返すと `ArgumentNullException` になるが、アプリが渡すパスはいつもフォルダーの中のファイルなので、プログラムの誤りとして扱う | 問題なし |

七箇条で見て問題がなかった点: 移した型の名前とメンバーは、クラス設計書 3 章のとおり。`BoardCursor` の端の判定は `Board.Contains` の 1 か所にあり、`MoveTo` のガード節もそれを使う。Web 版の Razor に残る文言は、Web 版だけのもの（旗モード）だけである。

### 残ること

- **ユーザーに確かめてもらうこと**: Web 版（`dotnet run --project Shos.Minesweeper`）をブラウザーで開き、次が前と同じであること。
  - 矢印キーでカーソルが動く（スマートフォンの縦の幅にして、縦と横を入れ替えた上級でも、見たとおりの方向に動く）
  - ツールバーのツールチップ（「難易度を変える」「新しいゲーム」「効果音（オン）」）、難易度ダイアログ（各行の大きさとベストタイム、カスタムの範囲と誤りの文）、勝利カードの文言
- 区切り 1 の #5（コンソール版を実際の端末で確かめる）は、区切り 3 でまとめて確かめる。
