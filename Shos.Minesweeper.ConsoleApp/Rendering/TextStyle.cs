namespace Shos.Minesweeper.ConsoleApp.Rendering;

/// <summary>文字の色（端末の 16 色）と反転表示。色が null なら、端末の既定の色（仕様書 5.6）。</summary>
public readonly record struct TextStyle(ConsoleColor? Foreground = null, ConsoleColor? Background = null, bool IsReversed = false);
