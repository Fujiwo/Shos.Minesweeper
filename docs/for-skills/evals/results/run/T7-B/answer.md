# T7 回答: CellView のリファクタリング

SKILL.md の表では「リファクタリング」に当たるので、refactoring.md を読んだ。安全網のテストが無いので、testing.md の「既存コードにテストを後から足す」「テスト基盤がない場合」も読んだ。

## 置いた仮定

- `IconOf` は `appearance` だけで結果が決まり、副作用が無い（本文は「略」）。
- 対象は .NET 5 以降と Avalonia 11 以降で、`Enum.GetValues<T>()` と LINQ を使える。
- 渡されたのはこの断片だけで、テストがあるかどうかは分からない。そのため、IDE の自動リファクタリング（変数の抽出、メソッドの抽出）に近い小さな手だけにし、構造を大きく作り変えない。
- 契約（外から見た振る舞い）は次の 3 点とし、これを保つ。
  - 見た目のクラスを 6 つともすべて外してから、1 つだけ付ける。
  - `Icon.Content` と `Icon.IsVisible` を同じように設定する。
  - 定義外の値には `ArgumentOutOfRangeException` を投げる。

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
        ReplaceAppearanceClass(appearance);
        ShowIcon(IconOf(appearance));
    }

    void ReplaceAppearanceClass(CellAppearance appearance)
    {
        Classes.RemoveAll(AppearanceClasses);
        Classes.Add(ClassOf(appearance));
    }

    void ShowIcon(object? icon)
    {
        Icon.Content = icon;
        Icon.IsVisible = icon is not null;
    }

    static object? IconOf(CellAppearance appearance) => /* 略 */ null;
}
```

## What と Why（臭いと技法の名前で）

1. `AppearanceClasses` と `ClassOf`: **重複したコード**
   - 症状: 見た目からクラス名への対応が、配列と switch の 2 か所に同じ文字列で書かれている。見た目を 1 つ足すと、両方を直すことになる（変更の分散）。片方を直し忘れると、古いクラスが外れずに残る。
   - 技法: 対応を持つ場所を `ClassOf` の 1 か所に決め、配列は列挙の全値を `ClassOf` に通して作る。
   - 同じ意図か: 配列は「見た目のクラスの全体」、switch は「見た目ごとのクラス」で、どちらも同じ対応表の表れである。たまたま似ているのではない。
   - 振る舞い: 列挙の順は Unopened, Opened, Flagged, Mine, Exploded, WrongFlag で、元の配列と同じ順・同じ 6 つの文字列になる。`RemoveAll` に渡す中身は変わらない。
2. `ShowAppearance` の中で `IconOf(appearance)` を 2 回呼んでいる: **重複したコード**
   - 症状: 同じ計算を 2 回している。`Content` に入れた値と、`IsVisible` の判定に使った値が同じものだと、コードから読み取れない。
   - 技法: 1 回だけ呼び、その結果を引数として `ShowIcon` に渡す（変数の抽出と、次のメソッドの抽出をまとめた形）。
3. `ShowAppearance`: **メソッドの中で抽象度がそろっていない**（臭いの一覧のどれにも当たらないので、症状で書く）
   - 症状: 「クラスを付け替える」「アイコンを出す」という 2 つの関心事が、`RemoveAll`、`Add`、`Content`、`IsVisible` という How の 4 行で並んでいる。
   - 技法: **メソッド抽出**で、`ReplaceAppearanceClass` と `ShowIcon` に分ける。`ShowAppearance` は「見た目のクラスを付け替え、その見た目のアイコンを出す」と、そのまま読めるようになる。

## 作らなかったもの・捨てた案

- `Dictionary<CellAppearance, string>` にする案: 未定義の値のときの例外が `KeyNotFoundException` に変わり、振る舞いが変わるので捨てた。
- 列挙の名前をケバブケース（`WrongFlag` → `wrong-flag`）に変換してクラス名を作る案: 列挙の名前の変更がスタイルに漏れる結合を生み、読み手は変換の規則を追う必要が出る。対応を明示的に書く switch の方が読みやすいので捨てた。
- 属性（`[CssClass("...")]`）や、見た目ごとのクラスに分けてポリモーフィズムで答えさせる案: 分岐は `ClassOf` の 1 か所だけで、種類も安定している。**疑わしき一般化**になるので入れなかった。`IconOf` の中にも同じ種類の分岐があるなら、2 か所目の分岐になるが、本文が「略」なので判断を保留する（下の「範囲外の気づき」）。
- `Classes.RemoveAll` と `Add` を、差分だけ変える形（同じクラスなら何もしない、など）にする最適化: 変更の通知の回数と順序が変わり、振る舞いが変わりうる。計測の根拠も無いので入れなかった。
- 利用者への振る舞いと、公開の API（`ShowAppearance` の名前と引数）は変えていない。足したメソッドはどれも private である。

## 検証結果

- ビルドもテストも実行していない。環境が無く、渡されたのも断片だけなので、検証できていない。
- 次は、机上で突き合わせて確かめた。
  - `Enum.GetValues<CellAppearance>()` の順と、元の配列の順が同じ 6 要素で一致する。
  - `IconOf` を 1 回にしても、仮定（副作用が無い）の下では `Content` と `IsVisible` の結果が同じになる。
- 安全網として足すべき、振る舞いを押さえるテストの表（テスト基盤があれば、リファクタリングの前に足す）:

| 入力 / 状態 | 期待 |
|---|---|
| 各 `CellAppearance`（6 値） | `ClassOf` の結果が、元の対応表の文字列と一致する |
| `(CellAppearance)99` | `ClassOf` が `ArgumentOutOfRangeException` を投げる |
| `AppearanceClasses` | 元の 6 つの文字列と同じ順序・同じ内容 |
| `Flagged` の後に `Opened` を表示する | クラスは `opened` だけで、`flagged` が残らない |
| アイコンが null になる見た目 | `Icon.IsVisible == false` |

  - `ClassOf` と `AppearanceClasses` は Avalonia を起動せずに確かめられる。ただし private なので、テストするには internal にして `InternalsVisibleTo` を使うなど、公開の範囲を変える必要があり、その判断はユーザーに委ねる。
  - `ShowAppearance` の確かめには、ヘッドレスの Avalonia か、実際に動かしての確認が必要になる。
- 実装をわざと壊して、テストが失敗することの確認もしていない。

## 範囲外の気づき

- `IconOf`: **スイッチ文の疑い**。本文が「略」だが、中で `CellAppearance` ごとに分岐しているなら、`ClassOf` と同じ種類の分岐が 2 か所目に現れていることになる。その場合は、見た目ごとのクラス名とアイコンを 1 つの対応（例: 1 つの switch でその見た目の表示の情報を返す）にまとめる余地がある。
- 対応を 1 か所にまとめたので、`ClassOf` に腕を足し忘れて列挙の値だけを足すと、`CellView` の型を初期化するときに例外になる。元のコードは、その見た目を表示したときに初めて例外になった。どちらも、今は存在しない値についての違いで、現在の振る舞いは変わらない。ただし、失敗が早く出るようになる（テストで捕まえやすくなる）点は、差として知らせておく。

## ユーザーの判断が要る点

- `ClassOf` と `AppearanceClasses` をテストするために、公開の範囲を広げてよいか（internal と `InternalsVisibleTo`）。
- `IconOf` の中身を見せてもらえれば、2 か所目の分岐かどうかを判断し、対応の一本化を提案できる。
