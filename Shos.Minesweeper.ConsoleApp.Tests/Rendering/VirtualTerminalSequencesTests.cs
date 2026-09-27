using Shos.Minesweeper.ConsoleApp.Rendering;

namespace Shos.Minesweeper.ConsoleApp.Tests.Rendering;

/// <summary>VT のシーケンス。行と列は 0 から数えて受け取り、端末には 1 から数えて渡す。</summary>
public class VirtualTerminalSequencesTests
{
    [Fact]
    public void TopLeftIsRowOneColumnOne()
        => Assert.Equal("\e[1;1H", VirtualTerminalSequences.MoveCursorTo(row: 0, column: 0));

    [Fact]
    public void RowAndColumnAreCountedFromOne()
        => Assert.Equal("\e[3;6H", VirtualTerminalSequences.MoveCursorTo(row: 2, column: 5));
}
