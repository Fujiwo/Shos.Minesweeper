using Bunit;
using Shos.Minesweeper.Components;
using Shos.Minesweeper.Display;
using Shos.Minesweeper.GameLogic;
using Shos.Minesweeper.Tests.GameLogic;

namespace Shos.Minesweeper.Tests.Components;

public class BoardViewTests : ComponentTestBase
{
    // スマートフォンの縦画面（390×700）の上級。16 列×30 行、マス 21px で表示する
    static readonly BoardPlacement TransposedExpert = BoardPlacement.Calculate(382, 636, Difficulty.Expert);

    [Fact]
    public void BoardIsAGridNamedWithTheDisplayedRowsAndColumns()
    {
        var cut = RenderBoard(new Game(Difficulty.Expert, Time), TransposedExpert);

        Assert.Equal("盤面、30 行 16 列", cut.Find("[role=grid]").GetAttribute("aria-label"));
        Assert.Equal(30, cut.FindAll("[role=row]").Count);
        Assert.Equal(480, cut.FindAll("[role=gridcell]").Count);
    }

    [Fact]
    public void CellSizeAndFrameWidthArePassedToCss()
    {
        var cut = RenderBoard(new Game(Difficulty.Expert, Time), TransposedExpert);

        var style = cut.Find("[role=grid]").GetAttribute("style");
        Assert.Contains("--cell-size: 21px", style);
        Assert.Contains("--frame-width: 3px", style);
        Assert.Contains("--column-count: 16", style);
    }

    [Fact]
    public void TransposedBoardShowsBoardColumnsAsDisplayedRows()
    {
        var cut = RenderBoard(new Game(Difficulty.Expert, Time), TransposedExpert);

        var secondCellOfFirstRow = cut.FindAll("[role=row]")[0].QuerySelectorAll("[role=gridcell]")[1];
        Assert.Equal("cell-1-0", secondCellOfFirstRow.Id);
        Assert.Equal("1 行 2 列、未開放", secondCellOfFirstRow.GetAttribute("aria-label"));
    }

    [Fact]
    public void CellsShowTheGameState()
    {
        var game = TestGames.FromPicture(TestGames.WallPicture);
        game.Open(new CellPosition(2, 0));

        var cut = RenderBoard(game, BoardPlacement.Calculate(400, 400, game.Difficulty));

        var number = cut.Find("#cell-1-1");
        Assert.Equal("cell opened n3", number.ClassName);
        Assert.Equal("2 行 2 列、3", number.GetAttribute("aria-label"));
        Assert.Equal("3", number.TextContent.Trim());
        Assert.Equal("cell closed", cut.Find("#cell-0-4").ClassName);
        Assert.Equal("cell opened", cut.Find("#cell-2-0").ClassName);
    }

    [Fact]
    public void FlaggedCellShowsTheFlagIcon()
    {
        var game = TestGames.FromPicture(TestGames.WallPicture);
        game.ToggleFlag(new CellPosition(0, 4));

        var cut = RenderBoard(game, BoardPlacement.Calculate(400, 400, game.Difficulty));

        Assert.NotNull(cut.Find("#cell-0-4 svg"));
        Assert.Empty(cut.FindAll("#cell-0-3 svg"));
    }

    IRenderedComponent<BoardView> RenderBoard(Game game, BoardPlacement placement)
        => Render<BoardView>(parameters => parameters
               .Add(board => board.Game, game)
               .Add(board => board.Placement, placement));
}
