using Avalonia.Input;
using Shos.Minesweeper.Desktop.Input;
using Shos.Minesweeper.Presentation;

namespace Shos.Minesweeper.Desktop.Tests.Input;

/// <summary>デスクトップ版のキーの割り当て（仕様書 4.3）。</summary>
public class KeyboardMappingTests
{
    [Theory]
    [InlineData(Key.Space, CellAction.Open)]
    [InlineData(Key.Enter, CellAction.Open)]
    [InlineData(Key.F,     CellAction.ToggleFlag)]
    [InlineData(Key.A,     CellAction.None)]
    public void SpaceEnterAndFActOnTheCell(Key key, CellAction action)
        => Assert.Equal(action, KeyboardMapping.ActionFor(key));

    [Theory]
    [InlineData(Key.Up,    Direction.Up)]
    [InlineData(Key.Down,  Direction.Down)]
    [InlineData(Key.Left,  Direction.Left)]
    [InlineData(Key.Right, Direction.Right)]
    public void ArrowKeysMoveTheCursor(Key key, Direction direction)
        => Assert.Equal(direction, KeyboardMapping.DirectionFor(key));

    // デスクトップ版は H・J・K・L を使わない（仕様書 4.3。Web 版と同じ）
    [Fact]
    public void LettersDoNotMoveTheCursor()
        => Assert.Null(KeyboardMapping.DirectionFor(Key.H));

    [Fact]
    public void F2IsTheNewGameKey()
        => Assert.True(KeyboardMapping.IsNewGameKey(Key.F2));

    [Fact]
    public void OtherFunctionKeysAreNotTheNewGameKey()
        => Assert.False(KeyboardMapping.IsNewGameKey(Key.F3));
}
