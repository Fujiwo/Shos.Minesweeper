# 課題 T8: 読み上げの名前が UI オートメーションの木に出ない不具合

## 1. 結論

原因は、ビューモデルでも名前の付け方でもなく、**Avalonia の `Border` が UI オートメーションの木（Control ビュー・Content ビュー）に入らない要素として公開されること**である。
`AutomationProperties.Name` は付いているが、その要素自体が木から外されているので、Inspect.exe にもスクリーン リーダーにも見えない。

直し方は、名前を付けた 3 つの `Border` に `AutomationProperties.AccessibilityView="Content"` と、役割を表す `AutomationProperties.ControlTypeOverride` を付けること（コードは 4 章）。中の `TextBlock` は二重に読まれないように木から外す。

## 2. 調べたこと（出どころ）

Avalonia のソースコード（GitHub の AvaloniaUI/Avalonia、`release/12.1` ブランチと `master`）を読んだ。

| 調べたもの | 分かったこと | 出どころ |
|---|---|---|
| `Control.OnCreateAutomationPeer()` | 既定の実装は `new NoneAutomationPeer(this)` を返す。専用のピアを持たないコントロール（`Border`、`Panel` など）は、このピアになる | https://github.com/AvaloniaUI/Avalonia/blob/master/src/Avalonia.Controls/Control.cs |
| `NoneAutomationPeer` | コメントは「非対話的、またはアプリの論理構造に寄与しない要素として公開する」。`GetAutomationControlTypeCore()` が `None`、`IsControlElementCore()` と `IsContentElementCore()` が `false` を返す | https://github.com/AvaloniaUI/Avalonia/blob/master/src/Avalonia.Controls/Automation/Peers/NoneAutomationPeer.cs |
| `ControlAutomationPeer` | `GetNameCore()` は `AutomationProperties.GetName(Owner)` を返す（名前自体はピアに渡っている）。`IsControlElementOverrideCore()` / `IsContentElementOverrideCore()` は、`AccessibilityView` が `Default` ならピアの `IsControlElementCore()` / `IsContentElementCore()` に従い、そうでなければ `view >= Control` / `view >= Content` で決める。型は `AutomationProperties.GetControlTypeOverride(Owner) ?? GetAutomationControlTypeCore()` | https://github.com/AvaloniaUI/Avalonia/blob/release/12.1/src/Avalonia.Controls/Automation/Peers/ControlAutomationPeer.cs |
| `AutomationProperties` | 12.1 に `AccessibilityView`（`Default`、`Raw`、`Control`、`Content`）、`IsControlElementOverride`、`ControlTypeOverride`、`LiveSetting` の添付プロパティがある | https://github.com/AvaloniaUI/Avalonia/blob/release/12.1/src/Avalonia.Controls/Automation/AutomationProperties.cs |
| 公式文書のアクセシビリティ | `AccessibilityView` の `Default` は「コントロールのオートメーション ピアが決める」。独自のコントロールは `OnCreateAutomationPeer` の上書きを勧めている | https://docs.avaloniaui.net/docs/app-development/accessibility |

つまり、`Border` のピアは名前を持っているのに、「コントロール要素でもコンテンツ要素でもない」と答えている。Inspect.exe の既定の表示は Control ビューで、Narrator などのスクリーン リーダーもこのビューをたどるので、`Border` は飛ばされ、名前は出ない（Inspect.exe で Raw ビューに切り替えると見えるはずである。これが原因の確かめ方の 1 つになる）。
ビューモデルのテストが Green なのは当然で、不具合は XAML と Avalonia の既定の振る舞いの間にある。

## 3. 進め方

アプリを手元で動かせず、Avalonia.Headless も使えないので、次の順で進める。

1. **原因の仮説を、ソースで裏付ける**（2 章で済ませた）。
2. **自動テストで再現する（できる範囲で）**。`ControlAutomationPeer.CreatePeerForElement(control)` は、ウィンドウを開かずに `Control` のピアを作れる公開 API である。XAML を読み込まずに、同じ添付プロパティを付けた `Border` をテストの中で作り、ピアの `IsControlElement()`・`IsContentElement()`・`GetName()`・`GetAutomationControlType()` を確かめる（5.1）。
   - 仮定: Avalonia のコントロールは、アプリを起動しなくても、テストのスレッドで `new` できる（`Dispatcher.UIThread` は最初に触ったスレッドを UI スレッドとして扱う）。もしテストの環境で例外になるなら、このテストは諦め、5.2 の XAML の文字列の検査と、6 章の実機の確認に頼る。
   - このテストが確かめるのは「付けた属性で、ピアが木に入ると答えること」であり、XAML にその属性が付いていることは確かめない。そこで、XAML の対象の要素にこの属性が付いていることを、XAML のファイルを XML として読んで確かめるテストも足す（5.2）。
3. **赤を確かめてから直す**。修正前の `Border`（属性なし）ではピアの `IsControlElement()` が `false` になるテストを先に書き、属性を付けて Green にする。
4. **XAML を直す**（4 章）。
5. **実機で確かめる**（6 章。Windows 11 の上で、ユーザーか、動かせる人が行う）。自動テストでは Windows の UIA のプロバイダー（Avalonia.Win32 の橋渡し）の振る舞いまでは確かめられないので、これは省けない。

## 4. 直し方（コードの変更）

### 4.1 XAML

仮定: 画面の XAML は次のような形である（実物は見ていない）。名前は `x:Name` ではなく、ビューモデルの読み上げの名前にバインドしているものとする。

変更前:

```xml
<Border AutomationProperties.Name="{Binding MineCounterName}">
  <TextBlock Text="{Binding MineCounterText}" />
</Border>
```

変更後（3 つの `Border` に同じ形を当てる）:

```xml
<!-- 残り地雷数 -->
<Border AutomationProperties.Name="{Binding MineCounterName}"
        AutomationProperties.AccessibilityView="Content"
        AutomationProperties.ControlTypeOverride="Text">
  <TextBlock Text="{Binding MineCounterText}"
             AutomationProperties.AccessibilityView="Raw" />
</Border>

<!-- 経過時間 -->
<Border AutomationProperties.Name="{Binding TimerName}"
        AutomationProperties.AccessibilityView="Content"
        AutomationProperties.ControlTypeOverride="Text">
  <TextBlock Text="{Binding TimerText}"
             AutomationProperties.AccessibilityView="Raw" />
</Border>

<!-- 盤面 -->
<Border AutomationProperties.Name="{Binding BoardName}"
        AutomationProperties.AccessibilityView="Content"
        AutomationProperties.ControlTypeOverride="Group">
  <!-- マスの並び（中のマスはそれぞれのピアで木に出るので、ここは変えない） -->
  ...
</Border>
```

要点:

- `AccessibilityView="Content"` で、`Border` のピアが Control ビューと Content ビューの両方に入る（`Content >= Control` なので、`IsControlElement` と `IsContentElement` がどちらも `true` になる）。`Control` にすると Content ビューには入らない。表示している値は利用者にとって内容なので `Content` にする。
- `ControlTypeOverride` を付けないと、型が `None` のままで、スクリーン リーダーが役割を言えない。残り地雷数と経過時間は `Text`、盤面はマスをまとめる `Group` にする。
- 中の `TextBlock` は、そのままだと `TextBlock` のピアが自分の文字（例: 「010」）で木に出て、`Border` の名前と二重に読まれる。数字だけでは意味が分からないので、`Raw` にして Control ビューから外し、意味の付いた `Border` の名前だけを読ませる。
  - 仮定: `Border` の名前に値が含まれている（例: 「残り地雷数 10」）。名前が「残り地雷数」だけで値を含まないなら、`TextBlock` は外さずに残し、`Border` の型を `Group` にする。
- 盤面の中のマスは、ボタンなどのピアを持つコントロールなら、そのまま `Group` の子として木に出る。盤面が 1 つのコントロールで描いている（マスが子の要素でない）なら、このままではマスは木に出ない。それはこの不具合の範囲の外なので、別の課題として記録する。

### 4.2 属性の置き場所

同じ 3 つの属性を 3 か所に書くのは重複だが、要素は 3 つだけで、それぞれ型が違う（`Text` と `Group`）。スタイルのクラスに括り出すと、XAML を読むだけで読み上げの扱いが分からなくなるので、各要素に直接書く。独自のコントロール（`Border` を継いで `OnCreateAutomationPeer` を上書きする）も作らない。添付プロパティで足りるからである。

## 5. テスト（Desktop.Tests に足す）

### 5.1 ピアの振る舞い（仕組みの確かめ）

```csharp
using Avalonia.Automation;
using Avalonia.Automation.Peers;
using Avalonia.Controls;
using Xunit;

public class AutomationTreeTests
{
    [Fact]
    public void BorderWithoutAccessibilityViewIsHiddenFromTheControlView()
    {
        var border = new Border();
        AutomationProperties.SetName(border, "残り地雷数 10");

        var peer = ControlAutomationPeer.CreatePeerForElement(border);

        Assert.False(peer.IsControlElement());   // 不具合の再現: 名前はあるが木に出ない
        Assert.Equal("残り地雷数 10", peer.GetName());
    }

    [Theory]
    [InlineData(AutomationControlType.Text)]
    [InlineData(AutomationControlType.Group)]
    public void BorderWithContentViewIsExposedWithItsNameAndType(AutomationControlType type)
    {
        var border = new Border();
        AutomationProperties.SetName(border, "残り地雷数 10");
        AutomationProperties.SetAccessibilityView(border, AccessibilityView.Content);
        AutomationProperties.SetControlTypeOverride(border, type);

        var peer = ControlAutomationPeer.CreatePeerForElement(border);

        Assert.True(peer.IsControlElement());
        Assert.True(peer.IsContentElement());
        Assert.Equal("残り地雷数 10", peer.GetName());
        Assert.Equal(type, peer.GetAutomationControlType());
    }
}
```

1 つ目は、不具合の原因の仮説を固定するテストである。これが `true` になる版に Avalonia を上げたら、原因の説明が変わったことに気付ける。

### 5.2 XAML に属性が付いていること（変更の確かめ）

画面の XAML（例: `MainWindow.axaml`）をテストの出力にコピーする（csproj で `<None Include="..\Shos.Minesweeper.Desktop\MainWindow.axaml" CopyToOutputDirectory="PreserveNewest" />`）。`XDocument` で読み、`AutomationProperties.Name` を持つすべての要素に `AutomationProperties.AccessibilityView` が付いていることを確かめる。

```csharp
[Fact]
public void EveryNamedElementInTheMainWindowIsInTheAutomationTree()
{
    var document = XDocument.Load("MainWindow.axaml");
    var named = document.Descendants()
        .Where(element => element.Attribute("AutomationProperties.Name") is not null)
        .ToList();

    Assert.NotEmpty(named);
    Assert.All(named, element =>
        Assert.NotNull(element.Attribute("AutomationProperties.AccessibilityView")));
}
```

対象を「名前を持つ要素すべて」にしたのは、今後 `Border` や `Panel` に名前だけを付ける同じ間違いを防ぐためである。ただし、ボタンなど、もともと木に出るコントロールにも属性を求めることになる。それが煩わしいなら、対象を `Border` と `Panel` の要素に絞る（仮定: 今の XAML で名前を持つのは、この 3 つの `Border` だけ）。

## 6. 実機での確認（Windows 11、ユーザーが行う）

1. 発行したアプリを起動し、Inspect.exe（Windows SDK）か Accessibility Insights for Windows を、既定の Control ビューで開く。
2. 修正前の版で、Raw ビューに切り替えると 3 つの `Border` の名前が見え、Control ビューでは見えないことを確かめる（原因の裏付け）。
3. 修正後の版で、Control ビューに 3 つの要素が出て、`Name` が期待どおり、`ControlType` が Text / Group、`IsContentElement` が true であることを確かめる。中の `TextBlock` が Control ビューに重ねて出ないことも見る。
4. ゲームを進め、残り地雷数と経過時間が変わったあとに Inspect.exe で `Name` が新しい値になっていることを確かめる。
5. Narrator を起動し、スキャン モード（Caps Lock + Space）で上下の矢印で要素を順にたどって、3 つの名前が読まれ、数字だけが重ねて読まれないことを確かめる。

## 7. 残る懸念（今回は直さない）

- `ControlAutomationPeer` の `OwnerPropertyChanged` が変更の通知を出すのは、表示・位置・`AutomationId`・`ItemStatus` などで、`AutomationProperties.Name` の変更の通知は、ソースを見た範囲では見当たらなかった。値が変わっても、スクリーン リーダーが自動で読み直すとは限らない（利用者が要素に移れば、その時の名前は読まれる）。変わるたびに知らせたいなら `AutomationProperties.LiveSetting` を使うが、経過時間は毎秒変わってうるさいので付けない。残り地雷数に付けるかは、仕様として決めてから行う。
- 修正は Avalonia 12.1 の既定の振る舞い（`Border` が `NoneAutomationPeer`）に依存している。5.1 の 1 つ目のテストで、この前提が変わったら気付けるようにした。
