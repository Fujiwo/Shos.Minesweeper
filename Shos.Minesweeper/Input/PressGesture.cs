using Shos.Minesweeper.Presentation;

namespace Shos.Minesweeper.Input;

/// <summary>1 回の「押して離す」を、タップ・長押し・右クリックに判定する（仕様書 4.1、4.4）。</summary>
public sealed class PressGesture : IDisposable
{
    public static readonly TimeSpan LongPressDelay = TimeSpan.FromMilliseconds(400);
    public const double CancelDistance = 10;

    const long MainButton = 0;
    const long SecondaryButton = 2;

    enum State { Idle, Pressing, LongPressed }

    readonly TimeProvider timeProvider;
    readonly Action<PressKind> recognized;
    State state = State.Idle;
    PointerInput start;
    ITimer? longPressTimer;
    int pressNumber;

    public PressGesture(TimeProvider timeProvider, Action<PressKind> recognized)
    {
        this.timeProvider = timeProvider;
        this.recognized = recognized;
    }

    /// <summary>押下中（長押しの成立前）。押下中の表示に使う。</summary>
    public bool IsPressing => state == State.Pressing;

    /// <summary>タッチかペンで押下中。長押しの円に使う。</summary>
    public bool IsWaitingForLongPress => IsPressing && longPressTimer is not null;

    public void Down(PointerInput input)
    {
        // 押している間に別の指で触れても、最初のポインターだけを追う
        if (state != State.Idle)
            return;
        if (IsMouse(input) && input.Button == SecondaryButton) {
            recognized(PressKind.RightClick);
            return;
        }
        if (input.Button != MainButton)
            return;

        start = input;
        state = State.Pressing;
        // マウスには長押しがない（仕様書 4.1）
        if (!IsMouse(input))
            StartLongPressWait();
    }

    public void Move(PointerInput input)
    {
        if (IsPressing && IsTracked(input) && DistanceFromStart(input) >= CancelDistance)
            Reset();
    }

    public void Up(PointerInput input)
    {
        if (!IsTracked(input))
            return;
        var wasPressing = IsPressing;
        Reset();
        // 長押しが成立した後は、離してもタップにしない（二重に操作しない。仕様書 4.1）
        if (wasPressing)
            recognized(PressKind.Tap);
    }

    public void Cancel(PointerInput input)
    {
        if (IsTracked(input))
            Reset();
    }

    /// <summary>追っている押下を捨てる。</summary>
    public void Reset()
    {
        StopLongPressWait();
        state = State.Idle;
    }

    public void Dispose() => Reset();

    void StartLongPressWait()
    {
        // 前の押下のタイマーの呼び出しが遅れて届いても、今の押下の長押しとして扱わないように、押下に番号を付ける
        var number = ++pressNumber;
        longPressTimer = timeProvider.CreateTimer(_ => OnLongPressElapsed(number), null, LongPressDelay, Timeout.InfiniteTimeSpan);
    }

    void StopLongPressWait()
    {
        longPressTimer?.Dispose();
        longPressTimer = null;
    }

    void OnLongPressElapsed(int number)
    {
        if (!IsPressing || number != pressNumber)
            return;
        StopLongPressWait();
        state = State.LongPressed;
        recognized(PressKind.LongPress);
    }

    bool IsTracked(PointerInput input) => state != State.Idle && input.PointerId == start.PointerId;

    double DistanceFromStart(PointerInput input)
        => Math.Sqrt(Math.Pow(input.ClientX - start.ClientX, 2) + Math.Pow(input.ClientY - start.ClientY, 2));

    static bool IsMouse(PointerInput input) => input.PointerType == "mouse";
}
