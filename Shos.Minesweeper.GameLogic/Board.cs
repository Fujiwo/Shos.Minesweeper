namespace Shos.Minesweeper.GameLogic;

public sealed class Board
{
    // マスごとの値は 1 次元の配列で持ち、位置から添字への変換は IndexOf だけで行う
    readonly CellState[] states;
    readonly bool[] mines;
    readonly int[] adjacentMineCounts;

    public int Width { get; }
    public int Height { get; }

    internal Board(int width, int height)
    {
        Width = width;
        Height = height;
        states = new CellState[width * height];
        mines = new bool[width * height];
        adjacentMineCounts = new int[width * height];
    }

    public Cell CellAt(CellPosition position)
    {
        var index = IndexOf(position);
        // 開いていないマスの数字を渡すと、地雷の位置を割り出せてしまう
        var adjacentMineCount = states[index] == CellState.Opened ? adjacentMineCounts[index] : 0;
        return new(states[index], adjacentMineCount);
    }

    /// <summary>コードで開く範囲。開いた数字のマスなら周囲の未開放のマス（旗でないもの）、そうでなければ空。</summary>
    public IReadOnlyList<CellPosition> ChordTargetsOf(CellPosition position)
        => CellAt(position).IsOpenedNumber
           ? [.. NeighborsOf(position).Where(neighbor => StateAt(neighbor) == CellState.Closed)]
           : [];

    internal bool HasOpenedMine { get; private set; }

    internal int FlagCount => states.Count(state => state == CellState.Flagged);

    internal bool AreAllSafeCellsOpened
        => Enumerable.Range(0, states.Length).All(index => mines[index] || states[index] == CellState.Opened);

    internal bool HasMineAt(CellPosition position) => mines[IndexOf(position)];

    internal IEnumerable<CellPosition> AllPositions
        => Enumerable.Range(0, Height).SelectMany(row => Enumerable.Range(0, Width).Select(column => new CellPosition(row, column)));

    internal IEnumerable<CellPosition> NeighborsOf(CellPosition position)
        => from rowOffset in Enumerable.Range(-1, 3)
           from columnOffset in Enumerable.Range(-1, 3)
           where (rowOffset, columnOffset) != (0, 0)
           let neighbor = new CellPosition(position.Row + rowOffset, position.Column + columnOffset)
           where Contains(neighbor)
           select neighbor;

    internal void PlaceMines(IReadOnlyCollection<CellPosition> positions)
    {
        foreach (var position in positions) {
            mines[IndexOf(position)] = true;
            foreach (var neighbor in NeighborsOf(position))
                adjacentMineCounts[IndexOf(neighbor)]++;
        }
    }

    /// <summary>「開く」（仕様書 3.4）。未開放なら開き、開いた数字のマスならコードを行う。それ以外は何もしない。</summary>
    internal void Open(CellPosition position)
    {
        if (CellAt(position).IsOpenedNumber)
            Chord(position);
        else
            OpenInChain([position]);
    }

    void Chord(CellPosition position)
    {
        var flagCount = NeighborsOf(position).Count(neighbor => StateAt(neighbor) == CellState.Flagged);
        if (flagCount == adjacentMineCounts[IndexOf(position)])
            OpenInChain(ChordTargetsOf(position));
    }

    // 0 のマスから周囲へ広げる。深い再帰にならないように待ち行列で広げる
    void OpenInChain(IEnumerable<CellPosition> positions)
    {
        var pending = new Queue<CellPosition>(positions);
        while (pending.TryDequeue(out var position)) {
            var index = IndexOf(position);
            if (states[index] != CellState.Closed)
                continue;
            states[index] = CellState.Opened;
            if (mines[index])
                HasOpenedMine = true;
            else if (adjacentMineCounts[index] == 0)
                foreach (var neighbor in NeighborsOf(position))
                    pending.Enqueue(neighbor);
        }
    }

    internal void ToggleFlag(CellPosition position)
    {
        var index = IndexOf(position);
        states[index] = states[index] switch {
            CellState.Closed  => CellState.Flagged,
            CellState.Flagged => CellState.Closed,
            var state         => state
        };
    }

    internal void FlagAllMines()
    {
        for (var index = 0; index < states.Length; index++)
            if (mines[index])
                states[index] = CellState.Flagged;
    }

    bool Contains(CellPosition position)
        => 0 <= position.Row && position.Row < Height && 0 <= position.Column && position.Column < Width;

    CellState StateAt(CellPosition position) => states[IndexOf(position)];

    int IndexOf(CellPosition position)
    {
        if (!Contains(position))
            throw new ArgumentOutOfRangeException(nameof(position), position, "盤面の外の位置です。");
        return position.Row * Width + position.Column;
    }
}
