namespace Shos.Minesweeper.Display;

/// <summary>
/// 1 つのマスの演出。DelayRatio は開始の遅れの比（0〜1）で、CSS が演出ごとの最大の遅れを掛ける。
/// 時間の値を CSS だけに置くため、C# は比だけを持つ（アーキテクチャー設計書 14 章の決定 5）。
/// </summary>
public readonly record struct CellAnimation(CellAnimationKind Kind, double DelayRatio);
