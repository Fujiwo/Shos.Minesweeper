using Shos.Minesweeper.Display;
using Shos.Minesweeper.GameLogic;

namespace Shos.Minesweeper.Tests.Display;

public class DifficultyNamesTests
{
    [Theory]
    [InlineData(DifficultyKind.Beginner,     "初級")]
    [InlineData(DifficultyKind.Intermediate, "中級")]
    [InlineData(DifficultyKind.Expert,       "上級")]
    [InlineData(DifficultyKind.Custom,       "カスタム")]
    public void EachDifficultyHasItsJapaneseName(DifficultyKind kind, string name)
        => Assert.Equal(name, DifficultyNames.Of(kind));
}
