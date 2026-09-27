using Microsoft.Extensions.Time.Testing;
using Shos.Minesweeper.ConsoleApp.Screens;
using Shos.Minesweeper.GameLogic;

namespace Shos.Minesweeper.ConsoleApp.Tests.Screens;

/// <summary>キーの一覧（仕様書 5.3、UI デザイン 3.6）。</summary>
public class HelpScreenTests
{
    readonly GameScreen game = new(new FakeTimeProvider(), new BestTimesFile(Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N"), "best-times.json")));

    [Fact]
    public void HelpMatchesTheUiDesign()
        => Assert.Equal([
            "キーの一覧",
            "",
            "  矢印キー、H J K L   カーソルを動かす（H 左、J 下、K 上、L 右）",
            "  Space、Enter        開く（数字のマスではコード）",
            "  F                   旗を立てる・外す",
            "  N                   新しいゲーム",
            "  D                   難易度を選ぶ",
            "  ?                   このキーの一覧",
            "  Q、Ctrl+C           終わる",
            "",
            "何かキーを押すと戻ります。",
        ], new HelpScreen(game).Render().Lines.Select(line => line.Text));

    [Theory]
    [InlineData(ConsoleKey.A)]
    [InlineData(ConsoleKey.Q)]
    [InlineData(ConsoleKey.Escape)]
    public void AnyKeyReturnsToTheGame(ConsoleKey key)
        => Assert.Same(game, new HelpScreen(game).HandleKey(Keys.Of(key)));
}
