using Shos.Minesweeper.GameLogic;

namespace Shos.Minesweeper.Presentation.Tests;

/// <summary>カスタムの入力の文言（Web 版の UI デザイン 2.3、7 章）。Web 版、デスクトップ版、コンソール版で使う。</summary>
public class CustomDifficultyTextsTests
{
    [Fact]
    public void RangeTellsTheMinimumAndMaximum()
        => Assert.Equal("5〜30", CustomDifficultyTexts.RangeOf(Difficulty.WidthRange));

    // 9×9 の地雷数の上限は 81 − 9 = 72
    [Fact]
    public void MineCountRangeFollowsTheWidthAndHeight()
        => Assert.Equal("1〜72", CustomDifficultyTexts.MineCountRangeOf(9, 9));

    [Theory]
    [InlineData(null, 9)]
    [InlineData(4,    9)]
    [InlineData(9,    25)]
    public void MineCountRangeIsAFormulaWhenTheWidthOrHeightIsInvalid(int? width, int? height)
        => Assert.Equal("1〜（幅×高さ − 9）", CustomDifficultyTexts.MineCountRangeOf(width, height));

    [Fact]
    public void InvalidValueTellsTheRange()
        => Assert.Equal("5〜30 の整数を入力してください", CustomDifficultyTexts.InvalidValueOf("5〜30"));
}
