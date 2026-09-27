using Microsoft.Extensions.Time.Testing;
using Shos.Minesweeper.ConsoleApp.Screens;
using Shos.Minesweeper.ConsoleApp.Terminal;
using Shos.Minesweeper.GameLogic;

namespace Shos.Minesweeper.ConsoleApp.Tests.Screens;

/// <summary>画面の移り変わりと、端末が小さいときの扱い（仕様書 5.5、5.9、クラス設計書 5.4）。</summary>
public class ScreenNavigatorTests
{
    // 初級のゲームの画面は 76 列 × 15 行が要る（UI デザイン 3.2）
    static readonly TerminalSize LargeEnough = new(80, 24);
    static readonly TerminalSize OneRowShort = new(80, 14);
    static readonly TerminalSize OneColumnShort = new(75, 24);

    readonly GameScreen game;
    readonly ScreenNavigator navigator;

    public ScreenNavigatorTests()
    {
        // 保存はしない（勝たない）ので、ファイルは書かれない
        game = new GameScreen(new FakeTimeProvider(), new BestTimesFile(Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N"), "best-times.json")));
        navigator = new ScreenNavigator(game);
    }

    [Fact]
    public void CurrentScreenIsShownWhenTheTerminalIsLargeEnough()
        => Assert.Equal(TextsOf(game.Render()), TextsOf(navigator.Render(LargeEnough)));

    [Fact]
    public void TooSmallScreenIsShownWhenOneRowIsMissing()
        => Assert.Equal("端末の画面を広げてください。", navigator.Render(OneRowShort).Lines[0].Text);

    [Fact]
    public void TooSmallScreenIsShownWhenOneColumnIsMissing()
        => Assert.Equal("端末の画面を広げてください。", navigator.Render(OneColumnShort).Lines[0].Text);

    [Fact]
    public void TooSmallScreenTellsTheRequiredAndActualSizes()
        => Assert.Contains("76 列 x 15 行 が要ります。", TextsOf(navigator.Render(OneRowShort)));

    [Fact]
    public void KeysGoToTheCurrentScreen()
    {
        navigator.HandleKey(Keys.Of(ConsoleKey.RightArrow), LargeEnough);

        Assert.Equal("| #[#]# # # # # # # |", navigator.Render(LargeEnough).Lines[3].Text);
    }

    [Fact]
    public void QuitFromTheGameScreenRequestsExit()
    {
        navigator.HandleKey(Keys.Of(ConsoleKey.Q), LargeEnough);

        Assert.True(navigator.IsExitRequested);
    }

    [Fact]
    public void CtrlCRequestsExit()
    {
        navigator.HandleKey(Keys.Of(ConsoleKey.C, '\u0003', control: true), LargeEnough);

        Assert.True(navigator.IsExitRequested);
    }

    // 端末が小さい間は、盤面の操作を受けない（仕様書 5.5）
    [Fact]
    public void BoardKeysAreIgnoredWhileTheTerminalIsTooSmall()
    {
        navigator.HandleKey(Keys.Of(ConsoleKey.RightArrow), OneRowShort);

        Assert.Equal("|[#]# # # # # # # # |", navigator.Render(LargeEnough).Lines[3].Text);
    }

    [Fact]
    public void QuitWorksWhileTheTerminalIsTooSmall()
    {
        navigator.HandleKey(Keys.Of(ConsoleKey.Q), OneRowShort);

        Assert.True(navigator.IsExitRequested);
    }

    // 端末が小さい間も、D で難易度の選択を開ける。選択の画面はゲームの画面より行が少ないので、盤面が収まらない端末でも出せる（仕様書 5.5）
    [Fact]
    public void DifficultySelectionOpensWhileTheTerminalIsTooSmallForTheBoard()
    {
        navigator.HandleKey(Keys.Of(ConsoleKey.D), OneRowShort);

        Assert.Equal("難易度を選んでください", navigator.Render(OneRowShort).Lines[0].Text);
    }

    // 小さい端末で上級を選ぶと、また端末を広げるよう知らせる。中級と上級は 22 行が要る
    [Fact]
    public void ChoosingABoardThatDoesNotFitShowsTheTooSmallScreenAgain()
    {
        navigator.HandleKey(Keys.Of(ConsoleKey.D), OneRowShort);
        navigator.HandleKey(Keys.Of(ConsoleKey.D3), OneRowShort);

        Assert.Contains("76 列 x 22 行 が要ります。", TextsOf(navigator.Render(OneRowShort)));
    }

    [Fact]
    public void OtherKeysDoNotRequestExit()
    {
        navigator.HandleKey(Keys.Of(ConsoleKey.A), LargeEnough);

        Assert.False(navigator.IsExitRequested);
    }

    static string[] TextsOf(ConsoleApp.Rendering.Frame frame) => [.. frame.Lines.Select(line => line.Text)];
}
