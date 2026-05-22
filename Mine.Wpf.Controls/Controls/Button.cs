using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;

namespace Mine.Wpf.Controls.Controls;
/// <summary>
/// Material 3 按钮，支持 Filled / Tonal / Outlined / Text / Elevated / FAB / ExtendedFAB 变体。
/// </summary>
[TemplatePart(Name = PartRipple,  Type = typeof(Primitives.RippleDecorator))]
[TemplatePart(Name = PartContent, Type = typeof(ContentPresenter))]
[TemplateVisualState(Name = "Normal",    GroupName = "CommonStates")]
[TemplateVisualState(Name = "MouseOver", GroupName = "CommonStates")]
[TemplateVisualState(Name = "Pressed",   GroupName = "CommonStates")]
[TemplateVisualState(Name = "Disabled",  GroupName = "CommonStates")]
[TemplateVisualState(Name = "Loading",   GroupName = "LoadingStates")]
[TemplateVisualState(Name = "Idle",      GroupName = "LoadingStates")]
public class Button : ButtonBase
{
    private const string PartRipple  = "PART_Ripple";
    private const string PartContent = "PART_Content";
    static Button()
    {
        DefaultStyleKeyProperty.OverrideMetadata(
            typeof(Button),
            new FrameworkPropertyMetadata(typeof(Button)));
    }
    // ── 变体 ─────────────────────────────────────────────────────
    public static readonly DependencyProperty VariantProperty =
        DependencyProperty.Register(nameof(Variant), typeof(ButtonVariant), typeof(Button),
            new PropertyMetadata(ButtonVariant.Filled));
    public ButtonVariant Variant
    {
        get => (ButtonVariant)GetValue(VariantProperty);
        set => SetValue(VariantProperty, value);
    }
    // ── 图标 ─────────────────────────────────────────────────────
    public static readonly DependencyProperty IconProperty =
        DependencyProperty.Register(nameof(Icon), typeof(object), typeof(Button),
            new PropertyMetadata(null));
    public object? Icon
    {
        get => GetValue(IconProperty);
        set => SetValue(IconProperty, value);
    }
    // ── 加载状态 ──────────────────────────────────────────────────
    public static readonly DependencyProperty IsLoadingProperty =
        DependencyProperty.Register(nameof(IsLoading), typeof(bool), typeof(Button),
            new PropertyMetadata(false, OnIsLoadingChanged));
    public bool IsLoading
    {
        get => (bool)GetValue(IsLoadingProperty);
        set => SetValue(IsLoadingProperty, value);
    }
    private static void OnIsLoadingChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is Button btn)
            VisualStateManager.GoToState(btn, (bool)e.NewValue ? "Loading" : "Idle", true);
    }
    // ── 圆角 ─────────────────────────────────────────────────────
    public static readonly DependencyProperty CornerRadiusProperty =
        DependencyProperty.Register(nameof(CornerRadius), typeof(CornerRadius), typeof(Button),
            new PropertyMetadata(new CornerRadius(20)));
    public CornerRadius CornerRadius
    {
        get => (CornerRadius)GetValue(CornerRadiusProperty);
        set => SetValue(CornerRadiusProperty, value);
    }
    // ── 海拔等级 ──────────────────────────────────────────────────
    public static readonly DependencyProperty ElevationProperty =
        DependencyProperty.Register(nameof(Elevation), typeof(int), typeof(Button),
            new PropertyMetadata(0));
    public int Elevation
    {
        get => (int)GetValue(ElevationProperty);
        set => SetValue(ElevationProperty, value);
    }
    // ── 模板应用 ──────────────────────────────────────────────────
    public override void OnApplyTemplate()
    {
        base.OnApplyTemplate();
        VisualStateManager.GoToState(this, IsLoading ? "Loading" : "Idle", false);
        VisualStateManager.GoToState(this, IsEnabled ? "Normal" : "Disabled", false);
    }
    protected override void OnRenderSizeChanged(SizeChangedInfo sizeInfo)
    {
        base.OnRenderSizeChanged(sizeInfo);
        // FAB 变体保持正圆形
        if (Variant == ButtonVariant.Fab)
        {
            double size = Math.Max(ActualWidth, ActualHeight);
            CornerRadius = new CornerRadius(size / 2);
        }
    }
}
/// <summary>按钮变体枚举。</summary>
public enum ButtonVariant
{
    Filled,
    Tonal,
    Outlined,
    Text,
    Elevated,
    Fab,
    ExtendedFab
}
