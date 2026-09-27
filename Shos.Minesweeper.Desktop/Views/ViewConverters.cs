using Avalonia.Data.Converters;
using Shos.Minesweeper.Desktop.ViewModels;

namespace Shos.Minesweeper.Desktop.Views;

/// <summary>XAML で使う値の変換。</summary>
public static class ViewConverters
{
    /// <summary>顔の表情を、リセット ボタンのアイコンにする（Web 版の Toolbar の Face と同じ対応）。</summary>
    public static FuncValueConverter<FaceKind, IconKind> FaceIcon { get; } = new(face => face switch {
        FaceKind.Surprised => IconKind.FaceSurprised,
        FaceKind.Won       => IconKind.FaceWon,
        FaceKind.Lost      => IconKind.FaceLost,
        _                  => IconKind.FaceNormal
    });
}
