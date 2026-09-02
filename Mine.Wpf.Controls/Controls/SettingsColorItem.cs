using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace Mine.Wpf.Controls.Controls;

/// <summary>
/// 颜色选择设置项：左侧图标 + 标题/描述，右侧圆形色块展示 <see cref="SelectedColor"/>，
/// 整行可点击（触发 <see cref="Click"/> 事件），通常与 FlyoutService + ColorPicker 组合，
/// 点击后弹出颜色选择面板。
/// </summary>
[TemplateVisualState(Name = "Normal",    GroupName = "CommonStates")]
[TemplateVisualState(Name = "MouseOver", GroupName = "CommonStates")]
[TemplateVisualState(Name = "Pressed",   GroupName = "CommonStates")]
[TemplateVisualState(Name = "Disabled",  GroupName = "CommonStates")]
public class SettingsColorItem : ContentControl
{
    static SettingsColorItem()
    {
        DefaultStyleKeyProperty.OverrideMetadata(
            typeof(SettingsColorItem),
            new FrameworkPropertyMetadata(typeof(SettingsColorItem)));
    }

    // ── Header ───────────────────────────────────────────────────
    public static readonly DependencyProperty HeaderProperty =
        DependencyProperty.Register(nameof(Header), typeof(object), typeof(SettingsColorItem),
            new PropertyMetadata(null));
    public object? Header
    {
        get => GetValue(HeaderProperty);
        set => SetValue(HeaderProperty, value);
    }

    // ── Description ──────────────────────────────────────────────
    public static readonly DependencyProperty DescriptionProperty =
        DependencyProperty.Register(nameof(Description), typeof(object), typeof(SettingsColorItem),
            new PropertyMetadata(null));
    public object? Description
    {
        get => GetValue(DescriptionProperty);
        set => SetValue(DescriptionProperty, value);
    }

    // ── HeaderIcon（Material Symbols 字符或任意 Content） ──────────
    public static readonly DependencyProperty HeaderIconProperty =
        DependencyProperty.Register(nameof(HeaderIcon), typeof(object), typeof(SettingsColorItem),
            new PropertyMetadata(null));
    public object? HeaderIcon
    {
        get => GetValue(HeaderIconProperty);
        set => SetValue(HeaderIconProperty, value);
    }

    // ── SelectedColor（色块展示的颜色） ─────────────────────────────
    public static readonly DependencyProperty SelectedColorProperty =
        DependencyProperty.Register(nameof(SelectedColor), typeof(Color), typeof(SettingsColorItem),
            new FrameworkPropertyMetadata(Colors.Gray, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));
    public Color SelectedColor
    {
        get => (Color)GetValue(SelectedColorProperty);
        set => SetValue(SelectedColorProperty, value);
    }

    // ── ShowChevron（是否在色块右侧显示前进箭头） ───────────────────
    public static readonly DependencyProperty ShowChevronProperty =
        DependencyProperty.Register(nameof(ShowChevron), typeof(bool), typeof(SettingsColorItem),
            new PropertyMetadata(true));
    public bool ShowChevron
    {
        get => (bool)GetValue(ShowChevronProperty);
        set => SetValue(ShowChevronProperty, value);
    }

    // ── CornerRadius ─────────────────────────────────────────────
    public static readonly DependencyProperty CornerRadiusProperty =
        DependencyProperty.Register(nameof(CornerRadius), typeof(CornerRadius), typeof(SettingsColorItem),
            new PropertyMetadata(new CornerRadius(12)));
    public CornerRadius CornerRadius
    {
        get => (CornerRadius)GetValue(CornerRadiusProperty);
        set => SetValue(CornerRadiusProperty, value);
    }

    // ── Click 事件 ───────────────────────────────────────────────
    public static readonly RoutedEvent ClickEvent =
        EventManager.RegisterRoutedEvent(
            nameof(Click),
            RoutingStrategy.Bubble,
            typeof(RoutedEventHandler),
            typeof(SettingsColorItem));
    public event RoutedEventHandler Click
    {
        add => AddHandler(ClickEvent, value);
        remove => RemoveHandler(ClickEvent, value);
    }

    protected override void OnMouseLeftButtonUp(System.Windows.Input.MouseButtonEventArgs e)
    {
        base.OnMouseLeftButtonUp(e);
        if (IsEnabled)
            RaiseEvent(new RoutedEventArgs(ClickEvent, this));
    }
}
