using Bunit;
using Microsoft.AspNetCore.Components.Web;
using Shos.Minesweeper.Components;
using Shos.Minesweeper.GameLogic;

namespace Shos.Minesweeper.Tests.Components;

/// <summary>勝利カード（UI デザイン 2.4）。</summary>
public class WinCardTests : ComponentTestBase
{
    int playAgainCount;
    int closeCount;

    [Fact]
    public void CardIsAModelessDialogNamedByItsHeading()
    {
        var cut = RenderCard(45, new BestTimeResult(BestTimeOutcome.NotEligible, null));

        var dialog = cut.Find("[role=dialog]");
        Assert.False(dialog.HasAttribute("aria-modal"));
        Assert.Equal("クリア！", cut.Find($"#{dialog.GetAttribute("aria-labelledby")}").TextContent);
        Assert.Contains("タイム 45 秒", cut.Find(".time").TextContent);
    }

    [Theory]
    [InlineData(BestTimeOutcome.Updated,     52,   "ベストタイム更新！（これまで 52 秒）", true)]
    [InlineData(BestTimeOutcome.FirstRecord, null, "ベストタイムを記録しました",          true)]
    [InlineData(BestTimeOutcome.NotUpdated,  30,   "ベスト 30 秒",                        false)]
    public void BestTimeLineFollowsTheResult(BestTimeOutcome outcome, int? previousSeconds, string text, bool hasStar)
    {
        var cut = RenderCard(45, new BestTimeResult(outcome, previousSeconds));

        var line = cut.Find(".best-time");
        Assert.Equal(text, line.TextContent.Trim());
        Assert.Equal(hasStar, line.QuerySelector("svg[data-kind=Star]") is not null);
    }

    [Fact]
    public void CustomHasNoBestTimeLine()
        => Assert.Empty(RenderCard(45, new BestTimeResult(BestTimeOutcome.NotEligible, null)).FindAll(".best-time"));

    [Fact]
    public void ButtonsReportTheChoice()
    {
        var cut = RenderCard(45, new BestTimeResult(BestTimeOutcome.NotEligible, null));

        cut.Find("button.play-again").Click();
        cut.Find("button.close").Click();

        Assert.Equal((1, 1), (playAgainCount, closeCount));
    }

    [Fact]
    public void EscapeClosesTheCard()
    {
        var cut = RenderCard(45, new BestTimeResult(BestTimeOutcome.NotEligible, null));

        cut.Find("[role=dialog]").KeyDown(new KeyboardEventArgs { Key = "Escape" });

        Assert.Equal(1, closeCount);
    }

    [Fact]
    public void FocusMovesToTheHeadingNotToAButton()
    {
        var cut = RenderCard(45, new BestTimeResult(BestTimeOutcome.NotEligible, null));

        var heading = cut.Find("h2");
        Assert.Equal("-1", heading.GetAttribute("tabindex"));
        JSInterop.VerifyFocusAsyncInvoke().Arguments[0].ShouldBeElementReferenceTo(heading);
    }

    IRenderedComponent<WinCard> RenderCard(int seconds, BestTimeResult bestTime)
        => Render<WinCard>(parameters => parameters
               .Add(card => card.Seconds, seconds)
               .Add(card => card.BestTime, bestTime)
               .Add(card => card.OnPlayAgain, () => playAgainCount++)
               .Add(card => card.OnClose, () => closeCount++));
}
