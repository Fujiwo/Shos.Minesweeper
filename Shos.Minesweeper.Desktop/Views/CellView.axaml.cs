using System.ComponentModel;
using System.Globalization;
using Avalonia.Animation;
using Avalonia.Controls;
using Shos.Minesweeper.Desktop.ViewModels;
using Shos.Minesweeper.GameLogic;

namespace Shos.Minesweeper.Desktop.Views;

/// <summary>
/// 1 つのマス。見せ方からクラス（未開放、旗、開いた、地雷、踏んだ地雷、誤った旗）と数字とアイコンを決める（Web 版の CellPresentation に当たる）。
/// ポインターのイベントは、盤面（BoardView）がまとめて受ける。演出の組み立ては ViewAnimations にあり、ここは始めて止めるだけにする。
/// </summary>
public partial class CellView : UserControl
{
    // 数字はマスの 60%、アイコンは 70% の大きさ（Web 版の UI デザイン 4.3）
    const double NumberScale = 0.6;
    const double IconScale = 0.7;
    const int MaxNumber = 8;

    static readonly string[] AppearanceClasses = [.. Enum.GetValues<CellAppearance>().Select(ClassOf)];

    CellViewModel? cell;
    CancellationTokenSource? runningAnimations;

    public CellView() => InitializeComponent();

    protected override void OnDataContextChanged(EventArgs e)
    {
        StopAnimations();
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
        switch (e.PropertyName) {
            case nameof(CellViewModel.Appearance):
                ShowAppearance();
                break;
            case nameof(CellViewModel.Animation) or nameof(CellViewModel.IsFlagJustPlaced):
                Animate();
                break;
        }
    }

    // 演出は、ビューモデルが新しい見せ方を知らせた後に付ける。付け直されたら、前の演出を止めて始め直す
    void Animate()
    {
        StopAnimations();
        if (cell is null)
            return;
        var animations = ViewAnimations.CellAnimationsOf(cell, Cover, NumberText, Icon, Bounds.Height);
        if (animations.Count == 0)
            return;
        runningAnimations = new CancellationTokenSource();
        Run(animations, runningAnimations.Token);
    }

    // 終わるのを待たない。async void にするのは、演出の誤り（プログラムの誤り）を握りつぶさず、UI のスレッドの例外としてアプリに届けるため
    // （Task を捨てると、例外が見えなくなり、覆いが残ったまま気づけない）
    async void Run(IReadOnlyList<(Animation Animation, Animatable Target)> animations, CancellationToken cancellation)
    {
        Cover.IsVisible = animations.Any(animation => animation.Target == Cover);
        await Task.WhenAll(animations.Select(animation => animation.Animation.RunAsync(animation.Target, cancellation)));
        if (!cancellation.IsCancellationRequested)
            Cover.IsVisible = false;
    }

    void StopAnimations()
    {
        runningAnimations?.Cancel();
        runningAnimations = null;
        Cover.IsVisible = false;
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
        var icon = IconOf(appearance);
        Icon.IsVisible = icon is not null;
        if (icon is { } kind)
            Icon.Kind = kind;
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
