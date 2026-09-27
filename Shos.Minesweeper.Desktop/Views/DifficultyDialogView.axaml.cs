using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.VisualTree;
using Shos.Minesweeper.Desktop.ViewModels;
using Shos.Minesweeper.GameLogic;
using Shos.Minesweeper.Presentation;

namespace Shos.Minesweeper.Desktop.Views;

/// <summary>
/// 難易度ダイアログ（クラス設計書 4.10）。開いたときのフォーカス、Esc と幕のクリックで閉じること、誤った欄へのフォーカスを受け持つ。
/// 値の検証は DifficultyDialogViewModel にある。
/// </summary>
public partial class DifficultyDialogView : UserControl
{
    /// <summary>カスタムの見出し（「カスタム」）。</summary>
    public static string CustomTitle { get; } = DifficultyNames.Of(DifficultyKind.Custom);

    public DifficultyDialogView()
    {
        InitializeComponent();
        Backdrop.PointerPressed += (_, _) => Dialog?.Close();
    }

    DifficultyDialogViewModel? Dialog => DataContext as DifficultyDialogViewModel;

    /// <summary>
    /// 開いたときのフォーカスを置く。今の難易度の行に、カスタムのゲーム中なら「幅」の欄に置く（Web 版のクラス設計書 9.1 の決定 4）。
    /// 行と欄は表示してから作られるので、MainWindow が表示の後に呼ぶ。
    /// </summary>
    public void FocusFirst(NavigationMethod method)
    {
        if (Dialog is not { } dialog)
            return;
        if (dialog.IsCustomCurrent) {
            TextBoxOf(dialog.Width)?.Focus(method);
            return;
        }
        var index = dialog.Rows.ToList().FindIndex(row => row.IsCurrent);
        RowsControl.ContainerFromIndex(index)?.GetVisualDescendants().OfType<Button>().FirstOrDefault()?.Focus(method);
    }

    protected override void OnKeyDown(KeyEventArgs e)
    {
        base.OnKeyDown(e);
        if (e.Handled || e.Key != Key.Escape || Dialog is not { } dialog)
            return;
        e.Handled = true;
        dialog.Close();
    }

    // 誤りがあれば、最初の誤った欄にフォーカスを移す。フォーカスは画面の部品の仕事なので、ビューモデルは欄を返すだけにしている
    void OnStartCustomClick(object? sender, RoutedEventArgs e)
    {
        if (Dialog?.StartCustom() is { } invalidField)
            TextBoxOf(invalidField)?.Focus(NavigationMethod.Tab);
    }

    TextBox? TextBoxOf(CustomFieldViewModel field)
        => this.GetVisualDescendants().OfType<TextBox>().FirstOrDefault(textBox => textBox.DataContext == field);
}
