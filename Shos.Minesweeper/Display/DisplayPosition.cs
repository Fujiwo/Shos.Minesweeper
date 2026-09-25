namespace Shos.Minesweeper.Display;

/// <summary>表示している向きでのマスの位置。盤面の位置（CellPosition）とは別の型にして、取り違えを防ぐ。</summary>
public readonly record struct DisplayPosition(int Row, int Column);
