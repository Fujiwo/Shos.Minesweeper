using Shos.Minesweeper.GameLogic;

namespace Shos.Minesweeper.Presentation;

/// <summary>
/// ツールバーの文言（Web 版の UI デザイン 2.2、7 章、10.3）。Web 版とデスクトップ版で使う。
/// 旗モードの文言は、旗モードのある Web 版だけに置く（デスクトップ版・コンソール版のクラス設計書 3.5）。
/// </summary>
public static class ToolbarTexts
{
    public const string DifficultyToolTip = "難易度を変える";

    /// <summary>リセット ボタンの名前とツールチップ。</summary>
    public const string NewGame = "新しいゲーム";

    /// <summary>効果音 ボタンの名前。オンかオフかは、ボタンの状態として伝える。</summary>
    public const string SoundEffects = "効果音";

    /// <summary>難易度 ボタンの読み上げの名前（「難易度、初級」）。</summary>
    public static string DifficultyButtonNameOf(DifficultyKind kind) => $"難易度、{DifficultyNames.Of(kind)}";

    /// <summary>残り地雷数の読み上げの名前（「残り地雷 10」）。</summary>
    public static string RemainingMinesOf(int count) => $"残り地雷 {count}";

    /// <summary>経過時間の読み上げの名前（「経過時間 12 秒」）。</summary>
    public static string ElapsedTimeOf(int seconds) => $"経過時間 {seconds} 秒";

    public static string SoundEffectsToolTipOf(bool isEnabled) => isEnabled ? "効果音（オン）" : "効果音（オフ）";
}
