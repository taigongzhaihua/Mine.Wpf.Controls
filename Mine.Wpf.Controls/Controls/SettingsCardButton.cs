using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;

namespace Mine.Wpf.Controls.Controls;

/// <summary>
/// 可点击的设置项卡片：整行作为按钮（支持键盘 Tab/Enter/Space 激活、Command 绑定），
/// 常用于"点击进入详情页"等导航场景。未指定 <see cref="ActionContent"/> 时，
/// 右侧自动显示一个默认的前进箭头图标。
/// </summary>
[TemplateVisualState(Name = "Normal",    GroupName = "CommonStates")]
[TemplateVisualState(Name = "MouseOver", GroupName = "CommonStates")]
[TemplateVisualState(Name = "Pressed",   GroupName = "CommonStates")]
[TemplateVisualState(Name = "Disabled",  GroupName = "CommonStates")]
public class SettingsCardButton : ButtonBase
{
    static SettingsCardButton()
    {
        DefaultStyleKeyProperty.OverrideMetadata(
            typeof(SettingsCardButton),
            new FrameworkPropertyMetadata(typeof(SettingsCardButton)));
    }

    // ── Header ───────────────────────────────────────────────────
    public static readonly DependencyProperty HeaderProperty =
        DependencyProperty.Register(nameof(Header), typeof(object), typeof(SettingsCardButton),
            new PropertyMetadata(null));
    public object? Header
    {
        get => GetValue(HeaderProperty);
        set => SetValue(HeaderProperty, value);
    }

    // ── Description ──────────────────────────────────────────────
    public static readonly DependencyProperty DescriptionProperty =
        DependencyProperty.Register(nameof(Description), typeof(object), typeof(SettingsCardButton),
            new PropertyMetadata(null));
    public object? Description
    {
        get => GetValue(DescriptionProperty);
        set => SetValue(DescriptionProperty, value);
    }

    // ── HeaderIcon（Material Symbols 字符或任意 Content） ──────────
    public static readonly DependencyProperty HeaderIconProperty =
        DependencyProperty.Register(nameof(HeaderIcon), typeof(object), typeof(SettingsCardButton),
            new PropertyMetadata(null));
    public object? HeaderIcon
    {
        get => GetValue(HeaderIconProperty);
        set => SetValue(HeaderIconProperty, value);
    }

    // ── ActionContent（右侧操作区；为 null 时显示默认前进箭头） ────
    public static readonly DependencyProperty ActionContentProperty =
        DependencyProperty.Register(nameof(ActionContent), typeof(object), typeof(SettingsCardButton),
            new PropertyMetadata(null));
    public object? ActionContent
    {
        get => GetValue(ActionContentProperty);
        set => SetValue(ActionContentProperty, value);
    }

    // ── CornerRadius ─────────────────────────────────────────────
    public static readonly DependencyProperty CornerRadiusProperty =
        DependencyProperty.Register(nameof(CornerRadius), typeof(CornerRadius), typeof(SettingsCardButton),
            new PropertyMetadata(new CornerRadius(12)));
    public CornerRadius CornerRadius
    {
        get => (CornerRadius)GetValue(CornerRadiusProperty);
        set => SetValue(CornerRadiusProperty, value);
    }

    public override void OnApplyTemplate()
    {
        base.OnApplyTemplate();
        VisualStateManager.GoToState(this, IsEnabled ? "Normal" : "Disabled", false);
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
            VisualStateManager.GoToState(this, IsPressed ? "Pressed" : "Normal", true);
    }

    protected override void OnIsPressedChanged(DependencyPropertyChangedEventArgs e)
    {
        base.OnIsPressedChanged(e);
        if (IsEnabled)
            VisualStateManager.GoToState(this, IsPressed ? "Pressed" : (IsMouseOver ? "MouseOver" : "Normal"), true);
    }
}
