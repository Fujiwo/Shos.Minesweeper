using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

namespace Shos.Minesweeper.Browser;

/// <summary>browser.js の関数を呼ぶ窓口。JavaScript を呼ぶのはこのクラスだけである。</summary>
public sealed class BrowserFeatures(IJSRuntime jsRuntime) : IAsyncDisposable
{
    Task<IJSObjectReference>? module;

    // browser.js は最初に使うときに読み込む。相対パスなので、<base href> がサブパスでも読み込める
    Task<IJSObjectReference> Module => module ??= jsRuntime.InvokeAsync<IJSObjectReference>("import", "./js/browser.js").AsTask();

    /// <summary>要素の内側の大きさを監視し、変わるたびに resized を呼ぶ。戻り値を破棄すると監視を止める。</summary>
    public async ValueTask<IAsyncDisposable> ObserveSizeAsync(ElementReference element, Func<double, double, Task> resized)
    {
        var observation = new SizeObservation(resized);
        await observation.StartAsync(await Module, element);
        return observation;
    }

    /// <summary>振動に対応した端末だけ振動させる。対応していない端末（iOS の Safari など）では何もしない。</summary>
    public async ValueTask VibrateAsync(int milliseconds)
        => await (await Module).InvokeVoidAsync("vibrate", milliseconds);

    /// <summary>localStorage から読む。読めない（保存が禁止されている、値がない）ときは null。</summary>
    public async ValueTask<string?> ReadStorageAsync(string key)
        => await (await Module).InvokeAsync<string?>("readStorage", key);

    /// <summary>localStorage に書く。書けないときは何もしない（仕様書 3.8。メモリーの記録は残る）。</summary>
    public async ValueTask WriteStorageAsync(string key, string value)
        => await (await Module).InvokeVoidAsync("writeStorage", key, value);

    /// <summary>
    /// 要素の上で、矢印キーと Space の既定の動作（ページのスクロール）を止める。
    /// Blazor の :preventDefault はキーごとに切り替えられず、すべて止めると Tab キーで外に出られなくなるため（アーキテクチャー設計書 9.1）。
    /// </summary>
    public async ValueTask SuppressKeyScrollingAsync(ElementReference element)
        => await (await Module).InvokeVoidAsync("suppressKeyScrolling", element);

    /// <summary>効果音の波形（float の並びのバイト列）を名前で渡しておく。以後、その名前で鳴らせる（アーキテクチャー設計書 9.4）。</summary>
    public async ValueTask LoadSoundAsync(string name, byte[] samples, int sampleRate)
        => await (await Module).InvokeVoidAsync("loadSound", name, samples, sampleRate);

    /// <summary>渡しておいた効果音を鳴らす。利用者がまだ操作していないとき、Web Audio がないときは、何もしない。</summary>
    public async ValueTask PlaySoundAsync(string name)
        => await (await Module).InvokeVoidAsync("playSound", name);

    public async ValueTask DisposeAsync()
    {
        if (module is not null)
            await (await module).DisposeAsync();
    }
}
