using System.Windows;
using System.Windows.Controls.Primitives;

namespace Mine.Wpf.Controls.Controls;

/// <summary>Material Design 3 Switch（拨动开关）。</summary>
[TemplateVisualState(Name = "Unchecked", GroupName = "CheckStates")]
[TemplateVisualState(Name = "Checked",   GroupName = "CheckStates")]
public class Switch : ToggleButton
{
    static Switch()
    {
        DefaultStyleKeyProperty.OverrideMetadata(
            typeof(Switch),
            new FrameworkPropertyMetadata(typeof(Switch)));
    }

    public override void OnApplyTemplate()
    {
        base.OnApplyTemplate();
        GoToCheckState(false);
    }

    protected override void OnChecked(RoutedEventArgs e)
    {
        base.OnChecked(e);
        GoToCheckState(true);
    }

    protected override void OnUnchecked(RoutedEventArgs e)
    {
        base.OnUnchecked(e);
        GoToCheckState(true);
    }

    private void GoToCheckState(bool animate)
    {
        var state = IsChecked == true ? "Checked" : "Unchecked";
        VisualStateManager.GoToState(this, state, animate);
    }

    // ── 开启状态图标（Material Symbols 字符） ──────────────────────
    public static readonly DependencyProperty OnIconProperty =
        DependencyProperty.Register(nameof(OnIcon), typeof(string), typeof(Switch),
            new PropertyMetadata(null));

    /// <summary>选中（On）状态下 Thumb 中显示的 Material Symbols 字符。为 null 时不显示图标。</summary>
    public string? OnIcon
    {
        get => (string?)GetValue(OnIconProperty);
        set => SetValue(OnIconProperty, value);
    }

    // ── 关闭状态图标 ──────────────────────────────────────────────
    public static readonly DependencyProperty OffIconProperty =
        DependencyProperty.Register(nameof(OffIcon), typeof(string), typeof(Switch),
            new PropertyMetadata(null));

    /// <summary>未选中（Off）状态下 Thumb 中显示的 Material Symbols 字符。为 null 时不显示图标。</summary>
    public string? OffIcon
    {
        get => (string?)GetValue(OffIconProperty);
        set => SetValue(OffIconProperty, value);
    }

    // ── 标签位置 ──────────────────────────────────────────────────
    public static readonly DependencyProperty LabelPlacementProperty =
        DependencyProperty.Register(nameof(LabelPlacement), typeof(LabelPlacement), typeof(Switch),
            new PropertyMetadata(LabelPlacement.Right));

    public LabelPlacement LabelPlacement
    {
        get => (LabelPlacement)GetValue(LabelPlacementProperty);
        set => SetValue(LabelPlacementProperty, value);
    }
}


