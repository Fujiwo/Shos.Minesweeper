using Shos.Minesweeper.GameLogic;

namespace Shos.Minesweeper.Presentation;

/// <summary>
/// 盤面とマスの読み上げの名前（Web 版の UI デザイン 6.4、デスクトップ版の UI デザイン 2.11）。
/// 行と列は、表示している向きで 0 から数えた値を受け取り、1 から数えて名前にする。
/// Web 版は表示の位置（縦と横を入れ替えることがある）を、デスクトップ版は盤面の位置を渡す（デスクトップ版・コンソール版のクラス設計書 3.4）。
/// </summary>
public static class BoardNames
{
    /// <summary>「盤面、9 行 9 列」。</summary>
    public static string Of(int rowCount, int columnCount) => $"盤面、{rowCount} 行 {columnCount} 列";

    /// <summary>「3 行 5 列、未開放」。</summary>
    public static string CellOf(int row, int column, CellAppearance appearance, int adjacentMineCount)
        => $"{row + 1} 行 {column + 1} 列、{StateNameOf(appearance, adjacentMineCount)}";

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
