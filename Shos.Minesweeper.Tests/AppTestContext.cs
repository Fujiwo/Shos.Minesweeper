using Bunit;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Time.Testing;
using Microsoft.JSInterop;
using Shos.Minesweeper.Browser;

namespace Shos.Minesweeper.Tests.Components;

/// <summary>コンポーネントのテストの共通の準備。時刻は偽物、JavaScript の呼び出しは bUnit の偽物で受ける。</summary>
public abstract class ComponentTestBase : BunitContext
{
    protected FakeTimeProvider Time { get; } = new();

    protected ComponentTestBase()
    {
        JSInterop.Mode = JSRuntimeMode.Loose;
        Services.AddSingleton<TimeProvider>(Time);
        Services.AddScoped<BrowserFeatures>();
        Services.AddScoped<BestTimeStorage>();
    }

    /// <summary>盤面の領域の大きさが変わったことを、ブラウザーの代わりに知らせる。</summary>
    protected async Task NotifyBoardAreaResizedAsync(double width, double height)
    {
        var invocation = JSInterop.Invocations.Last(invocation => invocation.Identifier == "observeSize");
        var observation = (DotNetObjectReference<SizeObservation>)invocation.Arguments[1]!;
        await observation.Value.NotifyResized(width, height);
    }
}
