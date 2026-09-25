namespace Shos.Minesweeper.Input;

/// <summary>
/// ポインターのイベントのうち、押し方の判定に要る値。
/// PointerType（"mouse"・"touch"・"pen"）と Button（0 が主ボタン、2 が右ボタン）は DOM の値のまま持つ。
/// </summary>
public readonly record struct PointerInput(long PointerId, string PointerType, long Button, double ClientX, double ClientY);
