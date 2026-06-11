using System.Windows;
using System.Windows.Media.Animation;

namespace Mine.Wpf.Controls.Animations;

/// <summary>
/// CornerRadius 平滑动画
/// </summary>
public class CornerRadiusAnimation : AnimationTimeline
{
    public static readonly DependencyProperty FromProperty =
        DependencyProperty.Register(nameof(From), typeof(CornerRadius?), typeof(CornerRadiusAnimation));

    public static readonly DependencyProperty ToProperty =
        DependencyProperty.Register(nameof(To), typeof(CornerRadius?), typeof(CornerRadiusAnimation));

    public CornerRadius? From
    {
        get => (CornerRadius?)GetValue(FromProperty);
        set => SetValue(FromProperty, value);
    }

    public CornerRadius? To
    {
        get => (CornerRadius?)GetValue(ToProperty);
        set => SetValue(ToProperty, value);
    }

    public override Type TargetPropertyType => typeof(CornerRadius);

    protected override Freezable CreateInstanceCore() => new CornerRadiusAnimation();

    public override object GetCurrentValue(object defaultOriginValue, object defaultDestinationValue, AnimationClock animationClock)
    {
        if (animationClock?.CurrentProgress == null)
            return defaultOriginValue;

        var from = From ?? (CornerRadius)defaultOriginValue;
        var to = To ?? (CornerRadius)defaultDestinationValue;
        var progress = animationClock.CurrentProgress.Value;

        if (EasingFunction != null)
            progress = EasingFunction.Ease(progress);

        return new CornerRadius(
            Lerp(from.TopLeft, to.TopLeft, progress),
            Lerp(from.TopRight, to.TopRight, progress),
            Lerp(from.BottomRight, to.BottomRight, progress),
            Lerp(from.BottomLeft, to.BottomLeft, progress)
        );
    }

    private static double Lerp(double from, double to, double progress)
        => from + (to - from) * progress;

    public IEasingFunction? EasingFunction { get; set; }
}
