using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;

namespace Mine.Wpf.Controls.Controls;

/// <summary>
/// Material 3 Side Sheet。
/// 从侧边（左或右）滑入的面板，支持 Standard（非模态，无遮罩）和 Modal（模态，有遮罩）两种变体。
/// </summary>
/// <remarks>
/// 用法：
/// <code>
/// &lt;mine:SideSheet IsOpen="{Binding IsOpen}" Variant="Modal" Placement="Right" SheetWidth="360"&gt;
///     &lt;mine:SideSheet.SheetContent&gt; ... &lt;/mine:SideSheet.SheetContent&gt;
///     ... 主体内容 ...
/// &lt;/mine:SideSheet&gt;
/// </code>
/// </remarks>
[TemplatePart(Name = PartScrim,      Type = typeof(UIElement))]
[TemplatePart(Name = PartSheetPanel, Type = typeof(FrameworkElement))]
public class SideSheet : ContentControl
{
    private const string PartScrim      = "PART_Scrim";
    private const string PartSheetPanel = "PART_SheetPanel";

    private UIElement?        _scrim;
    private FrameworkElement? _sheetPanel;
    private bool              _templateApplied;
    private int               _animationGeneration;

    static SideSheet()
    {
        DefaultStyleKeyProperty.OverrideMetadata(
            typeof(SideSheet), new FrameworkPropertyMetadata(typeof(SideSheet)));
    }

    // ── IsOpen ────────────────────────────────────────────────────────
    public static readonly DependencyProperty IsOpenProperty =
        DependencyProperty.Register(nameof(IsOpen), typeof(bool), typeof(SideSheet),
            new PropertyMetadata(false, OnIsOpenChanged));

    /// <summary>面板是否打开。</summary>
    public bool IsOpen
    {
        get => (bool)GetValue(IsOpenProperty);
        set => SetValue(IsOpenProperty, value);
    }

    private static void OnIsOpenChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        => (d as SideSheet)?.UpdateState(animate: true);

    // ── Variant ───────────────────────────────────────────────────────
    public static readonly DependencyProperty VariantProperty =
        DependencyProperty.Register(nameof(Variant), typeof(SideSheetVariant), typeof(SideSheet),
            new PropertyMetadata(SideSheetVariant.Modal));

    /// <summary>Modal（有遮罩）或 Standard（无遮罩，与内容并排或覆盖）。</summary>
    public SideSheetVariant Variant
    {
        get => (SideSheetVariant)GetValue(VariantProperty);
        set => SetValue(VariantProperty, value);
    }

    // ── Placement ─────────────────────────────────────────────────────
    public static readonly DependencyProperty PlacementProperty =
        DependencyProperty.Register(nameof(Placement), typeof(SideSheetPlacement), typeof(SideSheet),
            new PropertyMetadata(SideSheetPlacement.Right, OnPlacementChanged));

    /// <summary>面板出现在左侧还是右侧，默认 Right。</summary>
    public SideSheetPlacement Placement
    {
        get => (SideSheetPlacement)GetValue(PlacementProperty);
        set => SetValue(PlacementProperty, value);
    }

    private static void OnPlacementChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        => (d as SideSheet)?.UpdateState(animate: false);

    // ── SheetContent ──────────────────────────────────────────────────
    public static readonly DependencyProperty SheetContentProperty =
        DependencyProperty.Register(nameof(SheetContent), typeof(object), typeof(SideSheet),
            new PropertyMetadata(null));

    /// <summary>Side Sheet 面板内的内容。</summary>
    public object? SheetContent
    {
        get => GetValue(SheetContentProperty);
        set => SetValue(SheetContentProperty, value);
    }

    // ── SheetContentTemplate ──────────────────────────────────────────
    public static readonly DependencyProperty SheetContentTemplateProperty =
        DependencyProperty.Register(nameof(SheetContentTemplate), typeof(DataTemplate), typeof(SideSheet),
            new PropertyMetadata(null));

    public DataTemplate? SheetContentTemplate
    {
        get => (DataTemplate?)GetValue(SheetContentTemplateProperty);
        set => SetValue(SheetContentTemplateProperty, value);
    }

    // ── SheetWidth ────────────────────────────────────────────────────
    public static readonly DependencyProperty SheetWidthProperty =
        DependencyProperty.Register(nameof(SheetWidth), typeof(double), typeof(SideSheet),
            new PropertyMetadata(360.0));

    /// <summary>面板宽度，默认 360。</summary>
    public double SheetWidth
    {
        get => (double)GetValue(SheetWidthProperty);
        set => SetValue(SheetWidthProperty, value);
    }

    // ── CloseOnScrimClick ─────────────────────────────────────────────
    public static readonly DependencyProperty CloseOnScrimClickProperty =
        DependencyProperty.Register(nameof(CloseOnScrimClick), typeof(bool), typeof(SideSheet),
            new PropertyMetadata(true));

    /// <summary>点击遮罩时是否自动关闭，默认 true。仅 Modal 变体有效。</summary>
    public bool CloseOnScrimClick
    {
        get => (bool)GetValue(CloseOnScrimClickProperty);
        set => SetValue(CloseOnScrimClickProperty, value);
    }

    // ── ScrimOpacity ──────────────────────────────────────────────────
    public static readonly DependencyProperty ScrimOpacityProperty =
        DependencyProperty.Register(nameof(ScrimOpacity), typeof(double), typeof(SideSheet),
            new PropertyMetadata(0.4));

    /// <summary>遮罩层最终透明度，默认 0.4。仅 Modal 变体有效。</summary>
    public double ScrimOpacity
    {
        get => (double)GetValue(ScrimOpacityProperty);
        set => SetValue(ScrimOpacityProperty, value);
    }

    // ── CornerRadius ──────────────────────────────────────────────────
    public static readonly DependencyProperty CornerRadiusProperty =
        DependencyProperty.Register(nameof(CornerRadius), typeof(CornerRadius), typeof(SideSheet),
            new PropertyMetadata(new CornerRadius(0)));

    /// <summary>面板圆角，Modal 变体通常设左侧圆角，默认 0（嵌入式）。</summary>
    public CornerRadius CornerRadius
    {
        get => (CornerRadius)GetValue(CornerRadiusProperty);
        set => SetValue(CornerRadiusProperty, value);
    }

    // ── Opened / Closed 路由事件 ──────────────────────────────────────
    public static readonly RoutedEvent OpenedEvent =
        EventManager.RegisterRoutedEvent(nameof(Opened), RoutingStrategy.Bubble,
            typeof(RoutedEventHandler), typeof(SideSheet));

    public event RoutedEventHandler Opened
    {
        add    => AddHandler(OpenedEvent, value);
        remove => RemoveHandler(OpenedEvent, value);
    }

    public static readonly RoutedEvent ClosedEvent =
        EventManager.RegisterRoutedEvent(nameof(Closed), RoutingStrategy.Bubble,
            typeof(RoutedEventHandler), typeof(SideSheet));

    public event RoutedEventHandler Closed
    {
        add    => AddHandler(ClosedEvent, value);
        remove => RemoveHandler(ClosedEvent, value);
    }

    // ── Template ──────────────────────────────────────────────────────
    public override void OnApplyTemplate()
    {
        base.OnApplyTemplate();

        if (_scrim != null)
            _scrim.MouseLeftButtonDown -= OnScrimClicked;

        _scrim      = GetTemplateChild(PartScrim)      as UIElement;
        _sheetPanel = GetTemplateChild(PartSheetPanel) as FrameworkElement;

        if (_scrim != null)
            _scrim.MouseLeftButtonDown += OnScrimClicked;

        _templateApplied = true;
        UpdateState(animate: false);
    }

    private void OnScrimClicked(object sender, MouseButtonEventArgs e)
    {
        if (CloseOnScrimClick) IsOpen = false;
    }

    // ── 动画 ──────────────────────────────────────────────────────────
    private void UpdateState(bool animate)
    {
        if (!_templateApplied || _sheetPanel == null) return;

        var generation = ++_animationGeneration;
        var duration   = new Duration(TimeSpan.FromMilliseconds(animate ? 300 : 0));
        var ease       = new CubicEase { EasingMode = EasingMode.EaseOut };

        bool isModal = Variant == SideSheetVariant.Modal;

        if (IsOpen)
        {
            if (isModal && _scrim != null)
            {
                _scrim.Visibility = Visibility.Visible;
                AnimateScrim(0, ScrimOpacity, duration, ease);
            }
            _sheetPanel.Visibility = Visibility.Visible;
            AnimateSlide(closing: false, duration, ease);
            RaiseEvent(new RoutedEventArgs(OpenedEvent, this));
        }
        else
        {
            if (isModal && _scrim != null)
            {
                AnimateScrim(ScrimOpacity, 0, duration, ease, () =>
                {
                    if (_animationGeneration == generation && _scrim != null)
                        _scrim.Visibility = Visibility.Collapsed;
                });
            }
            AnimateSlide(closing: true, duration, ease, () =>
            {
                if (_animationGeneration == generation)
                    _sheetPanel.Visibility = Visibility.Collapsed;
            });
            RaiseEvent(new RoutedEventArgs(ClosedEvent, this));
        }
    }

    private void AnimateScrim(double from, double to, Duration duration, IEasingFunction ease,
                               Action? completed = null)
    {
        if (_scrim == null) return;
        var anim = new DoubleAnimation(from, to, duration) { EasingFunction = ease };
        if (completed != null) anim.Completed += (_, _) => completed();
        _scrim.BeginAnimation(OpacityProperty, anim);
    }

    private void AnimateSlide(bool closing, Duration duration, IEasingFunction ease,
                               Action? completed = null)
    {
        if (_sheetPanel == null) return;

        var transform = EnsureTransform();

        double panelW = _sheetPanel.ActualWidth > 0 ? _sheetPanel.ActualWidth : SheetWidth;

        double closedOffset = Placement == SideSheetPlacement.Right
            ? panelW + 8
            : -(panelW + 8);

        double from = closing ? 0            : closedOffset;
        double to   = closing ? closedOffset : 0;

        var anim = new DoubleAnimation(from, to, duration) { EasingFunction = ease };
        if (completed != null) anim.Completed += (_, _) => completed();
        transform.BeginAnimation(TranslateTransform.XProperty, anim);
    }

    private TranslateTransform EnsureTransform()
    {
        if (_sheetPanel!.RenderTransform is TranslateTransform { IsFrozen: false } t) return t;
        var transform = new TranslateTransform();
        _sheetPanel.RenderTransform = transform;
        return transform;
    }
}

/// <summary>Side Sheet 变体。</summary>
public enum SideSheetVariant
{
    /// <summary>模态（有遮罩），点击遮罩可关闭。</summary>
    Modal,
    /// <summary>标准（无遮罩），与页面内容共存。</summary>
    Standard,
}

/// <summary>Side Sheet 出现方向。</summary>
public enum SideSheetPlacement
{
    /// <summary>从左侧滑入。</summary>
    Left,
    /// <summary>从右侧滑入（默认）。</summary>
    Right,
}
