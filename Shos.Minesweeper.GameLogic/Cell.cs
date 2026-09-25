namespace Shos.Minesweeper.GameLogic;

/// <summary>1 つのマスの状態と数字。AdjacentMineCount は開いたマスでだけ意味を持つ。</summary>
public readonly record struct Cell(CellState State, int AdjacentMineCount)
{
    public bool IsOpenedNumber => State == CellState.Opened && AdjacentMineCount > 0;

    /// <summary>「開く」で何か起きるか（仕様書 3.4）。開いた数字のマスではコードになる。</summary>
    public bool CanOpen => State == CellState.Closed || IsOpenedNumber;

    /// <summary>「旗」で何か起きるか（仕様書 3.4）。</summary>
    public bool CanToggleFlag => State != CellState.Opened;
}
