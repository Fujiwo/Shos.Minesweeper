# コードレビュー

実装（工程 11）の区切りごとのコードレビューを、ここに追記する。区切りはクラス設計書（docs/05-class-design.md）の 8 章のとおりである。

- レビューは Claude が行い、ユーザーが承認する。ユーザーからの指摘も、同じ区切りの節に記録する。
- 観点は sustainable-code-jp スキルの七箇条と Think Simple（引き算の設計）である。
- 指摘は、スキルの書式（`<場所>: <臭いの名前>（<症状の根拠>）→ <技法> → <結果と検証>`）で書き、該当する七箇条の箇条名を添える。

| 区切り | 内容 | 状態 |
|--------|------|------|
| 1 | テストの土台とゲームのルール | 指摘をすべて反映済み。ユーザーが承認した（2026-09-25） |
| 2 | 盤面の表示 | 指摘をすべて反映済み。ユーザーが承認した（2026-09-25） |
| 3 | マウスとタッチの操作 | 指摘をすべて反映済み。ユーザーが承認した（2026-09-25） |
| 4 | ツールバー | 指摘をすべて反映済み。ユーザーが承認した（2026-09-25） |
| 5 | キーボードと読み上げ | 指摘をすべて反映済み。ユーザーが承認した（2026-09-25） |
| 6 | 難易度とベストタイム | 指摘をすべて反映済み。ユーザーが承認した（2026-09-25） |
| 7 | ページ全体の仕上げ | 指摘をすべて反映済み。ユーザーが承認した（2026-09-26） |
| 工程 12 のやり直し | R5 の後のコード全体（「工程 12: コードレビューのやり直し」） | ユーザーが承認した（2026-09-26）。指摘は、リファクタリングのやり直しで反映する |

## 区切り 1: テストの土台とゲームのルール

| 項目 | 内容 |
|------|------|
| レビュー日 | 2026-09-25 |
| 対象 | `Shos.Minesweeper.GameLogic` のすべて、`Shos.Minesweeper.Tests/GameLogic/`、`global.json`、各プロジェクトの設定 |
| 意図（ひとことで） | 盤面の規則、1 回のゲームの進行、難易度、ベストタイムの規則を、UI に依存しない C# で作り、テストで確かめられるようにする |

### 作ったもの

| 種類 | 内容 |
|------|------|
| プロジェクト | `Shos.Minesweeper.GameLogic`（クラスライブラリ）、`Shos.Minesweeper.Tests`（xUnit v3）。アプリから GameLogic を参照する |
| GameLogic の型 | `CellPosition`、`DifficultyKind`、`AllowedRange`、`Difficulty`、`CustomDifficultyValidation`、`CellState`、`Cell`、`Board`、`GameStatus`、`CellAppearance`、`MineChooser`、`Game`、`BestTimeOutcome`、`BestTimeResult`、`BestTimes`（クラス設計書 3 章のとおり） |
| テスト | `DifficultyTests`、`BoardTests`、`GameTests`、`BestTimesTests`、補助の `TestGames`（盤面を文字の絵で与え、結果も絵で比べる） |
| その他 | `global.json`（テストを Microsoft.Testing.Platform で動かす）。CLAUDE.md の「コマンド」にテストの実行方法を書いた |

テストファーストで進め、型ごとに Red（コンパイルエラーか、振る舞いのテストの失敗）を確かめてから実装した。

### 指摘

| # | 箇条 | 指摘 | 対応 |
|---|------|------|------|
| 1 | 的確なメソッド | `Game` の private メソッド: 読む順と呼ぶ順が違う（`ThrowIfOver`、`Win`、`ThrowIfInvalidMines`、`End`、`Start` の順に並び、`Open` の流れと関係がない。また静的メソッドの `ChooseMinesRandomly` がフィールドとプロパティの間にあった）→ メンバーの並べ替え | 公開メンバーの後に `ChooseMinesRandomly` を置き、private メソッドを `Open` の流れ（`ThrowIfOver` → `Start` → `ThrowIfInvalidMines` → `Win` → `End`）の順にした → 92 件 Green |
| 2 | Once And Only Once、的確な名前 | テストの盤面の絵: 重複したコード（同じ絵が `BoardTests` と `GameTests` に書かれ、しかも `ChordPicture` と `OneCellLeftPicture` という別の名前が付いていた）→ `TestGames` の 1 か所に集め、名前を `WallPicture` と `OneCellLeftPicture` にそろえた | 92 件 Green |
| 3 | Testable | `Game.ChooseMinesRandomly` のガード節（地雷数が候補の数より多い）: 契約にテストが張られていない → テストを足した | `RandomChoiceOfMoreMinesThanCandidatesIsAProgramError` を足した → 92 件 Green |

実装の途中で行ったリファクタリング（指摘ではなく、Refactor の一手として行ったもの）:

- `Board`: 重複したコード（`states[IndexOf(...)]` が 3 か所）→ `StateAt` に名前を付けて抽出した。
- `BestTimes.Record`: `previous <= seconds` は、`previous` が `null` のときに偽になる C# の規則に頼っていて、読み手に分かりにくい → `previous is int best && best <= seconds` と書き、`null` の扱いをコードに表した。

### 設計書との違い

| 違い | 理由 | 反映 |
|------|------|------|
| テストを Microsoft.Testing.Platform で動かし、`global.json` を置いた。VSTest 用のパッケージは入れていない | 最新の安定版の xUnit v3（4.0.1）は、.NET 10 の SDK では VSTest のモードで `dotnet test` できない。テストの絞り込みは `--filter-class`・`--filter-method` で行う | クラス設計書 7.1、CLAUDE.md の「コマンド」 |
| テンプレートにあったカバレッジの計測（`coverlet.collector`）は入れていない | 使う予定がない（引き算） | — |

### 引き算の点検

- クラス設計書にない公開メンバーや型は足していない。`Board` の内部の補助（`StateAt`、`Contains`、`IndexOf`、`Chord`、`OpenInChain`）は private である。
- `FlagCount` と `AreAllSafeCellsOpened` は、数を別に数えて持たずに、読むたびに配列を数える。上級でも 480 マスで、数を持つと開く・旗の操作のたびに数を合わせ続ける必要がある。遅いと分かったら、数を持つ形に変える。

### 良い点

- `Board` の盤面を変える操作は `internal` で、テストも `Game` を通して行っている。UI が `Game` を飛ばして盤面を変えられないことを、コンパイラーが守る。
- `TestGames` の絵で、盤面の状態がテストの中でそのまま見える（例: 敗北後の `.2F#x` / `.3X##`）。
- 地雷の配置で、「最初に開いたマスの周りに置かない」規則は本物のまま確かめている（候補を記録する偽の選び方と、本番の乱数を 50 回ずつ繰り返すテスト）。
- 前提を満たさない呼び出し（勝敗の後の操作、盤面の外の位置、選び方の誤り）は、ガード節で例外にし、テストを張っている。

### 検証結果

- `dotnet build Shos.Minesweeper.slnx`: 警告 0、エラー 0
- `dotnet test --project Shos.Minesweeper.Tests`: 92 件すべて成功

## 区切り 2: 盤面の表示

| 項目 | 内容 |
|------|------|
| レビュー日 | 2026-09-25 |
| 対象 | `Display/`（`DisplayPosition`、`BoardPlacement`、`IconKind`、`CellPresentation`）、`Browser/`（`BrowserFeatures`、`SizeObservation`）、`wwwroot/js/browser.js`、`Components/`（`BoardArea`、`BoardView`、`Icon`、`FlagShape`、`MineShape`）、`Pages/GamePage`、`Program.cs`、`_Imports.razor`、`wwwroot/css/app.css`、`wwwroot/index.html`、対応するテスト |
| 意図（ひとことで） | 盤面の領域の大きさを測り、UI デザイン 3.2 の規則で向きとマスの大きさを決めて、ゲームの状態どおりに盤面を描く |

### 作ったもの

| 種類 | 内容 |
|------|------|
| 表示の判断（xUnit） | `BoardPlacement`（UI デザイン 3.3 の表の 48 通りをテストにした）、`CellPresentation`（クラス、アイコン、数字、読み上げの名前） |
| ブラウザーとの境界 | `BrowserFeatures.ObserveSizeAsync` と `browser.js` の `observeSize`（`ResizeObserver`）。監視は `SizeObservation` で表し、破棄すると監視を止める |
| コンポーネント（bUnit） | `BoardArea`（大きさを測って置き方を子に渡す）、`BoardView`（描画だけ。操作は区切り 3 以降）、`Icon`（旗、誤った旗、地雷、踏んだ地雷）、`GamePage`（初級の盤面を出す骨組み） |
| 組み立て | `Program.cs` から `HttpClient` を消し、`TimeProvider` と `BrowserFeatures` を登録した。`Pages/Home.razor` を `GamePage.razor` に名前を変えた（`git mv`） |
| 見た目 | `app.css` に配色のトークン（ライトとダーク）とページの背景・書体・見えない見出しを置いた。`index.html` で CSS の分離を有効にした |

### 指摘

| # | 箇条 | 指摘 | 対応 |
|---|------|------|------|
| 1 | 意図を表現、単一責務 | `BoardView` のマスの中身: 数字を出すかどうかを `adjacentMineCount > 0` だけで決めていた（「開いていないマスの数字は 0」という `Board` の別の規則に頼っていて、「開いた数字のマスだけに数字を出す」という意図がコードにない。また、見た目の判断が `CellPresentation` と `BoardView` に分かれていた）→ `CellPresentation.NumberTextOf` に移し、表でテストした | 踏んだ地雷（開いたマスで数字を持つ）には数字を出さないことも、テストで確かめた → 188 件 Green |

### 設計書との違い

| 違い | 理由 | 反映 |
|------|------|------|
| 監視を表す `SizeObservation` を `public` のクラスにした（コンストラクターは `internal`） | bUnit のテストで、JavaScript の代わりに `NotifyResized` を呼ぶため。JavaScript から呼ばれるメソッドは、もともと公開される | クラス設計書 4.4 |
| `CellPresentation.NumberTextOf` を足した | 上の指摘 1 | クラス設計書 4.3 |
| `IconKind` は、この区切りで使う 4 つだけを定義した | 絵のない値を先に作らない。ほかのアイコンは、使う区切りで絵と一緒に足す | クラス設計書 4.3 |
| 旗と地雷の形を `FlagShape`・`MineShape` に分けた | 旗と誤った旗、地雷と踏んだ地雷で同じ形を使うため（Once And Only Once） | クラス設計書 5.2 の `Icon` |
| 配色のトークンを、区切り 7 ではなくこの区切りで `app.css` に置いた | マスの見た目に要るため。`index.html` の言語や文言などは、区切り 7 で行う | — |

### 引き算の点検

- `BrowserFeatures` には、この区切りで使う `ObserveSizeAsync` だけを作った。振動、localStorage、キーのスクロールの抑止は、使う区切りで足す。
- `BoardView` には、描画に要る引数（`Game`、`Placement`）だけを持たせた。旗モードとイベントは、使う区切りで足す。
- `GamePage` のレイアウトは、盤面の領域だけにした。ツールバーの場所は、区切り 4 で足す。

### 良い点

- `BoardPlacement` のテストが UI デザイン 3.3 の表そのもので、48 通りがすべて設計どおりの値になった。
- 盤面の大きさの判断は C#、スクロールの要否は CSS（`overflow: auto`）、立体の縁の太さは CSS（`round()`）と、判断の置き場所が 1 か所ずつになっている。
- 枠の太さは C# の定数（`BoardPlacement.FrameWidth`）を CSS の変数で渡し、計算と見た目で値が食い違わない。

### 検証結果

- `dotnet build Shos.Minesweeper.slnx`: 警告 0、エラー 0
- `dotnet test --project Shos.Minesweeper.Tests`: 188 件すべて成功
- 実際の表示: アプリを起動し、ヘッドレスの Chrome で画面の大きさを指定して写真を撮った（DevTools Protocol で大きさを指定。作業用のスクリプトで、リポジトリには置いていない）。初級のマスの大きさは、390×700 で 41px、844×350 で 37px、1280×650 で 48px になった。まだツールバーがないので上級などの値は UI デザインの表と比べられないが、ライトとダークの配色、立体の縁、盤面の枠が UI デザインの見本（docs/images/cell-states.svg）のとおりに描けていることを確かめた。
- 数字、旗、地雷のマスの実際の見た目は、盤面を操作できるようになる区切り 3 で写真を撮って確かめる。

### 作業環境で起きたこと

- Visual Studio でアプリをデバッグ実行していた間と、MSBuild の常駐プロセスが残っていた間に、コマンドラインのビルドが `obj/Debug/net10.0/tmp-webcil` を消せずに失敗した（MSB4018）。デバッグ実行を止め、常駐プロセスを止めて解消した。対処を CLAUDE.md の「コマンド」に書いた。

## 区切り 3: マウスとタッチの操作

| 項目 | 内容 |
|------|------|
| レビュー日 | 2026-09-25 |
| 対象 | `Input/`（`PointerInput`、`PressKind`、`PressGesture`、`CellAction`、`InputMapping`）、`Components/BoardView`（操作）、`Components/LongPressRing`、`Components/Icon`（スコップ）、`BrowserFeatures.VibrateAsync` と `browser.js` の `vibrate`、`Pages/GamePage`（開く・旗）、対応するテスト |
| 意図（ひとことで） | マウスのクリック・右クリックと、タッチのタップ・長押しを判定し、旗モードとマスの状態から「開く」「旗」を決めてゲームに伝える。押下中の表示と長押しの円で、操作の進み具合を見せる |

### 作ったもの

| 種類 | 内容 |
|------|------|
| 押し方の判定（xUnit） | `PressGesture`。クラス設計書 4.2 の状態の表を行ごとにテストにした（399/400 ミリ秒、9.9/10px の境界、別の指、長押しの後の離し、破棄を含む） |
| 操作の割り当て（xUnit） | `InputMapping`。押し方 3 × モード 2 × マス 4 の 24 通りをテストにした |
| 盤面の操作（bUnit） | `BoardView` が `pointerdown` をマスで、`pointermove`・`pointerup`・`pointercancel`・`pointerleave` を盤面で受ける。右クリックのメニューを止める。押下中の表示（未開放ならそのマス、開いた数字のマスならコードで開く範囲）、長押しの円、振動、勝敗の後の無視、新しいゲームでの押下の破棄、旗モードの枠の縞模様 |
| 長押しの円 | `LongPressRing`。3 層の輪を CSS のアニメーションで 400 ミリ秒かけて満たす。輪の上端に、成立したときの操作（旗・スコップ）を示す |
| ページ | `GamePage` が「開く」「旗」の意図を受けて `Game` を呼ぶ |

### 指摘

| # | 箇条 | 指摘 | 対応 |
|---|------|------|------|
| 1 | 的確な名前 | `BoardView.FollowGestureAsync`: 不適切な名前（「従う」だけでは、押し方の判定の状態に合わせて長押しの円と押下中の表示を更新する、という仕事が分からない）→ 名前の変更 | `UpdateFromGestureAsync` に変え、仕事をコメントにも書いた → 254 件 Green |

実装の途中にテストで見つけて直したもの:

- 長押しの後に表示が変わらなかった: タイマーから届いた長押しは Blazor のイベントではないので、描き直しが自動では起きない → 扱い終えたら `StateHasChanged` を呼ぶようにした（`PressedDisplayEndsWhenLongPressIsRecognized`、`RingDisappearsWhenLongPressIsRecognized` が Red から Green になった）。
- マスのクラスの末尾に空白が残った: 押下中のクラスを足す書き方のせいで、押下中でないときに `"cell opened n3 "` になった → クラスの文字列を `CellClassOf` で組み立てるようにした。

### 設計書との違い・補った決定

| 内容 | 理由 | 反映 |
|------|------|------|
| 長押しを扱った後に `StateHasChanged` を呼び、例外は `DispatchExceptionAsync` で Blazor のエラーの表示に渡す | 上の「テストで見つけて直したもの」。例外を捨てると、誤りがあっても画面に出ない | クラス設計書 5.2 の `BoardView` |
| 長押しの円のアニメーションの長さを `PressGesture.LongPressDelay` から CSS の変数で渡し、数はインバリアント カルチャーで書き出す | 判定と見た目の値を 1 か所にするため。小数点に「,」を使う言語の端末で CSS が壊れないようにするため | クラス設計書 5.2 の `LongPressRing` |
| `PressGesture` は押下に番号を付け、前の押下のタイマーの呼び出しが遅れて届いても無視する | 前の押下のタイマーが、次の押下を長押しと判定しないようにするため | — |
| `IconKind.Shovel` を足した（柄と刃の形） | 旗モードでの長押しの円に使う（UI デザイン 4.4） | — |

### 引き算の点検

- `GamePage` は、まだ旗モードも顔も持たない（区切り 4 で足す）。`BoardView` は `OnPressingChanged` を出すが、`GamePage` は区切り 4 で受ける。
- `BrowserFeatures` には、この区切りで使う `VibrateAsync` だけを足した。

### 良い点

- 押し方の判定（`PressGesture`）と割り当て（`InputMapping`）が Blazor に依存せず、表の形でテストできている。仕様書 4.1 の表の「旗モードでも開いた数字のマスはコード」「通常のモードの長押しは開放済みに効かない」も、表の行として読める。
- 長押しの円を出すかどうかと、長押しで何をするかが、同じ `InputMapping` の答えから決まる。円が出たのに何も起きない、という食い違いが起きない。

### 検証結果

- `dotnet build Shos.Minesweeper.slnx`: 警告 0、エラー 0
- `dotnet test --project Shos.Minesweeper.Tests`: 254 件すべて成功
- 実際の画面: アプリを起動し、ヘッドレスの Chrome に DevTools Protocol で本物のマウスとタッチの操作を送った（390×700、ライトとダーク）。左クリックで開く、右クリックで旗、タッチの長押しで旗（途中の円と押下中のくぼみを撮影）、地雷を開いて負ける、までを確かめ、数字の色・旗・地雷・踏んだ地雷・誤った旗が UI デザインのとおりに描けていることを写真で確かめた。
- まだ確かめていないこと: スマートフォンの実機での長押し（iOS の Safari で文字選択やコールアウトが出ないか、Android の Chrome で長押しが途切れないか、振動）。アーキテクチャー設計書 16 章のリスクで、ユーザーの実機での確認が要る。

### ユーザーの確認事項の扱い

報告の時に、スマートフォンの実機での長押しの確認を「今すぐ行うか、工程 12 でまとめて行うか」を尋ねた。ユーザーは個別の回答をせずに次の区切りへ進めたので、結合テストでまとめて行う。その後、リファクタリングと結合テストの順序を入れ替えたので（2026-09-25）、結合テストは工程 13 である。この記録のほかの箇所の工程の番号は、入れ替えた後の番号にそろえた。

### 残る課題

- `BoardView` は、描画に加えてポインターの操作の流れを受け持ち、区切り 5 でキーボードも加わる。区切り 5 の後に、長さと責務を見直す（巨大なクラスになりかけていないか）。

## 区切り 4: ツールバー

| 項目 | 内容 |
|------|------|
| レビュー日 | 2026-09-25 |
| 対象 | `Components/Toolbar`、`Components/ToolbarCounter`、`Components/ElapsedTime`、`Components/Icon`（顔 4 種、時計、山形）、`Display/DifficultyNames`、`Pages/GamePage`（レイアウト、リセット、旗モード、押下中の顔）、対応するテスト |
| 意図（ひとことで） | 難易度・残り地雷数・リセット（顔）・経過時間・旗モードを 1 本のツールバーに並べ、画面の向きと大きさに合わせて上バーと横バーを切り替える |

### 作ったもの

| 種類 | 内容 |
|------|------|
| ツールバー（bUnit） | `Toolbar`。難易度ボタン（表示名と山形。押されたことを知らせるだけで、ダイアログは区切り 6）、残り地雷数、リセット ボタン（顔 4 種）、経過時間、旗モード ボタン（`aria-pressed`、オンは塗りで示す）。ツールチップと読み上げの名前は UI デザイン 2.2、6.4 のとおり |
| 数字の表示 | `ToolbarCounter`。アイコンと数字の書式を 1 か所に置いた。4 文字（-100 以下）は小さくする |
| 経過時間（bUnit） | `ElapsedTime`。250 ミリ秒ごとに確かめ、表示する秒が変わったときだけ描き直す。破棄するとタイマーを止める |
| 難易度の表示名（xUnit） | `DifficultyNames`。区切り 6 の予定だったが、難易度ボタンで要るので、この区切りで作った |
| ページ | `GamePage` にツールバーを置き、リセット（同じ難易度で新しいゲーム）、旗モード（新しいゲームでも保つ）、押下中の顔を加えた。上バー（高さ 52px）と横バー（幅 88px）を切り替える |

### 指摘

| # | 箇条 | 指摘 | 対応 |
|---|------|------|------|
| 1 | 単一責務（変更の置き場所） | `GamePage.razor.css`: 不適切な関係（横バーに切り替えるときに、`::deep` で `Toolbar` と `ToolbarCounter` の中のクラス名（`.counter .shown`、`.counter-value`、`.difficulty`、`.flag-mode-label`）に踏み込んでいた。部品の中の並べ方を変えるたびに、ページの CSS も直すことになる）→ 責務の切り出し: ツールバーの置き場所（`toolbar-area`）を CSS のコンテナーにし、各部品が置き場所の縦長・横長をコンテナークエリーで見て、自分の並べ方を決めるようにした。切り替えの条件（メディアクエリー）は、ページの 1 か所のまま | `::deep` はページと `Toolbar` の CSS からなくなった。写真で、上バー・横バー・幅の狭い画面の見た目が変わらないことを確かめた → 275 件 Green |

直している途中に写真で見つけて直したもの:

- 横バーでリセット ボタンだけが幅いっぱいに広がらなかった: 上バーの `.reset { width: 44px }` のほうが、コンテナークエリーの中の `button { width: 100% }` より CSS の詳細度が高かった → 横バーの規則を `.toolbar button` にした。

テストで見つけて直したもの:

- 経過時間の描き直しの回数のテストが失敗した: bUnit の `RenderCount` は子の部品（`ToolbarCounter`）の描き直しも数えるので、1 回の描き直しで 2 増える → 「1 秒に届くまでは増えない」「届いたら増え、表示が 1 になる」を確かめる形にした（コードの誤りではなく、テストの前提の誤り）。

### 設計書との違い・補った決定

| 内容 | 理由 | 反映 |
|------|------|------|
| `DifficultyNames` をこの区切りで作った | 難易度ボタンで要るため | — |
| 上バーと横バーの切り替えで、各部品がコンテナークエリーで自分の並べ方を決める | 上の指摘 1 | クラス設計書 5.2 の `Toolbar` |
| `ElapsedTime` は `ShouldRender` を使わず、表示する秒が変わったときだけ `StateHasChanged` を呼ぶ | 変わったときだけ描き直しを求めれば足り、仕組みが 1 つ少なくて済む | クラス設計書 5.2 の `ElapsedTime` |
| `ToolbarCounter` に `Class`（置く場所ごとのクラス）を足した。数字はインバリアント カルチャーで書く | 残り地雷数と経過時間を見分けるため。マイナスを「-」で書くため（言語によっては別の記号になる） | クラス設計書 5.2 の `ToolbarCounter` |
| `Icon` の SVG に `data-kind`（アイコンの種類）を付けた | テストで顔を見分けるため | クラス設計書 5.2 の `Icon` |
| ボタンを指したときの色は、背景を文字の色に 8% 寄せる（`color-mix`） | 「ライトでは少し暗く、ダークでは少し明るく」（UI デザイン 5.6）を、配色ごとに値を持たずに 1 つの書き方で表せる | — |

### 引き算の点検

- 難易度ボタンは、押されたことを知らせるだけにした（ダイアログは区切り 6）。`Toolbar` のフォーカスを移すメソッド（`FocusResetButtonAsync` など）は、使う区切り（5、6）で足す。
- 旗モードの文字の出し分けは CSS だけで行い、C# で画面の幅を見ない。

### 良い点

- 顔、リセット、旗モード、経過時間の止まり方が、`Game` の状態から描かれていて、ツールバーは状態のコピーを持たない（アーキテクチャー設計書 7.1）。
- 経過時間だけが毎秒描き直され、盤面は描き直されない（アーキテクチャー設計書 7.3）。

### 検証結果

- `dotnet build Shos.Minesweeper.slnx`: 警告 0、エラー 0
- `dotnet test --project Shos.Minesweeper.Tests`: 275 件すべて成功
- 実際の画面（ヘッドレスの Chrome、DevTools Protocol で大きさを指定）:
  - 初級のマスの大きさが、UI デザイン 3.3 の表の値と一致した（360×640 で 38、390×700 で 41、844×350 で 37、640×320 で 34、1280×650 で 48、768×950 で 48）。盤面の領域の大きさも UI デザイン 3.2 の式どおりだった（例: 390×700 で 382×636、844×350 で 744×342）。
  - 上バー（390×700、320×640、1280×650）と横バー（844×350、640×320）、旗モードのオン（ボタンの塗り、盤面の枠の縞模様）、ダークの配色を写真で確かめた。1280×650 では旗モードの文字が出て、横バーでは出ない。
  - 最初のマスを開いてから 2.3 秒後に経過時間が「2」になり、負けると顔が「敗北」になって、経過時間が止まることを確かめた。
- 上級とカスタムの盤面でのツールバーと盤面の見た目は、難易度を変えられるようになる区切り 6 で確かめる。

## 区切り 5: キーボードと読み上げ

| 項目 | 内容 |
|------|------|
| レビュー日 | 2026-09-25 |
| 対象 | `Input/`（`Direction`、`BoardCursor`、`InputMapping` のキーボードの割り当て）、`Display/Announcements`、`Components/BoardView`（キーボード、選択中のマス）、`BrowserFeatures.SuppressKeyScrollingAsync` と `browser.js` の `suppressKeyScrolling`、`Pages/GamePage`（読み上げ用の領域、メモリーの中のベストタイム）、対応するテスト |
| 意図（ひとことで） | キーボードだけでゲームを最後まで進められるようにし、新しいゲームと勝敗をスクリーンリーダーに知らせる |

### 作ったもの

| 種類 | 内容 |
|------|------|
| 選択中のマス（xUnit） | `BoardCursor`。位置を盤面の座標で持ち、表示の向きで 1 マスずつ動かす。端では止まる。画面の向きが変わっても同じマスを選んだまま |
| キーボードの操作（bUnit） | 盤面は Tab キーの移動先を 1 つにし（`tabindex="0"`）、`aria-activedescendant` で選択中のマスを示す。矢印キーで動かし、Space・Enter で開き、F で旗。勝敗の後も矢印キーでは動かせる。新しいゲームで左上に戻る。矢印キーと Space でページがスクロールしないように、`browser.js` で既定の動作を止める |
| 選択中のマスの見た目 | 盤面にキーボードのフォーカスがあるとき（`:focus-visible`）だけ、内側 2px（`focus-inner`）と外側 3px（`focus`）の二重の枠を出す |
| 読み上げ（xUnit、bUnit） | `Announcements`（新しいゲーム、勝利（ベストタイムの 4 通り）、敗北の文）。`GamePage` に読み上げ用の領域（`aria-live="polite"`）を置き、リセットと勝敗で知らせる。ページを開いたときは知らせない |
| ベストタイム | `GamePage` がメモリーの中で `BestTimes` を持ち、勝ったときに記録して、読み上げの文に使う（保存は区切り 6） |

### 指摘

| # | 箇条 | 指摘 | 対応 |
|---|------|------|------|
| 1 | ルールの統一、Testable | キーボードの割り当て: 置き場所がそろっていない（マウスとタッチの割り当て（仕様書 4.1）は `InputMapping` に置いて表でテストしているのに、キーボードの割り当て（仕様書 4.5）は `BoardView` の中の `DirectionOf`・`ActionOf` にあり、bUnit でしか確かめられない）→ 責務の移動: `InputMapping.ActionForKey`・`DirectionForKey` に移し、表でテストした | `BoardView` は 16 行短くなった → 321 件 Green |
| 2 | 意図を表現 | `Announcements` のベストタイムの文: 「カスタムは何も付けない」を `_`（それ以外）で表していた（意図がコードに出ていない。想定外の値も黙って空になる）→ `BestTimeOutcome.NotEligible => ""` と明示し、想定外の値は例外にした | 321 件 Green |

直している途中に見つけて直したもの:

- 読み上げの見えない文字（U+200B）が、`GamePage` とテストのソースに、エスケープ（`​`）ではなく文字そのもので入っていた。ソースで読めるようにエスケープに直し、リポジトリのほかのファイルにこの文字が入っていないことを確かめた。
- テストで `ClassName`（null になりうる）を使った箇所に null の警告が出た → クラスの一覧（`ClassList`）で確かめる形にした。警告 0 に戻した。

### `BoardView` の長さと責務の見直し（区切り 3 の残る課題）

`BoardView` は 232 行（指摘 1 の後）で、描画と、ポインター・キーボードのイベントをつなぐ短いメソッドからなる。判断そのもの（押し方、割り当て、選択中のマス、マスの見た目、置き方）は、すべて別のクラスにあり、表でテストしている。`BoardView` の仕事は、クラス設計書のとおり「マスを描き、操作を意図に変えて伝える」の 1 つで、メソッドはどれも 10 行前後である。巨大なクラスには当たらないと判断し、これ以上は分けない。

### 設計書との違い・補った決定

| 内容 | 理由 | 反映 |
|------|------|------|
| キーボードの割り当てを `InputMapping` に置いた | 上の指摘 1 | クラス設計書 4.2 |
| 同じ文を続けて知らせるときは、末尾に見えない文字を足して中身を変える | クラス設計書で「方法は工程 11 で決める」としていた点。文を一度空にする方法は、描き直しを 2 回に分ける必要があり、仕組みが増える | クラス設計書 5.2 の `GamePage` |
| `BestTimes` を区切り 5 からメモリーの中で持つ | 勝ったときの読み上げの文に、ベストタイムの結果が要るため | クラス設計書 5.2 の `GamePage` |
| 盤面の要素そのもののフォーカスの枠は消し、選択中のマスの枠で示す | 盤面の枠とマスの枠が二重に出ないようにするため（UI デザイン 6.3 はマスの枠だけを定めている） | — |

### 引き算の点検

- 勝利カード、難易度ダイアログ、フォーカスを移すメソッド（`Toolbar.FocusResetButtonAsync` など）は、区切り 6 で作る。
- 経過時間は、毎秒は読み上げない（UI デザイン 6.4）。読み上げ用の領域に入れるのは、新しいゲームと勝敗だけにした。

### 良い点

- 画面の向きが変わっても選択中のマスが変わらないことを、`BoardCursor` のテスト（`CursorKeepsTheSameCellWhenTheOrientationChanges`）で確かめている（アーキテクチャー設計書レビューの指摘 4 の再発を防ぐ）。
- マウスとタッチとキーボードが、どれも最後は同じ `OnOpen`・`OnToggleFlag` に集まり、`GamePage` は入力の種類を知らない。

### 検証結果

- `dotnet build Shos.Minesweeper.slnx`: 警告 0、エラー 0
- `dotnet test --project Shos.Minesweeper.Tests`: 321 件すべて成功
- 実際のブラウザー（ヘッドレスの Chrome、DevTools Protocol でキーを送った。390×700、ライトとダーク）:
  - Tab キーの移動の順が、難易度 → リセット → 旗モード → 盤面になった（UI デザイン 6.3）。
  - 矢印キーで選択中のマスが動き、右端で止まり、ページはスクロールしなかった（`scrollY` は 0）。Enter で開き、F で旗が立った。
  - 選択中のマスの二重の枠が、ライト（外側が青、内側が白）とダーク（外側が黄、内側が黒）で UI デザインのとおりに出ることを写真で確かめた。
- まだ確かめていないこと: スクリーンリーダー（1 種類。仕様書 6.3）での実際の読み上げ。工程 13 で、ユーザーの環境で確かめる。

## 区切り 6: 難易度とベストタイム

| 項目 | 内容 |
|------|------|
| レビュー日 | 2026-09-25 |
| 対象 | `Browser/BestTimeStorage`、`BrowserFeatures` の localStorage の読み書きと `browser.js` の `readStorage`・`writeStorage`、`Components/DifficultyDialog`、`Components/WinCard`、`Components/Icon`（星、チェック、警告、×）、`Components/Toolbar`（フォーカスを移すメソッド）、`Pages/GamePage`（ダイアログ、`inert`、勝利カード、ベストタイムの読み込みと保存、フォーカスの移動）、`Difficulty.FindMineCountRange`、対応するテスト |
| 意図（ひとことで） | 難易度を選べるようにし、勝ったときにタイムとベストタイムの更新を示して、ベストタイムをブラウザーに残す |

### 作ったもの

| 種類 | 内容 |
|------|------|
| ベストタイムの保存（bUnit の JavaScript の偽物） | `BestTimeStorage`。形式は `{"Beginner":23,"Expert":301}`。読めない値（JSON でない、キーが違う、整数でない、0〜999 の外、カスタム）は「記録なし」として捨てる。JSON は `JsonDocument` と `Utf8JsonWriter` で読み書きし、リフレクションを使わない（公開するときのトリミングで壊れないようにするため） |
| 難易度ダイアログ（bUnit） | `DifficultyDialog`。初級〜上級の行（大きさ、地雷数、ベストタイム、現在の難易度のチェック）、カスタムの入力欄（初期値は現在の盤面、入力できる範囲の表示、地雷数の範囲は入力中の幅と高さで変わる）、誤りの表示（枠、警告のアイコンと文、`aria-invalid`、最初の誤りの欄へのフォーカス）。閉じ方は ×、Esc、幕。開いたときは現在の難易度の行（カスタムなら幅の欄）にフォーカス |
| 勝利カード（bUnit） | `WinCard`。タイムとベストタイムの行（UI デザイン 2.4 の表の出し分け）、「もう一度」「閉じる」、Esc で閉じる。フォーカスはボタンではなく見出しに移す。150 ミリ秒のフェードイン（動きを減らす設定では出さない） |
| ページ | 難易度ボタンでダイアログを開き、ほかの部分を `inert` にする。難易度を選ぶと新しいゲームを始めて知らせ、難易度ボタンにフォーカスを戻す。勝ったら記録し、更新したときだけ保存し、勝利カードを出して知らせる。勝利カードを閉じたら（「もう一度」でも）リセット ボタンにフォーカスを移す。ページを開いたときに保存されたベストタイムを読む |

### 指摘

| # | 箇条 | 指摘 | 対応 |
|---|------|------|------|
| 1 | Once And Only Once | 「幅と高さが正しいときだけ地雷数の上限が決まる」という規則: 重複したコード（`Difficulty.ValidateCustom` と `DifficultyDialog.RangeTextOf` の 2 か所で、幅と高さが範囲の中かを確かめていた。範囲の規則が変わると 2 か所を直すことになる）→ `Difficulty.FindMineCountRange`（幅か高さが誤っていれば `null`）に名前を付けて 1 か所にし、両方から使う | テストを 5 件足した（Red を確かめてから実装）→ 371 件 Green |

直している途中に見つけて直したもの:

- `BestTimeStorage` の保存と読み直しのテストが失敗した: `BrowserFeatures` は最初に読み込んだモジュールを持ち続けるので、保存のときに読み込んだモジュールが、後から決めた読み込みの結果を返さなかった（テストの前提の誤り）→ 読み込みの結果を決めてから `BrowserFeatures` を新しく作る補助にした。
- フォーカスのテストが失敗した: bUnit は、描き直した要素の参照の印（`blazor:elementreference`）を空にする（テストの前提の誤り）→ 最初の描画のときに印の値を取っておき、フォーカスを受けた `ElementReference` の ID と比べる形にした。
- 誤りの文の期待値の誤り: 高さが誤っているときの地雷数の誤りの文を「1〜72 の…」としていたが、クラス設計書 9.1 の決定 1 のとおり、高さが誤っていると上限は決まらず「1〜（幅×高さ − 9） の…」になるのが正しい → テストの期待値を直し、幅と高さが正しいときの「1〜72 の…」を別のテストにした。
- ページのテストで、5×5・地雷 16 のカスタムを始める手順が 2 か所に重複していた → `StartWinningCustomGame` にまとめた。

### 設計書との違い・補った決定

| 内容 | 理由 | 反映 |
|------|------|------|
| `Difficulty.FindMineCountRange` を足した | 上の指摘 1 | クラス設計書 3.3、5.2 の `DifficultyDialog` |
| フォーカスは描き直しの後に移す | 閉じた直後は `inert` が残っていて、その場で移しても効かないため | クラス設計書 5.2 の `GamePage` |
| 盤面の領域と勝利カードを `board-region` の中に置いた | 勝利カードを盤面の領域の中央に重ね、ダイアログの間は両方を `inert` にするため | クラス設計書 5.2 の `GamePage` |
| カスタムの入力欄の状態を `CustomField` にまとめた | 3 つの欄を同じ書き方で描くため（データの群れに型を付ける） | クラス設計書 5.2 の `DifficultyDialog` |
| 勝ったときのベストタイムの保存は、記録を更新したときだけ行う | クラス設計書 3.6 のとおり（`IsNewBest`） | — |

### 引き算の点検

- `BrowserFeatures` の localStorage の窓口は、読むと書くの 2 つだけにした。消す機能は作らない（仕様書 3.8「記録を消す機能は持たない」）。
- カスタムの入力で全角数字は受け付けない（クラス設計書 10 章）。

### 良い点

- 勝利カードのベストタイムの行（UI デザイン 2.4 の表）が、`BestTimeResult` の 4 つの結果にそのまま対応していて、表の行ごとにテストがある。
- 保存できない環境でも、例外は `browser.js` の中で受け止め、C# は `null`（記録なし）か何もしないかだけを扱う（アーキテクチャー設計書 9.1）。

### 検証結果

- `dotnet build Shos.Minesweeper.slnx`: 警告 0、エラー 0
- `dotnet test --project Shos.Minesweeper.Tests`: 371 件すべて成功
- 実際のブラウザー（ヘッドレスの Chrome、DevTools Protocol）:
  - **UI デザイン 3.3 の表**: 12 の画面の大きさで、上級とカスタムの最大（30×24）を選び、マスの大きさ・縦横の入れ替え・スクロールの有無が、表の値とすべて一致した（例: 上級は 390×700 で縦横を入れ替えて 21px、640×320 で 20px でスクロール、1280×650 で 36px。カスタムの最大は 768×950 で入れ替えて 29px、1280×650 で 24px）。区切り 4 の初級と合わせて、表の 48 通りすべてを実際の画面で確かめた。
  - 難易度ダイアログ（ライト、ダークでの誤りの表示、横向きの画面でダイアログの中がスクロールすること）と勝利カードを写真で確かめた。開いたときのフォーカス（現在の難易度の行）、誤りのときのフォーカス（最初の誤りの欄）、勝利カードのフォーカス（見出し「クリア！」）を確かめた。
  - 保存の形式で localStorage に書いてからページを開き直し、ダイアログに「ベスト 23 秒 / 記録なし / ベスト 301 秒」と出ることを確かめた。
- まだ確かめていないこと: 初級〜上級で実際に勝って記録が保存されること（乱数の盤面で勝つ手順を自動で作れないため）。工程 13 で、実際に遊んで確かめる。保存のしくみそのものは、`BestTimeStorage` のテストと上の読み込みの確認で確かめている。

## 区切り 7: ページ全体の仕上げ

| 項目 | 内容 |
|------|------|
| レビュー日 | 2026-09-25 |
| 対象 | `wwwroot/index.html`、`wwwroot/css/app.css`（読み込み中の表示）、`Components/BoardView.razor.css`（旗のアニメーション）、`Pages/NotFound`、`HostPageTests` |
| 意図（ひとことで） | ページ全体を日本語と UI デザインの配色にそろえ、操作の結果を知らせる残りのアニメーションを加える |

### 作ったもの

| 種類 | 内容 |
|------|------|
| ホストページ | 言語を `ja`、題名を「マインスイーパー」にし、ブラウザーのツールバーの色（`theme-color`）をライトとダークのページの背景色にした。エラーの表示を「エラーが発生しました。」「再読み込み」にし、閉じるボタンは絵文字（🗙）をやめて「×」と読み上げの名前「閉じる」にした（UI デザイン 1.1、2.5） |
| 読み込み中の表示 | 文字を「読み込み中」にし、円の色を配色のトークン（`grid`、`accent`）にした |
| 旗のアニメーション | 旗が立ったときに、旗のアイコンを 150 ミリ秒かけて 60% から 100% に広げる。動きを減らす設定では出さない（UI デザイン 5.2、5.7） |
| 見つからないときのページ | 日本語にし、マインスイーパーに戻るリンクを置いた（仕様書 5.5） |
| テスト | `HostPageTests`。`index.html` を読んで、言語・題名・ツールバーの色・拡大を禁止していないこと・エラーの文言を確かめる。.NET 10 のビルドが書き換えるプレースホルダーと `<base href="/">` が残っていることも確かめ、うっかり消すのを防ぐ（CLAUDE.md の「構成とポイント」） |

### 指摘

| # | 箇条 | 指摘 | 対応 |
|---|------|------|------|
| 1 | Think Simple（引き算） | `app.css` の `.blazor-error-boundary`: 使われていないスタイル（テンプレートのもので、このアプリは `ErrorBoundary` を使わない。英語の文言「An error has occurred.」も含む）→ 削った | 379 件 Green |
| 2 | ルールの統一（見た目） | 見つからないときのページ: 余白がなく文字が画面の端に貼り付き、リンクが既定の紫で、ダークの配色で見えにくかった（写真で見つけた）→ 余白を付け、リンクを文字の色にして下線で示し、フォーカスの枠をほかと同じにした | 379 件 Green |

### 引き算の点検

- テンプレートの `icon-192.png` は、`index.html` から参照されていないが、消していない。ファビコンは UI デザインで決めていないので、工程 14（リリース準備）で扱うかを決める。
- エラーの表示の見た目（テンプレートの黄色の帯）は、UI デザイン 2.5 が文言だけを定めているので、変えていない。

### 検証結果

- `dotnet build Shos.Minesweeper.slnx`: 警告 0、エラー 0
- `dotnet test --project Shos.Minesweeper.Tests`: 379 件すべて成功
- 実際のブラウザー（ヘッドレスの Chrome）: 通信を遅くして読み込み中の表示（「読み込み中」、配色に合わせた円）を撮った。読み込んだ後のページの言語が `ja`、題名が「マインスイーパー」、`theme-color` がライトとダークの 2 つになり、見えない見出しがあることを確かめた。`/not-found` で日本語のページが出ることを確かめた。
- 公開用のビルド（`dotnet publish -c Release`、出力は作業用フォルダー）: 警告なしで通り、プレースホルダー（importmap、読み込むスクリプトの名前、preload）が書き換えられ、`js/browser.js` が含まれた。圧縮（Brotli）後の `_framework` は約 2.8MB。
- **アーキテクチャー設計書 16 章のリスク「`browser.js` を、サブパス（`/Shos.Minesweeper/`）に置いたときにも読み込めるか」**: 公開用のビルドを、GitHub Pages と同じく `/Shos.Minesweeper/` の下に置き、`<base href>` をそのサブパスに書き換えた小さな静的サーバー（作業用のスクリプト）で配った。盤面の領域の大きさの監視（`browser.js`）が効いて盤面が描かれ、クリック、右クリック、長押し、経過時間、敗北までが開発用のサーバーと同じに動いた。このリスクは工程 11 の範囲では確かめられた。GitHub Pages そのもので確かめるのは工程 16。
- 旗のアニメーションは、短い（150 ミリ秒）ので写真では確かめていない。工程 13 で目で確かめる。

## 工程 11 のまとめ（工程 13 に送る事項）

区切り 1〜7 で、仕様書・UI デザインの機能をすべて実装した。テストは 379 件（xUnit と bUnit）で、すべて成功している。実際のブラウザーでは、UI デザイン 3.3 の表の 48 通り（12 の画面 × 4 つの盤面）のマスの大きさを確かめた。

工程 13（結合テストとデバッグ）で確かめること。その前の工程 12（リファクタリング）では、振る舞いを変えないので、これらは確かめない:

| 事項 | 出どころ |
|------|----------|
| スマートフォンの実機での長押し（iOS の Safari で文字選択やコールアウトが出ないか、Android の Chrome で長押しが途切れないか、振動） | 区切り 3、アーキテクチャー設計書 16 章 |
| タッチで押したときに、ポインターのイベントが盤面の要素に想定どおり届くか（暗黙のポインターの捕捉） | クラス設計書レビューの残る課題 |
| 上級の盤面で、操作から描き直しまでが 100 ミリ秒以内か（仕様書 6.2）。マスごとの `pointerdown` の登録（480 個）の影響 | アーキテクチャー設計書 7.3、16 章 |
| スクリーンリーダー（1 種類）での読み上げ（マスの位置と状態、新しいゲーム、勝敗） | 区切り 5、仕様書 6.3 |
| 初級〜上級で実際に勝って、ベストタイムが保存され、開き直しても残ること | 区切り 6 |
| 旗のアニメーション、勝利カードのフェードイン、動きを減らす設定 | 区切り 7 |
| 数字の色、長押しの判定時間、振動の長さ、円の大きさの実機での確認 | UI デザイン 9 章 |

## 工程 12: リファクタリング

| 項目 | 内容 |
|------|------|
| 作成日 | 2026-09-26 |
| 状態 | 対象の一覧をユーザーが承認した（2026-09-26）。R1〜R5 を反映済み。R5 で設計が変わったので、アーキテクチャー設計書レビュー、クラス設計書レビュー、コードレビュー、リファクタリングをやり直す（ユーザーの指示）。アーキテクチャー設計書とクラス設計書の再レビューは承認された（2026-09-26）。コードレビューのやり直しは、下の「工程 12: コードレビューのやり直し」 |
| 観点 | 機能のまとまり（区切り）をまたぐ見直し（CLAUDE.md の「各工程で扱う内容」）。sustainable-code-jp スキルの「リファクタリング」と、臭いと技法の名前 |

### 対象の一覧（案）

| # | 場所 | 臭い（症状の根拠） | 技法 | 優先 |
|---|------|--------------------|------|------|
| R1 | テストプロジェクト | 変更の発散（1 つのテストプロジェクトが、GameLogic のテストと Web アプリのテストの両方を抱え、GameLogic のテストを流すだけでも Web アプリと bUnit のビルドが要る。WPF 版・コンソール版を作ると決めた（2026-09-26）ので、GameLogic のテストは Web アプリから独立させる必要がある） | 責務の切り出し: `Shos.Minesweeper.GameLogic.Tests`（GameLogic のテスト）、`Shos.Minesweeper.TestSupport`（盤面を絵で書く補助 `TestGames`。クラスライブラリ）、`Shos.Minesweeper.Tests`（Web アプリのテスト。今のプロジェクトを残す）に分ける | 高（ユーザーの決定） |
| R2 | CSS（`Toolbar`、`WinCard`、`DifficultyDialog`、`NotFound`） | 重複したコード（ボタンの基本の見た目（枠、角の丸み 8px、背景、文字の色、指したときの色）が 3 つのコンポーネントに、フォーカスの枠（`focus` の色の 3px を 2px 離す。UI デザイン 6.3）が 5 か所に書かれている。区切りごとに書いたので、まとまりをまたいで重なった） | ページ全体の決まりとして `app.css` の 1 か所に置き、各コンポーネントには大きさなど固有の指定だけを残す | 中 |
| R3 | テスト（`BoardViewPointerTests`、`GamePageTests`、`DifficultyDialogTests`） | 重複したコード（ポインターのイベントを作る補助 `Mouse`・`Touch` が 2 つのクラスに、描き直した要素の参照の ID を取っておいて比べる手順が 2 つのクラスに書かれている） | `ComponentTestBase` に移して 1 か所にする | 中 |
| R4 | `ComponentTestBase` | 不適切な名前（コンポーネントのテストだけでなく、`BestTimeStorageTests` の土台にも使っている。仕事は「Web アプリのテストのための DI と JavaScript の偽物の準備」） | 名前の変更: `AppTestContext` にする | 低 |
| R5 | `Display/DifficultyNames`、`Display/Announcements`、`Input/InputMapping`（ポインターの割り当て）、`Input/PressKind`、`Input/CellAction`、`Browser/BestTimeStorage`（JSON の形式） | 変更の分散（WPF 版・コンソール版を作ると決めた。この型は UI の技術に依存しないのに Web アプリの中にあるので、そのままでは各アプリに同じ規則を書くことになり、規則が変わると複数のアプリを直すことになる）。R1〜R4 の承認の後に、ユーザーの指示で追加した（2026-09-26） | 責務の移動: UI の技術に依存しない共有部品のライブラリ `Shos.Minesweeper.Presentation` を作り、移す。`InputMapping` の DOM のキー名の割り当て（`ActionForKey`・`DirectionForKey`）は Web 専用なので、Web アプリの `KeyboardMapping` に分ける。`BestTimeStorage` は、保存の形式（JSON の読み書き）と保存先（localStorage）を分け、形式を `BestTimesJson` として移す。盤面の置き方（`BoardPlacement` など）、`PressGesture`、`CellPresentation` は、形が UI の設計に左右されるので移さない | 高（ユーザーの指示） |

進め方: R1（プロジェクトの構成）→ R3・R4（テストの補助。新しい構成の中で整える）→ R2（CSS。写真で見た目が変わらないことを確かめる）の順に、一手ごとに全テストを Green に保つ。R1 の後は、アーキテクチャー設計書 4 章（「テストプロジェクトを 1 つにする理由」）、クラス設計書 7.1、CLAUDE.md の「コマンド」を直す。

### 見送るもの（検討した結果）

| 候補 | 見送る理由 |
|------|------------|
| Esc キーで閉じる処理（`WinCard` と `DifficultyDialog` の 2 か所） | 3 行ずつで、2 か所だけである。部品や補助に出すと、読む対象が増える割に得るものが少ない |
| 勝利カードの文（`WinCard`）と勝利の読み上げの文（`Announcements`）のベストタイムの出し分け | 文言が違う（UI デザイン 2.4 と 6.4）。同じ意図の重複ではない（たまたま似ているだけのものはまとめない） |
| `CellPresentation` の Web 専用の部分（CSS のクラス名）と共通の部分（読み上げの名前、数字）を分ける、WPF 版・コンソール版と共有する表示のライブラリを作る | WPF 版・コンソール版の一巡で、実際に要るものだけを移す（CLAUDE.md の「目的」の「今後」）。今分けると、何が共通かを推測で決めることになる |
| `BoardView` を分ける | 区切り 5 で見直し、分けないと判断した（判断はすべて別のクラスにあり、残りは描画とイベントのつなぎ） |
| マスを 1 つずつのコンポーネントにする（描き直しの範囲を狭める） | 性能の改善で、リファクタリングではない。工程 13 で実機で計ってから、必要なら行う（アーキテクチャー設計書 7.3） |
| 使われていない `icon-192.png` | ファビコンの扱いは、工程 14（リリース準備）で決める |

### 結果

一覧の順に、一手ごとに全テストを流して Green を保った。振る舞いは変えていない（テストの件数は 379 件のまま、すべて成功）。

| # | 結果 | 検証 |
|---|------|------|
| R1 | `Shos.Minesweeper.GameLogic.Tests`（GameLogic のテスト 4 クラス、97 件）、`Shos.Minesweeper.TestSupport`（`TestGames` を `public` にして移した）、`Shos.Minesweeper.Tests`（Web アプリのテスト）に分けた。テストのファイルは `git mv` で移し、履歴を追えるようにした。GameLogic のテストの名前空間は `Shos.Minesweeper.GameLogic.Tests` にし、要らなくなった `using` を消した | `dotnet test --project Shos.Minesweeper.GameLogic.Tests` が、Web アプリをビルドせずに 97 件成功した。`dotnet test`（全体）は 379 件成功 |
| R3 | ポインターのイベントを作る補助（`Mouse`・`Touch`）、「マスを押して離す」の補助（`Click`）、フォーカスの移り先を確かめる補助（`ElementReferenceIdOf`・`LastFocusedId`）を、テストの共通の土台に移した。`GamePageTests` で 5 か所に書いていた「押して離す」の 2 行の組も `Click` にした（見直しの途中で見つけた、同じ種類の重複） | 379 件成功。使わなくなった `using` を消し、ビルドの警告は 0 |
| R4 | `ComponentTestBase` を `AppTestContext` に名前を変え、コンポーネントのフォルダーからテストプロジェクトの直下（名前空間 `Shos.Minesweeper.Tests`）に移した | 379 件成功 |
| R2 | ボタンの基本の見た目（枠、角の丸み、背景、文字の色、書体、指したときの色）とフォーカスの枠を `app.css` の 1 か所に置き、`Toolbar`・`WinCard`・`DifficultyDialog`・`NotFound` の CSS から重複を消した。各コンポーネントには、大きさなど固有の指定だけを残した | 379 件成功。変える前と同じ画面（ツールバーの上バー・横バー・ダークの旗モード、選択中のマスの枠、難易度ダイアログの誤りの表示、勝利カード）を撮り直し、見た目が変わっていないことを確かめた。フォーカスの移り先も変わっていない |

反映した文書:

| 文書 | 変更 |
|------|------|
| アーキテクチャー設計書 | 4 章（構成図、プロジェクトの参照の図、「テストプロジェクトを GameLogic 用とアプリ用に分ける理由」）、9.3（`app.css` の中身） |
| クラス設計書 | 7.1（3 つのテストプロジェクトと、テストクラスの置き場所）、7.2（`TestGames` の置き場所） |
| CLAUDE.md | 「コマンド」（5 つのプロジェクト、テストの実行と絞り込みの方法） |

気づいたこと（作業の環境）:

- ソリューション全体に `--filter-class` を付けて実行すると、ビルドのロック（MSB4018）に 3 回続けて当たり、確かめられなかった。当てはまるテストのないプロジェクトが失敗扱いになるおそれもあるので、CLAUDE.md には、絞り込みはプロジェクトを指定して行うと書いた。
- `dotnet test` の中のビルドがロックに当たったときは、直前に `dotnet build` が通っていれば、`dotnet test --no-build` でテストだけを流せる。

### R5 の結果（2026-09-26）

R1〜R4 の承認の後に、ユーザーの指示で R5 を追加した。WPF 版・コンソール版を作ることを CLAUDE.md の「目的」に前提として書き、UI の技術に依存しない共有部品のライブラリ `Shos.Minesweeper.Presentation` を作った。振る舞いは変えていない。

| 一手 | 内容 | 検証 |
|------|------|------|
| 1 | `Shos.Minesweeper.Presentation`（GameLogic だけに依存）と `Shos.Minesweeper.Presentation.Tests` を作り、`DifficultyNames`・`Announcements`・`PressKind`・`CellAction`・`InputMapping` とそのテストを `git mv` で移した。`InputMapping` のうち DOM のキー名に依存する部分（`ActionForKey`・`DirectionForKey`）は、Web アプリの `Input/KeyboardMapping`（`ActionFor`・`DirectionFor`）に分け、テストも `KeyboardMappingTests` に分けた | 3 つのテストプロジェクトで 379 件成功（件数は変わらない） |
| 2 | 保存の形式のテスト（`BestTimesJsonTests`）を Presentation の側に先に書き（Red を確かめた）、`BestTimeStorage` から JSON の読み書きを `BestTimesJson`（`Parse`・`Serialize`）に移した。`BestTimeStorage` は保存先（localStorage のキー）との読み書きだけになった。形式のいろいろな値を確かめるテストは `BestTimesJsonTests` に移し、`BestTimeStorageTests` はキーでの読み書きと読めないときのテストだけにした（テストの重複をなくした） | 383 件成功 |
| 3 | 実際のブラウザーで、キーボード操作（Tab の順、Enter で開く、F で旗）、勝ったときの流れ、保存したベストタイムの読み込みが、今までどおり動くことを確かめた | ヘッドレスの Chrome |

移さなかったもの（形が UI の設計に左右されるので、WPF 版・コンソール版の設計で要る形が見えてから移す）: `BoardPlacement`・`DisplayPosition`・`BoardCursor`・`Direction`（盤面の置き方と、表示の向きでの選択）、`PointerInput`・`PressGesture`・`KeyboardMapping`（DOM の値で入力を受ける）、`CellPresentation`・`IconKind`（CSS のクラスと SVG のアイコン）。

反映した文書:

| 文書 | 変更 |
|------|------|
| CLAUDE.md | 「目的」に WPF 版・コンソール版を作る前提と共有部品の置き場所、「コードの現状」にやり直しの計画、「コマンド」に 7 つのプロジェクトと Presentation のテストの実行方法 |
| アーキテクチャー設計書 | 状態、1.1 の方針、3 章の関心事の置き場所（5、7、13）、4 章の構成図・参照の図・「共有する部品を別のプロジェクトにする理由」、5 章の図と依存の規則、6.2（Presentation とアプリの C# クラス）、10 章（形式）、12 章（テストの方針） |
| クラス設計書 | 状態、1.1 の方針、2 章の型の一覧、4.2（`InputMapping` と `KeyboardMapping`）、4.3（`DifficultyNames`・`Announcements`）、4.4（`BestTimesJson` と `BestTimeStorage`）、7.1 と 7.3（テストプロジェクトとテストの観点） |

次にやること（ユーザーの指示）: アーキテクチャー設計書レビュー → クラス設計書レビュー → コードレビュー → リファクタリングの順にやり直し、その後に工程 13 に進む。

## 工程 12: コードレビューのやり直し（2026-09-26）

| 項目 | 内容 |
|------|------|
| レビュー日 | 2026-09-26 |
| 対象 | R5 の後のコード全体。重点は、R5 で作った・移したもの（`Shos.Minesweeper.Presentation` とそのテスト、`Input/KeyboardMapping`、`Browser/BestTimeStorage` とそのテスト、各プロジェクトの参照）と、アーキテクチャー設計書・クラス設計書の再レビューで決めたこととの食い違い |
| 意図（ひとことで） | WPF 版・コンソール版と共有できる、UI の技術に依存しない部品を Web アプリから分け、振る舞いを変えずに、Web アプリがそれを使う形にする |
| 観点 | sustainable-code-jp スキルの七箇条と、臭いと技法の名前。R1〜R4 はコード全体を見直した結果なので、それ以外の場所は、R5 の影響（使わなくなった `using`、古い置き場所を指すコメント）に絞って見直した |

### 指摘

指摘はここに記録し、直すのはこの後のリファクタリングのやり直しで行う（一覧に載せて、ユーザーの承認を得てから）。

| # | 重大度 | 箇条 | 指摘 | 直し方の案 |
|---|--------|------|------|------------|
| 1 | 中 | 的確な名前、単一責務 | `Shos.Minesweeper.Presentation/BestTimesJson.cs`・`Shos.Minesweeper.Presentation.Tests/BestTimesJsonTests.cs`: 置き場所と中身の不一致（表示と入力の部品を置く Presentation に、保存の形式がある。形式の中身は `BestTimes`・`Difficulty.Presets`・`Game.MaxElapsedSeconds` だけでできていて、変わる理由も GameLogic の側にある。アーキテクチャー設計書とクラス設計書の再レビューで、GameLogic に置くと決めた）→ 責務の移動 | `BestTimesJson` を GameLogic（名前空間 `Shos.Minesweeper.GameLogic`）へ、`BestTimesJsonTests` を GameLogic.Tests へ `git mv` で移す。`BestTimeStorage` の `using`、`BestTimeStorageTests` のコメントの参照先、Presentation の csproj のコメント（「記録の保存の形式」）を直す |
| 2 | 軽微 | 的確な名前 | `Presentation/InputMapping`: 不適切な名前（R5 でキーボードの割り当てを `KeyboardMapping` に分けたので、今は押し方（`PressKind`）の割り当てだけを受け持つ。それなのに名前は入力全体を指していて、`KeyboardMapping` と並べると、キーボードも `InputMapping` の一部のように読める）→ 名前の変更 | `PressMapping` にする（`PressMapping.ActionFor(PressKind, bool, Cell)`）。テストクラスも `PressMappingTests` にし、アーキテクチャー設計書とクラス設計書の名前を直す。公開する名前の変更なので、ユーザーの判断で決める |
| 3 | 軽微 | （七箇条の外: 後始末） | `BestTimesJson.Parse`・`RootObjectOf`: 後始末の漏れ（`JsonDocument` は `IDisposable` で、借りた配列をプールに返すのは `Dispose` のときである。`RootObjectOf` が `RootElement` を返す形なので、`using` を付けられない）→ メソッドの形を変える | 文書を返す補助（`DocumentOf`。読めなければ `null`）に変え、`Parse` の中で `using` で持って、文書が生きている間に値を読む。振る舞いは変わらないので、今のテストがそのまま安全網になる。直すのは指摘 1 で移した後 |
| 4 | 軽微 | Once And Only Once | `BestTimeStorageTests.UnreadableStorageMeansNoRecords` の `"not json"`: 重複したコード（テスト）（同じ値を `BestTimesJsonTests.UnreadableTextMeansNoRecords` で確かめている。R5 で形式のテストを移したときの残りで、形式が変わると 2 か所を直すことになる）→ 重複の削除 | 保存先に特有の場合（値がない・保存が禁止されている＝`null`）だけを残し、`[Fact]` にする。キーから読んだ値を形式に渡すことは `RecordsAreLoadedFromTheStorageKey` で確かめている |

指摘がなかったもの:

- 各プロジェクトの参照は、アーキテクチャー設計書 4 章の図どおりである（Presentation → GameLogic だけ、Presentation.Tests → Presentation だけ、Web アプリ → GameLogic・Presentation）。Presentation にも GameLogic にも、Blazor と JavaScript への依存はない。
- R5 で使わなくなった `using` は残っていない（`dotnet format style --diagnostics IDE0005 --severity info --verify-no-changes` で指摘なし）。古い置き場所（`Display`・`Input` の `DifficultyNames`・`InputMapping` など、`ComponentTestBase`）を指すコメントも残っていない。ただし、指摘 1 の移動に関わる 3 か所は除く。
- `KeyboardMapping` は、DOM のキー名を扱う部分だけで、`Input` にある。テストは表（`[Theory]`）で、仕様書 4.5 のキーと、キーでないもの（`a`・`Tab`）を確かめている。

### 引き算の点検

| 単位・仕組み | 解いている問題 | 判定 |
|--------------|----------------|------|
| `Shos.Minesweeper.Presentation`（5 つの型） | 表示の文言と押し方の割り当てを、WPF 版・コンソール版と共有する（ユーザーが作ると決めた） | 残す |
| `Shos.Minesweeper.Presentation.Tests` | Presentation を、Web アプリをビルドせずに確かめる | 残す |
| `KeyboardMapping` | DOM のキー名から操作と方向を決める規則を、表でテストできる形で 1 か所に置く | 残す。WPF 版・コンソール版はキーの受け方が違うので、共有しない |
| `BestTimeStorage` | localStorage のキーと、`BrowserFeatures` への橋渡し | 残す（クラス設計書の再レビューの引き算の点検のとおり） |

### 良い点

- R5 は、移す型のテストを一緒に `git mv` で移しているので、テストの履歴を追える。件数も移す前と後で変わっていない（形式のテストを先に書いた一手の 4 件を除く）。
- `InputMapping` の中の DOM に依存する部分だけを `KeyboardMapping` に分けていて、共有する側に Web の都合（キーの名前）が漏れていない。
- `BestTimeStorage` は、保存先の知識（キーの名前、`BrowserFeatures` を通ること）だけを持つ 2 行のメソッドになり、形式の規則と混ざっていない。

### 検証結果

- `dotnet build Shos.Minesweeper.slnx`: 警告 0、エラー 0
- `dotnet test`: 383 件すべて成功
- `dotnet format style Shos.Minesweeper.slnx --diagnostics IDE0005 --severity info --verify-no-changes`: 指摘なし（ファイルは変えていない）
- このレビューでは、コードを変えていない

### ユーザーの確認事項の扱い

指摘 2（`InputMapping` を `PressMapping` に変えるか）は、確認事項として挙げ、ユーザーは個別の回答をせずにレビューを承認した（2026-09-26）。これまでの前例（docs/reviews/02-spec-review.md）に従い、推した案どおり、名前を変えることで確定した。

## 工程 12: リファクタリングのやり直し

| 項目 | 内容 |
|------|------|
| 作成日 | 2026-09-26 |
| 状態 | 対象の一覧をユーザーが承認した（2026-09-26）。RR1〜RR4 を反映済み。工程 12 の完了をユーザーが承認した（2026-09-26） |
| 観点 | 「コードレビューのやり直し」の指摘 1〜4。sustainable-code-jp スキルの「リファクタリング」 |

### 対象の一覧

| # | 場所 | 臭い（症状の根拠） | 技法 | 出どころ |
|---|------|--------------------|------|----------|
| RR1 | `Presentation/BestTimesJson`、`Presentation.Tests/BestTimesJsonTests` | 置き場所と中身の不一致（保存の形式が、表示と入力の部品のプロジェクトにある） | 責務の移動: GameLogic（名前空間 `Shos.Minesweeper.GameLogic`）と GameLogic.Tests へ `git mv` で移す。`BestTimeStorage` の `using`、`BestTimeStorageTests` のコメント、Presentation の csproj のコメントを直す | 指摘 1 |
| RR2 | `BestTimesJson.Parse`・`RootObjectOf` | 後始末の漏れ（`JsonDocument` を破棄していない） | メソッドの形を変える: 文書を返す補助（`DocumentOf`）にし、`Parse` の中で `using` で持つ | 指摘 3 |
| RR3 | `BestTimeStorageTests.UnreadableStorageMeansNoRecords` | 重複したコード（テスト）（`"not json"` を形式のテストでも確かめている） | 重複の削除: `null` の場合だけを残し、`[Fact]` にする（テストは 383 件から 382 件になる） | 指摘 4 |
| RR4 | `Presentation/InputMapping`、`InputMappingTests` | 不適切な名前（押し方の割り当てだけなのに、入力全体を指す名前） | 名前の変更: `PressMapping`、`PressMappingTests`。`git mv` でファイル名も変える | 指摘 2 |

進め方: RR1 → RR2（移した後の場所で直す）→ RR3 → RR4 の順に、一手ごとに全テストを流して Green を保つ。振る舞いは変えない。

反映する文書:

| 文書 | 変更 |
|------|------|
| クラス設計書 | 状態と 3.7 の「コードはリファクタリングのやり直しで移す」を、移したことに改める（RR1）。`InputMapping` を `PressMapping` に改める（RR4） |
| アーキテクチャー設計書 | `InputMapping` を `PressMapping` に改める（RR4） |
| CLAUDE.md | 変えない（「目的」と「コマンド」は、すでに `BestTimesJson` を GameLogic に置く書き方になっている）。やり直しが済んだら「コードの現状」を直す |

見送るもの: 工程 12 の「見送るもの」から変わらない。

最後に全テストが Green であることを確かめる。ブラウザーでの確認は、続く工程 13 で行う。

### 結果

一覧の順に、一手ごとにビルドと全テストを流して Green を保った。振る舞いは変えていない。

| # | 結果 | 検証 |
|---|------|------|
| RR1 | `BestTimesJson` を `Shos.Minesweeper.GameLogic` へ、`BestTimesJsonTests` を `Shos.Minesweeper.GameLogic.Tests` へ `git mv` で移し、名前空間を GameLogic にした（同じ名前空間になったので、`using Shos.Minesweeper.GameLogic` は要らなくなった）。`BestTimeStorage` から `using Shos.Minesweeper.Presentation` を消し、コメントの参照先（`BestTimeStorage`、`BestTimeStorageTests`）と Presentation の csproj のコメントを直した | 383 件成功。GameLogic.Tests は 111 件（`BestTimesJsonTests` の 14 件を含む）、Presentation.Tests は 35 件 |
| RR2 | `RootObjectOf`（ルートの要素を返す）を `DocumentOf`（文書を返す。読めなければ `null`）に変え、`Parse` の中で `using var document` として持ち、文書が生きている間に値を読み終えるようにした。ルートがオブジェクトかどうかは、`Parse` の中のプロパティのパターン（`{ ValueKind: JsonValueKind.Object }`）で確かめる | 383 件成功（`BestTimesJsonTests` がそのまま安全網になった） |
| RR3 | `BestTimeStorageTests.UnreadableStorageMeansNoRecords`（`null` と `"not json"` の `[Theory]`）を、`MissingStorageValueMeansNoRecords`（`null` だけの `[Fact]`）にした。読めない形式の値は `BestTimesJsonTests` で確かめると、コメントで示した | 382 件成功（1 件減った） |
| RR4 | `InputMapping` を `PressMapping` に、`InputMappingTests` を `PressMappingTests` に、`git mv` でファイル名ごと変え、`BoardView` の 2 か所の呼び出しを直した | 382 件成功。ビルドの警告は 0 |

反映した文書:

| 文書 | 変更 |
|------|------|
| クラス設計書 | 状態、3.7（コードを移したこと）、`InputMapping` を `PressMapping` に（2 章、3.4、4.1、4.2、5.2、7.1、7.3、8 章）。4.2 の経緯の説明では、当時の名前 `InputMapping` を残した。7.3 の `BestTimeStorageTests` の観点（RR3） |
| アーキテクチャー設計書 | 状態、`InputMapping` を `PressMapping` に（3 章、4 章、5 章の図、6.2、6.3、8.2 の図） |
| CLAUDE.md | 「コードの現状」（やり直しが済んだこと） |

レビューの記録（docs/reviews/ の各ファイル）は、その時点の記録なので、古い名前のまま残した。

作業の環境で起きたこと: RR2 のビルドで、既知のロック（MSB4018）に 1 回当たった。`dotnet build-server shutdown` の後にビルドし直して通った（その間のテストは前のビルドで流れていたので、ビルドが通った後に流し直した）。

検証結果（最後）:

- `dotnet build Shos.Minesweeper.slnx`: 警告 0、エラー 0
- `dotnet test`: 382 件すべて成功
- ブラウザーでの確認は、続く工程 13 で行う（振る舞いは変えていない）

## 工程 13: 結合テストとデバッグ

| 項目 | 内容 |
|------|------|
| 開始日 | 2026-09-26 |
| 状態 | 自動での確認と、ユーザーの実機での確認が済んだ。不具合は見つからなかった。工程 13 の完了をユーザーが承認した（2026-09-26） |
| 対象 | 「工程 11 のまとめ」の「工程 13 で確かめること」。公開用のビルド（`dotnet publish -c Release`）を、GitHub Pages と同じサブパス（`/Shos.Minesweeper/`）で配って確かめた |
| 方法 | ヘッドレスの Chrome を DevTools Protocol で動かし、本物のマウス・タッチ・キーの入力を送った（作業用のスクリプト） |

### 自動で確かめたこと

| 事項 | 確かめ方 | 結果 |
|------|----------|------|
| 初級〜上級で実際に勝ち、ベストタイムが保存され、開き直しても残る | 盤面の数字から安全なマスを求めて開く簡単な解き手で、本物のクリックで遊んだ（行き詰まったら推測で開くので、負けたらリセットして続ける）。上級は 4 回目で勝った | 3 つとも勝った。勝利カード（「クリア！ タイム n 秒 ベストタイムを記録しました」）、読み上げの文（「クリア。n 秒。ベストタイムを記録しました。」）、localStorage（`{"Beginner":0,"Intermediate":2,"Expert":7}`）が正しく、ページを開き直すと、難易度ダイアログに 3 つのベストタイムが出た |
| 勝利カードのフォーカス | 勝った直後のフォーカスの位置 | 見出し「クリア！」に移っていた |
| 旗のアニメーションと、動きを減らす設定（UI デザイン 5.7） | 右クリックで旗を立て、`prefers-reduced-motion` を切り替えて、アイコンのアニメーションを読んだ | 通常は `flag-planted`（0.15 秒）、動きを減らす設定では `none` |
| 勝利カードのフェードインと、動きを減らす設定 | 勝った後に同じく切り替えた | 通常は `fade-in`（0.15 秒）、動きを減らす設定では `none` |
| 長押しの円と、動きを減らす設定 | タッチで長押しして、円のアニメーションを読んだ | 動きを減らす設定でも `fill-ring`（0.4 秒）が残る（UI デザイン 5.7 のとおり） |
| タッチの操作と、ポインターのイベントの届き方（暗黙のポインターの捕捉） | タッチを模擬して、長押し・タップ・ずらし・盤面の外まで動かして離す、を行った | 長押し: 待っている間は円が出て、400 ミリ秒で旗になり（円は消える）、離しても開かない。旗のマスのタップ: 何も起きない。10px 以上ずらす: 取り消し（開かず、旗も立たず、顔も戻る）。盤面の外まで動かして離す: 取り消し。タップ: 押している間は顔が「驚き」、離すと開いて顔が戻る。マスで押し始めたポインターのイベントは、盤面の要素に届いている |

### 上級の盤面の応答（仕様書 6.2: 100 ミリ秒以内）

Event Timing（入力から次の描画まで）で測った。前のクリックの処理が次の測定に混ざらないように、1 回ずつ間を空けてクリックした（開く、旗、推測で負けたらリセット）。

| 条件 | 中央値 | 95% 点 | 最大 | 100 ms を超えた回数 |
|------|--------|--------|------|---------------------|
| この PC、1 回目（60 回） | 16 ms | 40 ms | 88 ms | 0 |
| この PC、2 回目（60 回） | 16 ms | 32 ms | 96 ms | 0 |
| CPU を 4 倍遅くする、1 回目（30 回） | 80 ms | 224 ms | 312 ms | 5 |
| CPU を 4 倍遅くする、2 回目（30 回） | 80 ms | 496 ms | 520 ms | 10 |

- この PC では目標を満たす。CPU を遅くすると、中央値は目標の中だが、3 回に 1 回ほど超える。4 倍遅くした条件が、実機のスマートフォンとどれだけ近いかは分からないので、判断は実機で行う（仕様書 6.2 の「実機での確認に使うスマートフォンで確かめる」）。
- 最初の測定（中央値 72 ms、95% 点 272 ms）は、ほかの処理（ビルドと、別の Chrome）と重なっていた。静かなときに測り直したのが上の表である。測定はこのくらい揺れる。
- キーボードの矢印（盤面の描き直し 1 回）の処理は、中央値 29 ms（4 倍遅い条件で 76 ms）だった。ポインターの操作はその 2〜3 倍かかる。コードを読むと、1 回のポインターの操作で `BoardView` が 2 回描かれている見込みがある（`BoardView` 自身の描き直しと、`OnPressingChanged` を受けた `GamePage` → `BoardArea` → `BoardView` の描き直し）。

**試したが採らなかったこと**: マスを 1 つずつのコンポーネント（`BoardCell`。見た目が変わったマスだけを描き直す）にする試作を、コードの外（作業用のコピー）で作って測った。マスごとのイベントの登録を外すと、描き直しの処理は約半分になった（29 ms → 14 ms）ので、効きそうに見えた。しかし、試作を通して測ると、この PC では差がなく、CPU を 4 倍遅くした条件では、中央値 120 ms・64 ms、最大 1,328 ms・3,832 ms と、かえって悪い回があった（大きく開いたときに、480 個のコンポーネントがそれぞれ描き直されるためと見られる）。計測で効果を示せないので、入れない（sustainable-code-jp の「計測してから、ボトルネックだけを直す」）。

実機で遅いと分かったら、次の順に試す:

1. 1 回の操作で `BoardView` が 2 回描かれていないかを、bUnit の描画回数で確かめ、そうなら 1 回にする（描き直しの回数を減らす。コンポーネントの構成は変えない）。
2. それでも足りなければ、マスごとのイベントの登録（480 個）を減らす方法を、改めて測って決める。

### 実機で確かめること（ユーザー）

| 事項 | 端末 | 見るところ |
|------|------|------------|
| 長押し | iOS の Safari、Android の Chrome | 文字選択やコールアウト（長押しのメニュー）が出ない。長押しが途中で切れない。旗が立つときに振動する（Android。iOS の Safari は振動に対応していない） |
| タッチの操作 | 同上 | タップで開く、長押しで旗、指をずらすと取り消し、ダブルタップで拡大しない |
| 上級の盤面の応答 | 実機での確認に使うスマートフォン | 開く・旗・コードの操作で、遅れを感じない（仕様書 6.2 と受け入れ条件 12） |
| 読み上げ | スクリーンリーダー 1 種類（VoiceOver、TalkBack、NVDA のどれか） | マスの位置と状態（「3 行 5 列、未開放」など）、新しいゲーム、勝敗が読み上げられる |
| 見た目 | 各端末 | 数字の色、長押しの円の大きさ、旗のアニメーション、勝利カードのフェードイン（UI デザイン 9 章の「実機で確かめる」項目） |
| 画面の向き | スマートフォン、タブレット | 縦と横を切り替えると、盤面が画面に合わせて置き直される |

結果（2026-09-26）: ユーザーが実機で上の項目を確かめ、「正常」と報告した。上級の盤面の応答も、実機で遅れを感じなかったので、描き直しを減らす変更（上の 1. と 2.）は行わない。

### 見つかった不具合

なし（自動での確認と、実機での確認の範囲）。コードは変えていない。

### 作業の環境で起きたこと

- 試作の発行が、既知のロック（MSB4018）で続けて失敗した。発行の処理が作った一時フォルダーを消せない、というもので、Dropbox の同期が新しいファイルを掴んでいるためと見られる。ソースを Dropbox の外（作業用のフォルダー）に写して発行すると通った。
- 作業用のフォルダーに git worktree を作ると、ビルドの中間ファイルのパスが Windows の上限（260 文字）を超えて失敗した。Web アプリのフォルダー名を短くして写すと通った。

## 1.1.0 の実装（工程 11）

区切りはクラス設計書（docs/05-class-design.md）の 12.10 のとおりである。レビューの進め方と指摘の書式は、1.0.0 と同じである。

| 区切り | 内容 | 状態 |
|--------|------|------|
| 1 | 操作の結果 | 指摘を反映済み。ユーザーが承認した（2026-09-26） |
| 2 | 効果音の部品 | 指摘を反映済み。ユーザーが承認した（2026-09-26） |
| 3 | 1 回のゲームの進め方 | 指摘を反映済み。ユーザーが承認した（2026-09-26） |
| 4 | ブラウザーで鳴らす | 指摘を反映済み。ユーザーが承認した（2026-09-26） |
| 5 | 見た目の洗練 | 作業中 |
| 6 | 置き方 | 未着手 |
| 7 | 演出 | 未着手 |

## 1.1.0 区切り 1: 操作の結果

| 項目 | 内容 |
|------|------|
| レビュー日 | 2026-09-26 |
| 対象 | `Shos.Minesweeper.GameLogic` の `MoveOutcome`、`MoveResult`、`Board`、`Game`。`Shos.Minesweeper.GameLogic.Tests/GameTests.cs` |
| 意図（ひとことで） | 盤面の操作（開く・旗）が、その操作で起きたことを値で返すようにする。効果音と演出の元になる |

### 作ったもの

| 種類 | 内容 |
|------|------|
| GameLogic の型 | `MoveOutcome`（`NoChange`・`Opened`・`FlagPlaced`・`FlagRemoved`）、`MoveResult`（操作したマス、起きたこと、新たに開いたマス、操作の後のゲームの状態） |
| 変えたメンバー | `Game.Open`・`Game.ToggleFlag` が `MoveResult` を返す。`Board.Open`（`internal`）が新たに開いたマスを返す |
| テスト | `GameTests` に 12 件（クラス設計書 12.2 の表のすべての行。0 の連鎖、数字の 1 マス、何も起きない開く、旗の数が合わないコード、開くマスが残っていないコード、最初の一手で勝つ、コードで勝つ、地雷を開く、誤った旗のコードで 2 つの地雷を開く、旗を立てる・外す、開いたマスへの旗） |

テストファーストで進めた。12 件を先に書き、`MoveOutcome` がないこと・`Open` が値を返さないことによるコンパイルエラー（Red）を確かめてから実装した。画面（`GamePage`）は戻り値を使わないので、振る舞いは 1.0.0 のままである。

### 指摘

| # | 箇条 | 指摘 | 対応 |
|---|------|------|------|
| 1 | 意図を表現 | `GameTests` の `FencedPocketPicture` の説明: 誤解を招くコメント（「(1, 0) と (1, 2) の旗で連鎖から囲った盤面」と、絵そのものに旗があるように読める。絵に描けるのは地雷だけで、旗はテストの中で立てる）→ コメントを直す | 「(1, 0) と (1, 2) に旗を立ててから右下を開くと、(0, 1) が連鎖から外れて残る盤面」に直した → 123 件 Green |

実装の途中で決めたこと:

- `Game.ToggleFlag` は、操作の後のマスの状態だけで起きたことを決める（旗なら `FlagPlaced`、未開放なら `FlagRemoved`、開放済みなら `NoChange`）。「旗」は未開放と旗を入れ替えるだけなので、後の状態から前の状態が決まる。この理由をコメントに書いた。前と後の 2 つの状態の組で分ける書き方より、読む場合分けが少ない。
- 開く操作の結果は、`ResultOfOpening`（private）の 1 か所で作る。何も開かなかったとき（`CanOpen` が偽、コードで何も開かなかった）も同じメソッドを通るので、「開いたマスがあれば `Opened`、なければ `NoChange`」の規則が 1 か所にある。

### 設計書との違い

ない。

### 引き算の点検

- `MoveResult` に、作るための静的メソッドや「何かが起きたか」のプロパティは足していない（クラス設計書 12.12）。
- 盤面の絵 `FencedPocketPicture` は `GameTests` でしか使わないので、`TestGames` に置かず、テストクラスの中に置いた（複数のテストクラスで使う絵だけを `TestGames` に置く。1.0.0 の区切り 1 の指摘 2）。

### 良い点

- `Board.Open` の変更は、連鎖の待ち行列で開いたマスを一覧に足すだけで、開く規則そのものには触れていない。1.0.0 の `BoardTests` と `GameTests`（111 件）は、書き換えずにそのまま通った。
- 誤った旗のコードで 2 つの地雷を開く場合も、旗で連鎖を止める盤面の絵で、テストの中に見える形で確かめている。

### 検証結果

- `dotnet build Shos.Minesweeper.slnx`: 警告 0、エラー 0
- `dotnet test`: 394 件すべて成功（GameLogic 123、Presentation 35、Web 236）

### 範囲外の気づき

- `GameTests` が 66 件になり、テストクラスとして長くなってきた（長いクラス）。操作の結果のテストを別のクラスに分けるかは、工程 12 のリファクタリングで判断する。

## 1.1.0 区切り 2: 効果音の部品

| 項目 | 内容 |
|------|------|
| レビュー日 | 2026-09-26 |
| 対象 | `Shos.Minesweeper.Presentation` の `SoundEffect`、`SoundEffectMapping`、`SoundEffectSynthesizer`。`Shos.Minesweeper.Presentation.Tests` の `SoundEffectMappingTests`、`SoundEffectSynthesizerTests`。docs/sounds-preview.html |
| 意図（ひとことで） | どの操作の結果でどの効果音を鳴らすかと、効果音の波形を、UI の技術に依存しない C# で作る。Web 版と WPF 版が同じ定義を使う |

### 作ったもの

| 種類 | 内容 |
|------|------|
| Presentation の型 | `SoundEffect`（6 つの効果音）、`SoundEffectMapping.EffectFor`（クラス設計書 12.3 の表）、`SoundEffectSynthesizer`（`SampleRate` = 44100、`Synthesize`） |
| テスト | `SoundEffectMappingTests`（表の 11 行）、`SoundEffectSynthesizerTests`（6 つの効果音ごとに、長さ、最大の振幅、始まりと終わりが 0 に近いこと、同じ波形になること。24 件） |

テストファーストで進めた。テストを先に書き、`SoundEffect` がないことによるコンパイルエラー（Red）を確かめてから実装した。

**合成の中身**: 各効果音を、音の部品（音色、始まりと終わりの周波数、始まりの時刻、長さ、最大振幅）の配列として 1 か所の表（`PartsOf`）に持つ。表の 1 行が UI デザイン 10.6 の 1 つの音に当たる。音量と周波数は、試聴のページの `exponentialRampToValueAtTime` と同じ指数関数で変える。雑音の低域通過は、Web Audio の `BiquadFilterNode` の仕様の式（lowpass。Q はデシベルとして読む）で、境の周波数をサンプルごとに変える。

### 試聴のページとの比べ合わせ

試聴のページの音の定義を、ヘッドレスの Chrome の `OfflineAudioContext`（44100Hz）で波形にし、C# の波形と比べた。耳で聞く代わりに、10 ミリ秒ごとの音量（RMS）と、波形が 0 をまたぐ回数（音程の目安）を比べた。

| 効果音 | 最大の振幅（C# / Web Audio） | 0 をまたぐ回数（C# / Web Audio） | 10 ミリ秒ごとの音量の差（最大） |
|--------|------------------------------|----------------------------------|--------------------------------|
| 開く | 0.153 / 0.151 | 73 / 73 | 0% |
| 連鎖 | 0.141 / 0.138 | 207 / 207 | 4% |
| 旗を立てる | 0.196 / 0.196 | 97 / 97 | 0% |
| 旗を外す | 0.196 / 0.196 | 97 / 97 | 0% |
| 負け | 0.329 / 0.323 | 49 / 52 | 12% |
| 勝ち | 0.158 / 0.154 | 1124 / 1123 | 3% |

- 音程のある 5 つの音は、ほぼ同じ波形になった。三角波の差（C# は角のある三角波、Web Audio は帯域を制限した三角波）は、最大の振幅の 2〜3% の差に表れている。
- 負けの音の差は、雑音の乱数の並びが違うためである。音量の推移（10 ミリ秒ごとの値）はそろっている。
- 耳での聞き比べは、ユーザーの環境で行う。作業用のフォルダーに、C# で合成した 6 つの音の WAV を書き出した。

**合成にかかる時間**: 6 つを合わせて 3.0 ミリ秒（この PC のネイティブの .NET、Release）。WebAssembly の .NET は数十倍遅くなりうるので、ブラウザーでの時間は、合成を呼ぶようになる区切り 4 で計る（アーキテクチャー設計書 16 章）。

### 指摘

| # | 箇条 | 指摘 | 対応 |
|---|------|------|------|
| 1 | Testable | `SoundEffectSynthesizerTests` の振幅のテスト: 弱いアサーション（「0.05 以上、1 以下」で、設計の値（UI デザイン 10.6 の最大振幅 × 0.8）を大きく外れても通る）→ 設計の最大振幅の表でテストする | 「設計の値の半分以上、設計の値以下」を確かめる `LoudestSampleIsAtMostTheDesignedPeak` に書き換えた。全体の音量を一時的に 1.0 にすると 6 件中 5 件が失敗すること（Red）を確かめてから、元に戻した（負けの音は 2 つの部品が同時に最大にならないので、上限まで余裕がある）→ 70 件 Green |
| 2 | 意図を表現 | `SoundEffectSynthesizer.PartsOf` の連鎖の行: 表の中の手続き（連鎖だけが `Select` で 3 音を作っていて、勝ちの 4 音の書き方と違う。UI デザイン 10.6 の表と行ごとに読み比べにくい）→ 1 音ずつ書く | 3 音を勝ちと同じ形で書いた → 70 件 Green。比べ合わせの結果も変わらない |
| 3 | ルールの統一（設計との整合） | docs/sounds-preview.html の雑音: 設計との食い違い（UI デザイン 10.6 は「どの音も 4 ミリ秒で立ち上げる」だが、試聴のページの雑音だけ立ち上げなしで始まっていた。C# は設計に合わせて立ち上げている）→ 試聴のページを設計に合わせる | 雑音にも 4 ミリ秒の立ち上げを加えた。負けの音の最大の振幅が 0.264 / 0.329 から 0.323 / 0.329 に近づいた |

実装の途中で決めたこと:

- 音源（位相、フィルターの状態、乱数）は、時刻を渡すと値を返す関数（`Func<double, double>`）にし、状態は関数の中に閉じ込めた。音色ごとのクラスを作るより短く、音色の違いが `SourceOf` の 1 か所に集まる。
- 周波数は、UI デザイン 10.6 の丸めた値（523Hz など）ではなく、試聴のページと同じ音名の周波数（C5 = 523.25Hz など）にし、名前の付いた定数（`C5`、`E5`、`G5`、`C6`）で書いた。
- 波形の長さは、秒 × 44100 を四捨五入する。切り上げにすると、0.035 × 2 + 0.08 の計算の誤差で 1 サンプル長くなる。

### 設計書との違い

- クラス設計書 12.3 の「Q は Web Audio の既定の 1」に、「Web Audio の仕様どおりデシベルとして読む」を書き足した（実装で確かめた点）。

### 引き算の点検

- 音色ごとのクラスや、部品のインターフェイスは作っていない。部品は `private` の record 1 つで、音色は `private` の enum である。
- 合成した波形のキャッシュは作っていない（クラス設計書 12.12）。

### 良い点

- UI デザイン 10.6 の表と、`PartsOf` の表が 1 行ずつ対応していて、音を調整するときに直す場所が 1 か所に決まる。
- `SoundEffectMapping` は、`(Status, Outcome)` の組の `switch` 1 つで、仕様書 5.6 の「勝敗が決まったら勝ちか負けの音だけ」が上の 2 行に見える。

### 検証結果

- `dotnet build Shos.Minesweeper.slnx`: 警告 0、エラー 0
- `dotnet test`: 429 件すべて成功（GameLogic 123、Presentation 70、Web 236）

## 1.1.0 区切り 3: 1 回のゲームの進め方

| 項目 | 内容 |
|------|------|
| レビュー日 | 2026-09-26 |
| 対象 | `Shos.Minesweeper.Presentation` の `SoundEffectOutput`、`GameSession`。`Shos.Minesweeper.TestSupport` の `TestGames`。`Shos.Minesweeper.Presentation.Tests`（`GameSessionTests`、プロジェクトの参照）。`Pages/GamePage.razor` |
| 意図（ひとことで） | 盤面の操作、直前の操作、効果音を音の出口に渡すことを、どの版でも使う 1 つのクラスにまとめる。版によって違うのは、出口を渡すかどうかだけにする |

### 作ったもの

| 種類 | 内容 |
|------|------|
| Presentation の型 | `SoundEffectOutput`（音の出口の delegate）、`GameSession`（`Game`、`LastMove`、`StartNewGame`、`Open`、`ToggleFlag`） |
| テストの補助 | `TestGames.DifficultyOf`・`MineChooserOf`（盤面の絵から難易度と地雷の選び方を作る）。`FromPicture` はこの 2 つを使う形にした |
| テスト | `GameSessionTests` 11 件（新しいセッション、結果を返す、操作ごとの効果音、勝ちと負けは 1 つの音だけ、直前の操作、何も起きない操作、新しいゲーム、同じ地雷の選び方、出口なし、勝敗の後の操作）。`Presentation.Tests` から `TestSupport` を参照した（クラス設計書 12.11 の A7） |
| 変えたもの | `GamePage` が `Game` の代わりに `GameSession` を持ち、盤面の操作を `GameSession` を通して行う。勝敗の判断は、`Open` の戻り値の `Status` で行う。音の出口はまだ渡さない（区切り 4 で渡す） |

進めた順:

1. 準備的リファクタリング: `TestGames` に `DifficultyOf`・`MineChooserOf` を加え、`FromPicture` をこの 2 つで書き直した。振る舞いは変えていない → GameLogic の 123 件 Green。
2. `GameSessionTests` を書き、`GameSession` がないことによるコンパイルエラー（Red）を確かめてから、`SoundEffectOutput` と `GameSession` を作った → Presentation の 81 件 Green。
3. `GamePage` を `GameSession` に置き換えた。既存の `GamePageTests` などを書き換えずに通ることで、振る舞いが変わらないことを確かめた → 440 件 Green。

### 指摘

| # | 箇条 | 指摘 | 対応 |
|---|------|------|------|
| 1 | 的確な名前 | `GameSessionTests.SessionWithoutOutputPlaysTheSameGameSilently`: 名前が確かめていることより多くを言っている（「Silently（鳴らない）」は、出口がないので観測できず、アサーションもない。確かめているのは、出口がなくても例外なく進み、直前の操作が残ること）→ 名前を確かめていることに合わせる | `SessionWithoutOutputPlaysWithoutErrors` に改めた → 81 件 Green |

実装の途中で決めたこと:

- 何も起きなかった操作の扱い（直前の操作を置き換えず、音も鳴らさない）と、操作の後の扱い（覚える、鳴らす）は、`ReactTo` の 1 か所に置き、`Open` と `ToggleFlag` はどちらも `ReactTo(Game.Xxx(position))` の 1 行にした。2 つの操作で扱いが同じであることが、形で分かる。
- コンストラクターは `StartNewGame` を呼んで最初のゲームを作る。`Game` が null でないことをコンパイラーに伝えるため、`StartNewGame` に `[MemberNotNull(nameof(Game))]` を付けた。ゲームを作る処理が 1 か所になる。
- 「UI は `Game` の `Open` と `ToggleFlag` を直接呼ばない」（クラス設計書 12.4）を、`GameSession` の説明のコメントに書いた。

### 設計書との違い

ない。

### 引き算の点検

- 音の出口は delegate 1 つで、インターフェイスや、音を鳴らせるかを表す値は作っていない。
- `GameSession` に、ベストタイム、読み上げの文、効果音のオンとオフは入れていない（アーキテクチャー設計書 6.2）。

### 良い点

- 版によって違うのが「出口を渡すかどうか」だけであることが、テストで見える。`SessionOf` は出口（`played.Add`）を渡し、`SessionWithoutOutputPlaysWithoutErrors` は渡さずに、同じ操作を同じ結果で進める。
- `GamePage` の置き換えは、`game` を `session.Game` に変え、盤面の操作を `session` に向けただけで、1.0.0 のコンポーネントのテストはそのまま通った。

### 検証結果

- `dotnet build Shos.Minesweeper.slnx`: 警告 0、エラー 0
- `dotnet test`: 440 件すべて成功（GameLogic 123、Presentation 81、Web 236）

## 1.1.0 区切り 4: ブラウザーで鳴らす

| 項目 | 内容 |
|------|------|
| レビュー日 | 2026-09-26 |
| 対象 | `wwwroot/js/browser.js`（効果音）、`Browser/BrowserFeatures.cs`、`Browser/SoundEffectPlayer.cs`、`Browser/SoundSettingStorage.cs`、`Display/IconKind.cs`、`Components/Icon.razor`・`Icon.razor.css`・`SpeakerShape.razor`、`Components/Toolbar.razor`・`Toolbar.razor.css`、`Pages/GamePage.razor`、`Program.cs`。テスト（`SoundEffectPlayerTests`、`SoundSettingStorageTests`、`ToolbarTests`、`GamePageTests`、`AppTestContext`） |
| 意図（ひとことで） | 盤面の操作の効果音がブラウザーで鳴り、ツールバーのボタンで消せて、その設定が残るようにする |

### 作ったもの

| 種類 | 内容 |
|------|------|
| JavaScript | `loadSound`（波形を複写して `Float32Array` で覚える）、`playSound`（`AudioBufferSourceNode` を作って鳴らす）、利用者の操作と見なされるイベント（`keydown`、マウスの `pointerdown`、タッチとペンの `pointerup`、`touchend`）を `document` で捕捉の段階に受けて、`AudioContext` を作る・動かす処理（アーキテクチャー設計書 9.4） |
| C# | `BrowserFeatures.LoadSoundAsync`・`PlaySoundAsync`、`SoundEffectPlayer`（`IsEnabled`、`PrepareAsync`、`Play`）、`SoundSettingStorage`（`"on"`・`"off"`） |
| 画面 | ツールバーの最後に効果音 ボタン（`aria-pressed`、ツールチップ、`SoundOn`・`SoundOff` のアイコン）。スピーカーの形は、旗や地雷と同じく小さな部品（`SpeakerShape`）にした |
| つなぎ | `GamePage` が `SoundEffectPlayer.Play` を音の出口として `GameSession` に渡す。ページを開いたときに設定を読み、最初の描画の後に効果音を用意し、ボタンで切り替えて保存する。`Program.cs` に 2 つを登録した |
| テスト | 20 件（`SoundSettingStorageTests` 7、`SoundEffectPlayerTests` 6、`ToolbarTests` 1、`GamePageTests` 6）。既存の `ToolbarTests` の 2 件（押したことを伝える、Tab の順）に効果音 ボタンを加えた |

テストファーストで進めた。テストを先に書き、`SoundEffectPlayer` がないことによるコンパイルエラー（Red）を確かめてから実装した。

### ブラウザーで確かめたこと

公開用にビルドしたものを、GitHub Pages と同じサブパス（`/Shos.Minesweeper/`）で配り、ヘッドレスの Chrome で確かめた。鳴ったかどうかは、ページの中で `createBufferSource` の呼び出しを数えて見た（Chrome の WebAudio の観測は、この部品の作成を知らせなかった）。

| 確かめたこと | 結果 |
|--------------|------|
| 操作の前 | `AudioContext` は作られていない |
| 最初の操作（マウスで開く） | `AudioContext` が作られて動き（suspended → running）、6 つの効果音の `AudioBuffer` が C# の波形と同じ長さで作られ、音が 1 回鳴った |
| 旗を立てる | 2 回目の音が鳴った |
| 効果音 ボタンでオフにして旗を立てる | 鳴らない。ボタンは `aria-pressed="false"`・「効果音（オフ）」、localStorage は `"off"` |
| ページを開き直す | ボタンはオフのまま、アイコンは `SoundOff` |
| オンに戻す | 鳴らない（UI デザイン 10.8 の決定 10）。localStorage は `"on"` |
| コンソール | エラーも警告もない |
| ツールバーの幅（320×640、360×740、390×844、640×320） | ボタンを加えても、はみ出す要素はない。寸法の調整は区切り 5 で行う |

**WebAssembly での合成の時間**: 各効果音が JavaScript に届いた時刻の間隔から求めた。6 つを合わせて 20〜30 ミリ秒（3 回とも同じ程度。1 つあたり 0〜12 ミリ秒）で、この PC のネイティブの .NET（3 ミリ秒）の 7〜10 倍だった。心配した数百ミリ秒にはならなかった（アーキテクチャー設計書 16 章に結果を書いた）。スマートフォンでの時間は工程 13 で確かめる。

### 指摘

| # | 箇条 | 指摘 | 対応 |
|---|------|------|------|
| 1 | 意図を表現 | `GamePage.OnAfterRenderAsync` のコメント: 事実と違うコメント（「最初の表示を遅らせないように、描いた後に行う」とあるが、盤面は最初の描画の後に領域の大きさが分かってから描くので、合成は盤面の最初の表示の前に走り、その分（PC で 20〜30 ミリ秒）盤面の表示を遅らせる。上の計測で分かった）→ コメントを事実に合わせ、用意する時期は区切り 6 で見直す | コメントに、盤面の最初の表示が合成の分遅れうることと、区切り 6 で見直すことを書いた。区切り 6 で、`GamePage` が盤面の領域の大きさを持つようになったら、盤面を初めて描いた後に用意する形を検討する → 460 件 Green |

### 設計書との違い・補った決定

- アーキテクチャー設計書 16 章のリスクのうち、合成の時間、`Float32Array` の読み方、試聴のページとの違いの 3 つに、確かめた結果を書いた。

### 引き算の点検

- 音を鳴らせるかを表す値や、Web Audio がないブラウザーのための別の仕組みは作っていない。Web Audio がなければ、`browser.js` が何もしないだけである。
- `navigator.audioSession` は設定していない（iOS の消音スイッチに従う。仕様書 5.6）。

### 良い点

- `GamePage` が音について書いたのは、出口を渡すこと、設定を読むこと、用意すること、切り替えることの 4 か所だけで、どの操作でどの音を鳴らすかは `GameSession` と `SoundEffectMapping` に任せている。
- 鳴らすときの失敗は、すべて `browser.js` の中で受け止め、C# には何も返さない（アーキテクチャー設計書 9.1）。

### 検証結果

- `dotnet build Shos.Minesweeper.slnx`: 警告 0、エラー 0
- `dotnet test`: 460 件すべて成功（GameLogic 123、Presentation 81、Web 256）
- ブラウザー（ヘッドレスの Chrome）: 上の表のとおり

### 作業環境で起きたこと

- Web アプリのビルドが、既知のロック（MSB4018。`obj/Debug/net10.0/tmp-webcil` を消せない）で 4 回続けて失敗した。`tmp-webcil` には、前のビルドの一時フォルダーが多く残っていた。このフォルダーを消してからビルドすると通った。公開用のビルドは、前回と同じく、ソースを Dropbox の外に写して行った。
