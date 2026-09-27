using Avalonia;
using Avalonia.Animation;
using Avalonia.Animation.Easings;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Media;
using Avalonia.Styling;
using Avalonia.VisualTree;
using Shos.Minesweeper.Desktop.ViewModels;
using Shos.Minesweeper.Presentation;
using PathShape = Avalonia.Controls.Shapes.Path;

namespace Shos.Minesweeper.Desktop.Views;

/// <summary>
/// 演出を、Avalonia のアニメーションとして組み立てる（クラス設計書 4.11）。動き（どの部品の、何を、どう変えるか）は、
/// Web 版の Components/BoardView.razor.css と WinCard.razor.css の keyframes のとおりで、長さと遅れは CellAnimationTimings にある。
/// 遅れは「CellAnimation.DelayRatio × 最大の遅れ」で、遅れの間は最初の見た目を保つ（Web 版の backwards）。
/// </summary>
public static class ViewAnimations
{
    // CSS の ease-out、ease-in と同じ曲線
    static readonly Easing EaseOut = new SplineEasing(0, 0, 0.58, 1);
    static readonly Easing EaseIn = new SplineEasing(0.42, 0, 1, 1);

    // 旗が跳ねる高さ（マスの 20%）
    const double FlagBounceHeightRatio = 0.2;

    /// <summary>
    /// 1 つのマスの演出と、その対象。対象は、未開放のタイルの覆い（cover）、数字（number）、アイコン（icon）のどれかである。
    /// 旗が広がる演出（直前に旗を立てた）と、BoardAnimation の演出（開く、爆発、現れる、跳ねる）を合わせて返す。
    /// </summary>
    public static IReadOnlyList<(Animation Animation, Animatable Target)> CellAnimationsOf(CellViewModel cell, Border cover, TextBlock number, Control icon, double cellSize)
    {
        var animations = new List<(Animation, Animatable)>();
        if (cell.IsFlagJustPlaced)
            animations.Add((ScaleFrom(0.6, CellAnimationTimings.FlagPlanted, TimeSpan.Zero), icon));
        if (cell.Animation is not { } animation)
            return animations;
        switch (animation.Kind) {
            case CellAnimationKind.Reveal:
                var revealDelay = DelayOf(animation, CellAnimationTimings.RevealMaxDelay);
                animations.Add((TileLeave(CellAnimationTimings.Reveal, revealDelay), cover));
                animations.Add((NumberAppear(CellAnimationTimings.Reveal, revealDelay), number));
                break;
            case CellAnimationKind.Explode:
                animations.Add((ScaleFrom(1.2, CellAnimationTimings.Explode, TimeSpan.Zero), icon));
                break;
            case CellAnimationKind.MineAppear:
                var appearDelay = DelayOf(animation, CellAnimationTimings.AppearMaxDelay);
                animations.Add((TileLeave(CellAnimationTimings.Appear, appearDelay), cover));
                animations.Add((ScaleFrom(0, CellAnimationTimings.Appear, appearDelay), icon));
                break;
            case CellAnimationKind.WrongFlagAppear:
                // × だけが広がって現れる。× はアイコンのテンプレートの中にあるので、テンプレートを当ててから探す
                if (CrossOf(icon) is { } cross)
                    animations.Add((ScaleFrom(0, CellAnimationTimings.Appear, DelayOf(animation, CellAnimationTimings.AppearMaxDelay)), cross));
                break;
            case CellAnimationKind.FlagBounce:
                animations.Add((FlagBounce(cellSize * FlagBounceHeightRatio, DelayOf(animation, CellAnimationTimings.FlagBounceMaxDelay)), icon));
                break;
        }
        return animations;
    }

    /// <summary>勝利カードが現れる（不透明度 0 から、150 ミリ秒）。</summary>
    public static Animation WinCardFadeIn()
        => AnimationOf(CellAnimationTimings.WinCardFadeIn, TimeSpan.Zero, EaseOut, FillMode.Backward,
                       Frame(0, [(Visual.OpacityProperty, 0.0)]), Frame(1, [(Visual.OpacityProperty, 1.0)]));

    static TimeSpan DelayOf(CellAnimation animation, TimeSpan maxDelay) => maxDelay * animation.DelayRatio;

    // 大きさを from から元の大きさに戻す（旗が広がる、爆発、現れる）
    static Animation ScaleFrom(double from, TimeSpan duration, TimeSpan delay)
        => AnimationOf(duration, delay, EaseOut, FillMode.Backward, Frame(0, Scale(from)), Frame(1, Scale(1)));

    // 覆いを縮めながら消す（Web 版の tile-leave）。消えた後は、演出を止めるまで消えたままにする（覆いは演出の間だけ出す）
    static Animation TileLeave(TimeSpan duration, TimeSpan delay)
        => AnimationOf(duration, delay, EaseIn, FillMode.Both,
                       Frame(0, [(Visual.OpacityProperty, 1.0), .. Scale(1)]),
                       Frame(1, [(Visual.OpacityProperty, 0.0), .. Scale(0.8)]));

    // 数字は変化の後半で現れる（Web 版の number-appear。前半は透明のまま）
    static Animation NumberAppear(TimeSpan duration, TimeSpan delay)
        => AnimationOf(duration, delay, new LinearEasing(), FillMode.Backward,
                       Frame(0, [(Visual.OpacityProperty, 0.0)]), Frame(0.5, [(Visual.OpacityProperty, 0.0)]), Frame(1, [(Visual.OpacityProperty, 1.0)]));

    // 上に跳ねて戻る。遅れの間は元の位置のまま（Web 版は backwards を付けていない）
    static Animation FlagBounce(double height, TimeSpan delay)
        => AnimationOf(CellAnimationTimings.FlagBounce, delay, EaseOut, FillMode.None,
                       Frame(0, [(TranslateTransform.YProperty, 0.0)]),
                       Frame(0.5, [(TranslateTransform.YProperty, -height)]),
                       Frame(1, [(TranslateTransform.YProperty, 0.0)]));

    static Animation AnimationOf(TimeSpan duration, TimeSpan delay, Easing easing, FillMode fillMode, params KeyFrame[] frames)
    {
        var animation = new Animation { Duration = duration, Delay = delay, Easing = easing, FillMode = fillMode };
        foreach (var frame in frames)
            animation.Children.Add(frame);
        return animation;
    }

    static KeyFrame Frame(double cue, IEnumerable<(AvaloniaProperty Property, object Value)> values)
    {
        var frame = new KeyFrame { Cue = new Cue(cue) };
        foreach (var (property, value) in values)
            frame.Setters.Add(new Setter(property, value));
        return frame;
    }

    // 大きさと位置は、ScaleTransform と TranslateTransform の値として変える。Avalonia は RenderTransform を丸ごと変えるアニメーションを持たず、
    // これらの値を変えるときは、対象の RenderTransform に変換を用意して変える
    static (AvaloniaProperty, object)[] Scale(double scale) => [(ScaleTransform.ScaleXProperty, scale), (ScaleTransform.ScaleYProperty, scale)];

    static PathShape? CrossOf(Control icon)
    {
        (icon as TemplatedControl)?.ApplyTemplate();
        return icon.GetVisualDescendants().OfType<PathShape>().FirstOrDefault(path => path.Classes.Contains("cross"));
    }
}
