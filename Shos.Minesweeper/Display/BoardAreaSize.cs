namespace Shos.Minesweeper.Display;

/// <summary>盤面の領域の大きさ（px）。BoardArea が測り、GamePage が置き方を求めるのに使う。</summary>
public readonly record struct BoardAreaSize(double Width, double Height);
