# T8 の回答: `Border` に付けた `AutomationProperties.Name` が UI オートメーションに出ない

## 適用したスキルの範囲

バグの原因調査なので「判断を伴う作業」の「バグ修正」として扱い、quality-gates.md の「バグ修正」と「ガード節」を読んだ。画面のテストが使えず、再現テストを置く足場がないので、testing.md の「既存コードにテストを後から足す」（と「テスト基盤がない場合」）も読んだ。「バグ修正」の手順 2 に「原因が外のフレームワークの振る舞いにあるときは、直し方を試す前に、その仕組み（型、文書、ソース）を調べる。当て推量の試行を重ねない」とあるので、まず Avalonia 12.1.0 のソースで仕組みを確かめた。

## 1. 調べたこと（出どころ）

Avalonia のタグ `12.1.0` のソースを読んだ。

| # | 分かったこと | 出どころ |
|---|---|---|
| 1 | `Control.OnCreateAutomationPeer()` の既定の実装は `return new NoneAutomationPeer(this);` | https://raw.githubusercontent.com/AvaloniaUI/Avalonia/master/src/Avalonia.Controls/Control.cs |
| 2 | `Border`（基底は `Decorator`）も `Decorator` も `OnCreateAutomationPeer` をオーバーライドしない。つまり `Border` のピアは `NoneAutomationPeer` になる | https://raw.githubusercontent.com/AvaloniaUI/Avalonia/12.1.0/src/Avalonia.Controls/Border.cs 、同 Decorator.cs |
| 3 | `NoneAutomationPeer` は `IsControlElementCore()` と `IsContentElementCore()` が `false`、コントロールの種類は `None` を返す（「自動化には対話しない要素、アプリの論理構造に寄与しない要素として見せる」ピア） | https://raw.githubusercontent.com/AvaloniaUI/Avalonia/master/src/Avalonia.Controls/Automation/Peers/NoneAutomationPeer.cs |
| 4 | 名前は `ControlAutomationPeer.GetNameCore()` が `AutomationProperties.GetName(Owner)` から取るので、ピアは名前を持っている。子の列挙（`GetChildrenCore`）は見えている子をすべて並べ、コントロール要素かどうかでは絞らない | https://raw.githubusercontent.com/AvaloniaUI/Avalonia/12.1.0/src/Avalonia.Controls/Automation/Peers/ControlAutomationPeer.cs |
| 5 | Windows の UIA の提供側は `IsControlElement` を `Peer.IsControlElement()` で、`ControlType` の `None` を `UIA_GroupControlTypeId` で返す | https://raw.githubusercontent.com/AvaloniaUI/Avalonia/12.1.0/src/Windows/Avalonia.Win32.Automation/AutomationNode.cs |
| 6 | `IsControlElement` と `IsContentElement` は、添付プロパティで変えられる。`IsControlElementOverrideCore` は、`AutomationProperties.IsControlElementOverride` があればそれを使い、なければ `AccessibilityView` が `Default` のときはピアの既定、それ以外は `view >= AccessibilityView.Control` を返す。`IsContentElementOverrideCore` も同じ形で、`view >= AccessibilityView.Content` を返す | 同 ControlAutomationPeer.cs（12.1.0） |
| 7 | `AccessibilityView` の値は `Default, Raw, Control, Content` の順。`Content` にすると、Control ビューと Content ビューの両方に入る | https://raw.githubusercontent.com/AvaloniaUI/Avalonia/12.1.0/src/Avalonia.Controls/Automation/AutomationProperties.cs |

### 原因（上の表から導いたこと）

`Border` のピアは `NoneAutomationPeer` なので、`IsControlElement = false`、`IsContentElement = false` で UIA に出る。名前は持っているが、要素は Raw ビューにしか入らない。Inspect.exe の既定の表示（Control ビュー）やナレーターなどのスクリーン リーダーは、コントロール要素でないものを飛ばすので、名前が見えず、読み上げられない。つまり、名前の付け方は正しいが、付けた要素が UIA の Control/Content ビューから外されている。ビューモデルはこれに関わらないので、ビューモデルのテストが Green でも矛盾しない。

（仮定: `Border` は `IsVisible = true` で、名前は空でない。Inspect.exe を「Raw View」にすれば、今でも名前付きの Group の要素として見えるはずである。これは下の 2.1 の確認で裏付けを取る。）

## 2. 直すための進め方

### 2.1 まず再現と裏付け（Windows の実機。ユーザーか、Windows で動かせる人が行う）

1. 今の版を起動し、Inspect.exe で次の 2 点を見る。
   - 既定の Control ビューで、3 つの `Border` の名前が見えないこと（不具合の再現）
   - 「Options > Raw View」に切り替えると、3 つの要素が `ControlType: Group`、`IsControlElement: false`、`Name: <付けた名前>` で見えること（上の原因の裏付け）
2. 裏付けが取れなければ、原因の見立てが違うので、下の直し方に進まずに観察した結果を返してもらう（別の原因として、`Border` が見えていない、名前のバインディングが空、ウィンドウの UIA がそもそも出ていない、などを疑う）。

### 2.2 再現テストを先に足す（Red を確かめる）

画面のテスト（Avalonia.Headless）は使えないが、ピアの判定は Avalonia の公開 API で、ウィンドウを出さずに確かめられる見込みがある。`ControlAutomationPeer.CreatePeerForElement(control)` はピアを作る公開の static メソッドで、`IsControlElement()`、`IsContentElement()`、`GetName()` を持つ。期待値は「読み上げの名前が UIA に出る」という仕様から決まる（今の誤りの `false` をアサートしない）。

```csharp
// Shos.Minesweeper.Desktop.Tests
public class AccessibleNameExposureTests
{
    // 読み上げの名前を付けた Border は、UIA の Control/Content ビューに出る（出ないとスクリーン リーダーが読まない）
    [Theory]
    [InlineData("BoardFrame")]
    [InlineData("MineCounterFrame")]
    [InlineData("TimerFrame")]
    public void NamedFrameIsExposedToScreenReaders(string frameName)
    {
        var view = new GameView();                         // 盤面と表示を持つ UserControl（仮定。下の注）
        var frame = view.FindControl<Border>(frameName)!;

        var peer = ControlAutomationPeer.CreatePeerForElement(frame);

        Assert.True(peer.IsControlElement());
        Assert.True(peer.IsContentElement());
        Assert.False(string.IsNullOrWhiteSpace(peer.GetName()));
    }
}
```

- 直す前に流して、`IsControlElement()` が `false` で Red になることを確かめる。
- 注（仮定）: 3 つの `Border` が `Window` の XAML に直接あると、`Window` はプラットフォームなしでは作れない見込みが高い。その場合は、`UserControl` のビューがあればそれを作る。どちらも作れない（`InitializeComponent` や `VerifyAccess` が例外になる）なら、ここで構造を変えてまで（テストのためにビューを分けるなど）テストを置かない。それはテストのない状態での構造変更なので、ユーザーに確かめてからにする（testing.md の「既存コードにテストを後から足す」の 4）。そのときは、2.4 の実機の確認だけを検証とし、確かめられなかったことを報告に書く。
- `x:Name` の値（`BoardFrame` など）は仮の名前である。実際の XAML の名前に合わせる。名前がなければ、テストのために `x:Name` を付けるのは、振る舞いを変えない最小の変更として足してよい。

### 2.3 直す（最小の変更）

3 つの `Border` に `AutomationProperties.AccessibilityView="Content"` を足す。名前を付けた要素そのものを、UIA の Control/Content ビューに入れる変更である。

```xml
<!-- 盤面 -->
<Border x:Name="BoardFrame"
        AutomationProperties.Name="{Binding BoardAccessibleName}"
        AutomationProperties.AccessibilityView="Content">
    ...
</Border>

<!-- 残り地雷数 -->
<Border x:Name="MineCounterFrame"
        AutomationProperties.Name="{Binding MineCounterAccessibleName}"
        AutomationProperties.AccessibilityView="Content">
    <TextBlock Text="{Binding MineCounterText}" />
</Border>

<!-- 経過時間 -->
<Border x:Name="TimerFrame"
        AutomationProperties.Name="{Binding TimerAccessibleName}"
        AutomationProperties.AccessibilityView="Content">
    <TextBlock Text="{Binding TimerText}" />
</Border>
```

（バインディングの名前は仮のもので、既存のものはそのままにする。変えるのは `AccessibilityView` の 1 行ずつだけである。）

- `Control` ではなく `Content` にする理由: この 3 つは利用者に伝える情報を持つ要素なので、Content ビューにも入れる。表の 6、7 のとおり、`Content` は Control ビューも満たす。
- コントロールの種類は、`None` が UIA では `Group` になる（表の 5）。「名前の付いたまとまり」として読まれるので、今回は変えない。

### 2.4 確かめる

1. 2.2 のテストが Green になること。全テスト（`dotnet test`）が Green のままであること。
2. Windows の実機で、Inspect.exe の既定の Control ビューに、3 つの要素が名前付きで出ること（`IsControlElement: true`、`IsContentElement: true`）。
3. ナレーターで、Tab やスキャン モードで 3 つの要素に移ったときに、名前が読まれること。

## 3. 捨てた案

- **`Border` を継いだ部品を作り、`OnCreateAutomationPeer` を上書きする**: 同じ結果を得るのに、クラスとピアのクラスが増える。Avalonia に添付プロパティ（`AccessibilityView`）が用意されているので使わない。
- **`AutomationProperties.IsControlElementOverride="True"` を使う**: Control ビューには入るが、`IsContentElement` は `false` のままになる。情報を伝える要素には `AccessibilityView="Content"` の方が合う。
- **名前を中の `TextBlock` に移す**: `TextBlock` のピアは Control 要素なので出るようにはなるが、名前を付ける場所が変わる（仕様の「Border に名前」を変える）うえ、盤面の `Border` には合う中身がない。最小の変更ではない。
- **`Border` にスタイルで一括して `AccessibilityView` を付ける**: 飾りの `Border` まで UIA に出て、雑音になる。名前を付けた 3 か所だけに付ける。

## 4. 作らなかったもの・置いた仮定

- `AutomationProperties.ControlTypeOverride` による種類の変更、ライブリージョン（`LiveSetting`）による経過時間・残り地雷数の変化の自動の読み上げは入れていない。頼まれていない振る舞いの追加であり、仕様にあるかどうかを確かめてから扱う。
- ガード節は足さない。原因は前提条件の破れ（呼び出し元の誤り）ではなく、フレームワークの既定のピアの選び方なので、入口の検証で再発を検知する場所がない。再発の検知は 2.2 のテストが担う。
- 仮定: 使っている Avalonia は 12.1 の系列（12.1.0〜12.1.3）で、調べた 12.1.0 のソースと同じ振る舞いである。3 つの `Border` は `IsVisible = true` である。

## 5. 検証の結果

- 手元でアプリを動かせないので、ビルド、テスト、実機の確認は、どれも実行していない。上のコードは未検証である。
- 確かめたのは、Avalonia 12.1.0 のソースを読んで、原因の見立てと、`AccessibilityView="Content"` で `IsControlElement`・`IsContentElement` が `true` になる経路（表の 6）だけである。
- 2.2 のテストが、プラットフォームなしでビューを作れるかどうかは確かめていない（2.2 の注）。

## 6. 範囲外の気づき

- 同じ原因は、`Border` や `Panel`（`Grid`、`StackPanel` など）に `AutomationProperties.Name` を付けたほかの場所にも起きうる。XAML を `AutomationProperties.Name` で検索し、ピアを持たない要素に付けた箇所がないかを見るとよい（ここでは直さない）。

## 7. ユーザーの判断が要る点

- 2.1 の実機の確認（Inspect.exe、ナレーター）を、誰が Windows で行うか。
- 2.2 のテストがプラットフォームなしで動かないとき、テストのためにビューの構造を変えるか、実機の確認だけで済ませるか。
- 経過時間・残り地雷数が変わったときに、スクリーン リーダーが自動で読み上げるべきか（ライブリージョン）。今の仕様にない振る舞いなので、決めてもらう。
