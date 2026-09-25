using Shos.Minesweeper.GameLogic;

namespace Shos.Minesweeper.Input;

/// <summary>入力から行う操作を決める。マウスとタッチは押し方と旗モードとマスから（仕様書 4.1）、キーボードはキーから（仕様書 4.5）。</summary>
public static class InputMapping
{
    public static CellAction ActionFor(PressKind press, bool isFlagMode, Cell cell)
    {
        // 開いた数字のマスへのタップは、モードにかかわらずコードになる（仕様書 4.1）
        var action = press == PressKind.Tap && cell.IsOpenedNumber ? CellAction.Open : ActionByMode(press, isFlagMode);
        // そのマスに効かない操作（仕様書 3.4 の「何もしない」）は、何もしないことにする。長押しの円もこれで出さない
        return IsEffective(action, cell) ? action : CellAction.None;
    }

    /// <summary>キーボードの操作。キーの名前は DOM の KeyboardEvent.key の値。旗モードはキーボードの操作に影響しない（仕様書 4.5）。</summary>
    public static CellAction ActionForKey(string key)
        => key switch {
            " " or "Enter" => CellAction.Open,
            "f" or "F"     => CellAction.ToggleFlag,
            _              => CellAction.None
        };

    public static Direction? DirectionForKey(string key)
        => key switch {
            "ArrowUp"    => Direction.Up,
            "ArrowDown"  => Direction.Down,
            "ArrowLeft"  => Direction.Left,
            "ArrowRight" => Direction.Right,
            _            => null
        };

    static CellAction ActionByMode(PressKind press, bool isFlagMode)
        => (press, isFlagMode) switch {
            (PressKind.RightClick, _)    => CellAction.ToggleFlag,
            (PressKind.Tap, false)       => CellAction.Open,
            (PressKind.Tap, true)        => CellAction.ToggleFlag,
            (PressKind.LongPress, false) => CellAction.ToggleFlag,
            (PressKind.LongPress, true)  => CellAction.Open,
            _                            => throw new ArgumentOutOfRangeException(nameof(press), press, null)
        };

    static bool IsEffective(CellAction action, Cell cell)
        => action switch {
            CellAction.Open       => cell.CanOpen,
            CellAction.ToggleFlag => cell.CanToggleFlag,
            _                     => false
        };
}
