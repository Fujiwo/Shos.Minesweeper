namespace Shos.Minesweeper.GameLogic;

/// <summary>ベストタイムを記録した結果。PreviousSeconds は、記録する前のベストタイム（なければ null）。</summary>
public readonly record struct BestTimeResult(BestTimeOutcome Outcome, int? PreviousSeconds)
{
    public bool IsNewBest => Outcome is BestTimeOutcome.FirstRecord or BestTimeOutcome.Updated;
}
