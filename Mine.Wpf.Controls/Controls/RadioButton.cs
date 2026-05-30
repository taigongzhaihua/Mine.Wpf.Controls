using System.Windows;

namespace Mine.Wpf.Controls.Controls;

/// <summary>Material 3 单选按钮。</summary>
[TemplateVisualState(Name = "Normal",      GroupName = "CommonStates")]
[TemplateVisualState(Name = "MouseOver",   GroupName = "CommonStates")]
[TemplateVisualState(Name = "Pressed",     GroupName = "CommonStates")]
[TemplateVisualState(Name = "Disabled",    GroupName = "CommonStates")]
[TemplateVisualState(Name = "Unchecked",   GroupName = "CheckStates")]
[TemplateVisualState(Name = "Checked",     GroupName = "CheckStates")]
public class RadioButton : System.Windows.Controls.RadioButton
{
    static RadioButton()
    {
        DefaultStyleKeyProperty.OverrideMetadata(
            typeof(RadioButton),
            new FrameworkPropertyMetadata(typeof(RadioButton)));
    }

    // ── 标签位置 ──────────────────────────────────────────────────
    public static readonly DependencyProperty LabelPlacementProperty =
        DependencyProperty.Register(nameof(LabelPlacement), typeof(LabelPlacement), typeof(RadioButton),
            new PropertyMetadata(LabelPlacement.Right));

    public LabelPlacement LabelPlacement
    {
        get => (LabelPlacement)GetValue(LabelPlacementProperty);
        set => SetValue(LabelPlacementProperty, value);
    }

    // ── 生命周期 ──────────────────────────────────────────────────
    public override void OnApplyTemplate()
    {
        base.OnApplyTemplate();
        UpdateStates(false);
    }

    protected override void OnChecked(RoutedEventArgs e)
    {
        base.OnChecked(e);
        VisualStateManager.GoToState(this, "Checked", true);
    }

    protected override void OnUnchecked(RoutedEventArgs e)
    {
        base.OnUnchecked(e);
        VisualStateManager.GoToState(this, "Unchecked", true);
    }

    protected override void OnMouseEnter(System.Windows.Input.MouseEventArgs e)
    {
        base.OnMouseEnter(e);
        if (IsEnabled)
            VisualStateManager.GoToState(this, "MouseOver", true);
    }

    protected override void OnMouseLeave(System.Windows.Input.MouseEventArgs e)
    {
        base.OnMouseLeave(e);
        if (IsEnabled)
            VisualStateManager.GoToState(this, "Normal", true);
    }

    protected override void OnIsPressedChanged(System.Windows.DependencyPropertyChangedEventArgs e)
    {
        base.OnIsPressedChanged(e);
        if (!IsEnabled) return;
        VisualStateManager.GoToState(this, IsPressed ? "Pressed" : IsMouseOver ? "MouseOver" : "Normal", true);
    }

    private void UpdateStates(bool useTransitions)
    {
        VisualStateManager.GoToState(this, IsEnabled ? "Normal" : "Disabled", useTransitions);
        VisualStateManager.GoToState(this, IsChecked == true ? "Checked" : "Unchecked", useTransitions);
    }
}

