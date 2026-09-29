# T8: 読み上げの名前が UI オートメーションの木に出ない不具合

使ったスキルの区分: 「判断を伴う作業」の「バグ修正」（原因の調査が要るため）。quality-gates.md の「バグ修正」「ガード節」を読み、再現テストを置く足場がないので testing.md の「既存コードにテストを後から足す」「テスト基盤がない場合」も読んだ。

## 1. What（何を直すか）

`Border` に付けた `AutomationProperties.Name` が、Windows の UI オートメーションのコントロール ビューとコンテンツ ビュー（Inspect.exe の既定の表示とスクリーン リーダーが見る木）に出るようにする。表示や操作は変えない。

## 2. 原因（調べたこと。推測ではなく、Avalonia のソースで確かめた）

スキルの「原因が外のフレームワークの振る舞いにあるときは、直し方を試す前に、その仕組みを調べる」に従い、当て推量で直し方を試さずに Avalonia のソース（タグ `12.1.0` と `master`。どのファイルをどちらで読んだかは末尾の注）を読んだ。

1. `Control.OnCreateAutomationPeer()` の既定は `new NoneAutomationPeer(this)` を返す。`Border` はこれをオーバーライドしていない（`Border : Decorator` で、自動化のコードはない）。
   出どころ: `src/Avalonia.Controls/Control.cs`、`src/Avalonia.Controls/Border.cs`
2. `NoneAutomationPeer` は `IsControlElementCore() => false`、`IsContentElementCore() => false`、コントロールの種類は `None` を返す。クラスの説明は「非対話的、またはアプリの論理構造に寄与しない要素」である。
   出どころ: `src/Avalonia.Controls/Automation/Peers/NoneAutomationPeer.cs`
3. `ControlAutomationPeer.IsControlElementOverrideCore()` は次のとおりで、`AccessibilityView` が `Default` のままだと 2 の `false` が使われる。
   ```csharp
   if (AutomationProperties.GetIsControlElementOverride(Owner) is { } isControlElement)
       return isControlElement;
   var view = AutomationProperties.GetAccessibilityView(Owner);
   return view == AccessibilityView.Default ? IsControlElementCore() : view >= AccessibilityView.Control;
   ```
   `IsContentElementOverrideCore()` も同じ形で `view >= AccessibilityView.Content` を見る。`GetNameCore()` は `AutomationProperties.GetName(Owner)` を返すので、**名前自体はピアに届いている**。
   出どころ: `src/Avalonia.Controls/Automation/Peers/ControlAutomationPeer.cs`
4. Windows の橋渡し（`AutomationNode`）は、UIA の `IsControlElement`・`IsContentElement` をそのまま `Peer.IsControlElement()`・`Peer.IsContentElement()` で答える。
   出どころ: `src/Windows/Avalonia.Win32.Automation/AutomationNode.cs`

したがって、名前の付いた `Border` は UIA の Raw ビューにだけ現れ、`IsControlElement = false` のため、Inspect.exe の既定（コントロール ビュー）にも、ナレーターなどが辿る木にも出ない。これが症状の原因である。ビューモデルのテストが Green なのは、原因がビューの XAML とフレームワークの既定値の組み合わせにあり、ビューモデルに関わらないためである。

補足（直し方の選択に効く事実）:
- `TextBlockAutomationPeer.GetNameCore()` は `Owner.Inlines?.Text ?? Owner.Text` で、`AutomationProperties.Name` を見ない。名前を中の `TextBlock` に移す直し方は効かない。
  出どころ: `src/Avalonia.Controls/Automation/Peers/TextBlockAutomationPeer.cs`
- `AutomationProperties` には `AccessibilityView`（Default / Raw / Control / Content）と `ControlTypeOverride`、`IsControlElementOverride` がある。公式文書も、ピアが不十分な要素は `AccessibilityView` / `ControlTypeOverride` で補い、それで足りなければ `OnCreateAutomationPeer` をオーバーライドする、としている。
  出どころ: `src/Avalonia.Controls/Automation/AutomationProperties.cs`、https://docs.avaloniaui.net/docs/app-development/accessibility

## 3. 直し方（コードの変更）

### 置いた仮定
- 名前は各 `Border` に XAML で `AutomationProperties.Name="{Binding ...}"` のように付いている。
- 残り地雷数・経過時間の `Border` の名前は、値を含む文（例:「残り地雷 10 個」「経過 25 秒」）で、中の `TextBlock` は数字だけを表示している。
- 盤面の `Border` は、マス（ボタンなど）を子に持つ入れ物である。

### 変更（最小の変更）
名前を付けた 3 つの `Border` に、`AccessibilityView="Content"`（コントロール ビューとコンテンツ ビューの両方に入る）と、読み上げで伝える種類 `ControlTypeOverride` を足す。

```xml
<!-- 盤面: マスを束ねる入れ物として出す -->
<Border AutomationProperties.Name="{Binding BoardAccessibleName}"
        AutomationProperties.AccessibilityView="Content"
        AutomationProperties.ControlTypeOverride="Group">
    ...マス...
</Border>

<!-- 残り地雷数: 値を含む名前の文字列として出す -->
<Border AutomationProperties.Name="{Binding RemainingMinesAccessibleName}"
        AutomationProperties.AccessibilityView="Content"
        AutomationProperties.ControlTypeOverride="Text">
    <!-- 名前と同じ数字を二重に読ませないため、中の数字は Raw ビューだけに置く -->
    <TextBlock Text="{Binding RemainingMinesText}"
               AutomationProperties.AccessibilityView="Raw" />
</Border>

<!-- 経過時間: 残り地雷数と同じ形 -->
<Border AutomationProperties.Name="{Binding ElapsedTimeAccessibleName}"
        AutomationProperties.AccessibilityView="Content"
        AutomationProperties.ControlTypeOverride="Text">
    <TextBlock Text="{Binding ElapsedTimeText}"
               AutomationProperties.AccessibilityView="Raw" />
</Border>
```

（バインドの名前は仮のもので、既存の名前をそのまま使う。）

### Why（選んだ案と捨てた案）
- **選んだ案: XAML の添付プロパティ**。フレームワークが用意した口（`AccessibilityView`、`ControlTypeOverride`）で、ピアの既定を要素ごとに上書きする。新しい型を作らず、ビューモデルも変えない。
- 捨てた案 1: `Border` を継承して `OnCreateAutomationPeer` で `ControlAutomationPeer` を返す独自のコントロール。同じことが添付プロパティでできるので、読む対象（新しいクラス）を増やすだけである（引き算）。
- 捨てた案 2: 名前を中の `TextBlock` に移す。上の補足のとおり、`TextBlockAutomationPeer` は `AutomationProperties.Name` を無視するので効かない。
- 捨てた案 3: `IsControlElementOverride="True"` だけを付ける。コントロール ビューには入るが、コンテンツ ビューには入らない。値を伝える要素なので、両方に入る `Content` にした。
- 中の `TextBlock` を `Raw` にするのは、`Border` を木に出すと、中の数字（`TextBlock` の `Text` が名前になる）も並んで読まれ、同じ値が二重に読まれるためである。2 の仮定（名前が値を含む）が外れて、名前が「残り地雷」のように見出しだけなら、この行は付けずに、中の `TextBlock` に値を読ませる方が合う。

### 作らなかったもの
- 独自のオートメーション ピアのクラス、共通のスタイルやテンプレートへのまとめ（3 か所で、同じ形の分岐が増えているわけではないため）。
- 値が変わったときに自動で読み上げる仕組み（`AutomationProperties.LiveSetting`）。症状は「名前が木に出ない」ことで、変化の通知は頼まれていない。経過時間を毎秒読み上げるとうるさいので、入れるかは仕様の判断である（下の 5 章）。

## 4. 進め方と検証

アプリを手元で動かせず、画面のテスト（Avalonia.Headless.XUnit）も使えないので、次の順で進める。

1. **再現テストを先に書き、Red を確かめる（可能なら）。** ビューモデルのテストのプロジェクトで、Avalonia のアプリを起動せずに、ピアの判定だけを確かめるテストを試す。
   ```csharp
   [Fact]
   public void NamedBorderIsExposedToControlAndContentViews()
   {
       var border = new Border();
       AutomationProperties.SetName(border, "残り地雷 10 個");
       AutomationProperties.SetAccessibilityView(border, AccessibilityView.Content);

       var peer = ControlAutomationPeer.CreatePeerForElement(border);

       Assert.True(peer.IsControlElement());
       Assert.True(peer.IsContentElement());
       Assert.Equal("残り地雷 10 個", peer.GetName());
   }
   ```
   - Red の確かめ方: `SetAccessibilityView` の行を外すと `IsControlElement()` が `false` で失敗することを見て、戻す。これで、原因の理解（`NoneAutomationPeer` の既定）がテストで裏づけられる。
   - 限界: このテストは「その設定なら木に出る」ことを確かめるだけで、実際の XAML に設定が書かれていることは確かめない。ビューの XAML を読み込むテストは、Avalonia の初期化が要り、Headless の枠組みが使えないこのプロジェクトでは足場がない。`Border` の生成でもスレッド（`VerifyAccess`）や初期化で失敗するなら、このテストは置かない。
   - 足場を作る案（Avalonia.Headless を xUnit の統合なしで `HeadlessUnitTestSession` から使う、など）は、テストの仕組みの導入に当たるので、ユーザーの確認を得るまで行わない。
2. **XAML を 3 章のとおりに直す。** ビルドし、全テストが Green であることを確かめる。
3. **実行環境で確かめる（ユーザーに依頼する。実際の木はここでしか確かめられない）。** Windows 11 で発行したアプリを動かし、次を見る。
   - Inspect.exe（既定のコントロール ビュー）か Accessibility Insights for Windows で、3 つの要素が出て、`Name`、`ControlType`（Group / Text）、`IsControlElement = true`、`IsContentElement = true` であること。
   - 残り地雷数・経過時間の中の数字が別の要素として重ねて出ていないこと。
   - ナレーターのスキャン モードで、3 つが読み上げられ、数字が二重に読まれないこと。
   - 旗を立てて残り地雷数が変わった後、要素を選び直すと新しい値が読まれること（名前はバインドで変わるが、UIA への変化の通知が出るかは未確認のため）。
4. 周りで見つけたことは直さずに報告する。

### 検証結果（この課題の中で行ったこと）
- 行った: Avalonia 12.1.0 のソースと公式文書で原因と直し方の仕組みを確かめた（2 章）。
- 行っていない: ビルド、テストの実行、アプリの実行。手元にプロジェクトもアプリもないため。1 のテストが Avalonia の初期化なしに動くかも未確認である。

## 5. ユーザーの判断が要る点
- 再現テストの足場: 1 のテストが動かないとき、Avalonia.Headless を xUnit の統合なしで使う足場を入れるか（ツールの導入に当たる）。
- 読み上げの種類: 盤面を `Group` にしたが、マスの並びを表として伝えたいなら `DataGrid`/`Table` の方が合う場合がある（そのときはマスの側に行・列の情報が要り、変更が大きくなる）。
- 値が変わったときの自動の読み上げ（`LiveSetting`）: 残り地雷数には `Polite` が役に立つが、経過時間に付けると毎秒読み上げられる。入れるかは仕様で決める。

## 6. 範囲外の気づき
- 同じ原因が、名前を付けた他の非対話的な要素（`Panel`、`Grid`、`StackPanel`、`Image` などの既定の `NoneAutomationPeer` の要素）にもありうる。名前を付けた要素を XAML で一覧にし、同じ設定が要るかを見るとよい。放置すると、画面を足すたびに同じ不具合が入る。

## 出どころ
- Avalonia 12.1.0 のソース（GitHub、タグ `12.1.0` と `master`）:
  - https://github.com/AvaloniaUI/Avalonia/blob/12.1.0/src/Avalonia.Controls/Control.cs
  - https://github.com/AvaloniaUI/Avalonia/blob/12.1.0/src/Avalonia.Controls/Border.cs
  - https://github.com/AvaloniaUI/Avalonia/blob/12.1.0/src/Avalonia.Controls/Automation/Peers/NoneAutomationPeer.cs
  - https://github.com/AvaloniaUI/Avalonia/blob/12.1.0/src/Avalonia.Controls/Automation/Peers/ControlAutomationPeer.cs
  - https://github.com/AvaloniaUI/Avalonia/blob/12.1.0/src/Avalonia.Controls/Automation/Peers/TextBlockAutomationPeer.cs
  - https://github.com/AvaloniaUI/Avalonia/blob/12.1.0/src/Avalonia.Controls/Automation/AutomationProperties.cs
  - https://github.com/AvaloniaUI/Avalonia/blob/12.1.0/src/Windows/Avalonia.Win32.Automation/AutomationNode.cs
- Avalonia の文書「Accessibility」: https://docs.avaloniaui.net/docs/app-development/accessibility
- 注: `Control.cs`、`Border.cs`、`NoneAutomationPeer.cs`、`AutomationProperties.cs` は `master` で読み、`TextBlockAutomationPeer.cs`、`AutomationNode.cs` は `12.1.0` のタグで読んだ。`ControlAutomationPeer.cs` は両方で読み、判定は同じだった。`master` で読んだファイルの 12.1.0 での内容は、上のリンクのタグで確かめるとよい。
