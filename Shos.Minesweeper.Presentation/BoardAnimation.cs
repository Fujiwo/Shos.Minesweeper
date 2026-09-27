using Shos.Minesweeper.GameLogic;

namespace Shos.Minesweeper.Presentation;

/// <summary>
/// 直前の操作から、マスごとの演出を決める（Web 版のクラス設計書 12.5 の表、UI デザイン 10.7）。
/// Web 版とデスクトップ版で使う（docs/desktop-console/05-class-design.md の 3.3）。
/// </summary>
public static class BoardAnimation
{
    /// <summary>演出するマスと、その演出。演出しないマスは含めない。旗の操作では空。</summary>
    public static IReadOnlyDictionary<CellPosition, CellAnimation> Of(MoveResult move, Game game)
    {
        var animations = new Dictionary<CellPosition, CellAnimation>();
        bool IsExploded(CellPosition position) => game.AppearanceOf(position) == CellAppearance.ExplodedMine;

        AddAtOnce(animations, CellAnimationKind.Explode, move.OpenedPositions.Where(IsExploded), delayRatio: 0);
        AddSpreading(animations, CellAnimationKind.Reveal, move.OpenedPositions.Where(position => !IsExploded(position)), move.Position);
        switch (move.Status) {
            case GameStatus.Lost:
                AddSpreading(animations, CellAnimationKind.MineAppear, PositionsShowing(game, CellAppearance.Mine), move.Position);
                // 誤った旗の × は、最後の地雷と同時に現れる（クラス設計書 12.11 の決定 11）
                AddAtOnce(animations, CellAnimationKind.WrongFlagAppear, PositionsShowing(game, CellAppearance.WrongFlag), delayRatio: 1);
                break;
            case GameStatus.Won:
                AddSpreading(animations, CellAnimationKind.FlagBounce, PositionsShowing(game, CellAppearance.Flagged), move.Position);
                break;
        }
        return animations;
    }

    static void AddAtOnce(Dictionary<CellPosition, CellAnimation> animations, CellAnimationKind kind, IEnumerable<CellPosition> positions, double delayRatio)
    {
        foreach (var position in positions)
            animations[position] = new(kind, delayRatio);
    }

    // 操作したマスから近い順に始まるように、遅れの比を「距離 ÷ そのマスの中での最大の距離」にする。最大が 0 なら、比は 0
    static void AddSpreading(Dictionary<CellPosition, CellAnimation> animations, CellAnimationKind kind, IEnumerable<CellPosition> positions, CellPosition origin)
    {
        var distances = positions.ToDictionary(position => position, position => DistanceBetween(origin, position));
        var farthest = distances.Values.DefaultIfEmpty(0).Max();
        foreach (var (position, distance) in distances)
            animations[position] = new(kind, farthest == 0 ? 0 : distance / farthest);
    }

    static IEnumerable<CellPosition> PositionsShowing(Game game, CellAppearance appearance)
        => game.Board.AllPositions.Where(position => game.AppearanceOf(position) == appearance);

    // 盤面の座標でのマスの中心どうしの距離（マスの数で数える）。表示で縦と横を入れ替えても変わらない（アーキテクチャー設計書 8.6）
    static double DistanceBetween(CellPosition from, CellPosition to)
        => Math.Sqrt(Math.Pow(to.Row - from.Row, 2) + Math.Pow(to.Column - from.Column, 2));
}
