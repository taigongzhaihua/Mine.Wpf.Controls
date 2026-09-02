using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;

namespace Mine.Wpf.Controls.Controls;

/// <summary>InfoBar 严重级别。</summary>
public enum InfoBarSeverity { Info, Success, Warning, Error }

/// <summary>
/// 内联信息提示条（InfoBar）：用于在页面/表单内联展示状态消息，
/// 支持 Info/Success/Warning/Error 四种语义色、可选标题、描述、操作按钮与关闭按钮。
/// 与 Toast/Snackbar 不同，InfoBar 常驻于布局中，不自动消失、不悬浮。
/// </summary>
[TemplatePart(Name = PartCloseButton, Type = typeof(ButtonBase))]
public class InfoBar : ContentControl
{
    private const string PartCloseButton = "PART_CloseButton";
    private ButtonBase? _closeButton;

    static InfoBar()
    {
        DefaultStyleKeyProperty.OverrideMetadata(
            typeof(InfoBar),
            new FrameworkPropertyMetadata(typeof(InfoBar)));
    }

    // ── Severity ─────────────────────────────────────────────────
    public static readonly DependencyProperty SeverityProperty =
        DependencyProperty.Register(nameof(Severity), typeof(InfoBarSeverity), typeof(InfoBar),
            new PropertyMetadata(InfoBarSeverity.Info));
    public InfoBarSeverity Severity
    {
        get => (InfoBarSeverity)GetValue(SeverityProperty);
        set => SetValue(SeverityProperty, value);
    }

    // ── Title ────────────────────────────────────────────────────
    public static readonly DependencyProperty TitleProperty =
        DependencyProperty.Register(nameof(Title), typeof(object), typeof(InfoBar),
            new PropertyMetadata(null));
    public object? Title
    {
        get => GetValue(TitleProperty);
        set => SetValue(TitleProperty, value);
    }

    // ── Message（描述文本，与 Content 二选一使用） ─────────────────
    public static readonly DependencyProperty MessageProperty =
        DependencyProperty.Register(nameof(Message), typeof(object), typeof(InfoBar),
            new PropertyMetadata(null));
    public object? Message
    {
        get => GetValue(MessageProperty);
        set => SetValue(MessageProperty, value);
    }

    // ── ActionContent（右侧操作区，如按钮/链接） ───────────────────
    public static readonly DependencyProperty ActionContentProperty =
        DependencyProperty.Register(nameof(ActionContent), typeof(object), typeof(InfoBar),
            new PropertyMetadata(null));
    public object? ActionContent
    {
        get => GetValue(ActionContentProperty);
        set => SetValue(ActionContentProperty, value);
    }

    // ── IsOpen ───────────────────────────────────────────────────
    public static readonly DependencyProperty IsOpenProperty =
        DependencyProperty.Register(nameof(IsOpen), typeof(bool), typeof(InfoBar),
            new FrameworkPropertyMetadata(true, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, OnIsOpenChanged));
    public bool IsOpen
    {
        get => (bool)GetValue(IsOpenProperty);
        set => SetValue(IsOpenProperty, value);
    }

    private static void OnIsOpenChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is not InfoBar bar) return;
        if (!(bool)e.NewValue)
            bar.RaiseEvent(new RoutedEventArgs(ClosedEvent, bar));
    }

    // ── IsClosable ───────────────────────────────────────────────
    public static readonly DependencyProperty IsClosableProperty =
        DependencyProperty.Register(nameof(IsClosable), typeof(bool), typeof(InfoBar),
            new PropertyMetadata(true));
    public bool IsClosable
    {
        get => (bool)GetValue(IsClosableProperty);
        set => SetValue(IsClosableProperty, value);
    }

    // ── CornerRadius ─────────────────────────────────────────────
    public static readonly DependencyProperty CornerRadiusProperty =
        DependencyProperty.Register(nameof(CornerRadius), typeof(CornerRadius), typeof(InfoBar),
            new PropertyMetadata(new CornerRadius(8)));
    public CornerRadius CornerRadius
    {
        get => (CornerRadius)GetValue(CornerRadiusProperty);
        set => SetValue(CornerRadiusProperty, value);
    }

    // ── Closed 事件 ──────────────────────────────────────────────
    public static readonly RoutedEvent ClosedEvent =
        EventManager.RegisterRoutedEvent(
            nameof(Closed),
            RoutingStrategy.Bubble,
            typeof(RoutedEventHandler),
            typeof(InfoBar));
    public event RoutedEventHandler Closed
    {
        add => AddHandler(ClosedEvent, value);
        remove => RemoveHandler(ClosedEvent, value);
    }

    public override void OnApplyTemplate()
    {
        if (_closeButton != null)
            _closeButton.Click -= OnCloseButtonClick;

        base.OnApplyTemplate();

        _closeButton = GetTemplateChild(PartCloseButton) as ButtonBase;
        if (_closeButton != null)
            _closeButton.Click += OnCloseButtonClick;
    }

    private void OnCloseButtonClick(object sender, RoutedEventArgs e)
    {
        IsOpen = false;
    }
}
