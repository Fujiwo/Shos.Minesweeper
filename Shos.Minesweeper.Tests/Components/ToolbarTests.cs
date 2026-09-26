using Bunit;
using Shos.Minesweeper.Components;
using Shos.Minesweeper.GameLogic;
using Shos.Minesweeper.TestSupport;

namespace Shos.Minesweeper.Tests.Components;

/// <summary>ツールバー（UI デザイン 2.2、4.4 の顔、5.3、6.4）。</summary>
public class ToolbarTests : AppTestContext
{
    int difficultyClicks;
    int resetClicks;
    int flagModeClicks;
    int soundClicks;

    [Fact]
    public void DifficultyButtonShowsTheCurrentDifficulty()
    {
        var cut = RenderToolbar(new Game(Difficulty.Beginner, Time));

        var button = cut.Find("button.difficulty");
        Assert.Contains("初級", button.TextContent);
        Assert.Equal("難易度、初級", button.GetAttribute("aria-label"));
        Assert.Equal("難易度を変える", button.GetAttribute("title"));
    }

    [Fact]
    public void RemainingMineCountIsShownAndRead()
    {
        var game = new Game(Difficulty.Beginner, Time);
        game.ToggleFlag(new CellPosition(0, 0));

        var cut = RenderToolbar(game);

        var counter = cut.Find(".remaining-mines");
        Assert.Contains("9", counter.QuerySelector("[aria-hidden=true]")!.TextContent);
        Assert.Equal("残り地雷 9", counter.QuerySelector(".visually-hidden")!.TextContent);
    }

    [Fact]
    public void FourCharacterCountIsMarkedToBeShrunk()
    {
        var game = new Game(Difficulty.Custom(20, 10, 1), Time);
        for (var column = 0; column < 20; column++)
            for (var row = 0; row < 6; row++)
                if (row * 20 + column < 101)
                    game.ToggleFlag(new CellPosition(row, column));

        var cut = RenderToolbar(game);

        Assert.Contains("long", cut.Find(".remaining-mines").ClassList);
        Assert.Contains("-100", cut.Find(".remaining-mines").TextContent);
    }

    [Fact]
    public void ResetButtonShowsTheNormalFaceBeforeTheGameEnds()
        => Assert.Equal("FaceNormal", FaceOf(RenderToolbar(new Game(Difficulty.Beginner, Time))));

    [Fact]
    public void ResetButtonShowsTheSurprisedFaceWhilePressing()
        => Assert.Equal("FaceSurprised", FaceOf(RenderToolbar(new Game(Difficulty.Beginner, Time), isPressing: true)));

    [Fact]
    public void ResetButtonShowsTheWonFaceAfterWinning()
    {
        var game = TestGames.FromPicture(TestGames.OneCellLeftPicture);
        game.Open(new CellPosition(4, 4));
        game.Open(new CellPosition(0, 1));

        Assert.Equal("FaceWon", FaceOf(RenderToolbar(game, isPressing: true)));
    }

    [Fact]
    public void ResetButtonShowsTheLostFaceAfterLosing()
    {
        var game = TestGames.FromPicture(TestGames.WallPicture);
        game.Open(new CellPosition(2, 0));
        game.Open(new CellPosition(2, 2));

        Assert.Equal("FaceLost", FaceOf(RenderToolbar(game)));
    }

    [Fact]
    public void ResetButtonIsNamedNewGame()
    {
        var button = RenderToolbar(new Game(Difficulty.Beginner, Time)).Find("button.reset");

        Assert.Equal("新しいゲーム", button.GetAttribute("aria-label"));
        Assert.Equal("新しいゲーム", button.GetAttribute("title"));
    }

    [Fact]
    public void FlagModeButtonTellsItsState()
    {
        var off = RenderToolbar(new Game(Difficulty.Beginner, Time)).Find("button.flag-mode");
        var on = RenderToolbar(new Game(Difficulty.Beginner, Time), isFlagMode: true).Find("button.flag-mode");

        Assert.Equal("false", off.GetAttribute("aria-pressed"));
        Assert.Equal("旗モード（オフ）", off.GetAttribute("title"));
        Assert.Equal("true", on.GetAttribute("aria-pressed"));
        Assert.Equal("旗モード（オン）", on.GetAttribute("title"));
        Assert.Contains("旗モード", on.TextContent);
    }

    [Fact]
    public void SoundButtonTellsItsStateByTheIconAndTheTooltip()
    {
        var on = RenderToolbar(new Game(Difficulty.Beginner, Time)).Find("button.sound");
        var off = RenderToolbar(new Game(Difficulty.Beginner, Time), isSoundEnabled: false).Find("button.sound");

        Assert.Equal("効果音", on.GetAttribute("aria-label"));
        Assert.Equal("true", on.GetAttribute("aria-pressed"));
        Assert.Equal("効果音（オン）", on.GetAttribute("title"));
        Assert.Equal("SoundOn", on.QuerySelector("svg")!.GetAttribute("data-kind"));
        Assert.Equal("false", off.GetAttribute("aria-pressed"));
        Assert.Equal("効果音（オフ）", off.GetAttribute("title"));
        Assert.Equal("SoundOff", off.QuerySelector("svg")!.GetAttribute("data-kind"));
    }

    [Fact]
    public void ButtonsReportClicks()
    {
        var cut = RenderToolbar(new Game(Difficulty.Beginner, Time));

        cut.Find("button.difficulty").Click();
        cut.Find("button.reset").Click();
        cut.Find("button.flag-mode").Click();
        cut.Find("button.sound").Click();

        Assert.Equal((1, 1, 1, 1), (difficultyClicks, resetClicks, flagModeClicks, soundClicks));
    }

    [Fact]
    public void TabOrderFollowsTheToolbarOrder()
    {
        var cut = RenderToolbar(new Game(Difficulty.Beginner, Time));

        var focusable = cut.FindAll("button").Select(button => button.ClassList[0]);

        Assert.Equal(["difficulty", "reset", "flag-mode", "sound"], focusable);
    }

    IRenderedComponent<Toolbar> RenderToolbar(Game game, bool isPressing = false, bool isFlagMode = false, bool isSoundEnabled = true)
        => Render<Toolbar>(parameters => parameters
               .Add(toolbar => toolbar.Game, game)
               .Add(toolbar => toolbar.IsPressing, isPressing)
               .Add(toolbar => toolbar.IsFlagMode, isFlagMode)
               .Add(toolbar => toolbar.IsSoundEnabled, isSoundEnabled)
               .Add(toolbar => toolbar.OnDifficultyClick, () => difficultyClicks++)
               .Add(toolbar => toolbar.OnResetClick, () => resetClicks++)
               .Add(toolbar => toolbar.OnFlagModeClick, () => flagModeClicks++)
               .Add(toolbar => toolbar.OnSoundClick, () => soundClicks++));

    static string? FaceOf(IRenderedComponent<Toolbar> cut) => cut.Find("button.reset svg").GetAttribute("data-kind");
}
