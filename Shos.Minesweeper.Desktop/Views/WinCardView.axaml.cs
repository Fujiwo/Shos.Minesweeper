using Avalonia.Controls;
using Avalonia.Input;

namespace Shos.Minesweeper.Desktop.Views;

/// <summary>勝利カード（クラス設計書 4.10）。Esc で閉じることと、閉じたときのフォーカスは、MainWindow が受け持つ。</summary>
public partial class WinCardView : UserControl
{
    public WinCardView() => InitializeComponent();

    /// <summary>
    /// 出したときに、見出しにフォーカスを移す。最後のマスを開くために押した Space や Enter を続けて押したときに、
    /// 意図せず新しいゲームが始まらないようにするため（Web 版の UI デザイン 2.4）。
    /// </summary>
    public void FocusHeading(NavigationMethod method) => Heading.Focus(method);
}
