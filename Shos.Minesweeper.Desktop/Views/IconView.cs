using Avalonia;
using Avalonia.Controls.Primitives;

namespace Shos.Minesweeper.Desktop.Views;

/// <summary>
/// アイコン。Web 版の SVG と同じ 24×24 の座標で描いた形を、種類ごとのテンプレート（Assets/Icons.axaml）で出す。
/// 大きさは置く場所が決め、形は拡大率によらずぼやけない。Foreground が、Web 版の currentColor に当たる。
/// </summary>
public sealed class IconView : TemplatedControl
{
    public static readonly StyledProperty<IconKind> KindProperty = AvaloniaProperty.Register<IconView, IconKind>(nameof(Kind));

    public IconKind Kind
    {
        get => GetValue(KindProperty);
        set => SetValue(KindProperty, value);
    }
}
