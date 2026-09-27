using Shos.Minesweeper.GameLogic;
using Shos.Minesweeper.Presentation;

namespace Shos.Minesweeper.Desktop.ViewModels;

/// <summary>難易度ダイアログの、初級〜上級の 1 行（クラス設計書 4.5）。開いている間に値が変わらないので、変化を知らせない。</summary>
public sealed class DifficultyRowViewModel(Difficulty difficulty, int? bestSeconds, bool isCurrent)
{
    // 現在の難易度の読み上げの状態。Web 版は aria-current で、ブラウザーが言い方を決める
    const string CurrentStatus = "現在の難易度";

    public Difficulty Difficulty { get; } = difficulty;

    public string Name => DifficultyNames.Of(Difficulty.Kind);

    /// <summary>「9×9・地雷 10」。</summary>
    public string SizeText => DifficultyDialogTexts.SizeOf(Difficulty);

    /// <summary>「ベスト 23 秒」。記録がなければ「記録なし」。</summary>
    public string BestTimeText => DifficultyDialogTexts.BestTimeOf(bestSeconds);

    /// <summary>現在の難易度か（チェックの印）。</summary>
    public bool IsCurrent { get; } = isCurrent;

    /// <summary>行のボタンの読み上げの名前。Web 版のボタンと同じく、行の文をすべて読む。</summary>
    public string AccessibleName => $"{Name}、{SizeText}、{BestTimeText}";

    /// <summary>読み上げの状態。現在の難易度だけ「現在の難易度」。</summary>
    public string? ItemStatus => IsCurrent ? CurrentStatus : null;
}
