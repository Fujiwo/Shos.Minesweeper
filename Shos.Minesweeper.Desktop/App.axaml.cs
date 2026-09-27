using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using Shos.Minesweeper.Desktop.Platform;
using Shos.Minesweeper.Desktop.ViewModels;
using Shos.Minesweeper.Desktop.Views;
using Shos.Minesweeper.GameLogic;

namespace Shos.Minesweeper.Desktop;

/// <summary>アプリの起動と組み立て（クラス設計書 4.12）。DI のコンテナーは使わず、ここで手で渡す（アーキテクチャー設計書 7.7）。</summary>
public partial class App : Application
{
    public override void Initialize() => AvaloniaXamlLoader.Load(this);

    public override void OnFrameworkInitializationCompleted()
    {
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop) {
            var viewModel = new GameViewModel(TimeProvider.System, new BestTimesFile(DataFilePaths.BestTimes));
            desktop.MainWindow = new MainWindow { DataContext = viewModel };
        }
        base.OnFrameworkInitializationCompleted();
    }
}
