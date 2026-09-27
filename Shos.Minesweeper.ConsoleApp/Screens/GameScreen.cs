using System.Globalization;
using Shos.Minesweeper.ConsoleApp.Rendering;
using Shos.Minesweeper.GameLogic;
using Shos.Minesweeper.Presentation;

namespace Shos.Minesweeper.ConsoleApp.Screens;

/// <summary>
/// ゲームの画面（仕様書 5.2〜5.4、UI デザイン 3.1〜3.6）。1 回のゲームの進め方、カーソル、ベストタイムを持ち、
/// 上の行、盤面、状態の行、キーの案内の行を描く。効果音は鳴らさない（音の出口を渡さない。CLAUDE.md の「目的」）。
/// </summary>
public sealed class GameScreen : IScreen
{
    public const string KeyGuide = "矢印/HJKL 移動  Space 開く  F 旗  N 新しいゲーム  D 難易度  ? ヘルプ  Q 終了";

    /// <summary>KeyGuide の表示の幅（升）。どの盤面の行（最大 63 升）よりも広いので、必要な端末の幅になる（UI デザイン 3.2）。</summary>
    public const int KeyGuideColumns = 76;

    const string NotStartedMessage = "マスを開くと始まります。";
    const string NewGameHint = "N で新しいゲーム。";
    static readonly TextStyle WinStyle = new(Foreground: ConsoleColor.Green);
    static readonly TextStyle LossStyle = new(Foreground: ConsoleColor.Red);

    readonly GameSession session;
    readonly BestTimesFile bestTimesFile;
    BoardCursor cursor = new();
    BestTimeResult winBestTime;

    public GameScreen(TimeProvider timeProvider, BestTimesFile bestTimesFile, MineChooser? chooseMines = null)
    {
        this.bestTimesFile = bestTimesFile;
        BestTimes = bestTimesFile.Load();
        session = new GameSession(Difficulty.Beginner, timeProvider, chooseMines: chooseMines);
    }

    /// <summary>今のゲームの難易度。</summary>
    public Difficulty Difficulty => Game.Difficulty;

    public BestTimes BestTimes { get; }

    Game Game => session.Game;

    /// <summary>新しいゲームを始める。カーソルは左上に戻る（Web 版と同じ。クラス設計書 9.1 の決定 9）。</summary>
    public void StartNewGame(Difficulty difficulty)
    {
        session.StartNewGame(difficulty);
        cursor = new BoardCursor();
    }

    public IScreen? HandleKey(ConsoleKeyInfo key)
    {
        // 勝敗が決まった後も、盤面を見て回れるようにカーソルは動かせる（UI デザイン 3.4）
        if (KeyboardMapping.DirectionFor(key) is { } direction) {
            cursor.Move(direction, Game.Board);
            return this;
        }
        switch (KeyboardMapping.CommandFor(key)) {
            case GameCommand.NewGame:
                StartNewGame(Difficulty);
                return this;
            case GameCommand.SelectDifficulty:
                return new DifficultySelectionScreen(this);
            case GameCommand.ShowHelp:
                return new HelpScreen(this);
            case GameCommand.Quit:
                return null;
        }
        if (!Game.IsOver)
            Act(KeyboardMapping.ActionFor(key));
        return this;
    }

    public Frame Render()
        => new([TopLine(), FrameLine.Of(""), .. BoardLines.Of(Game, cursor.Position), StatusLine(), FrameLine.Of(KeyGuide)]);

    void Act(CellAction action)
    {
        switch (action) {
            case CellAction.Open:
                if (session.Open(cursor.Position).Status == GameStatus.Won)
                    RecordBestTime();
                break;
            case CellAction.ToggleFlag:
                session.ToggleFlag(cursor.Position);
                break;
        }
    }

    // 保存できなくても、メモリーの記録は残る（仕様書 6.2）
    void RecordBestTime()
    {
        winBestTime = BestTimes.Record(Difficulty.Kind, Game.ElapsedSeconds);
        if (winBestTime.IsNewBest)
            bestTimesFile.Save(BestTimes);
    }

    // UI デザイン 3.3。数は右にそろえる（残り地雷は 4 升、経過時間は 3 升）。端末の言語によらずマイナスを「-」で書く
    FrameLine TopLine()
        => FrameLine.Of(string.Create(CultureInfo.InvariantCulture,
               $"{DifficultyNames.Of(Difficulty.Kind)} {Difficulty.Width}x{Difficulty.Height}   残り地雷{Game.RemainingMineCount,4}   経過時間 {Game.ElapsedSeconds,3} 秒"));

    // UI デザイン 3.5。勝利と敗北の文は、Web 版の読み上げの文と同じ（Presentation の Announcements）
    FrameLine StatusLine()
        => Game.Status switch {
            GameStatus.NotStarted => FrameLine.Of(NotStartedMessage),
            GameStatus.Won        => FrameLine.Of(Announcements.Won(Game.ElapsedSeconds, winBestTime) + NewGameHint, WinStyle),
            GameStatus.Lost       => FrameLine.Of(Announcements.Lost + NewGameHint, LossStyle),
            _                     => FrameLine.Of("")
        };
}
