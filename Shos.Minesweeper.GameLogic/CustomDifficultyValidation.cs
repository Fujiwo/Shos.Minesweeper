namespace Shos.Minesweeper.GameLogic;

public readonly record struct CustomDifficultyValidation(bool IsWidthValid, bool IsHeightValid, bool IsMineCountValid)
{
    public bool IsValid => IsWidthValid && IsHeightValid && IsMineCountValid;
}
