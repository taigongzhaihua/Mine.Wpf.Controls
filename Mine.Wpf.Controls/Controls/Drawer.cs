using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;

namespace Mine.Wpf.Controls.Controls;

/// <summary>
/// Material 3 Navigation Drawer：从边缘滑入的侧边抽屉面板。
/// 用法：将页面主体放在 Content，抽屉内容放在 DrawerContent，通过 IsOpen 控制开关。
/// </summary>
/// <remarks>
/// 结构：
/// <code>
/// &lt;mine:Drawer IsOpen="{Binding IsNavOpen}" DrawerWidth="320"&gt;
///     &lt;mine:Drawer.DrawerContent&gt; ... 导航菜单 ... &lt;/mine:Drawer.DrawerContent&gt;
///     ... 主体内容 ...
/// &lt;/mine:Drawer&gt;
/// </code>
/// </remarks>
[TemplatePart(Name = PartScrim,       Type = typeof(UIElement))]
[TemplatePart(Name = PartDrawerPanel, Type = typeof(FrameworkElement))]
[TemplatePart(Name = PartDrawerHost,  Type = typeof(ContentControl))]
public class Drawer : ContentControl
{
    private const string PartScrim       = "PART_Scrim";
    private const string PartDrawerPanel = "PART_DrawerPanel";
    private const string PartDrawerHost  = "PART_DrawerHost";

    private UIElement?       _scrim;
    private FrameworkElement? _drawerPanel;
    private bool _templateApplied;
    // 记录最近一次 close-completed 的动画 token，防止旧动画的 completed 覆盖新状态
    private int _animationGeneration;

    static Drawer()
    {
        DefaultStyleKeyProperty.OverrideMetadata(
            typeof(Drawer), new FrameworkPropertyMetadata(typeof(Drawer)));
    }

    // ── IsOpen ───────────────────────────────────────────────────────
    public static readonly DependencyProperty IsOpenProperty =
        DependencyProperty.Register(nameof(IsOpen), typeof(bool), typeof(Drawer),
            new PropertyMetadata(false, OnIsOpenChanged));

    /// <summary>抽屉是否打开。</summary>
    public bool IsOpen
    {
        get => (bool)GetValue(IsOpenProperty);
        set => SetValue(IsOpenProperty, value);
    }

    private static void OnIsOpenChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        => (d as Drawer)?.UpdateDrawerState(animate: true);

    // ── DrawerContent ─────────────────────────────────────────────────
    public static readonly DependencyProperty DrawerContentProperty =
        DependencyProperty.Register(nameof(DrawerContent), typeof(object), typeof(Drawer),
            new PropertyMetadata(null));

    /// <summary>抽屉面板内的内容。</summary>
    public object? DrawerContent
    {
        get => GetValue(DrawerContentProperty);
        set => SetValue(DrawerContentProperty, value);
    }

    // ── DrawerContentTemplate ─────────────────────────────────────────
    public static readonly DependencyProperty DrawerContentTemplateProperty =
        DependencyProperty.Register(nameof(DrawerContentTemplate), typeof(DataTemplate), typeof(Drawer),
            new PropertyMetadata(null));

    public DataTemplate? DrawerContentTemplate
    {
        get => (DataTemplate?)GetValue(DrawerContentTemplateProperty);
        set => SetValue(DrawerContentTemplateProperty, value);
    }

    // ── DrawerPlacement ───────────────────────────────────────────────
    public static readonly DependencyProperty DrawerPlacementProperty =
        DependencyProperty.Register(nameof(DrawerPlacement), typeof(DrawerPlacement), typeof(Drawer),
            new PropertyMetadata(DrawerPlacement.Left));

    /// <summary>抽屉滑入方向，默认从左侧。</summary>
    public DrawerPlacement DrawerPlacement
    {
        get => (DrawerPlacement)GetValue(DrawerPlacementProperty);
        set => SetValue(DrawerPlacementProperty, value);
    }

    // ── DrawerWidth ───────────────────────────────────────────────────
    public static readonly DependencyProperty DrawerWidthProperty =
        DependencyProperty.Register(nameof(DrawerWidth), typeof(double), typeof(Drawer),
            new PropertyMetadata(320.0));

    /// <summary>抽屉面板宽度（Left/Right 方向有效），默认 320。</summary>
    public double DrawerWidth
    {
        get => (double)GetValue(DrawerWidthProperty);
        set => SetValue(DrawerWidthProperty, value);
    }

    // ── DrawerHeight ──────────────────────────────────────────────────
    public static readonly DependencyProperty DrawerHeightProperty =
        DependencyProperty.Register(nameof(DrawerHeight), typeof(double), typeof(Drawer),
            new PropertyMetadata(double.NaN));

    /// <summary>抽屉面板高度（Top/Bottom 方向有效），默认 NaN（自动）。</summary>
    public double DrawerHeight
    {
        get => (double)GetValue(DrawerHeightProperty);
        set => SetValue(DrawerHeightProperty, value);
    }

    // ── ScrimOpacity ──────────────────────────────────────────────────
    public static readonly DependencyProperty ScrimOpacityProperty =
        DependencyProperty.Register(nameof(ScrimOpacity), typeof(double), typeof(Drawer),
            new PropertyMetadata(0.4));

    /// <summary>遮罩层最终透明度，默认 0.4。</summary>
    public double ScrimOpacity
    {
        get => (double)GetValue(ScrimOpacityProperty);
        set => SetValue(ScrimOpacityProperty, value);
    }

    // ── CloseOnScrimClick ─────────────────────────────────────────────
    public static readonly DependencyProperty CloseOnScrimClickProperty =
        DependencyProperty.Register(nameof(CloseOnScrimClick), typeof(bool), typeof(Drawer),
            new PropertyMetadata(true));

    /// <summary>点击遮罩时是否自动关闭，默认 true。</summary>
    public bool CloseOnScrimClick
    {
        get => (bool)GetValue(CloseOnScrimClickProperty);
        set => SetValue(CloseOnScrimClickProperty, value);
    }

    // ── Opened / Closed 路由事件 ──────────────────────────────────────
    public static readonly RoutedEvent OpenedEvent =
        EventManager.RegisterRoutedEvent(nameof(Opened), RoutingStrategy.Bubble,
            typeof(RoutedEventHandler), typeof(Drawer));

    public event RoutedEventHandler Opened
    {
        add    => AddHandler(OpenedEvent, value);
        remove => RemoveHandler(OpenedEvent, value);
    }

    public static readonly RoutedEvent ClosedEvent =
        EventManager.RegisterRoutedEvent(nameof(Closed), RoutingStrategy.Bubble,
            typeof(RoutedEventHandler), typeof(Drawer));

    public event RoutedEventHandler Closed
    {
        add    => AddHandler(ClosedEvent, value);
        remove => RemoveHandler(ClosedEvent, value);
    }

    // ── Template ─────────────────────────────────────────────────────
    public override void OnApplyTemplate()
    {
        base.OnApplyTemplate();

        _scrim = GetTemplateChild(PartScrim) as UIElement;
        _drawerPanel = GetTemplateChild(PartDrawerPanel) as FrameworkElement;

        if (_scrim != null)
            _scrim.MouseLeftButtonDown += OnScrimClicked;

        _templateApplied = true;
        UpdateDrawerState(animate: false);
    }

    private void OnScrimClicked(object sender, MouseButtonEventArgs e)
    {
        if (CloseOnScrimClick) IsOpen = false;
    }

    // ── 动画 ──────────────────────────────────────────────────────────
    private void UpdateDrawerState(bool animate)
    {
        if (!_templateApplied || _drawerPanel == null || _scrim == null) return;

        // 每次状态更新递增 generation，使旧的 completed 回调失效
        var generation = ++_animationGeneration;

        var duration = animate
            ? new Duration(TimeSpan.FromMilliseconds(300))
            : new Duration(TimeSpan.Zero);

        var ease = new CubicEase { EasingMode = EasingMode.EaseOut };

        if (IsOpen)
        {
            _scrim.Visibility        = Visibility.Visible;
            _drawerPanel.Visibility  = Visibility.Visible;

            AnimateScrim(0, ScrimOpacity, duration, ease);
            AnimateSlide(closed: false, duration, ease);
            RaiseEvent(new RoutedEventArgs(OpenedEvent, this));
        }
        else
        {
            AnimateScrim(ScrimOpacity, 0, duration, ease, () =>
            {
                if (_animationGeneration == generation)
                    _scrim.Visibility = Visibility.Collapsed;
            });
            AnimateSlide(closed: true, duration, ease, () =>
            {
                if (_animationGeneration == generation)
                    _drawerPanel.Visibility = Visibility.Collapsed;
            });
            RaiseEvent(new RoutedEventArgs(ClosedEvent, this));
        }
    }

    private void AnimateScrim(double from, double to, Duration duration, IEasingFunction ease,
                               Action? completed = null)
    {
        if (_scrim == null) return;
        var anim = new DoubleAnimation(from, to, duration) { EasingFunction = ease };
        if (completed != null)
            anim.Completed += (_, _) => completed();
        _scrim.BeginAnimation(OpacityProperty, anim);
    }

    private void AnimateSlide(bool closed, Duration duration, IEasingFunction ease,
                               Action? completed = null)
    {
        if (_drawerPanel == null) return;

        // 确保有 TranslateTransform
        var transform = _drawerPanel.RenderTransform as TranslateTransform;
        if (transform == null)
        {
            transform = new TranslateTransform();
            _drawerPanel.RenderTransform = transform;
        }

        // 计算"关闭态"的偏移（面板完全移出屏幕）
        // 开启态偏移始终为 0
        double closedX = 0, closedY = 0;
        switch (DrawerPlacement)
        {
            case DrawerPlacement.Left:
                // 尺寸未知时用 DrawerWidth 兜底
                closedX = -(_drawerPanel.ActualWidth > 0 ? _drawerPanel.ActualWidth : DrawerWidth) - 8;
                break;
            case DrawerPlacement.Right:
                closedX = (_drawerPanel.ActualWidth > 0 ? _drawerPanel.ActualWidth : DrawerWidth) + 8;
                break;
            case DrawerPlacement.Top:
                closedY = -(_drawerPanel.ActualHeight > 0 ? _drawerPanel.ActualHeight : DrawerHeight) - 8;
                break;
            case DrawerPlacement.Bottom:
                closedY = (_drawerPanel.ActualHeight > 0 ? _drawerPanel.ActualHeight : DrawerHeight) + 8;
                break;
        }

        double fromX = closed ? 0      : closedX;
        double toX   = closed ? closedX : 0;
        double fromY = closed ? 0      : closedY;
        double toY   = closed ? closedY : 0;

        var animX = new DoubleAnimation(fromX, toX, duration) { EasingFunction = ease };
        var animY = new DoubleAnimation(fromY, toY, duration) { EasingFunction = ease };

        if (completed != null)
            animX.Completed += (_, _) => completed();

        transform.BeginAnimation(TranslateTransform.XProperty, animX);
        transform.BeginAnimation(TranslateTransform.YProperty, animY);
    }
}

/// <summary>抽屉滑入方向。</summary>
public enum DrawerPlacement
{
    /// <summary>从左侧滑入（默认）。</summary>
    Left,
    /// <summary>从右侧滑入。</summary>
    Right,
    /// <summary>从顶部滑入。</summary>
    Top,
    /// <summary>从底部滑入。</summary>
    Bottom,
}





