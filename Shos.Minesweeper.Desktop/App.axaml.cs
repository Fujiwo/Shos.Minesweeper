using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using Shos.Minesweeper.Desktop.Views;

namespace Shos.Minesweeper.Desktop;

/// <summary>アプリの起動と組み立て（クラス設計書 4.12）。区切り 1 では、空のウィンドウを出すだけである。</summary>
public partial class App : Application
{
    public override void Initialize() => AvaloniaXamlLoader.Load(this);

    public override void OnFrameworkInitializationCompleted()
    {
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
            desktop.MainWindow = new MainWindow();
        base.OnFrameworkInitializationCompleted();
    }
}
