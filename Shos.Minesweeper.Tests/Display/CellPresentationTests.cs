using Shos.Minesweeper.Display;
using Shos.Minesweeper.GameLogic;

namespace Shos.Minesweeper.Tests.Display;

/// <summary>マスの見た目と読み上げの名前（クラス設計書 4.3 の表、UI デザイン 4.2、6.4）。</summary>
public class CellPresentationTests
{
    [Theory]
    [InlineData(CellAppearance.Closed,       0, "closed")]
    [InlineData(CellAppearance.Flagged,      0, "flagged")]
    [InlineData(CellAppearance.Opened,       0, "opened")]
    [InlineData(CellAppearance.Opened,       1, "opened n1")]
    [InlineData(CellAppearance.Opened,       8, "opened n8")]
    [InlineData(CellAppearance.Mine,         0, "mine")]
    [InlineData(CellAppearance.ExplodedMine, 0, "exploded")]
    [InlineData(CellAppearance.WrongFlag,    0, "wrong-flag")]
    public void CssClassFollowsTheAppearance(CellAppearance appearance, int adjacentMineCount, string cssClass)
        => Assert.Equal(cssClass, CellPresentation.CssClassOf(appearance, adjacentMineCount));

    [Theory]
    [InlineData(CellAppearance.Flagged,      IconKind.Flag)]
    [InlineData(CellAppearance.Mine,         IconKind.Mine)]
    [InlineData(CellAppearance.ExplodedMine, IconKind.ExplodedMine)]
    [InlineData(CellAppearance.WrongFlag,    IconKind.WrongFlag)]
    public void IconFollowsTheAppearance(CellAppearance appearance, IconKind icon)
        => Assert.Equal(icon, CellPresentation.IconOf(appearance));

    [Theory]
    [InlineData(CellAppearance.Closed)]
    [InlineData(CellAppearance.Opened)]
    public void ClosedAndOpenedCellsHaveNoIcon(CellAppearance appearance)
        => Assert.Null(CellPresentation.IconOf(appearance));

    [Theory]
    [InlineData(CellAppearance.Opened,       3, "3")]
    [InlineData(CellAppearance.Opened,       0, "")]
    [InlineData(CellAppearance.Closed,       0, "")]
    [InlineData(CellAppearance.ExplodedMine, 2, "")]    // 踏んだ地雷は開いたマスでも数字を出さない
    public void NumberIsShownOnlyOnOpenedNumberCells(CellAppearance appearance, int adjacentMineCount, string text)
        => Assert.Equal(text, CellPresentation.NumberTextOf(appearance, adjacentMineCount));

    [Theory]
    [InlineData(CellAppearance.Closed,       0, "3 行 5 列、未開放")]
    [InlineData(CellAppearance.Flagged,      0, "3 行 5 列、旗")]
    [InlineData(CellAppearance.Opened,       0, "3 行 5 列、空白")]
    [InlineData(CellAppearance.Opened,       2, "3 行 5 列、2")]
    [InlineData(CellAppearance.Mine,         0, "3 行 5 列、地雷")]
    [InlineData(CellAppearance.ExplodedMine, 0, "3 行 5 列、踏んだ地雷")]
    [InlineData(CellAppearance.WrongFlag,    0, "3 行 5 列、誤った旗")]
    public void AccessibleNameTellsThePositionFromOneAndTheState(CellAppearance appearance, int adjacentMineCount, string name)
        => Assert.Equal(name, CellPresentation.AccessibleNameOf(new DisplayPosition(2, 4), appearance, adjacentMineCount));
}
