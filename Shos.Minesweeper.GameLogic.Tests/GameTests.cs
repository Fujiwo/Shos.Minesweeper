using Microsoft.Extensions.Time.Testing;
using Shos.Minesweeper.TestSupport;

namespace Shos.Minesweeper.GameLogic.Tests;

public class GameTests
{
    [Fact]
    public void NewGameIsNotStarted()
    {
        var game = new Game(Difficulty.Beginner, new FakeTimeProvider());

        Assert.Equal(GameStatus.NotStarted, game.Status);
        Assert.False(game.IsOver);
        Assert.Equal(Difficulty.Beginner, game.Difficulty);
    }

    [Fact]
    public void NewGameHasAllCellsClosed()
    {
        var game = TestGames.FromPicture("""
            *....
            .....
            .....
            .....
            .....
            """);

        TestGames.AssertPicture("""
            #####
            #####
            #####
            #####
            #####
            """, game);
    }

    [Fact]
    public void NewGameBoardHasTheSizeOfTheDifficulty()
    {
        var game = new Game(Difficulty.Expert, new FakeTimeProvider());

        Assert.Equal(30, game.Board.Width);
        Assert.Equal(16, game.Board.Height);
    }

    [Fact]
    public void NewGameHasAllMinesRemainingAndNoElapsedTime()
    {
        var game = new Game(Difficulty.Intermediate, new FakeTimeProvider());

        Assert.Equal(40, game.RemainingMineCount);
        Assert.Equal(0, game.ElapsedSeconds);
    }

    [Fact]
    public void FirstOpenStartsTheGame()
    {
        var game = TestGames.FromPicture(TestGames.WallPicture);

        game.Open(new CellPosition(2, 0));

        Assert.Equal(GameStatus.Playing, game.Status);
    }

    [Fact]
    public void OpeningFlaggedCellBeforeStartDoesNotStartTheGame()
    {
        var game = TestGames.FromPicture(TestGames.WallPicture);
        game.ToggleFlag(new CellPosition(2, 0));

        game.Open(new CellPosition(2, 0));

        Assert.Equal(GameStatus.NotStarted, game.Status);
    }

    [Fact]
    public void OpeningMineLosesTheGame()
    {
        var game = TestGames.FromPicture(TestGames.WallPicture);
        game.Open(new CellPosition(2, 0));

        game.Open(new CellPosition(2, 2));

        Assert.Equal(GameStatus.Lost, game.Status);
        Assert.True(game.IsOver);
    }

    [Fact]
    public void OpeningAllSafeCellsWinsTheGameAndFlagsAllMines()
    {
        var game = TestGames.FromPicture(TestGames.OneCellLeftPicture);
        game.Open(new CellPosition(4, 4));

        game.Open(new CellPosition(0, 1));

        Assert.Equal(GameStatus.Won, game.Status);
        Assert.True(game.IsOver);
        Assert.Equal(0, game.RemainingMineCount);
        TestGames.AssertPicture("""
            F2F1.
            1211.
            .....
            .....
            .....
            """, game);
    }

    [Fact]
    public void FirstOpenCanWinImmediately()
    {
        var game = TestGames.FromPicture("""
            *....
            .....
            .....
            .....
            ....*
            """);

        game.Open(new CellPosition(2, 2));

        Assert.Equal(GameStatus.Won, game.Status);
    }

    [Fact]
    public void RemainingMineCountIsMineCountMinusFlagsAndCanBeNegative()
    {
        var game = new Game(Difficulty.Custom(5, 5, 1), new FakeTimeProvider());

        game.ToggleFlag(new CellPosition(0, 0));
        game.ToggleFlag(new CellPosition(0, 1));
        game.ToggleFlag(new CellPosition(0, 2));

        Assert.Equal(-2, game.RemainingMineCount);
    }

    [Theory]
    [InlineData(0, 0, 21)]    // 角: 周囲は 3 マス
    [InlineData(0, 2, 19)]    // 辺: 周囲は 5 マス
    [InlineData(2, 2, 16)]    // 中央: 周囲は 8 マス
    public void MineCandidatesExcludeTheFirstOpenedCellAndItsNeighbors(int row, int column, int candidateCount)
    {
        var firstPosition = new CellPosition(row, column);
        IReadOnlyList<CellPosition> candidates = [];
        var game = new Game(Difficulty.Custom(5, 5, 1), new FakeTimeProvider(), (given, count) => {
            candidates = given;
            return [given[0]];
        });

        game.Open(firstPosition);

        Assert.Equal(candidateCount, candidates.Count);
        Assert.DoesNotContain(candidates, candidate => Math.Abs(candidate.Row - row) <= 1 && Math.Abs(candidate.Column - column) <= 1);
    }

    [Fact]
    public void ElapsedTimeDoesNotRunBeforeTheFirstOpen()
    {
        var time = new FakeTimeProvider();
        var game = TestGames.FromPicture(TestGames.WallPicture, time);

        time.Advance(TimeSpan.FromSeconds(5));

        Assert.Equal(0, game.ElapsedSeconds);
    }

    [Fact]
    public void ElapsedTimeIsWholeSecondsSinceTheFirstOpen()
    {
        var time = new FakeTimeProvider();
        var game = TestGames.FromPicture(TestGames.WallPicture, time);
        time.Advance(TimeSpan.FromSeconds(5));
        game.Open(new CellPosition(2, 0));

        time.Advance(TimeSpan.FromMilliseconds(2999));

        Assert.Equal(2, game.ElapsedSeconds);
    }

    [Fact]
    public void ElapsedTimeStopsAt999()
    {
        var time = new FakeTimeProvider();
        var game = TestGames.FromPicture(TestGames.WallPicture, time);
        game.Open(new CellPosition(2, 0));

        time.Advance(TimeSpan.FromSeconds(1000));

        Assert.Equal(Game.MaxElapsedSeconds, game.ElapsedSeconds);
    }

    [Fact]
    public void ElapsedTimeStopsWhenTheGameIsLost()
    {
        var time = new FakeTimeProvider();
        var game = TestGames.FromPicture(TestGames.WallPicture, time);
        game.Open(new CellPosition(2, 0));
        time.Advance(TimeSpan.FromSeconds(3));
        game.Open(new CellPosition(2, 2));

        time.Advance(TimeSpan.FromSeconds(10));

        Assert.Equal(3, game.ElapsedSeconds);
    }

    [Fact]
    public void ElapsedTimeStopsWhenTheGameIsWon()
    {
        var time = new FakeTimeProvider();
        var game = TestGames.FromPicture(TestGames.OneCellLeftPicture, time);
        game.Open(new CellPosition(4, 4));
        time.Advance(TimeSpan.FromSeconds(7));
        game.Open(new CellPosition(0, 1));

        time.Advance(TimeSpan.FromSeconds(10));

        Assert.Equal(7, game.ElapsedSeconds);
    }

    [Fact]
    public void ElapsedTimeIsZeroWhenTheFirstOpenWins()
    {
        var time = new FakeTimeProvider();
        var game = TestGames.FromPicture("""
            *....
            .....
            .....
            .....
            ....*
            """, time);
        game.Open(new CellPosition(2, 2));

        time.Advance(TimeSpan.FromSeconds(10));

        Assert.Equal(0, game.ElapsedSeconds);
    }

    [Fact]
    public void OpeningAfterTheGameIsOverIsAProgramError()
    {
        var game = TestGames.FromPicture(TestGames.WallPicture);
        game.Open(new CellPosition(2, 0));
        game.Open(new CellPosition(2, 2));

        Assert.Throws<InvalidOperationException>(() => game.Open(new CellPosition(2, 3)));
    }

    [Fact]
    public void TogglingFlagAfterTheGameIsOverIsAProgramError()
    {
        var game = TestGames.FromPicture(TestGames.OneCellLeftPicture);
        game.Open(new CellPosition(4, 4));
        game.Open(new CellPosition(0, 1));

        Assert.Throws<InvalidOperationException>(() => game.ToggleFlag(new CellPosition(0, 0)));
    }

    public static TheoryData<int, int> OutsidePositions => new() { { -1, 0 }, { 0, -1 }, { 5, 0 }, { 0, 5 } };

    [Theory]
    [MemberData(nameof(OutsidePositions))]
    public void PositionsOutsideTheBoardAreProgramErrors(int row, int column)
    {
        var game = TestGames.FromPicture(TestGames.WallPicture);
        var outside = new CellPosition(row, column);

        Assert.Throws<ArgumentOutOfRangeException>(() => game.Open(outside));
        Assert.Throws<ArgumentOutOfRangeException>(() => game.ToggleFlag(outside));
        Assert.Throws<ArgumentOutOfRangeException>(() => game.AppearanceOf(outside));
        Assert.Throws<ArgumentOutOfRangeException>(() => game.Board.CellAt(outside));
        Assert.Throws<ArgumentOutOfRangeException>(() => game.Board.ChordTargetsOf(outside));
    }

    [Fact]
    public void ChoosingMinesOutsideTheCandidatesIsAProgramError()
    {
        var game = new Game(Difficulty.Custom(5, 5, 1), new FakeTimeProvider(), (_, _) => [new CellPosition(0, 0)]);

        Assert.Throws<InvalidOperationException>(() => game.Open(new CellPosition(0, 0)));
    }

    [Fact]
    public void ChoosingADifferentNumberOfMinesIsAProgramError()
    {
        var game = new Game(Difficulty.Custom(5, 5, 2), new FakeTimeProvider(), (candidates, _) => [candidates[0]]);

        Assert.Throws<InvalidOperationException>(() => game.Open(new CellPosition(0, 0)));
    }

    [Fact]
    public void ChoosingTheSamePositionTwiceIsAProgramError()
    {
        var game = new Game(Difficulty.Custom(5, 5, 2), new FakeTimeProvider(), (candidates, _) => [candidates[0], candidates[0]]);

        Assert.Throws<InvalidOperationException>(() => game.Open(new CellPosition(0, 0)));
    }

    [Fact]
    public void RandomChoiceTakesTheMineCountOfDistinctCandidates()
    {
        CellPosition[] candidates = [new(0, 0), new(0, 1), new(0, 2), new(0, 3), new(0, 4)];

        var mines = Game.ChooseMinesRandomly(candidates, 3);

        Assert.Equal(3, mines.Distinct().Count());
        Assert.All(mines, mine => Assert.Contains(mine, candidates));
    }

    [Fact]
    public void RandomChoiceOfMoreMinesThanCandidatesIsAProgramError()
        => Assert.Throws<ArgumentOutOfRangeException>(() => Game.ChooseMinesRandomly([new CellPosition(0, 0)], 2));

    [Theory]
    [InlineData(0, 0)]      // 角
    [InlineData(0, 10)]     // 辺
    [InlineData(8, 15)]     // 中央付近
    public void FirstOpenedCellIsAlwaysZeroWithRandomMines(int row, int column)
    {
        var position = new CellPosition(row, column);
        for (var trial = 0; trial < 50; trial++) {
            var game = new Game(Difficulty.Expert, new FakeTimeProvider());

            game.Open(position);

            Assert.Equal(new Cell(CellState.Opened, 0), game.Board.CellAt(position));
        }
    }

    [Fact]
    public void FlaggedCellsBeforeStartAreStillMineCandidates()
    {
        IReadOnlyList<CellPosition> candidates = [];
        var game = new Game(Difficulty.Custom(5, 5, 1), new FakeTimeProvider(), (given, count) => {
            candidates = given;
            return [given[0]];
        });
        game.ToggleFlag(new CellPosition(4, 4));

        game.Open(new CellPosition(0, 0));

        Assert.Contains(new CellPosition(4, 4), candidates);
    }

    // 操作の結果（クラス設計書 12.2 の表）

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
