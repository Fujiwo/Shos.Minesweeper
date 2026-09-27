using Avalonia.Input;
using Shos.Minesweeper.Presentation;

namespace Shos.Minesweeper.Desktop.Input;

/// <summary>
/// デスクトップ版のキーの割り当て（仕様書 4.3）。Web 版とコンソール版の KeyboardMapping と同じ形で、キーの型だけが違う
/// （アーキテクチャー設計書 6.2）。修飾キー（Ctrl など）は見ない（Web 版と同じ）。
/// </summary>
public static class KeyboardMapping
{
    public static CellAction ActionFor(Key key)
        => key switch {
            Key.Space or Key.Enter => CellAction.Open,
            Key.F                  => CellAction.ToggleFlag,
            _                      => CellAction.None
        };

    public static Direction? DirectionFor(Key key)
        => key switch {
            Key.Up    => Direction.Up,
            Key.Down  => Direction.Down,
            Key.Left  => Direction.Left,
            Key.Right => Direction.Right,
            _         => null
        };

    /// <summary>F2。Windows のマインスイーパーの定番の新しいゲームのキー（仕様書 4.3）。</summary>
    public static bool IsNewGameKey(Key key) => key == Key.F2;
}
