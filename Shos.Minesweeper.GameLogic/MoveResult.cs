namespace Shos.Minesweeper.GameLogic;

/// <summary>
/// 1 回の盤面の操作（手）の結果。効果音と演出は、この値から決める（クラス設計書 12.2）。
/// OpenedPositions は新たに開いたマスで、Opened のときだけ 1 つ以上ある。勝ったときに自動で立てた旗は含まない。
/// Status は操作の後のゲームの状態で、Won か Lost なら、その操作で勝敗が決まった。
/// </summary>
public readonly record struct MoveResult(CellPosition Position, MoveOutcome Outcome, IReadOnlyList<CellPosition> OpenedPositions, GameStatus Status);
