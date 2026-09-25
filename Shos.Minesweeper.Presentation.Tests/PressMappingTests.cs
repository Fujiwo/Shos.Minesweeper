using Shos.Minesweeper.GameLogic;

namespace Shos.Minesweeper.Presentation.Tests;

/// <summary>押し方と旗モードとマスから、行う操作を決める（仕様書 4.1 の表と 3.4 の表、UI デザイン 5.2）。</summary>
public class PressMappingTests
{
    public enum CellKind { Closed, Flagged, OpenedNumber, OpenedZero }

    public static TheoryData<PressKind, bool, CellKind, CellAction> AllCombinations => new() {
        // 通常のモード
        { PressKind.Tap,        false, CellKind.Closed,       CellAction.Open },
        { PressKind.Tap,        false, CellKind.Flagged,      CellAction.None },
        { PressKind.Tap,        false, CellKind.OpenedNumber, CellAction.Open },          // コード
        { PressKind.Tap,        false, CellKind.OpenedZero,   CellAction.None },
        { PressKind.RightClick, false, CellKind.Closed,       CellAction.ToggleFlag },
        { PressKind.RightClick, false, CellKind.Flagged,      CellAction.ToggleFlag },
        { PressKind.RightClick, false, CellKind.OpenedNumber, CellAction.None },
        { PressKind.RightClick, false, CellKind.OpenedZero,   CellAction.None },
        { PressKind.LongPress,  false, CellKind.Closed,       CellAction.ToggleFlag },
        { PressKind.LongPress,  false, CellKind.Flagged,      CellAction.ToggleFlag },
        { PressKind.LongPress,  false, CellKind.OpenedNumber, CellAction.None },          // 旗は開放済みに効かない
        { PressKind.LongPress,  false, CellKind.OpenedZero,   CellAction.None },
        // 旗モード
        { PressKind.Tap,        true,  CellKind.Closed,       CellAction.ToggleFlag },
        { PressKind.Tap,        true,  CellKind.Flagged,      CellAction.ToggleFlag },
        { PressKind.Tap,        true,  CellKind.OpenedNumber, CellAction.Open },          // 旗モードでもコード
        { PressKind.Tap,        true,  CellKind.OpenedZero,   CellAction.None },
        { PressKind.RightClick, true,  CellKind.Closed,       CellAction.ToggleFlag },
        { PressKind.RightClick, true,  CellKind.Flagged,      CellAction.ToggleFlag },
        { PressKind.RightClick, true,  CellKind.OpenedNumber, CellAction.None },
        { PressKind.RightClick, true,  CellKind.OpenedZero,   CellAction.None },
        { PressKind.LongPress,  true,  CellKind.Closed,       CellAction.Open },
        { PressKind.LongPress,  true,  CellKind.Flagged,      CellAction.None },          // 旗のマスは開けない
        { PressKind.LongPress,  true,  CellKind.OpenedNumber, CellAction.Open },          // コード
        { PressKind.LongPress,  true,  CellKind.OpenedZero,   CellAction.None },
    };

    [Theory]
    [MemberData(nameof(AllCombinations))]
    public void ActionFollowsThePressTheModeAndTheCell(PressKind press, bool isFlagMode, CellKind cellKind, CellAction action)
        => Assert.Equal(action, PressMapping.ActionFor(press, isFlagMode, CellOf(cellKind)));

    static Cell CellOf(CellKind kind)
        => kind switch {
            CellKind.Closed       => new(CellState.Closed, 0),
            CellKind.Flagged      => new(CellState.Flagged, 0),
            CellKind.OpenedNumber => new(CellState.Opened, 2),
            CellKind.OpenedZero   => new(CellState.Opened, 0),
            _                     => throw new ArgumentOutOfRangeException(nameof(kind), kind, null)
        };
}
