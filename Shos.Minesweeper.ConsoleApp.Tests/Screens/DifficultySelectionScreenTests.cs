using Microsoft.Extensions.Time.Testing;
using Shos.Minesweeper.ConsoleApp.Screens;
using Shos.Minesweeper.GameLogic;

namespace Shos.Minesweeper.ConsoleApp.Tests.Screens;

/// <summary>難易度の選択とカスタムの入力（仕様書 5.7、UI デザイン 3.7）。</summary>
public sealed class DifficultySelectionScreenTests : IDisposable
{
    readonly string folder = Path.Combine(Path.GetTempPath(), "Shos.Minesweeper.Tests", Guid.NewGuid().ToString("N"));
    readonly GameScreen game;
    readonly DifficultySelectionScreen screen;

    public DifficultySelectionScreenTests()
    {
        var path = Path.Combine(folder, "best-times.json");
        var bestTimes = new BestTimes();
        bestTimes.Record(DifficultyKind.Beginner, 23);
        bestTimes.Record(DifficultyKind.Intermediate, 98);
        new BestTimesFile(path).Save(bestTimes);
        game = new GameScreen(new FakeTimeProvider(), new BestTimesFile(path));
        screen = new DifficultySelectionScreen(game);
    }

    public void Dispose() => Directory.Delete(folder, recursive: true);

    [Fact]
    public void ScreenMatchesTheUiDesign()
        => Assert.Equal([
            "難易度を選んでください",
            "",
            "> 1  初級        9x9   地雷 10   ベスト  23 秒   （今の難易度）",
            "  2  中級      16x16   地雷 40   ベスト  98 秒",
            "  3  上級      30x16   地雷 99   記録なし",
            "  4  カスタム",
            "",
            "1〜4 のキー、または矢印と Enter で選びます。Esc で戻ります。",
        ], TextsOf(screen.Render()));

    [Fact]
    public void TextCursorIsHiddenWhileChoosing()
        => Assert.Null(screen.Render().CursorRow);

    [Fact]
    public void NumberKeyStartsThatDifficultyAndReturnsToTheGame()
    {
        var next = screen.HandleKey(Keys.Of(ConsoleKey.D3));

        Assert.Same(game, next);
        Assert.Equal(Difficulty.Expert, game.Difficulty);
    }

    // IME がオンのままでも選べるように、キーの文字を整えてから読む（仕様書 3 章、5.3）
    [Fact]
    public void FullWidthNumberAlsoSelects()
    {
        screen.HandleKey(new ConsoleKeyInfo('２', default, shift: false, alt: false, control: false));

        Assert.Equal(Difficulty.Intermediate, game.Difficulty);
    }

    [Fact]
    public void ArrowAndEnterSelectTheMarkedRow()
    {
        screen.HandleKey(Keys.Of(ConsoleKey.DownArrow));
        Assert.StartsWith("> 2", TextsOf(screen.Render())[3]);

        screen.HandleKey(Keys.Of(ConsoleKey.Enter));

        Assert.Equal(Difficulty.Intermediate, game.Difficulty);
    }

    [Fact]
    public void MarkStopsAtTheTop()
    {
        screen.HandleKey(Keys.Of(ConsoleKey.UpArrow));

        Assert.StartsWith("> 1", TextsOf(screen.Render())[2]);
    }

    [Fact]
    public void MarkStopsAtTheBottom()
    {
        for (var step = 0; step < 6; step++)
            screen.HandleKey(Keys.Of(ConsoleKey.DownArrow));

        Assert.StartsWith("> 4", TextsOf(screen.Render())[5]);
    }

    // 今と同じ難易度を選んでも、新しいゲームを始める（Web 版の UI デザイン 8 章の決定 7）
    [Fact]
    public void ChoosingTheSameDifficultyStartsANewGame()
    {
        game.HandleKey(Keys.Of(ConsoleKey.Spacebar));

        new DifficultySelectionScreen(game).HandleKey(Keys.Of(ConsoleKey.D1));

        Assert.Equal("マスを開くと始まります。", TextsOf(game.Render())[13]);
    }

    // 選ばずに戻ったときは、前のゲームを続ける（仕様書 5.7）
    [Fact]
    public void EscapeReturnsToTheSameGame()
    {
        game.HandleKey(Keys.Of(ConsoleKey.Spacebar));

        var next = screen.HandleKey(Keys.Of(ConsoleKey.Escape));

        Assert.Same(game, next);
        Assert.Equal("", TextsOf(game.Render())[13]);
    }

    [Fact]
    public void CurrentCustomDifficultyIsMarkedOnTheCustomRow()
    {
        game.StartNewGame(Difficulty.Custom(10, 10, 10));

        var lines = TextsOf(new DifficultySelectionScreen(game).Render());

        Assert.Equal("> 4  カスタム   （今の難易度）", lines[5]);
    }

    [Fact]
    public void ChoosingCustomAsksTheValuesBelowWithTheTextCursor()
    {
        var next = screen.HandleKey(Keys.Of(ConsoleKey.D4));

        var frame = screen.Render();
        Assert.Same(screen, next);
        Assert.Equal(["", "カスタム（Enter だけで [ ] の中の値。Esc でやめる）", "  幅（5〜30）[9]: "], TextsOf(frame)[8..]);
        Assert.Equal(10, frame.CursorRow);
    }

    [Fact]
    public void ThreeCustomValuesStartTheCustomGame()
    {
        screen.HandleKey(Keys.Of(ConsoleKey.D4));
        IScreen? next = screen;
        foreach (var text in new[] { "20", "10", "30" })
            next = Enter(text);

        Assert.Same(game, next);
        Assert.Equal(Difficulty.Custom(20, 10, 30), game.Difficulty);
    }

    // Esc で、カスタムをやめて前のゲームの画面に戻る（UI デザイン 3.7）
    [Fact]
    public void EscapeDuringCustomReturnsToTheGame()
    {
        screen.HandleKey(Keys.Of(ConsoleKey.D4));
        Enter("20");

        var next = screen.HandleKey(Keys.Of(ConsoleKey.Escape));

        Assert.Same(game, next);
        Assert.Equal(Difficulty.Beginner, game.Difficulty);
    }

    // カスタムの入力中は、数字のキーは入力の文字である（難易度を選ばない）
    [Fact]
    public void NumberKeysAreTypedDuringCustom()
    {
        screen.HandleKey(Keys.Of(ConsoleKey.D4));

        screen.HandleKey(Keys.Of(ConsoleKey.D2));

        Assert.Equal("  幅（5〜30）[9]: 2", TextsOf(screen.Render())[10]);
        Assert.Equal(Difficulty.Beginner, game.Difficulty);
    }

    IScreen? Enter(string text)
    {
        foreach (var character in text)
            screen.HandleKey(new ConsoleKeyInfo(character, default, shift: false, alt: false, control: false));
        return screen.HandleKey(Keys.Of(ConsoleKey.Enter));
    }

    static string[] TextsOf(ConsoleApp.Rendering.Frame frame) => [.. frame.Lines.Select(line => line.Text)];
}
