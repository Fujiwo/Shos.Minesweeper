using System.ComponentModel;
using System.Globalization;
using Avalonia.Controls;
using Shos.Minesweeper.Desktop.ViewModels;
using Shos.Minesweeper.GameLogic;

namespace Shos.Minesweeper.Desktop.Views;

/// <summary>
/// 1 つのマス。見せ方からクラス（未開放、旗、開いた、地雷、踏んだ地雷、誤った旗）と数字とアイコンを決める（Web 版の CellPresentation に当たる）。
/// ポインターのイベントは、盤面（BoardView）がまとめて受ける。
/// </summary>
public partial class CellView : UserControl
{
    // 数字はマスの 60%、アイコンは 70% の大きさ（Web 版の UI デザイン 4.3）
    const double NumberScale = 0.6;
    const double IconScale = 0.7;
    const int MaxNumber = 8;

    static readonly string[] AppearanceClasses = ["closed", "flagged", "opened", "mine", "exploded", "wrong-flag"];

    CellViewModel? cell;

    public CellView() => InitializeComponent();

    protected override void OnDataContextChanged(EventArgs e)
    {
        if (cell is not null)
            cell.PropertyChanged -= OnCellChanged;
        cell = DataContext as CellViewModel;
        if (cell is not null)
            cell.PropertyChanged += OnCellChanged;
        ShowAppearance();
        base.OnDataContextChanged(e);
    }

    protected override void OnSizeChanged(SizeChangedEventArgs e)
    {
        base.OnSizeChanged(e);
        NumberText.FontSize = Math.Max(1, e.NewSize.Height * NumberScale);
        Icon.Width = Icon.Height = e.NewSize.Height * IconScale;
    }

    void OnCellChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(CellViewModel.Appearance))
            ShowAppearance();
    }

    void ShowAppearance()
    {
        if (cell is null)
            return;
        var appearance = cell.Appearance;
        foreach (var name in AppearanceClasses)
            Classes.Set(name, name == ClassOf(appearance));
        NumberText.Text = cell.Number > 0 ? cell.Number.ToString(CultureInfo.InvariantCulture) : "";
        for (var number = 1; number <= MaxNumber; number++)
            NumberText.Classes.Set($"n{number}", number == cell.Number);
        Icon.IsVisible = IconOf(appearance) is not null;
        if (IconOf(appearance) is { } icon)
            Icon.Kind = icon;
    }

    static string ClassOf(CellAppearance appearance)
        => appearance switch {
            CellAppearance.Closed       => "closed",
            CellAppearance.Flagged      => "flagged",
            CellAppearance.Opened       => "opened",
            CellAppearance.Mine         => "mine",
            CellAppearance.ExplodedMine => "exploded",
            CellAppearance.WrongFlag    => "wrong-flag",
            _                           => throw new ArgumentOutOfRangeException(nameof(appearance), appearance, null)
        };

    static IconKind? IconOf(CellAppearance appearance)
        => appearance switch {
            CellAppearance.Flagged      => IconKind.Flag,
            CellAppearance.Mine         => IconKind.Mine,
            CellAppearance.ExplodedMine => IconKind.ExplodedMine,
            CellAppearance.WrongFlag    => IconKind.WrongFlag,
            _                           => null
        };
}
