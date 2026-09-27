using Shos.Minesweeper.ConsoleApp.Rendering;
using static Shos.Minesweeper.ConsoleApp.Rendering.VirtualTerminalSequences;

namespace Shos.Minesweeper.ConsoleApp.Tests.Rendering;

/// <summary>前回と違う行だけを端末に書く（アーキテクチャー設計書 8.1、クラス設計書 5.2）。</summary>
public class FrameWriterTests
{
    readonly StringWriter output = new();

    [Fact]
    public void FirstWriteWritesEveryLineAndHidesTheCursor()
    {
        var writer = new FrameWriter(output, usesColor: true);

        writer.Write(FrameOf("ab", "cd"));

        Assert.Equal(MoveCursorTo(0, 0) + "ab" + ClearToEndOfLine + MoveCursorTo(1, 0) + "cd" + ClearToEndOfLine + HideCursor,
                     output.ToString());
    }

    [Fact]
    public void SameFrameWritesNothing()
    {
        var writer = new FrameWriter(output, usesColor: true);
        writer.Write(FrameOf("ab", "cd"));
        output.GetStringBuilder().Clear();

        writer.Write(FrameOf("ab", "cd"));

        Assert.Equal("", output.ToString());
    }

    [Fact]
    public void OnlyChangedLinesAreWritten()
    {
        var writer = new FrameWriter(output, usesColor: true);
        writer.Write(FrameOf("ab", "cd", "ef"));
        output.GetStringBuilder().Clear();

        writer.Write(FrameOf("ab", "CD", "ef"));

        Assert.Equal(MoveCursorTo(1, 0) + "CD" + ClearToEndOfLine + HideCursor, output.ToString());
    }

    [Fact]
    public void LinesNoLongerInTheFrameAreCleared()
    {
        var writer = new FrameWriter(output, usesColor: true);
        writer.Write(FrameOf("ab", "cd", "ef"));
        output.GetStringBuilder().Clear();

        writer.Write(FrameOf("ab"));

        Assert.Equal(MoveCursorTo(1, 0) + ClearToEndOfLine + MoveCursorTo(2, 0) + ClearToEndOfLine + HideCursor, output.ToString());
    }

    [Fact]
    public void StyledTextIsWrappedInTheStyleAndReset()
    {
        var writer = new FrameWriter(output, usesColor: true);
        var red = new TextStyle(Foreground: ConsoleColor.Red);

        writer.Write(new Frame([new FrameLine(new StyledText("| "), new StyledText("F", red), new StyledText(" |"))]));

        Assert.Equal(MoveCursorTo(0, 0) + "| " + StyleOf(red) + "F" + ResetStyle + " |" + ClearToEndOfLine + HideCursor,
                     output.ToString());
    }

    // 環境変数 NO_COLOR のとき（仕様書 5.6）。色も反転も書かない
    [Fact]
    public void NoColorWritesNoStyles()
    {
        var writer = new FrameWriter(output, usesColor: false);
        var cursor = new TextStyle(Foreground: ConsoleColor.DarkGray, IsReversed: true);

        writer.Write(new Frame([new FrameLine(new StyledText("["), new StyledText("#", cursor), new StyledText("]"))]));

        Assert.Equal(MoveCursorTo(0, 0) + "[#]" + ClearToEndOfLine + HideCursor, output.ToString());
    }

    // 行の残りを消すシーケンスはカーソルを動かさないので、カーソルの行を最後に書くと、カーソルが入力の直後に残る
    [Fact]
    public void CursorLineIsWrittenLastAndTheCursorIsShown()
    {
        var writer = new FrameWriter(output, usesColor: true);

        writer.Write(new Frame([FrameLine.Of("幅: 2"), FrameLine.Of("誤り")], cursorRow: 0));

        Assert.Equal(MoveCursorTo(0, 0) + "幅: 2" + ClearToEndOfLine + MoveCursorTo(1, 0) + "誤り" + ClearToEndOfLine
                     + MoveCursorTo(0, 0) + "幅: 2" + ClearToEndOfLine + ShowCursor,
                     output.ToString());
    }

    [Fact]
    public void InvalidatedWriterClearsTheScreenAndWritesEveryLine()
    {
        var writer = new FrameWriter(output, usesColor: true);
        writer.Write(FrameOf("ab", "cd"));
        output.GetStringBuilder().Clear();

        writer.Invalidate();
        writer.Write(FrameOf("ab", "cd"));

        Assert.Equal(ClearScreen + MoveCursorTo(0, 0) + "ab" + ClearToEndOfLine + MoveCursorTo(1, 0) + "cd" + ClearToEndOfLine + HideCursor,
                     output.ToString());
    }

    static Frame FrameOf(params string[] lines) => new([.. lines.Select(line => FrameLine.Of(line))]);
}
