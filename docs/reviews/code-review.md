# コードレビュー

実装（工程 11）の区切りごとのコードレビューを、ここに追記する。区切りはクラス設計書（docs/05-class-design.md）の 8 章のとおりである。

- レビューは Claude が行い、ユーザーが承認する。ユーザーからの指摘も、同じ区切りの節に記録する。
- 観点は sustainable-code-jp スキルの七箇条と Think Simple（引き算の設計）である。
- 指摘は、スキルの書式（`<場所>: <臭いの名前>（<症状の根拠>）→ <技法> → <結果と検証>`）で書き、該当する七箇条の箇条名を添える。

| 区切り | 内容 | 状態 |
|--------|------|------|
| 1 | テストの土台とゲームのルール | 指摘をすべて反映済み。ユーザーの承認待ち |
| 2 | 盤面の表示 | 未着手 |
| 3 | マウスとタッチの操作 | 未着手 |
| 4 | ツールバー | 未着手 |
| 5 | キーボードと読み上げ | 未着手 |
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
