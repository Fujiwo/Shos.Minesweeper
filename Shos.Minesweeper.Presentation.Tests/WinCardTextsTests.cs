using Shos.Minesweeper.GameLogic;

namespace Shos.Minesweeper.Presentation.Tests;

/// <summary>勝利カードの文言のうち、値を埋め込むもの（Web 版の UI デザイン 2.4、7 章）。</summary>
public class WinCardTextsTests
{
    [Fact]
    public void WinCardTimeTellsTheSeconds()
        => Assert.Equal("タイム 45 秒", WinCardTexts.TimeOf(45));

    [Theory]
    [InlineData(BestTimeOutcome.Updated,     50,   "ベストタイム更新！（これまで 50 秒）")]
    [InlineData(BestTimeOutcome.FirstRecord, null, "ベストタイムを記録しました")]
    [InlineData(BestTimeOutcome.NotUpdated,  40,   "ベスト 40 秒")]
    public void WinCardBestTimeFollowsTheOutcome(BestTimeOutcome outcome, int? previousSeconds, string text)
        => Assert.Equal(text, WinCardTexts.BestTimeOf(new BestTimeResult(outcome, previousSeconds)));

    // カスタムは記録しないので、ベストタイムの行を出さない（Web 版の UI デザイン 2.4）
    [Fact]
    public void WinCardHasNoBestTimeLineForCustom()
        => Assert.Null(WinCardTexts.BestTimeOf(new BestTimeResult(BestTimeOutcome.NotEligible, null)));
}
