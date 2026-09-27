using Bunit;
using Shos.Minesweeper.Pages;

namespace Shos.Minesweeper.Tests.Components;

/// <summary>ゲームのページと効果音のつなぎ（仕様書 5.6。1.1.0）。鳴らしたかどうかは、browser.js の playSound の呼び出しで見る。</summary>
public class GamePageSoundTests : AppTestContext
{
    // 合成で盤面の最初の表示を遅らせないように、盤面を描いてから用意する
    [Fact]
    public void SoundsAreNotPreparedBeforeTheBoardIsShown()
    {
        Render<GamePage>();

        Assert.DoesNotContain(JSInterop.Invocations, invocation => invocation.Identifier == "loadSound");
    }

    // 用意は、盤面の描画を画面に出してから（処理をいったん返してから）行うので、終わるのを待つ
    [Fact]
    public async Task SoundsArePreparedAfterTheBoardIsShown()
    {
        var cut = Render<GamePage>();

        await NotifyBoardAreaResizedAsync(352, 576);

        cut.WaitForAssertion(() => Assert.Equal(6, JSInterop.Invocations.Count(invocation => invocation.Identifier == "loadSound")));
    }

    [Fact]
    public async Task OpeningACellPlaysItsSound()
    {
        var cut = await RenderGamePageWithBoardAsync();

        Click(cut, "#cell-4-4", Mouse());

        // 最初に開いたマスは必ず 0 なので、乱数の盤面でも連鎖になる（まれに、その一手で勝つ）
        Assert.Contains(Assert.Single(PlayedSounds()), new[] { "Chain", "Won" });
    }

    [Fact]
    public async Task FlaggingACellPlaysTheFlagSound()
    {
        var cut = await RenderGamePageWithBoardAsync();

        cut.Find("#cell-4-4").PointerDown(Mouse(button: 2));

        Assert.Equal(["FlagPlaced"], PlayedSounds());
    }

    [Fact]
    public async Task SoundButtonTurnsSoundOffAndSavesIt()
    {
        var cut = await RenderGamePageWithBoardAsync();

        cut.Find("button.sound").Click();
        cut.Find("#cell-4-4").PointerDown(Mouse(button: 2));

        Assert.Equal("false", cut.Find("button.sound").GetAttribute("aria-pressed"));
        Assert.Empty(PlayedSounds());
        var write = JSInterop.Invocations.Last(invocation => invocation.Identifier == "writeStorage");
        Assert.Equal(("Shos.Minesweeper.SoundEffects", "off"), (write.Arguments[0], write.Arguments[1]));
    }

    // オンに戻しても、音は鳴らさない（UI デザイン 10.8 の決定 10）
    [Fact]
    public async Task TurningSoundBackOnPlaysNothing()
    {
        var cut = await RenderGamePageWithBoardAsync();

        cut.Find("button.sound").Click();
        cut.Find("button.sound").Click();

        Assert.Equal("true", cut.Find("button.sound").GetAttribute("aria-pressed"));
        Assert.Empty(PlayedSounds());
    }

    [Fact]
    public async Task SavedSoundSettingIsShownOnTheSoundButton()
    {
        JSInterop.SetupModule("./js/browser.js").Setup<string?>("readStorage", "Shos.Minesweeper.SoundEffects").SetResult("off");

        var cut = await RenderGamePageWithBoardAsync();

        Assert.Equal("false", cut.Find("button.sound").GetAttribute("aria-pressed"));
    }

    string[] PlayedSounds()
        => [.. JSInterop.Invocations.Where(invocation => invocation.Identifier == "playSound").Select(invocation => (string)invocation.Arguments[0]!)];
}
