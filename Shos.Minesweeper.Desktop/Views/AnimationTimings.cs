namespace Shos.Minesweeper.Desktop.Views;

/// <summary>
/// 演出の長さと、遅れの最大（クラス設計書 4.11）。Web 版の Components/BoardView.razor.css の animation と同じ値にする。
/// 遅れはマスごとに「CellAnimation.DelayRatio × 最大の遅れ」になり、演出を code-behind で組み立てるので、XAML でなくここに置く。
/// 値が Web 版と同じことは、WebStyleConsistencyTests で確かめる。
/// </summary>
public static class CellAnimationTimings
{
    /// <summary>旗が広がる（Web 版の flag-planted）。</summary>
    public static readonly TimeSpan FlagPlanted = TimeSpan.FromMilliseconds(150);

    /// <summary>タイルが消えて数字が出る（Web 版の reveal）。</summary>
    public static readonly TimeSpan Reveal = TimeSpan.FromMilliseconds(100);
    public static readonly TimeSpan RevealMaxDelay = TimeSpan.FromMilliseconds(150);

    /// <summary>踏んだ地雷の爆発の形が縮む（Web 版の explode）。</summary>
    public static readonly TimeSpan Explode = TimeSpan.FromMilliseconds(150);

    /// <summary>地雷と誤った旗の × が現れる（Web 版の mine-appear、wrong-flag-appear）。</summary>
    public static readonly TimeSpan Appear = TimeSpan.FromMilliseconds(150);
    public static readonly TimeSpan AppearMaxDelay = TimeSpan.FromMilliseconds(400);

    /// <summary>勝ったときに旗が跳ねる（Web 版の flag-bounce）。</summary>
    public static readonly TimeSpan FlagBounce = TimeSpan.FromMilliseconds(300);
    public static readonly TimeSpan FlagBounceMaxDelay = TimeSpan.FromMilliseconds(300);

    /// <summary>勝利カードが現れる（Web 版の WinCard.razor.css の fade-in）。</summary>
    public static readonly TimeSpan WinCardFadeIn = TimeSpan.FromMilliseconds(150);
}
