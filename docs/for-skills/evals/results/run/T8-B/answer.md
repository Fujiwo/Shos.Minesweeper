# T8 の回答: `Border` の読み上げの名前が UI オートメーションに出ない不具合

スキルの分類: **バグ修正**（原因の調査を伴うので「判断を伴う作業」）。読んだ reference は quality-gates.md の「バグ修正」「ガード節」と、再現テストを置く足場がないので testing.md の「既存コードにテストを後から足す」「テスト基盤がない場合」。

## 1. 置いた仮定

アプリのコードは見られないので、次の仮定を置いた。違っていれば、それに合わせて読み替える。

- A1: 3 つの表示は、ウィンドウの `.axaml`（ここでは `MainWindow.axaml` とする）に、次の形で書いてある。名前は属性で付けていて、スタイルでは付けていない。
  ```xml
  <Border AutomationProperties.Name="{Binding MinesLeftSpeech}"> <TextBlock Text="{Binding MinesLeftText}"/> </Border>
  ```
- A2: 名前には値も入っている（例:「残り地雷数 10」「経過時間 25 秒」）。値の入らないラベル（「残り地雷数」だけ）の場合は、4.2 の子の扱いが変わる。
- A3: 盤面のマスは、それぞれ別の要素（`Button` など）で、マスの名前は出ている。出ないのは、`Border` に付けた名前だけ。
- A4: テストプロジェクトは `Shos.Minesweeper.Desktop.Tests`（xUnit v3。Avalonia は起動しない）。

## 2. 調べたこと（原因）

Avalonia 12.1.0 のソースを読んだ（出どころは 7 章）。

1. `Control.OnCreateAutomationPeer()` は、既定で **`NoneAutomationPeer`** を返す。`Border`（`Decorator`）はこれを上書きしないので、`Border` の peer は `NoneAutomationPeer` になる。
   ```csharp
   protected virtual AutomationPeer OnCreateAutomationPeer() => new NoneAutomationPeer(this);
   ```
2. `NoneAutomationPeer` は `IsControlElementCore() => false`、`IsContentElementCore() => false`、種類は `AutomationControlType.None` である。
3. どちらのビューに出るかは `ControlAutomationPeer.IsControlElementOverrideCore()` が決める。`AutomationProperties.IsControlElementOverride` が付いていればその値を使う。付いていなければ `AccessibilityView` を見て、`Default` なら peer の既定（上の `false`）を使う。**`AutomationProperties.Name` はこの判断に関わらない。**
   ```csharp
   if (AutomationProperties.GetIsControlElementOverride(Owner) is { } isControlElement) return isControlElement;
   var view = AutomationProperties.GetAccessibilityView(Owner);
   return view == AccessibilityView.Default ? IsControlElementCore() : view >= AccessibilityView.Control;
   ```
4. Windows の橋渡し（`Avalonia.Win32.Automation/AutomationNode.cs`）は、この値をそのまま `UIA_IsControlElementPropertyId`・`UIA_IsContentElementPropertyId` として返す。名前は `GetNameCore()` が `AutomationProperties.GetName(Owner)` を読むので、**名前そのものは peer に届いている**。
5. Inspect.exe は既定で Control ビューを表示し、スクリーン リーダー（ナレーターなど）も Control ビューと Content ビューをたどる。そのため、`IsControlElement = false` の要素は、名前が付いていても**見えない**。Raw ビューの中にだけある。子の `TextBlock`（`TextBlockAutomationPeer`。Control ビューに出る）は親をとばして現れるので、読まれるとすれば「10」のような数字だけになる。

**原因**: 自分たちの XAML が「`Border` に `AutomationProperties.Name` を付ければ読まれる」という前提を置いていた。ところが Avalonia では、`Border` のような配置のための要素は、既定でオートメーションの Control ビューと Content ビューから外れる。名前は付いているが、要素そのものが木に出ていない。ビューモデルのテストが Green なのは、名前の文字列は正しく、ずれが XAML と Avalonia の間にあるからである。

`WPF` では `Border` に peer がそもそもない。一方、Avalonia は peer を作るが、それをビューから外す。症状は同じでも仕組みは違う。

## 3. 進め方

quality-gates.md の「バグ修正」の順に進める。

1. **原因の裏付け**（手元で動かせる人に 1 分で頼めること）: Inspect.exe を **Raw View** に切り替えると、3 つの `Border` がその名前で現れ、`IsControlElement: false` になっているはず。これで 2 章の説明が正しいと確かめられる。出なければ、仮定 A1（名前の付け方）から見直す。
2. **再現テストを先に書き、Red を確かめる**（4.1）。画面のテストは使えないので、XAML を XML として読み、「読み上げの名前を付けた `Border` は、木に出る指定も持つ」という契約を確かめる。この時点では 3 か所とも指定がないので Red になる。
3. **最小の変更で直す**（4.2）。3 つの `Border` に `AutomationProperties.AccessibilityView="Content"` を付ける。2 のテストが Green になり、既存のテストもすべて Green であることを確かめる。
4. **実行環境で確かめる**（ユーザーにお願いすること。5 章の表）。Inspect.exe の Control ビューとナレーターで、3 つが名前と種類つきで出るかを見る。これは自分では行えない。テストで確かめられるのは「XAML に指定がある」ことまでで、「Windows で読まれる」ことは実機でしか確かめられない。
5. 周りで気づいたことは直さず、報告にとどめる（6 章）。

## 4. 直し方（コードの変更）

### 4.1 再現テスト（先に足して Red を確かめる）

テストプロジェクトの csproj に、デスクトップ版の `.axaml` を出力先に写す指定を足す。

```xml
<!-- Shos.Minesweeper.Desktop.Tests.csproj -->
<ItemGroup>
  <!-- 画面のテストが使えないので、XAML を XML として読んでオートメーションの指定を確かめる -->
  <None Include="..\Shos.Minesweeper.Desktop\**\*.axaml" LinkBase="Axaml" CopyToOutputDirectory="PreserveNewest" />
</ItemGroup>
```

```csharp
using System.Xml.Linq;

namespace Shos.Minesweeper.Desktop.Tests;

public class AutomationMarkupTests
{
    // Avalonia の Border は既定の peer が NoneAutomationPeer で、Control/Content ビューに出ない。
    // 名前だけを付けても読まれないので、名前を付けた Border は AccessibilityView も持つ必要がある。
    public static TheoryData<string> AxamlFiles()
    {
        var data = new TheoryData<string>();
        foreach (var path in Directory.EnumerateFiles(Path.Combine(AppContext.BaseDirectory, "Axaml"), "*.axaml", SearchOption.AllDirectories))
            data.Add(Path.GetRelativePath(AppContext.BaseDirectory, path));
        return data;
    }

    [Theory]
    [MemberData(nameof(AxamlFiles))]
    public void BordersWithAutomationNameAreExposedToAutomation(string relativePath)
    {
        var document = XDocument.Load(Path.Combine(AppContext.BaseDirectory, relativePath));

        var hiddenNamedBorders = document.Descendants()
            .Where(element => element.Name.LocalName == "Border")
            .Where(border => border.Attribute("AutomationProperties.Name") is not null)
            .Where(border => border.Attribute("AutomationProperties.AccessibilityView") is null)
            .Select(border => border.Attribute("AutomationProperties.Name")!.Value)
            .ToList();

        Assert.Empty(hiddenNamedBorders);   // 失敗したら、指定のない Border の名前（バインディング）が出る
    }
}
```

- 最初に、今の XAML で **Red**（3 件の名前が出る）になることを確かめる。今の誤り（指定がないこと）を期待値にしない。
- 見るのは「名前を付けたすべての `Border`」である。これからの表示の追加でも、同じ誤りを見つけられる。

### 4.2 修正（XAML だけ）

```xml
<!-- 残り地雷数 -->
<Border AutomationProperties.Name="{Binding MinesLeftSpeech}"
        AutomationProperties.AccessibilityView="Content"
        AutomationProperties.ControlTypeOverride="Text">
  <TextBlock Text="{Binding MinesLeftText}"
             AutomationProperties.AccessibilityView="Raw"/>
</Border>

<!-- 経過時間 -->
<Border AutomationProperties.Name="{Binding ElapsedSpeech}"
        AutomationProperties.AccessibilityView="Content"
        AutomationProperties.ControlTypeOverride="Text">
  <TextBlock Text="{Binding ElapsedText}"
             AutomationProperties.AccessibilityView="Raw"/>
</Border>

<!-- 盤面（中のマスは、それぞれ自分の名前で読まれる） -->
<Border AutomationProperties.Name="{Binding BoardSpeech}"
        AutomationProperties.AccessibilityView="Content"
        AutomationProperties.ControlTypeOverride="Group">
  <!-- マスの並び（変えない） -->
</Border>
```

指定ごとの理由:

| 指定 | 理由 |
|---|---|
| `AccessibilityView="Content"` | 原因への直接の手当て。`view >= Control` で Control ビューにも出る。どれも情報を持つ要素なので、Control ではなく Content にした |
| `ControlTypeOverride="Text"`（数の 2 つ） | 何も付けないと、種類の `None` は UIA の **Group** として出る（`ToUiaControlType` の `None => Group`）。ナレーターは「グループ」と読むので、値を示す表示として Text にした |
| `ControlTypeOverride="Group"`（盤面） | 既定でも Group になるが、マスをまとめる入れ物であることを XAML に書いておく。省いても振る舞いは同じなので、既存の流儀に合わせて省いてもよい |
| 子の `TextBlock` に `AccessibilityView="Raw"` | 仮定 A2 では、親の名前に値が入っている。子の `TextBlock`（Control ビューに出る）が残ると「残り地雷数 10」「10」と二度読まれるので、子は Raw に下げる。A2 が違い、名前がラベルだけなら、この指定は付けない（子の数字が値として読まれる） |

### 4.3 捨てた案

- **`Border` を継いだクラスで `OnCreateAutomationPeer` を上書きする**: 添付プロパティで足りるのに、クラスと peer が増える。引き算で捨てた。
- **`AutomationProperties.IsControlElementOverride="True"` だけを付ける**: Control ビューには出るが、Content ビューに出ないので、Content ビューを読む支援技術で抜ける。`AccessibilityView` の方が意図（読ませたい内容である）に合う。
- **名前を子の `TextBlock` に移す**: 盤面には `TextBlock` がないので、1 つの方法で 3 か所を直せない。
- **スタイル（`Style Selector="Border"`）でまとめて指定する**: 名前のない装飾用の `Border` まで木に出てしまう。付けるのは 3 か所だけなので、名前の隣に書く方が意図が読める。
- **peer を作って確かめるテスト**（`ControlAutomationPeer.CreatePeerForElement(new Border { ... }).IsControlElement()`）: 確かめるのはライブラリの振る舞いで、自分たちの XAML ではない。なので、修正で Red から Green にならない。また `GetOrCreateAutomationPeer` が `VerifyAccess()`（UI スレッドの検査）を呼ぶので、Avalonia を起動しない xUnit では、スレッドによっては失敗しうる。前提の裏付けは、3 章 1 の Raw ビューでの確認で代える。

## 5. 検証

- **実行したこと**: Avalonia 12.1.0 のソースの該当箇所を読んだ（7 章）。
- **実行できなかったこと**: ビルド、テスト、アプリの起動。4.1 のテストが Red になること、4.2 の後に Green になることは確かめていない。サンドボックスの外のコードは読めず、手元に Avalonia もない。

実機で確かめる項目（ユーザーにお願いする）:

| # | 見る場所 | 期待すること |
|---|---|---|
| 1 | 直す前の Inspect の Raw View | 3 つの `Border` が名前つき、`IsControlElement: false` で出る（原因の裏付け） |
| 2 | 直した後の Inspect の Control View | 3 つが名前つきで出る。種類は、数の 2 つが Text、盤面が Group |
| 3 | ナレーターで、Tab と走査モードで移る | 「残り地雷数 10」などと 1 回だけ読む（二重に読まない） |
| 4 | 旗を立てる、時間が進む | Inspect で名前が新しい値に変わる（バインディングが peer に届いている） |

## 6. 報告

- **What**: 読み上げの名前を付けた 3 つの `Border` に、`AccessibilityView="Content"` と、種類の上書き（`ControlTypeOverride`）を足す。子の `TextBlock` は Raw に下げる。再現テストとして、XAML の指定を確かめる xUnit のテストと、XAML を出力先に写す csproj の指定を足す。準備的リファクタリングはない。
- **Why**: 原因は、Avalonia の `Border` の peer（`NoneAutomationPeer`）が Control ビューと Content ビューから外れることにある。添付プロパティ 1 つで、原因に直接手が届く。
- **作らなかったもの**: `Border` を継いだクラスと、独自の peer。値が変わったときに知らせる仕組み（ライブ リージョン）。タイマーが毎秒読まれてもうるさいので、頼まれていない。
- **範囲外の気づき**: 「XAML に置いた前提が、ビューモデルのテストでは見えない」という網の穴がある（テストの届かない所に振る舞いがある）。4.1 のテストが一部をふさぐが、ほかのオートメーションの指定（キーボードの操作、フォーカスの順序）は、今も実機でしか確かめられない。
- **ユーザーの判断が要る点**:
  1. 仮定 A2（名前に値が入っているか）。これで、子の `TextBlock` を Raw にするかが決まる。
  2. 経過時間を、値が変わるたびに知らせるか（ライブ リージョン）。仕様の判断なので、今回は入れていない。
  3. XAML を XML として読むテスト（4.1）を置き続けるか。壊れやすさ（XAML の書き方を属性からスタイルに変えると誤って Red になる）と、同じ誤りを繰り返さない効き目との引き換えになる。

## 7. 出どころ

- Avalonia 12.1.0 `src/Avalonia.Controls/Control.cs`（`OnCreateAutomationPeer`）: https://github.com/AvaloniaUI/Avalonia/blob/12.1.0/src/Avalonia.Controls/Control.cs
- `NoneAutomationPeer`: https://github.com/AvaloniaUI/Avalonia/blob/12.1.0/src/Avalonia.Controls/Automation/Peers/NoneAutomationPeer.cs
- `ControlAutomationPeer`（`IsControlElementOverrideCore`、`GetNameCore`）: https://github.com/AvaloniaUI/Avalonia/blob/12.1.0/src/Avalonia.Controls/Automation/Peers/ControlAutomationPeer.cs
- `AutomationProperties`（`AccessibilityView`、`ControlTypeOverride`）: https://github.com/AvaloniaUI/Avalonia/blob/master/src/Avalonia.Controls/Automation/AutomationProperties.cs （master を参照した）
- Windows の橋渡し `AutomationNode.cs`（`ToUiaControlType`、`GetPropertyValue`）: https://github.com/AvaloniaUI/Avalonia/blob/12.1.0/src/Windows/Avalonia.Win32.Automation/AutomationNode.cs
- Avalonia の文書「Accessibility」: https://docs.avaloniaui.net/docs/app-development/accessibility
- NoneAutomationPeer の API 文書: https://docs.avaloniaui.net/api/avalonia/automation/peers/noneautomationpeer
