using System.ComponentModel;
using Shos.Minesweeper.GameLogic;
using Shos.Minesweeper.Presentation;

namespace Shos.Minesweeper.Desktop.ViewModels;

/// <summary>
/// 難易度ダイアログ（クラス設計書 4.5。Web 版の DifficultyDialog に当たる）。行とカスタムの入力欄を持ち、カスタムの値を確かめる。
/// 選んだ難易度と、閉じたことは、持ち主（GameViewModel）に返す。フォーカスの移動は Views が行う。
/// </summary>
public sealed class DifficultyDialogViewModel
{
    readonly Action<Difficulty> select;
    readonly Action close;

    public DifficultyDialogViewModel(Difficulty current, BestTimes bestTimes, Action<Difficulty> select, Action close)
    {
        this.select = select;
        this.close = close;
        Rows = [.. Difficulty.Presets.Select(preset => new DifficultyRowViewModel(preset, bestTimes.SecondsOf(preset.Kind), preset.Kind == current.Kind))];
        IsCustomCurrent = current.Kind == DifficultyKind.Custom;
        // 入力欄の初期値は、今の盤面の値（Web 版の UI デザイン 8 章の決定 3）
        Width = new(CustomDifficultyTexts.Width, current.Width, () => CustomDifficultyTexts.RangeOf(Difficulty.WidthRange));
        Height = new(CustomDifficultyTexts.Height, current.Height, () => CustomDifficultyTexts.RangeOf(Difficulty.HeightRange));
        MineCount = new(CustomDifficultyTexts.MineCount, current.MineCount, () => CustomDifficultyTexts.MineCountRangeOf(Width.Value, Height.Value));
        // 地雷数の上限は、入力中の幅と高さで決まる（Web 版の UI デザイン 2.3）
        Width.PropertyChanged += RefreshMineCountRange;
        Height.PropertyChanged += RefreshMineCountRange;
    }

    /// <summary>初級・中級・上級。</summary>
    public IReadOnlyList<DifficultyRowViewModel> Rows { get; }

    public CustomFieldViewModel Width { get; }

    public CustomFieldViewModel Height { get; }

    public CustomFieldViewModel MineCount { get; }

    /// <summary>今の難易度がカスタムか。開いたときのフォーカスを、行でなく「幅」の欄に置く（Web 版のクラス設計書 9.1 の決定 4）。</summary>
    public bool IsCustomCurrent { get; }

    public void Select(DifficultyRowViewModel row) => select(row.Difficulty);

    /// <summary>カスタムの値が正しければ、その難易度を選んで null を返す。誤りがあれば、欄に誤りを示し、最初の誤った欄を返す。</summary>
    public CustomFieldViewModel? StartCustom()
    {
        var validation = Difficulty.ValidateCustom(Width.Value, Height.Value, MineCount.Value);
        Width.IsInvalid = !validation.IsWidthValid;
        Height.IsInvalid = !validation.IsHeightValid;
        MineCount.IsInvalid = !validation.IsMineCountValid;
        if (!validation.IsValid)
            return new[] { Width, Height, MineCount }.First(field => field.IsInvalid);
        select(Difficulty.Custom(Width.Value!.Value, Height.Value!.Value, MineCount.Value!.Value));
        return null;
    }

    public void Close() => close();

    void RefreshMineCountRange(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(CustomFieldViewModel.Value))
            MineCount.RefreshRange();
    }
}
