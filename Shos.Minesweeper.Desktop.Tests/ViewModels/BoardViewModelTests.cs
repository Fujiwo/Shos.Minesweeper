using System.ComponentModel;
using Avalonia;
using Avalonia.Input;
using Microsoft.Extensions.Time.Testing;
using Shos.Minesweeper.Desktop.ViewModels;
using Shos.Minesweeper.GameLogic;
using Shos.Minesweeper.Presentation;
using Shos.Minesweeper.TestSupport;

namespace Shos.Minesweeper.Desktop.Tests.ViewModels;

/// <summary>盤面とマスのビューモデル（クラス設計書 4.4）。操作の意図は requestAction で受け、盤面を変えるのは GameSession を直接呼んで確かめる。</summary>
public class BoardViewModelTests
{
    readonly GameSession session = new(Difficulty.Beginner, new FakeTimeProvider(), chooseMines: TestGames.MineChooserOf(TestGames.BeginnerWallPicture));
    readonly List<(CellAction Action, CellPosition Position)> requests = [];
    readonly BoardViewModel board;
    int pressingChangedCount;

    public BoardViewModelTests()
        => board = new BoardViewModel(session, (action, position) => requests.Add((action, position)), () => pressingChangedCount++);

    [Fact]
    public void BoardHasACellForEachPositionInRowOrder()
    {
        Assert.Equal(81, board.Cells.Count);
        Assert.Equal(new CellPosition(1, 0), board.Cells[9].Position);
    }

    [Fact]
    public void IndexOfFindsThePositionInCells()
    {
        Assert.All(board.Cells, cell => Assert.Same(cell, board.Cells[board.IndexOf(cell.Position)]));
    }

    [Fact]
    public void BoardAndCellsHaveAccessibleNames()
    {
        Assert.Equal("盤面、9 行 9 列", board.AccessibleName);
        Assert.Equal("1 行 1 列、未開放", board.Cells[0].AccessibleName);
    }

    [Fact]
    public void CellSizeIsTheDefaultBeforeTheAreaIsKnown()
        => Assert.Equal(32, board.CellSize);

    [Fact]
    public void CellSizeFollowsTheArea()
    {
        board.SetAreaSize(new Size(456, 456));   // (456 − 6) ÷ 9 = 50 → 上限の 48

        Assert.Equal(48, board.CellSize);
        Assert.Equal(9 * 48, board.CellsWidth);
    }

    [Fact]
    public void PressingAClosedCellShowsOnlyThatCellPressed()
    {
        board.Press(new CellPosition(2, 3));

        Assert.True(board.IsPressing);
        Assert.Equal([new CellPosition(2, 3)], PressedPositions());
        Assert.Equal(1, pressingChangedCount);
        Assert.Equal(new CellPosition(2, 3), board.CursorPosition);
    }

    // 開いた数字のマスを押したときは、コードで開く範囲を押下中にする（Web 版の UI デザイン 5.1）
    [Fact]
    public void PressingAnOpenedNumberShowsTheChordTargetsPressed()
    {
        Show(session.Open(new CellPosition(0, 0)));
        Show(session.ToggleFlag(new CellPosition(0, 4)));

        board.Press(new CellPosition(0, 3));

        Assert.Equal([new CellPosition(1, 4)], PressedPositions());
    }

    [Fact]
    public void ReleasingOverThePressedCellOpensIt()
    {
        board.Press(new CellPosition(2, 3));

        board.Release(isOverPressedCell: true);

        Assert.Equal([(CellAction.Open, new CellPosition(2, 3))], requests);
        Assert.False(board.IsPressing);
        Assert.Empty(PressedPositions());
        Assert.Equal(2, pressingChangedCount);
    }

    // 押したマスの外で離したら取り消す（クラス設計書 9.1 の決定 1）
    [Fact]
    public void ReleasingOutsideThePressedCellCancels()
    {
        board.Press(new CellPosition(2, 3));

        board.Release(isOverPressedCell: false);

        Assert.Empty(requests);
        Assert.False(board.IsPressing);
    }

    [Fact]
    public void LosingThePointerCancelsThePress()
    {
        board.Press(new CellPosition(2, 3));

        board.CancelPress();

        Assert.Empty(requests);
        Assert.False(board.IsPressing);
    }

    [Fact]
    public void ReleasingWithoutPressingDoesNothing()
    {
        board.Release(isOverPressedCell: true);

        Assert.Empty(requests);
        Assert.Equal(0, pressingChangedCount);
    }

    // 旗のマスをタップしても、開かない（Web 版の仕様書 3.4 の「何もしない」）
    [Fact]
    public void ReleasingOverAFlagDoesNothing()
    {
        Show(session.ToggleFlag(new CellPosition(2, 3)));
        board.Press(new CellPosition(2, 3));

        board.Release(isOverPressedCell: true);

        Assert.Empty(requests);
    }

    [Fact]
    public void RightButtonTogglesTheFlagAtOnce()
    {
        board.PressRight(new CellPosition(4, 5));

        Assert.Equal([(CellAction.ToggleFlag, new CellPosition(4, 5))], requests);
        Assert.Equal(new CellPosition(4, 5), board.CursorPosition);
    }

    [Fact]
    public void RightButtonOnAnOpenedCellDoesNothing()
    {
        Show(session.Open(new CellPosition(0, 0)));

        board.PressRight(new CellPosition(0, 0));

        Assert.Empty(requests);
    }

    // 勝敗が決まった後は、押下中にもせず、操作もしない。カーソルは動く
    [Fact]
    public void AfterTheGameIsOverPressesOnlyMoveTheCursor()
    {
        Lose();

        board.Press(new CellPosition(5, 5));
        board.Release(isOverPressedCell: true);
        board.PressRight(new CellPosition(6, 6));

        Assert.False(board.IsPressing);
        Assert.Empty(requests);
        Assert.Equal(new CellPosition(6, 6), board.CursorPosition);
    }

    [Fact]
    public void ArrowKeysMoveTheCursor()
    {
        Assert.True(board.HandleKey(Key.Right));
        Assert.True(board.HandleKey(Key.Down));

        Assert.Equal(new CellPosition(1, 1), board.CursorPosition);
    }

    [Theory]
    [InlineData(Key.Space, CellAction.Open)]
    [InlineData(Key.Enter, CellAction.Open)]
    [InlineData(Key.F,     CellAction.ToggleFlag)]
    public void KeysActOnTheCursorCell(Key key, CellAction action)
    {
        board.HandleKey(Key.Right);

        Assert.True(board.HandleKey(key));

        Assert.Equal([(action, new CellPosition(0, 1))], requests);
    }

    [Fact]
    public void OtherKeysAreNotHandled()
        => Assert.False(board.HandleKey(Key.A));

    [Fact]
    public void AfterTheGameIsOverArrowsMoveButSpaceDoesNothing()
    {
        Lose();

        board.HandleKey(Key.Left);
        board.HandleKey(Key.Space);

        Assert.Equal(new CellPosition(0, 3), board.CursorPosition);
        Assert.Empty(requests);
    }

    // 描き直しを 100 ミリ秒以内に保つため、変わったマスだけを知らせる（アーキテクチャー設計書 7.3）
    [Fact]
    public void ShowingAMoveNotifiesOnlyTheChangedCells()
    {
        var notified = WatchCells();

        var move = session.Open(new CellPosition(0, 0));
        board.Show(move, withAnimation: false);

        Assert.Equal(move.OpenedPositions.ToHashSet(), notified);
    }

    [Fact]
    public void ShowingAMoveUpdatesTheCell()
    {
        Show(session.Open(new CellPosition(0, 0)));

        Assert.Equal(CellAppearance.Opened, board.Cells[3].Appearance);
        Assert.Equal(2, board.Cells[3].Number);
        Assert.Equal("1 行 4 列、2", board.Cells[3].AccessibleName);
    }

    [Fact]
    public void NumberIsZeroForBlankAndClosedCells()
    {
        Show(session.Open(new CellPosition(0, 0)));

        Assert.Equal(0, board.Cells[0].Number);
        Assert.Equal(0, board.Cells[8].Number);
    }

    [Fact]
    public void MoveThatChangedNothingNotifiesNothing()
    {
        Show(session.Open(new CellPosition(0, 0)));
        var notified = WatchCells();

        board.Show(session.Open(new CellPosition(0, 0)), withAnimation: false);

        Assert.Empty(notified);
    }

    // 勝敗が決まったら、地雷の表示と自動の旗が変わるので、全マスを知らせる
    [Fact]
    public void LosingNotifiesEveryCell()
    {
        Show(session.Open(new CellPosition(0, 0)));
        var notified = WatchCells();

        Show(session.Open(new CellPosition(0, 4)));

        Assert.Equal(81, notified.Count);
    }

    // 新しいゲームでは、カーソルを左上に戻す（クラス設計書 9.1 の決定 9）
    [Fact]
    public void NewGameOfTheSameSizeKeepsTheCellsAndResetsTheCursor()
    {
        var cells = board.Cells;
        board.HandleKey(Key.Down);
        Show(session.Open(new CellPosition(0, 0)));

        session.StartNewGame(Difficulty.Beginner);
        board.ShowNewGame();

        Assert.Same(cells, board.Cells);
        Assert.Equal(CellAppearance.Closed, board.Cells[0].Appearance);
        Assert.Equal(new CellPosition(0, 0), board.CursorPosition);
    }

    [Fact]
    public void NewGameOfAnotherSizeMakesNewCells()
    {
        session.StartNewGame(Difficulty.Intermediate);

        board.ShowNewGame();

        Assert.Equal(256, board.Cells.Count);
        Assert.Equal(16, board.ColumnCount);
        Assert.Equal("盤面、16 行 16 列", board.AccessibleName);
    }

    [Fact]
    public void NewGameClearsThePress()
    {
        board.Press(new CellPosition(2, 3));

        session.StartNewGame(Difficulty.Beginner);
        board.ShowNewGame();

        Assert.False(board.IsPressing);
        Assert.Empty(PressedPositions());
    }

    // 演出（Web 版の UI デザイン 10.7）。種類と遅れの比は BoardAnimation が決め、盤面はマスに付けるだけにする
    [Fact]
    public void AnimatedOpeningRevealsTheOpenedCells()
    {
        var move = session.Open(new CellPosition(0, 0));

        ShowAnimated(move);

        var animated = board.Cells.Where(cell => cell.Animation is not null).ToList();
        Assert.Equal(move.OpenedPositions.ToHashSet(), animated.Select(cell => cell.Position).ToHashSet());
        Assert.All(animated, cell => Assert.Equal(CellAnimationKind.Reveal, cell.Animation!.Value.Kind));
        Assert.Equal(0, board.Cells[0].Animation!.Value.DelayRatio);
    }

    // アニメーション効果がオフなら、演出を付けない（UI デザイン 2.10）
    [Fact]
    public void OpeningWithoutAnimationAddsNoAnimation()
    {
        Show(session.Open(new CellPosition(0, 0)));

        Assert.All(board.Cells, cell => Assert.Null(cell.Animation));
    }

    [Fact]
    public void AnimationChangesAreNotified()
    {
        var changed = new List<string?>();
        ((INotifyPropertyChanged)board.Cells[0]).PropertyChanged += (_, e) => changed.Add(e.PropertyName);

        ShowAnimated(session.Open(new CellPosition(0, 0)));

        Assert.Contains(nameof(CellViewModel.Animation), changed);
    }

    [Fact]
    public void NextMoveClearsThePreviousAnimations()
    {
        ShowAnimated(session.Open(new CellPosition(0, 0)));

        ShowAnimated(session.ToggleFlag(new CellPosition(0, 4)));

        Assert.All(board.Cells, cell => Assert.Null(cell.Animation));
    }

    // 何も起きなかった操作では、前の演出を止めない（Web 版と同じ）
    [Fact]
    public void MoveThatChangedNothingKeepsTheAnimations()
    {
        ShowAnimated(session.Open(new CellPosition(0, 0)));

        ShowAnimated(session.Open(new CellPosition(0, 0)));

        Assert.NotNull(board.Cells[0].Animation);
    }

    // 旗を立てたら、旗が広がる（Web 版の UI デザイン 5.2）。外したときは広がらない
    [Fact]
    public void PlacingAFlagMarksItAsJustPlaced()
    {
        ShowAnimated(session.ToggleFlag(new CellPosition(2, 3)));
        Assert.True(board.Cells[board.IndexOf(new CellPosition(2, 3))].IsFlagJustPlaced);

        ShowAnimated(session.ToggleFlag(new CellPosition(2, 3)));
        Assert.False(board.Cells[board.IndexOf(new CellPosition(2, 3))].IsFlagJustPlaced);
    }

    // 旗が広がる演出も、アニメーション効果がオフなら出さない（クラス設計書 9.1 の決定 4）
    [Fact]
    public void PlacingAFlagWithoutAnimationDoesNotMarkIt()
    {
        Show(session.ToggleFlag(new CellPosition(2, 3)));

        Assert.False(board.Cells[board.IndexOf(new CellPosition(2, 3))].IsFlagJustPlaced);
    }

    [Fact]
    public void NextMoveClearsTheJustPlacedFlag()
    {
        ShowAnimated(session.ToggleFlag(new CellPosition(2, 3)));

        ShowAnimated(session.Open(new CellPosition(0, 0)));

        Assert.False(board.Cells[board.IndexOf(new CellPosition(2, 3))].IsFlagJustPlaced);
    }

    [Fact]
    public void LosingAnimatesTheExplosionAndTheMines()
    {
        Show(session.Open(new CellPosition(0, 0)));

        ShowAnimated(session.Open(new CellPosition(0, 4)));

        Assert.Equal(CellAnimationKind.Explode, board.Cells[4].Animation!.Value.Kind);
        Assert.Equal(CellAnimationKind.MineAppear, board.Cells[9 + 4].Animation!.Value.Kind);
    }

    [Fact]
    public void NewGameClearsTheAnimations()
    {
        ShowAnimated(session.ToggleFlag(new CellPosition(2, 3)));
        ShowAnimated(session.Open(new CellPosition(0, 0)));

        session.StartNewGame(Difficulty.Beginner);
        board.ShowNewGame();

        Assert.All(board.Cells, cell => Assert.Null(cell.Animation));
        Assert.All(board.Cells, cell => Assert.False(cell.IsFlagJustPlaced));
    }

    void Lose()
    {
        Show(session.Open(new CellPosition(0, 0)));
        Show(session.Open(new CellPosition(0, 4)));
        board.PressRight(new CellPosition(0, 4));   // カーソルを (0, 4) に置く。負けた後なので操作はしない
    }

    void Show(MoveResult move) => board.Show(move, withAnimation: false);

    void ShowAnimated(MoveResult move) => board.Show(move, withAnimation: true);

    CellPosition[] PressedPositions() => [.. board.Cells.Where(cell => cell.IsPressed).Select(cell => cell.Position)];

    HashSet<CellPosition> WatchCells()
    {
        var notified = new HashSet<CellPosition>();
        foreach (var cell in board.Cells)
            ((INotifyPropertyChanged)cell).PropertyChanged += (_, _) => notified.Add(cell.Position);
        return notified;
    }
}
