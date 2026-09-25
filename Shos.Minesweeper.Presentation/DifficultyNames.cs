using Shos.Minesweeper.GameLogic;

namespace Shos.Minesweeper.Presentation;

/// <summary>難易度の表示名。ツールバー、難易度ダイアログ、読み上げで使う（UI デザイン 7 章）。</summary>
public static class DifficultyNames
{
    public static string Of(DifficultyKind kind)
        => kind switch {
            DifficultyKind.Beginner     => "初級",
            DifficultyKind.Intermediate => "中級",
            DifficultyKind.Expert       => "上級",
            DifficultyKind.Custom       => "カスタム",
            _                           => throw new ArgumentOutOfRangeException(nameof(kind), kind, null)
        };
}
