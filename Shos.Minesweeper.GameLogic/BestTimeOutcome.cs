namespace Shos.Minesweeper.GameLogic;

public enum BestTimeOutcome
{
    NotEligible,    // カスタム。記録しない（仕様書 3.8）
    FirstRecord,
    Updated,
    NotUpdated      // 同じ値を含む
}
