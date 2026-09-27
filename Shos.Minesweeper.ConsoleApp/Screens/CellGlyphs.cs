using Shos.Minesweeper.ConsoleApp.Rendering;
using Shos.Minesweeper.GameLogic;

namespace Shos.Minesweeper.ConsoleApp.Screens;

/// <summary>
/// マスの見せ方から、記号（ASCII の 1 文字）と色を決める（UI デザイン 3.4 の表）。
/// 幅があいまいな文字（■、● など）を使わないのは、端末によって盤面の列がずれるためである（仕様書 5.4）。
/// </summary>
public static class CellGlyphs
{
    // 数字の色。7 は、背景が黒い端末で見えない黒の代わりに既定の文字色（null）にする（UI デザイン 3.4）
    static readonly ConsoleColor?[] NumberColors = [
        null, ConsoleColor.Blue, ConsoleColor.Green, ConsoleColor.Red, ConsoleColor.Magenta,
        ConsoleColor.DarkYellow, ConsoleColor.Cyan, null, ConsoleColor.DarkGray
    ];

    public static StyledText Of(CellAppearance appearance, int adjacentMineCount)
        => appearance switch {
            CellAppearance.Closed       => new("#", new TextStyle(Foreground: ConsoleColor.DarkGray)),
            CellAppearance.Flagged      => new("F", new TextStyle(Foreground: ConsoleColor.Red)),
            CellAppearance.Opened       => adjacentMineCount == 0
                                           ? new(" ")
                                           : new(adjacentMineCount.ToString(), new TextStyle(Foreground: NumberColors[adjacentMineCount])),
            CellAppearance.Mine         => new("*"),
            CellAppearance.ExplodedMine => new("@", new TextStyle(ConsoleColor.White, ConsoleColor.DarkRed)),
            CellAppearance.WrongFlag    => new("X", new TextStyle(Foreground: ConsoleColor.Yellow)),
            _                           => throw new ArgumentOutOfRangeException(nameof(appearance), appearance, null)
        };
}
