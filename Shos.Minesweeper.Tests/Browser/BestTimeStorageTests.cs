using Bunit;
using Microsoft.Extensions.DependencyInjection;
using Shos.Minesweeper.Browser;
using Shos.Minesweeper.GameLogic;

namespace Shos.Minesweeper.Tests.Browser;

/// <summary>
/// ベストタイムの保存先（localStorage）とのやり取り（仕様書 3.8、6.4）。localStorage は bUnit の JavaScript の偽物で受ける。
/// 保存の形式そのものは、Shos.Minesweeper.GameLogic.Tests の BestTimesJsonTests で確かめる。
/// </summary>
public class BestTimeStorageTests : AppTestContext
{
    [Fact]
    public async Task RecordsAreLoadedFromTheStorageKey()
    {
        var bestTimes = await LoadAsync("""{"Beginner":23}""");

        Assert.Equal(23, bestTimes.SecondsOf(DifficultyKind.Beginner));
    }

    // 保存が禁止されている、または値がない。読めない形式の値は、BestTimesJsonTests で確かめる
    [Fact]
    public async Task MissingStorageValueMeansNoRecords()
    {
        var bestTimes = await LoadAsync(null);

        Assert.All(Difficulty.Presets, preset => Assert.Null(bestTimes.SecondsOf(preset.Kind)));
    }

    [Fact]
    public async Task RecordsAreSavedToTheStorageKey()
    {
        await Storage.SaveAsync(new BestTimes(new Dictionary<DifficultyKind, int> { [DifficultyKind.Beginner] = 23 }));

        var write = Assert.Single(JSInterop.Invocations, invocation => invocation.Identifier == "writeStorage");
        Assert.Equal(BestTimeStorage.StorageKey, write.Arguments[0]);
        Assert.Equal("""{"Beginner":23}""", write.Arguments[1]);
    }

    BestTimeStorage Storage => Services.GetRequiredService<BestTimeStorage>();

    // BrowserFeatures は最初に読み込んだモジュールを持ち続けるので、読み込みの結果を決めてから新しく作る
    async Task<BestTimes> LoadAsync(string? stored)
    {
        JSInterop.SetupModule("./js/browser.js").Setup<string?>("readStorage", BestTimeStorage.StorageKey).SetResult(stored);
        return await new BestTimeStorage(new BrowserFeatures(JSInterop.JSRuntime)).LoadAsync();
    }
}
