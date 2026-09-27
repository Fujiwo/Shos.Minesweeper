using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using Avalonia.Threading;
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
            var soundEffectPlayer = new SoundEffectPlayer();
            var viewModel = new GameViewModel(TimeProvider.System,
                                              new BestTimesFile(DataFilePaths.BestTimes),
                                              new SoundSettingFile(DataFilePaths.SoundSetting),
                                              soundEffectPlayer.Play,
                                              AnimationEffects.AreEnabled);
            var window = new MainWindow { DataContext = viewModel };
            // 効果音の準備（波形の合成と音の出力を開くこと）は、最初の表示を遅らせないように、ウィンドウを表示した後に行う（アーキ 7.2）
            window.Opened += (_, _) => Dispatcher.UIThread.Post(soundEffectPlayer.Prepare, DispatcherPriority.Background);
            desktop.Exit += (_, _) => soundEffectPlayer.Dispose();
            desktop.MainWindow = window;
        }
        base.OnFrameworkInitializationCompleted();
    }
}
