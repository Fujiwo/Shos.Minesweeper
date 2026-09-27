using Shos.Minesweeper.GameLogic;

namespace Shos.Minesweeper.Presentation.Tests;

/// <summary>盤面とマスの読み上げの名前（Web 版の UI デザイン 6.4、デスクトップ版・コンソール版のクラス設計書 3.4）。</summary>
public class BoardNamesTests
{
    [Fact]
    public void BoardNameTellsTheRowAndColumnCounts()
        => Assert.Equal("盤面、30 行 16 列", BoardNames.Of(rowCount: 30, columnCount: 16));

    [Theory]
    [InlineData(CellAppearance.Closed,       0, "3 行 5 列、未開放")]
    [InlineData(CellAppearance.Flagged,      0, "3 行 5 列、旗")]
    [InlineData(CellAppearance.Opened,       0, "3 行 5 列、空白")]
    [InlineData(CellAppearance.Opened,       2, "3 行 5 列、2")]
    [InlineData(CellAppearance.Mine,         0, "3 行 5 列、地雷")]
    [InlineData(CellAppearance.ExplodedMine, 0, "3 行 5 列、踏んだ地雷")]
    [InlineData(CellAppearance.WrongFlag,    0, "3 行 5 列、誤った旗")]
    public void CellNameTellsThePositionFromOneAndTheState(CellAppearance appearance, int adjacentMineCount, string name)
        => Assert.Equal(name, BoardNames.CellOf(row: 2, column: 4, appearance, adjacentMineCount));
}
