# マインスイーパー デスクトップ版・コンソール版 調査書

| 項目 | 内容 |
|------|------|
| 工程 | 1. 調査書作成（デスクトップ版・コンソール版の一巡） |
| 作成日 | 2026-09-27 |
| 状態 | 作成した（レビュー前）。提出の後、ユーザーの決定で、デスクトップ版の技術を WPF から Avalonia UI に改め、1.1、2 章、4 章、6〜9 章を書き直した（2026-09-27） |
| 入力 | CLAUDE.md（目的・設計方針）、Web 版の調査書（docs/01-research.md）と仕様書（docs/02-spec.md）、Web 版 1.1.0 のコード |

## 1. 目的と範囲

Web 版（1.1.0）と同じゲームを、デスクトップ版とコンソール版として作る（CLAUDE.md の「目的」）。この調査書では、次を調べて整理する。

- デスクトップ版の技術の選定の経緯（1.1）
- Web 版から、そのまま使えるもの、Web アプリに残っていて共有を検討するもの、見直しが要る仕様（2 章）
- デスクトップと端末のマインスイーパーやゲームの慣習（3 章）
- Avalonia UI で作るときの技術的な考慮点（4 章）
- コンソールで作るときの技術的な考慮点（5 章）
- 配布と公開（6 章）、リポジトリとビルドへの影響（7 章）

ゲームのルール、定番の難易度と機能、アルゴリズム（Web 版の調査書の 2〜6 章）は、UI の技術によらずそのまま当てはまるので、ここでは繰り返さない。

この調査書では、1.1 のユーザーの決定のほかは決定を行わない。仕様書で決めるべき論点を 8 章に、アーキテクチャー設計以降で決めることを 9 章にまとめる。

### 1.1 デスクトップ版の技術の選定

最初の版の調査書は、WPF で作る前提で書いた。提出の後、ユーザーが次の条件を示した。

- Linux の上で、ビルド、テスト、公開（発行）ができること
- Windows のデスクトップ アプリとして動くこと

WPF は Windows でしか動かない。Linux の上でも `EnableWindowsTargeting` を付ければビルドはできる（Microsoft Learn の NETSDK1100）が、WPF に依存するテストは実行できない。そこで、WPF 以外の方法を比べた。

| 方法 | 条件に合うか | 主な特徴 |
|------|--------------|----------|
| Avalonia UI | 合う | XAML と MVVM で書き、WPF に近い。画面のテストを、ディスプレイのない Linux で動かせる（Avalonia.Headless.XUnit）。Windows では UI オートメーションでスクリーンリーダーに対応する。フレームワークは MIT ライセンスで無料 |
| Photino.Blazor | 合う | Web 版の画面を WebView2 の中でほぼそのまま使える。ただし、中身は Web 版をウィンドウに入れたものになる。元のプロジェクトとは別に、保守をうたうフォークが出ている |
| Uno Platform（`net10.0-desktop`） | 合う | WinUI の API で書き、Skia で描く。SDK が大きく、覚えることが多い |
| WinForms、WinUI 3 | 合わない | WPF と同じく Windows 専用 |
| .NET MAUI | 合わない | Windows 向けのビルドに Windows が要る |
| BlazorWebView | 合わない | WPF、WinForms、MAUI の中で動く部品で、それらの制約を受ける |
| ゲームの描画ライブラリ（MonoGame、Raylib-cs など） | ビルドは合う | 標準の部品とスクリーンリーダーへの対応がなく、Web 版のアクセシビリティの目標（WCAG 2.2 AA）に合わない |

**ユーザーは Avalonia UI を選んだ（2026-09-27）。** 以後、この一巡では、Avalonia UI で作る版を「デスクトップ版」と呼ぶ。Avalonia の有料の製品（Plus、Pro の開発ツールと追加の部品、WPF を動かす XPF）は使わない。

## 2. Web 版から引き継ぐもの

### 2.1 そのまま使える部品

1.1.0 までに、UI の技術に依存しない部品を二つのプロジェクトに切り出した。どちらも `net10.0` のクラスライブラリで、デスクトップ版からもコンソール版からも参照できる（どちらも `net10.0` を対象にできる）。

| プロジェクト | 部品 | デスクトップ版・コンソール版での使い方 |
|--------------|------|----------------------------------------|
| GameLogic | ゲームのルール（`Game`、`Board`、`Difficulty`、カスタムの検証、`MoveResult`）、ベストタイム（`BestTimes`）と保存の形式（`BestTimesJson`） | そのまま使う。経過時間は `TimeProvider` から計算するので、画面の更新の間隔によらない |
| Presentation | 1 回のゲームの進め方（`GameSession`） | そのまま使う。音の出口（`SoundEffectOutput`）は、デスクトップ版は渡し、コンソール版は渡さない |
| Presentation | 効果音の種類、鳴らす音を決める表（`SoundEffectMapping`）、波形の合成（`SoundEffectSynthesizer`） | デスクトップ版が使う。合成の結果は、44,100 Hz、モノラル、-1〜1 の `float` の配列である。再生の方法に合わせて、WAV（16 ビットの PCM など）に変換する必要がある（4.6） |
| Presentation | 入力された文字列の整え方（`InputText`） | そのまま使う。ただし、発行の設定によって正規化が効かなくなる（5.9。デスクトップ版にも当てはまる） |
| Presentation | 押し方からの操作の割り当て（`PressMapping`） | デスクトップ版のマウス（左クリックと右クリック）で使える。タッチに対応するなら、長押しも使える。コンソール版は押し方がないので使わない |
| Presentation | 難易度の表示名（`DifficultyNames`）、読み上げの文（`Announcements`） | そのまま使える。読み上げの文は、デスクトップ版では UI オートメーションのライブ リージョン（4.5）に、コンソール版では状態の行などに使える |

### 2.2 Web アプリに残っていて、共有を検討するもの

CLAUDE.md の「目的」は、形が UI の設計に左右されるものは、この一巡で要るものが見えてから移す、としている。Web アプリに残っているもののうち、候補になるものを挙げる。移すかどうかはアーキテクチャー設計で決める（9 章）。

| 部品 | 今の置き場所と形 | 共有するときの論点 |
|------|------------------|--------------------|
| キーの割り当て（`KeyboardMapping`） | DOM の `KeyboardEvent.key` の文字列を受け取る | 規則（Space・Enter で開く、F で旗、矢印で移動）は仕様書 4.5 で、どの版も同じにできる。キーの型は UI ごとに違う（Avalonia の `Key`、コンソールの `ConsoleKey`）ので、共有するなら、規則とキーの型を分ける必要がある |
| 選択しているマス（`BoardCursor`）と方向（`Direction`） | 盤面の置き方（`BoardPlacement`）を使い、表示の向きで動かす | 盤面を入れ替えて表示しない版では、置き方に依存しない形が要る |
| 盤面の置き方（`BoardPlacement`） | CSS の px で、マスの大きさと入れ替えを決める | デスクトップ版でウィンドウの大きさに合わせるなら、同じ規則を使える（単位は Avalonia の論理的な単位）。コンソール版は文字の升目なので、規則が違う |
| 押し方の判定（`PressGesture`） | 1 回の「押して離す」を、タップ・長押し・右クリックに判定する（400 ミリ秒、10px 動いたら取り消す）。入力は DOM のポインターの値（種類の文字列、ボタンの番号） | Avalonia のポインターのイベントも、マウス・タッチ・ペンを 1 つの仕組みで扱う（4.3）。デスクトップ版で Web 版と同じ長押しにするなら、入力の型を DOM から切り離せば使える |
| マスの読み上げの名前（`CellPresentation` の一部） | 「3 行 5 列、未開放」などの文を作る。同じクラスに CSS のクラス名もある | 文を作る部分は、デスクトップ版の UI オートメーションの名前に使える。CSS のクラス名の部分は Web アプリに残す |
| 演出の順序（`BoardAnimation`） | 直前の操作から、マスごとの演出と遅れの比を決める。`Game` と `MoveResult` だけに依存する | 見せ方によらない規則なので、デスクトップ版で演出を作るなら使える |
| Razor に直接書いた文言 | 勝利カード（「クリア！」「タイム N 秒」、ベストタイムの行）、ツールバーの名前（「残り地雷 N」「経過時間 N 秒」「新しいゲーム」「旗モード」「効果音」）、難易度ダイアログ（「幅」「高さ」「地雷数」、範囲の「1〜（幅×高さ − 9）」、「ベスト N 秒」「記録なし」） | 各版が同じ文言を書くと、同じ意図が重複する。一方、文言の組み立て方は画面の形に左右される |
| 保存（`BestTimeStorage`、`SoundSettingStorage`） | ブラウザーの localStorage | 保存先は版ごとに違う（4.8、5.8）。形式は `BestTimesJson` を共有済みである |

### 2.3 Web 版の仕様のうち、見直しが要るもの

Web 版の仕様書（docs/02-spec.md）の各節が、デスクトップ版とコンソール版にそのまま当てはまるかを次に示す。「論点」は 8 章の論点の番号である。

| Web 版の仕様書 | 内容 | デスクトップ版 | コンソール版 |
|----------------|------|----------------|--------------|
| 3 章（3.1 の入力を除く） | 難易度、ゲームの状態、地雷の配置、コード、勝敗の表示、ベストタイムの規則 | そのまま | そのまま。表示の方法は論点 15 |
| 3.1 の入力された文字列 | 前後の空白を除き、NFKC で整える | そのまま（`InputText`）。実行環境の注意がある（5.9） | そのまま。実行環境の注意がある（5.9） |
| 4.1 操作の割り当て | クリック、右クリック、長押し | マウスはそのまま。タッチは論点 5 | 当てはまらない（論点 14） |
| 4.2〜4.4 タッチ、旗モード、長押し | タッチの端末のための仕様 | 論点 5、6 | 当てはまらない |
| 4.5 キーボード操作 | 矢印、Space・Enter、F、Tab | そのまま使える。足すキーは論点 11 | 論点 13 |
| 5.1 画面の構成 | 画面は 1 つで、ページの移動はない | 論点 11 | 論点 17 |
| 5.2、5.3 盤面の大きさと回転 | 画面に収め、必要なら縦と横を入れ替える | 論点 7 | 論点 15、16 |
| 5.4 配色、ダークモード、動きの抑制 | OS の設定に合わせる | 論点 9 | 論点 15（端末の配色に従う） |
| 5.5 言語 | 日本語だけ | そのまま | 論点 22（端末の文字の幅） |
| 5.6 効果音 | 既定で鳴らす、消せる、重ねて鳴らす | 論点 10 | 当てはまらない（鳴らさない。CLAUDE.md の「目的」） |
| 5.7 演出 | 連鎖の波、負けと勝ちの演出 | 論点 8 | 論点 20 |
| 6.1 動作環境 | 各 OS のブラウザー | 論点 2 | 論点 3 |
| 6.2 性能 | 操作から 100 ミリ秒以内。読み込み中の表示 | 論点 25 | 論点 25 |
| 6.3 アクセシビリティ | WCAG 2.2 AA、表として読み上げ、勝敗の読み上げ | 論点 12 | 論点 19 |
| 6.4 データとプライバシー | 外部と通信しない。保存するのはベストタイムと効果音の設定 | そのまま。保存先は論点 21 | そのまま。保存先は論点 21 |
| 7 章 公開 | GitHub Pages | 論点 26〜29 | 論点 26〜29 |
| 9 章 対象外 | ？マーク、統計、途中経過の保存など | 論点 23 | 論点 23 |

## 3. デスクトップと端末の慣習

### 3.1 Windows のマインスイーパー

デスクトップ版の利用者が思い浮かべるのは、Windows に付いていたマインスイーパーである。次の点が定番として知られている（出典未確認。記憶と一般的な説明による）。

- メニューの「ゲーム」から、新しいゲーム、難易度（初級・中級・上級・カスタム）、ベストタイム（統計）、オプションを選ぶ。新しいゲームのショートカットは F2 である。
- ウィンドウの大きさは盤面に合わせて決まり、盤面をウィンドウに合わせて拡大・縮小することはしない（Windows 7 版は、ウィンドウの大きさを変えるとマスも大きくなる）。
- Windows 7 版には、効果音と、アニメーションのオン・オフ、途中のゲームを保存して続きから遊ぶ設定がある。効果音を消せることは Web 版の調査書 5.5 で確かめた（Microsoft Q&A）。

### 3.2 端末で動くゲームの慣習

端末で動く、画面全体を使うプログラム（エディターやゲームなど）には、次の慣習がある（出典未確認。一般的な作りによる）。

- 操作はキーボードだけで行う。移動は矢印キーのほか、vi の h・j・k・l を使えることが多い。終了は q、ヘルプは ? が多い。
- 画面全体を使うときは、端末の「代替画面」に切り替えて描き、終わったら元の画面に戻す。終了した後の端末に、ゲームの画面が残らない。
- 色は補助にとどめ、色を出さない設定を尊重する。環境変数 `NO_COLOR` が空でない値で設定されていたら、色を付けないという約束がある（no-color.org）。

## 4. Avalonia UI で作るときの考慮点

### 4.1 対象と前提

- Avalonia のアプリは `net10.0` を対象にできる（WPF の `net10.0-windows` のような OS 専用の対象は要らない）。そのため、Linux の上でもビルドとテストができる（1.1）。
- 同じアプリが Windows、macOS、Linux で動く。どの OS を動作環境にするかは論点 2 とする。
- フレームワークは MIT ライセンスである。NuGet の最新の安定版は 12 系である（Avalonia.Headless.XUnit 12.0.4 など）。

### 4.2 画面の作り方

#### 画面の構造

- Avalonia でも、WPF と同じく、画面（XAML）と、画面の状態と操作（ビューモデル）を分ける MVVM の形が一般的である。ビューモデルは Avalonia のコントロールに依存しないので、xUnit でテストできる。
- XAML のバインディングは、ビルドの時にコンパイルする「コンパイル済みのバインディング」（`x:DataType` を付ける）を使える。トリミングや Native AOT（6.1）で発行するには、これが要る（Avalonia Docs）。
- MVVM の定型の記述を減らすライブラリ（CommunityToolkit.Mvvm など）があるが、使わなくても書ける。ライブラリの導入はユーザーの確認が要る（9 章）。

#### 盤面の描き方

盤面のマスは、上級で 480、カスタムの最大（30×24）で 720 ある。描き方には次の二通りがある。

| 方法 | 長所 | 短所 |
|------|------|------|
| マスごとにコントロールを置く（`ItemsControl` と `UniformGrid` など） | 見た目、フォーカス、UI オートメーションの名前を、マスごとに標準の仕組みで付けられる | 要素が多い。720 のマスで描き直しの速さが足りるかは、実装で確かめる |
| 盤面を 1 つのコントロールとして自分で描く | 描画は軽い | マスごとのフォーカスと UI オートメーションを、自分で作る必要がある |

### 4.3 入力

- **マウス・タッチ・ペン**: Avalonia は、どれも「ポインター」として同じイベント（`PointerPressed`、`PointerReleased` など）で届ける（Avalonia Docs）。Web 版のポインターのイベントと同じ考え方なので、左クリック・右クリックは `PressMapping` で、長押しは Web 版の押し方の判定（2.2 の `PressGesture`）の規則で扱える。
- **長押しのジェスチャ**: Avalonia には長押しのイベント（`Holding`）もある。コントロールごとに `IsHoldingEnabled` で有効にし、マウスで起こすには `IsHoldWithMouseEnabled` も要る。長押しと見なすまでの時間は、プラットフォームの設定（`HoldWaitDuration`）で決まる（Avalonia Docs）。Web 版の 400 ミリ秒や、長押しの進行の表示（Web 版の仕様書 4.4）に合わせるなら、このイベントではなく自分で判定する。`Holding` が起きないという報告もある（AvaloniaUI/Avalonia の issue #17208）。タッチにどこまで対応するかは論点 5 とする。
- **キーボード**: `KeyDown` のイベントで、`Key` の値として受ける。Tab でのフォーカスの移動は、Avalonia の標準の仕組みで行える。新しいゲームの F2（3.1）など、デスクトップのアプリの慣習のキーを足すかは論点 11 とする。

### 4.4 表示

#### 高 DPI

- Avalonia は、論理的な単位で描き、Windows では既定でモニターごとの DPI に合わせる（Avalonia Docs）。WPF のようにマニフェストで宣言する必要はない。

#### ウィンドウの大きさと盤面

- Windows の定番（3.1）は、盤面に合わせてウィンドウの大きさを決める。Web 版は、表示の領域に合わせてマスの大きさを決め、縦と横を入れ替える（Web 版の仕様書 5.2）。デスクトップ版でどちらにするかは論点 7 とする。
- ウィンドウに合わせる場合、`Viewbox` で盤面全体を拡大・縮小する方法と、`BoardPlacement` と同じ規則でマスの大きさを計算する方法がある。

#### ダークモードとハイコントラスト

- Avalonia の標準のテーマ（Fluent）は、既定で OS のライト・ダークの設定に従い、OS の設定が変わると切り替わる（Avalonia Docs）。WPF の `ThemeMode` のような試験的な API ではない。
- OS の設定は、プラットフォームの設定（`PlatformSettings.GetColorValues()`）で読める。ライトかダークか（`ThemeVariant`）と、ハイコントラストを求めているか（`ContrastPreference`）が分かり、変わったときは `ColorValuesChanged` のイベントが起きる（Avalonia Docs）。

#### 動きを減らす設定

- **Avalonia は、OS の「アニメーション効果」の設定を読む API を持っていない。** Avalonia の標準の部品も、この設定に従っていない（AvaloniaUI/Avalonia の issue #19405。未解決）。
- Windows では、Win32 の `SystemParametersInfo`（`SPI_GETCLIENTAREAANIMATION`）で読める（Microsoft Learn）。デスクトップ版で演出を作るなら、OS ごとに自分で読む必要がある。
- 演出（Web 版の仕様書 5.7）は、Avalonia のアニメーションと遷移（`Transitions`）で作れる。どこまで作るかは論点 8 とする。

### 4.5 アクセシビリティ

- Avalonia の各コントロールには、スクリーンリーダーに役割や状態を伝える「オートメーション ピア」がある。Windows では UI オートメーション、macOS では NSAccessibility、Linux では AT-SPI に伝わり、ナレーターや NVDA が読める。標準のコントロールには最初から付いている（Avalonia Docs）。
- マスの名前は `AutomationProperties.Name` で付けられる。自分で描くコントロール（4.2）では、オートメーション ピアを自分で作る。
- 勝敗などの知らせは、Web 版の `aria-live` に当たる `AutomationProperties.LiveSetting`（Off、Polite、Assertive）で行える（Avalonia Docs）。Windows のナレーターで実際に読み上げられるかは確かめていない。実装の最初に確かめる。
- 盤面を表（行と列）として読み上げさせるには、UI オートメーションの Grid と GridItem のパターンを返す必要がある。Avalonia の標準のコントロールでこれを返せるかは確かめていない。返せなければ、オートメーション ピアを自分で作る。どこまで対応するかは論点 12 とする。

### 4.6 効果音

Avalonia には、音を鳴らす仕組みがない。Web 版の仕様書 5.6 の要求（1 回の操作で 1 つ、続けて操作したときは前の音を止めずに重ねて鳴らす、既定で鳴らす、消した設定を保つ）と照らすと、次の方法がある。

| 方法 | 動く OS | 重ねて鳴らせるか | そのほか |
|------|---------|------------------|----------|
| `System.Media.SoundPlayer`（NuGet の System.Windows.Extensions） | Windows だけ | 鳴らせない（同時に 1 つ。別の音を鳴らすと前の音が止まる。開発者の記事による） | WAV を含む `Stream` を渡せる。ビルドはどの OS でもでき、Windows 以外で呼ぶと動かない（確かめていない） |
| NAudio の再生の部品（NAudio.WinMM、NAudio.Wasapi） | Windows だけ | 鳴らせる（音を混ぜる部品がある） | PCM の波形を直接渡せる。パッケージは .NET Standard 2.0 で、どの OS でもビルドできるが、再生の部品は Windows 専用である（NAudio のリポジトリ） |
| OS をまたぐ音のライブラリ（OpenAL のバインディングなど） | Windows、macOS、Linux | ライブラリによる | macOS や Linux でも鳴らすなら要る。ライブラリと、OS ごとのネイティブのライブラリを選ぶ必要がある（調べていない） |

- WPF の `MediaPlayer` は WPF の一部なので、使えない。
- `SoundPlayer` だけで作ると、Web 版の仕様書 5.6 の「重ねて鳴らす」を満たせない。デスクトップ版で「重ねて鳴らす」を求めるかは論点 10 とする。どの方法で鳴らすかは、動作環境（論点 2）にも左右される。
- ライブラリの導入はユーザーの確認が要る。開発者の記事による点は、実装のときに実際に鳴らして確かめる（CLAUDE.md の「実行環境での確認」）。
- Windows には iOS の消音スイッチに当たるものはない。音量は、OS の音量ミキサーでアプリごとに変えられる。

### 4.7 タイマー

- 1 秒ごとの表示の更新は、UI のスレッドで動く Avalonia の `DispatcherTimer` で行える。
- 経過時間は `Game.ElapsedSeconds`（`TimeProvider` から計算）を読むので、タイマーの間隔がずれても表示はずれない。Web 版と同じである。

### 4.8 保存

- ベストタイムと効果音の設定は、利用者ごとのアプリのデータの場所（`Environment.SpecialFolder.LocalApplicationData`）の下に、ファイルとして置くのが一般的である。場所は OS によって違う（5.8）。形式は `BestTimesJson` を使う。
- 書き込みに失敗しても遊べるようにする（Web 版の仕様書 3.8 と同じ考え方）。
- デスクトップ版とコンソール版で同じファイルを使えば、同じ PC ではベストタイムを共有できる。その場合、二つを同時に動かすと、後から書いたほうが先の記録を上書きするおそれがある。共有するかは論点 21 とする。

### 4.9 テスト

- ビューモデルと、UI に依存しない部品は、xUnit でテストできる。
- 画面のテストには、Avalonia の「ヘッドレス」の仕組み（Avalonia.Headless.XUnit）を使える。ウィンドウを画面に出さずに Avalonia を動かすので、ディスプレイのない Linux の CI でも実行できる。テストには `[Fact]` の代わりに `[AvaloniaFact]` を付け、テストのプロジェクトに `[AvaloniaTestApplication]` を 1 つ置く（Avalonia Docs）。最新版（12.0.4）は xUnit v3 に対応している（NuGet）ので、このリポジトリのテストの構成（xUnit v3、Microsoft.Testing.Platform）と合う見込みである。Microsoft.Testing.Platform で動くかは確かめていない。
- Web 版では bUnit でコンポーネントを確かめた。デスクトップ版では、ヘッドレスの仕組みで同じことができる見込みである。どこまで自動でテストするかは、9 章で決める。
- ヘッドレスは本物の描画と OS とのやり取り（DPI、スクリーンリーダー、音）を通らないので、それらは実機で確かめる。

### 4.10 例外

- UI のスレッドで捕まえなかった例外は、`Dispatcher.UIThread.UnhandledException` で受けられる。受けたことにしても、アプリが安全に続けられるとは限らない（Avalonia Docs）。macOS でこのイベントが起きないという報告がある（AvaloniaUI/Avalonia の issue #17759）。
- 1.1.0 では、設計で決めた境界の外に例外が出る不具合（B1。効果音の例外の受け止め漏れ）があった（docs/reviews/code-review.md）。デスクトップ版でも、例外の境界（どこで受け止め、利用者に何を見せるか）を設計で決める。

## 5. コンソールで作るときの考慮点

### 5.1 対象と前提

- コンソールのアプリは `net10.0` を対象にし、Windows、macOS、Linux で動かせる。
- 動かす端末は OS ごとに違う。Windows 11 22H2 から、コンソールのアプリは既定で Windows Terminal の中で動く（設定で従来のコンソール ホストに戻せる。Microsoft の開発者ブログ）。macOS ではターミナル（Terminal.app）や iTerm2、Linux では多くの端末がある。
- どの OS と端末を対象にし、誰がどの実機で確かめるかは論点 3 とする。

### 5.2 入力

- キーは `Console.ReadKey(intercept: true)` で、画面に文字を出さずに 1 つずつ受け取れる。キーが来ているかは `Console.KeyAvailable` で分かる。
- **`System.Console` にはマウスの API がない。** マウスを使うには、Windows ではコンソールの API（`ReadConsoleInput`）を直接呼び、Unix では端末のマウスの報告（VT のエスケープ シーケンス）を自分で解釈する必要がある。`System.Console` もコンソールの入力を読むので、自前の読み取りと両立させる工夫が要る（Microsoft Learn の Q&A、microsoft/terminal の issue）。端末の UI のライブラリ（Terminal.Gui など）は、これを引き受けてマウスに対応している。マウスに対応するかは論点 14 とする。
- カスタムの幅・高さ・地雷数のような文字の入力は、1 行を読む（`Console.ReadLine`）か、自分で入力欄を作る。日本語の入力（IME）がオンのままだと全角の数字が入るので、`InputText` で整える（Web 版の仕様書 3.1）。
- Ctrl+C は、既定ではプログラムを止める。`Console.CancelKeyPress` で受けるか、`Console.TreatControlCAsInput` でキーとして受けて、端末の状態（5.3）を元に戻してから終える必要がある。

### 5.3 画面の描き方

- カーソルの位置（`Console.SetCursorPosition`）、文字の色（`Console.ForegroundColor`、16 色）、カーソルの表示（`Console.CursorVisible`）は、`System.Console` で扱える。
- 24 ビットの色や代替画面（3.2）などは、VT のエスケープ シーケンスを出力して使う。Windows では、出力の VT の解釈（`ENABLE_VIRTUAL_TERMINAL_PROCESSING`）を有効にする必要がある場合がある（Microsoft Learn）。.NET が自動で有効にするかは確かめていない。実装のときに、Windows Terminal と従来のコンソール ホストの両方で確かめる。
- 画面を消してから描き直す（`Console.Clear`）と、ちらつく。変わった部分だけを書く、または 1 画面分をまとめて書く必要がある。書き込みの回数が多いと、端末によっては遅い（確かめていない）。
- 終了するとき（例外や Ctrl+C を含む）は、色、カーソルの表示、代替画面を元に戻す。戻さないと、利用者の端末が使いにくい状態で残る（4.10 と同じく、例外の境界の問題である）。

### 5.4 文字の幅

- 端末は文字を升目に並べる。ASCII の文字は 1 升、日本語の漢字や仮名、全角の文字は 2 升を使う。
- Unicode で幅が「あいまい」（East Asian Width が Ambiguous）とされる文字（■□●○★※①Φ など）は、端末とその設定によって 1 升にも 2 升にもなる。Windows Terminal は既定で 1 升、従来のコンソール ホストは日本語などのフォントでは 2 升にする（microsoft/terminal の issue #370、#14702 などの報告による）。絵文字の幅も端末によって違う。
- マスの記号にあいまいな幅の文字を使うと、端末によって盤面の列がずれる。マスの記号は、ASCII の文字か、幅が 2 升と決まっている文字（全角の英数字や記号）から選ぶ必要がある。
- 盤面の外の日本語の文言も 2 升を使うので、並べるときに幅を数える必要がある。.NET には表示の幅を求める API がない（確かめた範囲では見当たらない）。
- Windows の従来のコンソールは、コード ページ（日本語の Windows では 932）で文字を出す。コード ページにない文字は「?」になる。`Console.OutputEncoding` を UTF-8 にすると避けられるが、そのコンソールのコード ページ自体が変わる。

### 5.5 画面の大きさ

- 端末の既定の大きさは、macOS のターミナルで 80 列×24 行、Windows Terminal で 120 列×30 行である（出典未確認。実機で確かめる）。
- 1 マスを 2 升で描くと、盤面の幅は、上級（30 列）で 60 升、枠を入れて 62 升程度になり、80 列に収まる。高さは、上級（16 行）に上の表示と枠を足しても 24 行に収まる見込みである。
- カスタムの最大（30×24、Web 版の仕様書 3.1）は、24 行の端末には収まらない。1 マスを 3 升で描くと、上級でも 80 列に収まらない。
- 大きさは `Console.WindowWidth`、`Console.WindowHeight` で読める。変わったことを知らせるイベントは、`System.Console` にはない。Unix では `SIGWINCH` のシグナルを `PosixSignalRegistration` で受けられる。Windows で同じことができるかは確かめていない。定期的に大きさを読み直す方法もある。
- 盤面が収まらないときの扱いは論点 16 とする。

### 5.6 色

- `ConsoleColor` の 16 色が実際にどの色で見えるかは、端末の配色の設定で決まる。背景が黒か白かも、利用者の設定による。そのため、アプリの側でコントラスト比を保証できない。例えば、7 の数字を黒で描くと、黒い背景の端末では見えない。
- 数字は文字なので、色がなくても読める。旗、地雷、誤った旗、踏んだ地雷も、記号の形で区別すれば、色を出さない設定（`NO_COLOR`。3.2）でも遊べる。

### 5.7 タイマー

- キーを待っている間も、経過時間を 1 秒ごとに描き直す必要がある。`Console.ReadKey` は、キーが来るまで戻らない。
- 方法は二通りある。`Console.KeyAvailable` を短い間隔で調べながら時刻を見る方法と、キーを読むスレッドを別に置いて、描く側に知らせる方法である。
- 複数のスレッドから描くと、カーソルの移動と文字の出力が混ざって画面が崩れる。描くのは 1 か所に限る必要がある。

### 5.8 保存

- 保存先は、デスクトップ版と同じく `Environment.SpecialFolder.LocalApplicationData` の下が候補である。この場所は OS によって違う。Windows は `%LOCALAPPDATA%`、Linux は `$HOME/.local/share`（`XDG_DATA_HOME`）、macOS は .NET 8 から `$HOME/Library/Application Support` になった（Microsoft Learn の .NET 8 の破壊的変更）。
- コンソール版では効果音を鳴らさないので、保存するのはベストタイムだけになる見込みである。

### 5.9 文字列の正規化と実行環境

1.1.0 では、ブラウザーの .NET が NFKC に対応しておらず、例外になった（1.1.0 の B2）。コンソール版にも、実行環境で振る舞いが変わる点がある。デスクトップ版も、トリミングや Native AOT で発行する（6.1）なら同じ注意が要る。

- .NET の「グローバリゼーションのインバリアント モード」（`InvariantGlobalization`）では、正規化のデータがないので、`String.Normalize` は**例外を出さずに、元の文字列をそのまま返す**（dotnet/runtime の設計文書）。全角の数字は半角にならず、カスタムの入力を数として読めなくなる。例外が出ないので、気づきにくい。
- Native AOT のコンソールのテンプレート（`dotnet new console --aot`）は、`InvariantGlobalization` を `true` にする。発行の大きさを減らすために、この設定を使うことが多い。
- Linux でインバリアント モードを使わない場合、.NET は OS の ICU のライブラリを読み込む。ICU が入っていないと、プログラムは起動の時点でエラーになって終わる（Microsoft Learn）。ICU をアプリに同梱する方法（app-local ICU）もある。
- Windows では、OS に付いている ICU（または NLS）を使うので、この問題は起きない。
- そのため、macOS や Linux に配るときは、「インバリアント モードを使わない（ICU に依存する）」か、「使って、NFKC の代わりの整え方を用意する」かを決める必要がある（9 章）。

### 5.10 テスト

- 端末への入出力を、差し替えられる形（インターフェイスなど）にしておけば、画面の描き方を文字列として xUnit でテストできる。
- 実際の端末での見え方（文字の幅、色、代替画面）は、テストでは確かめられないので、実機で確かめる。

### 5.11 ライブラリ

| ライブラリ | 内容 |
|------------|------|
| なし（`System.Console` だけ） | 依存が増えない。マウス、文字の幅、画面の大きさの変化は自分で扱う |
| Spectre.Console | 表や色付きの文字の出力、選択肢などの対話の部品。画面全体を使うゲームの部品ではない |
| Terminal.Gui（v2） | 画面全体を使う端末の UI の部品一式。Windows、macOS、Linux に対応し、マウス、24 ビットの色、Unicode に対応する（Terminal.Gui のサイト） |

どれを使うかはアーキテクチャー設計で決める（9 章）。ライブラリの導入はユーザーの確認が要る。

## 6. 配布と公開

### 6.1 発行の形

| 形 | 内容 | デスクトップ版 | コンソール版 |
|----|------|----------------|--------------|
| フレームワーク依存 | 利用者の PC に .NET 10 のランタイムが要る。発行物は小さい | できる | できる |
| 自己完結 | ランタイムを含める。ランタイムは要らないが、発行物は大きい | できる | できる |
| 単一ファイル | 1 つの実行ファイルにまとめる（`PublishSingleFile`）。描画に使うネイティブのライブラリ（SkiaSharp など）を含めるには `IncludeNativeLibrariesForSelfExtract` が要る | できる | できる |
| トリミング | 使わないコードを除いて小さくする | できる。コンパイル済みのバインディング（4.2）が要る（Avalonia Docs） | できる |
| Native AOT | ネイティブのコードにして、起動を速く、発行物を小さくする。発行する OS の上に、その OS の C++ のツール（Windows は Visual Studio の C++、Linux は clang、macOS は Xcode の Command Line Tools）が要る（Microsoft Learn） | できる（Avalonia Docs）。ただし、Windows 向けは Windows の上で作ることになり、「Linux の上で公開する」（1.1）と合わない。インバリアント モードの注意（5.9）がある | できる。同じ注意がある |
| .NET ツール | NuGet のパッケージとして配り、`dotnet tool install` で入れる。.NET 10 からは、OS ごとのパッケージ（自己完結や AOT を含む）と、入れずに 1 回だけ動かす `dnx` がある（Microsoft Learn） | 当てはまらない | できる。利用者に .NET の SDK が要る |

- **Linux の上で Windows 向けに発行する**: `dotnet publish -r win-x64` で、Windows 向けのランタイムと、NuGet に入っている Windows 用のネイティブのライブラリを含めて発行できる見込みである。ただし、`win-x64` で発行したアプリが SkiaSharp のネイティブのライブラリを読み込めなかったという報告がある（AvaloniaUI/Avalonia の議論 #13853）。Linux の上で発行したものが Windows で動くかは、実装の最初の区切りで確かめる（CLAUDE.md の「実行環境での確認」）。

### 6.2 公開先

- **GitHub Releases**: タグに、OS ごとの zip などを付けて公開する。Web 版と同じリポジトリで扱える。
- **NuGet.org**: .NET ツール（6.1）として配る場合の公開先。
- **Microsoft Store、winget、インストーラー（MSIX など）**: デスクトップ版の配り方の選択肢。手順と審査が増える。MSIX の作成に Windows のツールが要るかは調べていない。
- Web 版の GitHub Pages には、実行ファイルを置く仕組みはない（Web 版のページからリンクすることはできる）。

### 6.3 署名

- **Windows**: インターネットから入れた署名のない実行ファイルは、SmartScreen が「Windows によって PC が保護されました」と警告する。署名のないファイルは、版ごとに評判がゼロから始まる（Microsoft Learn の SmartScreen の評判）。署名にはコード署名の証明書が要る。
- **macOS**: ブラウザーで入れた署名・公証のない実行ファイルは、Gatekeeper が開くのを止める。利用者は、隔離の属性（`com.apple.quarantine`）を `xattr` で外すなどの操作が要る。公証には Apple の開発者のアカウントが要る。
- **Linux**: 実行の許可（`chmod +x`）が要る。zip で配ると、許可が失われることがある（確かめていない）。
- 署名するか、しないで警告を受け入れて手順で案内するかは、論点 28 とする。

### 6.4 版番号とタグ

- Web 版は `v1.0.0`、`v1.1.0` のタグで公開した。デスクトップ版とコンソール版に別の版番号を付けるなら、タグの名前を分ける必要がある（例: `desktop-v1.0.0`）。
- 3 つの版を同じ版番号で出すなら、Web 版に変更がなくても版番号が上がる。

## 7. リポジトリとビルドへの影響

- **公開のワークフロー**: Web 版の公開のワークフロー（`.github/workflows/deploy.yml`）は、Ubuntu の上でリポジトリ直下の `dotnet test` を実行する。Avalonia のプロジェクトは `net10.0` なので、ソリューションに加えても Ubuntu の上でビルドとテストができる見込みである（ヘッドレスのテストを含む。4.9）。WPF で問題になった「ワークフローが失敗する」ことは、起きない見込みである。実装の最初の区切りで確かめる。
- **デスクトップ版とコンソール版の公開**: Web 版とは別に、発行して GitHub Releases などに置く仕組みが要る（6 章）。Ubuntu の上で Windows 向けに発行できるかは、6.1 のとおり確かめる。
- **実機での確認**: CI が Linux でも、デスクトップ版が Windows で実際に動くか（表示、DPI、スクリーンリーダー、音）は、Windows の実機で確かめる。
- **ソリューション**: デスクトップ版とコンソール版のプロジェクトとテストのプロジェクトを加える。
- **共有の部品の変更**: 2.2 の部品を Presentation に移すと、Web 版のコードも変わる。Web 版のテスト（bUnit を含む）で振る舞いが変わらないことを確かめ、Web 版を公開し直すかを決める必要がある。Presentation のプロジェクト ファイルのコメントには「WPF」と書いてあるので、そのときに改める。
- **文書**: この一巡の成果物は docs/desktop-console/ に置く（CLAUDE.md の「開発手順」）。共有の部品を変えたときは、Web 版の設計書（docs/04-architecture.md、docs/05-class-design.md）との関係を示す必要がある。

## 8. 仕様書で決めるべき論点

調査の結果、仕様書で決める必要がある論点を次にまとめる。Web 版の仕様書で決めたことのうち、ゲームのルール（Web 版の仕様書 3 章）は、両方の版で変えない前提である。

### 8.1 全体

| # | 論点 | 選択肢 | 関連する節 |
|---|------|--------|------------|
| 1 | 作る順序と公開の単位 | 二つを同時に公開する / できた方から公開する | 6.4 |
| 2 | デスクトップ版の動作環境 | Windows だけ / macOS と Linux も（Avalonia なら動かせるが、音の鳴らし方と実機の確認が増える）。Windows 11 だけ / Windows 10 も。x64 だけ / Arm64 も | 4.1、4.6 |
| 3 | コンソール版の動作環境 | Windows だけ / macOS と Linux も。対象の端末（Windows Terminal、従来のコンソール ホスト、macOS のターミナル、Linux の端末）。実機で確かめる者と端末 | 5.1 |

### 8.2 デスクトップ版

| # | 論点 | 選択肢 | 関連する節 |
|---|------|--------|------------|
| 4 | 画面の技術 | **決定済み**: Avalonia UI（ユーザーの決定、2026-09-27） | 1.1 |
| 5 | タッチへの対応 | 対応しない（マウスとキーボード） / Avalonia の長押しのイベントに任せる（時間は OS の設定） / Web 版と同じ長押し（400 ミリ秒、進行の表示） | 4.3 |
| 6 | 旗モード | 置かない / Web 版と同じく置く（論点 5 と関わる） | 2.3 |
| 7 | ウィンドウの大きさと盤面 | 盤面に合わせてウィンドウを決める（Windows の定番） / ウィンドウに合わせてマスの大きさを変える（Web 版の 5.2）。縦と横の入れ替えをするか | 3.1、4.4 |
| 8 | 演出 | Web 版と同じ / 一部だけ / しない。OS の「アニメーション効果」に従う（Avalonia では自分で読む） | 4.4 |
| 9 | ダークモードとハイコントラスト | OS の設定に合わせる（Fluent のテーマの既定） / ライトだけ。ハイコントラストへの対応 | 4.4 |
| 10 | 効果音 | Web 版の 5.6 と同じにするか。特に「重ねて鳴らす」を求めるか。切り替えの置き場所と設定の保存 | 4.6 |
| 11 | メニューとキー | Web 版と同じツールバー / Windows の定番のメニュー（ゲーム、難易度、ベストタイム）。F2 などのショートカットを足すか | 3.1、4.3 |
| 12 | スクリーンリーダーへの対応 | Web 版と同じ（表として読み、勝敗を知らせる） / マスの名前と勝敗の知らせだけ | 4.5 |

### 8.3 コンソール版

| # | 論点 | 選択肢 | 関連する節 |
|---|------|--------|------------|
| 13 | 操作のキー | 矢印（と h・j・k・l）、Space・Enter で開く、F で旗。新しいゲーム、難易度の選択、終了、ヘルプのキー | 3.2、5.2 |
| 14 | マウスへの対応 | 対応しない / 対応する（ライブラリか、OS ごとの自前の処理） | 5.2、5.11 |
| 15 | 盤面の表し方 | 1 マスの幅（2 升 / 3 升）、マスの記号（ASCII / 全角）、色（16 色 / 24 ビット / 色なし）。`NO_COLOR` に従うか | 5.4、5.6 |
| 16 | 画面に収まらないとき | 広げるように知らせて待つ / 盤面をスクロールする / カスタムの大きさを端末に合わせて制限する | 5.5 |
| 17 | 画面の形 | 画面全体を使う（代替画面） / 操作のたびに盤面を行として出力する | 3.2、5.3 |
| 18 | カスタムの入力の方法 | 1 行ずつ尋ねる / 入力欄を作る | 5.2 |
| 19 | スクリーンリーダーへの対応 | 対象外 / 読み上げの文（`Announcements`）を状態の行に出す / 行として出力する形（論点 17）で対応する | 5.3、2.1 |
| 20 | 演出 | しない / 簡単なものだけ | 2.3 |

### 8.4 両方

| # | 論点 | 選択肢 | 関連する節 |
|---|------|--------|------------|
| 21 | 保存 | 保存先。デスクトップ版とコンソール版でベストタイムを共有するか | 4.8、5.8 |
| 22 | 画面の言語 | 日本語だけ（Web 版と同じ） / コンソール版は英語も | 5.4 |
| 23 | 対象外の機能 | Web 版の仕様書 9 章をそのまま引き継ぐか（途中経過の保存は、Windows 7 版にある） | 3.1 |
| 24 | ゲームの終わり方 | 閉じる・終了の操作。途中のゲームがあるときに確かめるか | 3.2、5.2 |
| 25 | 性能 | 操作から画面に反映されるまでの目標（Web 版は 100 ミリ秒）。起動の時間の目標 | 4.2、5.3、6.1 |
| 26 | 配布の形 | フレームワーク依存 / 自己完結 / 単一ファイル / Native AOT / .NET ツール（コンソール版） | 6.1 |
| 27 | 公開先 | GitHub Releases / NuGet.org / Microsoft Store・winget | 6.2 |
| 28 | 署名 | しない（警告の越え方を案内する） / する | 6.3 |
| 29 | 版番号とタグ | 版ごとの版番号とタグの名前 / 3 つの版で共通の版番号 | 6.4 |

## 9. アーキテクチャー設計以降で決めること

仕様（何ができるか）ではなく、作り方に関わるものを挙げる。仕様書では決めず、アーキテクチャー設計（工程 7）以降で決める。

| 事項 | 内容 | 関連する節 |
|------|------|------------|
| 共有する部品 | 2.2 の部品のうち、何を Presentation に移すか。移した後の Web 版の確かめ方 | 2.2、7 |
| デスクトップ版の効果音の再生の方法 | `SoundPlayer` / NAudio / OS をまたぐ音のライブラリ。論点 2、10 の決定による | 4.6 |
| ライブラリの導入 | MVVM（CommunityToolkit.Mvvm など）、画面のテスト（Avalonia.Headless.XUnit）、音の再生、端末の UI（Spectre.Console、Terminal.Gui）。どれも導入にはユーザーの確認が要る | 4.2、4.6、4.9、5.11 |
| 盤面の描き方（デスクトップ版） | マスごとのコントロール / 自分で描く。UI オートメーションとの関係 | 4.2、4.5 |
| 動きを減らす設定の読み方（デスクトップ版） | Avalonia にないので、OS ごとに読む（Windows は `SystemParametersInfo`） | 4.4 |
| 入力と描画のスレッド（コンソール） | キーの待ち方と、1 秒ごとの描き直しの両立 | 5.7 |
| 正規化と実行環境 | インバリアント モードを使うか。使うなら、NFKC の代わりをどうするか | 5.9 |
| 例外の境界 | デスクトップ版とコンソール版のそれぞれで、捕まえなかった例外をどこで受け、端末の状態をどう戻すか | 4.10、5.3 |
| 画面のテストの範囲 | デスクトップ版の画面、コンソールの出力を、どこまで自動のテストで確かめ、どこから実機で確かめるか | 4.9、5.10 |
| 公開のワークフロー | Web 版のワークフローとの関係。デスクトップ版とコンソール版を Linux の上で発行して公開する仕組み | 6、7 |

## 10. 参考資料

デスクトップ版の技術の選定（1.1）:

- [NETSDK1100: Set the EnableWindowsTargeting property to true - Microsoft Learn](https://learn.microsoft.com/dotnet/core/tools/sdk-errors/netsdk1100)
- [Avalonia UI Pricing](https://avaloniaui.net/pricing)、[Accelerate Licensing Changes - Avalonia UI Blog](https://avaloniaui.net/blog/building-a-sustainable-future-for-avalonia)
- [Photino.Blazor - GitHub](https://github.com/tryphotino/photino.Blazor)、[PhotinoX.Blazor - GitHub](https://github.com/ivanvoyager/PhotinoX.Blazor)
- [Publishing Your App For Desktop - Uno Platform](https://platform.uno/docs/articles/uno-publishing-desktop.html)
- [ASP.NET Core Blazor Hybrid - Microsoft Learn](https://learn.microsoft.com/aspnet/core/blazor/hybrid/)

Avalonia UI:

- [Pointer devices - Avalonia Docs](https://docs.avaloniaui.net/docs/input-interaction/pointer)
- [Gestures - Avalonia Docs](https://docs.avaloniaui.net/docs/input-interaction/gestures)
- [Tap Events: Holding event not firing - AvaloniaUI/Avalonia issue #17208](https://github.com/AvaloniaUI/Avalonia/issues/17208)
- [Windows - Avalonia Docs](https://docs.avaloniaui.net/docs/platform-specific-guides/windows)（DPI）
- [Platform Settings - Avalonia Docs](https://docs.avaloniaui.net/docs/concepts/services/platform-settings)
- [Setting theme variants - Avalonia Docs](https://docs.avaloniaui.net/docs/guides/styles-and-resources/how-to-use-theme-variants)
- [Respect system animation settings - AvaloniaUI/Avalonia issue #19405](https://github.com/AvaloniaUI/Avalonia/issues/19405)
- [Client area animation parameter - Microsoft Learn](https://learn.microsoft.com/windows/win32/winauto/client-area-animation)
- [Accessibility - Avalonia Docs](https://docs.avaloniaui.net/docs/app-development/accessibility)
- [AutomationLiveSetting - Avalonia Docs](https://docs.avaloniaui.net/api/avalonia/automation/automationlivesetting)
- [Headless Testing with XUnit - Avalonia Docs](https://docs.avaloniaui.net/docs/testing/headless-xunit)
- [NuGet Gallery | Avalonia.Headless.XUnit](https://www.nuget.org/packages/Avalonia.Headless.XUnit)
- [Handling unhandled exceptions - Avalonia Docs](https://docs.avaloniaui.net/docs/concepts/unhandledexceptions)
- [Dispatcher.UIThread.UnhandledException not triggering on MacOS - AvaloniaUI/Avalonia issue #17759](https://github.com/AvaloniaUI/Avalonia/issues/17759)
- [Native AOT - Avalonia Docs](https://docs.avaloniaui.net/docs/deployment/native-aot)
- [dotnet publish -r win-x64, error on run: Unable to load DLL 'libSkiaSharp' - AvaloniaUI/Avalonia discussion #13853](https://github.com/AvaloniaUI/Avalonia/discussions/13853)
- [NAudio - GitHub](https://github.com/naudio/NAudio)（再生の部品が Windows 専用であること）

コンソール:

- [Windows Terminal is now the Default in Windows 11 - Windows Command Line](https://devblogs.microsoft.com/commandline/windows-terminal-is-now-the-default-in-windows-11/)
- [Console Virtual Terminal Sequences - Microsoft Learn](https://learn.microsoft.com/windows/console/console-virtual-terminal-sequences)
- [Mouse in C# Console Application - Microsoft Learn（Q&A の保管記事）](https://learn.microsoft.com/archive/msdn-technet-forums/294915d7-c9e4-4a47-ac21-5822af3a96f3)
- [Ambiguous width character in CJK environment - microsoft/terminal issue #370](https://github.com/microsoft/terminal/issues/370)
- [East Asian Ambiguous Width are broken - microsoft/terminal issue #14702](https://github.com/microsoft/terminal/issues/14702)
- [PosixSignalRegistration.Create Method - Microsoft Learn](https://learn.microsoft.com/dotnet/api/system.runtime.interopservices.posixsignalregistration.create)
- [NO_COLOR](https://no-color.org/)
- [Terminal.Gui v2](https://tui-cs.github.io/Terminal.Gui/)

.NET の実行環境と発行:

- [GetFolderPath behavior on Unix (.NET 8 breaking change) - Microsoft Learn](https://learn.microsoft.com/dotnet/core/compatibility/core-libraries/8.0/getfolderpath-unix)
- [Globalization invariant mode - dotnet/runtime](https://github.com/dotnet/runtime/blob/main/docs/design/features/globalization-invariant-mode.md)
- [Globalization and ICU - Microsoft Learn](https://learn.microsoft.com/dotnet/core/extensions/globalization-icu)
- [Globalization config settings - Microsoft Learn](https://learn.microsoft.com/dotnet/core/runtime-config/globalization)
- [BertTokenizer fails to strip accents under invariant globalization - dotnet/machinelearning issue #7728](https://github.com/dotnet/machinelearning/issues/7728)（AOT のテンプレートがインバリアント モードを有効にすること）
- [Native AOT deployment overview - Microsoft Learn](https://learn.microsoft.com/dotnet/core/deploying/native-aot/)
- [Create RID-specific, self-contained, and AOT .NET tools - Microsoft Learn](https://learn.microsoft.com/dotnet/core/tools/rid-specific-tools)
- [What's new in the SDK and tooling for .NET 10 - Microsoft Learn](https://learn.microsoft.com/dotnet/core/whats-new/dotnet-10/sdk)
- [SmartScreen reputation for Windows app developers - Microsoft Learn](https://learn.microsoft.com/windows/apps/package-and-deploy/smartscreen-reputation)
- [Gatekeeper (macOS) - Wikipedia](https://en.wikipedia.org/wiki/Gatekeeper_(macOS))
- Web 版の調査書（docs/01-research.md）の参考資料（ルール、WCAG、`SoundPlayer` など）
