using Shos.Minesweeper.Display;
using Shos.Minesweeper.GameLogic;
using Shos.Minesweeper.Presentation;

namespace Shos.Minesweeper.Tests.Display;

public class BoardPlacementTests
{
    public enum Layout { TopBar, SideBar }

    static readonly Difficulty CustomMaximum = Difficulty.Custom(30, 24, 99);

    // UI デザイン 3.3 の表。R は縦と横を入れ替えること
    public static TheoryData<int, int, Layout, string, int, bool> UiDesignTable => new() {
        { 360,  640,  Layout.TopBar,  "Beginner",      38, false },
        { 360,  640,  Layout.TopBar,  "Intermediate",  21, false },
        { 360,  640,  Layout.TopBar,  "Expert",        20, true  },
        { 360,  640,  Layout.TopBar,  "CustomMaximum", 20, true  },
        { 390,  700,  Layout.TopBar,  "Beginner",      41, false },
        { 390,  700,  Layout.TopBar,  "Intermediate",  23, false },
        { 390,  700,  Layout.TopBar,  "Expert",        21, true  },
        { 390,  700,  Layout.TopBar,  "CustomMaximum", 20, true  },
        { 430,  800,  Layout.TopBar,  "Beginner",      46, false },
        { 430,  800,  Layout.TopBar,  "Intermediate",  26, false },
        { 430,  800,  Layout.TopBar,  "Expert",        24, true  },
        { 430,  800,  Layout.TopBar,  "CustomMaximum", 20, true  },
        { 640,  320,  Layout.SideBar, "Beginner",      34, false },
        { 640,  320,  Layout.SideBar, "Intermediate",  20, false },
        { 640,  320,  Layout.SideBar, "Expert",        20, false },
        { 640,  320,  Layout.SideBar, "CustomMaximum", 20, false },
        { 844,  350,  Layout.SideBar, "Beginner",      37, false },
        { 844,  350,  Layout.SideBar, "Intermediate",  21, false },
        { 844,  350,  Layout.SideBar, "Expert",        21, false },
        { 844,  350,  Layout.SideBar, "CustomMaximum", 20, false },
        { 932,  400,  Layout.SideBar, "Beginner",      42, false },
        { 932,  400,  Layout.SideBar, "Intermediate",  24, false },
        { 932,  400,  Layout.SideBar, "Expert",        24, false },
        { 932,  400,  Layout.SideBar, "CustomMaximum", 20, false },
        { 768,  950,  Layout.TopBar,  "Beginner",      48, false },
        { 768,  950,  Layout.TopBar,  "Intermediate",  47, false },
        { 768,  950,  Layout.TopBar,  "Expert",        29, true  },
        { 768,  950,  Layout.TopBar,  "CustomMaximum", 29, true  },
        { 1024, 1300, Layout.TopBar,  "Beginner",      48, false },
        { 1024, 1300, Layout.TopBar,  "Intermediate",  48, false },
        { 1024, 1300, Layout.TopBar,  "Expert",        41, true  },
        { 1024, 1300, Layout.TopBar,  "CustomMaximum", 41, true  },
        { 1024, 700,  Layout.TopBar,  "Beginner",      48, false },
        { 1024, 700,  Layout.TopBar,  "Intermediate",  39, false },
        { 1024, 700,  Layout.TopBar,  "Expert",        33, false },
        { 1024, 700,  Layout.TopBar,  "CustomMaximum", 26, false },
        { 1366, 950,  Layout.TopBar,  "Beginner",      48, false },
        { 1366, 950,  Layout.TopBar,  "Intermediate",  48, false },
        { 1366, 950,  Layout.TopBar,  "Expert",        45, false },
        { 1366, 950,  Layout.TopBar,  "CustomMaximum", 36, false },
        { 1280, 650,  Layout.TopBar,  "Beginner",      48, false },
        { 1280, 650,  Layout.TopBar,  "Intermediate",  36, false },
        { 1280, 650,  Layout.TopBar,  "Expert",        36, false },
        { 1280, 650,  Layout.TopBar,  "CustomMaximum", 24, false },
        { 1920, 950,  Layout.TopBar,  "Beginner",      48, false },
        { 1920, 950,  Layout.TopBar,  "Intermediate",  48, false },
        { 1920, 950,  Layout.TopBar,  "Expert",        48, false },
        { 1920, 950,  Layout.TopBar,  "CustomMaximum", 36, false },
    };

    [Theory]
    [MemberData(nameof(UiDesignTable))]
    public void CellSizeAndTransposeFollowTheUiDesignTable(
        int screenWidth, int screenHeight, Layout layout, string difficultyName, int cellSize, bool isTransposed)
    {
        var (areaWidth, areaHeight) = BoardAreaSizeOf(screenWidth, screenHeight, layout);

        var placement = BoardPlacement.Calculate(areaWidth, areaHeight, DifficultyNamed(difficultyName));

        Assert.Equal(cellSize, placement.CellSize);
        Assert.Equal(isTransposed, placement.IsTransposed);
    }

    [Fact]
    public void BoardIsNotTransposedWhenBothOrientationsGiveTheSameSize()
        => Assert.False(BoardPlacement.Calculate(300, 300, Difficulty.Intermediate).IsTransposed);

    [Fact]
    public void CellSizeIsAtMost48()
        => Assert.Equal(48, BoardPlacement.Calculate(2000, 2000, Difficulty.Beginner).CellSize);

    [Fact]
    public void CellSizeIsAtLeast20()
        => Assert.Equal(20, BoardPlacement.Calculate(100, 100, Difficulty.Expert).CellSize);

    [Fact]
    public void DisplayedRowsAndColumnsFollowTheBoardWhenNotTransposed()
    {
        var placement = BoardPlacement.Calculate(1280, 594, Difficulty.Expert);

        Assert.Equal(16, placement.RowCount);
        Assert.Equal(30, placement.ColumnCount);
    }

    [Fact]
    public void DisplayedRowsAndColumnsAreSwappedWhenTransposed()
    {
        var placement = TransposedExpert();

        Assert.Equal(30, placement.RowCount);
        Assert.Equal(16, placement.ColumnCount);
    }

    [Fact]
    public void PositionsAreTheSameWhenNotTransposed()
        => Assert.Equal(new CellPosition(3, 7), BoardPlacement.Calculate(1280, 594, Difficulty.Expert).ToBoard(new DisplayPosition(3, 7)));

    [Fact]
    public void RowAndColumnAreSwappedWhenTransposed()
        => Assert.Equal(new CellPosition(7, 3), TransposedExpert().ToBoard(new DisplayPosition(3, 7)));

    // 上級を 30 列×16 行、マス 36px で表示する: 16 × 36 + 3 × 2
    [Fact]
    public void BoardHeightIsTheDisplayedRowsAndTheFrame()
        => Assert.Equal(582, BoardPlacement.Calculate(1280, 594, Difficulty.Expert).BoardHeight);

    // 上級を 16 列×30 行、マス 21px で表示する: 30 × 21 + 3 × 2
    [Fact]
    public void BoardHeightUsesTheDisplayedRowsWhenTransposed()
        => Assert.Equal(636, TransposedExpert().BoardHeight);

    // 矢印キーの方向（表示の向き）を、カーソルが動く盤面の向きに変える（デスクトップ版・コンソール版のクラス設計書 3.8）
    [Theory]
    [InlineData(Direction.Up)]
    [InlineData(Direction.Down)]
    [InlineData(Direction.Left)]
    [InlineData(Direction.Right)]
    public void DirectionIsTheSameWhenNotTransposed(Direction direction)
        => Assert.Equal(direction, BoardPlacement.Calculate(1280, 594, Difficulty.Expert).ToBoard(direction));

    // 表示の (行, 列) は盤面の (列, 行) なので、表示で上（行 − 1）は盤面で左（列 − 1）、表示で右（列 + 1）は盤面で下（行 + 1）
    [Theory]
    [InlineData(Direction.Up,    Direction.Left)]
    [InlineData(Direction.Down,  Direction.Right)]
    [InlineData(Direction.Left,  Direction.Up)]
    [InlineData(Direction.Right, Direction.Down)]
    public void DirectionIsSwappedWhenTransposed(Direction displayed, Direction onBoard)
        => Assert.Equal(onBoard, TransposedExpert().ToBoard(displayed));

    // スマートフォンの縦画面（390×700）の上級。16 列×30 行で表示する
    static BoardPlacement TransposedExpert() => BoardPlacement.Calculate(382, 636, Difficulty.Expert);

    // UI デザイン 3.2 の式。ページの余白 4px×2、上バーは高さ 52px＋間 4px、横バーは幅 88px＋間 4px
    static (double Width, double Height) BoardAreaSizeOf(int screenWidth, int screenHeight, Layout layout)
        => layout == Layout.TopBar
           ? (screenWidth - 8, screenHeight - 8 - 52 - 4)
           : (screenWidth - 8 - 88 - 4, screenHeight - 8);

    static Difficulty DifficultyNamed(string name)
        => name switch {
            "Beginner"      => Difficulty.Beginner,
            "Intermediate"  => Difficulty.Intermediate,
            "Expert"        => Difficulty.Expert,
            "CustomMaximum" => CustomMaximum,
            _               => throw new ArgumentOutOfRangeException(nameof(name), name, null)
        };
}
