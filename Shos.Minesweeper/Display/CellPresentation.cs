using Shos.Minesweeper.GameLogic;
using Shos.Minesweeper.Presentation;

namespace Shos.Minesweeper.Display;

/// <summary>
/// マスの見た目（CSS のクラス、アイコン、数字。UI デザイン 4.2）。読み上げの名前は、デスクトップ版と共有するので
/// Presentation の BoardNames にある（デスクトップ版・コンソール版のクラス設計書 3.4）。
/// </summary>
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

    /// <summary>演出の CSS のクラス。動きと時間は CSS が決める（UI デザイン 10.7）。</summary>
    public static string CssClassOf(CellAnimationKind kind)
        => kind switch {
            CellAnimationKind.Reveal          => "reveal",
            CellAnimationKind.Explode         => "explode",
            CellAnimationKind.MineAppear      => "mine-appear",
            CellAnimationKind.WrongFlagAppear => "wrong-flag-appear",
            CellAnimationKind.FlagBounce      => "flag-bounce",
            _                                 => throw new ArgumentOutOfRangeException(nameof(kind), kind, null)
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
}
