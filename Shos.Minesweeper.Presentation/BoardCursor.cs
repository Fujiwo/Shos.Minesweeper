using Shos.Minesweeper.Display;
using Shos.Minesweeper.GameLogic;

namespace Shos.Minesweeper.Input;

/// <summary>
/// キーボードで選択しているマス（仕様書 4.5）。
/// 位置は盤面の座標で持つ。画面の向きが変わって盤面の縦と横が入れ替わっても、同じマスを選んだままにするためである。
/// </summary>
public sealed class BoardCursor
{
    public CellPosition Position { get; private set; } = new(0, 0);

    /// <summary>表示の向きで 1 マス動かす。表示の端では動かない（反対側に回り込まない）。</summary>
    public void Move(Direction direction, BoardPlacement placement)
    {
        var next = Neighbor(placement.ToDisplay(Position), direction);
        if (placement.Contains(next))
            Position = placement.ToBoard(next);
    }

    static DisplayPosition Neighbor(DisplayPosition position, Direction direction)
        => direction switch {
            Direction.Up    => position with { Row = position.Row - 1 },
            Direction.Down  => position with { Row = position.Row + 1 },
            Direction.Left  => position with { Column = position.Column - 1 },
            Direction.Right => position with { Column = position.Column + 1 },
            _               => throw new ArgumentOutOfRangeException(nameof(direction), direction, null)
        };
}
