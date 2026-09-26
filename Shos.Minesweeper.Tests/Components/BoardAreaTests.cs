using Bunit;
using Shos.Minesweeper.Components;
using Shos.Minesweeper.Display;

namespace Shos.Minesweeper.Tests.Components;

public class BoardAreaTests : AppTestContext
{
    readonly List<BoardAreaSize> reportedSizes = [];

    [Fact]
    public void ContentIsRenderedInTheArea()
    {
        var cut = RenderArea();

        Assert.Equal("盤面", cut.Find(".board-area > p").TextContent);
    }

    [Fact]
    public async Task ResizingReportsTheAreaSize()
    {
        RenderArea();

        await NotifyBoardAreaResizedAsync(352, 576);
        await NotifyBoardAreaResizedAsync(640, 312);

        Assert.Equal([new BoardAreaSize(352, 576), new BoardAreaSize(640, 312)], reportedSizes);
    }

    [Fact]
    public async Task SizeObservationStopsWhenTheAreaIsDisposed()
    {
        RenderArea();

        await DisposeComponentsAsync();

        Assert.Contains(JSInterop.Invocations, invocation => invocation.Identifier == "disconnect");
    }

    IRenderedComponent<BoardArea> RenderArea()
        => Render<BoardArea>(parameters => parameters
               .Add(area => area.OnResized, reportedSizes.Add)
               .AddChildContent("<p>盤面</p>"));
}
