using System.Globalization;
using System.Text.RegularExpressions;
using System.Xml.Linq;
using Shos.Minesweeper.Desktop.Views;

namespace Shos.Minesweeper.Desktop.Tests.Views;

/// <summary>
/// 配色のトークンと演出の時間が、Web 版と同じであること（アーキテクチャー設計書 7.5、クラス設計書 4.11）。
/// 形式が違うので 2 か所に書き、一致をここで確かめる。比べるファイルは、テストのプロジェクトにリンクして出力のフォルダーに写してある。
/// </summary>
public partial class WebStyleConsistencyTests
{
    static readonly XNamespace Avalonia = "https://github.com/avaloniaui";
    static readonly XNamespace Xaml = "http://schemas.microsoft.com/winfx/2006/xaml";

    [Theory]
    [InlineData("Light")]
    [InlineData("Dark")]
    public void ColorTokensAreTheSameAsTheWebVersion(string theme)
    {
        var webTokens = WebColorTokens(theme);
        var desktopColors = DesktopColors(theme);

        Assert.NotEmpty(webTokens);
        foreach (var (name, color) in webTokens) {
            Assert.True(desktopColors.TryGetValue(name, out var desktopColor), $"{theme} に {name} がない");
            Assert.Equal((name, color), (name, desktopColor));
        }
    }

    // 演出の名前、長さ、遅れの最大（遅れのない演出は null）の組
    [Fact]
    public void CellAnimationTimingsAreTheSameAsTheWebVersion()
    {
        var expected = new HashSet<(string, int, int?)> {
            ("flag-planted", Milliseconds(CellAnimationTimings.FlagPlanted), null),
            ("number-appear", Milliseconds(CellAnimationTimings.Reveal), Milliseconds(CellAnimationTimings.RevealMaxDelay)),
            ("tile-leave", Milliseconds(CellAnimationTimings.Reveal), Milliseconds(CellAnimationTimings.RevealMaxDelay)),
            ("explode", Milliseconds(CellAnimationTimings.Explode), null),
            ("tile-leave", Milliseconds(CellAnimationTimings.Appear), Milliseconds(CellAnimationTimings.AppearMaxDelay)),
            ("grow", Milliseconds(CellAnimationTimings.Appear), Milliseconds(CellAnimationTimings.AppearMaxDelay)),
            ("flag-bounce", Milliseconds(CellAnimationTimings.FlagBounce), Milliseconds(CellAnimationTimings.FlagBounceMaxDelay))
        };

        Assert.Equal(expected, AnimationsOf(ReadStyle("BoardView.razor.css")).ToHashSet());
    }

    [Fact]
    public void WinCardFadeInIsTheSameAsTheWebVersion()
        => Assert.Equal([("fade-in", Milliseconds(CellAnimationTimings.WinCardFadeIn), (int?)null)], AnimationsOf(ReadStyle("WinCard.razor.css")));

    // app.css の最初の :root はライト、@media (prefers-color-scheme: dark) の中の :root はダーク
    static Dictionary<string, string> WebColorTokens(string theme)
    {
        var css = ReadStyle("app.css");
        var darkStart = css.IndexOf("@media (prefers-color-scheme: dark)", StringComparison.Ordinal);
        var block = theme == "Light" ? css[..darkStart] : css[darkStart..];
        var root = RootBlock().Match(block).Groups[1].Value;
        return Token().Matches(root).ToDictionary(match => match.Groups[1].Value, match => ColorOf(match.Groups[2].Value.Trim()));
    }

    static Dictionary<string, string> DesktopColors(string theme)
    {
        var document = XDocument.Load(Path.Combine(AppContext.BaseDirectory, "Styles", "Colors.axaml"));
        var dictionary = document.Descendants(Avalonia + "ResourceDictionary").Single(element => (string?)element.Attribute(Xaml + "Key") == theme);
        return dictionary.Elements(Avalonia + "SolidColorBrush")
                         .ToDictionary(brush => (string)brush.Attribute(Xaml + "Key")!, brush => ((string)brush.Attribute("Color")!).ToLowerInvariant());
    }

    // CSS の色を、Avalonia の書き方（#rrggbb、透明度があれば #aarrggbb）にする
    static string ColorOf(string cssColor)
    {
        var rgba = Rgba().Match(cssColor);
        if (!rgba.Success)
            return cssColor.ToLowerInvariant();
        var alpha = (int)Math.Round(double.Parse(rgba.Groups[4].Value, CultureInfo.InvariantCulture) * 255, MidpointRounding.AwayFromZero);
        return string.Create(CultureInfo.InvariantCulture,
                             $"#{alpha:x2}{int.Parse(rgba.Groups[1].Value):x2}{int.Parse(rgba.Groups[2].Value):x2}{int.Parse(rgba.Groups[3].Value):x2}");
    }

    static IEnumerable<(string Name, int Duration, int? MaxDelay)> AnimationsOf(string css)
        => Animation().Matches(css).Select(match => (match.Groups[1].Value, int.Parse(match.Groups[2].Value),
                                                     match.Groups[3].Success ? int.Parse(match.Groups[3].Value) : (int?)null));

    static string ReadStyle(string name) => File.ReadAllText(Path.Combine(AppContext.BaseDirectory, "Styles", name));

    static int Milliseconds(TimeSpan time) => (int)time.TotalMilliseconds;

    [GeneratedRegex(@":root\s*\{([^}]*)\}")]
    private static partial Regex RootBlock();

    [GeneratedRegex(@"--([a-z0-9-]+)\s*:\s*([^;]+);")]
    private static partial Regex Token();

    [GeneratedRegex(@"rgba\(\s*(\d+)\s*,\s*(\d+)\s*,\s*(\d+)\s*,\s*([\d.]+)\s*\)")]
    private static partial Regex Rgba();

    // 「animation: 名前 長さms 変化の仕方 calc(var(--delay-ratio) * 遅れの最大ms) ...;」。動きを減らす設定の「animation: none」は含めない
    [GeneratedRegex(@"animation:\s*([a-z-]+)\s+(\d+)ms(?:[^;*]*\*\s*(\d+)ms)?[^;]*;")]
    private static partial Regex Animation();
}
