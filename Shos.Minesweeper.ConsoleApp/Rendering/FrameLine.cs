namespace Shos.Minesweeper.ConsoleApp.Rendering;

/// <summary>画面の 1 行。色の付いた文字の並びを、左から順に持つ。</summary>
public sealed class FrameLine
{
    public FrameLine(params IEnumerable<StyledText> parts)
    {
        Parts = [.. parts];
        Text = string.Concat(Parts.Select(part => part.Text));
    }

    /// <summary>1 つの色の文字だけの行。</summary>
    public static FrameLine Of(string text, TextStyle style = default) => new(new StyledText(text, style));

    public IReadOnlyList<StyledText> Parts { get; }

    /// <summary>色を除いた文字。テストで UI デザインの図と比べるのに使う。</summary>
    public string Text { get; }
}
