namespace Shos.Minesweeper.GameLogic;

/// <summary>1 回の盤面の操作で起きたことの種類（クラス設計書 12.2）。</summary>
public enum MoveOutcome
{
    NoChange,
    Opened,
    FlagPlaced,
    FlagRemoved
}
