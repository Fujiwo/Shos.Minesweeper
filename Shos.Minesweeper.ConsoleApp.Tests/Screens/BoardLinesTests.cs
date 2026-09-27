using Shos.Minesweeper.ConsoleApp.Screens;
using Shos.Minesweeper.GameLogic;

namespace Shos.Minesweeper.ConsoleApp.Tests.Screens;

/// <summary>盤面の行（UI デザイン 3.2、3.4）。1 マスは 2 升（区切り 1 升と記号 1 升）で、カーソルのマスは角かっこで挟む。</summary>
public class BoardLinesTests
{
    static readonly Game Beginner = new(Difficulty.Beginner, TimeProvider.System);

    [Fact]
    public void BeginnerBoardWithTheCursorAtTheTopLeftMatchesTheUiDesign()
        => Assert.Equal([
            "+-------------------+",
            "|[#]# # # # # # # # |",
            "| # # # # # # # # # |",
            "| # # # # # # # # # |",
            "| # # # # # # # # # |",
            "| # # # # # # # # # |",
            "| # # # # # # # # # |",
            "| # # # # # # # # # |",
            "| # # # # # # # # # |",
            "| # # # # # # # # # |",
            "+-------------------+",
        ], TextsOf(BoardLines.Of(Beginner, new CellPosition(0, 0))));

    // 右端の列では、閉じる角かっこが枠の前の区切りの升に来る
    [Fact]
    public void CursorAtTheRightEdgeClosesBeforeTheFrame()
        => Assert.Equal("| # # # # # # # #[#]|", TextsOf(BoardLines.Of(Beginner, new CellPosition(0, 8)))[1]);

    [Fact]
    public void CursorInTheMiddleIsBracketedOnBothSides()
        => Assert.Equal("| # # # #[#]# # # # |", TextsOf(BoardLines.Of(Beginner, new CellPosition(4, 4)))[5]);

    // 色を付けるときは、カーソルのマスの記号を反転表示にもする（UI デザイン 3.4）
    [Fact]
    public void CursorCellIsReversed()
    {
        var cursorRow = BoardLines.Of(Beginner, new CellPosition(0, 0))[1];

        var glyph = Assert.Single(cursorRow.Parts, part => part.Style.IsReversed);
        Assert.Equal("#", glyph.Text);
    }

    // 上級の盤面の 1 行は 63 升で、80 列の端末に収まる（UI デザイン 3.2）
    [Fact]
    public void ExpertRowIsSixtyThreeColumnsWide()
        => Assert.All(TextsOf(BoardLines.Of(new Game(Difficulty.Expert, TimeProvider.System), new CellPosition(0, 0))),
                      line => Assert.Equal(63, line.Length));

    static string[] TextsOf(IEnumerable<ConsoleApp.Rendering.FrameLine> lines) => [.. lines.Select(line => line.Text)];
}
