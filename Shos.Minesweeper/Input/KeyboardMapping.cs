using Shos.Minesweeper.Presentation;

namespace Shos.Minesweeper.Input;

/// <summary>
/// キーボードの操作の割り当て（仕様書 4.5）。キーの名前は DOM の KeyboardEvent.key の値なので、Web アプリにだけ置く。
/// 旗モードはキーボードの操作に影響しない。
/// </summary>
public static class KeyboardMapping
{
    public static CellAction ActionFor(string key)
        => key switch {
            " " or "Enter" => CellAction.Open,
            "f" or "F"     => CellAction.ToggleFlag,
            _              => CellAction.None
        };

    public static Direction? DirectionFor(string key)
        => key switch {
            "ArrowUp"    => Direction.Up,
            "ArrowDown"  => Direction.Down,
            "ArrowLeft"  => Direction.Left,
            "ArrowRight" => Direction.Right,
            _            => null
        };
}
