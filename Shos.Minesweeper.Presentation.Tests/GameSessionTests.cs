using Microsoft.Extensions.Time.Testing;
using Shos.Minesweeper.GameLogic;
using Shos.Minesweeper.TestSupport;

namespace Shos.Minesweeper.Presentation.Tests;

/// <summary>1 回のゲームの進め方と、音の出口（クラス設計書 12.4）。</summary>
public class GameSessionTests
{
    readonly List<SoundEffect> played = [];

    GameSession SessionOf(string picture)
        => new(TestGames.DifficultyOf(picture), new FakeTimeProvider(), played.Add, TestGames.MineChooserOf(picture));

    [Fact]
    public void NewSessionHasANotStartedGameAndNoLastMove()
    {
        var session = SessionOf(TestGames.WallPicture);

        Assert.Equal(GameStatus.NotStarted, session.Game.Status);
        Assert.Equal(TestGames.DifficultyOf(TestGames.WallPicture), session.Game.Difficulty);
        Assert.Null(session.LastMove);
    }

    [Fact]
    public void OpeningReturnsTheResultOfTheGame()
    {
        var session = SessionOf(TestGames.WallPicture);

        var move = session.Open(new CellPosition(2, 0));

        Assert.Equal(MoveOutcome.Opened, move.Outcome);
        Assert.Equal(10, move.OpenedPositions.Count);
        Assert.Equal(GameStatus.Playing, session.Game.Status);
    }

    [Fact]
    public void EachMovePassesItsEffectToTheOutput()
    {
        var session = SessionOf(TestGames.WallPicture);

        session.Open(new CellPosition(2, 0));        // 0 の連鎖
        session.Open(new CellPosition(0, 3));        // 数字の 1 マス
        session.ToggleFlag(new CellPosition(0, 4));
        session.ToggleFlag(new CellPosition(0, 4));

        Assert.Equal([SoundEffect.Chain, SoundEffect.Open, SoundEffect.FlagPlaced, SoundEffect.FlagRemoved], played);
    }

    [Fact]
    public void WinningPassesOnlyTheWinEffect()
    {
        var session = SessionOf(TestGames.OneCellLeftPicture);
        session.Open(new CellPosition(4, 4));

        session.Open(new CellPosition(0, 1));        // 最後の 1 マス

        Assert.Equal([SoundEffect.Chain, SoundEffect.Won], played);
    }

    [Fact]
    public void LosingPassesOnlyTheLossEffect()
    {
        var session = SessionOf(TestGames.WallPicture);
        session.Open(new CellPosition(2, 0));

        session.Open(new CellPosition(0, 2));        // 地雷

        Assert.Equal([SoundEffect.Chain, SoundEffect.Lost], played);
    }

    [Fact]
    public void MoveThatChangesSomethingBecomesTheLastMove()
    {
        var session = SessionOf(TestGames.WallPicture);

        var move = session.Open(new CellPosition(2, 0));

        Assert.Equal(move, session.LastMove);
    }

    [Fact]
    public void MoveThatChangesNothingPassesNoEffectAndKeepsTheLastMove()
    {
        var session = SessionOf(TestGames.WallPicture);
        var chain = session.Open(new CellPosition(2, 0));

        var move = session.Open(new CellPosition(2, 1));   // 「3」のまわりに旗がないコード

        Assert.Equal(MoveOutcome.NoChange, move.Outcome);
        Assert.Equal([SoundEffect.Chain], played);
        Assert.Equal(chain, session.LastMove);            // 前の操作の演出を止めない
    }

    [Fact]
    public void StartingNewGameReplacesTheGameAndClearsTheLastMove()
    {
        var session = SessionOf(TestGames.WallPicture);
        var previousGame = session.Game;
        session.Open(new CellPosition(2, 0));

        session.StartNewGame(Difficulty.Beginner);

        Assert.NotSame(previousGame, session.Game);
        Assert.Equal(Difficulty.Beginner, session.Game.Difficulty);
        Assert.Equal(GameStatus.NotStarted, session.Game.Status);
        Assert.Null(session.LastMove);
    }

    [Fact]
    public void NewGameUsesTheSameWayOfChoosingMines()
    {
        var session = SessionOf(TestGames.WallPicture);
        session.StartNewGame(TestGames.DifficultyOf(TestGames.WallPicture));

        session.Open(new CellPosition(2, 0));

        TestGames.AssertPicture("""
            .2###
            .3###
            .3###
            .3###
            .2###
            """, session.Game);
    }

    [Fact]
    public void SessionWithoutOutputPlaysWithoutErrors()
    {
        var picture = TestGames.WallPicture;
        var session = new GameSession(TestGames.DifficultyOf(picture), new FakeTimeProvider(), chooseMines: TestGames.MineChooserOf(picture));

        var move = session.Open(new CellPosition(2, 0));

        Assert.Equal(MoveOutcome.Opened, move.Outcome);
        Assert.Equal(move, session.LastMove);
    }

    [Fact]
    public void MovesAfterTheGameIsOverAreProgramErrors()
    {
        var session = SessionOf(TestGames.WallPicture);
        session.Open(new CellPosition(2, 0));
        session.Open(new CellPosition(0, 2));        // 負け

        Assert.Throws<InvalidOperationException>(() => session.Open(new CellPosition(0, 3)));
        Assert.Throws<InvalidOperationException>(() => session.ToggleFlag(new CellPosition(0, 3)));
    }
}
