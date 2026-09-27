using Shos.Minesweeper.ConsoleApp;
using Shos.Minesweeper.ConsoleApp.Rendering;
using Shos.Minesweeper.ConsoleApp.Screens;
using Shos.Minesweeper.ConsoleApp.Terminal;
using Shos.Minesweeper.GameLogic;

// コンソール版の組み立て（クラス設計書 5.9）。ベストタイムは版ごとのフォルダーに置き、デスクトップ版と共有しない（仕様書 6.2）
var bestTimesPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                                 "Shos.Minesweeper", "ConsoleApp", "best-times.json");
// 環境変数 NO_COLOR が空でない値で設定されていたら、色を付けない（仕様書 5.6）
var usesColor = string.IsNullOrEmpty(Environment.GetEnvironmentVariable("NO_COLOR"));

// 例外で終わるときも、Dispose が端末を元に戻してから、例外が元の画面に出る（アーキテクチャー設計書 10 章）
using var terminal = TerminalSession.Open();
var game = new GameScreen(TimeProvider.System, new BestTimesFile(bestTimesPath));
GameLoop.Run(terminal, new ScreenNavigator(game), new FrameWriter(terminal.Output, usesColor));
