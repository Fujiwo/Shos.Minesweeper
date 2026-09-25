using Bunit;
using Microsoft.Extensions.DependencyInjection;
using Shos.Minesweeper.Browser;
using Shos.Minesweeper.GameLogic;
using Shos.Minesweeper.Tests.Components;

namespace Shos.Minesweeper.Tests.Browser;

/// <summary>ベストタイムの保存（仕様書 3.8、6.4、アーキテクチャー設計書 10 章）。localStorage は bUnit の JavaScript の偽物で受ける。</summary>
public class BestTimeStorageTests : ComponentTestBase
{
    [Fact]
    public async Task NothingStoredMeansNoRecords()
    {
        var bestTimes = await LoadAsync(stored: null);

        Assert.All(Difficulty.Presets, preset => Assert.Null(bestTimes.SecondsOf(preset.Kind)));
    }

    [Fact]
    public async Task StoredRecordsAreLoaded()
    {
        var bestTimes = await LoadAsync("""{"Beginner":23,"Expert":301}""");

        Assert.Equal(23, bestTimes.SecondsOf(DifficultyKind.Beginner));
        Assert.Null(bestTimes.SecondsOf(DifficultyKind.Intermediate));
        Assert.Equal(301, bestTimes.SecondsOf(DifficultyKind.Expert));
    }

    [Theory]
    [InlineData("not json")]
    [InlineData("[23]")]
    [InlineData("\"Beginner\"")]
    public async Task UnreadableStorageMeansNoRecords(string stored)
    {
        var bestTimes = await LoadAsync(stored);

        Assert.Null(bestTimes.SecondsOf(DifficultyKind.Beginner));
    }

    [Theory]
    [InlineData("""{"Beginner":-1}""")]
    [InlineData("""{"Beginner":1000}""")]
    [InlineData("""{"Beginner":2.5}""")]
    [InlineData("""{"Beginner":"23"}""")]
    [InlineData("""{"beginner":23}""")]
    [InlineData("""{"0":23}""")]
    public async Task InvalidValuesAreTreatedAsNoRecord(string stored)
    {
        var bestTimes = await LoadAsync(stored);

        Assert.Null(bestTimes.SecondsOf(DifficultyKind.Beginner));
    }

    [Fact]
    public async Task CustomIsNeverLoaded()
    {
        var bestTimes = await LoadAsync("""{"Custom":10,"Intermediate":98}""");

        Assert.Null(bestTimes.SecondsOf(DifficultyKind.Custom));
        Assert.Equal(98, bestTimes.SecondsOf(DifficultyKind.Intermediate));
    }

    [Fact]
    public async Task RecordsAreSavedAsJsonWithoutMissingDifficulties()
    {
        var bestTimes = new BestTimes(new Dictionary<DifficultyKind, int> {
            [DifficultyKind.Expert] = 301,
            [DifficultyKind.Beginner] = 23
        });

        await Storage.SaveAsync(bestTimes);

        var write = Assert.Single(JSInterop.Invocations, invocation => invocation.Identifier == "writeStorage");
        Assert.Equal(BestTimeStorage.StorageKey, write.Arguments[0]);
        Assert.Equal("""{"Beginner":23,"Expert":301}""", write.Arguments[1]);
    }

    [Fact]
    public async Task SavedRecordsCanBeLoadedAgain()
    {
        await Storage.SaveAsync(new BestTimes(new Dictionary<DifficultyKind, int> { [DifficultyKind.Intermediate] = 98 }));
        var saved = (string?)JSInterop.Invocations.Single(invocation => invocation.Identifier == "writeStorage").Arguments[1];

        var bestTimes = await LoadAsync(saved);

        Assert.Equal(98, bestTimes.SecondsOf(DifficultyKind.Intermediate));
    }

    BestTimeStorage Storage => Services.GetRequiredService<BestTimeStorage>();

    // BrowserFeatures は最初に読み込んだモジュールを持ち続けるので、読み込みの結果を決めてから新しく作る
    async Task<BestTimes> LoadAsync(string? stored)
    {
        JSInterop.SetupModule("./js/browser.js").Setup<string?>("readStorage", BestTimeStorage.StorageKey).SetResult(stored);
        return await new BestTimeStorage(new BrowserFeatures(JSInterop.JSRuntime)).LoadAsync();
    }
}
