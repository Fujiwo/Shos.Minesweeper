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
    public const string ClearToEndOfLine = "\e[K";
    public const string ResetStyle = "\e[0m";

    /// <summary>カーソルを移す。行と列は 0 から数える（端末には 1 から数えて渡す）。</summary>
    public static string MoveCursorTo(int row, int column) => $"\e[{row + 1};{column + 1}H";
}
