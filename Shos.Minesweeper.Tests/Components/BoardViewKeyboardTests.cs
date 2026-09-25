using Bunit;
using Microsoft.AspNetCore.Components.Web;
using Shos.Minesweeper.Components;
using Shos.Minesweeper.Display;
using Shos.Minesweeper.GameLogic;
using Shos.Minesweeper.Tests.GameLogic;

namespace Shos.Minesweeper.Tests.Components;

/// <summary>盤面のキーボードの操作（仕様書 4.5、UI デザイン 6.3）。</summary>
public class BoardViewKeyboardTests : ComponentTestBase
{
    readonly List<CellPosition> opened = [];
    readonly List<CellPosition> flagged = [];

    [Fact]
    public void BoardIsOneTabStopThatPointsToTheSelectedCell()
    {
        var grid = RenderBoard(new Game(Difficulty.Beginner, Time)).Find("[role=grid]");

        Assert.Equal("0", grid.GetAttribute("tabindex"));
        Assert.Equal("cell-0-0", grid.GetAttribute("aria-activedescendant"));
    }

    [Fact]
    public void SelectedCellIsMarked()
    {
        var cut = RenderBoard(new Game(Difficulty.Beginner, Time));

        Assert.Equal(["cell-0-0"], cut.FindAll(".selected").Select(cell => cell.Id));
    }

    [Fact]
    public void ArrowKeysMoveTheSelectedCell()
    {
        var cut = RenderBoard(new Game(Difficulty.Beginner, Time));

        Press(cut, "ArrowRight");
        Press(cut, "ArrowDown");

        Assert.Equal("cell-1-1", cut.Find("[role=grid]").GetAttribute("aria-activedescendant"));
        Assert.Equal(["cell-1-1"], cut.FindAll(".selected").Select(cell => cell.Id));
    }

    [Fact]
    public void ArrowKeysFollowTheDisplayedDirectionOfATransposedBoard()
    {
        var game = new Game(Difficulty.Expert, Time);
        var cut = RenderBoard(game, BoardPlacement.Calculate(382, 636, Difficulty.Expert));

        Press(cut, "ArrowRight");

        Assert.Equal("cell-1-0", cut.Find("[role=grid]").GetAttribute("aria-activedescendant"));
    }

    [Theory]
    [InlineData(" ")]
    [InlineData("Enter")]
    public void SpaceAndEnterRequestOpeningTheSelectedCell(string key)
    {
        var cut = RenderBoard(new Game(Difficulty.Beginner, Time));
        Press(cut, "ArrowRight");

        Press(cut, key);

        Assert.Equal([new CellPosition(0, 1)], opened);
    }

    [Theory]
    [InlineData("f")]
    [InlineData("F")]
    public void FRequestsFlaggingTheSelectedCell(string key)
    {
        var cut = RenderBoard(new Game(Difficulty.Beginner, Time));

        Press(cut, key);

        Assert.Equal([new CellPosition(0, 0)], flagged);
    }

    [Fact]
    public void OtherKeysDoNothing()
    {
        var cut = RenderBoard(new Game(Difficulty.Beginner, Time));

        Press(cut, "a");
        Press(cut, "Tab");

        Assert.Empty(opened);
        Assert.Empty(flagged);
        Assert.Equal("cell-0-0", cut.Find("[role=grid]").GetAttribute("aria-activedescendant"));
    }

    [Fact]
    public void AfterTheGameIsOverArrowsStillMoveButOpenAndFlagDoNothing()
    {
        var game = TestGames.FromPicture(TestGames.WallPicture);
        game.Open(new CellPosition(2, 0));
        game.Open(new CellPosition(2, 2));
        var cut = RenderBoard(game);

        Press(cut, "ArrowRight");
        Press(cut, "Enter");
        Press(cut, "f");

        Assert.Equal("cell-0-1", cut.Find("[role=grid]").GetAttribute("aria-activedescendant"));
        Assert.Empty(opened);
        Assert.Empty(flagged);
    }

    [Fact]
    public void NewGameSelectsTheTopLeftCellAgain()
    {
        var cut = RenderBoard(new Game(Difficulty.Beginner, Time));
        Press(cut, "ArrowRight");

        cut.Render(parameters => parameters.Add(board => board.Game, new Game(Difficulty.Beginner, Time)));

        Assert.Equal("cell-0-0", cut.Find("[role=grid]").GetAttribute("aria-activedescendant"));
    }

    [Fact]
    public void ArrowAndSpaceKeysDoNotScrollThePage()
    {
        RenderBoard(new Game(Difficulty.Beginner, Time));

        Assert.Contains(JSInterop.Invocations, invocation => invocation.Identifier == "suppressKeyScrolling");
    }

    IRenderedComponent<BoardView> RenderBoard(Game game, BoardPlacement? placement = null)
        => Render<BoardView>(parameters => parameters
               .Add(board => board.Game, game)
               .Add(board => board.Placement, placement ?? BoardPlacement.Calculate(400, 400, game.Difficulty))
               .Add(board => board.OnOpen, (CellPosition position) => opened.Add(position))
               .Add(board => board.OnToggleFlag, (CellPosition position) => flagged.Add(position)));

    static void Press(IRenderedComponent<BoardView> cut, string key)
        => cut.Find("[role=grid]").KeyDown(new KeyboardEventArgs { Key = key });
}
