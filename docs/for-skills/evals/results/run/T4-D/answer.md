# T4 コードレビュー: BoardView の開く演出

## 読んだスキルの範囲
SKILL.md の表の「レビュー（ユーザーから依頼されたとき）」に従い、quality-gates.md の「七箇条をレビューの観点にする」と、refactoring.md の「コードの臭い」「臭い → 技法の対応表」を読んだ。テストのコードは示されておらず、インターフェイスなどの新設もないので、「必要なら読む」の reference は読んでいない。

## 対象の意図（一言で）
手を指したときに、変わったマスの表示を更新する。演出をするときは、開いたマスの覆いを近い順に消す。

## 置いた仮定
- `ViewAnimations.OpeningOrderOf` は、各マスについて（位置, 遅れ）を近い順に返す。「遅れ」が「前のマスとの間隔」なのか「演出を始めてからの時刻」なのかはコードから決まらないので、両方の場合を考えた（指摘 3）。
- `CellView.Update` は、マスの中身（数字など）を表示に出すが、覆いには触れない（演出が覆いを消しているので）。
- `Show` は UI スレッドから呼ばれる。Avalonia の `SynchronizationContext` があるので、`await` の後も UI スレッドに戻る。
- 手を指すたびに `Show` が呼ばれる。新しいゲーム（リセット、難易度の変更）では盤面の表示が作り直されるか、覆いが元に戻される。

## 指摘（重い順）

書式は `<場所>: <臭いの名前、または七箇条の外の性質>（<症状の根拠>）→ <技法・直し方>` とし、七箇条の箇条名を添える。

### 1. `Show` の `_ = PlayOpeningAsync(...)`: 正しさ（七箇条の外。捨てた非同期の処理の中の例外）
- 問題: 戻り値の `Task` を捨てているので、`PlayOpeningAsync` の中で起きた例外（`cellViews[position]` の `KeyNotFoundException`、`FadeOutAsync` の中の失敗など）は、誰にも観察されずに消える。演出が途中で止まり、残りのマスの覆いが残ったままになるが、ログにもテストにも出ない。「テストは Green で、画面でも見えた」のは、例外が起きない小さな開き方を見ただけで、失敗を隠している可能性がある。
- 直し方: 演出の `Task` を、ほかの箇所で待てる形で持つ（下の 2 の `currentAnimation`）。捨てるなら、少なくとも中で `try/catch` して、`OperationCanceledException` 以外を記録する。例外で演出が止まっても盤面が正しく見えるように、覆いの最終の状態は演出に頼らない（指摘 4）。

### 2. `Show` と `PlayOpeningAsync`: 正しさ（七箇条の外。後始末。取り消しがない）
- 問題: 演出の途中で次の `Show` が来ても、前の演出は止まらない。
  - 続けて手を指したとき: 二つの演出が同時に走り、同じ覆いに二度 `FadeOutAsync` をかけることがある（大きく開いた直後に、隣を開いた場合など）。
  - 新しいゲームを始めたとき: 前のゲームの演出が残り、**新しいゲームの覆いを消してしまう**。まだ開いていないマスが開いて見え、遊べない盤面に見える。盤面の表示が作り直されて `cellViews` の中身が変わっていれば、`KeyNotFoundException` になり、1 の理由で黙って消える。
  - コントロールが画面から外れたとき（ウィンドウを閉じる、難易度の変更で盤面を作り直す）も、演出は最後まで走る。
- 直し方: `CancellationTokenSource` を `BoardView` に持ち、`Show` の初めと、盤面の作り直し・`OnDetachedFromVisualTree` で前の演出を取り消す。`Task.Delay` と `FadeOutAsync` に `CancellationToken` を渡す。取り消したときは、残りの覆いを最終の状態に揃える（指摘 4）。

### 3. `PlayOpeningAsync` の `foreach` の中の二つの `await`: 正しさ（七箇条の外。時間の扱い）/ 意図を表現
- 問題: 各マスの消える演出（120 ms）を待ってから次のマスに進むので、演出の長さはマスの数に比例して積み上がる。上級で数百マスが一度に開くと、120 ms × マスの数で、数十秒かかる（400 マスなら 48 秒以上）。しかも「近いマスから順に」ではなく、1 マスずつの行列になり、同じ距離のマスが同時に消えない。
  - 「遅れ」が演出を始めてからの時刻なら、`Task.Delay(delay)` を順に待つことで遅れが足し算になり、遠いマスほど大きく遅れる。
  - 「遅れ」が前のマスとの間隔でも、120 ms の待ちが毎回足されるので、`OpeningOrderOf` で決めた間隔にならない。
  - どちらにしても、`OpeningOrderOf` が決めた時刻表（テストで確かめたはずのもの）と、画面の時刻が食い違う。テストが Green でも、この食い違いは確かめられていない。
- 直し方: マスごとに「始めてからの時刻」で消し始め、消える演出は待たずに並べる。`Task.WhenAll` で全体の終わりを待つ。

```csharp
async Task PlayOpeningAsync(IReadOnlyList<CellPosition> opened, CancellationToken token)
{
    var fades = ViewAnimations.OpeningOrderOf(opened)
                              .Select(step => RevealLaterAsync(cellViews[step.Position], step.Delay, token));
    await Task.WhenAll(fades);
}

static async Task RevealLaterAsync(CellView view, TimeSpan startsAt, CancellationToken token)
{
    await Task.Delay(startsAt, token);
    await view.FadeOutCoverAsync(token);
}
```

（`OpeningOrderOf` の遅れが間隔なら、演出を始めてからの時刻に直すのは `ViewAnimations` の側で行い、その変換を単体テストで確かめる。）

### 4. `Update` と `view.Cover.IsVisible = false`: 変更の分散 / 単一責務（覆いの状態の責務が二か所に分かれている）
- 問題: マスの表示の状態は `CellView.Update` が決めるのに、覆いを消す最終の状態（`IsVisible = false`）は演出の最後の行にしかない。そのため、
  - `withAnimation` が `false` のとき、誰が覆いを消すのかがこのコードから読めない。`Update` が消すなら演出は見えないはずで、消さないなら、演出を切った利用者（OS のアニメーション効果を切った人）には、開いたマスが開いて見えない。どちらかが食い違っている。
  - 演出が取り消されたり、例外で止まったり（指摘 1、2）すると、開いたマスに覆いが残り、盤面の見た目がゲームの状態と食い違う。
- 直し方: 覆いの最終の状態は `CellView.Update` で決める（開いたマスは「覆いなし」）。演出は、その上に一時的に重ねる見た目にする。例えば、`Update` は開いたマスの覆いを「消える前」の状態に置き、演出がないとき・取り消されたときは `CellView.RevealCoverNow()` で最終の状態に揃える。演出の有無で、盤面の最終の見た目が変わらないようにする。

### 5. `view.Cover.FadeOutAsync(...)` と `view.Cover.IsVisible`: メッセージの連鎖 / 不適切な関係
- 問題: `BoardView` が `CellView` の中の部品 `Cover` にまで踏み込み、消し方（フェードの長さ、消した後に見えなくすること）を知っている。覆いの作りを変えると、`BoardView` も直すことになる。
- 直し方: デメテルの法則に沿って、依頼先に委ねる。`CellView` に `Task FadeOutCoverAsync(CancellationToken)`（と 4 の `RevealCoverNow()`）を置き、`BoardView` は「このマスの覆いを消して」と頼むだけにする。

### 6. `TimeSpan.FromMilliseconds(120)`: 意図を表現 / 的確な名前（名前のない数）
- 問題: 120 が何の長さかは、呼び出しの形から推し量るしかない。演出の時間の決まりは `ViewAnimations` にあるのに、フェードの長さだけがビューに埋まっている。
- 直し方: `ViewAnimations.CoverFadeDuration` のような名前の定数にし、ほかの演出の時間と同じ場所に置く（5 の後なら `CellView` の中で使う）。

### 7. `Show(MoveResult move, bool withAnimation)`: 意図を表現（真偽の引数）
- 問題: 呼び出し側は `Show(move, true)` となり、`true` の意味が呼び出しの場所で読めない。また、`Show` は「表示の更新」と「演出の起動」の二つを行っている。
- 直し方: 重い指摘ではない。直すなら、`Show(move)` と `PlayOpening(move)` に分けるか、名前付き引数（`withAnimation: true`）で呼ぶ。4 を直して、演出の有無で最終の見た目が変わらなくなれば、分けるのが自然になる。

### 8. `PlayOpeningAsync` 全体: Testable
- 問題: 時刻が `Task.Delay` に固定され、演出の順序・取り消し・最終の状態を、Avalonia を動かさずに確かめる手段がない。「テストは Green」でも、1〜4 の不具合は、このテストでは見つからない形である。
- 直し方: 判断（どのマスをいつ消すか、取り消したときに何を最終の状態にするか）は `ViewAnimations` の側に寄せて単体テストで確かめ、ビューに残すのは「時刻が来たら頼む」ことだけにする。時刻を確かめたいなら、`TimeProvider` を渡して `Task.Delay(startsAt, timeProvider, token)` にする（テストのための差し替えは YAGNI 違反ではない）。1〜4 を直すときは、「新しいゲームを始めると前の演出が止まる」「演出を取り消しても開いたマスの覆いが残らない」を、再現するテストとして先に足す。

## 問題としなかった点
- `await` の後に UI の部品を触っている点: `Show` が UI スレッドから呼ばれる限り、Avalonia の `SynchronizationContext` により UI スレッドに戻るので問題ない（仮定による）。`ConfigureAwait(false)` を足してはいけない。
- `cellViews` を `Dictionary` で引いている点: 盤面の大きさに対して十分速く、計測なしに変える理由はない。

## まとめ
重いのは 1〜4 で、いずれも七箇条の外の「正しさ」（捨てた非同期の処理の例外、取り消しと後始末、時間の積み上がり）と、覆いの最終の状態の責務の分散である。小さく開いたときだけ画面で見たので、テストも画面の確認も通っている。上級で大きく開いたとき、演出の途中で次の手を指したとき、演出の途中でリセットしたときの 3 つを、実行環境で確かめることを勧める。
