using System.ComponentModel;
using Avalonia;
using Avalonia.Input;
using Shos.Minesweeper.Desktop.Input;
using Shos.Minesweeper.Desktop.Sizing;
using Shos.Minesweeper.GameLogic;
using Shos.Minesweeper.Presentation;

namespace Shos.Minesweeper.Desktop.ViewModels;

/// <summary>
/// 盤面の表示の状態（マス、カーソル、押下中、マスの大きさ）と、押し方とキーからの操作の意図（クラス設計書 4.4）。Web 版の BoardView に当たる。
/// 盤面を変えるのは GameViewModel で、操作の意図は requestAction で伝える。結果は Show と ShowNewGame で受け取る。
/// </summary>
public sealed class BoardViewModel : INotifyPropertyChanged
{
    readonly GameSession session;
    readonly Action<CellAction, CellPosition> requestAction;
    readonly Action pressingChanged;
    IReadOnlyList<CellViewModel> cells;
    BoardCursor cursor = new();
    CellPosition? pressedPosition;
    Size areaSize;

    public BoardViewModel(GameSession session, Action<CellAction, CellPosition> requestAction, Action pressingChanged)
    {
        this.session = session;
        this.requestAction = requestAction;
        this.pressingChanged = pressingChanged;
        cells = CellsOf(session);
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    /// <summary>行の順、行の中は列の順。</summary>
    public IReadOnlyList<CellViewModel> Cells => cells;

    public int RowCount => Board.Height;

    public int ColumnCount => Board.Width;

    /// <summary>「盤面、9 行 9 列」。</summary>
    public string AccessibleName => BoardNames.Of(RowCount, ColumnCount);

    /// <summary>20〜48。盤面の領域の大きさが分かるまでは、既定の 32。</summary>
    public int CellSize { get; private set; } = WindowSizing.DefaultCellSize;

    /// <summary>マスを並べた部分（枠を除く）の幅と高さ。</summary>
    public double CellsWidth => ColumnCount * CellSize;

    public double CellsHeight => RowCount * CellSize;

    /// <summary>カーソルのマス。キーボードのフォーカスを置くマスである（アーキテクチャー設計書 7.4）。</summary>
    public CellPosition CursorPosition => cursor.Position;

    public bool IsPressing => pressedPosition is not null;

    Board Board => session.Game.Board;

    /// <summary>盤面の領域（枠を含む）の大きさが変わった。</summary>
    public void SetAreaSize(Size size)
    {
        areaSize = size;
        UpdateCellSize();
    }

    /// <summary>
    /// 左ボタン（タッチ、ペン）を押した。カーソルを押したマスに移し、勝敗が決まっていなければ押下中にする。
    /// 押下中の表示の範囲は、未開放ならそのマス、開いた数字のマスならコードで開く範囲（Web 版の UI デザイン 5.1）。
    /// </summary>
    public void Press(CellPosition position)
    {
        MoveCursorTo(position);
        if (session.Game.IsOver)
            return;
        pressedPosition = position;
        foreach (var pressed in PressedPositionsOf(position))
            CellAt(pressed).SetPressed(true);
        Notify(nameof(IsPressing));
        pressingChanged();
    }

    /// <summary>離した。押したマスの上で離したときだけ、そのマスへの操作を伝える（外で離したら取り消す。クラス設計書 9.1 の決定 1）。</summary>
    public void Release(bool isOverPressedCell)
    {
        if (pressedPosition is not { } position)
            return;
        EndPress();
        if (isOverPressedCell)
            Request(PressMapping.ActionFor(PressKind.Tap, isFlagMode: false, Board.CellAt(position)), position);
    }

    /// <summary>押している間にポインターを失った。操作はしない。</summary>
    public void CancelPress()
    {
        if (pressedPosition is not null)
            EndPress();
    }

    /// <summary>右ボタンを押した。押した瞬間に旗を立てる・外す（Web 版と同じ）。</summary>
    public void PressRight(CellPosition position)
    {
        MoveCursorTo(position);
        if (!session.Game.IsOver)
            Request(PressMapping.ActionFor(PressKind.RightClick, isFlagMode: false, Board.CellAt(position)), position);
    }

    /// <summary>盤面で受けたキー。扱ったら true。矢印は勝敗が決まった後も動かせる（Web 版のクラス設計書 9.1 の決定 5）。</summary>
    public bool HandleKey(Key key)
    {
        if (KeyboardMapping.DirectionFor(key) is { } direction) {
            cursor.Move(direction, Board);
            Notify(nameof(CursorPosition));
            return true;
        }
        var action = KeyboardMapping.ActionFor(key);
        if (action == CellAction.None)
            return false;
        if (!session.Game.IsOver)
            requestAction(action, cursor.Position);
        return true;
    }

    /// <summary>
    /// 盤面の操作の結果を見せる。変わったマス（操作したマスと新たに開いたマス）だけを知らせ、描き直しを速く保つ
    /// （アーキテクチャー設計書 7.3）。勝敗が決まったら、地雷の表示と自動の旗が変わるので、全マスを知らせる。
    /// </summary>
    public void Show(MoveResult move)
    {
        if (move.Outcome == MoveOutcome.NoChange)
            return;
        if (session.Game.IsOver) {
            RefreshAll();
            return;
        }
        CellAt(move.Position).Refresh();
        foreach (var opened in move.OpenedPositions.Where(opened => opened != move.Position))
            CellAt(opened).Refresh();
    }

    /// <summary>新しいゲームの盤面を見せる。カーソルは左上に戻す（Web 版と同じ。クラス設計書 9.1 の決定 9）。</summary>
    public void ShowNewGame()
    {
        pressedPosition = null;
        cursor = new BoardCursor();
        if (HasTheSameShapeAsTheBoard(cells)) {
            foreach (var cell in cells) {
                cell.SetPressed(false);
                cell.Refresh();
            }
        } else {
            cells = CellsOf(session);
            Notify(nameof(Cells), nameof(RowCount), nameof(ColumnCount), nameof(AccessibleName));
        }
        UpdateCellSize();
        Notify(nameof(CursorPosition), nameof(IsPressing));
    }

    void EndPress()
    {
        pressedPosition = null;
        foreach (var cell in cells)
            cell.SetPressed(false);
        Notify(nameof(IsPressing));
        pressingChanged();
    }

    void Request(CellAction action, CellPosition position)
    {
        if (action != CellAction.None)
            requestAction(action, position);
    }

    void MoveCursorTo(CellPosition position)
    {
        cursor.MoveTo(position, Board);
        Notify(nameof(CursorPosition));
    }

    IEnumerable<CellPosition> PressedPositionsOf(CellPosition position)
        => Board.CellAt(position).State == CellState.Closed ? [position] : Board.ChordTargetsOf(position);

    void RefreshAll()
    {
        foreach (var cell in cells)
            cell.Refresh();
    }

    // 盤面の領域の大きさが分かるまで（0 × 0）は、既定の大きさにする
    void UpdateCellSize()
    {
        CellSize = areaSize.Width > 0 && areaSize.Height > 0
                   ? WindowSizing.CellSizeToFit(areaSize, session.Game.Difficulty)
                   : WindowSizing.DefaultCellSize;
        Notify(nameof(CellSize), nameof(CellsWidth), nameof(CellsHeight));
    }

    // 最後のマスの位置（高さ − 1, 幅 − 1）が同じなら、幅と高さが同じである。同じなら、マスを作り直さずに使い続ける
    bool HasTheSameShapeAsTheBoard(IReadOnlyList<CellViewModel> shownCells)
        => shownCells[^1].Position == new CellPosition(Board.Height - 1, Board.Width - 1);

    // マスは 1 次元の並びで持ち、位置からマスを引く計算はここだけに置く
    CellViewModel CellAt(CellPosition position) => cells[position.Row * ColumnCount + position.Column];

    static IReadOnlyList<CellViewModel> CellsOf(GameSession session)
        => [.. session.Game.Board.AllPositions.Select(position => new CellViewModel(session, position))];

    void Notify(params string[] propertyNames)
    {
        foreach (var name in propertyNames)
            PropertyChanged?.Invoke(this, new(name));
    }
}
