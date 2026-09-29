# T7: CellView のリファクタリング

使った判断基準: スキル D の「リファクタリング」の行に従い、refactoring.md の「進め方の要約」「コードの臭い」「臭い → 技法の対応表」「精神論ではなく臭いと技法の名前で語る」を読んだ。

## 直したコード

```csharp
public enum CellAppearance { Unopened, Opened, Flagged, Mine, Exploded, WrongFlag }

public sealed class CellView : UserControl
{
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

## What と Why（臭いと技法）

1. `AppearanceClasses` と `ClassOf`: 重複したコード（「見た目 → クラス名」の同じ対応が、配列と switch 式の 2 か所に書かれている。見た目を 1 つ足すか、クラス名を 1 つ変えると、両方を直す必要があり、配列だけ直し忘れると古いクラスが外れずに残る。この忘れはコンパイラーもテストも捕まえにくい）→ 一覧を対応の式から導く（`Enum.GetValues<CellAppearance>().Select(ClassOf)`）→ 対応の所在が `ClassOf` の 1 か所に確定した。
   - 残す側に switch 式を選んだ理由: switch 式は見た目ごとの名前を 1 行ずつ対で読め、値が足りない arm はコンパイラーが警告（CS8509 系の網羅性の警告）で教えてくれる。配列の側を残して添字で引く案は、列挙の並びと配列の並びが一致しているという暗黙の約束に頼るので捨てた。
2. `ShowAppearance` の `IconOf(appearance)` 2 回: 重複したコード（同じ値を 2 回求めている。`Content` に入れたものと、`IsVisible` の判定に使ったものが同じだという保証が、`IconOf` が純粋であることへの暗黙の依存になっている）→ 説明用の変数 `icon` に一度だけ受ける → 「表示するアイコンがあれば見せる」という意図がそのまま読め、`IconOf` の呼び出しは 1 回になった。

## 作らなかったもの・置いた仮定

- 列挙名からクラス名を機械的に作る案（`WrongFlag` → `wrong-flag` の kebab-case 変換）は入れなかった。switch 式を消せるが、クラス名はスタイル（XAML）との契約であり、列挙の名前を変えただけでスタイルが黙って外れるようになる。対応は明示的な方が安全と判断した。
- `ShowAppearance` を `ShowClassOf` と `ShowIcon` に分けるメソッド抽出は見送った。本体が 5 文で、空行による 2 段落で意図が読めるため、語彙を増やしても読む対象が増えるだけと判断した（引き算）。
- 属性（`Dictionary` や、見た目ごとのクラス名を持つ型など）への置き換えや、ポリモーフィズムへの置き換えはしない。分岐は `ClassOf` の 1 か所だけで、種類も安定しているため（switch そのものは臭いではない）。
- 仮定: `IconOf` は副作用のない関数である（`/* 略 */` の中身は見えない）。副作用があると、呼び出しを 1 回にしたことが振る舞いの変化になる。元のコードでも、2 回の呼び出しで null かどうかが変わると `Content` と `IsVisible` が食い違うので、純粋であることを前提にしていたと読んだ。
- 仮定: `Classes` は Avalonia の `Classes`（`AvaloniaList<string>`）で、`RemoveAll` は渡した名前をすべて外し、見た目以外のクラス（`:pointerover` などの疑似クラスや、他のスタイルのクラス）は残す。配列の中身と並びは元と同じなので、この呼び出しの結果は変わらない。

## 検証結果

- Avalonia のパッケージはネットワークなしでは取得できないため、実物の `UserControl` ではビルドしていない。
- 代わりに、作業フォルダーの `check/`（`dotnet run`、.NET 10.0.112）で、`Classes` と `Icon` を使う操作だけを持つ代用品にして、元のコードと直したコードを並べて比べた。
  - 6 つの見た目のすべての組（初期状態 → 見た目、見た目 → 見た目、計 42 通り）で、クラスの一覧、`Icon.Content`、`Icon.IsVisible` が一致した。見た目以外のクラス（`other`）が残ることも一致した。
  - 未定義の値（`(CellAppearance)99`）では、どちらも `ArgumentOutOfRangeException`（`Enum.GetValues` は定義された値だけを返すので、静的初期化子では投げない）。
  - 直したコードでは `IconOf` の呼び出しが 1 回になった。
- 結果: すべて一致。

## 振る舞いの差として知っておくこと

- 今のコードでは起きないが、今後、列挙に値を足して `ClassOf` に arm を足し忘れると、元のコードではその見た目を表示したときに初めて例外になるのに対し、直したコードでは `CellView` を初めて使うとき（静的初期化子の中）に `TypeInitializationException` になる。忘れが早く見つかる方向の差なので、良い差と判断した。

## 範囲外の気づき

- `ClassOf` の対応と XAML のスタイルのセレクター（`.wrong-flag` など）は、別のファイルにある同じ文字列である（変更の分散の芽）。直すなら、`AppearanceClasses` の各名前に当たるスタイルがあることを確かめるテストを足す程度でよい（今回は範囲外のため未実施）。

## ユーザーの判断が要る点

- `IconOf` が純粋であるという仮定が正しいか（違えば、呼び出しを 1 回にした変更は振る舞いを変える）。
