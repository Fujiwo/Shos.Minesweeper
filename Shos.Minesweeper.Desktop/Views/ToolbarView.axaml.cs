using Avalonia.Controls;
using Avalonia.Input;

namespace Shos.Minesweeper.Desktop.Views;

/// <summary>ツールバー（クラス設計書 4.10）。値と操作はバインディングだけで結び、フォーカスを移す口だけを MainWindow に見せる。</summary>
public partial class ToolbarView : UserControl
{
    public ToolbarView() => InitializeComponent();

    /// <summary>難易度ダイアログを閉じたときに、フォーカスを戻す（Web 版の UI デザイン 6.3）。</summary>
    public void FocusDifficultyButton(NavigationMethod method) => DifficultyButton.Focus(method);

    /// <summary>勝利カードを閉じたときに、フォーカスを移す（Web 版の UI デザイン 2.4）。</summary>
    public void FocusResetButton(NavigationMethod method) => ResetButton.Focus(method);
}
