using Bunit;
using Shos.Minesweeper.Components;
using Shos.Minesweeper.Display;
using Shos.Minesweeper.GameLogic;
using Shos.Minesweeper.TestSupport;

namespace Shos.Minesweeper.Tests.Components;

public class BoardViewTests : AppTestContext
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

    // 演出（UI デザイン 10.7。1.1.0）。左の 2 列が開き、(2, 0) から最も遠い (0, 1) の遅れの比が 1 になる（BoardAnimationTests）

    [Fact]
    public void AnimatedCellsGetTheAnimationClassAndDelayRatio()
    {
        var game = TestGames.FromPicture(TestGames.WallPicture);
        var move = game.Open(new CellPosition(2, 0));

        var cut = RenderBoard(game, WallPlacement, move);

        Assert.Equal("cell opened reveal", cut.Find("#cell-2-0").ClassName);
        Assert.Equal("--delay-ratio: 0", cut.Find("#cell-2-0").GetAttribute("style"));
        Assert.Equal("--delay-ratio: 1", cut.Find("#cell-0-1").GetAttribute("style"));
        Assert.Equal("cell closed", cut.Find("#cell-0-4").ClassName);
        Assert.False(cut.Find("#cell-0-4").HasAttribute("style"));
    }

    // 小数点に「,」を使う言語の端末でも、CSS の値は「.」で書く。(2, 1) の比は 1 ÷ √5
    [Fact]
    public void DelayRatioIsWrittenWithAPeriodInAnyCulture()
    {
        using var culture = UseCulture("de-DE");
        var game = TestGames.FromPicture(TestGames.WallPicture);
        var move = game.Open(new CellPosition(2, 0));

        var cut = RenderBoard(game, WallPlacement, move);

        Assert.StartsWith("--delay-ratio: 0.447", cut.Find("#cell-2-1").GetAttribute("style"));
    }

    // 押下中の表示のために描き直しても、演出のクラスと変数は変わらないので、アニメーションは続く（アーキテクチャー設計書 8.6）
    [Fact]
    public void PressingKeepsTheAnimations()
    {
        var game = TestGames.FromPicture(TestGames.WallPicture);
        var move = game.Open(new CellPosition(2, 0));
        var cut = RenderBoard(game, WallPlacement, move);

        cut.Find("#cell-0-4").PointerDown(Mouse());

        Assert.Contains("pressed", cut.Find("#cell-0-4").ClassList);
        Assert.Equal("cell opened n2 reveal", cut.Find("#cell-0-1").ClassName);
        Assert.Equal("--delay-ratio: 1", cut.Find("#cell-0-1").GetAttribute("style"));
    }

    [Fact]
    public void NewGameEndsTheAnimations()
    {
        var game = TestGames.FromPicture(TestGames.WallPicture);
        var move = game.Open(new CellPosition(2, 0));
        var cut = RenderBoard(game, WallPlacement, move);

        cut.Render(parameters => parameters
            .Add(board => board.Game, TestGames.FromPicture(TestGames.WallPicture))
            .Add(board => board.LastMove, null));

        Assert.Empty(cut.FindAll(".reveal"));
        Assert.Empty(cut.FindAll("[role=gridcell][style]"));
    }

    static readonly BoardPlacement WallPlacement = BoardPlacement.Calculate(400, 400, TestGames.DifficultyOf(TestGames.WallPicture));

    IRenderedComponent<BoardView> RenderBoard(Game game, BoardPlacement placement, MoveResult? lastMove = null)
        => Render<BoardView>(parameters => parameters
               .Add(board => board.Game, game)
               .Add(board => board.Placement, placement)
               .Add(board => board.LastMove, lastMove));
}
