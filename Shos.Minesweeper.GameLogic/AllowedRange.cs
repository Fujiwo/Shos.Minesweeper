namespace Shos.Minesweeper.GameLogic;

public readonly record struct AllowedRange(int Minimum, int Maximum)
{
    public bool Contains(int value) => Minimum <= value && value <= Maximum;
}
