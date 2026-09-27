using Shos.Minesweeper.GameLogic;

namespace Shos.Minesweeper.Presentation.Tests;

/// <summary>
/// キーボードで選択しているマス（デスクトップ版・コンソール版のクラス設計書 3.2）。盤面の座標で動く。
/// 表示で縦と横を入れ替えたときの方向は、Web 版の BoardPlacementTests（ToBoard）で確かめる。
/// </summary>
public class BoardCursorTests
{
    // 上級は 30 列×16 行
    static readonly Board Expert = new Game(Difficulty.Expert, TimeProvider.System).Board;

    [Fact]
    public void CursorStartsAtTheTopLeftCell()
        => Assert.Equal(new CellPosition(0, 0), new BoardCursor().Position);

    [Theory]
    [InlineData(Direction.Down,  1, 0)]
    [InlineData(Direction.Right, 0, 1)]
    public void CursorMovesOneCellInTheDirection(Direction direction, int row, int column)
    {
        var cursor = new BoardCursor();

        cursor.Move(direction, Expert);

        Assert.Equal(new CellPosition(row, column), cursor.Position);
    }

    [Fact]
    public void CursorMovesBackUpAndLeft()
    {
        var cursor = new BoardCursor();
        cursor.Move(Direction.Down, Expert);
        cursor.Move(Direction.Right, Expert);

        cursor.Move(Direction.Up, Expert);
        cursor.Move(Direction.Left, Expert);

        Assert.Equal(new CellPosition(0, 0), cursor.Position);
    }

    [Theory]
    [InlineData(Direction.Up)]
    [InlineData(Direction.Left)]
    public void CursorStopsAtTheTopAndLeftEdges(Direction direction)
    {
        var cursor = new BoardCursor();

        cursor.Move(direction, Expert);

        Assert.Equal(new CellPosition(0, 0), cursor.Position);
    }

    [Fact]
    public void CursorStopsAtTheRightEdge()
    {
        var cursor = new BoardCursor();

        for (var step = 0; step < 40; step++)
            cursor.Move(Direction.Right, Expert);

        Assert.Equal(new CellPosition(0, 29), cursor.Position);
    }

    [Fact]
    public void CursorStopsAtTheBottomEdge()
    {
        var cursor = new BoardCursor();

        for (var step = 0; step < 40; step++)
            cursor.Move(Direction.Down, Expert);

        Assert.Equal(new CellPosition(15, 0), cursor.Position);
    }

    // マウスで押したマスへ移す（デスクトップ版のアーキテクチャー設計書 14 章の決定 13）
    [Fact]
    public void CursorMovesToTheGivenCell()
    {
        var cursor = new BoardCursor();

        cursor.MoveTo(new CellPosition(15, 29), Expert);

        Assert.Equal(new CellPosition(15, 29), cursor.Position);
    }

    [Fact]
    public void CursorMovesOnFromTheCellItWasMovedTo()
    {
        var cursor = new BoardCursor();
        cursor.MoveTo(new CellPosition(5, 7), Expert);

        cursor.Move(Direction.Right, Expert);

        Assert.Equal(new CellPosition(5, 8), cursor.Position);
    }

    // 盤面の外の位置は、呼ぶ側の誤り（前提条件の破れ）なので、その場で知らせる（クラス設計書レビューの指摘 3）
    [Theory]
    [InlineData(16, 0)]
    [InlineData(0, 30)]
    [InlineData(-1, 0)]
    public void MovingToACellOutsideTheBoardIsAnError(int row, int column)
    {
        var cursor = new BoardCursor();

        Assert.Throws<ArgumentOutOfRangeException>(() => cursor.MoveTo(new CellPosition(row, column), Expert));
        Assert.Equal(new CellPosition(0, 0), cursor.Position);
    }
}
