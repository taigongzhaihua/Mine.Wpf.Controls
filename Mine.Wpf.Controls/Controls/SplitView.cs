using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;

namespace Mine.Wpf.Controls.Controls;

/// <summary>
/// SplitView：包含 Pane（侧边栏）和 Content（主内容区）的布局容器。
/// 支持 Overlay / Inline / CompactOverlay / CompactInline 四种显示模式。
/// </summary>
[TemplatePart(Name = PartPaneRoot,    Type = typeof(FrameworkElement))]
[TemplatePart(Name = PartContentRoot, Type = typeof(FrameworkElement))]
[TemplatePart(Name = PartScrim,       Type = typeof(Rectangle))]
public class SplitView : ContentControl
{
    private const string PartPaneRoot    = "PART_PaneRoot";
    private const string PartContentRoot = "PART_ContentRoot";
    private const string PartScrim       = "PART_Scrim";

    private static readonly Duration AnimDuration = new(TimeSpan.FromMilliseconds(250));
    private static readonly IEasingFunction Ease   = new CubicEase { EasingMode = EasingMode.EaseOut };

    static SplitView()
    {
        DefaultStyleKeyProperty.OverrideMetadata(
            typeof(SplitView),
            new FrameworkPropertyMetadata(typeof(SplitView)));
    }

    private FrameworkElement? _paneRoot;
    private FrameworkElement? _contentRoot;
    private Rectangle?        _scrim;
    private TranslateTransform _paneTranslate = new();

    // ── Pane 内容 ─────────────────────────────────────────────────────
    public static readonly DependencyProperty PaneProperty =
        DependencyProperty.Register(nameof(Pane), typeof(object), typeof(SplitView),
            new PropertyMetadata(null));

    public object? Pane
    {
        get => GetValue(PaneProperty);
        set => SetValue(PaneProperty, value);
    }

    // ── Pane DataTemplate ─────────────────────────────────────────────
    public static readonly DependencyProperty PaneTemplateProperty =
        DependencyProperty.Register(nameof(PaneTemplate), typeof(DataTemplate), typeof(SplitView),
            new PropertyMetadata(null));

    public DataTemplate? PaneTemplate
    {
        get => (DataTemplate?)GetValue(PaneTemplateProperty);
        set => SetValue(PaneTemplateProperty, value);
    }

    // ── IsPaneOpen ────────────────────────────────────────────────────
    public static readonly DependencyProperty IsPaneOpenProperty =
        DependencyProperty.Register(nameof(IsPaneOpen), typeof(bool), typeof(SplitView),
            new FrameworkPropertyMetadata(false,
                FrameworkPropertyMetadataOptions.BindsTwoWayByDefault,
                OnIsPaneOpenChanged));

    public bool IsPaneOpen
    {
        get => (bool)GetValue(IsPaneOpenProperty);
        set => SetValue(IsPaneOpenProperty, value);
    }

    private static void OnIsPaneOpenChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        => ((SplitView)d).ApplyState(animate: true);

    // ── OpenPaneLength ────────────────────────────────────────────────
    public static readonly DependencyProperty OpenPaneLengthProperty =
        DependencyProperty.Register(nameof(OpenPaneLength), typeof(double), typeof(SplitView),
            new PropertyMetadata(320.0, OnLayoutPropertyChanged));

    public double OpenPaneLength
    {
        get => (double)GetValue(OpenPaneLengthProperty);
        set => SetValue(OpenPaneLengthProperty, value);
    }

    // ── CompactPaneLength ─────────────────────────────────────────────
    public static readonly DependencyProperty CompactPaneLengthProperty =
        DependencyProperty.Register(nameof(CompactPaneLength), typeof(double), typeof(SplitView),
            new PropertyMetadata(50.0, OnLayoutPropertyChanged));

    public double CompactPaneLength
    {
        get => (double)GetValue(CompactPaneLengthProperty);
        set => SetValue(CompactPaneLengthProperty, value);
    }

    // ── DisplayMode ───────────────────────────────────────────────────
    public static readonly DependencyProperty DisplayModeProperty =
        DependencyProperty.Register(nameof(DisplayMode), typeof(SplitViewDisplayMode), typeof(SplitView),
            new PropertyMetadata(SplitViewDisplayMode.Overlay, OnLayoutPropertyChanged));

    public SplitViewDisplayMode DisplayMode
    {
        get => (SplitViewDisplayMode)GetValue(DisplayModeProperty);
        set => SetValue(DisplayModeProperty, value);
    }

    // ── PaneBackground ────────────────────────────────────────────────
    public static readonly DependencyProperty PaneBackgroundProperty =
        DependencyProperty.Register(nameof(PaneBackground), typeof(Brush), typeof(SplitView),
            new PropertyMetadata(null));

    public Brush? PaneBackground
    {
        get => (Brush?)GetValue(PaneBackgroundProperty);
        set => SetValue(PaneBackgroundProperty, value);
    }

    private static void OnLayoutPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        => ((SplitView)d).ApplyState(animate: false);

    // ── OnApplyTemplate ───────────────────────────────────────────────
    public override void OnApplyTemplate()
    {
        base.OnApplyTemplate();

        // 解绑旧事件
        if (_scrim != null)
            _scrim.MouseLeftButtonDown -= OnScrimClicked;

        _paneRoot    = GetTemplateChild(PartPaneRoot)    as FrameworkElement;
        _contentRoot = GetTemplateChild(PartContentRoot) as FrameworkElement;
        _scrim       = GetTemplateChild(PartScrim)       as Rectangle;

        if (_paneRoot != null)
        {
            _paneTranslate = new TranslateTransform();
            _paneRoot.RenderTransform = _paneTranslate;
        }

        if (_scrim != null)
            _scrim.MouseLeftButtonDown += OnScrimClicked;

        ApplyState(animate: false);
    }

    // ── 点击 Scrim 关闭 ───────────────────────────────────────────────
    private void OnScrimClicked(object sender, MouseButtonEventArgs e)
        => IsPaneOpen = false;

    // ── 状态应用 ──────────────────────────────────────────────────────
    /// <summary>
    /// 根据当前 DisplayMode / IsPaneOpen 重新计算并应用布局状态。
    /// 标记为 internal，供 NavigationView 在完成所有属性变更后显式调用，
    /// 避免中间状态下多次触发导致的 HoldEnd 动画干扰问题。
    /// </summary>
    internal void ApplyState(bool animate)
    {
        if (_paneRoot == null || _contentRoot == null) return;

        switch (DisplayMode)
        {
            case SplitViewDisplayMode.Overlay:
                ApplyOverlay(animate);
                break;
            case SplitViewDisplayMode.CompactOverlay:
                ApplyCompactOverlay(animate);
                break;
            case SplitViewDisplayMode.Inline:
                ApplyInline(animate, compactWidth: 0);
                break;
            case SplitViewDisplayMode.CompactInline:
                ApplyInline(animate, compactWidth: CompactPaneLength);
                break;
        }
    }

    /// <summary>
    /// Overlay：Pane 以平移动画从左侧滑入，关闭时完全隐藏（translateX = -OpenPaneLength）。
    /// 内容区不留边距，始终铺满。
    /// </summary>
    private void ApplyOverlay(bool animate)
    {
        var open = IsPaneOpen;

        // 清除其他模式可能遗留的动画（WPF HoldEnd 会阻止直接赋值生效）
        _paneRoot!.BeginAnimation(WidthProperty, null);
        _contentRoot!.BeginAnimation(MarginProperty, null);

        _paneRoot.Width = OpenPaneLength;
        Panel.SetZIndex(_paneRoot, 10);
        _contentRoot.Margin = new Thickness(0);

        var targetX            = open ? 0 : -OpenPaneLength;
        var targetScrimOpacity = open ? 0.32 : 0;

        if (animate)
        {
            _paneTranslate.BeginAnimation(TranslateTransform.XProperty,
                new DoubleAnimation(targetX, AnimDuration) { EasingFunction = Ease });
            _scrim?.BeginAnimation(OpacityProperty,
                new DoubleAnimation(targetScrimOpacity, AnimDuration) { EasingFunction = Ease });
        }
        else
        {
            _paneTranslate.BeginAnimation(TranslateTransform.XProperty, null);
            _paneTranslate.X = targetX;
            if (_scrim != null) { _scrim.BeginAnimation(OpacityProperty, null); _scrim.Opacity = targetScrimOpacity; }
        }

        if (_scrim != null) _scrim.IsHitTestVisible = open;
    }

    /// <summary>
    /// CompactOverlay：Pane 始终可见，关闭时宽度=CompactPaneLength，打开时就地展开到 OpenPaneLength（叠加内容，带 Scrim）。
    /// 内容区左边距始终 = CompactPaneLength，不随开关改变。
    /// </summary>
    private void ApplyCompactOverlay(bool animate)
    {
        var open = IsPaneOpen;

        Panel.SetZIndex(_paneRoot!, 10);
        // 平移归零（不做平移）
        _paneTranslate.BeginAnimation(TranslateTransform.XProperty, null);
        _paneTranslate.X = 0;

        // 内容区边距固定，不随 Pane 展开而变化
        _contentRoot!.BeginAnimation(MarginProperty, null);
        _contentRoot.Margin = new Thickness(CompactPaneLength, 0, 0, 0);

        var targetWidth        = open ? OpenPaneLength : CompactPaneLength;
        var targetScrimOpacity = open ? 0.32 : 0;

        if (animate)
        {
            _paneRoot!.BeginAnimation(WidthProperty,
                new DoubleAnimation(targetWidth, AnimDuration) { EasingFunction = Ease });
            _scrim?.BeginAnimation(OpacityProperty,
                new DoubleAnimation(targetScrimOpacity, AnimDuration) { EasingFunction = Ease });
        }
        else
        {
            _paneRoot!.BeginAnimation(WidthProperty, null);
            _paneRoot.Width = targetWidth;
            if (_scrim != null) { _scrim.BeginAnimation(OpacityProperty, null); _scrim.Opacity = targetScrimOpacity; }
        }

        if (_scrim != null) _scrim.IsHitTestVisible = open;
    }

    /// <summary>
    /// Inline / CompactInline：Pane 挤压内容区，不叠加，不显示 Scrim。
    /// compactWidth=0 → Inline（完全隐藏）；compactWidth>0 → CompactInline（保留细条）。
    /// </summary>
    private void ApplyInline(bool animate, double compactWidth)
    {
        var open = IsPaneOpen;

        var targetPaneWidth   = open ? OpenPaneLength : compactWidth;
        var    targetContentMargin = new Thickness(targetPaneWidth, 0, 0, 0);

        Panel.SetZIndex(_paneRoot!, 0);
        _paneTranslate.BeginAnimation(TranslateTransform.XProperty, null);
        _paneTranslate.X = 0;

        if (_scrim != null)
        {
            _scrim.BeginAnimation(OpacityProperty, null);
            _scrim.Opacity = 0;
            _scrim.IsHitTestVisible = false;
        }

        if (animate)
        {
            _paneRoot!.BeginAnimation(WidthProperty,
                new DoubleAnimation(targetPaneWidth, AnimDuration) { EasingFunction = Ease });
            _contentRoot!.BeginAnimation(MarginProperty,
                new ThicknessAnimation(targetContentMargin, AnimDuration) { EasingFunction = Ease });
        }
        else
        {
            _paneRoot!.BeginAnimation(WidthProperty, null);
            _paneRoot.Width = targetPaneWidth;
            _contentRoot!.BeginAnimation(MarginProperty, null);
            _contentRoot.Margin = targetContentMargin;
        }
    }

    // ── 公共 API ──────────────────────────────────────────────────────
    public void OpenPane()  => IsPaneOpen = true;
    public void ClosePane() => IsPaneOpen = false;
    public void TogglePane() => IsPaneOpen = !IsPaneOpen;
}

/// <summary>SplitView 显示模式。</summary>
public enum SplitViewDisplayMode
{
    /// <summary>Pane 完全叠加在内容上，关闭时完全隐藏。</summary>
    Overlay,
    /// <summary>Pane 与内容并排，关闭时完全隐藏（宽度=0）。</summary>
    Inline,
    /// <summary>Pane 叠加，关闭时保留 CompactPaneLength 宽度的细条。</summary>
    CompactOverlay,
    /// <summary>Pane 与内容并排，关闭时保留 CompactPaneLength 宽度的细条。</summary>
    CompactInline,
}

