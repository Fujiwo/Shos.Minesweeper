using System.ComponentModel;
using Shos.Minesweeper.Desktop.ViewModels;
using Shos.Minesweeper.GameLogic;

namespace Shos.Minesweeper.Desktop.Tests.ViewModels;

/// <summary>難易度ダイアログのビューモデル（クラス設計書 4.5）。</summary>
public sealed class DifficultyDialogViewModelTests
{
    readonly List<Difficulty> selected = [];
    int closedCount;

    DifficultyDialogViewModel DialogOf(Difficulty current, BestTimes? bestTimes = null)
        => new(current, bestTimes ?? new BestTimes(), selected.Add, () => closedCount++);

    [Fact]
    public void RowsAreTheThreePresets()
    {
        var dialog = DialogOf(Difficulty.Beginner);

        Assert.Equal([Difficulty.Beginner, Difficulty.Intermediate, Difficulty.Expert], dialog.Rows.Select(row => row.Difficulty));
        Assert.Equal(["初級", "中級", "上級"], dialog.Rows.Select(row => row.Name));
        Assert.Equal("9×9・地雷 10", dialog.Rows[0].SizeText);
    }

    [Fact]
    public void RowsTellTheBestTimes()
    {
        var bestTimes = new BestTimes(new Dictionary<DifficultyKind, int> { [DifficultyKind.Intermediate] = 80 });

        var dialog = DialogOf(Difficulty.Beginner, bestTimes);

        Assert.Equal(["記録なし", "ベスト 80 秒", "記録なし"], dialog.Rows.Select(row => row.BestTimeText));
    }

    // 現在の難易度は、チェックの印と、読み上げの状態で示す（色だけに頼らない）
    [Fact]
    public void TheCurrentDifficultyIsMarked()
    {
        var dialog = DialogOf(Difficulty.Intermediate);

        Assert.Equal([false, true, false], dialog.Rows.Select(row => row.IsCurrent));
        Assert.Equal([null, "現在の難易度", null], dialog.Rows.Select(row => row.ItemStatus));
        Assert.False(dialog.IsCustomCurrent);
    }

    [Fact]
    public void RowNameTellsAllItsTexts()
        => Assert.Equal("中級、16×16・地雷 40、記録なし", DialogOf(Difficulty.Beginner).Rows[1].AccessibleName);

    // カスタムのゲーム中は、どの行にも印を付けない。開いたときのフォーカスは「幅」の欄に置く（Web 版のクラス設計書 9.1 の決定 4）
    [Fact]
    public void CustomGameMarksNoRow()
    {
        var dialog = DialogOf(Difficulty.Custom(10, 8, 12));

        Assert.All(dialog.Rows, row => Assert.False(row.IsCurrent));
        Assert.True(dialog.IsCustomCurrent);
    }

    [Fact]
    public void SelectingARowSelectsItsDifficulty()
    {
        var dialog = DialogOf(Difficulty.Beginner);

        dialog.Select(dialog.Rows[2]);

        Assert.Equal([Difficulty.Expert], selected);
    }

    [Fact]
    public void CloseTellsTheOwner()
    {
        DialogOf(Difficulty.Beginner).Close();

        Assert.Equal(1, closedCount);
        Assert.Empty(selected);
    }

    // 入力欄の初期値は、今の盤面の値（Web 版の UI デザイン 8 章の決定 3）
    [Fact]
    public void FieldsStartWithTheCurrentBoard()
    {
        var dialog = DialogOf(Difficulty.Custom(10, 8, 12));

        Assert.Equal(["10", "8", "12"], [dialog.Width.Text, dialog.Height.Text, dialog.MineCount.Text]);
        Assert.Equal(["幅", "高さ", "地雷数"], [dialog.Width.Label, dialog.Height.Label, dialog.MineCount.Label]);
    }

    [Fact]
    public void FieldsTellTheirRanges()
    {
        var dialog = DialogOf(Difficulty.Beginner);

        Assert.Equal(["5〜30", "5〜24", "1〜72"], [dialog.Width.RangeText, dialog.Height.RangeText, dialog.MineCount.RangeText]);
    }

    [Fact]
    public void StartingAValidCustomGameSelectsIt()
    {
        var dialog = DialogOf(Difficulty.Beginner);
        dialog.Width.Text = "20";
        dialog.Height.Text = "10";
        dialog.MineCount.Text = "30";

        Assert.Null(dialog.StartCustom());

        Assert.Equal([Difficulty.Custom(20, 10, 30)], selected);
    }

    // 入力は、解釈する前に整える（前後の空白を除き、全角の数字も受け付ける。ユーザーの指示、2026-09-27）
    [Fact]
    public void FullWidthDigitsAndSurroundingSpacesAreAccepted()
    {
        var dialog = DialogOf(Difficulty.Beginner);
        dialog.Width.Text = " ２０ ";
        dialog.Height.Text = "１０　";
        dialog.MineCount.Text = "30";

        Assert.Null(dialog.StartCustom());

        Assert.Equal([Difficulty.Custom(20, 10, 30)], selected);
    }

    [Fact]
    public void InvalidFieldsAreMarkedAndTheFirstIsReturned()
    {
        var dialog = DialogOf(Difficulty.Beginner);
        dialog.Height.Text = "30";
        dialog.MineCount.Text = "abc";

        var first = dialog.StartCustom();

        Assert.Same(dialog.Height, first);
        Assert.Equal([false, true, true], [dialog.Width.IsInvalid, dialog.Height.IsInvalid, dialog.MineCount.IsInvalid]);
        Assert.Equal("5〜24 の整数を入力してください", dialog.Height.ErrorText);
        Assert.Null(dialog.Width.ErrorText);
        Assert.Empty(selected);
    }

    // 誤りを直しても、次に「カスタムで始める」を押すまでは、誤りの表示を残す（Web 版と同じ）
    [Fact]
    public void InvalidMarkIsKeptUntilTheNextStart()
    {
        var dialog = DialogOf(Difficulty.Beginner);
        dialog.Width.Text = "3";
        dialog.StartCustom();

        dialog.Width.Text = "10";

        Assert.True(dialog.Width.IsInvalid);
    }

    // 地雷数の上限は、入力中の幅と高さから求める。幅か高さが誤っていれば、式で示す（Web 版の UI デザイン 2.3）
    [Fact]
    public void MineCountRangeFollowsTheWidthAndHeight()
    {
        var dialog = DialogOf(Difficulty.Beginner);
        var changed = Watch(dialog.MineCount);

        dialog.Width.Text = "10";
        Assert.Equal("1〜81", dialog.MineCount.RangeText);
        Assert.Contains(nameof(CustomFieldViewModel.RangeText), changed);

        dialog.Height.Text = "x";
        Assert.Equal("1〜（幅×高さ − 9）", dialog.MineCount.RangeText);
    }

    [Fact]
    public void MineCountErrorFollowsTheRange()
    {
        var dialog = DialogOf(Difficulty.Beginner);
        dialog.MineCount.Text = "100";
        dialog.StartCustom();
        var changed = Watch(dialog.MineCount);

        dialog.Width.Text = "20";

        Assert.Equal("1〜171 の整数を入力してください", dialog.MineCount.ErrorText);
        Assert.Contains(nameof(CustomFieldViewModel.ErrorText), changed);
    }

    // 読み上げの補足は、欄の範囲。誤りがあれば、範囲を含む誤りの文にする（Web 版の aria-describedby に当たる）
    [Fact]
    public void HelpTextTellsTheRangeOrTheError()
    {
        var dialog = DialogOf(Difficulty.Beginner);
        Assert.Equal("5〜30", dialog.Width.HelpText);

        dialog.Width.Text = "50";
        dialog.StartCustom();

        Assert.Equal("5〜30 の整数を入力してください", dialog.Width.HelpText);
    }

    [Fact]
    public void ChangingTheTextIsNotified()
    {
        var dialog = DialogOf(Difficulty.Beginner);
        var changed = Watch(dialog.Width);

        dialog.Width.Text = "12";

        Assert.Equal(12, dialog.Width.Value);
        Assert.Contains(nameof(CustomFieldViewModel.Text), changed);
    }

    static List<string?> Watch(INotifyPropertyChanged source)
    {
        var changed = new List<string?>();
        source.PropertyChanged += (_, e) => changed.Add(e.PropertyName);
        return changed;
    }
}
