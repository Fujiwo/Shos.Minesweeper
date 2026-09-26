namespace Shos.Minesweeper.Presentation;

/// <summary>効果音の種類（仕様書 5.6）。名前は、鳴らす出来事（MoveOutcome、GameStatus）にそろえた。</summary>
public enum SoundEffect
{
    Open,
    Chain,
    FlagPlaced,
    FlagRemoved,
    Lost,
    Won
}
