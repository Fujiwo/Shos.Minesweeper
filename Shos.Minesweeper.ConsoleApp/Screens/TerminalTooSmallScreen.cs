using Shos.Minesweeper.ConsoleApp.Rendering;
using Shos.Minesweeper.ConsoleApp.Terminal;

namespace Shos.Minesweeper.ConsoleApp.Screens;

/// <summary>
/// 端末が小さいときの画面（仕様書 5.5、UI デザイン 3.8）。盤面を描かず、要る大きさを知らせて待つ。
/// 狭い端末でも読めるように、文を決まった位置で 30 升ほどの短い行に分ける（文字の幅を数えて折り返さない。クラス設計書 9.1 の決定 5）。
/// </summary>
public static class TerminalTooSmallScreen
{
    public static Frame Render(TerminalSize required, TerminalSize actual)
        => new([
            FrameLine.Of("端末の画面を広げてください。"),
            FrameLine.Of("このゲームには"),
            FrameLine.Of($"{required.Columns} 列 x {required.Rows} 行 が要ります。"),
            FrameLine.Of($"（今は {actual.Columns} 列 x {actual.Rows} 行）"),
            FrameLine.Of(""),
            FrameLine.Of("D 難易度  Q 終了"),
        ]);
}
