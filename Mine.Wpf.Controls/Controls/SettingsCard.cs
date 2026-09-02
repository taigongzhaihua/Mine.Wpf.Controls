using System.Windows;
using System.Windows.Controls;

namespace Mine.Wpf.Controls.Controls;

/// <summary>
/// 单行设置项卡片：左侧图标 + 标题/描述，右侧操作区（如 Switch、ComboBox 等）。
/// 可通过 <see cref="IsClickable"/> 启用点击态视觉反馈。
/// </summary>
[TemplateVisualState(Name = "Normal",    GroupName = "CommonStates")]
[TemplateVisualState(Name = "MouseOver", GroupName = "CommonStates")]
[TemplateVisualState(Name = "Pressed",   GroupName = "CommonStates")]
[TemplateVisualState(Name = "Disabled",  GroupName = "CommonStates")]
public class SettingsCard : ContentControl
{
    static SettingsCard()
    {
        DefaultStyleKeyProperty.OverrideMetadata(
            typeof(SettingsCard),
            new FrameworkPropertyMetadata(typeof(SettingsCard)));
    }

    // ── Header ───────────────────────────────────────────────────
    public static readonly DependencyProperty HeaderProperty =
        DependencyProperty.Register(nameof(Header), typeof(object), typeof(SettingsCard),
            new PropertyMetadata(null));
    public object? Header
    {
        get => GetValue(HeaderProperty);
        set => SetValue(HeaderProperty, value);
    }

    // ── Description ──────────────────────────────────────────────
    public static readonly DependencyProperty DescriptionProperty =
        DependencyProperty.Register(nameof(Description), typeof(object), typeof(SettingsCard),
            new PropertyMetadata(null));
    public object? Description
    {
        get => GetValue(DescriptionProperty);
        set => SetValue(DescriptionProperty, value);
    }

    // ── HeaderIcon（Material Symbols 字符或任意 Content） ──────────
    public static readonly DependencyProperty HeaderIconProperty =
        DependencyProperty.Register(nameof(HeaderIcon), typeof(object), typeof(SettingsCard),
            new PropertyMetadata(null));
    public object? HeaderIcon
    {
        get => GetValue(HeaderIconProperty);
        set => SetValue(HeaderIconProperty, value);
    }

    // ── ActionContent（右侧操作区，如 Switch/ComboBox/Button） ────
    public static readonly DependencyProperty ActionContentProperty =
        DependencyProperty.Register(nameof(ActionContent), typeof(object), typeof(SettingsCard),
            new PropertyMetadata(null));
    public object? ActionContent
    {
        get => GetValue(ActionContentProperty);
        set => SetValue(ActionContentProperty, value);
    }

    // ── IsClickable ──────────────────────────────────────────────
    public static readonly DependencyProperty IsClickableProperty =
        DependencyProperty.Register(nameof(IsClickable), typeof(bool), typeof(SettingsCard),
            new PropertyMetadata(false));
    public bool IsClickable
    {
        get => (bool)GetValue(IsClickableProperty);
        set => SetValue(IsClickableProperty, value);
    }

    // ── CornerRadius ─────────────────────────────────────────────
    public static readonly DependencyProperty CornerRadiusProperty =
        DependencyProperty.Register(nameof(CornerRadius), typeof(CornerRadius), typeof(SettingsCard),
            new PropertyMetadata(new CornerRadius(12)));
    public CornerRadius CornerRadius
    {
        get => (CornerRadius)GetValue(CornerRadiusProperty);
        set => SetValue(CornerRadiusProperty, value);
    }

    // ── ClickCommand ─────────────────────────────────────────────
    public static readonly RoutedEvent ClickEvent =
        EventManager.RegisterRoutedEvent(
            nameof(Click),
            RoutingStrategy.Bubble,
            typeof(RoutedEventHandler),
            typeof(SettingsCard));

    public event RoutedEventHandler Click
    {
        add => AddHandler(ClickEvent, value);
        remove => RemoveHandler(ClickEvent, value);
    }

    protected override void OnMouseLeftButtonUp(System.Windows.Input.MouseButtonEventArgs e)
    {
        base.OnMouseLeftButtonUp(e);
        if (IsClickable && IsEnabled)
        {
            RaiseEvent(new RoutedEventArgs(ClickEvent, this));
        }
    }
}
