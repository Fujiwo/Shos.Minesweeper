using Shos.Minesweeper.GameLogic;

namespace Shos.Minesweeper.Presentation;

/// <summary>
/// 難易度ダイアログの文言（Web 版の UI デザイン 2.3、7 章）。Web 版とデスクトップ版で使う。
/// カスタムの入力欄の文言は、コンソール版も使うので CustomDifficultyTexts にある。
/// </summary>
public static class DifficultyDialogTexts
{
    public const string Title = "難易度";
    public const string Close = "閉じる";
    public const string StartCustom = "カスタムで始める";

    /// <summary>難易度の行の大きさ（「9×9・地雷 10」）。</summary>
    public static string SizeOf(Difficulty difficulty) => $"{difficulty.Width}×{difficulty.Height}・地雷 {difficulty.MineCount}";

    /// <summary>難易度の行のベストタイム（「ベスト 23 秒」）。記録がなければ「記録なし」。</summary>
    public static string BestTimeOf(int? seconds) => seconds is { } value ? $"ベスト {value} 秒" : "記録なし";
}
