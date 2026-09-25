using Bunit;
using Shos.Minesweeper.Components;
using Shos.Minesweeper.GameLogic;

namespace Shos.Minesweeper.Tests.Components;

public class BoardAreaTests : ComponentTestBase
{
    [Fact]
    public void ContentIsNotRenderedUntilTheAreaSizeIsKnown()
    {
        var cut = RenderArea(Difficulty.Beginner);

        Assert.Empty(cut.FindAll("p"));
    }

    [Fact]
    public async Task ContentReceivesThePlacementForTheAreaSize()
    {
        var cut = RenderArea(Difficulty.Beginner);

        await NotifyBoardAreaResizedAsync(352, 576);

        cut.WaitForAssertion(() => Assert.Equal("38", cut.Find("p").TextContent));
    }

    [Fact]
    public async Task PlacementIsRecalculatedWhenTheDifficultyChanges()
    {
        var cut = RenderArea(Difficulty.Beginner);
        await NotifyBoardAreaResizedAsync(352, 576);

        cut.Render(parameters => parameters.Add(area => area.Difficulty, Difficulty.Expert));

        Assert.Equal("20", cut.Find("p").TextContent);
    }

    [Fact]
    public async Task SizeObservationStopsWhenTheAreaIsDisposed()
    {
        RenderArea(Difficulty.Beginner);

        await DisposeComponentsAsync();

        Assert.Contains(JSInterop.Invocations, invocation => invocation.Identifier == "disconnect");
    }

    IRenderedComponent<BoardArea> RenderArea(Difficulty difficulty)
        => Render<BoardArea>(parameters => parameters
               .Add(area => area.Difficulty, difficulty)
               .Add(area => area.ChildContent, placement => $"<p>{placement.CellSize}</p>"));
}
