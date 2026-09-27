using System.ComponentModel;
using Avalonia.Input;
using Microsoft.Extensions.Time.Testing;
using Shos.Minesweeper.Desktop.ViewModels;
using Shos.Minesweeper.GameLogic;
using Shos.Minesweeper.TestSupport;

namespace Shos.Minesweeper.Desktop.Tests.ViewModels;

/// <summary>ゲームの画面のビューモデル（クラス設計書 4.3）。区切り 5 の分（盤面の操作、ツールバー、勝敗、F2、経過時間）。</summary>
public sealed class GameViewModelTests : IDisposable
{
    // 左上を開くと左側が、右上を開くと右側が開いて勝つ。(0, 4) は地雷
    const string WallBoard = """
        ....*....
        ....*....
        ....*....
        ....*....
        ....*....
        ....*....
        ....*....
        ....*....
        ....*...*
        """;

    readonly FakeTimeProvider time = new();
    readonly string folder = Path.Combine(Path.GetTempPath(), "Shos.Minesweeper.Tests", Guid.NewGuid().ToString("N"));
    readonly GameViewModel game;

    public GameViewModelTests()
        => game = new GameViewModel(time, new BestTimesFile(BestTimesPath), TestGames.MineChooserOf(WallBoard));

    string BestTimesPath => Path.Combine(folder, "best-times.json");

    public void Dispose()
    {
        if (Directory.Exists(folder))
            Directory.Delete(folder, recursive: true);
    }

    [Fact]
    public void ToolbarShowsTheBeginnerGame()
    {
        Assert.Equal("初級", game.DifficultyName);
        Assert.Equal("難易度、初級", game.DifficultyButtonName);
        Assert.Equal(10, game.RemainingMineCount);
        Assert.Equal("残り地雷 10", game.RemainingMinesName);
        Assert.Equal(0, game.ElapsedSeconds);
        Assert.Equal("経過時間 0 秒", game.ElapsedTimeName);
        Assert.Equal(FaceKind.Normal, game.Face);
    }

    // 起動したときは、新しいゲームを読み上げない（Web 版のクラス設計書 9.1 の決定 7）
    [Fact]
    public void NothingIsAnnouncedAtStartup()
        => Assert.Equal("", game.Announcement);

    [Fact]
    public void PlacingAFlagDecreasesTheRemainingMines()
    {
        var changed = Watch();

        game.Board.PressRight(new CellPosition(2, 2));

        Assert.Equal(9, game.RemainingMineCount);
        Assert.Contains(nameof(GameViewModel.RemainingMineCount), changed);
        Assert.Contains(nameof(GameViewModel.RemainingMinesName), changed);
    }

    [Fact]
    public void OpeningACellShowsItOnTheBoard()
    {
        Open(new CellPosition(0, 0));

        Assert.Equal(CellAppearance.Opened, game.Board.Cells[0].Appearance);
    }

    [Fact]
    public void FaceIsSurprisedWhilePressing()
    {
        var changed = Watch();

        game.Board.Press(new CellPosition(2, 2));

        Assert.Equal(FaceKind.Surprised, game.Face);
        Assert.Contains(nameof(GameViewModel.Face), changed);
    }

    [Fact]
    public void LosingIsAnnouncedAndTheFaceIsLost()
    {
        Open(new CellPosition(0, 0));

        Open(new CellPosition(0, 4));

        Assert.Equal("ゲームオーバー。地雷を開きました。", game.Announcement);
        Assert.Equal(FaceKind.Lost, game.Face);
    }

    [Fact]
    public void WinningIsAnnouncedAndTheBestTimeIsSaved()
    {
        Open(new CellPosition(0, 0));
        time.Advance(TimeSpan.FromSeconds(45));

        Open(new CellPosition(0, 8));

        Assert.Equal("クリア。45 秒。ベストタイムを記録しました。", game.Announcement);
        Assert.Equal(FaceKind.Won, game.Face);
        Assert.Equal(45, new BestTimesFile(BestTimesPath).Load().SecondsOf(DifficultyKind.Beginner));
    }

    [Fact]
    public void NewGameStartsOverAndIsAnnounced()
    {
        Open(new CellPosition(0, 0));

        game.NewGame();

        Assert.Equal(CellAppearance.Closed, game.Board.Cells[0].Appearance);
        Assert.Equal("新しいゲーム、初級、9×9、地雷 10。", game.Announcement);
    }

    // 同じ文を続けて知らせるときは、中身を変えて、ライブ リージョンに読ませる（Web 版と同じ）
    [Fact]
    public void RepeatedAnnouncementIsChangedSoThatItIsReadAgain()
    {
        game.NewGame();
        var first = game.Announcement;

        game.NewGame();

        Assert.NotEqual(first, game.Announcement);
        Assert.StartsWith(first, game.Announcement);
    }

    [Fact]
    public void F2StartsANewGame()
    {
        Open(new CellPosition(0, 0));

        Assert.True(game.HandleKey(Key.F2));

        Assert.Equal(CellAppearance.Closed, game.Board.Cells[0].Appearance);
    }

    [Fact]
    public void OtherKeysAreNotHandledByTheWindow()
        => Assert.False(game.HandleKey(Key.F3));

    // 経過時間の表示は、Views のタイマーが確かめるたびに、秒が変わったときだけ知らせる（アーキテクチャー設計書 7.3）
    [Fact]
    public void ElapsedTimeIsNotifiedOnlyWhenTheSecondChanges()
    {
        Open(new CellPosition(0, 0));
        var changed = Watch();

        time.Advance(TimeSpan.FromMilliseconds(250));
        game.UpdateElapsedTime();
        Assert.DoesNotContain(nameof(GameViewModel.ElapsedSeconds), changed);

        time.Advance(TimeSpan.FromMilliseconds(750));
        game.UpdateElapsedTime();
        Assert.Equal(1, game.ElapsedSeconds);
        Assert.Equal("経過時間 1 秒", game.ElapsedTimeName);
        Assert.Contains(nameof(GameViewModel.ElapsedSeconds), changed);
    }

    // 盤面を操作したときも、表示している秒を今の値にそろえる
    [Fact]
    public void ElapsedTimeIsRefreshedByMoves()
    {
        Open(new CellPosition(0, 0));
        time.Advance(TimeSpan.FromSeconds(3));

        game.Board.PressRight(new CellPosition(5, 5));

        Assert.Equal(3, game.ElapsedSeconds);
    }

    void Open(CellPosition position)
    {
        game.Board.Press(position);
        game.Board.Release(isOverPressedCell: true);
    }

    List<string?> Watch()
    {
        var changed = new List<string?>();
        ((INotifyPropertyChanged)game).PropertyChanged += (_, e) => changed.Add(e.PropertyName);
        return changed;
    }
}
