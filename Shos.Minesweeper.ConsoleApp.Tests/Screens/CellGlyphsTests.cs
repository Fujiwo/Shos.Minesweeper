using Shos.Minesweeper.ConsoleApp.Rendering;
using Shos.Minesweeper.ConsoleApp.Screens;
using Shos.Minesweeper.GameLogic;

namespace Shos.Minesweeper.ConsoleApp.Tests.Screens;

/// <summary>マスの記号と色（UI デザイン 3.4 の表）。記号はどれも ASCII の 1 文字で、色がなくても区別できる。</summary>
public class CellGlyphsTests
{
    [Theory]
    [InlineData(CellAppearance.Closed,  0, "#", ConsoleColor.DarkGray)]
    [InlineData(CellAppearance.Flagged, 0, "F", ConsoleColor.Red)]
    [InlineData(CellAppearance.WrongFlag, 0, "X", ConsoleColor.Yellow)]
    public void GlyphAndColorFollowTheAppearance(CellAppearance appearance, int adjacentMineCount, string glyph, ConsoleColor color)
        => Assert.Equal(new StyledText(glyph, new TextStyle(Foreground: color)), CellGlyphs.Of(appearance, adjacentMineCount));

    [Fact]
    public void OpenedZeroIsABlank()
        => Assert.Equal(new StyledText(" "), CellGlyphs.Of(CellAppearance.Opened, 0));

    [Fact]
    public void MineUsesTheDefaultColor()
        => Assert.Equal(new StyledText("*"), CellGlyphs.Of(CellAppearance.Mine, 0));

    [Fact]
    public void ExplodedMineIsWhiteOnDarkRed()
        => Assert.Equal(new StyledText("@", new TextStyle(ConsoleColor.White, ConsoleColor.DarkRed)), CellGlyphs.Of(CellAppearance.ExplodedMine, 0));

    [Theory]
    [InlineData(1, ConsoleColor.Blue)]
    [InlineData(2, ConsoleColor.Green)]
    [InlineData(3, ConsoleColor.Red)]
    [InlineData(4, ConsoleColor.Magenta)]
    [InlineData(5, ConsoleColor.DarkYellow)]
    [InlineData(6, ConsoleColor.Cyan)]
    [InlineData(8, ConsoleColor.DarkGray)]
    public void NumbersHaveTheirColors(int number, ConsoleColor color)
        => Assert.Equal(new StyledText(number.ToString(), new TextStyle(Foreground: color)), CellGlyphs.Of(CellAppearance.Opened, number));

    // 7 の黒は、背景が黒い端末で見えないので、既定の文字色にする
    [Fact]
    public void SevenUsesTheDefaultColor()
        => Assert.Equal(new StyledText("7"), CellGlyphs.Of(CellAppearance.Opened, 7));
}
