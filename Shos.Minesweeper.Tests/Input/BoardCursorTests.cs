using Shos.Minesweeper.Display;
using Shos.Minesweeper.GameLogic;
using Shos.Minesweeper.Input;

namespace Shos.Minesweeper.Tests.Input;

/// <summary>キーボードで選択しているマス（仕様書 4.5、クラス設計書 4.2）。</summary>
public class BoardCursorTests
{
    // 上級を、そのままの向き（30 列×16 行）と、縦と横を入れ替えた向き（16 列×30 行）で置いたもの
    static readonly BoardPlacement ExpertAsIs = BoardPlacement.Calculate(1280, 594, Difficulty.Expert);
    static readonly BoardPlacement ExpertTransposed = BoardPlacement.Calculate(382, 636, Difficulty.Expert);

    [Fact]
    public void CursorStartsAtTheTopLeftCell()
        => Assert.Equal(new CellPosition(0, 0), new BoardCursor().Position);

    [Theory]
    [InlineData(Direction.Down,  1, 0)]
    [InlineData(Direction.Right, 0, 1)]
    public void CursorMovesOneCellInTheDisplayedDirection(Direction direction, int row, int column)
    {
        var cursor = new BoardCursor();

        cursor.Move(direction, ExpertAsIs);

        Assert.Equal(new CellPosition(row, column), cursor.Position);
    }

    [Fact]
    public void CursorMovesBackUpAndLeft()
    {
        var cursor = new BoardCursor();
        cursor.Move(Direction.Down, ExpertAsIs);
        cursor.Move(Direction.Right, ExpertAsIs);

        cursor.Move(Direction.Up, ExpertAsIs);
        cursor.Move(Direction.Left, ExpertAsIs);

        Assert.Equal(new CellPosition(0, 0), cursor.Position);
    }

    [Theory]
    [InlineData(Direction.Up)]
    [InlineData(Direction.Left)]
    public void CursorStopsAtTheTopAndLeftEdges(Direction direction)
    {
        var cursor = new BoardCursor();

        cursor.Move(direction, ExpertAsIs);

        Assert.Equal(new CellPosition(0, 0), cursor.Position);
    }

    [Fact]
    public void CursorStopsAtTheRightEdge()
    {
        var cursor = new BoardCursor();

        for (var step = 0; step < 40; step++)
            cursor.Move(Direction.Right, ExpertAsIs);

        Assert.Equal(new CellPosition(0, 29), cursor.Position);
    }

    [Fact]
    public void CursorFollowsTheDisplayedDirectionWhenTheBoardIsTransposed()
    {
        var cursor = new BoardCursor();

        cursor.Move(Direction.Right, ExpertTransposed);   // 表示で右 = 盤面で下

        Assert.Equal(new CellPosition(1, 0), cursor.Position);
    }

    [Fact]
    public void CursorStopsAtTheBottomEdgeOfTheTransposedBoard()
    {
        var cursor = new BoardCursor();

        for (var step = 0; step < 40; step++)
            cursor.Move(Direction.Down, ExpertTransposed);   // 表示で下 = 盤面で右。表示は 30 行

        Assert.Equal(new CellPosition(0, 29), cursor.Position);
    }

    [Fact]
    public void CursorKeepsTheSameCellWhenTheOrientationChanges()
    {
        var cursor = new BoardCursor();
        cursor.Move(Direction.Right, ExpertAsIs);
        cursor.Move(Direction.Right, ExpertAsIs);

        cursor.Move(Direction.Down, ExpertTransposed);   // 向きが変わった後: (0, 2) は表示で (2, 0)。下は盤面で右

        Assert.Equal(new CellPosition(0, 3), cursor.Position);
    }
}
