namespace Shos.Minesweeper.GameLogic;

/// <summary>プレイヤーに見せるマスの姿。Mine、ExplodedMine、WrongFlag は敗北した後にだけ現れる。</summary>
public enum CellAppearance
{
    Closed,
    Flagged,
    Opened,
    Mine,
    ExplodedMine,
    WrongFlag
}
