# T4 コードレビュー: BoardView の開く演出

## 前提（仮定）

コードの外側は見えないので、次のように仮定して読んだ。仮定が違うと結論が変わる指摘には、その旨を書いた。

- `Show` は UI スレッドから呼ばれる。Avalonia の `SynchronizationContext` があるので、`await` の後も UI スレッドで続く。
- `CellView.Update(cell)` はマスの中身（数字、旗など）を書き換えるが、`Cover` の表示は変えない（変えるなら演出は見えないはずで、「画面で見えた」と合わない）。
- `Cover` は、新しいゲームのときに `IsVisible = true` に戻される。
- `ViewAnimations.OpeningOrderOf` は、起点に近い順に `(位置, 遅れ)` を返す。遅れが「演出の始まりからの時刻」か「一つ前からの間隔」かは分からない。
- `Show` は、同じ `BoardView` に対して、次の手、新しいゲーム、難易度の変更のたびに呼ばれる。演出の最中にも呼ばれうる。

## 指摘（重い順）

### 1. 演出の最中に次の `Show` や新しいゲームが来ると、古い演出が新しい盤面の覆いを消す（正しさ）

- 場所: `Show` の `_ = PlayOpeningAsync(...)` と、`PlayOpeningAsync` 全体
- 問題: 始めた演出を止める手段がない。広く開いた直後にリセットすると、古い演出が続き、新しいゲームのまだ開いていないマスの `Cover` を `IsVisible = false` にする。利用者には、開いていないマスが開いたように見える（中身が前のゲームのままか空に見える）。難易度を小さい盤面に変えた場合は、`cellViews[position]` が `KeyNotFoundException` になる（指摘 2 のとおり、その例外は誰にも見えない）。連続して開いたときも、二つの演出が同時に走り、順序が崩れる。テストが Green で画面でも見えたのは、一手ずつ演出が終わるのを待ってから操作したためと考える。
- 直し方: 演出を `CancellationTokenSource` で止められるようにする。`Show` の先頭と、新しいゲームの準備（盤面の作り直し）と、`DetachedFromVisualTree` で、走っている演出を取り消す。取り消したときは、その演出が受け持っていたマスを最終の状態（覆いなし）に一度にする。`Task.Delay` と `FadeOutAsync` にトークンを渡し、`OperationCanceledException` は「取り消された」として受け止める。

```csharp
CancellationTokenSource? opening;

public void Show(MoveResult move, bool withAnimation)
{
    CancelOpening();
    foreach (var cell in move.ChangedCells)
        cellViews[cell.Position].Update(cell);

    if (withAnimation)
        StartOpening(move.OpenedCells);
    else
        Uncover(move.OpenedCells);
}

void CancelOpening()
{
    opening?.Cancel();   // 取り消された演出は、自分のマスを最終の状態にしてから抜ける
    opening = null;
}
```

### 2. `_ =` で捨てたタスクの例外が消える（正しさ）

- 場所: `Show` の `_ = PlayOpeningAsync(move.OpenedCells);`
- 問題: 演出の中で例外が起きても（指摘 1 の `KeyNotFoundException`、`FadeOutAsync` の失敗など）、観測されずに消え、演出がそこで止まる。止まった先のマスは覆いが残ったままになり、原因の手がかりも残らない。
- 直し方: 演出を始める側で例外を受け止める一か所を作る。取り消しは正常な終わりとして扱い、それ以外の例外は記録したうえで、残りのマスを最終の状態にする（演出が失敗してもゲームの表示は正しく保つ）。

```csharp
async void StartOpening(IReadOnlyList<CellPosition> opened)   // UI のイベントの起点として async void を 1 か所だけ使う
{
    var cts = opening = new CancellationTokenSource();
    try {
        await PlayOpeningAsync(opened, cts.Token);
    } catch (OperationCanceledException) {
    } catch (Exception e) {
        Trace.TraceError(e.ToString());
    } finally {
        Uncover(opened);          // 取り消し・失敗・完了のどれでも最終の状態にそろえる
        if (opening == cts) opening = null;
        cts.Dispose();
    }
}
```

`async void` を避けたいなら、`_ =` の代わりに、例外を受け止める継続を付ける拡張メソッド（`Forget(onError)` など）を用意する。いずれにしても、捨てる前に例外の行き先を決める。

### 3. `withAnimation` が偽のとき、覆いが消えない（整合・正しさ）

- 場所: `Show` の `if (withAnimation)`
- 問題: 仮定のとおり `Update` が覆いに触れないなら、覆いを消すのは演出の中だけである。OS のアニメーション効果を切った利用者（演出を出さない場合）には、開いたマスが覆われたままに見える。逆に `Update` が覆いを消しているなら、演出は何も見せないはずで、「画面で見えた」ことと合わない。どちらにしても、覆いの最終の状態を決める責任が `Update` と演出に分かれていて、読むだけではどちらが正しいか分からない。
- 直し方: 「最終の状態を決める」ことと「そこへ向かう途中を見せる」ことを分ける。覆いの最終の状態（開いたマスは覆いなし）は、演出の有無にかかわらず `Show` が必ず作る（上の `Uncover`）。演出は、覆いを一時的に見せておいて消していくだけにする。演出なしの場合の覆いの状態を確かめるテストを足す。

### 4. 一マスずつフェードを待つので、順に消えず、広く開くと長くかかる（正しさ・整合）

- 場所: `PlayOpeningAsync` の `foreach` の中の `await Task.Delay(delay);` と `await view.Cover.FadeOutAsync(...)`
- 問題: 各マスで「遅れを待つ → 120 ms のフェードが終わるまで待つ」を繰り返すので、次のマスのフェードは前のマスのフェードが終わるまで始まらない。
  - 遅れが「始まりからの時刻」なら、実際の時刻は遅れの合計に 120 ms × マスの数が足され、設計した時刻と大きくずれる。
  - どちらの意味でも、フェードの 120 ms がマスの数だけ積み重なる。上級で 0 のマスを押して 100 マス開けば、12 秒以上かかる。同じ距離のマスが同時に消えず、「近いマスから順に広がる」見え方にならない（一つずつ消える）。
  - その間ずっと覆いが残るので、指摘 1 の重なりが起きやすくなる。
- 直し方: 遅れを「始まりからの時刻」と定め（`ViewAnimations` に書いて、テストで確かめる）、各マスのフェードをその時刻に始めて、並べて走らせる。フェードの終わりは一つずつ待たず、最後に `Task.WhenAll` でまとめて待つ。

```csharp
async Task PlayOpeningAsync(IReadOnlyList<CellPosition> opened, CancellationToken token)
{
    var fades = ViewAnimations.OpeningOrderOf(opened)
        .Select(step => FadeCoverAsync(cellViews[step.Position], step.Delay, token));
    await Task.WhenAll(fades);
}

async Task FadeCoverAsync(CellView view, TimeSpan startAt, CancellationToken token)
{
    await Task.Delay(startAt, token);
    await view.Cover.FadeOutAsync(ViewAnimations.CoverFadeDuration, token);
    view.Cover.IsVisible = false;
}
```

マスの数が多いときは、演出の全体の長さに上限を設ける（遅れを縮める、一定の数を超えたら演出を省く）ことも `ViewAnimations` で決め、テストする。

### 5. フェードの後に `Opacity` が 0 のまま残るおそれがある（正しさ。要確認）

- 場所: `await view.Cover.FadeOutAsync(...)` の後の `view.Cover.IsVisible = false;`
- 問題: `FadeOutAsync` が `Opacity` を 0 にして終わる作りなら、新しいゲームで `IsVisible = true` に戻しても、覆いは透明のままで見えない。1 ゲーム目では気づけず、2 ゲーム目以降で出る不具合である。
- 直し方: 覆いを「見せる」処理（新しいゲームの準備、演出の始まり）で、`IsVisible = true` と `Opacity = 1` を必ずそろえて戻す。または、フェードの終わりに `Opacity` を 1 に戻してから `IsVisible = false` にする。どちらかに決め、2 ゲーム目の覆いが見えることを確かめる。

### 6. 演出の最中の入力と勝敗の表示の順（整合。要確認）

- 場所: `Show` の呼び出し元と、`Cover` の当たり判定
- 問題: 演出の間、モデルでは開いたマスに覆いが残っている。`Cover` が当たり判定を受けるなら、押したときに何が起きるかが見た目と合わない可能性がある。また、この手で勝ったとき、勝利カードが演出の途中で出て、覆いの残る盤面に重なる。
- 直し方: `Cover.IsHitTestVisible = false` にして、入力は常にモデルの状態で判断する。勝利・敗北の表示を演出の後に出すかどうかを UI デザインで決め、出す場合は演出の完了（`PlayOpeningAsync` のタスク）を待ってから出す。

### 7. 時間の値がビューに直に書かれている（引き算の範囲の軽い指摘）

- 場所: `TimeSpan.FromMilliseconds(120)`
- 問題: 並びと遅れは `ViewAnimations` にあるのに、フェードの長さだけがビューにあり、演出の時間の決まりが二か所に分かれている。全体の長さ（指摘 4）を計算するにも、この値が要る。
- 直し方: `ViewAnimations.CoverFadeDuration` のような名前を付けて、並びと同じ場所に置く。

## テストについて

今のテストは一手ずつの演出を確かめただけで、上の問題を捕まえられない。画面を起動しないテストとして、少なくとも次を足す。時間は `TimeProvider`（`Task.Delay(delay, timeProvider, token)`）を差し込んで、`FakeTimeProvider` で進める。

- 演出の最中に新しいゲームを始めると、新しい盤面の覆いがすべて残る（指摘 1）
- 演出の最中に次の手を開くと、前の演出のマスも含めて、開いたマスがすべて覆いなしになる（指摘 1、2）
- 演出なしで開くと、開いたマスが覆いなしになる（指摘 3）
- 同じ距離のマスが同じ時刻に消え始め、全体の長さが上限を超えない（指摘 4。並びと時刻の計算は `ViewAnimations` の単体テストで確かめられる）
- 2 ゲーム目の覆いが見える（指摘 5）

## 良い点

- 開く並びと遅れの計算を `ViewAnimations.OpeningOrderOf` に分けているので、順序はビューを起動せずにテストできる。
- マスの中身の更新（`ChangedCells`）と演出（`OpenedCells`）を分けて受け取っていて、何を更新し、何を見せるかの区別がはっきりしている。
