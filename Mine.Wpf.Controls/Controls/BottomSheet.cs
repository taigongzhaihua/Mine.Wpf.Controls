using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;

namespace Mine.Wpf.Controls.Controls;

/// <summary>
/// Material 3 Bottom Sheet。
/// 从底部滑入的面板，支持 Standard（非模态，无遮罩）和 Modal（模态，有遮罩）两种变体。
/// </summary>
/// <remarks>
/// 用法：
/// <code>
/// &lt;mine:BottomSheet IsOpen="{Binding IsOpen}" Variant="Modal" SheetHeight="400"&gt;
///     &lt;mine:BottomSheet.SheetContent&gt; ... &lt;/mine:BottomSheet.SheetContent&gt;
///     ... 主体内容 ...
/// &lt;/mine:BottomSheet&gt;
/// </code>
/// </remarks>
[TemplatePart(Name = PartScrim,       Type = typeof(UIElement))]
[TemplatePart(Name = PartSheetPanel,  Type = typeof(FrameworkElement))]
[TemplatePart(Name = PartDragHandle,  Type = typeof(UIElement))]
public class BottomSheet : ContentControl
{
    private const string PartScrim      = "PART_Scrim";
    private const string PartSheetPanel = "PART_SheetPanel";
    private const string PartDragHandle = "PART_DragHandle";

    private UIElement?        _scrim;
    private FrameworkElement? _sheetPanel;
    private UIElement?        _dragHandle;
    private bool              _templateApplied;
    private int               _animationGeneration;

    // 拖拽光标（WPF 矢量渲染，运行时生成）
    private static readonly Cursor? _cursorGrab     = CursorFactory.CreateGrab();
    private static readonly Cursor? _cursorGrabbing = CursorFactory.CreateGrabbing();

    // 拖拽关闭相关
    private bool   _isDragging;
    private double _dragStartY;
    private double _dragStartTranslateY;

    static BottomSheet()
    {
        DefaultStyleKeyProperty.OverrideMetadata(
            typeof(BottomSheet), new FrameworkPropertyMetadata(typeof(BottomSheet)));
    }

    // ── IsOpen ────────────────────────────────────────────────────────
    public static readonly DependencyProperty IsOpenProperty =
        DependencyProperty.Register(nameof(IsOpen), typeof(bool), typeof(BottomSheet),
            new PropertyMetadata(false, OnIsOpenChanged));

    /// <summary>面板是否打开。</summary>
    public bool IsOpen
    {
        get => (bool)GetValue(IsOpenProperty);
        set => SetValue(IsOpenProperty, value);
    }

    private static void OnIsOpenChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        => (d as BottomSheet)?.UpdateState(animate: true);

    // ── Variant ───────────────────────────────────────────────────────
    public static readonly DependencyProperty VariantProperty =
        DependencyProperty.Register(nameof(Variant), typeof(BottomSheetVariant), typeof(BottomSheet),
            new PropertyMetadata(BottomSheetVariant.Modal));

    /// <summary>Modal（有遮罩）或 Standard（无遮罩，覆盖在内容上方）。</summary>
    public BottomSheetVariant Variant
    {
        get => (BottomSheetVariant)GetValue(VariantProperty);
        set => SetValue(VariantProperty, value);
    }

    // ── SheetContent ──────────────────────────────────────────────────
    public static readonly DependencyProperty SheetContentProperty =
        DependencyProperty.Register(nameof(SheetContent), typeof(object), typeof(BottomSheet),
            new PropertyMetadata(null));

    /// <summary>Bottom Sheet 面板内的内容。</summary>
    public object? SheetContent
    {
        get => GetValue(SheetContentProperty);
        set => SetValue(SheetContentProperty, value);
    }

    // ── SheetContentTemplate ──────────────────────────────────────────
    public static readonly DependencyProperty SheetContentTemplateProperty =
        DependencyProperty.Register(nameof(SheetContentTemplate), typeof(DataTemplate), typeof(BottomSheet),
            new PropertyMetadata(null));

    public DataTemplate? SheetContentTemplate
    {
        get => (DataTemplate?)GetValue(SheetContentTemplateProperty);
        set => SetValue(SheetContentTemplateProperty, value);
    }

    // ── SheetHeight ───────────────────────────────────────────────────
    public static readonly DependencyProperty SheetHeightProperty =
        DependencyProperty.Register(nameof(SheetHeight), typeof(double), typeof(BottomSheet),
            new PropertyMetadata(double.NaN));

    /// <summary>面板高度，默认 NaN（自适应内容）。</summary>
    public double SheetHeight
    {
        get => (double)GetValue(SheetHeightProperty);
        set => SetValue(SheetHeightProperty, value);
    }

    // ── MaxSheetHeight ────────────────────────────────────────────────
    public static readonly DependencyProperty MaxSheetHeightProperty =
        DependencyProperty.Register(nameof(MaxSheetHeight), typeof(double), typeof(BottomSheet),
            new PropertyMetadata(double.PositiveInfinity));

    /// <summary>面板最大高度，防止内容过多时面板超出容器。</summary>
    public double MaxSheetHeight
    {
        get => (double)GetValue(MaxSheetHeightProperty);
        set => SetValue(MaxSheetHeightProperty, value);
    }

    // ── ShowDragHandle ────────────────────────────────────────────────
    public static readonly DependencyProperty ShowDragHandleProperty =
        DependencyProperty.Register(nameof(ShowDragHandle), typeof(bool), typeof(BottomSheet),
            new PropertyMetadata(true));

    /// <summary>是否显示顶部拖拽手柄，默认 true。</summary>
    public bool ShowDragHandle
    {
        get => (bool)GetValue(ShowDragHandleProperty);
        set => SetValue(ShowDragHandleProperty, value);
    }

    // ── EnableDragToClose ─────────────────────────────────────────────
    public static readonly DependencyProperty EnableDragToCloseProperty =
        DependencyProperty.Register(nameof(EnableDragToClose), typeof(bool), typeof(BottomSheet),
            new PropertyMetadata(true));

    /// <summary>是否允许向下拖拽关闭，默认 true。</summary>
    public bool EnableDragToClose
    {
        get => (bool)GetValue(EnableDragToCloseProperty);
        set => SetValue(EnableDragToCloseProperty, value);
    }

    // ── CloseOnScrimClick ─────────────────────────────────────────────
    public static readonly DependencyProperty CloseOnScrimClickProperty =
        DependencyProperty.Register(nameof(CloseOnScrimClick), typeof(bool), typeof(BottomSheet),
            new PropertyMetadata(true));

    /// <summary>点击遮罩时是否自动关闭，默认 true。</summary>
    public bool CloseOnScrimClick
    {
        get => (bool)GetValue(CloseOnScrimClickProperty);
        set => SetValue(CloseOnScrimClickProperty, value);
    }

    // ── ScrimOpacity ──────────────────────────────────────────────────
    public static readonly DependencyProperty ScrimOpacityProperty =
        DependencyProperty.Register(nameof(ScrimOpacity), typeof(double), typeof(BottomSheet),
            new PropertyMetadata(0.4));

    /// <summary>遮罩层最终透明度，默认 0.4。仅 Modal 变体有效。</summary>
    public double ScrimOpacity
    {
        get => (double)GetValue(ScrimOpacityProperty);
        set => SetValue(ScrimOpacityProperty, value);
    }

    // ── CornerRadius ──────────────────────────────────────────────────
    public static readonly DependencyProperty CornerRadiusProperty =
        DependencyProperty.Register(nameof(CornerRadius), typeof(CornerRadius), typeof(BottomSheet),
            new PropertyMetadata(new CornerRadius(28, 28, 0, 0)));

    /// <summary>面板顶部圆角，默认 28（MD3 ExtraLarge Top）。</summary>
    public CornerRadius CornerRadius
    {
        get => (CornerRadius)GetValue(CornerRadiusProperty);
        set => SetValue(CornerRadiusProperty, value);
    }

    // ── Opened / Closed 路由事件 ──────────────────────────────────────
    public static readonly RoutedEvent OpenedEvent =
        EventManager.RegisterRoutedEvent(nameof(Opened), RoutingStrategy.Bubble,
            typeof(RoutedEventHandler), typeof(BottomSheet));

    public event RoutedEventHandler Opened
    {
        add    => AddHandler(OpenedEvent, value);
        remove => RemoveHandler(OpenedEvent, value);
    }

    public static readonly RoutedEvent ClosedEvent =
        EventManager.RegisterRoutedEvent(nameof(Closed), RoutingStrategy.Bubble,
            typeof(RoutedEventHandler), typeof(BottomSheet));

    public event RoutedEventHandler Closed
    {
        add    => AddHandler(ClosedEvent, value);
        remove => RemoveHandler(ClosedEvent, value);
    }

    // ── Template ──────────────────────────────────────────────────────
    public override void OnApplyTemplate()
    {
        base.OnApplyTemplate();

        // 解绑旧引用
        if (_scrim != null)
            _scrim.MouseLeftButtonDown -= OnScrimClicked;
        if (_dragHandle != null)
            _dragHandle.MouseLeftButtonDown -= OnDragStart;
        if (_sheetPanel != null)
        {
            _sheetPanel.MouseMove         -= OnDragMove;
            _sheetPanel.MouseLeftButtonUp -= OnDragEnd;
        }

        _scrim      = GetTemplateChild(PartScrim)      as UIElement;
        _sheetPanel = GetTemplateChild(PartSheetPanel) as FrameworkElement;
        _dragHandle = GetTemplateChild(PartDragHandle) as UIElement;

        if (_scrim != null)
            _scrim.MouseLeftButtonDown += OnScrimClicked;

        // 拖拽起始只监听手柄区域，避免内容子元素吃掉事件
        if (_dragHandle != null && EnableDragToClose)
        {
            _dragHandle.MouseLeftButtonDown += OnDragStart;
            // 悬停显示张开手掌光标
            if (_dragHandle is FrameworkElement feDragHandle)
                feDragHandle.Cursor = _cursorGrab ?? Cursors.Hand;
        }

        // Move / Up 挂在 sheetPanel：CaptureMouse 后可跨子元素收到
        if (_sheetPanel != null && EnableDragToClose)
        {
            _sheetPanel.MouseMove         += OnDragMove;
            _sheetPanel.MouseLeftButtonUp += OnDragEnd;
        }

        _templateApplied = true;
        UpdateState(animate: false);
    }

    private void OnScrimClicked(object sender, MouseButtonEventArgs e)
    {
        if (CloseOnScrimClick) IsOpen = false;
    }

    // ── 拖拽关闭 ──────────────────────────────────────────────────────
    private void OnDragStart(object sender, MouseButtonEventArgs e)
    {
        if (!EnableDragToClose || _sheetPanel == null) return;
        _isDragging = true;
        _dragStartY = e.GetPosition(this).Y;

        // 清除动画（FillBehavior=HoldEnd 会持有属性，直接赋值无效）
        // 面板打开时基准始终是 0，不读 t.Y（SnapBack 动画持有期间本地值仍为拖拽偏移）
        var t = EnsureTransform();
        t.BeginAnimation(TranslateTransform.YProperty, null);
        t.Y = 0;
        _dragStartTranslateY = 0;

        _sheetPanel.CaptureMouse();
        Mouse.OverrideCursor = _cursorGrabbing ?? Cursors.ScrollAll; // 按下时用抓握光标
        e.Handled = true;
    }

    private void OnDragMove(object sender, MouseEventArgs e)
    {
        if (!_isDragging || _sheetPanel == null || e.LeftButton != MouseButtonState.Pressed) return;

        var deltaY    = e.GetPosition(this).Y - _dragStartY;
        var translateY = _dragStartTranslateY + deltaY;
        // 只允许向下拖（正方向）
        if (translateY < 0) translateY = 0;
        SetTranslateY(translateY);
    }

    private void OnDragEnd(object sender, MouseButtonEventArgs e)
    {
        if (!_isDragging || _sheetPanel == null) return;
        _isDragging = false;
        _sheetPanel.ReleaseMouseCapture();
        Mouse.OverrideCursor = null; // 恢复光标

        var threshold = (_sheetPanel.ActualHeight > 0 ? _sheetPanel.ActualHeight : 200) * 0.35;
        var current   = GetTranslateY();

        if (current > threshold)
            IsOpen = false;
        else
            SnapBack();
    }

    private void SnapBack()
    {
        if (_sheetPanel == null) return;
        var transform = EnsureTransform();
        var from      = transform.Y;
        var anim      = new DoubleAnimation(from, 0, new Duration(TimeSpan.FromMilliseconds(250)))
                        { EasingFunction = new CubicEase { EasingMode = EasingMode.EaseOut } };
        // 动画结束后清除动画持有并显式归零，防止下次 BeginAnimation(null) 恢复出残留的本地值
        anim.Completed += (_, _) =>
        {
            transform.BeginAnimation(TranslateTransform.YProperty, null);
            transform.Y = 0;
        };
        transform.BeginAnimation(TranslateTransform.YProperty, anim);
    }

    // ── 动画 ──────────────────────────────────────────────────────────
    private void UpdateState(bool animate)
    {
        if (!_templateApplied || _sheetPanel == null) return;

        var generation = ++_animationGeneration;
        var duration   = new Duration(TimeSpan.FromMilliseconds(animate ? 300 : 0));
        var ease       = new CubicEase { EasingMode = EasingMode.EaseOut };

        var isModal = Variant == BottomSheetVariant.Modal;

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
        var panelH  = _sheetPanel.ActualHeight > 0 ? _sheetPanel.ActualHeight : 400;
        var closedY = panelH + 8;

        // 关闭时从当前实际位置出发（拖拽后面板可能已有偏移），避免先跳回 0 再收缩
        var from = closing ? transform.Y : closedY;
        var to   = closing ? closedY     : 0;

        var anim = new DoubleAnimation(from, to, duration) { EasingFunction = ease };
        if (completed != null) anim.Completed += (_, _) => completed();
        transform.BeginAnimation(TranslateTransform.YProperty, anim);
    }

    private TranslateTransform EnsureTransform()
    {
        if (_sheetPanel!.RenderTransform is TranslateTransform t)
        {
            if (!t.IsFrozen) return t;
            var clone = t.Clone();
            _sheetPanel.RenderTransform = clone;
            return clone;
        }
        var transform = new TranslateTransform();
        _sheetPanel.RenderTransform = transform;
        return transform;
    }

    private double GetTranslateY()
        => _sheetPanel?.RenderTransform is TranslateTransform t ? t.Y : 0;

    private void SetTranslateY(double y)
        => EnsureTransform().Y = y;
}

/// <summary>Bottom Sheet 变体。</summary>
public enum BottomSheetVariant
{
    /// <summary>模态（有遮罩），点击遮罩或下拉可关闭。</summary>
    Modal,
    /// <summary>标准（无遮罩），覆盖在内容上方，用户可与底层内容交互。</summary>
    Standard,
}
