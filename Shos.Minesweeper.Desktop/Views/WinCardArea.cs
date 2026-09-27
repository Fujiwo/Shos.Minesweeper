using Avalonia;
using Avalonia.Controls;
using Shos.Minesweeper.Desktop.Sizing;

namespace Shos.Minesweeper.Desktop.Views;

/// <summary>
/// 勝利カードの置き場（Web 版の win-card-area。UI デザイン 10.5）。盤面の領域に重ね、カードを盤面の下か、領域の下端に置く。
/// 置き場そのものは押せない（背景がない）ので、カードの外は盤面を押せる。位置の計算は WindowSizing にある。
/// </summary>
public sealed class WinCardArea : Panel
{
    /// <summary>盤面（スクロールの領域）の高さ。盤面は、盤面の領域の縦の中央にある。</summary>
    public static readonly StyledProperty<double> BoardHeightProperty = AvaloniaProperty.Register<WinCardArea, double>(nameof(BoardHeight));

    static WinCardArea() => AffectsArrange<WinCardArea>(BoardHeightProperty);

    public double BoardHeight
    {
        get => GetValue(BoardHeightProperty);
        set => SetValue(BoardHeightProperty, value);
    }

    // 置き場は盤面の領域に重ねるだけなので、自分の大きさは求めない
    protected override Size MeasureOverride(Size availableSize)
    {
        foreach (var child in Children)
            child.Measure(new Size(WindowSizing.WinCardWidthOf(availableSize.Width), double.PositiveInfinity));
        return default;
    }

    protected override Size ArrangeOverride(Size finalSize)
    {
        var width = WindowSizing.WinCardWidthOf(finalSize.Width);
        foreach (var child in Children) {
            var top = WindowSizing.WinCardTopOf(finalSize.Height, BoardHeight, child.DesiredSize.Height);
            child.Arrange(new Rect((finalSize.Width - width) / 2, top, width, child.DesiredSize.Height));
        }
        return finalSize;
    }
}
