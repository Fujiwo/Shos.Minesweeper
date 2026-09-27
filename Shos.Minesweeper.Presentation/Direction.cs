namespace Shos.Minesweeper.Presentation;

/// <summary>
/// 上下左右の方向。BoardCursor は盤面の向きの方向で受け取る。Web 版は、表示の向きの矢印キーの方向を、
/// BoardPlacement.ToBoard で盤面の向きに変えてから渡す（盤面の縦と横を入れ替えて表示することがあるため）。
/// </summary>
public enum Direction
{
    Up,
    Down,
    Left,
    Right
}
