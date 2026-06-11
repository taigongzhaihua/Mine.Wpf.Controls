using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace Mine.Wpf.Controls.Controls;

/// <summary>
/// Material Design 3 Shield 控件。
/// 类似于 GitHub 徽章,包含左侧标签和右侧内容。
/// </summary>
public class Shield : Control
{
    static Shield()
    {
        DefaultStyleKeyProperty.OverrideMetadata(
            typeof(Shield),
            new FrameworkPropertyMetadata(typeof(Shield)));
    }

    // ── Label ──────────────────────────────────────────────────
    public static readonly DependencyProperty LabelProperty =
        DependencyProperty.Register(nameof(Label), typeof(string), typeof(Shield),
            new PropertyMetadata(string.Empty));

    public string Label
    {
        get => (string)GetValue(LabelProperty);
        set => SetValue(LabelProperty, value);
    }

    // ── Value ──────────────────────────────────────────────────
    public static readonly DependencyProperty ValueProperty =
        DependencyProperty.Register(nameof(Value), typeof(string), typeof(Shield),
            new PropertyMetadata(string.Empty));

    public string Value
    {
        get => (string)GetValue(ValueProperty);
        set => SetValue(ValueProperty, value);
    }

    // ── Color ──────────────────────────────────────────────────
    public static readonly DependencyProperty ColorProperty =
        DependencyProperty.Register(nameof(Color), typeof(Brush), typeof(Shield),
            new PropertyMetadata(null));

    public Brush? Color
    {
        get => (Brush?)GetValue(ColorProperty);
        set => SetValue(ColorProperty, value);
    }

    // ── LabelBackground ──────────────────────────────────────────
    public static readonly DependencyProperty LabelBackgroundProperty =
        DependencyProperty.Register(nameof(LabelBackground), typeof(Brush), typeof(Shield),
            new PropertyMetadata(null));

    public Brush? LabelBackground
    {
        get => (Brush?)GetValue(LabelBackgroundProperty);
        set => SetValue(LabelBackgroundProperty, value);
    }

    // ── CornerRadius ──────────────────────────────────────────
    public static readonly DependencyProperty CornerRadiusProperty =
        DependencyProperty.Register(nameof(CornerRadius), typeof(CornerRadius), typeof(Shield),
            new PropertyMetadata(new CornerRadius(4)));

    public CornerRadius CornerRadius
    {
        get => (CornerRadius)GetValue(CornerRadiusProperty);
        set => SetValue(CornerRadiusProperty, value);
    }
}
