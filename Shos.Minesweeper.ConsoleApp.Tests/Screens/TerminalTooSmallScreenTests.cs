using Shos.Minesweeper.ConsoleApp.Screens;
using Shos.Minesweeper.ConsoleApp.Terminal;

namespace Shos.Minesweeper.ConsoleApp.Tests.Screens;

/// <summary>端末が小さいときの画面（UI デザイン 3.8、クラス設計書 5.8）。狭い端末でも読めるように、短い行に分けて出す。</summary>
public class TerminalTooSmallScreenTests
{
    [Fact]
    public void ScreenTellsTheRequiredAndActualSizesInShortLines()
        => Assert.Equal([
            "端末の画面を広げてください。",
            "このゲームには",
            "76 列 x 30 行 が要ります。",
            "（今は 80 列 x 24 行）",
            "",
            "D 難易度  Q 終了",
        ], TerminalTooSmallScreen.Render(required: new TerminalSize(76, 30), actual: new TerminalSize(80, 24)).Lines.Select(line => line.Text));

    [Fact]
    public void ScreenHidesTheTextCursor()
        => Assert.Null(TerminalTooSmallScreen.Render(new TerminalSize(76, 30), new TerminalSize(80, 24)).CursorRow);
}
