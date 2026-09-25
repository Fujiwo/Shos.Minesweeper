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
