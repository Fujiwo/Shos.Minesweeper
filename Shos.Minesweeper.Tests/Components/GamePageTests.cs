using Bunit;
using Microsoft.AspNetCore.Components.Web;
using Shos.Minesweeper.Pages;

namespace Shos.Minesweeper.Tests.Components;

public class GamePageTests : AppTestContext
{
    [Fact]
    public async Task PageShowsABeginnerBoardFittingTheArea()
    {
        var cut = Render<GamePage>();

        await NotifyBoardAreaResizedAsync(352, 576);

        cut.WaitForAssertion(() => Assert.Equal("盤面、9 行 9 列", cut.Find("[role=grid]").GetAttribute("aria-label")));
        Assert.Equal(81, cut.FindAll("[role=gridcell]").Count);
    }

    [Fact]
    public async Task ClickingACellOpensIt()
    {
        var cut = await RenderPageWithBoardAsync();

        Click(cut, "#cell-4-4", Mouse());

        // 最初に開いたマスは必ず 0 なので、乱数の盤面でも開いた姿が決まる
        Assert.Equal("cell opened", cut.Find("#cell-4-4").ClassName);
    }

    [Fact]
    public async Task RightClickingACellFlagsIt()
    {
        var cut = await RenderPageWithBoardAsync();

        cut.Find("#cell-4-4").PointerDown(Mouse(button: 2));

        Assert.Equal("cell flagged", cut.Find("#cell-4-4").ClassName);
    }

    [Fact]
    public async Task ResetStartsANewGameOfTheSameDifficulty()
    {
        var cut = await RenderPageWithBoardAsync();
        cut.Find("#cell-4-4").PointerDown(Mouse(button: 2));

        cut.Find("button.reset").Click();

        Assert.Equal("cell closed", cut.Find("#cell-4-4").ClassName);
        Assert.Equal(81, cut.FindAll("[role=gridcell]").Count);
    }

    [Fact]
    public async Task FlagModeMakesTapsFlagAndSurvivesReset()
    {
        var cut = await RenderPageWithBoardAsync();

        cut.Find("button.flag-mode").Click();
        cut.Find("button.reset").Click();
        Click(cut, "#cell-4-4", Mouse());

        Assert.Equal("true", cut.Find("button.flag-mode").GetAttribute("aria-pressed"));
        Assert.Contains("flag-mode", cut.Find("[role=grid]").ClassList);
        Assert.Equal("cell flagged", cut.Find("#cell-4-4").ClassName);
    }

    [Fact]
    public async Task FaceIsSurprisedWhileACellIsPressed()
    {
        var cut = await RenderPageWithBoardAsync();

        cut.Find("#cell-4-4").PointerDown(Mouse());
        Assert.Equal("FaceSurprised", cut.Find("button.reset svg").GetAttribute("data-kind"));

        cut.Find("[role=grid]").PointerUp(Mouse());
        Assert.Equal("FaceNormal", cut.Find("button.reset svg").GetAttribute("data-kind"));
    }

    [Fact]
    public async Task NothingIsAnnouncedWhenThePageOpens()
    {
        var cut = await RenderPageWithBoardAsync();

        Assert.Equal("", AnnouncementOf(cut));
    }

    [Fact]
    public async Task ResetAnnouncesTheNewGame()
    {
        var cut = await RenderPageWithBoardAsync();

        cut.Find("button.reset").Click();

        Assert.Equal("新しいゲーム、初級、9×9、地雷 10。", AnnouncementOf(cut));
    }

    [Fact]
    public async Task SameAnnouncementTwiceStillChangesTheLiveRegion()
    {
        var cut = await RenderPageWithBoardAsync();
        cut.Find("button.reset").Click();
        var first = cut.Find("[aria-live=polite]").TextContent;

        cut.Find("button.reset").Click();

        // 中身が変わらないと、スクリーンリーダーは読み上げない。見えない文字の有無で中身を変える
        Assert.NotEqual(first, cut.Find("[aria-live=polite]").TextContent);
        Assert.Equal("新しいゲーム、初級、9×9、地雷 10。", AnnouncementOf(cut));
    }

    [Fact]
    public async Task TheEndOfTheGameIsAnnounced()
    {
        var cut = await RenderPageWithBoardAsync();

        // 乱数の盤面なので、勝敗が決まるまで未開放のマスを開き続ける
        while (cut.FindAll(".cell.closed").FirstOrDefault() is { } closed
               && cut.Find("button.reset svg").GetAttribute("data-kind") is "FaceNormal") {
            closed.PointerDown(Mouse());
            cut.Find("[role=grid]").PointerUp(Mouse());
        }

        var announcement = AnnouncementOf(cut);
        Assert.True(announcement.StartsWith("クリア。") || announcement == "ゲームオーバー。地雷を開きました。", announcement);
    }

    [Fact]
    public async Task KeyboardAloneCanOpenACell()
    {
        var cut = await RenderPageWithBoardAsync();

        cut.Find("[role=grid]").KeyDown(new KeyboardEventArgs { Key = "ArrowDown" });
        cut.Find("[role=grid]").KeyDown(new KeyboardEventArgs { Key = "Enter" });

        Assert.Contains("opened", cut.Find("#cell-1-0").ClassList);
    }

    [Fact]
    public async Task DifficultyButtonOpensTheDialogAndMakesTheRestInert()
    {
        var cut = await RenderPageWithBoardAsync();

        cut.Find("button.difficulty").Click();

        Assert.NotNull(cut.Find("[role=dialog][aria-modal=true]"));
        Assert.True(cut.Find(".toolbar-area").HasAttribute("inert"));
        Assert.True(cut.Find(".board-region").HasAttribute("inert"));
    }

    [Fact]
    public async Task ChoosingADifficultyStartsANewGameAndReturnsFocusToTheDifficultyButton()
    {
        var cut = Render<GamePage>();
        var difficultyButtonId = ElementReferenceIdOf(cut.Find("button.difficulty"));
        await NotifyBoardAreaResizedAsync(352, 576);
        cut.Find("button.difficulty").Click();

        cut.FindAll("button.preset")[2].Click();

        Assert.Empty(cut.FindAll("[role=dialog]"));
        Assert.False(cut.Find(".toolbar-area").HasAttribute("inert"));
        Assert.Contains("上級", cut.Find("button.difficulty").TextContent);
        Assert.Equal(480, cut.FindAll("[role=gridcell]").Count);
        Assert.Equal("新しいゲーム、上級、30×16、地雷 99。", AnnouncementOf(cut));
        Assert.Equal(difficultyButtonId, LastFocusedId());
    }

    [Fact]
    public async Task ClosingTheDialogKeepsTheGame()
    {
        var cut = Render<GamePage>();
        var difficultyButtonId = ElementReferenceIdOf(cut.Find("button.difficulty"));
        await NotifyBoardAreaResizedAsync(352, 576);
        Click(cut, "#cell-4-4", Mouse());
        cut.Find("button.difficulty").Click();

        cut.Find("button.close").Click();

        Assert.Empty(cut.FindAll("[role=dialog]"));
        Assert.Contains("opened", cut.Find("#cell-4-4").ClassList);
        Assert.Equal(difficultyButtonId, LastFocusedId());
    }

    [Fact]
    public async Task SavedBestTimesAreShownInTheDialog()
    {
        JSInterop.SetupModule("./js/browser.js").Setup<string?>("readStorage", "Shos.Minesweeper.BestTimes").SetResult("""{"Beginner":23}""");
        var cut = await RenderPageWithBoardAsync();

        cut.Find("button.difficulty").Click();

        Assert.Equal("ベスト 23 秒", cut.FindAll("button.preset .best")[0].TextContent);
    }

    [Fact]
    public async Task WinningShowsTheWinCard()
    {
        var cut = await RenderPageWithWinningCustomBoardAsync();

        Click(cut, "#cell-2-2", Mouse());

        Assert.Equal("クリア！", cut.Find(".win-card h2").TextContent);
        Assert.Empty(cut.FindAll(".win-card .best-time"));   // カスタムは記録しない
        Assert.Equal("クリア。0 秒。", AnnouncementOf(cut));
    }

    [Fact]
    public async Task ClosingTheWinCardShowsTheBoardAndFocusesTheResetButton()
    {
        var cut = Render<GamePage>();
        var resetButtonId = ElementReferenceIdOf(cut.Find("button.reset"));
        await WinCustomGameAsync(cut);

        cut.Find(".win-card button.close").Click();

        Assert.Empty(cut.FindAll(".win-card"));
        Assert.Equal("FaceWon", cut.Find("button.reset svg").GetAttribute("data-kind"));
        Assert.Equal(resetButtonId, LastFocusedId());
    }

    [Fact]
    public async Task PlayAgainStartsANewGameOfTheSameDifficulty()
    {
        var cut = Render<GamePage>();
        var resetButtonId = ElementReferenceIdOf(cut.Find("button.reset"));
        await WinCustomGameAsync(cut);

        cut.Find(".win-card button.play-again").Click();

        Assert.Empty(cut.FindAll(".win-card"));
        Assert.Equal("cell closed", cut.Find("#cell-2-2").ClassName);
        Assert.Equal(25, cut.FindAll("[role=gridcell]").Count);
        Assert.Equal(resetButtonId, LastFocusedId());
    }

    async Task<IRenderedComponent<GamePage>> RenderPageWithWinningCustomBoardAsync()
    {
        var cut = await RenderPageWithBoardAsync();
        StartWinningCustomGame(cut);
        return cut;
    }

    // 効果音（仕様書 5.6。1.1.0）

    [Fact]
    public async Task SoundsArePreparedAfterTheFirstRender()
    {
        Render<GamePage>();

        await NotifyBoardAreaResizedAsync(352, 576);

        Assert.Equal(6, JSInterop.Invocations.Count(invocation => invocation.Identifier == "loadSound"));
    }

    [Fact]
    public async Task OpeningACellPlaysItsSound()
    {
        var cut = await RenderPageWithBoardAsync();

        Click(cut, "#cell-4-4", Mouse());

        // 最初に開いたマスは必ず 0 なので、乱数の盤面でも連鎖になる（まれに、その一手で勝つ）
        Assert.Contains(Assert.Single(PlayedSounds()), new[] { "Chain", "Won" });
    }

    [Fact]
    public async Task FlaggingACellPlaysTheFlagSound()
    {
        var cut = await RenderPageWithBoardAsync();

        cut.Find("#cell-4-4").PointerDown(Mouse(button: 2));

        Assert.Equal(["FlagPlaced"], PlayedSounds());
    }

    [Fact]
    public async Task SoundButtonTurnsSoundOffAndSavesIt()
    {
        var cut = await RenderPageWithBoardAsync();

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
        var cut = await RenderPageWithBoardAsync();

        cut.Find("button.sound").Click();
        cut.Find("button.sound").Click();

        Assert.Equal("true", cut.Find("button.sound").GetAttribute("aria-pressed"));
        Assert.Empty(PlayedSounds());
    }

    [Fact]
    public async Task SavedSoundSettingIsShownOnTheSoundButton()
    {
        JSInterop.SetupModule("./js/browser.js").Setup<string?>("readStorage", "Shos.Minesweeper.SoundEffects").SetResult("off");

        var cut = await RenderPageWithBoardAsync();

        Assert.Equal("false", cut.Find("button.sound").GetAttribute("aria-pressed"));
    }

    string[] PlayedSounds()
        => [.. JSInterop.Invocations.Where(invocation => invocation.Identifier == "playSound").Select(invocation => (string)invocation.Arguments[0]!)];

    async Task WinCustomGameAsync(IRenderedComponent<GamePage> cut)
    {
        await NotifyBoardAreaResizedAsync(352, 576);
        StartWinningCustomGame(cut);
        Click(cut, "#cell-2-2", Mouse());
    }

    // 5×5・地雷 16 は、最初に開いたマスとその周り 9 マス以外がすべて地雷になるので、最初の一手で必ず勝つ（仕様書 3.2）
    static void StartWinningCustomGame(IRenderedComponent<GamePage> cut)
    {
        cut.Find("button.difficulty").Click();
        cut.Find("#custom-width").Input("5");
        cut.Find("#custom-height").Input("5");
        cut.Find("#custom-mine-count").Input("16");
        cut.Find("button.start-custom").Click();
    }

    static string AnnouncementOf(IRenderedComponent<GamePage> cut)
        => cut.Find("[aria-live=polite]").TextContent.Replace("\u200B", "").Trim();

    async Task<IRenderedComponent<GamePage>> RenderPageWithBoardAsync()
    {
        var cut = Render<GamePage>();
        await NotifyBoardAreaResizedAsync(352, 576);
        cut.WaitForElement("[role=grid]");
        return cut;
    }
}
