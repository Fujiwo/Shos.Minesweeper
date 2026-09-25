namespace Shos.Minesweeper.GameLogic;

/// <summary>初級〜上級のベストタイムと、その更新の規則（仕様書 3.8）。</summary>
public sealed class BestTimes
{
    readonly Dictionary<DifficultyKind, int> secondsByKind;

    public BestTimes() : this(new Dictionary<DifficultyKind, int>())
    {}

    public BestTimes(IReadOnlyDictionary<DifficultyKind, int> secondsByKind)
    {
        foreach (var (kind, seconds) in secondsByKind) {
            ThrowIfNotEligible(kind);
            ThrowIfOutOfRange(seconds);
        }
        this.secondsByKind = new(secondsByKind);
    }

    public int? SecondsOf(DifficultyKind kind) => secondsByKind.TryGetValue(kind, out var seconds) ? seconds : null;

    public BestTimeResult Record(DifficultyKind kind, int seconds)
    {
        ThrowIfOutOfRange(seconds);
        if (!IsEligible(kind))
            return new(BestTimeOutcome.NotEligible, null);

        var previous = SecondsOf(kind);
        if (previous is int best && best <= seconds)
            return new(BestTimeOutcome.NotUpdated, previous);

        secondsByKind[kind] = seconds;
        return new(previous is null ? BestTimeOutcome.FirstRecord : BestTimeOutcome.Updated, previous);
    }

    static bool IsEligible(DifficultyKind kind) => kind != DifficultyKind.Custom;

    static void ThrowIfNotEligible(DifficultyKind kind)
    {
        if (!IsEligible(kind))
            throw new ArgumentOutOfRangeException(nameof(kind), kind, "カスタムのベストタイムは記録しません。");
    }

    static void ThrowIfOutOfRange(int seconds)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(seconds);
        ArgumentOutOfRangeException.ThrowIfGreaterThan(seconds, Game.MaxElapsedSeconds);
    }
}
