using Shos.Minesweeper.GameLogic;

namespace Shos.Minesweeper.Presentation.Tests;

/// <summary>難易度ダイアログの文言のうち、値を埋め込むもの（Web 版の UI デザイン 2.3、7 章）。</summary>
public class DifficultyDialogTextsTests
{
    [Fact]
    public void DifficultySizeTellsTheWidthHeightAndMines()
        => Assert.Equal("9×9・地雷 10", DifficultyDialogTexts.SizeOf(Difficulty.Beginner));

    [Fact]
    public void BestTimeTellsTheSeconds()
        => Assert.Equal("ベスト 23 秒", DifficultyDialogTexts.BestTimeOf(23));

    [Fact]
    public void BestTimeWithoutRecordSaysNoRecord()
        => Assert.Equal("記録なし", DifficultyDialogTexts.BestTimeOf(null));
}
