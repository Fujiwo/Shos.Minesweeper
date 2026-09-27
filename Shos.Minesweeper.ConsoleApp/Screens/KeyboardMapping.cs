using Shos.Minesweeper.Presentation;

namespace Shos.Minesweeper.ConsoleApp.Screens;

/// <summary>
/// コンソール版のキーの割り当て（仕様書 5.3）。Web 版とデスクトップ版の KeyboardMapping と同じ形で、キーの型だけが違う
/// （アーキテクチャー設計書 6.2）。英字は ConsoleKey で見るので、大文字と小文字を区別しない。
/// </summary>
public static class KeyboardMapping
{
    /// <summary>矢印キーと、端末のプログラムの慣習の H・J・K・L（H 左、J 下、K 上、L 右）。</summary>
    public static Direction? DirectionFor(ConsoleKeyInfo key)
        => key.Key switch {
            ConsoleKey.UpArrow    or ConsoleKey.K => Direction.Up,
            ConsoleKey.DownArrow  or ConsoleKey.J => Direction.Down,
            ConsoleKey.LeftArrow  or ConsoleKey.H => Direction.Left,
            ConsoleKey.RightArrow or ConsoleKey.L => Direction.Right,
            _                                     => null
        };

    public static CellAction ActionFor(ConsoleKeyInfo key)
        => key.Key switch {
            ConsoleKey.Spacebar or ConsoleKey.Enter => CellAction.Open,
            ConsoleKey.F                            => CellAction.ToggleFlag,
            _                                       => CellAction.None
        };

    public static GameCommand CommandFor(ConsoleKeyInfo key)
        => key.Key switch {
            ConsoleKey.N => GameCommand.NewGame,
            ConsoleKey.D => GameCommand.SelectDifficulty,
            ConsoleKey.Q => GameCommand.Quit,
            _            => IsQuestionMark(key) ? GameCommand.ShowHelp : GameCommand.None
        };

    // 「?」は、キーボードの配列によってキー（ConsoleKey）が違うので、文字で見る。
    // IME がオンのままの全角の「？」も受けるように、文字を整えてから比べる（仕様書 3 章、5.3）
    static bool IsQuestionMark(ConsoleKeyInfo key) => InputText.Normalize(key.KeyChar.ToString()) == "?";

    /// <summary>Ctrl+C。Windows と Linux で届き方が違いうるので、キーと修飾の組と、制御文字の両方を見る。</summary>
    public static bool IsInterrupt(ConsoleKeyInfo key)
        => (key.Key == ConsoleKey.C && key.Modifiers.HasFlag(ConsoleModifiers.Control)) || key.KeyChar == '\u0003';
}
