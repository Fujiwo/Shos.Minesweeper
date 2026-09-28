using Microsoft.Extensions.Time.Testing;
using Shos.Minesweeper.ConsoleApp.Rendering;
using Shos.Minesweeper.ConsoleApp.Screens;
using Shos.Minesweeper.GameLogic;
using Shos.Minesweeper.TestSupport;

namespace Shos.Minesweeper.ConsoleApp.Tests.Screens;

/// <summary>ゲームの画面（仕様書 5.2〜5.4、UI デザイン 3.1〜3.6）。</summary>
public sealed class GameScreenTests : IDisposable
{
    const int StatusRow = 13;   // 上の行、空行、上の枠、9 行、下の枠の次

    readonly FakeTimeProvider time = new();
    readonly string folder = Path.Combine(Path.GetTempPath(), "Shos.Minesweeper.Tests", Guid.NewGuid().ToString("N"));
    readonly GameScreen screen;

    public GameScreenTests()
        => screen = new GameScreen(time, new BestTimesFile(BestTimesPath), TestGames.MineChooserOf(TestGames.BeginnerWallPicture));

    string BestTimesPath => Path.Combine(folder, "best-times.json");

    public void Dispose()
    {
        if (Directory.Exists(folder))
            Directory.Delete(folder, recursive: true);
    }

    [Fact]
    public void FirstScreenOfBeginnerMatchesTheUiDesign()
        => Assert.Equal([
            "初級 9x9   残り地雷  10   経過時間   0 秒",
            "",
            "+-------------------+",
            "|[#]# # # # # # # # |",
            "| # # # # # # # # # |",
            "| # # # # # # # # # |",
            "| # # # # # # # # # |",
            "| # # # # # # # # # |",
            "| # # # # # # # # # |",
            "| # # # # # # # # # |",
            "| # # # # # # # # # |",
            "| # # # # # # # # # |",
            "+-------------------+",
            "マスを開くと始まります。",
            GameScreen.KeyGuide,
        ], TextsOf(screen.Render()));

    [Fact]
    public void KeyGuideMatchesTheUiDesign()
        => Assert.Equal("矢印/HJKL 移動  Space 開く  F 旗  N 新しいゲーム  D 難易度  ? ヘルプ  Q 終了", GameScreen.KeyGuide);

    // 盤面の行数 + 6 行（上の行、空行、上下の枠、状態の行、キーの案内の行。UI デザイン 3.2）
    [Fact]
    public void ScreenHasTheBoardRowsAndSixLines()
        => Assert.Equal(9 + 6, screen.Render().Lines.Count);

    [Fact]
    public void ArrowKeyMovesTheCursor()
    {
        screen.HandleKey(Keys.Of(ConsoleKey.RightArrow));
        screen.HandleKey(Keys.Of(ConsoleKey.J));

        Assert.Equal("| #[#]# # # # # # # |", LineOf(4));
    }

    [Fact]
    public void SpaceOpensTheCellAndStartsTheGame()
    {
        screen.HandleKey(Keys.Of(ConsoleKey.Spacebar));

        Assert.Equal("|[ ]    2 # # # # # |", LineOf(3));
        Assert.Equal("", LineOf(StatusRow));
    }

    [Fact]
    public void FPlacesAFlagAndDecreasesTheRemainingMines()
    {
        screen.HandleKey(Keys.Of(ConsoleKey.F));

        Assert.Equal("|[F]# # # # # # # # |", LineOf(3));
        Assert.StartsWith("初級 9x9   残り地雷   9", LineOf(0));
    }

    [Fact]
    public void ElapsedTimeIsShownOnTheTopLine()
    {
        screen.HandleKey(Keys.Of(ConsoleKey.Spacebar));

        time.Advance(TimeSpan.FromSeconds(37));

        Assert.Equal("初級 9x9   残り地雷  10   経過時間  37 秒", LineOf(0));
    }

    [Fact]
    public void WinningShowsTheWinInGreenAndSavesTheBestTime()
    {
        WinAfter(TimeSpan.FromSeconds(45));

        var status = screen.Render().Lines[StatusRow];
        Assert.Equal("クリア。45 秒。ベストタイムを記録しました。N で新しいゲーム。", status.Text);
        Assert.All(status.Parts, part => Assert.Equal(ConsoleColor.Green, part.Style.Foreground));
        Assert.Equal(45, new BestTimesFile(BestTimesPath).Load().SecondsOf(DifficultyKind.Beginner));
    }

    [Fact]
    public void LosingShowsGameOverInRed()
    {
        Lose();

        var status = screen.Render().Lines[StatusRow];
        Assert.Equal("ゲームオーバー。地雷を開きました。N で新しいゲーム。", status.Text);
        Assert.All(status.Parts, part => Assert.Equal(ConsoleColor.Red, part.Style.Foreground));
    }

    // 勝敗が決まった後も、盤面を見て回れるようにカーソルは動く。開くと旗は何もしない（UI デザイン 3.4）
    [Fact]
    public void AfterTheGameIsOverTheCursorMovesButCellsDoNotChange()
    {
        Lose();
        var board = TextsOf(screen.Render())[3..12];

        screen.HandleKey(Keys.Of(ConsoleKey.LeftArrow));
        screen.HandleKey(Keys.Of(ConsoleKey.F));
        screen.HandleKey(Keys.Of(ConsoleKey.Spacebar));

        Assert.Equal(board.Select(line => line.Replace("[", " ").Replace("]", " ")),
                     TextsOf(screen.Render())[3..12].Select(line => line.Replace("[", " ").Replace("]", " ")));
        Assert.Equal("|      [2]@ # # # # |", LineOf(3));
    }

    // 新しいゲームでは、カーソルを左上に戻す（Web 版と同じ。クラス設計書 9.1 の決定 9）
    [Fact]
    public void NewGameStartsOverWithTheCursorAtTheTopLeft()
    {
        screen.HandleKey(Keys.Of(ConsoleKey.DownArrow));
        screen.HandleKey(Keys.Of(ConsoleKey.Spacebar));

        screen.HandleKey(Keys.Of(ConsoleKey.N));

        Assert.Equal("|[#]# # # # # # # # |", LineOf(3));
        Assert.Equal("マスを開くと始まります。", LineOf(StatusRow));
    }

    [Fact]
    public void DKeyOpensTheDifficultySelection()
        => Assert.IsType<DifficultySelectionScreen>(screen.HandleKey(Keys.Of(ConsoleKey.D)));

    [Fact]
    public void QuestionMarkOpensTheHelp()
        => Assert.IsType<HelpScreen>(screen.HandleKey(Keys.Of(ConsoleKey.Oem2, '?', shift: true)));

    [Fact]
    public void QuitKeyEndsTheApplication()
        => Assert.Null(screen.HandleKey(Keys.Of(ConsoleKey.Q)));

    [Fact]
    public void OtherKeysKeepTheScreen()
        => Assert.Same(screen, screen.HandleKey(Keys.Of(ConsoleKey.A)));

    void WinAfter(TimeSpan duration)
    {
        screen.HandleKey(Keys.Of(ConsoleKey.Spacebar));
        time.Advance(duration);
        MoveRight(8);
        screen.HandleKey(Keys.Of(ConsoleKey.Spacebar));
    }

    // 最初に開いたマスには地雷を置かない（最初の一手は安全）ので、左側を開いてから、壁の地雷 (0, 4) を開く
    void Lose()
    {
        screen.HandleKey(Keys.Of(ConsoleKey.Spacebar));
        MoveRight(4);
        screen.HandleKey(Keys.Of(ConsoleKey.Spacebar));
    }

    void MoveRight(int count)
    {
        for (var step = 0; step < count; step++)
            screen.HandleKey(Keys.Of(ConsoleKey.RightArrow));
    }

    string LineOf(int row) => screen.Render().Lines[row].Text;

    static string[] TextsOf(Frame frame) => [.. frame.Lines.Select(line => line.Text)];
}
