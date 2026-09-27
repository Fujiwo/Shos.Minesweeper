using Shos.Minesweeper.ConsoleApp.Screens;
using Shos.Minesweeper.Presentation;

namespace Shos.Minesweeper.ConsoleApp.Tests.Screens;

/// <summary>コンソール版のキーの割り当て（仕様書 5.3）。英字は大文字と小文字を区別しない。</summary>
public class KeyboardMappingTests
{
    [Theory]
    [InlineData(ConsoleKey.UpArrow,    Direction.Up)]
    [InlineData(ConsoleKey.DownArrow,  Direction.Down)]
    [InlineData(ConsoleKey.LeftArrow,  Direction.Left)]
    [InlineData(ConsoleKey.RightArrow, Direction.Right)]
    [InlineData(ConsoleKey.K,          Direction.Up)]
    [InlineData(ConsoleKey.J,          Direction.Down)]
    [InlineData(ConsoleKey.H,          Direction.Left)]
    [InlineData(ConsoleKey.L,          Direction.Right)]
    public void ArrowsAndHjklMoveTheCursor(ConsoleKey key, Direction direction)
        => Assert.Equal(direction, KeyboardMapping.DirectionFor(Keys.Of(key)));

    [Fact]
    public void UpperCaseLettersMoveTheCursorToo()
        => Assert.Equal(Direction.Left, KeyboardMapping.DirectionFor(Keys.Of(ConsoleKey.H, 'H', shift: true)));

    [Fact]
    public void OtherKeysDoNotMoveTheCursor()
        => Assert.Null(KeyboardMapping.DirectionFor(Keys.Of(ConsoleKey.A)));

    [Theory]
    [InlineData(ConsoleKey.Spacebar, CellAction.Open)]
    [InlineData(ConsoleKey.Enter,    CellAction.Open)]
    [InlineData(ConsoleKey.F,        CellAction.ToggleFlag)]
    [InlineData(ConsoleKey.A,        CellAction.None)]
    public void SpaceEnterAndFActOnTheCell(ConsoleKey key, CellAction action)
        => Assert.Equal(action, KeyboardMapping.ActionFor(Keys.Of(key)));

    [Theory]
    [InlineData(ConsoleKey.N, GameCommand.NewGame)]
    [InlineData(ConsoleKey.Q, GameCommand.Quit)]
    [InlineData(ConsoleKey.A, GameCommand.None)]
    public void LettersSelectScreenCommands(ConsoleKey key, GameCommand command)
        => Assert.Equal(command, KeyboardMapping.CommandFor(Keys.Of(key)));

    // Ctrl+C は、Windows では C と Ctrl の組で、端末によっては制御文字（U+0003）で届く
    [Fact]
    public void CtrlCIsAnInterrupt()
        => Assert.True(KeyboardMapping.IsInterrupt(Keys.Of(ConsoleKey.C, '\u0003', control: true)));

    [Fact]
    public void ControlCharacterThreeIsAnInterrupt()
        => Assert.True(KeyboardMapping.IsInterrupt(new ConsoleKeyInfo('\u0003', default, shift: false, alt: false, control: false)));

    [Fact]
    public void PlainCIsNotAnInterrupt()
        => Assert.False(KeyboardMapping.IsInterrupt(Keys.Of(ConsoleKey.C)));
}
