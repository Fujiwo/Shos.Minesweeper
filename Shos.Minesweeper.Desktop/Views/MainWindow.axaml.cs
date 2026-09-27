using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Threading;
using Shos.Minesweeper.Desktop.Sizing;
using Shos.Minesweeper.Desktop.ViewModels;

namespace Shos.Minesweeper.Desktop.Views;

/// <summary>
/// ウィンドウ（クラス設計書 4.10）。経過時間のタイマー、盤面に合わせたウィンドウの大きさ、F2、起動したときのフォーカスを受け持つ。
/// 判断はビューモデルと WindowSizing にあり、ここは Avalonia とつなぐだけにする。
/// </summary>
public partial class MainWindow : Window
{
    /// <summary>盤面の領域の上の余白（ツールバーの高さと、ツールバーと盤面の間）。</summary>
    public static Thickness BoardAreaMargin { get; } = new(0, WindowSizing.ToolbarHeight + WindowSizing.ToolbarGap, 0, 0);

    // 1 秒ごとのタイマーでは、秒の変わり目とずれて表示が最大 1 秒遅れるので、短い周期で確かめる（アーキテクチャー設計書 7.3）
    static readonly TimeSpan ElapsedTimeCheckInterval = TimeSpan.FromMilliseconds(250);

    readonly DispatcherTimer elapsedTimeTimer;

    public MainWindow()
    {
        InitializeComponent();
        MinWidth = WindowSizing.MinContentSize.Width;
        MinHeight = WindowSizing.MinContentSize.Height;
        elapsedTimeTimer = new DispatcherTimer(ElapsedTimeCheckInterval, DispatcherPriority.Normal, (_, _) => ViewModel?.UpdateElapsedTime());
        BoardArea.SizeChanged += (_, e) => ViewModel?.Board.SetAreaSize(e.NewSize);
        Opened += OnOpened;
        Closed += (_, _) => elapsedTimeTimer.Stop();
    }

    GameViewModel? ViewModel => DataContext as GameViewModel;

    // 最初に表示する前に、初級の盤面に合わせた大きさにしておく（画面の作業領域に収めるのは、表示した後）
    protected override void OnDataContextChanged(EventArgs e)
    {
        base.OnDataContextChanged(e);
        if (ViewModel is { } viewModel)
            (Width, Height) = WindowSizing.ContentSizeOf(viewModel.Difficulty);
    }

    // F2 は、どこにフォーカスがあっても効く（UI デザイン 2.11）。盤面のキーは、盤面が先に受けて扱う
    protected override void OnKeyDown(KeyEventArgs e)
    {
        base.OnKeyDown(e);
        if (e.Handled || ViewModel is not { } viewModel)
            return;
        var boardHadFocus = Board.IsKeyboardFocusWithin;
        if (!viewModel.HandleKey(e.Key))
            return;
        e.Handled = true;
        // 新しいゲームでカーソルは左上に戻るので、盤面にフォーカスがあれば、そこへ移す
        if (boardHadFocus)
            Board.FocusCursorCell(NavigationMethod.Directional);
    }

    void OnOpened(object? sender, EventArgs e)
    {
        FitToBoard();
        // 起動したときは、盤面にフォーカスを置く。枠は、キーを押すまで出さない（UI デザイン 2.11）
        Board.FocusCursorCell(NavigationMethod.Unspecified);
        elapsedTimeTimer.Start();
    }

    /// <summary>
    /// 既定のマスの大きさで盤面が収まるように、ウィンドウの大きさを決める。画面の作業領域に収まらなければ、縮めて動かす（UI デザイン 2.2）。
    /// 最大化しているときは、大きさを変えない（UI デザイン 4 章の決定 2）。
    /// </summary>
    void FitToBoard()
    {
        if (WindowState != WindowState.Normal || ViewModel is not { } viewModel)
            return;
        var content = WindowSizing.ContentSizeOf(viewModel.Difficulty);
        if (Screens.ScreenFromWindow(this) is not { } screen) {
            (Width, Height) = content;
            return;
        }
        var scaling = screen.Scaling;
        // 外枠（題名の帯と縁）の大きさ。表示した後にしか分からないので、今の値から求める
        var frame = FrameSize is { } frameSize ? new Size(frameSize.Width - ClientSize.Width, frameSize.Height - ClientSize.Height) : default;
        var window = new PixelRect(Position, PixelSize.FromSize(new Size(content.Width + frame.Width, content.Height + frame.Height), scaling));
        var kept = WindowSizing.KeepWithin(window, screen.WorkingArea);
        Width = kept.Width / scaling - frame.Width;
        Height = kept.Height / scaling - frame.Height;
        Position = kept.Position;
    }
}
