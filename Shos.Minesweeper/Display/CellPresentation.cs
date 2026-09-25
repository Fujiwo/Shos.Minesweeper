using Shos.Minesweeper.GameLogic;

namespace Shos.Minesweeper.Display;

/// <summary>マスの見た目（CSS のクラス、アイコン）と読み上げの名前（UI デザイン 4.2、6.4）。</summary>
public static class CellPresentation
{
    public static string CssClassOf(CellAppearance appearance, int adjacentMineCount)
        => appearance switch {
            CellAppearance.Closed       => "closed",
            CellAppearance.Flagged      => "flagged",
            CellAppearance.Opened       => adjacentMineCount == 0 ? "opened" : $"opened n{adjacentMineCount}",
            CellAppearance.Mine         => "mine",
            CellAppearance.ExplodedMine => "exploded",
            CellAppearance.WrongFlag    => "wrong-flag",
            _                           => throw new ArgumentOutOfRangeException(nameof(appearance), appearance, null)
        };

    public static IconKind? IconOf(CellAppearance appearance)
        => appearance switch {
            CellAppearance.Flagged      => IconKind.Flag,
            CellAppearance.Mine         => IconKind.Mine,
            CellAppearance.ExplodedMine => IconKind.ExplodedMine,
            CellAppearance.WrongFlag    => IconKind.WrongFlag,
            _                           => null
        };

    /// <summary>マスに出す数字。開いた数字のマスだけに出し、それ以外は空にする。</summary>
    public static string NumberTextOf(CellAppearance appearance, int adjacentMineCount)
        => appearance == CellAppearance.Opened && adjacentMineCount > 0 ? adjacentMineCount.ToString() : "";

    /// <summary>「3 行 5 列、未開放」のような名前。行と列は表示している向きで、1 から数える。</summary>
    public static string AccessibleNameOf(DisplayPosition position, CellAppearance appearance, int adjacentMineCount)
        => $"{position.Row + 1} 行 {position.Column + 1} 列、{StateNameOf(appearance, adjacentMineCount)}";

    static string StateNameOf(CellAppearance appearance, int adjacentMineCount)
        => appearance switch {
            CellAppearance.Closed       => "未開放",
            CellAppearance.Flagged      => "旗",
            CellAppearance.Opened       => adjacentMineCount == 0 ? "空白" : adjacentMineCount.ToString(),
            CellAppearance.Mine         => "地雷",
            CellAppearance.ExplodedMine => "踏んだ地雷",
            CellAppearance.WrongFlag    => "誤った旗",
            _                           => throw new ArgumentOutOfRangeException(nameof(appearance), appearance, null)
        };
}
