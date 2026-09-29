# T7 の回答: `CellView` の見た目の切り替えのリファクタリング

判断の基準: スキル C（sustainable-code-jp）。作業の種類は「リファクタリング」なので、references/refactoring.md を全部読んだ。安全網のテストがないため、「必要なら読む」の testing.md の「テストとリファクタリング」「既存コードにテストを後から足す」「テスト基盤がない場合」も読んだ。

## 直したコード

```csharp
public enum CellAppearance { Unopened, Opened, Flagged, Mine, Exploded, WrongFlag }

public sealed class CellView : UserControl
{
    // マスの見た目のクラスの全部。対応は ClassOf の 1 か所にだけ書き、一覧はそこから作る
    static readonly string[] AppearanceClasses =
        Enum.GetValues<CellAppearance>().Select(ClassOf).ToArray();

    static string ClassOf(CellAppearance appearance) => appearance switch {
        CellAppearance.Unopened  => "unopened",
        CellAppearance.Opened    => "opened",
        CellAppearance.Flagged   => "flagged",
        CellAppearance.Mine      => "mine",
        CellAppearance.Exploded  => "exploded",
        CellAppearance.WrongFlag => "wrong-flag",
        _ => throw new ArgumentOutOfRangeException(nameof(appearance))
    };

    public void ShowAppearance(CellAppearance appearance)
    {
        Classes.RemoveAll(AppearanceClasses);
        Classes.Add(ClassOf(appearance));

        var icon = IconOf(appearance);
        Icon.Content = icon;
        Icon.IsVisible = icon is not null;
    }

    static object? IconOf(CellAppearance appearance) => /* 略 */ null;
}
```

（`Select` のために `System.Linq` が要る。`ImplicitUsings` が有効なら追加の `using` は要らない。）

## What と Why（臭いと技法の名前で）

1. `AppearanceClasses` と `ClassOf`: 重複したコード（「見た目 → クラス名」の同じ対応が、配列の一覧と switch 式の二か所に書かれている。見た目を 1 つ足したり、クラス名を 1 つ変えたりするとき、二か所を揃えて直さなければならず、配列の側を直し忘れると、古いクラスが `RemoveAll` で消されずに残る。変更の分散でもある）→ 一方を他方から導く（一覧を対応の式から作る）。`Enum.GetValues<CellAppearance>().Select(ClassOf)` にし、対応の所在を `ClassOf` の 1 か所に決めた。
   - 同じ意図かどうか（判断ルール 6）: どちらも「見た目ごとのスタイルのクラス名」で、配列は「付けうるクラスの全部」、switch はその中から 1 つを選ぶもの。偶然似ているのではなく同じ対応なので、まとめてよい。
   - 導く向きに switch を残した理由: 一覧からは対応（どの見た目がどの名前か）を作れないが、対応からは一覧を作れる。また switch 式は、見た目を足したのに腕を書き忘れると、コンパイラーの警告（CS8524 を有効にしていれば）や `_` の腕の例外で気づける。
2. `ShowAppearance` の `IconOf(appearance)` の二度の呼び出し: 重複したコード（同じ値を二回求めている。表示するアイコンと、表示するかどうかの判断の元が、別々の呼び出しになっている）→ 説明用の変数の導入（`icon`）。「このアイコンを出し、アイコンがなければ隠す」が 1 つの値から読めるようになった。また、仮に `IconOf` が呼ぶたびに新しいオブジェクトを作るものでも、作るのが 1 回で済む。
3. `ShowAppearance` の中の空行: 「クラスを付け替える」「アイコンを出す」の二つのまとまりを空行で分けた。メソッド抽出（`ReplaceAppearanceClass` と `ShowIcon`）も考えたが、それぞれ 2〜3 行で、名前を付けても読む量が減らないので見送った。

## 振る舞いが変わらないことの根拠

- `AppearanceClasses` の中身: `Enum.GetValues<T>()` は定義された値を値の順（0〜5）に返すので、配列は `{ "unopened", "opened", "flagged", "mine", "exploded", "wrong-flag" }` で、元の配列と要素も順も同じ。`RemoveAll` に渡すだけなので、順は結果に関わらないが、順も揃っている。
- 静的フィールドの初期化で `ClassOf` を呼ぶ: `ClassOf` はほかの静的フィールドに依存しない静的メソッドなので、初期化の順の問題はない。定義された値だけを渡すので、`_` の腕の例外は起きない。
- `ClassOf` の対応と、定義にない値（`(CellAppearance)99` など）で `ArgumentOutOfRangeException` を投げることは、そのまま残した。
- クラスの付け替えの順（全部を消してから 1 つ足す）、`Icon.Content` と `Icon.IsVisible` を設定する順は、元のとおり。

## 置いた仮定

- `IconOf` は、同じ見た目に対して毎回同じ結果（少なくとも、null かどうかが同じ）を返し、副作用がない。本文が略されているので確かめられない。これが成り立たない場合（呼ぶたびに結果が変わる、呼んだ回数に意味がある）は、2 の変更で振る舞いが変わる。
- `Classes` は Avalonia の `Classes`（`RemoveAll(IEnumerable<string>)` と `Add(string)` を持つ）で、`Icon` は `ContentControl` などの `Content` と `IsVisible` を持つコントロールである。
- 「振る舞い」には、見た目を足したときに一覧だけを直し忘れる、といった将来の誤りの起こり方は含めない。なお、見た目を足して `ClassOf` の腕を書き忘れると、変更後は型の初期化の時点（最初に `CellView` を使うとき）で `TypeInitializationException` になる。変更前は、その見た目を表示するときに `ArgumentOutOfRangeException` になり、また一覧からも漏れていた。現在の 6 つの値ではどちらも起きない。

## 作らなかったもの

- クラス名を列挙子の名前から作る（`WrongFlag` → `wrong-flag` の変換）。名前の付け替えがスタイルの指定を黙って壊すようになり、対応が目で読めなくなるので入れなかった。
- `Dictionary<CellAppearance, string>` への置き換え。一覧は `.Values` で取れるが、定義にない値の例外が `KeyNotFoundException` に変わり、振る舞いが変わる。switch 式で足りる。
- 見た目ごとのクラス（ポリモーフィズム）への置き換え。分岐は `ClassOf` と `IconOf` の二か所にあるが、種類は盤面のマスの状態で安定しており、分岐を型に移しても読む量が増えるだけなので見送った（スイッチ文の臭いは、分岐が増殖して変更のたびに複数箇所を直す構造のときに問題になる）。
- 例外に実際の値を載せる（`new ArgumentOutOfRangeException(nameof(appearance), appearance, null)`）。例外のメッセージが変わるので、リファクタリングには含めなかった（下の「ユーザーの判断が要る点」）。

## 検証結果

- 安全網: 課題にはコードの断片だけがあり、テストもテスト基盤もない。スキルに従い、テストフレームワークは導入していない。構造を大きく作り変えず、一手ごとに小さく、元に戻せる変更（一覧を式から導く、変数の導入）だけにとどめた。
- 実行による確認: `ClassOf` と `AppearanceClasses` の部分（Avalonia に依存しない部分）を、一時的なファイルベースの C# プログラム（`dotnet run check.cs`）で、元の配列と要素・順が同じか、定義にない値で `ArgumentOutOfRangeException` になるかを確かめようとした。しかし、このサンドボックスでは `/tmp` が読み取り専用で、.NET の SDK が `/tmp/.dotnet` に名前付きのミューテックスを作れず（`mkdtemp ... EROFS`）、ビルドの前に失敗した。したがって、**コンパイルも実行もしていない**。上の「振る舞いが変わらないことの根拠」は、コードを読んで確かめたものである。一時的なファイルは消した。
- 確かめられていないこと: 実際の `CellView`（Avalonia）でのビルドと表示、`IconOf` の本文。

## 実際のプロジェクトで入れるなら押さえたいテスト（提案。まだ書いていない）

テストを書いてよければ、次の表を、契約（公開された `ShowAppearance`）に張って先に書き、Green を確かめてから上の変更を入れる。

| 入力の状況 | 期待 |
|---|---|
| 各見た目（6 つ）を表示する | `Classes` に、その見た目のクラスがちょうど 1 つだけある |
| `Flagged` を表示した後に `Opened` を表示する | `flagged` が消え、`opened` だけが残る |
| 見た目のクラス以外のクラス（例: `pressed`）が付いている状態で表示する | 見た目以外のクラスは残る |
| アイコンのある見た目 / ない見た目 | `Icon.IsVisible` が true / false で、`Icon.Content` が `IconOf` の結果 |
| 定義にない値 | `ArgumentOutOfRangeException` |

## 範囲外の気づき

- `ClassOf` と `IconOf`: 同じ `CellAppearance` による分岐が二か所にある（スイッチ文の芽）。三か所目が現れたら、見た目ごとの「クラス名とアイコン」を 1 つの記録（例: `record AppearanceStyle(string ClassName, object? Icon)`）にまとめる案がある。今は二か所なので見送った。

## ユーザーの判断が要る点

- `IconOf` が純粋な関数であるという仮定が正しいか（違えば、2 の変更は振る舞いを変える）。
- 例外に実際の値を載せる変更（メッセージが変わる）を、リファクタリングとは別の手順で入れるか。
- テストを足してよいか、足すならどのテストフレームワークを使うか。
