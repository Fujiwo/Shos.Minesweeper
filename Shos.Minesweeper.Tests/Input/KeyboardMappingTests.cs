using Shos.Minesweeper.Input;
using Shos.Minesweeper.Presentation;

namespace Shos.Minesweeper.Tests.Input;

/// <summary>キーボードの操作の割り当て（仕様書 4.5）。キーの名前は DOM の KeyboardEvent.key の値。</summary>
public class KeyboardMappingTests
{
    [Theory]
    [InlineData(" ",     CellAction.Open)]
    [InlineData("Enter", CellAction.Open)]
    [InlineData("f",     CellAction.ToggleFlag)]
    [InlineData("F",     CellAction.ToggleFlag)]
    [InlineData("a",     CellAction.None)]
    [InlineData("Tab",   CellAction.None)]
    public void KeysMapToActions(string key, CellAction action)
        => Assert.Equal(action, KeyboardMapping.ActionFor(key));

    [Theory]
    [InlineData("ArrowUp",    Direction.Up)]
    [InlineData("ArrowDown",  Direction.Down)]
    [InlineData("ArrowLeft",  Direction.Left)]
    [InlineData("ArrowRight", Direction.Right)]
    public void ArrowKeysMapToDirections(string key, Direction direction)
        => Assert.Equal(direction, KeyboardMapping.DirectionFor(key));

    [Theory]
    [InlineData("Enter")]
    [InlineData("f")]
    public void OtherKeysHaveNoDirection(string key)
        => Assert.Null(KeyboardMapping.DirectionFor(key));
}
