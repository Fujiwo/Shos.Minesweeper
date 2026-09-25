using Shos.Minesweeper.GameLogic;

namespace Shos.Minesweeper.Tests.GameLogic;

public class DifficultyTests
{
    [Fact]
    public void BeginnerIs9By9With10Mines()
        => AssertDifficulty(Difficulty.Beginner, DifficultyKind.Beginner, width: 9, height: 9, mineCount: 10);

    [Fact]
    public void IntermediateIs16By16With40Mines()
        => AssertDifficulty(Difficulty.Intermediate, DifficultyKind.Intermediate, width: 16, height: 16, mineCount: 40);

    [Fact]
    public void ExpertIs30By16With99Mines()
        => AssertDifficulty(Difficulty.Expert, DifficultyKind.Expert, width: 30, height: 16, mineCount: 99);

    [Fact]
    public void PresetsAreBeginnerIntermediateExpertInThisOrder()
        => Assert.Equal([Difficulty.Beginner, Difficulty.Intermediate, Difficulty.Expert], Difficulty.Presets);

    [Theory]
    [InlineData(4, false)]
    [InlineData(5, true)]
    [InlineData(30, true)]
    [InlineData(31, false)]
    public void CustomWidthMustBeFrom5To30(int width, bool isValid)
        => Assert.Equal(isValid, Difficulty.ValidateCustom(width, 9, 10).IsWidthValid);

    [Theory]
    [InlineData(4, false)]
    [InlineData(5, true)]
    [InlineData(24, true)]
    [InlineData(25, false)]
    public void CustomHeightMustBeFrom5To24(int height, bool isValid)
        => Assert.Equal(isValid, Difficulty.ValidateCustom(9, height, 10).IsHeightValid);

    [Theory]
    [InlineData(0, false)]
    [InlineData(1, true)]
    [InlineData(72, true)]    // 9 × 9 − 9
    [InlineData(73, false)]
    public void CustomMineCountMustBeFrom1ToCellCountMinus9(int mineCount, bool isValid)
        => Assert.Equal(isValid, Difficulty.ValidateCustom(9, 9, mineCount).IsMineCountValid);

    [Fact]
    public void CustomMineCountIsCheckedOnlyAgainstTheMinimumWhenWidthIsInvalid()
    {
        Assert.True(Difficulty.ValidateCustom(100, 9, 5000).IsMineCountValid);
        Assert.False(Difficulty.ValidateCustom(100, 9, 0).IsMineCountValid);
    }

    [Fact]
    public void CustomMineCountIsCheckedOnlyAgainstTheMinimumWhenHeightIsInvalid()
        => Assert.True(Difficulty.ValidateCustom(9, 100, 5000).IsMineCountValid);

    [Fact]
    public void MissingCustomValuesAreInvalid()
    {
        var validation = Difficulty.ValidateCustom(null, null, null);

        Assert.False(validation.IsWidthValid);
        Assert.False(validation.IsHeightValid);
        Assert.False(validation.IsMineCountValid);
    }

    [Fact]
    public void CustomValuesAreValidOnlyWhenAllAreValid()
    {
        Assert.True(Difficulty.ValidateCustom(9, 9, 10).IsValid);
        Assert.False(Difficulty.ValidateCustom(9, 9, 0).IsValid);
    }

    [Fact]
    public void MineCountRangeIsFrom1ToCellCountMinus9()
        => Assert.Equal(new AllowedRange(1, 247), Difficulty.MineCountRange(16, 16));

    [Theory]
    [InlineData(4, 9)]
    [InlineData(9, 25)]
    public void MineCountRangeRequiresValidWidthAndHeight(int width, int height)
        => Assert.Throws<ArgumentOutOfRangeException>(() => Difficulty.MineCountRange(width, height));

    [Fact]
    public void CustomDifficultyHasTheGivenValues()
        => AssertDifficulty(Difficulty.Custom(20, 10, 30), DifficultyKind.Custom, width: 20, height: 10, mineCount: 30);

    [Fact]
    public void CustomDifficultyIsNotEqualToThePresetOfTheSameSize()
        => Assert.NotEqual(Difficulty.Beginner, Difficulty.Custom(9, 9, 10));

    [Theory]
    [InlineData(4, 9, 10)]
    [InlineData(9, 25, 10)]
    [InlineData(9, 9, 73)]
    public void CustomDifficultyRequiresValidValues(int width, int height, int mineCount)
        => Assert.Throws<ArgumentOutOfRangeException>(() => Difficulty.Custom(width, height, mineCount));

    static void AssertDifficulty(Difficulty difficulty, DifficultyKind kind, int width, int height, int mineCount)
    {
        Assert.Equal(kind, difficulty.Kind);
        Assert.Equal(width, difficulty.Width);
        Assert.Equal(height, difficulty.Height);
        Assert.Equal(mineCount, difficulty.MineCount);
    }
}
