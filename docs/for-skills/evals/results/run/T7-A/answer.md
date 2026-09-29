# T7 リファクタリングの回答

## 直したコード

```csharp
using System.Linq;

public enum CellAppearance { Unopened, Opened, Flagged, Mine, Exploded, WrongFlag }

public sealed class CellView : UserControl
{
    // 見た目ごとのクラス名は ClassOf の 1 か所だけに書き、外すときの一覧はそこから作る
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
        Icon.Content   = icon;
        Icon.IsVisible = icon is not null;
    }

    static object? IconOf(CellAppearance appearance) => /* 略 */ null;
}
```

## 何をなぜ直したか

### 1. クラス名の重複をなくした（重複したコード／変更が 2 か所に散る）
- 元のコードは、同じ 6 つのクラス名を `AppearanceClasses` の配列と `ClassOf` の switch の 2 か所に書いていた。見た目を足したり名前を変えたりするとき、両方を同じに直さなければならない。
- 片方だけ直すと、コンパイルもテストも通ったまま、「前の見た目のクラスが外れずに残る」（配列に足し忘れた場合）という気づきにくい不具合になる。
- そこで、名前は `ClassOf` だけに置き、配列は列挙型のすべての値を `ClassOf` に通して作るようにした。見た目を足すときに直すのは switch の 1 行だけになる（足し忘れれば、初期化の時点で `ArgumentOutOfRangeException` になり、すぐ気づける）。
- 配列の中身と順序は、元と同じ（`Enum.GetValues` は値の順に返し、列挙型の宣言順と配列の順は一致している）。

### 2. `IconOf` を 2 回呼ぶのをやめた（同じ計算の繰り返し）
- 元は `Content` に入れるものと、`IsVisible` を決めるものを別々に求めていた。表示の有無は「いま入れたアイコンがあるか」で決まるので、1 回求めた値をローカル変数 `icon` に置き、両方に使うようにした。意図が読みやすくなり、2 回の結果が食い違う余地もなくなる。

### 直さなかったもの（引き算）
- switch を `Dictionary` や属性（`[Description]` など）に置き換えることはしなかった。今の規模では switch がいちばん読みやすく、仕組みを増やす根拠がない。
- `ArgumentOutOfRangeException` のガードは残した（範囲外の値を黙って扱わないため。振る舞いも変えない）。

## 振る舞いが変わらない理由と仮定
- 外すクラスの一覧・付けるクラス・`Content`・`IsVisible` は、元と同じ値になる。
- 仮定: `IconOf` は副作用がなく、同じ見た目に対して null かどうかが常に同じである（`/* 略 */` の部分がそうなっていると仮定した）。毎回新しいオブジェクトを作る実装だとしても、元のコードで `IsVisible` の判定に使ったオブジェクトは捨てられていたので、見える振る舞いは変わらない（作るオブジェクトが 1 つ減るだけ）。
- 静的フィールドの初期化で `ClassOf` を呼ぶが、`ClassOf` は他の静的フィールドに依存しないので、初期化の順序の問題は起きない。
- 確かめ方（提案）: 各 `CellAppearance` で `ShowAppearance` を続けて呼び、`Classes` に見た目のクラスがちょうど 1 つだけ残ること、`AppearanceClasses` が元の 6 つの文字列と同じ順序・内容であることをテストで確かめる。
