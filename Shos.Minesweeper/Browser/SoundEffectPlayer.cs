using System.Runtime.InteropServices;
using Shos.Minesweeper.Presentation;

namespace Shos.Minesweeper.Browser;

/// <summary>
/// Web 版の音の出口（クラス設計書 12.6）。効果音の波形を合成して browser.js に渡しておき、求められた効果音を鳴らす。
/// 効果音のオンとオフもここで持つ。オンとオフは、鳴らす実装がある版だけの事情だからである（アーキテクチャー設計書 14 章の決定 11）。
/// </summary>
public sealed class SoundEffectPlayer(BrowserFeatures browser)
{
    bool isPrepared;

    /// <summary>効果音を鳴らすかどうか。既定はオン（仕様書 5.6）。</summary>
    public bool IsEnabled { get; set; } = true;

    /// <summary>6 つの効果音を合成して、JavaScript に渡す。2 回目からは何もしない（見つからないページから戻ると、GamePage が作り直されて、また呼ぶため）。</summary>
    public async Task PrepareAsync()
    {
        if (isPrepared)
            return;
        isPrepared = true;
        foreach (var effect in Enum.GetValues<SoundEffect>())
            await browser.LoadSoundAsync(effect.ToString(), BytesOf(SoundEffectSynthesizer.Synthesize(effect)), SoundEffectSynthesizer.SampleRate);
    }

    /// <summary>効果音を鳴らす（SoundEffectOutput の形）。オフなら何もしない。</summary>
    public void Play(SoundEffect effect)
    {
        if (!IsEnabled)
            return;
        // 音の出口は、呼ばれたらすぐに返す約束なので、完了を待たずに捨てる（クラス設計書 12.4）。JavaScript の関数はこの呼び出しの中で動き始め、
        // 鳴らすときの失敗は JavaScript の側で受け止めるので、.NET に届く失敗はない
        _ = browser.PlaySoundAsync(effect.ToString()).AsTask();
    }

    // WebAssembly もブラウザーも、数の並びはリトルエンディアンなので、float の並びをそのまま渡せば、JavaScript で Float32Array として読める
    static byte[] BytesOf(float[] samples) => MemoryMarshal.AsBytes(samples.AsSpan()).ToArray();
}
