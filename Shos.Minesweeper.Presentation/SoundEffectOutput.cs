namespace Shos.Minesweeper.Presentation;

/// <summary>
/// 音の出口。効果音を 1 つ受け取って鳴らす。中身は各アプリが渡し、鳴らす実装のないアプリは渡さない（クラス設計書 12.4）。
/// 呼ばれたらすぐに返す（鳴り終わるのを待たない）。
/// </summary>
public delegate void SoundEffectOutput(SoundEffect effect);
