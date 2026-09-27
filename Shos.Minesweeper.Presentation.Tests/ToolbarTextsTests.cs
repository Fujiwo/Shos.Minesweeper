using Shos.Minesweeper.GameLogic;

namespace Shos.Minesweeper.Presentation.Tests;

/// <summary>ツールバーの文言のうち、値を埋め込むもの（Web 版の UI デザイン 2.2、7 章、10.3）。定数の文言は、Web 版の bUnit のテストが描かれた HTML で確かめる。</summary>
public class ToolbarTextsTests
{
    [Fact]
    public void DifficultyButtonNameIncludesTheDifficulty()
        => Assert.Equal("難易度、上級", ToolbarTexts.DifficultyButtonNameOf(DifficultyKind.Expert));

    [Fact]
    public void RemainingMinesNameIncludesTheCount()
        => Assert.Equal("残り地雷 -3", ToolbarTexts.RemainingMinesOf(-3));

    [Fact]
    public void ElapsedTimeNameIncludesTheSeconds()
        => Assert.Equal("経過時間 12 秒", ToolbarTexts.ElapsedTimeOf(12));

    [Theory]
    [InlineData(true,  "効果音（オン）")]
    [InlineData(false, "効果音（オフ）")]
    public void SoundEffectsToolTipTellsWhetherItIsOn(bool isEnabled, string toolTip)
        => Assert.Equal(toolTip, ToolbarTexts.SoundEffectsToolTipOf(isEnabled));
}
