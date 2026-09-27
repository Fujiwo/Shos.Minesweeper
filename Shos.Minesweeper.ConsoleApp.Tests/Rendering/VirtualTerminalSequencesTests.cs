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

    // 16 色（ConsoleColor）を SGR の番号にする。暗い色は 30〜37、明るい色は 90〜97、背景は 10 を足す
    [Theory]
    [InlineData(ConsoleColor.Black,       "\e[30m")]
    [InlineData(ConsoleColor.DarkRed,     "\e[31m")]
    [InlineData(ConsoleColor.DarkGreen,   "\e[32m")]
    [InlineData(ConsoleColor.DarkYellow,  "\e[33m")]
    [InlineData(ConsoleColor.DarkBlue,    "\e[34m")]
    [InlineData(ConsoleColor.DarkMagenta, "\e[35m")]
    [InlineData(ConsoleColor.DarkCyan,    "\e[36m")]
    [InlineData(ConsoleColor.Gray,        "\e[37m")]
    [InlineData(ConsoleColor.DarkGray,    "\e[90m")]
    [InlineData(ConsoleColor.Red,         "\e[91m")]
    [InlineData(ConsoleColor.Green,       "\e[92m")]
    [InlineData(ConsoleColor.Yellow,      "\e[93m")]
    [InlineData(ConsoleColor.Blue,        "\e[94m")]
    [InlineData(ConsoleColor.Magenta,     "\e[95m")]
    [InlineData(ConsoleColor.Cyan,        "\e[96m")]
    [InlineData(ConsoleColor.White,       "\e[97m")]
    public void ForegroundColorIsTheSgrNumber(ConsoleColor color, string sequence)
        => Assert.Equal(sequence, VirtualTerminalSequences.StyleOf(new TextStyle(Foreground: color)));

    [Fact]
    public void ForegroundBackgroundAndReverseAreCombined()
        => Assert.Equal("\e[97;41;7m", VirtualTerminalSequences.StyleOf(new TextStyle(ConsoleColor.White, ConsoleColor.DarkRed, IsReversed: true)));

    [Fact]
    public void DefaultStyleHasNoSequence()
        => Assert.Equal("", VirtualTerminalSequences.StyleOf(default));
}
