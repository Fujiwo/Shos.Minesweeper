using Microsoft.Extensions.Time.Testing;
using Shos.Minesweeper.GameLogic;

namespace Shos.Minesweeper.Tests.GameLogic;

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
}
