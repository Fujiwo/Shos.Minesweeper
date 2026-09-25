using Shos.Minesweeper.GameLogic;

namespace Shos.Minesweeper.Presentation;

/// <summary>押し方（タップ・長押し・右クリック）と旗モードとマスから、行う操作を決める（仕様書 4.1）。どの UI でも同じ規則である。</summary>
public static class InputMapping
{
    public static CellAction ActionFor(PressKind press, bool isFlagMode, Cell cell)
    {
        // 開いた数字のマスへのタップは、モードにかかわらずコードになる（仕様書 4.1）
        var action = press == PressKind.Tap && cell.IsOpenedNumber ? CellAction.Open : ActionByMode(press, isFlagMode);
        // そのマスに効かない操作（仕様書 3.4 の「何もしない」）は、何もしないことにする。長押しの円もこれで出さない
        return IsEffective(action, cell) ? action : CellAction.None;
    }

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
