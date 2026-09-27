using Shos.Minesweeper.ConsoleApp.Rendering;
using Shos.Minesweeper.ConsoleApp.Terminal;

namespace Shos.Minesweeper.ConsoleApp.Screens;

/// <summary>
/// 画面の移り変わり（クラス設計書 5.4）。今の画面を持ち、キーを渡して次の画面に移る。
/// どの画面でも効く Ctrl+C と、端末が今の画面に対して小さいときの扱い（仕様書 5.5）も受け持つ。
/// </summary>
public sealed class ScreenNavigator(GameScreen game)
{
    IScreen current = game;

    public bool IsExitRequested { get; private set; }

    public void HandleKey(ConsoleKeyInfo key, TerminalSize size)
    {
        if (KeyboardMapping.IsInterrupt(key)) {
            IsExitRequested = true;
            return;
        }
        // 端末が小さい間は、盤面の操作を受けない。受けるのは終わる操作だけ（難易度の選択の D は、区切り 4 で加える）
        if (!size.IsAtLeast(RequiredSizeOf(current.Render()))) {
            IsExitRequested = KeyboardMapping.CommandFor(key) == GameCommand.Quit;
            return;
        }
        if (current.HandleKey(key) is { } next)
            current = next;
        else
            IsExitRequested = true;
    }

    /// <summary>今の画面。端末が小さければ、端末を広げるよう知らせる画面。</summary>
    public Frame Render(TerminalSize size)
    {
        var frame = current.Render();
        var required = RequiredSizeOf(frame);
        return size.IsAtLeast(required) ? frame : TerminalTooSmallScreen.Render(required, size);
    }

    // 画面の行数と、キーの案内の行の幅（クラス設計書 9.1 の決定 7）。ゲームの画面では、盤面の行数 + 6 行 × 76 列になる
    static TerminalSize RequiredSizeOf(Frame frame) => new(GameScreen.KeyGuideColumns, frame.Lines.Count);
}
