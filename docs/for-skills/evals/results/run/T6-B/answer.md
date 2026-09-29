# T6 の報告: テストファーストで 4 つの型を作る

## 適用したスキルの範囲
作業の種類は「新機能の実装」（新しい振る舞いを決める。負の数の扱いなど、仕様の解釈が要る）。次を読んで進めた。
- 必ず読む: collaboration.md の「着手前」、testing.md の「テストファースト」「TDD の進め方」「テストケースの設計」
- 必要なら読む: quality-gates.md の「契約とガード節」（範囲の検証を書くため）、modeling.md の「良いモデルの条件」「Why→What→How」、object-design.md の「責務の割り当て」〜「変更理由を一つにする」「Expert」（新しいクラスを作るため）

## 着手前（What の言い直し）
- 作るもの: マスの位置と周りのマス（`CellPosition`）、盤面の大きさと地雷の数の妥当性（`BoardSize`）、残りの地雷の数の計算（`FlagCounter`）、経過時間の 3 桁の表示（`ElapsedTime`）
- 作らないもの: 盤面そのもの、旗の状態の管理、タイマー（時刻の取得）、表示の UI
- 境界: 4 つの型は互いに依存しない。`neighbors` は盤面の大きさを `BoardSize` ではなく、仕様どおり `(columns, rows)` の 2 つの整数で受け取る
- 考慮する例外: 負の列・行、範囲外の盤面、地雷の上限、旗が地雷を超えた場合、999 秒を超えた場合、負の秒

## What（作ったもの）
| ファイル | 中身 |
|----------|------|
| `cell_position.py` | `@dataclass(frozen=True) CellPosition(column, row)`。等しさとハッシュは dataclass が列と行から作る。負の値は `ValueError`。`neighbors(columns, rows)` は周りの 8 つのずれのうち、盤面の中にあるものを `CellPosition` のリストで返す（上の行から、左から右の順） |
| `board_size.py` | `@dataclass(frozen=True) BoardSize(columns, rows, mines)`。`__post_init__` のガード節で、列・行が 5〜30、地雷が 1〜(列×行−1) であることを確かめ、外れたら `ValueError` |
| `flag_counter.py` | `FlagCounter(mines).remaining_mines(flags)` が `mines - flags` を返す。0 で止めないので、旗が多いと負になる |
| `elapsed_time.py` | `ElapsedTime(seconds).display_text()` が `min(seconds, 999)` を 3 桁のゼロ埋めの文字列にする。負の秒は `ValueError` |
| `test_*.py` | 上の 4 つに対応するテスト（計 38 件） |

進め方は Red → Green → Refactor を 1 件か数件ずつ回した（`./run_tests.sh` を 25 回）。例:
- 型のない状態で import の失敗（Red）を見てから型を作った
- `FlagCounter` は、まず `return self._mines` の仮実装（Fake It）で Green にし、「3 本で 7」「10 本で 0」「12 本で −2」で三角測量して引き算にした
- `neighbors` は、まず内側のマスの 8 件だけを満たす実装にし、角と辺のテストで Red にしてから盤面の中への絞り込みを入れた
- `BoardSize` の地雷の上限は、正方形だと「列×列」の誤りを見逃すので、5×6 の盤面（30 マス）で 30 は拒否・29 は受理、を境界の両側で置いた

Refactor で行ったこと（振る舞いは変えず、毎回 Green を確認）:
- `neighbors` の周りの 8 つのずれを、名前の付いたモジュール定数 `_NEIGHBOR_OFFSETS` と `_surrounding_coordinates` に分けた
- `BoardSize` の列と行の範囲を `MIN_SIDE` / `MAX_SIDE` にまとめた（同じ意図の数値の重複）
- `BoardSize` にいったん足した公開のプロパティ `cell_count` を、頼まれていない公開 API なので消し、`__post_init__` の中の式に戻した
- 2 つの境界（5 と 30）を 1 つのテストで確かめていた箇所を、1 テスト 1 関心事になるように分けた

## Why（設計の理由と捨てた案）
- `CellPosition` と `BoardSize` を frozen の dataclass にした: 「等しさは列と行で決まる」を手書きの `__eq__` / `__hash__` なしで表せ、集合や辞書のキーにも使える。値オブジェクトなので不変にした
- 検証はコンストラクタ（`__post_init__`）の入口のガード節に置いた: 不変条件を生成時に成立させ、以後のメソッドを異常系のない一本道にするため。例外は Python 標準の `ValueError`
- `FlagCounter` は状態を持たず、旗の数を引数で受け取る形にした。捨てた案は `add_flag()` / `remove_flag()` で旗の数を内部に持つ形。旗の状態の持ち主は盤面の方であり（Expert）、ここで二重に数えると食い違いの元になるうえ、仕様は「引いた数を返す」だけなので、単純な方を選んだ
- `ElapsedTime` は値を受け取って表示の文字列を返すだけにし、時刻の取得（タイマー）は持たせなかった。これにより時刻の差し替えなしでテストできる
- 各テスト名は「入力の状況と期待される結果」の文にした（例: `test_1000_seconds_stops_at_999`、`test_more_flags_than_mines_become_negative`）

## 作らなかったもの・置いた仮定
- 仮定 1: `CellPosition` の「0 以上」は契約とみなし、負の値は `ValueError` にした（仕様に扱いの記述がないため。`BoardSize` と揃えた）
- 仮定 2: `neighbors` の戻り値はリスト。順序は仕様にないので、テストは集合として比べ、順序に依存させていない。自分自身が盤面の外にある場合の検証はしない（盤面の中のものだけを返す、という仕様のまま）
- 仮定 3: `ElapsedTime` は負の秒を `ValueError` にした。秒は整数を前提にし、小数の秒の切り捨ては入れていない（小数を渡すと `:03d` の書式で `ValueError` になる）
- 仮定 4: `FlagCounter` は、旗の数・地雷の数の検証をしない（地雷の数の妥当性は `BoardSize` の責務。負の旗の数は呼び出し側の誤りとして扱わない）
- 仮定 5: メソッド名は `remaining_mines`、`display_text` とした（仕様に名前がないため）
- 作らなかったもの: 型の検査（`int` 以外や `bool` を拒否する）、`BoardSize` の既定の難易度（初級・中級・上級）、`neighbors` に `BoardSize` を渡せる版。どれも問題文にない

## 検証結果
- `./run_tests.sh` で全テストを実行: `Ran 38 tests ... OK`（終了コード 0）。`test-log.txt` の最後の記録（run 25）も OK
- 途中の Red はすべてログに残っている（import の失敗、仮実装の失敗、範囲の検証がない状態での失敗）
- 最初から Green だったテスト: `CellPosition` の等しさの 4 件（dataclass が満たすため）、`neighbors` の重複なしの 1 件、`ElapsedTime` の 0 秒・42 秒・999 秒、`BoardSize` の境界の内側の受理のテスト。いずれも仕様を守り続ける番人として残した
- 途中で 2 回、サンドボックスの誤り（bwrap が作業フォルダーの外のファイルを作れない）でコマンドが実行されなかった。ファイルの内容を確かめ、同じ手順を Edit/Write のツールと絶対パスの実行でやり直したので、成果物への影響はない（この 2 回は `test-log.txt` に記録されていない）
- 静的解析のツールは使っていない（外部のパッケージを使わない条件のため）

## 範囲外の気づき
- なし（作業フォルダーは白紙から作ったため）

## ユーザーの判断が要る点
- 負の列・行・秒を `ValueError` にするか（仮定 1、3）
- `FlagCounter` を旗の数を内部に持つ形にするか（今は引数で受け取る純粋な計算）
- `ElapsedTime` に小数の秒を渡すことがあるか（あるなら切り捨ての仕様を決める）
