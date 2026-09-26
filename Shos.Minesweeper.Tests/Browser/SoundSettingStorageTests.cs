using Bunit;
using Microsoft.Extensions.DependencyInjection;
using Shos.Minesweeper.Browser;

namespace Shos.Minesweeper.Tests.Browser;

/// <summary>効果音のオンとオフの保存（仕様書 5.6、6.4、クラス設計書 12.11 の決定 14）。localStorage は bUnit の JavaScript の偽物で受ける。</summary>
public class SoundSettingStorageTests : AppTestContext
{
    [Fact]
    public async Task SoundIsOnWhenNothingIsStored()
        => Assert.True(await LoadAsync(null));

    [Fact]
    public async Task SoundIsOffWhenOffIsStored()
        => Assert.False(await LoadAsync("off"));

    // 既定はオン。読めない値は既定に戻す（ベストタイムと同じ扱い）
    [Theory]
    [InlineData("on")]
    [InlineData("OFF")]
    [InlineData("false")]
    public async Task AnyOtherValueMeansOn(string stored)
        => Assert.True(await LoadAsync(stored));

    [Theory]
    [InlineData(true, "on")]
    [InlineData(false, "off")]
    public async Task SettingIsSavedAsOnOrOff(bool isEnabled, string expected)
    {
        await Services.GetRequiredService<SoundSettingStorage>().SaveAsync(isEnabled);

        var write = Assert.Single(JSInterop.Invocations, invocation => invocation.Identifier == "writeStorage");
        Assert.Equal(SoundSettingStorage.StorageKey, write.Arguments[0]);
        Assert.Equal(expected, write.Arguments[1]);
    }

    // BrowserFeatures は最初に読み込んだモジュールを持ち続けるので、読み込みの結果を決めてから新しく作る
    async Task<bool> LoadAsync(string? stored)
    {
        JSInterop.SetupModule("./js/browser.js").Setup<string?>("readStorage", SoundSettingStorage.StorageKey).SetResult(stored);
        return await new SoundSettingStorage(new BrowserFeatures(JSInterop.JSRuntime)).LoadAsync();
    }
}
