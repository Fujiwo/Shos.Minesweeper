using Bunit;
using Shos.Minesweeper.Components;
using Shos.Minesweeper.GameLogic;
using Shos.Minesweeper.Tests.GameLogic;

namespace Shos.Minesweeper.Tests.Components;

/// <summary>経過時間の表示（仕様書 3.7、UI デザイン 2.2、6.4、アーキテクチャー設計書 7.3）。</summary>
public class ElapsedTimeTests : ComponentTestBase
{
    [Fact]
    public void ElapsedTimeIsZeroBeforeTheGameStarts()
    {
        var cut = RenderElapsedTime(new Game(Difficulty.Beginner, Time));

        Assert.Equal("経過時間 0 秒", cut.Find(".visually-hidden").TextContent);
    }

    [Fact]
    public void ElapsedTimeFollowsTheClock()
    {
        var game = TestGames.FromPicture(TestGames.WallPicture, Time);
        game.Open(new CellPosition(2, 0));
        var cut = RenderElapsedTime(game);

        Time.Advance(TimeSpan.FromSeconds(3));

        cut.WaitForAssertion(() => Assert.Equal("経過時間 3 秒", cut.Find(".visually-hidden").TextContent));
        Assert.Contains("3", cut.Find("[aria-hidden=true]").TextContent);
    }

    [Fact]
    public void ElapsedTimeIsRedrawnOnlyWhenTheShownSecondChanges()
    {
        var game = TestGames.FromPicture(TestGames.WallPicture, Time);
        game.Open(new CellPosition(2, 0));
        var cut = RenderElapsedTime(game);
        var renderCount = cut.RenderCount;

        for (var step = 0; step < 3; step++)
            Time.Advance(TimeSpan.FromMilliseconds(250));   // まだ 1 秒に届かない
        Assert.Equal(renderCount, cut.RenderCount);

        Time.Advance(TimeSpan.FromMilliseconds(250));
        // RenderCount は子の部品（ToolbarCounter）の描き直しも数える
        cut.WaitForAssertion(() => Assert.True(cut.RenderCount > renderCount));
        Assert.Equal("1", cut.Find(".counter-value").TextContent);
    }

    IRenderedComponent<ElapsedTime> RenderElapsedTime(Game game)
        => Render<ElapsedTime>(parameters => parameters.Add(elapsedTime => elapsedTime.Game, game));
}
