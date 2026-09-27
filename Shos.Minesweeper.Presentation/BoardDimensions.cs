namespace Shos.Minesweeper.Presentation;

/// <summary>
/// 盤面の寸法の決まり（論理的な px）。Web 版とデスクトップ版で同じ値を使う（Web 版の仕様書 5.2、デスクトップ版の仕様書 4.4）。
/// マスの大きさを領域に合わせる計算は、各版が持つ（Web 版は縦と横を入れ替えるかの判断と一体のため。
/// デスクトップ版・コンソール版のクラス設計書 3.6）。
/// </summary>
public static class BoardDimensions
{
    /// <summary>マスの大きさの下限。</summary>
    public const int MinCellSize = 20;

    /// <summary>マスの大きさの上限。</summary>
    public const int MaxCellSize = 48;

    /// <summary>盤面の枠の太さ。</summary>
    public const int FrameWidth = 3;
}
