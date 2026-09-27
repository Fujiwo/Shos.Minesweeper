using Shos.Minesweeper.GameLogic;
using Shos.Minesweeper.TestSupport;

namespace Shos.Minesweeper.Presentation.Tests;

/// <summary>直前の操作から決めるマスごとの演出（クラス設計書 12.5 の表、UI デザイン 10.7）。</summary>
public class BoardAnimationTests
{
    // 左の 2 列が開く。操作したマス (2, 0) から最も遠いのは (0, 1) と (4, 1) で、距離は √5
    [Fact]
    public void ChainRevealsTheOpenedCellsOutwardFromTheMove()
    {
        var game = TestGames.FromPicture(TestGames.WallPicture);
        var move = game.Open(new CellPosition(2, 0));

        var animations = BoardAnimation.Of(move, game);

        Assert.Equal(10, animations.Count);
        Assert.All(animations.Values, animation => Assert.Equal(CellAnimationKind.Reveal, animation.Kind));
        Assert.Equal(0, animations[new CellPosition(2, 0)].DelayRatio);
        Assert.Equal(1, animations[new CellPosition(0, 1)].DelayRatio);
        Assert.Equal(1, animations[new CellPosition(4, 1)].DelayRatio);
        Assert.Equal(1 / Math.Sqrt(5), animations[new CellPosition(2, 1)].DelayRatio, precision: 10);
    }

    [Fact]
    public void SingleOpenedCellRevealsWithoutDelay()
    {
        var game = TestGames.FromPicture(TestGames.WallPicture);
        game.Open(new CellPosition(2, 0));

        var move = game.Open(new CellPosition(0, 3));

        Assert.Equal(new Dictionary<CellPosition, CellAnimation> { [new CellPosition(0, 3)] = new(CellAnimationKind.Reveal, 0) },
                     BoardAnimation.Of(move, game));
    }

    [Fact]
    public void FlagMovesHaveNoAnimation()
    {
        var game = TestGames.FromPicture(TestGames.WallPicture);
        var placed = game.ToggleFlag(new CellPosition(0, 0));
        Assert.Empty(BoardAnimation.Of(placed, game));

        var removed = game.ToggleFlag(new CellPosition(0, 0));
        Assert.Empty(BoardAnimation.Of(removed, game));
    }

    // 最初の一手では地雷を開けないので、(4, 0) から開いてから (0, 0) の地雷を踏む。
    // ほかの地雷は距離 2 と 4、誤った旗 (2, 2) は最後の地雷と同時（クラス設計書 12.11 の決定 11）
    [Fact]
    public void LosingExplodesTheMineAndShowsTheOtherMinesOutward()
    {
        var game = TestGames.FromPicture("""
            *.*.*
            .....
            .....
            .....
            .....
            """);
        game.ToggleFlag(new CellPosition(2, 2));
        game.Open(new CellPosition(4, 0));

        var move = game.Open(new CellPosition(0, 0));

        Assert.Equal(new Dictionary<CellPosition, CellAnimation> {
            [new CellPosition(0, 0)] = new(CellAnimationKind.Explode, 0),
            [new CellPosition(0, 2)] = new(CellAnimationKind.MineAppear, 0.5),
            [new CellPosition(0, 4)] = new(CellAnimationKind.MineAppear, 1),
            [new CellPosition(2, 2)] = new(CellAnimationKind.WrongFlagAppear, 1),
        }, BoardAnimation.Of(move, game));
    }

    // (2, 2) から開くと (0, 1) だけが残る。(0, 1) を開いて勝つ。地雷の旗までの距離は 1、1、5（クラス設計書 12.11 の決定 12）
    [Fact]
    public void WinningRevealsTheLastCellAndBouncesTheFlagsOutward()
    {
        var game = TestGames.FromPicture("""
            *.*..
            .....
            .....
            .....
            ....*
            """);
        game.Open(new CellPosition(2, 2));

        var move = game.Open(new CellPosition(0, 1));

        Assert.Equal(GameStatus.Won, move.Status);
        Assert.Equal(new Dictionary<CellPosition, CellAnimation> {
            [new CellPosition(0, 1)] = new(CellAnimationKind.Reveal, 0),
            [new CellPosition(0, 0)] = new(CellAnimationKind.FlagBounce, 0.2),
            [new CellPosition(0, 2)] = new(CellAnimationKind.FlagBounce, 0.2),
            [new CellPosition(4, 4)] = new(CellAnimationKind.FlagBounce, 1),
        }, BoardAnimation.Of(move, game));
    }
}
