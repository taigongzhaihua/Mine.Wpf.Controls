using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Animation;
using System.Windows.Shapes;

namespace Mine.Wpf.Controls.Controls;

/// <summary>
/// Material Design 3 风格的可展开控件,支持流畅的展开/收起动画。
/// </summary>
[TemplateVisualState(Name = "Expanded",  GroupName = "ExpansionStates")]
[TemplateVisualState(Name = "Collapsed", GroupName = "ExpansionStates")]
[TemplatePart(Name = PartHeaderSite, Type = typeof(FrameworkElement))]
[TemplatePart(Name = PartStateLayerBorder, Type = typeof(Border))]
public class Expander : HeaderedContentControl
{
    private const string PartHeaderSite = "PART_HeaderSite";
    private const string PartStateLayerBorder = "StateLayerBorder";
    private FrameworkElement? _headerSite;
    private Border? _stateLayerBorder;
    private bool _isHeaderPressed;

    static Expander()
    {
        DefaultStyleKeyProperty.OverrideMetadata(
            typeof(Expander),
            new FrameworkPropertyMetadata(typeof(Expander)));
    }

    // ── IsExpanded ─────────────────────────────────────────────────
    public static readonly DependencyProperty IsExpandedProperty =
        DependencyProperty.Register(
            nameof(IsExpanded),
            typeof(bool),
            typeof(Expander),
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
        if (d is Expander expander)
        {
            expander.OnIsExpandedChanged((bool)e.OldValue, (bool)e.NewValue);
        }
    }

    protected virtual void OnIsExpandedChanged(bool oldValue, bool newValue)
    {
        UpdateVisualState(true);
        UpdateStateLayerCornerRadius();

        if (newValue)
        {
            RaiseEvent(new RoutedEventArgs(ExpandedEvent, this));
        }
        else
        {
            RaiseEvent(new RoutedEventArgs(CollapsedEvent, this));
        }
    }

    // ── ExpandDirection ────────────────────────────────────────────
    public static readonly DependencyProperty ExpandDirectionProperty =
        DependencyProperty.Register(
            nameof(ExpandDirection),
            typeof(ExpandDirection),
            typeof(Expander),
            new PropertyMetadata(ExpandDirection.Down));

    public ExpandDirection ExpandDirection
    {
        get => (ExpandDirection)GetValue(ExpandDirectionProperty);
        set => SetValue(ExpandDirectionProperty, value);
    }

    // ── CornerRadius ───────────────────────────────────────────────
    public static readonly DependencyProperty CornerRadiusProperty =
        DependencyProperty.Register(
            nameof(CornerRadius),
            typeof(CornerRadius),
            typeof(Expander),
            new PropertyMetadata(new CornerRadius(12)));

    public CornerRadius CornerRadius
    {
        get => (CornerRadius)GetValue(CornerRadiusProperty);
        set => SetValue(CornerRadiusProperty, value);
    }

    // ── Elevation ──────────────────────────────────────────────────
    public static readonly DependencyProperty ElevationProperty =
        DependencyProperty.Register(
            nameof(Elevation),
            typeof(int),
            typeof(Expander),
            new PropertyMetadata(0));

    public int Elevation
    {
        get => (int)GetValue(ElevationProperty);
        set => SetValue(ElevationProperty, value);
    }

    // ── AnimationDuration ──────────────────────────────────────────
    public static readonly DependencyProperty AnimationDurationProperty =
        DependencyProperty.Register(
            nameof(AnimationDuration),
            typeof(Duration),
            typeof(Expander),
            new PropertyMetadata(new Duration(TimeSpan.FromMilliseconds(300))));

    public Duration AnimationDuration
    {
        get => (Duration)GetValue(AnimationDurationProperty);
        set => SetValue(AnimationDurationProperty, value);
    }

    // ── Routed Events ──────────────────────────────────────────────
    public static readonly RoutedEvent ExpandedEvent =
        EventManager.RegisterRoutedEvent(
            nameof(Expanded),
            RoutingStrategy.Bubble,
            typeof(RoutedEventHandler),
            typeof(Expander));

    public event RoutedEventHandler Expanded
    {
        add => AddHandler(ExpandedEvent, value);
        remove => RemoveHandler(ExpandedEvent, value);
    }

    public static readonly RoutedEvent CollapsedEvent =
        EventManager.RegisterRoutedEvent(
            nameof(Collapsed),
            RoutingStrategy.Bubble,
            typeof(RoutedEventHandler),
            typeof(Expander));

    public event RoutedEventHandler Collapsed
    {
        add => AddHandler(CollapsedEvent, value);
        remove => RemoveHandler(CollapsedEvent, value);
    }

    // ── Overrides ──────────────────────────────────────────────────
    public override void OnApplyTemplate()
    {
        // 清理旧的事件处理
        if (_headerSite != null)
        {
            _headerSite.MouseEnter -= OnHeaderMouseEnter;
            _headerSite.MouseLeave -= OnHeaderMouseLeave;
            _headerSite.MouseLeftButtonDown -= OnHeaderMouseLeftButtonDown;
            _headerSite.MouseLeftButtonUp -= OnHeaderMouseLeftButtonUp;
        }

        base.OnApplyTemplate();

        // 获取标题区域并绑定事件
        _headerSite = GetTemplateChild(PartHeaderSite) as FrameworkElement;
        if (_headerSite != null)
        {
            _headerSite.MouseEnter += OnHeaderMouseEnter;
            _headerSite.MouseLeave += OnHeaderMouseLeave;
            _headerSite.MouseLeftButtonDown += OnHeaderMouseLeftButtonDown;
            _headerSite.MouseLeftButtonUp += OnHeaderMouseLeftButtonUp;
        }

        // 获取状态层边框
        _stateLayerBorder = GetTemplateChild(PartStateLayerBorder) as Border;

        UpdateVisualState(false);
        UpdateStateLayerCornerRadius();
    }

    // ── Header Mouse Events ────────────────────────────────────────
    private void OnHeaderMouseEnter(object sender, MouseEventArgs e)
    {
        VisualStateManager.GoToState(this, "MouseOver", true);
    }

    private void OnHeaderMouseLeave(object sender, MouseEventArgs e)
    {
        _isHeaderPressed = false;
        VisualStateManager.GoToState(this, "Normal", true);
    }

    private void OnHeaderMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        _isHeaderPressed = true;
        VisualStateManager.GoToState(this, "Pressed", true);
        e.Handled = true;
    }

    private void OnHeaderMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
    {
        if (_isHeaderPressed)
        {
            _isHeaderPressed = false;
            IsExpanded = !IsExpanded;

            // 恢复到 MouseOver 状态（如果鼠标仍在标题区域）
            if (_headerSite?.IsMouseOver == true)
            {
                VisualStateManager.GoToState(this, "MouseOver", true);
            }
            else
            {
                VisualStateManager.GoToState(this, "Normal", true);
            }
        }
        e.Handled = true;
    }

    // ── Visual State Management ────────────────────────────────────
    private void UpdateVisualState(bool useTransitions)
    {
        string expansionState = IsExpanded ? "Expanded" : "Collapsed";
        VisualStateManager.GoToState(this, expansionState, useTransitions);
    }

    private void UpdateStateLayerCornerRadius()
    {
        if (_stateLayerBorder == null) return;

        var cr = CornerRadius;
        _stateLayerBorder.CornerRadius = IsExpanded
            ? new CornerRadius(cr.TopLeft, cr.TopRight, 0, 0)
            : cr;
    }
}
