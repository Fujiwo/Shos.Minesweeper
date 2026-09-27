using Shos.Minesweeper.GameLogic;

namespace Shos.Minesweeper.Presentation;

/// <summary>
/// カスタムの入力の文言（欄の名前、範囲、誤り。Web 版の UI デザイン 2.3、7 章）。Web 版、デスクトップ版、コンソール版で使う。
/// </summary>
public static class CustomDifficultyTexts
{
    public const string Width = "幅";
    public const string Height = "高さ";
    public const string MineCount = "地雷数";

    // 地雷数の上限は幅と高さで決まるので、幅か高さが誤っているときは式で示す。
    // 「×」は幅があいまいな文字だが、コンソール版は幅と高さを先に確かめるので、この文を出すことはない
    const string UnknownMineCountRange = "1〜（幅×高さ − 9）";

    /// <summary>入力できる範囲（「5〜30」）。</summary>
    public static string RangeOf(AllowedRange range) => $"{range.Minimum}〜{range.Maximum}";

    /// <summary>地雷数の範囲。幅と高さが正しければ数で（「1〜72」）、そうでなければ式で示す。</summary>
    public static string MineCountRangeOf(int? width, int? height)
        => Difficulty.FindMineCountRange(width, height) is { } range ? RangeOf(range) : UnknownMineCountRange;

    /// <summary>範囲の外や整数でないときの誤りの文（「5〜30 の整数を入力してください」）。</summary>
    public static string InvalidValueOf(string rangeText) => $"{rangeText} の整数を入力してください";
}
