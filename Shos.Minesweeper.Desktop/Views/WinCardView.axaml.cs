using Avalonia.Controls;
using Avalonia.Input;
using Shos.Minesweeper.Desktop.ViewModels;

namespace Shos.Minesweeper.Desktop.Views;

/// <summary>
/// 勝利カード（クラス設計書 4.10）。出したときに現れる演出をする。Esc で閉じることと、閉じたときのフォーカスは、MainWindow が受け持つ。
/// </summary>
public partial class WinCardView : UserControl
{
    public WinCardView() => InitializeComponent();

    /// <summary>
    /// 出したときに、見出しにフォーカスを移す。最後のマスを開くために押した Space や Enter を続けて押したときに、
    /// 意図せず新しいゲームが始まらないようにするため（Web 版の UI デザイン 2.4）。
    /// </summary>
    public void FocusHeading(NavigationMethod method) => Heading.Focus(method);

    // カードは勝つたびに新しいビューモデルになるので、ビューモデルが変わったときに現れる演出をする。アニメーション効果がオフならしない。
    // 終わるのを待たない。async void にするのは、演出の誤りを握りつぶさないため（CellView と同じ）
    protected override async void OnDataContextChanged(EventArgs e)
    {
        base.OnDataContextChanged(e);
        if (DataContext is WinCardViewModel { FadesIn: true })
            await ViewAnimations.WinCardFadeIn().RunAsync(this);
    }
}
