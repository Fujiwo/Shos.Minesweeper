using Shos.Minesweeper.GameLogic;

namespace Shos.Minesweeper.Display;

/// <summary>読み上げ用の領域で知らせる文（UI デザイン 6.4）。</summary>
public static class Announcements
{
    public const string Lost = "ゲームオーバー。地雷を開きました。";

    /// <summary>
    /// 新しいゲームの文。盤面の大きさは、難易度ダイアログと同じ「幅×高さ」で書く。
    /// 縦と横を入れ替えて表示していても、盤面の名前（行数と列数）と食い違わないようにするためである（クラス設計書 9.1 の決定 6）。
    /// </summary>
    public static string NewGame(Difficulty difficulty)
        => $"新しいゲーム、{DifficultyNames.Of(difficulty.Kind)}、{difficulty.Width}×{difficulty.Height}、地雷 {difficulty.MineCount}。";

    public static string Won(int seconds, BestTimeResult bestTime)
        => $"クリア。{seconds} 秒。{BestTimeTextOf(bestTime)}";

    // 勝利カードのベストタイムの行（UI デザイン 2.4）に合わせて出し分ける
    static string BestTimeTextOf(BestTimeResult bestTime)
        => bestTime.Outcome switch {
            BestTimeOutcome.Updated     => "ベストタイムを更新しました。",
            BestTimeOutcome.FirstRecord => "ベストタイムを記録しました。",
            BestTimeOutcome.NotUpdated  => $"ベスト {bestTime.PreviousSeconds} 秒。",
            BestTimeOutcome.NotEligible => "",   // カスタムは記録しないので、ベストタイムの行を出さない
            _                           => throw new ArgumentOutOfRangeException(nameof(bestTime), bestTime.Outcome, null)
        };
}
