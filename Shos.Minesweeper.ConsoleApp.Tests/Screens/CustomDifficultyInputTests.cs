using Shos.Minesweeper.ConsoleApp.Screens;
using Shos.Minesweeper.GameLogic;

namespace Shos.Minesweeper.ConsoleApp.Tests.Screens;

/// <summary>カスタムの値を 1 行ずつ尋ねる（仕様書 5.7、UI デザイン 3.7）。</summary>
public class CustomDifficultyInputTests
{
    // 今の盤面は中級（16×16、地雷 40）。何も入れずに Enter を押すと、この値になる
    readonly CustomDifficultyInput input = new(Difficulty.Intermediate);

    [Fact]
    public void FirstPromptAsksTheWidthWithItsRangeAndTheCurrentValue()
    {
        Assert.Equal([
            "カスタム（Enter だけで [ ] の中の値。Esc でやめる）",
            "  幅（5〜30）[16]: ",
        ], TextsOf(input));
        Assert.Equal(1, input.PromptLineIndex);
    }

    [Fact]
    public void TypedTextIsShownOnThePrompt()
    {
        Type("20");

        Assert.Equal("  幅（5〜30）[16]: 20", TextsOf(input)[1]);
    }

    // 地雷数の範囲は、入力した幅と高さから求める（20 × 20 − 9 = 391）
    [Fact]
    public void AnsweredValuesStayAndTheMineCountRangeFollowsThem()
    {
        Enter("20");
        Enter("２０");

        Assert.Equal([
            "カスタム（Enter だけで [ ] の中の値。Esc でやめる）",
            "  幅（5〜30）[16]: 20",
            "  高さ（5〜24）[16]: 20",
            "  地雷数（1〜391）[40]: ",
        ], TextsOf(input));
        Assert.Equal(3, input.PromptLineIndex);
    }

    [Fact]
    public void ThreeValuesMakeTheCustomDifficulty()
    {
        Enter("20");
        Enter("10");
        Enter("30");

        Assert.Equal(Difficulty.Custom(20, 10, 30), input.Entered);
    }

    [Fact]
    public void EnterWithoutTextUsesTheCurrentValues()
    {
        Enter("");
        Enter("");
        Enter("");

        Assert.Equal(Difficulty.Custom(16, 16, 40), input.Entered);
    }

    // 前後の空白（全角を含む）を除き、全角の数字を半角にしてから読む（仕様書 3 章。ユーザーの指示、2026-09-27）
    [Fact]
    public void InputIsNormalizedBeforeItIsRead()
    {
        Enter("　１２ ");

        Assert.Equal("  幅（5〜30）[16]: 12", TextsOf(input)[1]);
    }

    [Fact]
    public void OutOfRangeValueShowsTheErrorAndAsksAgain()
    {
        Enter("20");
        Enter("20");

        Enter("500");

        Assert.Equal([
            "カスタム（Enter だけで [ ] の中の値。Esc でやめる）",
            "  幅（5〜30）[16]: 20",
            "  高さ（5〜24）[16]: 20",
            "  地雷数（1〜391）[40]: ",
            "  1〜391 の整数を入力してください",
        ], TextsOf(input));
        Assert.Null(input.Entered);
    }

    [Fact]
    public void ErrorLineIsRed()
        => Assert.All(ErrorLineAfter("abc").Parts, part => Assert.Equal(ConsoleColor.Red, part.Style.Foreground));

    // 何度誤っても、誤りの行は 1 行だけ（画面が端末の高さを超えないように。UI デザイン 3.7）
    [Fact]
    public void RepeatedErrorsKeepASingleErrorLine()
    {
        Enter("4");
        Enter("31");
        Enter("x");

        Assert.Equal(3, TextsOf(input).Length);
        Assert.Equal("  5〜30 の整数を入力してください", TextsOf(input)[2]);
    }

    [Fact]
    public void CorrectValueClearsTheErrorLine()
    {
        Enter("4");

        Enter("5");

        Assert.DoesNotContain(TextsOf(input), line => line.Contains("整数を入力してください"));
    }

    [Theory]
    [InlineData("4")]    // 下限の 1 つ下
    [InlineData("31")]   // 上限の 1 つ上
    [InlineData("1.5")]  // 整数でない
    public void WidthOutsideTheRangeIsAnError(string text)
    {
        Enter(text);

        Assert.Equal("  5〜30 の整数を入力してください", TextsOf(input)[2]);
    }

    [Theory]
    [InlineData("5")]
    [InlineData("30")]
    public void WidthAtTheEdgesOfTheRangeIsAccepted(string text)
    {
        Enter(text);

        Assert.StartsWith("  高さ", TextsOf(input)[2]);
    }

    [Fact]
    public void EscapeCancels()
    {
        Enter("20");

        input.HandleKey(Keys.Of(ConsoleKey.Escape));

        Assert.True(input.IsCancelled);
        Assert.Null(input.Entered);
    }

    ConsoleApp.Rendering.FrameLine ErrorLineAfter(string text)
    {
        Enter(text);
        return input.Lines[^1];
    }

    void Enter(string text)
    {
        Type(text);
        input.HandleKey(Keys.Of(ConsoleKey.Enter));
    }

    void Type(string text)
    {
        foreach (var character in text)
            input.HandleKey(new ConsoleKeyInfo(character, default, shift: false, alt: false, control: false));
    }

    static string[] TextsOf(CustomDifficultyInput input) => [.. input.Lines.Select(line => line.Text)];
}
