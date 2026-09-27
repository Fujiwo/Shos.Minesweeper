using System.Text;
using static Shos.Minesweeper.ConsoleApp.Rendering.VirtualTerminalSequences;

namespace Shos.Minesweeper.ConsoleApp.Rendering;

/// <summary>
/// 前回と違う行だけを端末に書く（アーキテクチャー設計書 8.1、クラス設計書 5.2）。ちらつきを防ぎ、書く量を 1 秒に 1 行ほどにする。
/// 行を書いた後に行の残りを消すので、文字の表示の幅（日本語は 2 升）を数えずに済む。
/// 色を付けない（NO_COLOR）ときは、色と反転のシーケンスを書かない。画面の単位は、いつも色を付けて Frame を作る（クラス設計書 9.2 の A3）。
/// </summary>
public sealed class FrameWriter(TextWriter output, bool usesColor)
{
    IReadOnlyList<string> writtenLines = [];
    bool isInvalidated;

    /// <summary>前回の Frame と違う行だけを書く。何も変わっていなければ何も書かない。</summary>
    public void Write(Frame frame)
    {
        var lines = frame.Lines.Select(Encode).ToArray();
        var text = new StringBuilder();
        if (isInvalidated)
            text.Append(ClearScreen);
        for (var row = 0; row < Math.Max(lines.Length, writtenLines.Count); row++) {
            var line = row < lines.Length ? lines[row] : "";
            if (row >= writtenLines.Count || line != writtenLines[row])
                AppendLine(text, row, line);
        }
        if (text.Length > 0)
            AppendCursor(text, frame.CursorRow, lines);
        writtenLines = lines;
        isInvalidated = false;
        output.Write(text.ToString());
        output.Flush();
    }

    /// <summary>次の Write で、画面を消してすべての行を書く。端末の大きさが変わって、端末が行を詰め直したときに使う。</summary>
    public void Invalidate()
    {
        writtenLines = [];
        isInvalidated = true;
    }

    static void AppendLine(StringBuilder text, int row, string line)
        => text.Append(MoveCursorTo(row, column: 0)).Append(line).Append(ClearToEndOfLine);

    // 行の残りを消すシーケンスはカーソルを動かさない。カーソルの行を最後にもう一度書くと、カーソルは文字の直後に残る
    static void AppendCursor(StringBuilder text, int? cursorRow, string[] lines)
    {
        if (cursorRow is not { } row) {
            text.Append(HideCursor);
            return;
        }
        AppendLine(text, row, lines[row]);
        text.Append(ShowCursor);
    }

    string Encode(FrameLine line) => string.Concat(line.Parts.Select(Encode));

    string Encode(StyledText part)
    {
        var style = usesColor ? StyleOf(part.Style) : "";
        return style.Length == 0 ? part.Text : style + part.Text + ResetStyle;
    }
}
