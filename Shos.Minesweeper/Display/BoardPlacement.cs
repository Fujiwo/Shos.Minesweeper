using Shos.Minesweeper.GameLogic;
using Shos.Minesweeper.Presentation;

namespace Shos.Minesweeper.Display;

/// <summary>盤面の置き方（向きとマスの大きさ）と、表示の座標と盤面の座標の変換（UI デザイン 3.2）。</summary>
public sealed record BoardPlacement
{
    public int CellSize { get; }
    public bool IsTransposed { get; }
    public int RowCount { get; }
    public int ColumnCount { get; }

    /// <summary>表示している向きでの、枠を含む盤面の高さ（px）。</summary>
    public int BoardHeight => RowCount * CellSize + BoardDimensions.FrameWidth * 2;

    BoardPlacement(int cellSize, bool isTransposed, int rowCount, int columnCount)
    {
        CellSize = cellSize;
        IsTransposed = isTransposed;
        RowCount = rowCount;
        ColumnCount = columnCount;
    }

    /// <summary>盤面の領域の大きさ（枠を含む）から、置き方を求める。</summary>
    public static BoardPlacement Calculate(double areaWidth, double areaHeight, Difficulty difficulty)
    {
        var width = areaWidth - BoardDimensions.FrameWidth * 2;
        var height = areaHeight - BoardDimensions.FrameWidth * 2;
        var asIs = FittingCellSize(width, height, columnCount: difficulty.Width, rowCount: difficulty.Height);
        var transposed = FittingCellSize(width, height, columnCount: difficulty.Height, rowCount: difficulty.Width);
        // 入れ替えたほうがマスが大きくなるときだけ入れ替える。同じなら入れ替えない
        var isTransposed = transposed > asIs;
        // マスの境目の線がにじまないように、整数の px にする
        var cellSize = Math.Clamp((int)Math.Floor(Math.Max(asIs, transposed)), BoardDimensions.MinCellSize, BoardDimensions.MaxCellSize);
        return isTransposed
               ? new(cellSize, isTransposed, rowCount: difficulty.Width, columnCount: difficulty.Height)
               : new(cellSize, isTransposed, rowCount: difficulty.Height, columnCount: difficulty.Width);
    }

    public CellPosition ToBoard(DisplayPosition position)
        => IsTransposed ? new(position.Column, position.Row) : new(position.Row, position.Column);

    /// <summary>
    /// 表示の向きの方向（矢印キー）を、盤面の向きの方向に変える。カーソルは盤面の座標で動くためである
    /// （デスクトップ版・コンソール版のクラス設計書 3.2）。表示の (行, 列) は、入れ替えているときは盤面の (列, 行) である。
    /// </summary>
    public Direction ToBoard(Direction direction)
        => !IsTransposed ? direction
           : direction switch {
               Direction.Up    => Direction.Left,
               Direction.Down  => Direction.Right,
               Direction.Left  => Direction.Up,
               Direction.Right => Direction.Down,
               _               => throw new ArgumentOutOfRangeException(nameof(direction), direction, null)
           };

    static double FittingCellSize(double width, double height, int columnCount, int rowCount)
        => Math.Min(width / columnCount, height / rowCount);
}
