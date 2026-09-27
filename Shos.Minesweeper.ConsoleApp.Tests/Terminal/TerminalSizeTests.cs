using Shos.Minesweeper.ConsoleApp.Terminal;

namespace Shos.Minesweeper.ConsoleApp.Tests.Terminal;

/// <summary>端末の大きさが、画面に必要な大きさに足りるか（UI デザイン 3.2、3.8）。</summary>
public class TerminalSizeTests
{
    static readonly TerminalSize Required = new(Columns: 76, Rows: 15);

    [Fact]
    public void SameSizeIsEnough()
        => Assert.True(new TerminalSize(76, 15).IsAtLeast(Required));

    [Fact]
    public void LargerSizeIsEnough()
        => Assert.True(new TerminalSize(120, 30).IsAtLeast(Required));

    [Fact]
    public void OneColumnShortIsNotEnough()
        => Assert.False(new TerminalSize(75, 30).IsAtLeast(Required));

    [Fact]
    public void OneRowShortIsNotEnough()
        => Assert.False(new TerminalSize(120, 14).IsAtLeast(Required));
}
