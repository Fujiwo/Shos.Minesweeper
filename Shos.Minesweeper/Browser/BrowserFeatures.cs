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

    public async ValueTask DisposeAsync()
    {
        if (module is not null)
            await (await module).DisposeAsync();
    }
}
