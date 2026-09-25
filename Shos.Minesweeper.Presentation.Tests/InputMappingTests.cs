using Shos.Minesweeper.GameLogic;
using Shos.Minesweeper.Input;

namespace Shos.Minesweeper.Tests.Input;

/// <summary>押し方と旗モードとマスから、行う操作を決める（仕様書 4.1 の表と 3.4 の表、UI デザイン 5.2）。</summary>
public class InputMappingTests
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
        => Assert.Equal(action, InputMapping.ActionFor(press, isFlagMode, CellOf(cellKind)));

    // キーボードの割り当て（仕様書 4.5）。キーの名前は DOM の KeyboardEvent.key の値
    [Theory]
    [InlineData(" ",     CellAction.Open)]
    [InlineData("Enter", CellAction.Open)]
    [InlineData("f",     CellAction.ToggleFlag)]
    [InlineData("F",     CellAction.ToggleFlag)]
    [InlineData("a",     CellAction.None)]
    [InlineData("Tab",   CellAction.None)]
    public void KeysMapToActions(string key, CellAction action)
        => Assert.Equal(action, InputMapping.ActionForKey(key));

    [Theory]
    [InlineData("ArrowUp",    Direction.Up)]
    [InlineData("ArrowDown",  Direction.Down)]
    [InlineData("ArrowLeft",  Direction.Left)]
    [InlineData("ArrowRight", Direction.Right)]
    public void ArrowKeysMapToDirections(string key, Direction direction)
        => Assert.Equal(direction, InputMapping.DirectionForKey(key));

    [Theory]
    [InlineData("Enter")]
    [InlineData("f")]
    public void OtherKeysHaveNoDirection(string key)
        => Assert.Null(InputMapping.DirectionForKey(key));

    static Cell CellOf(CellKind kind)
        => kind switch {
            CellKind.Closed       => new(CellState.Closed, 0),
            CellKind.Flagged      => new(CellState.Flagged, 0),
            CellKind.OpenedNumber => new(CellState.Opened, 2),
            CellKind.OpenedZero   => new(CellState.Opened, 0),
            _                     => throw new ArgumentOutOfRangeException(nameof(kind), kind, null)
        };
}
