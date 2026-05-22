using System.Windows;
using System.Windows.Media.Animation;
namespace Mine.Wpf.Controls.Animations;
/// <summary>
/// Material 3 动效令牌 —— 将时长和缓动函数暴露为静态资源。
/// XAML 中引用示例：{x:Static mine:MotionTokens.Short2Duration}
/// </summary>
public static class MotionTokens
{
    // 时长定义
    public static readonly Duration Short1     = new(TimeSpan.FromMilliseconds(50));
    public static readonly Duration Short2     = new(TimeSpan.FromMilliseconds(100));
    public static readonly Duration Short3     = new(TimeSpan.FromMilliseconds(150));
    public static readonly Duration Short4     = new(TimeSpan.FromMilliseconds(200));
    public static readonly Duration Medium1    = new(TimeSpan.FromMilliseconds(250));
    public static readonly Duration Medium2    = new(TimeSpan.FromMilliseconds(300));
    public static readonly Duration Long1      = new(TimeSpan.FromMilliseconds(350));
    public static readonly Duration Long2      = new(TimeSpan.FromMilliseconds(450));
    public static readonly Duration ExtraLong1 = new(TimeSpan.FromMilliseconds(550));
    public static readonly Duration ExtraLong2 = new(TimeSpan.FromMilliseconds(700));
    // 标准缓动（通用过渡）
    public static IEasingFunction Standard             => new CubicEase { EasingMode = EasingMode.EaseInOut };
    // 强调减速（进入 / 显著元素）
    public static IEasingFunction EmphasizedDecelerate => new CubicEase { EasingMode = EasingMode.EaseOut };
    // 标准加速（退出元素）
    public static IEasingFunction StandardAccelerate   => new CubicEase { EasingMode = EasingMode.EaseIn };
    /// <summary>快速创建带动效令牌的 DoubleAnimation。</summary>
    public static DoubleAnimation Animate(
        double to,
        Duration duration,
        IEasingFunction? easing = null,
        double? from = null)
    {
        var anim = new DoubleAnimation
        {
            To             = to,
            Duration       = duration,
            EasingFunction = easing ?? Standard,
        };
        if (from.HasValue) anim.From = from.Value;
        return anim;
    }
}