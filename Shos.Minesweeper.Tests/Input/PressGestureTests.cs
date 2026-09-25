using Microsoft.Extensions.Time.Testing;
using Shos.Minesweeper.Input;
using Shos.Minesweeper.Presentation;

namespace Shos.Minesweeper.Tests.Input;

/// <summary>押し方の判定（クラス設計書 4.2 の状態の表、仕様書 4.1、4.4）。</summary>
public class PressGestureTests
{
    const long MainButton = 0;
    const long MiddleButton = 1;
    const long SecondaryButton = 2;

    readonly FakeTimeProvider time = new();
    readonly List<PressKind> recognized = [];
    readonly PressGesture gesture;

    public PressGestureTests() => gesture = new PressGesture(time, recognized.Add);

    [Fact]
    public void MouseMainButtonPressStartsPressingWithoutWaitingForLongPress()
    {
        gesture.Down(Mouse(MainButton));

        Assert.True(gesture.IsPressing);
        Assert.False(gesture.IsWaitingForLongPress);
        Assert.Empty(recognized);
    }

    [Theory]
    [InlineData("touch")]
    [InlineData("pen")]
    public void TouchOrPenPressWaitsForLongPress(string pointerType)
    {
        gesture.Down(Pointer(pointerType));

        Assert.True(gesture.IsPressing);
        Assert.True(gesture.IsWaitingForLongPress);
    }

    [Fact]
    public void MouseSecondaryButtonIsRightClickImmediately()
    {
        gesture.Down(Mouse(SecondaryButton));

        Assert.Equal([PressKind.RightClick], recognized);
        Assert.False(gesture.IsPressing);
    }

    [Fact]
    public void OtherButtonsAreIgnored()
    {
        gesture.Down(Mouse(MiddleButton));

        Assert.False(gesture.IsPressing);
        Assert.Empty(recognized);
    }

    [Fact]
    public void ReleasingBeforeLongPressIsTap()
    {
        gesture.Down(Touch());
        time.Advance(TimeSpan.FromMilliseconds(399));

        gesture.Up(Touch());

        Assert.Equal([PressKind.Tap], recognized);
        Assert.False(gesture.IsPressing);
    }

    [Fact]
    public void HoldingTouchFor400MillisecondsIsLongPress()
    {
        gesture.Down(Touch());

        time.Advance(TimeSpan.FromMilliseconds(400));

        Assert.Equal([PressKind.LongPress], recognized);
        Assert.False(gesture.IsPressing);
    }

    [Fact]
    public void HoldingTouchFor399MillisecondsIsNotYetLongPress()
    {
        gesture.Down(Touch());

        time.Advance(TimeSpan.FromMilliseconds(399));

        Assert.Empty(recognized);
        Assert.True(gesture.IsPressing);
    }

    [Fact]
    public void ReleasingAfterLongPressDoesNothing()
    {
        gesture.Down(Touch());
        time.Advance(TimeSpan.FromMilliseconds(400));

        gesture.Up(Touch());

        Assert.Equal([PressKind.LongPress], recognized);
    }

    [Fact]
    public void HoldingMouseMainButtonIsStillTap()
    {
        gesture.Down(Mouse(MainButton));
        time.Advance(TimeSpan.FromSeconds(3));

        gesture.Up(Mouse(MainButton));

        Assert.Equal([PressKind.Tap], recognized);
    }

    [Fact]
    public void MovingLessThan10PixelsKeepsPressing()
    {
        gesture.Down(Touch(x: 100, y: 100));

        gesture.Move(Touch(x: 106, y: 107.9));   // 距離 9.9px

        Assert.True(gesture.IsPressing);
    }

    [Theory]
    [InlineData("touch")]
    [InlineData("mouse")]
    public void Moving10PixelsOrMoreCancelsThePress(string pointerType)
    {
        gesture.Down(Pointer(pointerType, x: 100, y: 100));

        gesture.Move(Pointer(pointerType, x: 106, y: 108));   // 距離 10px
        gesture.Up(Pointer(pointerType, x: 106, y: 108));
        time.Advance(TimeSpan.FromSeconds(1));

        Assert.False(gesture.IsPressing);
        Assert.Empty(recognized);
    }

    [Fact]
    public void CancelingBeforeLongPressRecognizesNothing()
    {
        gesture.Down(Touch());

        gesture.Cancel(Touch());
        time.Advance(TimeSpan.FromSeconds(1));
        gesture.Up(Touch());

        Assert.False(gesture.IsPressing);
        Assert.Empty(recognized);
    }

    [Fact]
    public void CancelingAfterLongPressReturnsToIdle()
    {
        gesture.Down(Touch());
        time.Advance(TimeSpan.FromMilliseconds(400));

        gesture.Cancel(Touch());
        gesture.Down(Touch());

        Assert.True(gesture.IsPressing);
    }

    [Fact]
    public void OtherPointersAreIgnoredWhilePressing()
    {
        gesture.Down(Touch(pointerId: 1));

        gesture.Down(Touch(pointerId: 2));
        gesture.Move(Touch(pointerId: 2, x: 500, y: 500));
        gesture.Cancel(Touch(pointerId: 2));
        gesture.Up(Touch(pointerId: 2));

        Assert.True(gesture.IsPressing);
        Assert.Empty(recognized);
    }

    [Fact]
    public void ResettingDiscardsThePressAndItsLongPressWait()
    {
        gesture.Down(Touch());

        gesture.Reset();
        time.Advance(TimeSpan.FromSeconds(1));

        Assert.False(gesture.IsPressing);
        Assert.Empty(recognized);
    }

    [Fact]
    public void DisposingStopsTheLongPressWait()
    {
        gesture.Down(Touch());

        gesture.Dispose();
        time.Advance(TimeSpan.FromSeconds(1));

        Assert.Empty(recognized);
    }

    static PointerInput Mouse(long button) => new(1, "mouse", button, 100, 100);

    static PointerInput Touch(long pointerId = 1, double x = 100, double y = 100) => new(pointerId, "touch", MainButton, x, y);

    static PointerInput Pointer(string pointerType, double x = 100, double y = 100) => new(1, pointerType, MainButton, x, y);
}
