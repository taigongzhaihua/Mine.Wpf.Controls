using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace Mine.Wpf.Controls.Controls;

/// <summary>
/// Material Design 3 玻璃卡片控件 - 半透明磨砂玻璃效果。
/// 使用色彩叠加 + 噪点纹理模拟玻璃质感。
/// </summary>
public class GlassCard : ContentControl
{
    static GlassCard()
    {
        DefaultStyleKeyProperty.OverrideMetadata(
            typeof(GlassCard),
            new FrameworkPropertyMetadata(typeof(GlassCard)));
    }

    // ══════════════════════════════════════════════════════════════
    // 依赖属性
    // ══════════════════════════════════════════════════════════════

    #region TintColor - 叠加颜色

    public static readonly DependencyProperty TintColorProperty =
        DependencyProperty.Register(nameof(TintColor), typeof(Color), typeof(GlassCard),
            new PropertyMetadata(Color.FromArgb(153, 255, 255, 255))); // 60% 白色

    public Color TintColor
    {
        get => (Color)GetValue(TintColorProperty);
        set => SetValue(TintColorProperty, value);
    }

    #endregion

    #region TintOpacity - 叠加不透明度

    public static readonly DependencyProperty TintOpacityProperty =
        DependencyProperty.Register(nameof(TintOpacity), typeof(double), typeof(GlassCard),
            new PropertyMetadata(0.85));

    public double TintOpacity
    {
        get => (double)GetValue(TintOpacityProperty);
        set => SetValue(TintOpacityProperty, value);
    }

    #endregion

    #region CornerRadius - 圆角

    public static readonly DependencyProperty CornerRadiusProperty =
        DependencyProperty.Register(nameof(CornerRadius), typeof(CornerRadius), typeof(GlassCard),
            new PropertyMetadata(new CornerRadius(12)));

    public CornerRadius CornerRadius
    {
        get => (CornerRadius)GetValue(CornerRadiusProperty);
        set => SetValue(CornerRadiusProperty, value);
    }

    #endregion
}

