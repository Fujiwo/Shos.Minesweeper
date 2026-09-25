namespace Shos.Minesweeper.GameLogic;

/// <summary>候補の中から、地雷を置く位置を地雷数だけ選ぶ。</summary>
public delegate IReadOnlyCollection<CellPosition> MineChooser(IReadOnlyList<CellPosition> candidates, int mineCount);
