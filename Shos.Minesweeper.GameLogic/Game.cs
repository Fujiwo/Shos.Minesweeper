namespace Shos.Minesweeper.GameLogic;

public sealed class Game
{
    public const int MaxElapsedSeconds = 999;

    readonly TimeProvider timeProvider;
    readonly MineChooser chooseMines;
    long startTimestamp;
    long? endTimestamp;

    public Difficulty Difficulty { get; }
    public Board Board { get; }
    public GameStatus Status { get; private set; } = GameStatus.NotStarted;
    public bool IsOver => Status is GameStatus.Won or GameStatus.Lost;
    public int RemainingMineCount => Difficulty.MineCount - Board.FlagCount;
    /// <summary>最初に開いてからの秒数（切り捨て）。勝敗が決まったら止まり、999 で止まる（仕様書 3.7）。</summary>
    public int ElapsedSeconds
    {
        get {
            if (Status == GameStatus.NotStarted)
                return 0;
            // 時計の時刻ではなく経過を測る値の差で求めるので、端末の時計を変えられても狂わない
            var elapsed = timeProvider.GetElapsedTime(startTimestamp, endTimestamp ?? timeProvider.GetTimestamp());
            return (int)Math.Min(elapsed.TotalSeconds, MaxElapsedSeconds);
        }
    }

    public Game(Difficulty difficulty, TimeProvider timeProvider, MineChooser? chooseMines = null)
    {
        Difficulty = difficulty;
        Board = new Board(difficulty.Width, difficulty.Height);
        this.timeProvider = timeProvider;
        this.chooseMines = chooseMines ?? ChooseMinesRandomly;
    }

    /// <summary>マスの見せ方。敗北した後は、地雷の位置と誤った旗も見せる（仕様書 3.6）。</summary>
    public CellAppearance AppearanceOf(CellPosition position)
        => (Board.CellAt(position).State, Board.HasMineAt(position), Status == GameStatus.Lost) switch {
            (CellState.Opened,  true,  _)    => CellAppearance.ExplodedMine,
            (CellState.Opened,  false, _)    => CellAppearance.Opened,
            (CellState.Flagged, false, true) => CellAppearance.WrongFlag,
            (CellState.Flagged, _,     _)    => CellAppearance.Flagged,
            (CellState.Closed,  true,  true) => CellAppearance.Mine,
            _                                => CellAppearance.Closed
        };

    public void Open(CellPosition position)
    {
        ThrowIfOver();
        if (!Board.CellAt(position).CanOpen)
            return;
        if (Status == GameStatus.NotStarted)
            Start(position);
        Board.Open(position);
        if (Board.HasOpenedMine)
            End(GameStatus.Lost);
        else if (Board.AreAllSafeCellsOpened)
            Win();
    }

    public void ToggleFlag(CellPosition position)
    {
        ThrowIfOver();
        Board.ToggleFlag(position);
    }

    /// <summary>本番の地雷の選び方。候補から一様ランダムに選ぶ（仕様書 3.3）。</summary>
    public static IReadOnlyCollection<CellPosition> ChooseMinesRandomly(IReadOnlyList<CellPosition> candidates, int mineCount)
    {
        ArgumentOutOfRangeException.ThrowIfGreaterThan(mineCount, candidates.Count);
        var shuffled = candidates.ToArray();
        Random.Shared.Shuffle(shuffled);
        return shuffled[..mineCount];
    }

    // 勝敗が決まった後の盤面の操作は受け付けない（仕様書 3.2）。UI はこの後に操作を送らない
    void ThrowIfOver()
    {
        if (IsOver)
            throw new InvalidOperationException("勝敗が決まった後は、盤面を操作できません。");
    }

    void Start(CellPosition firstPosition)
    {
        // 最初に開いたマスとその周囲には地雷を置かない（仕様書 3.3）
        var safePositions = Board.NeighborsOf(firstPosition).Append(firstPosition).ToHashSet();
        var candidates = Board.AllPositions.Where(position => !safePositions.Contains(position)).ToArray();
        var mines = chooseMines(candidates, Difficulty.MineCount);
        ThrowIfInvalidMines(mines, candidates);
        Board.PlaceMines(mines);
        startTimestamp = timeProvider.GetTimestamp();
        Status = GameStatus.Playing;
    }

    void ThrowIfInvalidMines(IReadOnlyCollection<CellPosition> mines, IReadOnlyCollection<CellPosition> candidates)
    {
        var isValid = mines.Count == Difficulty.MineCount
                      && mines.Distinct().Count() == mines.Count
                      && mines.All(candidates.Contains);
        if (!isValid)
            throw new InvalidOperationException("地雷は、候補の中から重ならないように地雷数だけ選んでください。");
    }

    void Win()
    {
        Board.FlagAllMines();   // 仕様書 3.6
        End(GameStatus.Won);
    }

    void End(GameStatus status)
    {
        endTimestamp = timeProvider.GetTimestamp();
        Status = status;
    }
}
