using Bunit;
using Microsoft.AspNetCore.Components.Web;
using Shos.Minesweeper.Pages;

namespace Shos.Minesweeper.Tests.Components;

public class GamePageTests : ComponentTestBase
{
    [Fact]
    public async Task PageShowsABeginnerBoardFittingTheArea()
    {
        var cut = Render<GamePage>();

        await NotifyBoardAreaResizedAsync(352, 576);

        cut.WaitForAssertion(() => Assert.Equal("盤面、9 行 9 列", cut.Find("[role=grid]").GetAttribute("aria-label")));
        Assert.Equal(81, cut.FindAll("[role=gridcell]").Count);
    }

    [Fact]
    public async Task ClickingACellOpensIt()
    {
        var cut = await RenderPageWithBoardAsync();

        cut.Find("#cell-4-4").PointerDown(Mouse(button: 0));
        cut.Find("[role=grid]").PointerUp(Mouse(button: 0));

        // 最初に開いたマスは必ず 0 なので、乱数の盤面でも開いた姿が決まる
        Assert.Equal("cell opened", cut.Find("#cell-4-4").ClassName);
    }

    [Fact]
    public async Task RightClickingACellFlagsIt()
    {
        var cut = await RenderPageWithBoardAsync();

        cut.Find("#cell-4-4").PointerDown(Mouse(button: 2));

        Assert.Equal("cell flagged", cut.Find("#cell-4-4").ClassName);
    }

    [Fact]
    public async Task ResetStartsANewGameOfTheSameDifficulty()
    {
        var cut = await RenderPageWithBoardAsync();
        cut.Find("#cell-4-4").PointerDown(Mouse(button: 2));

        cut.Find("button.reset").Click();

        Assert.Equal("cell closed", cut.Find("#cell-4-4").ClassName);
        Assert.Equal(81, cut.FindAll("[role=gridcell]").Count);
    }

    [Fact]
    public async Task FlagModeMakesTapsFlagAndSurvivesReset()
    {
        var cut = await RenderPageWithBoardAsync();

        cut.Find("button.flag-mode").Click();
        cut.Find("button.reset").Click();
        cut.Find("#cell-4-4").PointerDown(Mouse(button: 0));
        cut.Find("[role=grid]").PointerUp(Mouse(button: 0));

        Assert.Equal("true", cut.Find("button.flag-mode").GetAttribute("aria-pressed"));
        Assert.Contains("flag-mode", cut.Find("[role=grid]").ClassList);
        Assert.Equal("cell flagged", cut.Find("#cell-4-4").ClassName);
    }

    [Fact]
    public async Task FaceIsSurprisedWhileACellIsPressed()
    {
        var cut = await RenderPageWithBoardAsync();

        cut.Find("#cell-4-4").PointerDown(Mouse(button: 0));
        Assert.Equal("FaceSurprised", cut.Find("button.reset svg").GetAttribute("data-kind"));

        cut.Find("[role=grid]").PointerUp(Mouse(button: 0));
        Assert.Equal("FaceNormal", cut.Find("button.reset svg").GetAttribute("data-kind"));
    }

    async Task<IRenderedComponent<GamePage>> RenderPageWithBoardAsync()
    {
        var cut = Render<GamePage>();
        await NotifyBoardAreaResizedAsync(352, 576);
        cut.WaitForElement("[role=grid]");
        return cut;
    }

    static PointerEventArgs Mouse(long button)
        => new() { PointerId = 1, PointerType = "mouse", Button = button, ClientX = 100, ClientY = 100 };
}
