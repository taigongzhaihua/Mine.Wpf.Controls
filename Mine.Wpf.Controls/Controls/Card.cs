using System.Windows;
using System.Windows.Controls;
namespace Mine.Wpf.Controls.Controls;
/// <summary>
/// Material 3 卡片控件——支持 Elevated / Filled / Outlined 三种变体。
/// </summary>
[TemplateVisualState(Name = "Normal",    GroupName = "CommonStates")]
[TemplateVisualState(Name = "MouseOver", GroupName = "CommonStates")]
[TemplateVisualState(Name = "Pressed",   GroupName = "CommonStates")]
public class Card : ContentControl
{
    static Card()
    {
        DefaultStyleKeyProperty.OverrideMetadata(
            typeof(Card),
            new FrameworkPropertyMetadata(typeof(Card)));
    }
    // ── 变体 ─────────────────────────────────────────────────────
    public static readonly DependencyProperty VariantProperty =
        DependencyProperty.Register(nameof(Variant), typeof(CardVariant), typeof(Card),
            new PropertyMetadata(CardVariant.Elevated));
    public CardVariant Variant
    {
        get => (CardVariant)GetValue(VariantProperty);
        set => SetValue(VariantProperty, value);
    }
    // ── 圆角 ─────────────────────────────────────────────────────
    public static readonly DependencyProperty CornerRadiusProperty =
        DependencyProperty.Register(nameof(CornerRadius), typeof(CornerRadius), typeof(Card),
            new PropertyMetadata(new CornerRadius(12)));
    public CornerRadius CornerRadius
    {
        get => (CornerRadius)GetValue(CornerRadiusProperty);
        set => SetValue(CornerRadiusProperty, value);
    }
    // ── 是否可点击 ────────────────────────────────────────────────
    public static readonly DependencyProperty IsClickableProperty =
        DependencyProperty.Register(nameof(IsClickable), typeof(bool), typeof(Card),
            new PropertyMetadata(false));
    public bool IsClickable
    {
        get => (bool)GetValue(IsClickableProperty);
        set => SetValue(IsClickableProperty, value);
    }
    // ── 海拔等级 ──────────────────────────────────────────────────
    public static readonly DependencyProperty ElevationProperty =
        DependencyProperty.Register(nameof(Elevation), typeof(int), typeof(Card),
            new PropertyMetadata(1));
    public int Elevation
    {
        get => (int)GetValue(ElevationProperty);
        set => SetValue(ElevationProperty, value);
    }
}
/// <summary>卡片变体枚举。</summary>
public enum CardVariant { Elevated, Filled, Outlined }