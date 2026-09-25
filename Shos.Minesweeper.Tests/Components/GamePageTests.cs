using Bunit;
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
}
