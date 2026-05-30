using System.Windows;
using System.Windows.Controls.Primitives;

namespace Mine.Wpf.Controls.Controls;

/// <summary>Material Design 3 滑块控件。</summary>
[TemplatePart(Name = PartTrackBackground, Type = typeof(FrameworkElement))]
[TemplatePart(Name = PartTrackFill,       Type = typeof(FrameworkElement))]
[TemplatePart(Name = PartThumb,           Type = typeof(Thumb))]
[TemplatePart(Name = PartValuePopup,      Type = typeof(System.Windows.Controls.Primitives.Popup))]
[TemplateVisualState(Name = "Normal",     GroupName = "CommonStates")]
[TemplateVisualState(Name = "MouseOver",  GroupName = "CommonStates")]
[TemplateVisualState(Name = "Pressed",    GroupName = "CommonStates")]
[TemplateVisualState(Name = "Disabled",   GroupName = "CommonStates")]
public class Slider : System.Windows.Controls.Slider
{
    public const string PartTrackBackground = "PART_TrackBackground";
    public const string PartTrackFill       = "PART_TrackFill";
    public const string PartThumb           = "PART_Thumb";
    public const string PartValuePopup      = "PART_ValuePopup";

    static Slider()
    {
        DefaultStyleKeyProperty.OverrideMetadata(
            typeof(Slider),
            new FrameworkPropertyMetadata(typeof(Slider)));
    }

    // ── 是否显示值气泡 ────────────────────────────────────────────
    public static readonly DependencyProperty ShowValueLabelProperty =
        DependencyProperty.Register(nameof(ShowValueLabel), typeof(bool), typeof(Slider),
            new PropertyMetadata(false));

    public bool ShowValueLabel
    {
        get => (bool)GetValue(ShowValueLabelProperty);
        set => SetValue(ShowValueLabelProperty, value);
    }

    // ── 是否显示刻度点（discrete 模式） ───────────────────────────
    public static readonly DependencyProperty ShowStopIndicatorsProperty =
        DependencyProperty.Register(nameof(ShowStopIndicators), typeof(bool), typeof(Slider),
            new PropertyMetadata(false));

    public bool ShowStopIndicators
    {
        get => (bool)GetValue(ShowStopIndicatorsProperty);
        set => SetValue(ShowStopIndicatorsProperty, value);
    }

    // ── 值标签格式化字符串 ─────────────────────────────────────────
    public static readonly DependencyProperty ValueFormatProperty =
        DependencyProperty.Register(nameof(ValueFormat), typeof(string), typeof(Slider),
            new PropertyMetadata("{0:0}"));

    public string ValueFormat
    {
        get => (string)GetValue(ValueFormatProperty);
        set => SetValue(ValueFormatProperty, value);
    }

    // ── 格式化后的显示值（只读） ──────────────────────────────────
    private static readonly DependencyPropertyKey FormattedValuePropertyKey =
        DependencyProperty.RegisterReadOnly(nameof(FormattedValue), typeof(string), typeof(Slider),
            new PropertyMetadata("0"));

    public static readonly DependencyProperty FormattedValueProperty = FormattedValuePropertyKey.DependencyProperty;

    public string FormattedValue => (string)GetValue(FormattedValueProperty);

    // ── 生命周期 ──────────────────────────────────────────────────
    public override void OnApplyTemplate()
    {
        base.OnApplyTemplate();
        UpdateFormattedValue();
        UpdateStates(false);
    }

    protected override void OnValueChanged(double oldValue, double newValue)
    {
        base.OnValueChanged(oldValue, newValue);
        UpdateFormattedValue();
    }

    protected override void OnMouseEnter(System.Windows.Input.MouseEventArgs e)
    {
        base.OnMouseEnter(e);
        if (IsEnabled) VisualStateManager.GoToState(this, "MouseOver", true);
    }

    protected override void OnMouseLeave(System.Windows.Input.MouseEventArgs e)
    {
        base.OnMouseLeave(e);
        if (IsEnabled) VisualStateManager.GoToState(this, "Normal", true);
    }

    protected override void OnThumbDragStarted(DragStartedEventArgs e)
    {
        base.OnThumbDragStarted(e);
        if (IsEnabled) VisualStateManager.GoToState(this, "Pressed", true);
    }

    protected override void OnThumbDragCompleted(DragCompletedEventArgs e)
    {
        base.OnThumbDragCompleted(e);
        if (IsEnabled)
            VisualStateManager.GoToState(this, IsMouseOver ? "MouseOver" : "Normal", true);
    }

    protected override void OnPropertyChanged(DependencyPropertyChangedEventArgs e)
    {
        base.OnPropertyChanged(e);
        if (e.Property == IsEnabledProperty)
            UpdateStates(true);
    }

    private void UpdateStates(bool useTransitions) =>
        VisualStateManager.GoToState(this, IsEnabled ? "Normal" : "Disabled", useTransitions);

    private void UpdateFormattedValue()
    {
        try { SetValue(FormattedValuePropertyKey, string.Format(ValueFormat, Value)); }
        catch { SetValue(FormattedValuePropertyKey, Value.ToString("0")); }
    }
}


