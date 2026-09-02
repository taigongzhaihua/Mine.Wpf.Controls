using System.Windows;
using System.Windows.Controls;

namespace Mine.Wpf.Controls.Controls;

/// <summary>
/// 可展开的设置项：头部与 <see cref="SettingsCard"/> 一致（图标/标题/描述/操作区），
/// 展开区域承载子设置项（通过 Items 集合），支持流畅的展开/收起动画。
/// </summary>
[TemplateVisualState(Name = "Expanded",  GroupName = "ExpansionStates")]
[TemplateVisualState(Name = "Collapsed", GroupName = "ExpansionStates")]
[TemplatePart(Name = PartHeaderSite, Type = typeof(FrameworkElement))]
public class SettingsExpander : HeaderedItemsControl
{
    private const string PartHeaderSite = "PART_HeaderSite";
    private FrameworkElement? _headerSite;

    static SettingsExpander()
    {
        DefaultStyleKeyProperty.OverrideMetadata(
            typeof(SettingsExpander),
            new FrameworkPropertyMetadata(typeof(SettingsExpander)));
    }

    // ── IsExpanded ───────────────────────────────────────────────
    public static readonly DependencyProperty IsExpandedProperty =
        DependencyProperty.Register(
            nameof(IsExpanded),
            typeof(bool),
            typeof(SettingsExpander),
            new FrameworkPropertyMetadata(
                false,
                FrameworkPropertyMetadataOptions.BindsTwoWayByDefault,
                OnIsExpandedChanged));

    public bool IsExpanded
    {
        get => (bool)GetValue(IsExpandedProperty);
        set => SetValue(IsExpandedProperty, value);
    }

    private static void OnIsExpandedChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is SettingsExpander expander)
            VisualStateManager.GoToState(expander, (bool)e.NewValue ? "Expanded" : "Collapsed", true);
    }

    // ── Description ──────────────────────────────────────────────
    public static readonly DependencyProperty DescriptionProperty =
        DependencyProperty.Register(nameof(Description), typeof(object), typeof(SettingsExpander),
            new PropertyMetadata(null));
    public object? Description
    {
        get => GetValue(DescriptionProperty);
        set => SetValue(DescriptionProperty, value);
    }

    // ── HeaderIcon ───────────────────────────────────────────────
    public static readonly DependencyProperty HeaderIconProperty =
        DependencyProperty.Register(nameof(HeaderIcon), typeof(object), typeof(SettingsExpander),
            new PropertyMetadata(null));
    public object? HeaderIcon
    {
        get => GetValue(HeaderIconProperty);
        set => SetValue(HeaderIconProperty, value);
    }

    // ── ActionContent（头部右侧操作区） ────────────────────────────
    public static readonly DependencyProperty ActionContentProperty =
        DependencyProperty.Register(nameof(ActionContent), typeof(object), typeof(SettingsExpander),
            new PropertyMetadata(null));
    public object? ActionContent
    {
        get => GetValue(ActionContentProperty);
        set => SetValue(ActionContentProperty, value);
    }

    // ── CornerRadius ─────────────────────────────────────────────
    public static readonly DependencyProperty CornerRadiusProperty =
        DependencyProperty.Register(nameof(CornerRadius), typeof(CornerRadius), typeof(SettingsExpander),
            new PropertyMetadata(new CornerRadius(12)));
    public CornerRadius CornerRadius
    {
        get => (CornerRadius)GetValue(CornerRadiusProperty);
        set => SetValue(CornerRadiusProperty, value);
    }

    public override void OnApplyTemplate()
    {
        if (_headerSite != null)
            _headerSite.MouseLeftButtonUp -= OnHeaderClicked;

        base.OnApplyTemplate();

        _headerSite = GetTemplateChild(PartHeaderSite) as FrameworkElement;
        if (_headerSite != null)
            _headerSite.MouseLeftButtonUp += OnHeaderClicked;

        VisualStateManager.GoToState(this, IsExpanded ? "Expanded" : "Collapsed", false);
    }

    private void OnHeaderClicked(object sender, System.Windows.Input.MouseButtonEventArgs e)
    {
        IsExpanded = !IsExpanded;
    }
}
