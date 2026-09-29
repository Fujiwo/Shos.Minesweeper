# T4 コードレビュー: `BoardView` の開く演出

## 適用したスキルの範囲
作業の種類は「レビュー（ユーザーから依頼されたとき）」とした。SKILL.md の表に従い、quality-gates.md の「七箇条をレビューの観点にする」と、refactoring.md の「コードの臭い」「臭い → 技法の対応表」を読んだ。テストのコードは示されておらず、新しい抽象も足されていないので、「必要なら読む」の testing.md と simplicity.md は読んでいない。

## 意図（一言で）
「一手で変わったマスの表示をすぐに更新し、演出が有効なら、開いたマスの覆いを近いものから順にフェードアウトして消す」。

## 置いた仮定
- A1: `ViewAnimations.OpeningOrderOf` は、近い順に並んだ `(位置, 遅れ)` を返す。`delay` が意味するのは**演出の始まりからの遅れ**（波の形の演出で普通の形）とする。前のマスからの間隔という意味の場合も、下の R2 の指摘は（形を変えて）当てはまる。
- A2: `CellView.Update` は中身（数字・旗など）を描き換えるが、`Cover` の表示には触れない（触れるなら、演出が見えるはずがないので）。
- A3: `Show` は UI スレッドから呼ばれる。Avalonia の UI スレッドには `SynchronizationContext` があるので、`await` の後も UI スレッドに戻る。このため、スレッドの問題は指摘に入れていない。
- A4: 同じ `BoardView` が、新しいゲーム（リセット・難易度の変更）でも使い回される。
- A5: 盤面は最大で上級（30×16）かカスタム。一手で数百マスが開くことがある。

「テストは Green で、画面でも演出が見えた」は、狭い範囲が開いたときの確認だと考えられる。下の R1〜R4 は、広く開いたとき・演出の途中で次の操作をしたとき・例外が起きたときにだけ出る問題で、今の確認では見つからない。

---

## 指摘（重大な順）

### R1（正しさ: 捨てた非同期の処理の中の例外）
- **場所**: `Show` の `_ = PlayOpeningAsync(move.OpenedCells);`
- **問題**: 返された `Task` を捨てている。`PlayOpeningAsync` の中で起きた例外（例: R3 で盤面が作り直された後の `cellViews[position]` の `KeyNotFoundException`、`FadeOutAsync` の中の失敗）は、誰にも観測されない。演出はそこで止まり、残りのマスの覆いは表示されたまま残る。遊ぶ人から見ると「開いたはずのマスが開いていない」画面になり、ログにも出ない。
- **直し方**: 演出の中で例外を 1 か所で受け止める（取り消しは R3 の `CancellationTokenSource` で行う）。受け止めた後は、記録して、演出の最後の状態（覆いを消す）にする（R4 と合わせる）。`_ =` で捨てるなら、捨ててよい理由（中で受け止めている）をコードに書く。
  コードは末尾の「直した形」の `PlayOpeningAsync` の `try`/`catch` にある。

### R2（正しさ: 演出の長さが開いたマスの数に比例して延びる）
- **場所**: `PlayOpeningAsync` の `foreach` の中の `await Task.Delay(delay);` と `await view.Cover.FadeOutAsync(...)`
- **問題**: 1 マスずつ「待つ → フェードが終わるまで待つ」を順に `await` している。
  - 仮定 A1（始まりからの遅れ）の場合、遅れが**積み上がる**。n 番目のマスが消え始めるのは、指定した遅れではなく「それまでの遅れの合計 + 120 ms × (n − 1)」の後になる。
  - 遅れが間隔の意味の場合でも、フェードの 120 ms を毎回待つので、フェードが重ならない。100 マス開くと、少なくとも 12 秒かかる。
  
  上級で広く開いたときは、何秒も覆いが残り、その間に遊ぶ人はそこを押せてしまう（押すと何が起きるかは R3）。狭い範囲では気づかないので、「画面で見えた」と矛盾しない。
- **直し方**: 各マスの演出を「始まりから `delay` 後にフェードを始める」独立した処理にして、同時に走らせ、`Task.WhenAll` で待つ。こうすると、全体の長さは「最大の遅れ + 120 ms」で決まる。
  ```csharp
  Task PlayOpeningAsync(IReadOnlyList<CellPosition> opened, CancellationToken token) =>
      Task.WhenAll(ViewAnimations.OpeningOrderOf(opened)
          .Select(step => cellViews[step.Position].RevealAfterAsync(step.Delay, token)));
  ```
  `OpeningOrderOf` の `delay` がどちらの意味かは、この場で名前（例: `DelayFromStart`）か型のコメントで決めておく（的確な名前）。

### R3（正しさ: 演出が重なったとき・盤面が変わったときの後始末がない）
- **場所**: `Show` と `PlayOpeningAsync` の全体（取り消しの手段がない）
- **問題**: 演出の途中で次の `Show` が来ると、前の演出を止めずに新しい演出が並んで走る。特に次の場合に表示が壊れる。
  - リセット・新しいゲーム（A4）: 新しい盤面ではすべての覆いを出すはずなのに、前のゲームの演出が後から `IsVisible = false` にするので、**新しいゲームの未開放のマスの覆いが消える**。
  - 難易度の変更で `cellViews` が作り直された場合: 前の演出が、古い `CellView` に触れ続けるか、`KeyNotFoundException` になる（それが R1 で捨てられる）。
  - 地雷を開いて負けたとき・勝ったとき: 覆いが消える途中で、結果の表示（全地雷の表示や勝利カード）と重なる。
  - ウィンドウを閉じたとき: 画面から外れたコントロールに触れ続ける。
- **直し方**: `CancellationTokenSource` をフィールドに持つ。`Show` の初めと盤面の作り直し（と `DetachedFromVisualTree`）で前の演出を取り消し、次の手で取り消すときは、残りの覆いをすぐ消して最後の状態にする。`Task.Delay` と `FadeOutAsync` に `token` を渡す。
  コードは末尾の「直した形」の `FinishOpening`・`CancelOpening` にある。

### R4（正しさ・単一責務: 覆いの最後の状態が、演出が最後まで走ることに依存している）
- **場所**: `PlayOpeningAsync` の `view.Cover.IsVisible = false;` と、`Show` の `withAnimation` が偽の道
- **問題**: 「開いたマスの覆いは消える」という表示の**状態**を、演出（見た目の**経過**）の最後の一行だけが決めている。そのため、
  - 仮定 A2 のとおり `Update` が覆いに触れないなら、`withAnimation` が偽のとき（OS のアニメーション効果が切られているとき）は、覆いがまったく消えない。コードの上では、演出なしの道で覆いを消す処理が見当たらない。
  - 演出が取り消されたり失敗したりすると（R1、R3）、覆いが残る。
  
  逆に、`Update` が覆いを消しているなら、演出は消えている覆いをフェードしていることになり、演出が見えた事実と合わない。どちらにしても、覆いの状態の責任が `Update` と演出の二か所に分かれて、読んで決められない。
- **直し方**: 覆いを出すか消すかは、マスの状態から決める（`Update` か `HideCovers` の 1 か所）。演出は「消えることが決まった覆いを、見た目のうえでだけ遅らせて消す」ものにして、取り消し・失敗・演出なしのどの道でも、最後は同じ状態になるようにする（「直した形」の `FinishOpening`、`catch`、`Show` の `else`）。このとき、`withAnimation` が偽の道をテストで押さえる（今は Green でも、この道を確かめるテストがないと思われる）。
  - 七箇条: **単一責務**（覆いの状態を決める責任を 1 か所に閉じる）。

### R5 `PlayOpeningAsync` の `view.Cover.FadeOutAsync(...)` / `view.Cover.IsVisible`: メッセージの連鎖（`cellViews[position].Cover.FadeOutAsync` のように、`CellView` の中にある `Cover` までたどって操作している）→ デメテルの法則に沿って、依頼先に委ねる
- **問題**: `BoardView` が、`CellView` の中の作り（覆いが `Cover` という部品であること、フェードの後に `IsVisible` を落とすという手順）を知っている。覆いの作りを変えると、`BoardView` も変えなければならない。
- **直し方**: `CellView` に `RevealAfterAsync(TimeSpan delay, CancellationToken token)`（遅れて覆いを消す）と `HideCover()`（すぐに消す）を置き、`BoardView` は「このマスを見せる」とだけ頼む。
- 七箇条: **意図を表現**（`BoardView` のコードに「開いたマスを見せる」という What だけが残る）。

### R6 `PlayOpeningAsync` の `TimeSpan.FromMilliseconds(120)`: 名前のない数（マジックナンバー。臭いの表の外の症状）→ 名前を付けて、演出の順序と同じ場所に置く
- **問題**: フェードの長さ 120 ms が、順序と遅れを決める `ViewAnimations` と離れて、ビューのコードに埋め込まれている。演出の時間の決まりが二か所に分かれ、全体の長さ（R2 の「最大の遅れ + フェードの長さ」）を 1 か所で読めない。
- **直し方**: `ViewAnimations.CoverFadeDuration` のように名前を付けた定数にして、`OpeningOrderOf` と並べる。
- 七箇条: **Once And Only Once**（演出の時間の決まりを 1 か所に確定する）、**意図を表現**。

### R7（Testable）`PlayOpeningAsync` 全体: 時間と画面に直接つながっていて、R2〜R4 をテストで確かめられない
- **問題**: `Task.Delay` を直接呼び、Avalonia のコントロールを直接操作しているので、「演出の途中でリセットしたら覆いが正しく戻るか」「演出なしで覆いが消えるか」「全体の長さは最大の遅れ + フェードか」を、テストで確かめられない。テストが Green なのに R1〜R4 が残っているのは、このためである。
- **直し方**: 引き算の範囲で、次の二つだけを分ける。
  1. 「いつどのマスの覆いを消し始めるか」の予定（`OpeningOrderOf` とフェードの長さ）は、すでに `ViewAnimations` にある純粋な計算として置き、そのテストで「重ならない」「始まりからの遅れ」を確かめる。
  2. 「取り消されたら残りをすぐ消す」「演出なしならすぐ消す」の判断は、`Show` の中の短い分岐として置き、覆いの状態（`IsVisible`）を見るテストで確かめる。遅れを待たずに確かめたいなら、`TimeProvider`（.NET 8 以降の標準）を受け取り、`Task.Delay(delay, timeProvider, token)` にする（時刻を差し替えられるようにするのは、テストがすでに必要としているので YAGNI に当たらない）。
- 画面のテストのライブラリを使えない事情があるなら、判断をビューの外（ビューモデルか演出の予定の計算）に寄せる方がよい。

### R8（軽微）`Show(MoveResult move, bool withAnimation)`: フラグ引数（`bool` で、中の振る舞いが二つに分かれる。臭いの表の外の症状）→ 今のままでよいが、呼び出し側で意味が読めるようにする
- **問題**: 呼び出し側が `Show(move, true)` と書くと、`true` の意味が読めない。
- **直し方**: 分岐は 2 つで小さく、メソッドを二つに分けるほどではない。呼び出し側で名前付き引数（`withAnimation: settings.AnimationsEnabled`）にするか、変数に意図の名前を付けて渡す。これ以上の型（列挙型など）は、分岐が増えるまで足さない（引き算）。

---

## 直した形（R1〜R6 をまとめた例）
```csharp
public sealed class BoardView : UserControl
{
    readonly Dictionary<CellPosition, CellView> cellViews = new();
    CancellationTokenSource? opening;
    IReadOnlyList<CellPosition> coversBeingRevealed = [];

    public void Show(MoveResult move, bool withAnimation)
    {
        FinishOpening();   // 前の手の演出を止め、その覆いをすぐ消す
        foreach (var cell in move.ChangedCells)
            cellViews[cell.Position].Update(cell);

        if (withAnimation)
            StartOpening(move.OpenedCells);
        else
            HideCovers(move.OpenedCells);
    }

    // 盤面を作り直すとき・画面から外れたときに呼ぶ（古いマスには触れない）
    void CancelOpening()
    {
        opening?.Cancel();
        opening?.Dispose();
        opening = null;
        coversBeingRevealed = [];
    }

    void FinishOpening()
    {
        var remaining = coversBeingRevealed;
        CancelOpening();
        HideCovers(remaining);
    }

    void StartOpening(IReadOnlyList<CellPosition> opened)
    {
        opening = new CancellationTokenSource();
        coversBeingRevealed = opened;
        _ = PlayOpeningAsync(opened, opening.Token);   // 例外は中で受け止めるので、捨ててよい
    }

    async Task PlayOpeningAsync(IReadOnlyList<CellPosition> opened, CancellationToken token)
    {
        try {
            await Task.WhenAll(ViewAnimations.OpeningOrderOf(opened)
                .Select(step => cellViews[step.Position].RevealAfterAsync(step.DelayFromStart, token)));
        } catch (OperationCanceledException) {
            // 取り消した側（FinishOpening か CancelOpening）が、最後の状態を決める
        } catch (Exception e) {
            Trace.TraceError(e.ToString());   // 記録の仕方はアプリの流儀に合わせる
            if (!token.IsCancellationRequested)
                HideCovers(opened);
        }
    }

    void HideCovers(IEnumerable<CellPosition> opened)
    {
        foreach (var position in opened)
            cellViews[position].HideCover();
    }
}
```
- 取り消しを二つに分けたのは、最後の状態が理由で違うからである。次の手（`FinishOpening`）では、前の手で開いたマスの覆いは消えていなければならない。盤面の作り直し（`CancelOpening`）では、古いマスは捨てるので触れない。
- 演出が最後まで走ったときは、`RevealAfterAsync` の中で各マスの覆いが消える。`coversBeingRevealed` が残っていても、次の `FinishOpening` で消えている覆いをもう一度消すだけで、害はない。

## ユーザーの判断が要る点
1. `OpeningOrderOf` の遅れは、始まりからか、前のマスからか（R2 の直し方の形が変わる）。
2. 演出の途中に次の操作が来たとき、前の演出を「すぐ最後の状態にする」か、「最後まで続けながら新しい演出を重ねる」か。本レビューは前者を勧める。
3. 演出の途中で覆いが残っているマスを押せるか（押せないようにするなら、入力の側で演出中を知る必要がある）。
4. 負け・勝ちの表示と演出の順序（演出が終わってから結果を見せるか、すぐに見せるか）。

## 良い点
- 演出の順序と遅れの計算を `ViewAnimations.OpeningOrderOf` に分けてあり、ビューは再生だけをしている。R7 のテストの足場はすでにある。
- `ChangedCells` で中身を先に確定させ、演出を見た目だけのものにしようとしている方向は正しい（R4 はその方向を最後まで通すための指摘である）。
