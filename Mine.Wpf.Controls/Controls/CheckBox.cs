using System.Windows;

namespace Mine.Wpf.Controls.Controls;
/// <summary>Material 3 复选框，支持不确定状态和标签位置设置。</summary>
[TemplateVisualState(Name = "Unchecked",     GroupName = "CheckStates")]
[TemplateVisualState(Name = "Checked",       GroupName = "CheckStates")]
[TemplateVisualState(Name = "Indeterminate", GroupName = "CheckStates")]
public class CheckBox : System.Windows.Controls.CheckBox
{
    static CheckBox()
    {
        DefaultStyleKeyProperty.OverrideMetadata(
            typeof(CheckBox),
            new FrameworkPropertyMetadata(typeof(CheckBox)));
    }
    // ── 标签位置 ──────────────────────────────────────────────────
    public static readonly DependencyProperty LabelPlacementProperty =
        DependencyProperty.Register(nameof(LabelPlacement), typeof(LabelPlacement), typeof(CheckBox),
            new PropertyMetadata(LabelPlacement.Right));
    public LabelPlacement LabelPlacement
    {
        get => (LabelPlacement)GetValue(LabelPlacementProperty);
        set => SetValue(LabelPlacementProperty, value);
    }
    public override void OnApplyTemplate()
    {
        base.OnApplyTemplate();
        UpdateCheckState(false);
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
    protected override void OnIndeterminate(RoutedEventArgs e)
    {
        base.OnIndeterminate(e);
        VisualStateManager.GoToState(this, "Indeterminate", true);
    }
    private void UpdateCheckState(bool useTransitions)
    {
        var state = IsChecked switch
                    {
                        true  => "Checked",
                        false => "Unchecked",
                        null  => "Indeterminate"
                    };
        VisualStateManager.GoToState(this, state, useTransitions);
    }
}
/// <summary>标签位置枚举。</summary>
public enum LabelPlacement { Left, Right, Top, Bottom }
