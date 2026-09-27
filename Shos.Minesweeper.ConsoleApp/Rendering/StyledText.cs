namespace Shos.Minesweeper.ConsoleApp.Rendering;

/// <summary>同じ色の文字の並び。</summary>
public readonly record struct StyledText(string Text, TextStyle Style = default);
