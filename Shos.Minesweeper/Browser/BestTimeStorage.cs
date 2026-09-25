using Shos.Minesweeper.GameLogic;
using Shos.Minesweeper.Presentation;

namespace Shos.Minesweeper.Browser;

/// <summary>
/// BestTimes を localStorage に読み書きする（アーキテクチャー設計書 10 章）。
/// 保存の形式は、どのアプリでも同じ BestTimesJson を使い、このクラスは保存先だけを受け持つ。
/// </summary>
public sealed class BestTimeStorage(BrowserFeatures browser)
{
    public const string StorageKey = "Shos.Minesweeper.BestTimes";

    /// <summary>読めない値は「記録なし」として捨てる。例外は投げない。</summary>
    public async Task<BestTimes> LoadAsync()
        => BestTimesJson.Parse(await browser.ReadStorageAsync(StorageKey));

    /// <summary>書けなくても何もしない。メモリーの BestTimes は残る（仕様書 3.8）。</summary>
    public async Task SaveAsync(BestTimes bestTimes)
        => await browser.WriteStorageAsync(StorageKey, BestTimesJson.Serialize(bestTimes));
}
