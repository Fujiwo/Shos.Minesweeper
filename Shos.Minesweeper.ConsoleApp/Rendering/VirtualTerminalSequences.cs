namespace Shos.Minesweeper.ConsoleApp.Rendering;

/// <summary>
/// 端末を制御する VT のシーケンス（Microsoft の文書の「Console Virtual Terminal Sequences」）。
/// Windows Terminal、従来のコンソール ホスト（VT の解釈を有効にしたとき）、Linux の端末で同じものを使う。
/// </summary>
public static class VirtualTerminalSequences
{
    public const string EnterAlternateScreen = "\e[?1049h";
    public const string LeaveAlternateScreen = "\e[?1049l";
    public const string HideCursor = "\e[?25l";
    public const string ShowCursor = "\e[?25h";
    public const string ClearScreen = "\e[2J";
    public const string ClearToEndOfLine = "\e[K";
    public const string ResetStyle = "\e[0m";

    const int ReverseCode = 7;
    const int BackgroundOffset = 10;

    /// <summary>カーソルを移す。行と列は 0 から数える（端末には 1 から数えて渡す）。</summary>
    public static string MoveCursorTo(int row, int column) => $"\e[{row + 1};{column + 1}H";

    /// <summary>文字の色と反転のシーケンス（SGR）。既定の色で反転もしないときは空。</summary>
    public static string StyleOf(TextStyle style)
    {
        var codes = new List<int>();
        if (style.Foreground is { } foreground)
            codes.Add(ForegroundCodeOf(foreground));
        if (style.Background is { } background)
            codes.Add(ForegroundCodeOf(background) + BackgroundOffset);
        if (style.IsReversed)
            codes.Add(ReverseCode);
        return codes.Count == 0 ? "" : $"\e[{string.Join(';', codes)}m";
    }

    // ConsoleColor の並び（DarkBlue、DarkGreen…）は、SGR の色の並び（赤、緑、黄、青…）と違うので、表で対応させる
    static int ForegroundCodeOf(ConsoleColor color)
        => color switch {
            ConsoleColor.Black       => 30,
            ConsoleColor.DarkRed     => 31,
            ConsoleColor.DarkGreen   => 32,
            ConsoleColor.DarkYellow  => 33,
            ConsoleColor.DarkBlue    => 34,
            ConsoleColor.DarkMagenta => 35,
            ConsoleColor.DarkCyan    => 36,
            ConsoleColor.Gray        => 37,
            ConsoleColor.DarkGray    => 90,
            ConsoleColor.Red         => 91,
            ConsoleColor.Green       => 92,
            ConsoleColor.Yellow      => 93,
            ConsoleColor.Blue        => 94,
            ConsoleColor.Magenta     => 95,
            ConsoleColor.Cyan        => 96,
            ConsoleColor.White       => 97,
            _                        => throw new ArgumentOutOfRangeException(nameof(color), color, null)
        };
}
