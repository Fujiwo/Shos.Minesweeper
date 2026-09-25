using Shos.Minesweeper.TestSupport;

namespace Shos.Minesweeper.GameLogic.Tests;

/// <summary>盤面の規則（仕様書 3.4、3.5）。盤面を変えられるのは Game だけなので、Game を通して確かめる。</summary>
public class BoardTests
{
    static readonly CellPosition ChordNumber = new(1, 1);

    [Fact]
    public void OpeningZeroCellOpensNeighborsInChainUntilNumbers()
    {
        var game = TestGames.FromPicture(TestGames.WallPicture);

        game.Open(new CellPosition(2, 0));

        TestGames.AssertPicture("""
            .2###
            .3###
            .3###
            .3###
            .2###
            """, game);
    }

    [Fact]
    public void OpeningNumberCellOpensOnlyThatCell()
    {
        var game = TestGames.FromPicture(TestGames.WallPicture);
        game.Open(new CellPosition(2, 0));

        game.Open(new CellPosition(2, 3));

        TestGames.AssertPicture("""
            .2###
            .3###
            .3#3#
            .3###
            .2###
            """, game);
    }

    [Fact]
    public void ClosedCellDoesNotTellItsAdjacentMineCount()
    {
        var game = TestGames.FromPicture(TestGames.WallPicture);
        game.Open(new CellPosition(2, 0));

        Assert.Equal(new Cell(CellState.Closed, 0), game.Board.CellAt(new CellPosition(2, 3)));
    }

    [Fact]
    public void TogglingFlagOnClosedCellFlagsIt()
    {
        var game = TestGames.FromPicture(TestGames.WallPicture);

        game.ToggleFlag(new CellPosition(0, 4));

        Assert.Equal(CellState.Flagged, game.Board.CellAt(new CellPosition(0, 4)).State);
    }

    [Fact]
    public void TogglingFlagOnFlaggedCellClosesIt()
    {
        var game = TestGames.FromPicture(TestGames.WallPicture);
        game.ToggleFlag(new CellPosition(0, 4));

        game.ToggleFlag(new CellPosition(0, 4));

        Assert.Equal(CellState.Closed, game.Board.CellAt(new CellPosition(0, 4)).State);
    }

    [Fact]
    public void TogglingFlagOnOpenedCellDoesNothing()
    {
        var game = TestGames.FromPicture(TestGames.WallPicture);
        game.Open(new CellPosition(2, 0));

        game.ToggleFlag(new CellPosition(2, 1));

        Assert.Equal(CellState.Opened, game.Board.CellAt(new CellPosition(2, 1)).State);
    }

    [Fact]
    public void OpeningFlaggedCellDoesNothing()
    {
        var game = TestGames.FromPicture(TestGames.WallPicture);
        game.Open(new CellPosition(2, 0));
        game.ToggleFlag(new CellPosition(2, 3));

        game.Open(new CellPosition(2, 3));

        Assert.Equal(CellState.Flagged, game.Board.CellAt(new CellPosition(2, 3)).State);
    }

    [Fact]
    public void ChainDoesNotOpenFlaggedCells()
    {
        var game = TestGames.FromPicture(TestGames.WallPicture);
        game.ToggleFlag(new CellPosition(0, 0));

        game.Open(new CellPosition(4, 0));

        TestGames.AssertPicture("""
            F2###
            .3###
            .3###
            .3###
            .2###
            """, game);
    }

    [Fact]
    public void OpeningMineShowsAllMinesTheExplodedMineAndWrongFlags()
    {
        var game = TestGames.FromPicture(TestGames.WallPicture);
        game.Open(new CellPosition(2, 0));
        game.ToggleFlag(new CellPosition(0, 2));
        game.ToggleFlag(new CellPosition(0, 4));

        game.Open(new CellPosition(1, 2));

        TestGames.AssertPicture("""
            .2F#x
            .3X##
            .3*##
            .3*##
            .2*##
            """, game);
    }

    [Fact]
    public void ChordOpensClosedNeighborsWhenFlagCountEqualsNumber()
    {
        var game = StartChordGame();
        game.ToggleFlag(new CellPosition(0, 0));
        game.ToggleFlag(new CellPosition(0, 2));

        game.Open(ChordNumber);

        TestGames.AssertPicture("""
            F2F1.
            1211.
            .....
            .....
            .....
            """, game);
    }

    [Fact]
    public void ChordDoesNothingWhenFlagCountDiffersFromNumber()
    {
        var game = StartChordGame();
        game.ToggleFlag(new CellPosition(0, 0));

        game.Open(ChordNumber);

        TestGames.AssertPicture("""
            F##1.
            1211.
            .....
            .....
            .....
            """, game);
    }

    [Fact]
    public void ChordWithWrongFlagOpensMine()
    {
        var game = StartChordGame();
        game.ToggleFlag(new CellPosition(0, 0));
        game.ToggleFlag(new CellPosition(0, 1));

        game.Open(ChordNumber);

        TestGames.AssertPicture("""
            FxX1.
            1211.
            .....
            .....
            .....
            """, game);
    }

    [Fact]
    public void ChordTargetsOfOpenedNumberAreClosedNeighborsWithoutFlags()
    {
        var game = StartChordGame();
        game.ToggleFlag(new CellPosition(0, 0));

        Assert.Equal([new(0, 1), new(0, 2)], game.Board.ChordTargetsOf(ChordNumber));
    }

    [Fact]
    public void ChordTargetsOfClosedCellAreEmpty()
        => Assert.Empty(StartChordGame().Board.ChordTargetsOf(new CellPosition(0, 1)));

    [Fact]
    public void ChordTargetsOfOpenedZeroCellAreEmpty()
        => Assert.Empty(StartChordGame().Board.ChordTargetsOf(new CellPosition(3, 3)));

    static Game StartChordGame()
    {
        var game = TestGames.FromPicture(TestGames.OneCellLeftPicture);
        game.Open(new CellPosition(4, 4));
        TestGames.AssertPicture("""
            ###1.
            1211.
            .....
            .....
            .....
            """, game);
        return game;
    }
}
