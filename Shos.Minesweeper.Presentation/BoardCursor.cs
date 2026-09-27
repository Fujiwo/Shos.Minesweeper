using Shos.Minesweeper.GameLogic;

namespace Shos.Minesweeper.Presentation;

/// <summary>
/// キーボードで選択しているマス（Web 版の仕様書 4.5、デスクトップ版・コンソール版の仕様書 4.3、5.3）。
/// 位置と方向は盤面の座標で持つ。どの版でも同じ規則（端で止まる、回り込まない）で動く。
/// 新しいゲームでは、各版が作り直して左上に戻す（デスクトップ版・コンソール版のクラス設計書 3.2）。
/// </summary>
public sealed class BoardCursor
{
    public CellPosition Position { get; private set; } = new(0, 0);

    /// <summary>盤面の向きで 1 マス動かす。盤面の端では動かない（反対側に回り込まない）。</summary>
    public void Move(Direction direction, Board board)
    {
        var next = Neighbor(Position, direction);
        if (board.Contains(next))
            Position = next;
    }

    /// <summary>そのマスに移す（デスクトップ版で、マウスで押したマス）。盤面の外の位置は受け取らない。</summary>
    public void MoveTo(CellPosition position, Board board)
    {
        // 盤面の外を指したまま進むと、離れた場所の Board.CellAt で例外になり、原因を追いにくい。前提条件をここで確かめる
        if (!board.Contains(position))
            throw new ArgumentOutOfRangeException(nameof(position), position, "盤面の外の位置です。");
        Position = position;
    }

    static CellPosition Neighbor(CellPosition position, Direction direction)
        => direction switch {
            Direction.Up    => position with { Row = position.Row - 1 },
            Direction.Down  => position with { Row = position.Row + 1 },
            Direction.Left  => position with { Column = position.Column - 1 },
            Direction.Right => position with { Column = position.Column + 1 },
            _               => throw new ArgumentOutOfRangeException(nameof(direction), direction, null)
        };
}
