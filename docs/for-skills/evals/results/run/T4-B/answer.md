# T4 コードレビュー: `BoardView` の開く演出

## 使ったスキルと読んだ範囲
スキル B の「作業規模に応じた適用の軽重」では、これは「レビュー(ユーザーから依頼されたとき)」に当たるので、判断を伴う作業として扱った。次の節を読んだ。
- quality-gates.md の「七箇条をレビューの観点にする」
- refactoring.md の「コードの臭い」「臭い → 技法の対応表」(書式を確かめるために「精神論ではなく臭いと技法の名前で語る」も読んだ)

testing.md と simplicity.md は、「必要なら読む」の条件に当たらないので読まなかった。レビューするコードにテストは含まれず、新しい抽象もない。

## 対象の意図(一言で)
`Show` は、1 手で変わったマスを描き直し、演出が有効なときは開いたマスの覆いを近い順にフェードアウトさせる。

## 置いた仮定
- A1: `ViewAnimations.OpeningOrderOf` が返す `delay` は、演出の**開始から**数えた時刻である(起点からの距離に比例する。同じ距離のマスは同じ値)。前のマスからの間隔ではない。
- A2: `CellView.Update` は、演出が有効なときに開いたマスの覆いを表示したままにする。無効なときは `Update` か別の誰かが覆いを隠す(コードには書かれていない)。
- A3: `Show` は UI スレッドから呼ばれる。Avalonia の `SynchronizationContext` があるので、`await` の続きも UI スレッドで動く。
- A4: 同じ `BoardView` と `cellViews` を、次の手、リセット、難易度の変更でも使い続ける(または、`cellViews` を作り直す)。

---

## 指摘(重いものから)

### 1.【正しさ・重大】`Show` の `_ = PlayOpeningAsync(...)`: 投げっぱなしの非同期で、止められない
- **場所**: `Show` の `_ = PlayOpeningAsync(move.OpenedCells);`。どの七箇条にも当てはまらない指摘なので、「正しさ」とした。
- **問題**:
  1. **取り消しがない。** 演出の途中で次の `Show`(次の手)、リセット、難易度の変更が起きても、前の演出は動き続ける。リセットで覆いを戻したマスに、前のゲームの演出が後から届いて `Cover.IsVisible = false` にする。その結果、新しいゲームなのに開いたように見えるマスができる(未開放のマスの中身は見えないとしても、見た目の状態と盤面の状態が食い違う)。前の手の演出と次の手の演出が並んで走るとき、同じマスの覆いを二重にフェードさせることもある。
  2. **例外が消える。** `_ =` で捨てているので、`FadeOutAsync` や `cellViews[position]` の例外(A4 で盤面を作り直したあとの `KeyNotFoundException` など)は誰にも観測されない。演出は途中で止まり、残りのマスは覆われたままになる。
  3. **残りのマスを片付けない。** 例外でも取り消しでも途中で止まると、残りのマスの覆いが見えたままになる。このとき、見た目と状態が食い違う。
- **テストが Green で、画面でも見えたのに見つからなかった理由**: 1 手ずつ待ってから操作すると起きない。演出の途中で続けて操作したとき、リセットしたとき、演出の長い大きな連鎖(上級)のときに起きる。
- **直し方**: 演出を `CancellationTokenSource` で管理する。新しい `Show`、リセット、盤面の作り直し、コントロールを外すとき(`OnDetachedFromVisualTree`)に、前の演出を取り消す。取り消しでも例外でも、`finally` で、残りのマスを最後の状態(覆いなし)にする。例外は捨てずに、ログに残す。
  ```csharp
  CancellationTokenSource? openingAnimation;

  public void Show(MoveResult move, bool withAnimation)
  {
      StopOpeningAnimation();                 // 前の演出を止め、覆いを最後の状態にする
      foreach (var cell in move.ChangedCells)
          cellViews[cell.Position].Update(cell);

      if (withAnimation)
          StartOpeningAnimation(move.OpenedCells);
      else
          HideCovers(move.OpenedCells);
  }
  ```
  `StartOpeningAnimation` の中では、`try { await PlayOpeningAsync(opened, token); } catch (OperationCanceledException) { } finally { HideCovers(opened); }` のように、終わり方を 1 か所にまとめる。例外は `catch` でログに渡す。`Task.Delay(delay, token)` と `FadeOutAsync(..., token)` にトークンを渡す。`FadeOutAsync` がトークンを受け取れないときは、ループの各段で `token.ThrowIfCancellationRequested()` を呼ぶ。

### 2.【正しさ】`PlayOpeningAsync` のループ: 待ち時間が積み重なり、演出が一マスずつ直列になる
- **場所**: `foreach` の中の `await Task.Delay(delay);` と、それに続く `await view.Cover.FadeOutAsync(...)`。
- **問題**: 各マスで「`delay` を待つ → 120 ms かけて消えるのを待つ」を順番に行っている。そのため、A1 の「開始からの時刻」が、前のマスが消え終わってからの時間として働く。n 番目のマスが消え始めるのは、Σdelay + 120 ms × (n−1) の後になる。同じ距離のマスも一つずつ消えるので、波紋のように広がらない。0 を開いて 200 マスが開く連鎖では、フェードだけで 24 秒かかる。この間、盤面の見た目が状態に追いつかない。操作の数が少なく、開くマスが少ないと目立たないので、画面では「見えた」になる。
- **直し方**: すべてのマスの演出を同時に始め、各マスの `delay` を開始からの時刻として使う。全部が終わるのを `Task.WhenAll` で待つ。
  ```csharp
  Task PlayOpeningAsync(IReadOnlyList<CellPosition> opened, CancellationToken token) =>
      Task.WhenAll(ViewAnimations.OpeningOrderOf(opened)
          .Select(step => FadeOutCoverAsync(cellViews[step.Position], step.Delay, token)));

  static async Task FadeOutCoverAsync(CellView view, TimeSpan delay, CancellationToken token)
  {
      await Task.Delay(delay, token);
      await view.Cover.FadeOutAsync(CoverFadeDuration, token);
      view.Cover.IsVisible = false;
  }
  ```
  A1 が誤りで、`delay` が前のマスからの間隔である場合は、フェードを待たずに次へ進むだけでよい(フェードの `await` を外して、タスクを集める)。どちらの意味なのかは `OpeningOrderOf` の名前か型(例: `StartsAt`)で表す(的確な名前)。

### 3.【Testable】演出の時間の決まりが、Avalonia を起動しないと確かめられない場所にある
- **場所**: `PlayOpeningAsync` 全体。`Task.Delay` を直に呼んでいることと、120 ms が埋め込まれていること。
- **問題**: 順序は `ViewAnimations.OpeningOrderOf` に分けてあり、そこは良い。しかし、指摘 1 と 2 の不具合は「いつ、どのマスが消え始めるか」「途中で止めたら残りはどうなるか」という時間の決まりにある。この決まりはビューの中の `Task.Delay` に埋まっている。そのため、xUnit のテストでは確かめられず、Green でも見逃す。
- **直し方**: 「開始からの時刻の列を作る」「取り消されたら残りを最後の状態にする」の判断を、ビューの外(`ViewAnimations` かビューモデル)に置く。待ち時間は `TimeProvider` で差し替えられるようにする(時刻の差し替えは YAGNI 違反ではない。判断ルール 3)。そして、次の場合を再現するテストを先に足す: (a) 同じ距離のマスが同じ時刻に始まる、(b) 演出の途中で次の手が来たら、前の演出の残りが即座に最後の状態になり、新しい手の覆いには触れない、(c) リセットの後に前の演出が覆いを消さない。これは、不具合を再現するテストを先に足してから直すという、バグ修正の進め方である。

### 4.【意図を表現・単一責務】覆いの最後の状態が、`withAnimation` によって別の場所で決まる
- **場所**: `Show` の `Update(cell)` と `if (withAnimation)`。
- **問題**: 演出がないとき、開いたマスの覆いを誰が隠すのかがこのコードからは読めない(A2)。`Update` が覆いを隠すのなら、演出があるときは、隠す前の覆いをフェードさせることになり、効果が見えない。隠さないのなら、演出がないときに覆いが残る。「覆いを最後にどの状態にするか」という一つの関心事が、`Update` と `PlayOpeningAsync` の二か所に分かれている(暗黙の順序の依存)。
- **直し方**: 指摘 1 の形で、`Show` が「演出するなら `StartOpeningAnimation`、しないなら `HideCovers`」と What を並べる。`CellView.Update` は、覆いの表示に触れない(または「演出を待つ」状態を明示する)。こうして、覆いの最後の状態は `HideCovers` の 1 か所で決まる(Once And Only Once)。

### 5.【正しさ・要確認】覆いの `Opacity` がフェードの後に 0 のまま残るおそれ
- **場所**: `await view.Cover.FadeOutAsync(...)` の後の `view.Cover.IsVisible = false;`。
- **問題**: `FadeOutAsync` が `Opacity` を 0 にする実装なら、次のゲームで `IsVisible = true` に戻しても、覆いは透明のままになる。それで、未開放のマスが開いたように見える(指摘 1 のリセットの場合と同じ症状)。`FadeOutAsync` の実装が見えないので、要確認とした。
- **直し方**: 覆いを表示に戻す側(リセット、`Update`)で `Opacity = 1` も戻す。または、フェードの後に `IsVisible = false` と `Opacity = 1` を一組で設定する「覆いを隠す」メソッドを `CellView` に置き、`HideCovers` と演出の最後の両方からそれを呼ぶ(Once And Only Once)。

### 6.【的確な名前】マジックナンバー 120
- **場所**: `TimeSpan.FromMilliseconds(120)`。
- **問題**: 120 ms が何の時間なのかは、値だけでは分からない。UI デザインで決めた値なら、その出典も読めない。
- **直し方**: `static readonly TimeSpan CoverFadeDuration = TimeSpan.FromMilliseconds(120);` のように、名前の付いた定数にする。遅延の決まり(`OpeningOrderOf`)と同じ場所に置くと、演出の時間の決まりが 1 か所にまとまる。

---

## 問題なしとしたもの
- `await` の後に UI の要素に触れている点: A3 のとおり UI スレッドで再開するので、問題はない。逆に、`ConfigureAwait(false)` を付けてはいけない。
- 順序の計算を `ViewAnimations.OpeningOrderOf` に分けてあること: 単一責務の点で良い。このまま残す。
- `withAnimation` の bool 引数: OS のアニメーション設定に従う切り替えで、呼ぶ側は一つと見られる。今はフラグ引数として問題にしない(引き算)。

## ユーザーの判断が要る点
- `delay` の意味(A1): 開始からの時刻か、前のマスからの間隔か。指摘 2 の直し方はこれで変わる。
- 演出の途中で次の手が来たとき、前の演出を即座に終わらせる(提案)のか、続けるのか。仕様・UI デザインで決まっていなければ、決める必要がある。
- 連鎖が大きいときに演出の長さに上限を設けるか(例: 全体を 600 ms に収まるように縮める)。これは仕様の判断なので、提案にとどめる。
