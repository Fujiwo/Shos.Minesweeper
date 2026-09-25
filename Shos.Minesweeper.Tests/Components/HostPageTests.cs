using AngleSharp.Html.Parser;
using Bunit;
using Shos.Minesweeper.Pages;

namespace Shos.Minesweeper.Tests.Components;

/// <summary>ホストページ（wwwroot/index.html）と、見つからないときのページ（UI デザイン 2.5、仕様書 5.5）。</summary>
public class HostPageTests : AppTestContext
{
    static readonly Lazy<AngleSharp.Html.Dom.IHtmlDocument> IndexHtml = new(() =>
        new HtmlParser().ParseDocument(File.ReadAllText(Path.Combine(RepositoryRoot(), "Shos.Minesweeper", "wwwroot", "index.html"))));

    [Fact]
    public void PageLanguageIsJapanese()
        => Assert.Equal("ja", IndexHtml.Value.DocumentElement.GetAttribute("lang"));

    [Fact]
    public void PageTitleIsMinesweeper()
        => Assert.Equal("マインスイーパー", IndexHtml.Value.Title);

    [Fact]
    public void BrowserToolbarColorFollowsThePageBackgroundOfEachColorScheme()
    {
        var themeColors = IndexHtml.Value.QuerySelectorAll("meta[name=theme-color]")
                                   .Select(meta => (meta.GetAttribute("media"), meta.GetAttribute("content")));

        Assert.Equal([("(prefers-color-scheme: light)", "#eef1f5"), ("(prefers-color-scheme: dark)", "#101216")], themeColors);
    }

    [Fact]
    public void PageZoomIsNotBlocked()
    {
        var viewport = IndexHtml.Value.QuerySelector("meta[name=viewport]")!.GetAttribute("content")!;

        Assert.DoesNotContain("user-scalable=no", viewport);
        Assert.DoesNotContain("maximum-scale", viewport);
    }

    [Fact]
    public void ErrorMessageIsJapanese()
    {
        var errorUi = IndexHtml.Value.QuerySelector("#blazor-error-ui")!;

        Assert.Contains("エラーが発生しました。", errorUi.TextContent);
        Assert.Equal("再読み込み", errorUi.QuerySelector("a.reload")!.TextContent);
        Assert.Equal("閉じる", errorUi.QuerySelector(".dismiss")!.GetAttribute("aria-label"));
    }

    // .NET 10 のビルドが書き換えるプレースホルダーは、消したり変えたりしない（CLAUDE.md の「構成とポイント」）
    [Fact]
    public void BuildPlaceholdersAreKept()
    {
        var document = IndexHtml.Value;

        Assert.NotNull(document.QuerySelector("script[type=importmap]"));
        Assert.NotNull(document.QuerySelector("link[rel=preload][id=webassembly]"));
        Assert.NotNull(document.QuerySelector("script[src='_framework/blazor.webassembly#[.{fingerprint}].js']"));
        Assert.Equal("/", document.QuerySelector("base")!.GetAttribute("href"));
    }

    [Fact]
    public void ScopedStylesAreLoaded()
        => Assert.NotNull(IndexHtml.Value.QuerySelector("link[href='Shos.Minesweeper.styles.css']"));

    [Fact]
    public void NotFoundPageIsJapanese()
    {
        var cut = Render<NotFound>();

        Assert.Equal("ページが見つかりません", cut.Find("h1").TextContent);
        Assert.Equal("マインスイーパーに戻る", cut.Find("a").TextContent);
    }

    static string RepositoryRoot()
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);
        while (directory is not null && !File.Exists(Path.Combine(directory.FullName, "Shos.Minesweeper.slnx")))
            directory = directory.Parent;
        return directory?.FullName ?? throw new DirectoryNotFoundException("リポジトリの直下（Shos.Minesweeper.slnx のある場所）が見つかりません。");
    }
}
