namespace Shos.Minesweeper.Browser;

/// <summary>
/// 効果音のオンとオフを localStorage に読み書きする（仕様書 5.6、6.4）。
/// 値は、localStorage を開いて読んでも意味が分かるように "on" と "off" の文字にする（クラス設計書 12.11 の決定 14）。
/// </summary>
public sealed class SoundSettingStorage(BrowserFeatures browser)
{
    public const string StorageKey = "Shos.Minesweeper.SoundEffects";

    const string On = "on";
    const string Off = "off";

    /// <summary>"off" ならオフ。それ以外（値がない、読めない）は、既定のオン（仕様書 5.6）。例外は投げない。</summary>
    public async Task<bool> LoadAsync()
        => await browser.ReadStorageAsync(StorageKey) != Off;

    /// <summary>書けなくても何もしない。ページを開いている間は、設定が残る（仕様書 5.6）。</summary>
    public async Task SaveAsync(bool isEnabled)
        => await browser.WriteStorageAsync(StorageKey, isEnabled ? On : Off);
}
