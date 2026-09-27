namespace Shos.Minesweeper.Presentation;

/// <summary>
/// 1 つのマスの演出。DelayRatio は開始の遅れの比（0〜1）で、各アプリが演出ごとの最大の遅れを掛ける
/// （Web 版は CSS、デスクトップ版は CellAnimationTimings）。時間の値は各アプリの見た目の側だけに置き、
/// ここでは比だけを持つ（Web 版のアーキテクチャー設計書 14 章の決定 5）。
/// </summary>
public readonly record struct CellAnimation(CellAnimationKind Kind, double DelayRatio);
