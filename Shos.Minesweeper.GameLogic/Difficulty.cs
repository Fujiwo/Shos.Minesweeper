using System.Runtime.CompilerServices;

namespace Shos.Minesweeper.GameLogic;

public sealed record Difficulty
{
    public static Difficulty Beginner { get; } = new(DifficultyKind.Beginner, width: 9, height: 9, mineCount: 10);
    public static Difficulty Intermediate { get; } = new(DifficultyKind.Intermediate, width: 16, height: 16, mineCount: 40);
    public static Difficulty Expert { get; } = new(DifficultyKind.Expert, width: 30, height: 16, mineCount: 99);
    public static IReadOnlyList<Difficulty> Presets { get; } = [Beginner, Intermediate, Expert];

    public static AllowedRange WidthRange { get; } = new(5, 30);
    public static AllowedRange HeightRange { get; } = new(5, 24);

    // 最初に開いたマスとその周囲 8 マスには地雷を置かないので、その分を上限から除く
    const int SafeCellCount = 9;
    const int MinimumMineCount = 1;

    public static AllowedRange MineCountRange(int width, int height)
    {
        ThrowIfOutOfRange(width, WidthRange);
        ThrowIfOutOfRange(height, HeightRange);
        return new(MinimumMineCount, width * height - SafeCellCount);
    }

    public static CustomDifficultyValidation ValidateCustom(int? width, int? height, int? mineCount)
    {
        var isWidthValid = width is int w && WidthRange.Contains(w);
        var isHeightValid = height is int h && HeightRange.Contains(h);
        // 地雷数の上限は幅と高さで決まるので、どちらかが誤っているときは下限だけを確かめる
        var mineCountRange = isWidthValid && isHeightValid
                             ? MineCountRange(width!.Value, height!.Value)
                             : new AllowedRange(MinimumMineCount, int.MaxValue);
        var isMineCountValid = mineCount is int m && mineCountRange.Contains(m);
        return new(isWidthValid, isHeightValid, isMineCountValid);
    }

    public static Difficulty Custom(int width, int height, int mineCount)
    {
        ThrowIfOutOfRange(mineCount, MineCountRange(width, height));
        return new(DifficultyKind.Custom, width, height, mineCount);
    }

    public DifficultyKind Kind { get; }
    public int Width { get; }
    public int Height { get; }
    public int MineCount { get; }

    Difficulty(DifficultyKind kind, int width, int height, int mineCount)
    {
        Kind = kind;
        Width = width;
        Height = height;
        MineCount = mineCount;
    }

    static void ThrowIfOutOfRange(int value, AllowedRange range, [CallerArgumentExpression(nameof(value))] string? paramName = null)
    {
        if (!range.Contains(value))
            throw new ArgumentOutOfRangeException(paramName, value, $"{range.Minimum}〜{range.Maximum} の値にしてください。");
    }
}
