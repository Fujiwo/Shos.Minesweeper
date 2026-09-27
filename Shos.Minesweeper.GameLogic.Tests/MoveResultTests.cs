using Shos.Minesweeper.TestSupport;

namespace Shos.Minesweeper.GameLogic.Tests;

/// <summary>1 回の盤面の操作の結果（Game.Open と Game.ToggleFlag の戻り値。クラス設計書 12.2 の表）。</summary>
public class MoveResultTests
{
    /// <summary>(1, 0) と (1, 2) に旗を立ててから右下を開くと、2 つの地雷の間の (0, 1) が連鎖から外れて残る盤面。(1, 1) は「2」になる。</summary>
    const string FencedPocketPicture = """
        *.*..
        .....
        .....
        .....
        .....
        """;

    [Fact]
    public void OpeningZeroCellReportsEveryCellOpenedByTheChain()
    {
        var game = TestGames.FromPicture(TestGames.WallPicture);
        var position = new CellPosition(2, 0);

        var move = game.Open(position);

        Assert.Equal(position, move.Position);
        Assert.Equal(MoveOutcome.Opened, move.Outcome);
        Assert.Equivalent(LeftTwoColumnsOfWall(), move.OpenedPositions, strict: true);
        Assert.Equal(GameStatus.Playing, move.Status);
    }

    [Fact]
    public void OpeningNumberCellReportsOnlyThatCell()
    {
        var game = TestGames.FromPicture(TestGames.WallPicture);
        game.Open(new CellPosition(2, 0));

        var move = game.Open(new CellPosition(0, 3));

        Assert.Equal(MoveOutcome.Opened, move.Outcome);
        Assert.Equal([new CellPosition(0, 3)], move.OpenedPositions);
        Assert.Equal(GameStatus.Playing, move.Status);
    }

    [Fact]
    public void OpeningOpenedZeroCellReportsNoChange()
    {
        var game = TestGames.FromPicture(TestGames.WallPicture);
        game.Open(new CellPosition(2, 0));

        var move = game.Open(new CellPosition(2, 0));

        Assert.Equal(MoveOutcome.NoChange, move.Outcome);
        Assert.Empty(move.OpenedPositions);
        Assert.Equal(GameStatus.Playing, move.Status);
    }

    [Fact]
    public void ChordWithFewerFlagsThanTheNumberReportsNoChange()
    {
        var game = TestGames.FromPicture(TestGames.WallPicture);
        game.Open(new CellPosition(2, 0));

        var move = game.Open(new CellPosition(2, 1));   // 「3」のまわりに旗がない

        Assert.Equal(MoveOutcome.NoChange, move.Outcome);
        Assert.Empty(move.OpenedPositions);
    }

    [Fact]
    public void ChordWithNoClosedCellLeftReportsNoChange()
    {
        var game = TestGames.FromPicture(TestGames.WallPicture);
        game.Open(new CellPosition(2, 0));
        foreach (var row in new[] { 1, 2, 3 })
            game.ToggleFlag(new CellPosition(row, 2));

        var move = game.Open(new CellPosition(2, 1));   // 「3」のまわりは、旗と開いたマスだけ

        Assert.Equal(MoveOutcome.NoChange, move.Outcome);
        Assert.Empty(move.OpenedPositions);
    }

    [Fact]
    public void FirstOpenThatWinsReportsTheOpenedCellsButNotTheAutomaticFlags()
    {
        var game = TestGames.FromPicture("""
            *....
            .....
            .....
            .....
            .....
            """);

        var move = game.Open(new CellPosition(4, 4));

        Assert.Equal(MoveOutcome.Opened, move.Outcome);
        Assert.Equal(24, move.OpenedPositions.Count);
        Assert.DoesNotContain(new CellPosition(0, 0), move.OpenedPositions);
        Assert.Equal(GameStatus.Won, move.Status);
    }

    [Fact]
    public void ChordThatOpensTheLastSafeCellReportsTheWin()
    {
        var game = TestGames.FromPicture(TestGames.OneCellLeftPicture);
        game.Open(new CellPosition(4, 4));
        game.ToggleFlag(new CellPosition(0, 0));
        game.ToggleFlag(new CellPosition(0, 2));

        var move = game.Open(new CellPosition(1, 1));   // 「2」のまわりに旗が 2 つ

        Assert.Equal(MoveOutcome.Opened, move.Outcome);
        Assert.Equal([new CellPosition(0, 1)], move.OpenedPositions);
        Assert.Equal(GameStatus.Won, move.Status);
    }

    [Fact]
    public void OpeningMineReportsTheMineAndTheLoss()
    {
        var game = TestGames.FromPicture(TestGames.WallPicture);
        game.Open(new CellPosition(2, 0));

        var move = game.Open(new CellPosition(0, 2));

        Assert.Equal(MoveOutcome.Opened, move.Outcome);
        Assert.Equal([new CellPosition(0, 2)], move.OpenedPositions);
        Assert.Equal(GameStatus.Lost, move.Status);
    }

    [Fact]
    public void ChordWithWrongFlagsReportsEveryOpenedMine()
    {
        var game = TestGames.FromPicture(FencedPocketPicture);
        game.ToggleFlag(new CellPosition(1, 0));
        game.ToggleFlag(new CellPosition(1, 2));
        game.Open(new CellPosition(4, 4));

        var move = game.Open(new CellPosition(1, 1));   // 「2」のまわりの旗は、どちらも誤り

        Assert.Equal(MoveOutcome.Opened, move.Outcome);
        Assert.Equivalent(new[] { new CellPosition(0, 0), new CellPosition(0, 1), new CellPosition(0, 2) }, move.OpenedPositions, strict: true);
        Assert.Equal(GameStatus.Lost, move.Status);
    }

    [Fact]
    public void FlaggingClosedCellReportsFlagPlaced()
    {
        var game = TestGames.FromPicture(TestGames.WallPicture);
        var position = new CellPosition(0, 0);

        var move = game.ToggleFlag(position);

        Assert.Equal(position, move.Position);
        Assert.Equal(MoveOutcome.FlagPlaced, move.Outcome);
        Assert.Empty(move.OpenedPositions);
        Assert.Equal(GameStatus.NotStarted, move.Status);
    }

    [Fact]
    public void UnflaggingReportsFlagRemoved()
    {
        var game = TestGames.FromPicture(TestGames.WallPicture);
        game.ToggleFlag(new CellPosition(0, 0));

        var move = game.ToggleFlag(new CellPosition(0, 0));

        Assert.Equal(MoveOutcome.FlagRemoved, move.Outcome);
        Assert.Empty(move.OpenedPositions);
    }

    [Fact]
    public void FlaggingOpenedCellReportsNoChange()
    {
        var game = TestGames.FromPicture(TestGames.WallPicture);
        game.Open(new CellPosition(2, 0));

        var move = game.ToggleFlag(new CellPosition(2, 0));

        Assert.Equal(MoveOutcome.NoChange, move.Outcome);
        Assert.Equal(GameStatus.Playing, move.Status);
    }

    static CellPosition[] LeftTwoColumnsOfWall()
        => [.. Enumerable.Range(0, 5).SelectMany(row => new[] { new CellPosition(row, 0), new CellPosition(row, 1) })];
}
