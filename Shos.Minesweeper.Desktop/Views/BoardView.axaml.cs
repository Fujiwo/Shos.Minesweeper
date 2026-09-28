using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.VisualTree;
using Shos.Minesweeper.Desktop.Input;
using Shos.Minesweeper.Desktop.ViewModels;
using Shos.Minesweeper.Presentation;

namespace Shos.Minesweeper.Desktop.Views;

/// <summary>
/// 盤面。ポインターとキーのイベントを受けて BoardViewModel に渡す（Web 版の BoardView と同じく、マスのイベントを盤面でまとめて受ける）。
/// カーソルのマスへフォーカスを移すのは、FocusCursorCell の 1 か所だけにする（クラス設計書 4.10）。
/// </summary>
public partial class BoardView : UserControl
{
    /// <summary>盤面の枠の太さ（Web 版と同じ値。Presentation の BoardDimensions）。</summary>
    public static Thickness FrameThickness { get; } = new(BoardDimensions.FrameWidth);

    CellView? pressedCell;

    public BoardView() => InitializeComponent();

    BoardViewModel? Board => DataContext as BoardViewModel;

    /// <summary>
    /// カーソルのマスにフォーカスを移す。キーで動いたときは、キーボードの移動として（Directional）移し、フォーカスの枠を出す。
    /// マウスで押したときは、押したことによるフォーカスに任せる（UI デザイン 2.11）。
    /// </summary>
    public void FocusCursorCell(NavigationMethod method)
    {
        if (Board is not { } board || CellViewAt(board.IndexOf(board.CursorPosition)) is not { } cell)
            return;
        KeyboardNavigation.SetTabOnceActiveElement(CellsControl, cell);
        cell.Focus(method);
    }

    protected override void OnKeyDown(KeyEventArgs e)
    {
        base.OnKeyDown(e);
        if (e.Handled || Board is not { } board || !board.HandleKey(e.Key))
            return;
        // 矢印キーを扱ったことにして、Avalonia の方向キーでのフォーカスの移動をさせない
        e.Handled = true;
        if (KeyboardMapping.DirectionFor(e.Key) is not null)
            FocusCursorCell(NavigationMethod.Directional);
    }

    protected override void OnPointerPressed(PointerPressedEventArgs e)
    {
        base.OnPointerPressed(e);
        if (Board is not { } board || CellViewOf(e.Source) is not { DataContext: CellViewModel cell } cellView)
            return;
        var properties = e.GetCurrentPoint(cellView).Properties;
        if (properties.IsRightButtonPressed) {
            board.PressRight(cell.Position);
        } else if (properties.IsLeftButtonPressed) {
            // タッチとペンのタップも、左ボタンとして届く（仕様書 4.3）
            pressedCell = cellView;
            board.Press(cell.Position);
        }
    }

    // 押したマスの上で離したときだけ開く（クラス設計書 9.1 の決定 1）。押したマスがポインターを捕らえているので、外で離しても届く
    protected override void OnPointerReleased(PointerReleasedEventArgs e)
    {
        base.OnPointerReleased(e);
        if (pressedCell is not { } cellView || Board is not { } board)
            return;
        pressedCell = null;
        board.Release(isOverPressedCell: new Rect(cellView.Bounds.Size).Contains(e.GetPosition(cellView)));
    }

    protected override void OnPointerCaptureLost(PointerCaptureLostEventArgs e)
    {
        base.OnPointerCaptureLost(e);
        if (pressedCell is null)
            return;
        pressedCell = null;
        Board?.CancelPress();
    }

    CellView? CellViewAt(int index)
        => CellsControl.ContainerFromIndex(index)?.GetVisualDescendants().OfType<CellView>().FirstOrDefault();

    static CellView? CellViewOf(object? source)
        => source as CellView ?? (source as Visual)?.FindAncestorOfType<CellView>();
}
