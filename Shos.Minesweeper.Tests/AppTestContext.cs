using AngleSharp.Dom;
using Bunit;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Time.Testing;
using Microsoft.JSInterop;
using Shos.Minesweeper.Browser;

namespace Shos.Minesweeper.Tests;

/// <summary>
/// Web アプリのテストの共通の準備。アプリと同じサービスを登録し、時刻は偽物、JavaScript の呼び出しは bUnit の偽物で受ける。
/// コンポーネントのテストと、JavaScript を呼ぶクラス（BestTimeStorage など）のテストで使う。
/// </summary>
public abstract class AppTestContext : BunitContext
{
    protected FakeTimeProvider Time { get; } = new();

    protected AppTestContext()
    {
        JSInterop.Mode = JSRuntimeMode.Loose;
        Services.AddSingleton<TimeProvider>(Time);
        Services.AddScoped<BrowserFeatures>();
        Services.AddScoped<BestTimeStorage>();
        Services.AddScoped<SoundEffectPlayer>();
        Services.AddScoped<SoundSettingStorage>();
    }

    /// <summary>盤面の領域の大きさが変わったことを、ブラウザーの代わりに知らせる。</summary>
    protected async Task NotifyBoardAreaResizedAsync(double width, double height)
    {
        var invocation = JSInterop.Invocations.Last(invocation => invocation.Identifier == "observeSize");
        var observation = (DotNetObjectReference<SizeObservation>)invocation.Arguments[1]!;
        await observation.Value.NotifyResized(width, height);
    }

    protected static PointerEventArgs Mouse(long button = 0)
        => new() { PointerId = 1, PointerType = "mouse", Button = button, ClientX = 100, ClientY = 100 };

    protected static PointerEventArgs Touch(double x = 100, double y = 100, double offsetX = 10, double offsetY = 10)
        => new() { PointerId = 1, PointerType = "touch", Button = 0, ClientX = x, ClientY = y, OffsetX = offsetX, OffsetY = offsetY };

    /// <summary>マスを押して離す。pointerdown はマスで、それ以外は盤面の要素で受ける（クラス設計書 5.2 の BoardView）。</summary>
    protected static void Click<TComponent>(IRenderedComponent<TComponent> cut, string cellSelector, PointerEventArgs pointer)
        where TComponent : IComponent
    {
        cut.Find(cellSelector).PointerDown(pointer);
        cut.Find("[role=grid]").PointerUp(pointer);
    }

    /// <summary>
    /// 要素の参照の ID。bUnit は、描き直した要素の参照の印（blazor:elementreference）を空にするので、
    /// フォーカスの移り先を確かめるときは、最初の描画のときにこれで取っておいて、LastFocusedId と比べる。
    /// </summary>
    protected static string? ElementReferenceIdOf(IElement element) => element.GetAttribute("blazor:elementreference");

    /// <summary>最後にフォーカスを移した要素の参照の ID。</summary>
    protected string LastFocusedId()
        => ((ElementReference)JSInterop.Invocations.Last(invocation => invocation.Identifier.EndsWith("focus")).Arguments[0]!).Id;
}
