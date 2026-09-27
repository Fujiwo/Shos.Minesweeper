using System.ComponentModel;
using Avalonia.Input;
using Shos.Minesweeper.Desktop.Input;
using Shos.Minesweeper.GameLogic;
using Shos.Minesweeper.Presentation;

namespace Shos.Minesweeper.Desktop.ViewModels;

/// <summary>
/// ゲームの画面のビューモデル（クラス設計書 4.3）。画面全体の状態の持ち主で、Web 版の GamePage に当たる。
/// 盤面の操作を GameSession に伝え、勝敗、ベストタイム、読み上げ、ツールバーの値を受け持つ。判断は各部品に任せ、つなぐ。
/// 難易度ダイアログと勝利カードは区切り 6、効果音と演出は区切り 7 で加える。
/// </summary>
public sealed class GameViewModel : INotifyPropertyChanged
{
    // 同じ文を続けて知らせるときに、中身を変えるための見えない文字（中身が変わらないと、ライブ リージョンは読み上げない）
    const string ZeroWidthSpace = "​";

    readonly GameSession session;
    readonly BestTimesFile bestTimesFile;
    readonly BestTimes bestTimes;
    int shownElapsedSeconds;

    public GameViewModel(TimeProvider timeProvider, BestTimesFile bestTimesFile, MineChooser? chooseMines = null)
    {
        this.bestTimesFile = bestTimesFile;
        bestTimes = bestTimesFile.Load();
        session = new GameSession(Difficulty.Beginner, timeProvider, chooseMines: chooseMines);
        Board = new BoardViewModel(session, Request, () => Notify(nameof(Face)));
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    public BoardViewModel Board { get; }

    /// <summary>今のゲームの難易度。Views がウィンドウの大きさを決めるのに使う。</summary>
    public Difficulty Difficulty => Game.Difficulty;

    public string DifficultyName => DifficultyNames.Of(Difficulty.Kind);

    public string DifficultyButtonName => ToolbarTexts.DifficultyButtonNameOf(Difficulty.Kind);

    public int RemainingMineCount => Game.RemainingMineCount;

    public string RemainingMinesName => ToolbarTexts.RemainingMinesOf(RemainingMineCount);

    /// <summary>表示している秒。UpdateElapsedTime と盤面の操作で、今の値にそろえる。</summary>
    public int ElapsedSeconds => shownElapsedSeconds;

    public string ElapsedTimeName => ToolbarTexts.ElapsedTimeOf(ElapsedSeconds);

    /// <summary>顔の表情（Web 版の Toolbar と同じ規則）。勝敗が決まった後は、押していても驚かない。</summary>
    public FaceKind Face
        => Game.Status switch {
            GameStatus.Won  => FaceKind.Won,
            GameStatus.Lost => FaceKind.Lost,
            _               => Board.IsPressing ? FaceKind.Surprised : FaceKind.Normal
        };

    /// <summary>ライブ リージョンの文（勝利、敗北、新しいゲーム）。起動したときは空。</summary>
    public string Announcement { get; private set; } = "";

    Game Game => session.Game;

    /// <summary>同じ難易度で新しいゲームを始める（リセット ボタン、F2）。</summary>
    public void NewGame()
    {
        session.StartNewGame(Difficulty);
        Board.ShowNewGame();
        Announce(Announcements.NewGame(Difficulty));
        NotifyToolbar();
    }

    /// <summary>ウィンドウで受けたキー。扱ったら true。</summary>
    public bool HandleKey(Key key)
    {
        if (!KeyboardMapping.IsNewGameKey(key))
            return false;
        NewGame();
        return true;
    }

    /// <summary>Views のタイマーが 250 ミリ秒ごとに呼ぶ。秒が変わったときだけ知らせる（Web 版の ElapsedTime と同じ考え方）。</summary>
    public void UpdateElapsedTime()
    {
        if (Game.ElapsedSeconds == shownElapsedSeconds)
            return;
        shownElapsedSeconds = Game.ElapsedSeconds;
        Notify(nameof(ElapsedSeconds), nameof(ElapsedTimeName));
    }

    void Request(CellAction action, CellPosition position)
    {
        var move = action == CellAction.Open ? session.Open(position) : session.ToggleFlag(position);
        Board.Show(move);
        if (move.Status == GameStatus.Won)
            Win();
        else if (move.Status == GameStatus.Lost)
            Announce(Announcements.Lost);
        NotifyToolbar();
    }

    // 保存できなくても、メモリーの記録は残る（仕様書 6.2）
    void Win()
    {
        var result = bestTimes.Record(Difficulty.Kind, Game.ElapsedSeconds);
        if (result.IsNewBest)
            bestTimesFile.Save(bestTimes);
        Announce(Announcements.Won(Game.ElapsedSeconds, result));
    }

    void Announce(string text)
    {
        Announcement = Announcement == text ? text + ZeroWidthSpace : text;
        Notify(nameof(Announcement));
    }

    void NotifyToolbar()
    {
        shownElapsedSeconds = Game.ElapsedSeconds;
        Notify(nameof(Difficulty), nameof(DifficultyName), nameof(DifficultyButtonName), nameof(RemainingMineCount), nameof(RemainingMinesName),
               nameof(ElapsedSeconds), nameof(ElapsedTimeName), nameof(Face));
    }

    void Notify(params string[] propertyNames)
    {
        foreach (var name in propertyNames)
            PropertyChanged?.Invoke(this, new(name));
    }
}
