using Shos.Minesweeper.GameLogic;
using Shos.Minesweeper.Presentation;

namespace Shos.Minesweeper.Desktop.ViewModels;

/// <summary>
/// 勝利カードに出す文（クラス設計書 4.6。Web 版の UI デザイン 2.4）。見出しとボタンの文言は定数なので、XAML から WinCardTexts を指す。
/// </summary>
public sealed class WinCardViewModel(int seconds, BestTimeResult bestTime)
{
    /// <summary>「タイム 45 秒」。</summary>
    public string TimeText { get; } = WinCardTexts.TimeOf(seconds);

    /// <summary>ベストタイムの行。カスタムは記録しないので null で、行を出さない。</summary>
    public string? BestTimeText { get; } = WinCardTexts.BestTimeOf(bestTime);

    /// <summary>ベストタイムを更新したか、初めて記録したか（星のアイコンを出し、太字にする）。</summary>
    public bool IsNewBest { get; } = bestTime.IsNewBest;
}
