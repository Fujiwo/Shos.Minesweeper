using Shos.Minesweeper.GameLogic;

namespace Shos.Minesweeper.Presentation;

/// <summary>操作の結果から、鳴らす効果音を決める（仕様書 5.6）。どの UI でも同じ規則である。</summary>
public static class SoundEffectMapping
{
    /// <summary>鳴らす効果音。何も起きなかった操作では null。1 回の操作で鳴らすのは 1 つだけである。</summary>
    public static SoundEffect? EffectFor(MoveResult move)
        => (move.Status, move.Outcome) switch {
            // 勝敗が決まった操作では、開いた音は鳴らさず、勝ちか負けの音だけを鳴らす
            (GameStatus.Won, _)             => SoundEffect.Won,
            (GameStatus.Lost, _)            => SoundEffect.Lost,
            (_, MoveOutcome.Opened)         => move.OpenedPositions.Count == 1 ? SoundEffect.Open : SoundEffect.Chain,
            (_, MoveOutcome.FlagPlaced)     => SoundEffect.FlagPlaced,
            (_, MoveOutcome.FlagRemoved)    => SoundEffect.FlagRemoved,
            _                               => null
        };
}
