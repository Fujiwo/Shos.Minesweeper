# T6 の報告: テストファーストで 4 つの型を作る

## 読んだスキルの範囲
作業は「新機能の実装」（新しい振る舞いを決める作業）とした。必ず読むものとして collaboration.md の「着手前」と、testing.md の「テストファースト」「TDD の進め方」「テストケースの設計」を読んだ。必要に応じて読むものとしては、新しいクラスを作るので object-design.md の「責務の割り当て」〜「Expert」と modeling.md の「良いモデルの条件」を、引数を検証するので quality-gates.md の「契約とガード節」を読んだ。

## 着手前の言い直し（What）
- 作るもの: マスの位置と周りのマス（`CellPosition`）、正しい盤面の大きさだけを表す値（`BoardSize`）、残りの地雷の数の表示に使う数（`FlagCounter`）、経過時間の表示に使う文字列（`ElapsedTime`）。
- 作らないもの: 盤面そのもの、地雷の配置、タイマー（時計を読む部分）、画面への表示。
- 境界: 4 つの型はどれも他の型に依存しない。`neighbors` は `BoardSize` ではなく、仕様どおり列の数と行の数を受け取る。
- 考慮する例外: 範囲の外の値、盤面の端と角、列と行が違う盤面、旗が地雷より多いとき、999 を超える秒。

## What（作ったもの）
| ファイル | 内容 |
|---|---|
| `cell_position.py` | `@dataclass(frozen=True)` で作った。等しさとハッシュは列と行で決まる。列か行が負なら `ValueError` になる。`neighbors(columns, rows)` は、盤面の中にある周りのマスを行優先の順に `list` で返す |
| `board_size.py` | `@dataclass(frozen=True)`。`__post_init__` のガード節で、列と行が 5〜30、地雷が 1〜(列×行−1) であることを確かめ、外れたら `ValueError` にする。概念に名前を付けるために `cells` と `max_mines` を置いた |
| `flag_counter.py` | `FlagCounter(mines)`。`place_flag()`、`remove_flag()` と、`remaining_mines`（地雷の数 − 旗の数。負にもなる）を持つ。旗が 1 本もないときの `remove_flag()` は `ValueError` になる |
| `elapsed_time.py` | `ElapsedTime(seconds).text` は `min(seconds, 999)` を `03d` の形にした文字列を返す。負の秒は `ValueError` になる |
| `test_*.py` | 4 つのファイルに合わせて 40 件のテストを書いた（CellPosition 15、BoardSize 11、FlagCounter 6、ElapsedTime 8） |

## 進め方（テストファースト）
型ごとに、テストを先に書いて Red を確かめ、最小限の実装で Green にし、次のテストで一般化した。流したのはすべて `./run_tests.sh` で、`test-log.txt` には run 1〜18 が残っている。Red（exit 1）と Green（exit 0）が交互に並ぶ。
- CellPosition: 値を持つこと → 等しさと負の値のガード → `neighbors`（中央・角・辺と、列と行を取り違えると失敗する長方形の盤面）の順に進めた。
- BoardSize: 値を持つこと → 範囲の境界（4/5、30/31、地雷 0/1、マス数−1/マス数、5×6 の盤面）の順に進めた。
- FlagCounter と ElapsedTime: 最初は Fake It（定数を返す）で Green にし、次のテストで三角測量して一般化した。

## Why（設計の理由）
- `CellPosition` と `BoardSize` は値なので、frozen な dataclass にした。仕様の「等しさは列と行で決まる」がそのまま満たされ、ハッシュも使える（集合や辞書のキーになる）。
- `BoardSize` は、不変条件をコンストラクタで成り立たせる形にした。作れた時点で正しい大きさであることが保証されるので、使う側は検証しなくてよい。
- `FlagCounter` は、名前の「カウンター」と、旗を 1 本ずつ立てたり外したりするゲームの操作に合わせて、状態を持つ形にした。捨てた案は、旗の数を引数に取る関数 `remaining(flags)` である。こちらのほうが単純だが、旗の数を誰が数えるかが型の外に出てしまう。
- `ElapsedTime` の 999 は、`MAX_SHOWN_SECONDS` という名前の定数にした。

## 作らなかったもの・置いた仮定
- 型の検査（整数でない値や bool を拒むこと）は入れなかった。仕様が求めているのは範囲だけである。
- `neighbors` は、自分自身が盤面の中にあるかを確かめない。盤面の外の位置で呼ばれても、盤面の中の周りのマスを返す。
- `neighbors` の戻り値は `list` とした。順序は仕様にないので、テストは集合で比べている（順序には依存しない）。
- 仮定 1: `CellPosition` の「0 以上」は前提条件として扱い、負の値は `ValueError` とした。
- 仮定 2: `FlagCounter` で、立てていない旗を外すのは契約違反とし、`ValueError` とした。
- 仮定 3: `FlagCounter` の地雷の数は検証しない。範囲は `BoardSize` が保証するものとした。
- 仮定 4: `ElapsedTime` は整数の秒を受け取る（小数の秒の扱いは決めていない）。負の秒は `ValueError` とした。表示は `text` プロパティで返す。
- 抽象（基底クラスやインターフェイス）は足していない。

## 検証結果
- 最後に `./run_tests.sh` を流し、40 件がすべて通った（`Ran 40 tests ... OK`。test-log.txt の run 18、exit 0）。Python は 3.12.3 である。
- 途中で 2 回、1 つのコマンドにファイルの書き込みとテストの実行をまとめたときに、サンドボックスが `bwrap: Can't create file ... Permission denied` で止まった。作業フォルダーの外に書こうとしたものではなく、コマンドそのものが動かなかった。それ以降は、ファイルは Write ツールで書き、テストの実行だけを Bash で行った。止まったコマンドは test-log.txt に何も残していない。

## 範囲外の気づき
- 盤面の外の位置で `neighbors` を呼べてしまう。盤面を作るときに、盤面が `BoardSize` を受け取って位置を検証するのがよいと考える（Expert）。今回は作っていない。
- `board_size.py` の `MIN_LENGTH` などは、モジュールの公開の定数である。外から使わないのであれば、`_` を付けて隠す案もある。

## ユーザーの判断が要る点
- `FlagCounter` の形（状態を持つか、旗の数を引数に取る関数にするか）と、旗がないときの `remove_flag()` を例外にするかどうか。
- `ElapsedTime` に小数の秒を渡すことを許すか（許すなら、切り捨てるのか四捨五入するのか）。
- `neighbors` の戻り値の順序を仕様として決めるか。
