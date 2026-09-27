using System.ComponentModel;
using Avalonia.Input;
using Microsoft.Extensions.Time.Testing;
using Shos.Minesweeper.Desktop.Platform;
using Shos.Minesweeper.Desktop.ViewModels;
using Shos.Minesweeper.GameLogic;
using Shos.Minesweeper.Presentation;
using Shos.Minesweeper.TestSupport;

namespace Shos.Minesweeper.Desktop.Tests.ViewModels;

/// <summary>
/// ゲームの画面のビューモデル（クラス設計書 4.3）。盤面の操作、ツールバー、勝敗、F2、経過時間、難易度ダイアログ、勝利カード、効果音、演出。
/// 音の出口とアニメーション効果の設定は、偽物を渡して確かめる（Windows の API と NAudio を呼ばない）。
/// </summary>
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
    readonly List<SoundEffect> played = [];
    readonly GameViewModel game;
    bool areAnimationEffectsEnabled = true;

    public GameViewModelTests() => game = NewGameViewModel();

    string BestTimesPath => Path.Combine(folder, "best-times.json");

    string SoundSettingPath => Path.Combine(folder, "sound.json");

    GameViewModel NewGameViewModel()
        => new(time, new BestTimesFile(BestTimesPath), new SoundSettingFile(SoundSettingPath), played.Add,
               () => areAnimationEffectsEnabled, TestGames.MineChooserOf(WallBoard));

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

    [Fact]
    public void WinningShowsTheWinCard()
    {
        var changed = Watch();

        Win(afterSeconds: 45);

        Assert.NotNull(game.WinCard);
        Assert.Equal("タイム 45 秒", game.WinCard.TimeText);
        Assert.Equal("ベストタイムを記録しました", game.WinCard.BestTimeText);
        Assert.True(game.WinCard.IsNewBest);
        Assert.Contains(nameof(GameViewModel.WinCard), changed);
    }

    [Fact]
    public void WinCardTellsTheBestTimeThatWasNotBeaten()
    {
        Win(afterSeconds: 45);
        game.NewGame();

        Win(afterSeconds: 60);

        Assert.Equal("ベスト 45 秒", game.WinCard!.BestTimeText);
        Assert.False(game.WinCard.IsNewBest);
    }

    [Fact]
    public void LosingShowsNoWinCard()
    {
        Open(new CellPosition(0, 0));

        Open(new CellPosition(0, 4));

        Assert.Null(game.WinCard);
    }

    [Fact]
    public void NewGameClosesTheWinCard()
    {
        Win(afterSeconds: 45);

        game.NewGame();

        Assert.Null(game.WinCard);
    }

    // カードを出していないときは、カードが閉じたとは知らせない（Views は、閉じたと知らされたときにフォーカスを移すかを決める）
    [Fact]
    public void NewGameWithoutTheWinCardDoesNotTellThatItClosed()
    {
        var changed = Watch();

        game.NewGame();

        Assert.DoesNotContain(nameof(GameViewModel.WinCard), changed);
    }

    // 閉じても、勝った盤面はそのまま残る
    [Fact]
    public void ClosingTheWinCardKeepsTheBoard()
    {
        Win(afterSeconds: 45);

        game.CloseWinCard();

        Assert.Null(game.WinCard);
        Assert.Equal(FaceKind.Won, game.Face);
    }

    [Fact]
    public void F2ClosesTheWinCard()
    {
        Win(afterSeconds: 45);

        game.HandleKey(Key.F2);

        Assert.Null(game.WinCard);
    }

    [Fact]
    public void OpeningTheDifficultyDialogShowsTheCurrentGame()
    {
        var changed = Watch();

        game.OpenDifficultyDialog();

        Assert.NotNull(game.DifficultyDialog);
        Assert.True(game.DifficultyDialog.Rows[0].IsCurrent);
        Assert.Contains(nameof(GameViewModel.DifficultyDialog), changed);
    }

    [Fact]
    public void DifficultyDialogTellsTheBestTimes()
    {
        Win(afterSeconds: 45);

        game.OpenDifficultyDialog();

        Assert.Equal("ベスト 45 秒", game.DifficultyDialog!.Rows[0].BestTimeText);
    }

    // F2 は、ダイアログの中にフォーカスを閉じ込めている間は効かない（UI デザイン 2.11）
    [Fact]
    public void F2DoesNothingWhileTheDifficultyDialogIsOpen()
    {
        Open(new CellPosition(0, 0));
        game.OpenDifficultyDialog();

        Assert.False(game.HandleKey(Key.F2));

        Assert.Equal(CellAppearance.Opened, game.Board.Cells[0].Appearance);
    }

    [Fact]
    public void SelectingADifficultyStartsItsGame()
    {
        var selectedCount = 0;
        game.DifficultySelected += () => selectedCount++;
        game.OpenDifficultyDialog();

        game.DifficultyDialog!.Select(game.DifficultyDialog.Rows[1]);

        Assert.Equal(Difficulty.Intermediate, game.Difficulty);
        Assert.Equal("中級", game.DifficultyName);
        Assert.Equal(16 * 16, game.Board.Cells.Count);
        Assert.Equal("新しいゲーム、中級、16×16、地雷 40。", game.Announcement);
        Assert.Null(game.DifficultyDialog);
        Assert.Equal(1, selectedCount);
    }

    // 今と同じ難易度を選んでも、新しいゲームを始め、ウィンドウを盤面に合わせ直す（Web 版の UI デザイン 8 章の決定 7）
    [Fact]
    public void SelectingTheSameDifficultyStartsANewGame()
    {
        var selectedCount = 0;
        game.DifficultySelected += () => selectedCount++;
        Open(new CellPosition(0, 0));
        game.OpenDifficultyDialog();

        game.DifficultyDialog!.Select(game.DifficultyDialog.Rows[0]);

        Assert.Equal(CellAppearance.Closed, game.Board.Cells[0].Appearance);
        Assert.Equal(1, selectedCount);
    }

    [Fact]
    public void StartingACustomGameUsesTheEnteredSize()
    {
        game.OpenDifficultyDialog();
        var dialog = game.DifficultyDialog!;
        dialog.Width.Text = "12";
        dialog.Height.Text = "6";
        dialog.MineCount.Text = "10";

        dialog.StartCustom();

        Assert.Equal(Difficulty.Custom(12, 6, 10), game.Difficulty);
        Assert.Equal(6, game.Board.RowCount);
        Assert.Equal(12, game.Board.ColumnCount);
    }

    // 閉じても、プレイ中のゲームはそのまま続く
    [Fact]
    public void ClosingTheDifficultyDialogKeepsTheGame()
    {
        Open(new CellPosition(0, 0));
        game.OpenDifficultyDialog();

        game.DifficultyDialog!.Close();

        Assert.Null(game.DifficultyDialog);
        Assert.Equal(CellAppearance.Opened, game.Board.Cells[0].Appearance);
    }

    // 勝利カードを出したまま難易度を選ぶと、カードも閉じる。
    // ダイアログを先に閉じたと知らせる。Views は、フォーカスを難易度 ボタンに戻してから、カードが消えたことを受けるので、
    // フォーカスをリセット ボタンに移さずに済む（クラス設計書 4.10）
    [Fact]
    public void SelectingADifficultyClosesTheDialogAndThenTheWinCard()
    {
        Win(afterSeconds: 45);
        game.OpenDifficultyDialog();
        var changed = Watch();

        game.DifficultyDialog!.Select(game.DifficultyDialog.Rows[0]);

        Assert.Null(game.WinCard);
        Assert.True(changed.IndexOf(nameof(GameViewModel.DifficultyDialog)) < changed.IndexOf(nameof(GameViewModel.WinCard)));
    }

    // 効果音（仕様書 4.6、UI デザイン 2.10）。鳴らす音を決めるのは GameSession と SoundEffectMapping で、ここはオンとオフだけを受け持つ（9.2 の A1）
    [Fact]
    public void SoundIsOnByDefault()
    {
        Assert.True(game.IsSoundEnabled);
        Assert.Equal("効果音（オン）", game.SoundEffectsToolTip);
    }

    [Fact]
    public void MovesPlayTheirSoundEffects()
    {
        Open(new CellPosition(0, 0));
        game.Board.PressRight(new CellPosition(5, 5));

        Assert.Equal([SoundEffect.Chain, SoundEffect.FlagPlaced], played);
    }

    [Fact]
    public void TurningTheSoundOffStopsTheSoundEffects()
    {
        var changed = Watch();

        game.ToggleSound();
        Open(new CellPosition(0, 0));

        Assert.False(game.IsSoundEnabled);
        Assert.Equal("効果音（オフ）", game.SoundEffectsToolTip);
        Assert.Empty(played);
        Assert.Contains(nameof(GameViewModel.IsSoundEnabled), changed);
        Assert.Contains(nameof(GameViewModel.SoundEffectsToolTip), changed);
    }

    // オンに戻しても、音は鳴らさない（UI デザイン 2.10）
    [Fact]
    public void TurningTheSoundOnAgainPlaysNothing()
    {
        game.ToggleSound();

        game.ToggleSound();

        Assert.True(game.IsSoundEnabled);
        Assert.Empty(played);
    }

    [Fact]
    public void SoundSettingIsSavedAndLoadedAtStartup()
    {
        game.ToggleSound();

        Assert.False(new SoundSettingFile(SoundSettingPath).Load());
        Assert.False(NewGameViewModel().IsSoundEnabled);
    }

    // 演出は、アニメーション効果の設定を操作のたびに読む（開いたまま OS の設定を変えても、次の演出から従う。UI デザイン 2.10）
    [Fact]
    public void MovesAreAnimatedWhenAnimationEffectsAreOn()
    {
        Open(new CellPosition(0, 0));

        Assert.NotNull(game.Board.Cells[0].Animation);
    }

    [Fact]
    public void MovesAreNotAnimatedWhenAnimationEffectsAreOff()
    {
        areAnimationEffectsEnabled = false;

        Open(new CellPosition(0, 0));

        Assert.Null(game.Board.Cells[0].Animation);
    }

    [Fact]
    public void ChangingTheAnimationSettingTakesEffectOnTheNextMove()
    {
        Open(new CellPosition(0, 0));

        areAnimationEffectsEnabled = false;
        game.Board.PressRight(new CellPosition(5, 5));

        Assert.False(game.Board.Cells[5 * 9 + 5].IsFlagJustPlaced);
    }

    // 勝利カードは、アニメーション効果がオンなら 150 ミリ秒で現れる（Web 版の WinCard.razor.css）
    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void WinCardFadesInOnlyWhenAnimationEffectsAreOn(bool isEnabled)
    {
        areAnimationEffectsEnabled = isEnabled;

        Win(afterSeconds: 45);

        Assert.Equal(isEnabled, game.WinCard!.FadesIn);
    }

    void Win(int afterSeconds)
    {
        Open(new CellPosition(0, 0));
        time.Advance(TimeSpan.FromSeconds(afterSeconds));
        Open(new CellPosition(0, 8));
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
