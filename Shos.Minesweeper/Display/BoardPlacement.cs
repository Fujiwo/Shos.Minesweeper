using Shos.Minesweeper.GameLogic;

namespace Shos.Minesweeper.Display;

/// <summary>盤面の置き方（向きとマスの大きさ）と、表示の座標と盤面の座標の変換（UI デザイン 3.2）。</summary>
public sealed record BoardPlacement
{
    public const int MinCellSize = 20;
    public const int MaxCellSize = 48;
    public const int FrameWidth = 3;

    public int CellSize { get; }
    public bool IsTransposed { get; }
    public int RowCount { get; }
    public int ColumnCount { get; }

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
        var width = areaWidth - FrameWidth * 2;
        var height = areaHeight - FrameWidth * 2;
        var asIs = FittingCellSize(width, height, columnCount: difficulty.Width, rowCount: difficulty.Height);
        var transposed = FittingCellSize(width, height, columnCount: difficulty.Height, rowCount: difficulty.Width);
        // 入れ替えたほうがマスが大きくなるときだけ入れ替える。同じなら入れ替えない
        var isTransposed = transposed > asIs;
        // マスの境目の線がにじまないように、整数の px にする
        var cellSize = Math.Clamp((int)Math.Floor(Math.Max(asIs, transposed)), MinCellSize, MaxCellSize);
        return isTransposed
               ? new(cellSize, isTransposed, rowCount: difficulty.Width, columnCount: difficulty.Height)
               : new(cellSize, isTransposed, rowCount: difficulty.Height, columnCount: difficulty.Width);
    }

    public DisplayPosition ToDisplay(CellPosition position)
        => IsTransposed ? new(position.Column, position.Row) : new(position.Row, position.Column);

    public CellPosition ToBoard(DisplayPosition position)
        => IsTransposed ? new(position.Column, position.Row) : new(position.Row, position.Column);

    public bool Contains(DisplayPosition position)
        => 0 <= position.Row && position.Row < RowCount && 0 <= position.Column && position.Column < ColumnCount;

    static double FittingCellSize(double width, double height, int columnCount, int rowCount)
        => Math.Min(width / columnCount, height / rowCount);
}
