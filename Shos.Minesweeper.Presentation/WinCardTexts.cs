using Shos.Minesweeper.GameLogic;

namespace Shos.Minesweeper.Presentation;

/// <summary>勝利カードの文言（Web 版の UI デザイン 2.4、7 章）。Web 版とデスクトップ版で使う。</summary>
public static class WinCardTexts
{
    public const string Title = "クリア！";
    public const string PlayAgain = "もう一度";
    public const string Close = "閉じる";

    /// <summary>「タイム 45 秒」。</summary>
    public static string TimeOf(int seconds) => $"タイム {seconds} 秒";

    /// <summary>ベストタイムの行。カスタムは記録しないので、行を出さない（null）。</summary>
    public static string? BestTimeOf(BestTimeResult bestTime)
        => bestTime.Outcome switch {
            BestTimeOutcome.Updated     => $"ベストタイム更新！（これまで {bestTime.PreviousSeconds} 秒）",
            BestTimeOutcome.FirstRecord => "ベストタイムを記録しました",
            BestTimeOutcome.NotUpdated  => $"ベスト {bestTime.PreviousSeconds} 秒",
            BestTimeOutcome.NotEligible => null,
            _                           => throw new ArgumentOutOfRangeException(nameof(bestTime), bestTime.Outcome, null)
        };
}
