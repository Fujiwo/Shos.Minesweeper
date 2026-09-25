using Shos.Minesweeper.GameLogic;

namespace Shos.Minesweeper.Presentation.Tests;

/// <summary>読み上げ用の領域で知らせる文（UI デザイン 6.4、クラス設計書 9.1 の決定 6）。</summary>
public class AnnouncementsTests
{
    [Fact]
    public void NewGameTellsTheDifficultySizeAndMines()
        => Assert.Equal("新しいゲーム、初級、9×9、地雷 10。", Announcements.NewGame(Difficulty.Beginner));

    [Fact]
    public void NewGameTellsTheSizeAsWidthByHeight()
        => Assert.Equal("新しいゲーム、カスタム、20×10、地雷 30。", Announcements.NewGame(Difficulty.Custom(20, 10, 30)));

    [Theory]
    [InlineData(BestTimeOutcome.Updated,     52,   "クリア。45 秒。ベストタイムを更新しました。")]
    [InlineData(BestTimeOutcome.FirstRecord, null, "クリア。45 秒。ベストタイムを記録しました。")]
    [InlineData(BestTimeOutcome.NotUpdated,  30,   "クリア。45 秒。ベスト 30 秒。")]
    [InlineData(BestTimeOutcome.NotEligible, null, "クリア。45 秒。")]
    public void WonTellsTheTimeAndTheBestTime(BestTimeOutcome outcome, int? previousSeconds, string text)
        => Assert.Equal(text, Announcements.Won(45, new BestTimeResult(outcome, previousSeconds)));

    [Fact]
    public void LostTellsThatAMineWasOpened()
        => Assert.Equal("ゲームオーバー。地雷を開きました。", Announcements.Lost);
}
