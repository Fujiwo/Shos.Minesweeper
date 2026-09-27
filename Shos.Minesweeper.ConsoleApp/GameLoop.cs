using Shos.Minesweeper.ConsoleApp.Rendering;
using Shos.Minesweeper.ConsoleApp.Screens;
using Shos.Minesweeper.ConsoleApp.Terminal;

namespace Shos.Minesweeper.ConsoleApp;

/// <summary>
/// 進行のループ（アーキテクチャー設計書 8.2）。1 つのスレッドで、キーを読み、画面を書き、少し待つ、を繰り返す。
/// 端末と時間の待ちだけを扱い、判断は持たない（判断は ScreenNavigator 以下にあり、テストで確かめる）。
/// </summary>
public static class GameLoop
{
    // キーから画面の反映まで 100 ミリ秒以内（仕様書 6.1）に余裕をもって収め、経過時間の秒の変わり目も遅れずに描くため
    static readonly TimeSpan PollInterval = TimeSpan.FromMilliseconds(30);

    public static void Run(TerminalSession terminal, ScreenNavigator navigator, FrameWriter writer)
    {
        TerminalSize? writtenSize = null;
        while (!navigator.IsExitRequested) {
            var size = terminal.Size;
            // 端末は、大きさが変わると行を詰め直すことがあるので、画面全体を書き直す（クラス設計書 9.1 の決定 6）
            if (size != writtenSize) {
                writer.Invalidate();
                writtenSize = size;
            }
            HandleAvailableKeys(terminal, navigator, size);
            if (navigator.IsExitRequested)
                break;
            writer.Write(navigator.Render(size));
            Thread.Sleep(PollInterval);
        }
    }

    static void HandleAvailableKeys(TerminalSession terminal, ScreenNavigator navigator, TerminalSize size)
    {
        foreach (var key in terminal.ReadAvailableKeys()) {
            navigator.HandleKey(key, size);
            if (navigator.IsExitRequested)
                return;
        }
    }
}
