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
| 5 | キーボードと読み上げ | 指摘をすべて反映済み。ユーザーの承認待ち |
| 6 | 難易度とベストタイム | 未着手 |
| 7 | ページ全体の仕上げ | 未着手 |

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

報告の時に、スマートフォンの実機での長押しの確認を「今すぐ行うか、工程 12 でまとめて行うか」を尋ねた。ユーザーは個別の回答をせずに次の区切りへ進めたので、工程 12（結合テスト）でまとめて行う。

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
- まだ確かめていないこと: スクリーンリーダー（1 種類。仕様書 6.3）での実際の読み上げ。工程 12 で、ユーザーの環境で確かめる。
