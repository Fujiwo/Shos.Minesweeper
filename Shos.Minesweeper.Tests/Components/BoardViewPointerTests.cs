using Bunit;
using Shos.Minesweeper.Components;
using Shos.Minesweeper.Display;
using Shos.Minesweeper.GameLogic;
using Shos.Minesweeper.Input;
using Shos.Minesweeper.TestSupport;

namespace Shos.Minesweeper.Tests.Components;

/// <summary>盤面のマウスとタッチの操作（仕様書 4.1、4.4、UI デザイン 5.1、5.2）。</summary>
public class BoardViewPointerTests : AppTestContext
{
    // (2, 0) から開くと、左の 2 列が開き、(1, 1) は周りに未開放の地雷が 3 つある「3」になる
    readonly Game game = TestGames.FromPicture(TestGames.WallPicture);
    readonly List<CellPosition> opened = [];
    readonly List<CellPosition> flagged = [];
    readonly List<bool> pressingChanges = [];

    public BoardViewPointerTests() => game.Open(new CellPosition(2, 0));

    [Fact]
    public void MouseClickOnClosedCellRequestsOpen()
    {
        var cut = RenderBoard();

        Click(cut, "#cell-2-3", Mouse());

        Assert.Equal([new CellPosition(2, 3)], opened);
        Assert.Empty(flagged);
    }

    [Fact]
    public void TouchTapOnClosedCellRequestsOpen()
    {
        var cut = RenderBoard();

        Click(cut, "#cell-2-3", Touch());

        Assert.Equal([new CellPosition(2, 3)], opened);
    }

    [Fact]
    public void RightClickRequestsFlagWhenTheButtonIsPressed()
    {
        var cut = RenderBoard();

        cut.Find("#cell-2-3").PointerDown(Mouse(button: 2));

        Assert.Equal([new CellPosition(2, 3)], flagged);
    }

    [Fact]
    public void TapInFlagModeRequestsFlag()
    {
        var cut = RenderBoard(isFlagMode: true);

        Click(cut, "#cell-2-3", Touch());

        Assert.Equal([new CellPosition(2, 3)], flagged);
    }

    [Fact]
    public void LongPressRequestsFlagAndVibrates()
    {
        var cut = RenderBoard();
        cut.Find("#cell-2-3").PointerDown(Touch());

        Time.Advance(PressGesture.LongPressDelay);

        cut.WaitForAssertion(() => Assert.Equal([new CellPosition(2, 3)], flagged));
        var vibration = Assert.Single(JSInterop.Invocations, invocation => invocation.Identifier == "vibrate");
        Assert.Equal(30, vibration.Arguments[0]);
    }

    [Fact]
    public void LongPressInFlagModeRequestsOpen()
    {
        var cut = RenderBoard(isFlagMode: true);
        cut.Find("#cell-2-3").PointerDown(Touch());

        Time.Advance(PressGesture.LongPressDelay);

        cut.WaitForAssertion(() => Assert.Equal([new CellPosition(2, 3)], opened));
    }

    [Fact]
    public void ReleasingAfterLongPressDoesNotRequestAgain()
    {
        var cut = RenderBoard();
        cut.Find("#cell-2-3").PointerDown(Touch());
        Time.Advance(PressGesture.LongPressDelay);
        cut.WaitForAssertion(() => Assert.Single(flagged));

        cut.Find("[role=grid]").PointerUp(Touch());

        Assert.Single(flagged);
        Assert.Empty(opened);
    }

    [Fact]
    public void LongPressThatDoesNothingNeitherRequestsNorVibrates()
    {
        var cut = RenderBoard();
        cut.Find("#cell-1-1").PointerDown(Touch());   // 開いた数字のマス。通常のモードの長押し（旗）は効かない

        Time.Advance(PressGesture.LongPressDelay);

        Assert.Empty(flagged);
        Assert.Empty(opened);
        Assert.DoesNotContain(JSInterop.Invocations, invocation => invocation.Identifier == "vibrate");
    }

    [Fact]
    public void MovingTheFingerCancelsTheTap()
    {
        var cut = RenderBoard();
        cut.Find("#cell-2-3").PointerDown(Touch(x: 100, y: 100));

        cut.Find("[role=grid]").PointerMove(Touch(x: 110, y: 100));
        cut.Find("[role=grid]").PointerUp(Touch(x: 110, y: 100));

        Assert.Empty(opened);
    }

    [Fact]
    public void LeavingTheBoardCancelsTheClick()
    {
        var cut = RenderBoard();
        cut.Find("#cell-2-3").PointerDown(Mouse());

        cut.Find("[role=grid]").PointerLeave(Mouse());
        cut.Find("[role=grid]").PointerUp(Mouse());

        Assert.Empty(opened);
    }

    [Fact]
    public void PressedClosedCellIsShownPressedUntilReleased()
    {
        var cut = RenderBoard();

        cut.Find("#cell-2-3").PointerDown(Mouse());
        Assert.Contains("pressed", cut.Find("#cell-2-3").ClassList);

        cut.Find("[role=grid]").PointerUp(Mouse());
        Assert.DoesNotContain("pressed", cut.Find("#cell-2-3").ClassList);
    }

    [Fact]
    public void PressingOpenedNumberShowsItsClosedNeighborsPressed()
    {
        var cut = RenderBoard();

        cut.Find("#cell-1-1").PointerDown(Mouse());

        Assert.Equal(["cell-0-2", "cell-1-2", "cell-2-2"], cut.FindAll(".pressed").Select(cell => cell.Id));
    }

    [Fact]
    public void PressedDisplayEndsWhenLongPressIsRecognized()
    {
        var cut = RenderBoard();
        cut.Find("#cell-2-3").PointerDown(Touch());

        Time.Advance(PressGesture.LongPressDelay);

        cut.WaitForAssertion(() => Assert.Empty(cut.FindAll(".pressed")));
    }

    [Fact]
    public void PressingChangesAreReported()
    {
        var cut = RenderBoard();

        Click(cut, "#cell-2-3", Mouse());

        Assert.Equal([true, false], pressingChanges);
    }

    [Fact]
    public void RingIsShownAroundThePressedCellWhileWaitingForLongPress()
    {
        var cut = RenderBoard();
        var cellSize = cut.Instance.Placement.CellSize;

        cut.Find("#cell-2-3").PointerDown(Touch(x: 100, y: 100, offsetX: 10, offsetY: 20));

        var style = cut.Find(".long-press-ring").GetAttribute("style");
        Assert.Contains($"left: {90 + cellSize / 2.0}px", style);
        Assert.Contains($"top: {80 + cellSize / 2.0}px", style);
    }

    [Fact]
    public void RingDisappearsWhenLongPressIsRecognized()
    {
        var cut = RenderBoard();
        cut.Find("#cell-2-3").PointerDown(Touch());

        Time.Advance(PressGesture.LongPressDelay);

        cut.WaitForAssertion(() => Assert.Empty(cut.FindAll(".long-press-ring")));
    }

    [Fact]
    public void RingIsNotShownWhenLongPressWouldDoNothing()
    {
        var cut = RenderBoard();

        cut.Find("#cell-1-1").PointerDown(Touch());

        Assert.Empty(cut.FindAll(".long-press-ring"));
    }

    [Fact]
    public void RingIsNotShownForMouse()
    {
        var cut = RenderBoard();

        cut.Find("#cell-2-3").PointerDown(Mouse());

        Assert.Empty(cut.FindAll(".long-press-ring"));
    }

    [Fact]
    public void PressesAreIgnoredAfterTheGameIsOver()
    {
        game.Open(new CellPosition(2, 2));   // 地雷
        var cut = RenderBoard();

        Click(cut, "#cell-2-3", Mouse());
        cut.Find("#cell-2-3").PointerDown(Mouse(button: 2));

        Assert.Empty(opened);
        Assert.Empty(flagged);
        Assert.Empty(cut.FindAll(".pressed"));
    }

    [Fact]
    public void NewGameDiscardsThePress()
    {
        var cut = RenderBoard();
        cut.Find("#cell-2-3").PointerDown(Mouse());

        cut.Render(parameters => parameters.Add(board => board.Game, new Game(game.Difficulty, Time)));
        cut.Find("[role=grid]").PointerUp(Mouse());

        Assert.Empty(opened);
        Assert.Empty(cut.FindAll(".pressed"));
    }

    [Fact]
    public void FlagModeIsShownByTheBoardFrame()
        => Assert.Contains("flag-mode", RenderBoard(isFlagMode: true).Find("[role=grid]").ClassList);

    [Fact]
    public void BrowserContextMenuIsSuppressed()
        => Assert.True(RenderBoard().Find("[role=grid]").HasAttribute("blazor:oncontextmenu:preventdefault"));

    IRenderedComponent<BoardView> RenderBoard(bool isFlagMode = false)
        => Render<BoardView>(parameters => parameters
               .Add(board => board.Game, game)
               .Add(board => board.Placement, BoardPlacement.Calculate(400, 400, game.Difficulty))
               .Add(board => board.IsFlagMode, isFlagMode)
               .Add(board => board.OnOpen, (CellPosition position) => opened.Add(position))
               .Add(board => board.OnToggleFlag, (CellPosition position) => flagged.Add(position))
               .Add(board => board.OnPressingChanged, (bool isPressing) => pressingChanges.Add(isPressing)));
}
