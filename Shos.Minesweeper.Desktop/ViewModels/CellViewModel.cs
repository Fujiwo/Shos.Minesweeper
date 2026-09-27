using System.ComponentModel;
using Shos.Minesweeper.GameLogic;
using Shos.Minesweeper.Presentation;

namespace Shos.Minesweeper.Desktop.ViewModels;

/// <summary>
/// 1 つのマスの見せ方（クラス設計書 4.4）。見せ方と数字と名前は、読むたびに今のゲームから求める（値を写して持たない）。
/// 値を変える操作は BoardViewModel だけが使う。
/// </summary>
public sealed class CellViewModel : INotifyPropertyChanged
{
    readonly GameSession session;

    internal CellViewModel(GameSession session, CellPosition position)
    {
        this.session = session;
        Position = position;
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    public CellPosition Position { get; }

    public CellAppearance Appearance => session.Game.AppearanceOf(Position);

    /// <summary>見せ方が開いたマスのときの数字（1〜8）。それ以外と、0 のマスは 0。</summary>
    public int Number => Appearance == CellAppearance.Opened ? AdjacentMineCount : 0;

    /// <summary>「3 行 5 列、未開放」。行と列は盤面の向きのまま（デスクトップ版は縦と横を入れ替えない）。</summary>
    public string AccessibleName => BoardNames.CellOf(Position.Row, Position.Column, Appearance, AdjacentMineCount);

    /// <summary>押下中の表示。</summary>
    public bool IsPressed { get; private set; }

    /// <summary>直前の操作で旗を立てたか（旗が広がる演出。クラス設計書 9.1 の決定 4）。</summary>
    public bool IsFlagJustPlaced { get; private set; }

    /// <summary>直前の操作の演出。演出しないマスは null。</summary>
    public CellAnimation? Animation { get; private set; }

    int AdjacentMineCount => session.Game.Board.CellAt(Position).AdjacentMineCount;

    /// <summary>ゲームが変わったことを知らせる（見せ方、数字、名前を読み直させる）。</summary>
    internal void Refresh() => Notify(nameof(Appearance), nameof(Number), nameof(AccessibleName));

    internal void SetPressed(bool isPressed)
    {
        if (IsPressed == isPressed)
            return;
        IsPressed = isPressed;
        Notify(nameof(IsPressed));
    }

    internal void SetFlagJustPlaced(bool isFlagJustPlaced)
    {
        if (IsFlagJustPlaced == isFlagJustPlaced)
            return;
        IsFlagJustPlaced = isFlagJustPlaced;
        Notify(nameof(IsFlagJustPlaced));
    }

    internal void SetAnimation(CellAnimation? animation)
    {
        if (Animation == animation)
            return;
        Animation = animation;
        Notify(nameof(Animation));
    }

    void Notify(params string[] propertyNames)
    {
        foreach (var name in propertyNames)
            PropertyChanged?.Invoke(this, new(name));
    }
}
