using Avalonia;
using Shos.Minesweeper.Desktop.Sizing;
using Shos.Minesweeper.GameLogic;

namespace Shos.Minesweeper.Desktop.Tests.Sizing;

/// <summary>ウィンドウとマスの大きさ（仕様書 4.4、UI デザイン 2.2）。単位は論理的な px。</summary>
public class WindowSizingTests
{
    // UI デザイン 2.2 の例。中身の幅は max(盤面の幅, 368) + 32、高さは盤面 + 52 + 4 + 32。盤面は マス × 32 + 枠 3 × 2
    [Theory]
    [InlineData("Beginner",     400, 382)]
    [InlineData("Intermediate", 550, 606)]
    [InlineData("Expert",       998, 606)]
    public void ContentSizeFitsTheBoardWithTheDefaultCellSize(string difficultyName, double width, double height)
        => Assert.Equal(new Size(width, height), WindowSizing.ContentSizeOf(DifficultyNamed(difficultyName)));

    // 初級の盤面の領域が 294 × 294 なら、(294 − 6) ÷ 9 = 32
    [Fact]
    public void CellSizeFillsTheBoardArea()
        => Assert.Equal(32, WindowSizing.CellSizeToFit(new Size(294, 294), Difficulty.Beginner));

    [Fact]
    public void CellSizeUsesTheNarrowerDirection()
        => Assert.Equal(25, WindowSizing.CellSizeToFit(new Size(800, 406), Difficulty.Expert));   // 縦 (406 − 6) ÷ 16 = 25

    [Fact]
    public void CellSizeIsRoundedDownToAWholePixel()
        => Assert.Equal(32, WindowSizing.CellSizeToFit(new Size(302, 302), Difficulty.Beginner));   // 296 ÷ 9 = 32.9

    [Fact]
    public void CellSizeIsAtMost48()
        => Assert.Equal(48, WindowSizing.CellSizeToFit(new Size(2000, 2000), Difficulty.Beginner));

    [Fact]
    public void CellSizeIsAtLeast20()
        => Assert.Equal(20, WindowSizing.CellSizeToFit(new Size(300, 200), Difficulty.Expert));

    static readonly PixelRect WorkArea = new(0, 0, 1366, 728);

    [Fact]
    public void WindowInsideTheWorkAreaIsKept()
    {
        var window = new PixelRect(100, 50, 400, 420);

        Assert.Equal(window, WindowSizing.KeepWithin(window, WorkArea));
    }

    // 大きくなってはみ出すときは、左上を動かして収める（UI デザイン 2.2）
    [Fact]
    public void WindowSpillingOverIsMovedBack()
        => Assert.Equal(new PixelRect(366, 88, 1000, 640), WindowSizing.KeepWithin(new PixelRect(600, 200, 1000, 640), WorkArea));

    [Fact]
    public void WindowLargerThanTheWorkAreaIsShrunk()
        => Assert.Equal(new PixelRect(10, 0, 1000, 728), WindowSizing.KeepWithin(new PixelRect(10, 10, 1000, 900), WorkArea));

    [Fact]
    public void WindowAboveTheWorkAreaIsMovedDown()
        => Assert.Equal(new PixelRect(0, 0, 400, 420), WindowSizing.KeepWithin(new PixelRect(-50, -20, 400, 420), WorkArea));

    // 勝利カードの幅は、盤面の領域の幅 − 16px と 360px の小さいほう（Web 版の UI デザイン 10.5）
    [Theory]
    [InlineData(500, 360)]
    [InlineData(368, 352)]
    public void WinCardIsNarrowerThanTheArea(double areaWidth, double expected)
        => Assert.Equal(expected, WindowSizing.WinCardWidthOf(areaWidth));

    // 盤面の下に余白があれば、カードは盤面の下端の 8px 下に置く（盤面は領域の縦の中央にある）
    [Fact]
    public void WinCardGoesBelowTheBoardWhenThereIsRoom()
        => Assert.Equal(508, WindowSizing.WinCardTopOf(areaHeight: 700, boardHeight: 300, cardHeight: 150));

    // 余白がなければ、カードは領域の下端の 8px 上に寄り、盤面の下の段に重なる（デスクトップ版の UI デザイン 2.6）
    [Fact]
    public void WinCardOverlapsTheBoardWhenThereIsNoRoom()
        => Assert.Equal(142, WindowSizing.WinCardTopOf(areaHeight: 300, boardHeight: 294, cardHeight: 150));

    [Fact]
    public void WinCardDoesNotGoAboveTheArea()
        => Assert.Equal(0, WindowSizing.WinCardTopOf(areaHeight: 100, boardHeight: 100, cardHeight: 150));

    static Difficulty DifficultyNamed(string name)
        => name switch {
            "Beginner"     => Difficulty.Beginner,
            "Intermediate" => Difficulty.Intermediate,
            _              => Difficulty.Expert
        };
}
