using Avalonia;
using Shos.Minesweeper.GameLogic;
using Shos.Minesweeper.Presentation;

namespace Shos.Minesweeper.Desktop.Sizing;

/// <summary>
/// ウィンドウとマスの大きさ（仕様書 4.4、UI デザイン 2.2）。単位は、Windows の拡大率を掛ける前の論理的な px。
/// 余白やツールバーの高さは、XAML も x:Static でこの型の値を使う（計算と見た目で、同じ数字を 2 か所に書かない。クラス設計書 4.8）。
/// </summary>
public static class WindowSizing
{
    /// <summary>起動したときと、難易度を変えたときのマスの大きさ。上級を 1366×768 の画面に収めるための値（UI デザイン 4 章の決定 1）。</summary>
    public const int DefaultCellSize = 32;

    public const double ContentMargin = 16;
    public const double ToolbarHeight = 52;
    public const double ToolbarGap = 4;
    public const double MinToolbarWidth = 368;
    public const double MaxToolbarWidth = 480;

    /// <summary>ツールバーと盤面のまとまりの周りの余白（XAML の Margin に使う）。</summary>
    public static Thickness ContentPadding { get; } = new(ContentMargin);

    /// <summary>ウィンドウの中身の最小の大きさ。ツールバーが 1 本に収まり、初級の盤面が下限のマスで収まる。</summary>
    public static Size MinContentSize { get; } = new(400, 320);

    /// <summary>既定のマスの大きさで、盤面とツールバーが収まるウィンドウの中身の大きさ。</summary>
    public static Size ContentSizeOf(Difficulty difficulty)
    {
        var boardWidth = BoardLengthOf(difficulty.Width);
        var boardHeight = BoardLengthOf(difficulty.Height);
        return new(Math.Max(boardWidth, MinToolbarWidth) + ContentMargin * 2,
                   boardHeight + ToolbarHeight + ToolbarGap + ContentMargin * 2);
    }

    /// <summary>盤面の領域（枠を含む）に収まる最大の整数のマスの大きさ（20〜48）。</summary>
    public static int CellSizeToFit(Size boardArea, Difficulty difficulty)
    {
        var frame = BoardDimensions.FrameWidth * 2;
        var fitting = Math.Min((boardArea.Width - frame) / difficulty.Width, (boardArea.Height - frame) / difficulty.Height);
        // マスの境目の線がにじまないように、整数の px にする（Web 版と同じ）
        return Math.Clamp((int)Math.Floor(fitting), BoardDimensions.MinCellSize, BoardDimensions.MaxCellSize);
    }

    /// <summary>
    /// ウィンドウ（外枠を含む）を、画面の作業領域に収める。大きすぎれば縮め、はみ出すなら左上を動かす（UI デザイン 2.2）。
    /// どちらも同じ単位（物理的な px）で渡す。
    /// </summary>
    public static PixelRect KeepWithin(PixelRect window, PixelRect workArea)
    {
        var width = Math.Min(window.Width, workArea.Width);
        var height = Math.Min(window.Height, workArea.Height);
        var x = Math.Clamp(window.X, workArea.X, workArea.Right - width);
        var y = Math.Clamp(window.Y, workArea.Y, workArea.Bottom - height);
        return new(x, y, width, height);
    }

    static double BoardLengthOf(int cellCount) => cellCount * DefaultCellSize + BoardDimensions.FrameWidth * 2;
}
