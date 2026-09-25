using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

namespace Shos.Minesweeper.Browser;

/// <summary>1 つの要素の大きさの監視。JavaScript の ResizeObserver から呼ばれ、破棄すると監視を止める。</summary>
public sealed class SizeObservation : IAsyncDisposable
{
    readonly Func<double, double, Task> resized;
    DotNetObjectReference<SizeObservation>? reference;
    IJSObjectReference? observer;

    internal SizeObservation(Func<double, double, Task> resized) => this.resized = resized;

    internal async Task StartAsync(IJSObjectReference module, ElementReference element)
    {
        reference = DotNetObjectReference.Create(this);
        observer = await module.InvokeAsync<IJSObjectReference>("observeSize", element, reference);
    }

    [JSInvokable]
    public Task NotifyResized(double width, double height) => resized(width, height);

    public async ValueTask DisposeAsync()
    {
        if (observer is not null) {
            await observer.InvokeVoidAsync("disconnect");
            await observer.DisposeAsync();
        }
        reference?.Dispose();
    }
}
