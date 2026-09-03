using System.Windows;
using System.Windows.Automation.Peers;
using System.Windows.Automation.Provider;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Shapes;
using System.Windows.Threading;
namespace Mine.Wpf.Controls.Controls;
public enum RangeSliderValueLabelMode
{
    Hidden,
    Hover,
    Drag,
    HoverOrDrag,
    Always
}
/// <summary>
/// Material 3 双端滑块，支持选取范围区间 [RangeStart, RangeEnd]。
/// </summary>
[TemplatePart(Name = PartRoot, Type = typeof(FrameworkElement))]
[TemplatePart(Name = PartTrack, Type = typeof(FrameworkElement))]
[TemplatePart(Name = PartTrackLeft, Type = typeof(FrameworkElement))]
[TemplatePart(Name = PartFill, Type = typeof(FrameworkElement))]
[TemplatePart(Name = PartTrackRight, Type = typeof(FrameworkElement))]
[TemplatePart(Name = PartThumbStart, Type = typeof(Thumb))]
[TemplatePart(Name = PartThumbEnd, Type = typeof(Thumb))]
[TemplatePart(Name = PartStartValueLabel, Type = typeof(FrameworkElement))]
[TemplatePart(Name = PartEndValueLabel, Type = typeof(FrameworkElement))]
[TemplatePart(Name = PartStopIndicatorsHost, Type = typeof(Canvas))]
public class RangeSlider : Control
{
    private const string PartRoot = "PART_Root";
    private const string PartTrack = "PART_Track";
    private const string PartTrackLeft = "PART_TrackLeft";
    private const string PartFill = "PART_Fill";
    private const string PartTrackRight = "PART_TrackRight";
    private const string PartThumbStart = "PART_ThumbStart";
    private const string PartThumbEnd = "PART_ThumbEnd";
    private const string PartStartValueLabel = "PART_StartValueLabel";
    private const string PartEndValueLabel = "PART_EndValueLabel";
    private const string PartStopIndicatorsHost = "PART_StopIndicatorsHost";
    private const double ThumbSize = 44;
    private const double ThumbRadius = ThumbSize / 2;
    private const double StopIndicatorSize = 4;
    private const double LabelGap = 8;
    private const double LabelStackSpacing = 4;
    private const double LabelHostHeight = 52;
    private const double LabelTrackSpacing = 4;
    private FrameworkElement? _root;
    private FrameworkElement? _track;
    private FrameworkElement? _trackLeft;
    private FrameworkElement? _fill;
    private FrameworkElement? _trackRight;
    private Thumb? _thumbStart;
    private Thumb? _thumbEnd;
    private FrameworkElement? _startValueLabel;
    private FrameworkElement? _endValueLabel;
    private Canvas? _stopIndicatorsHost;
    private bool _isStartHovered;
    private bool _isEndHovered;
    private bool _isStartDragging;
    private bool _isEndDragging;
    static RangeSlider()
    {
        DefaultStyleKeyProperty.OverrideMetadata(
            typeof(RangeSlider), new FrameworkPropertyMetadata(typeof(RangeSlider)));
    }

    protected override AutomationPeer OnCreateAutomationPeer() => new RangeSliderAutomationPeer(this);
    public static readonly DependencyProperty MinimumProperty =
        DependencyProperty.Register(nameof(Minimum), typeof(double), typeof(RangeSlider),
            new PropertyMetadata(0.0, OnRangeChanged));
    public double Minimum
    {
        get => (double)GetValue(MinimumProperty);
        set => SetValue(MinimumProperty, value);
    }
    public static readonly DependencyProperty MaximumProperty =
        DependencyProperty.Register(nameof(Maximum), typeof(double), typeof(RangeSlider),
            new PropertyMetadata(100.0, OnRangeChanged));
    public double Maximum
    {
        get => (double)GetValue(MaximumProperty);
        set => SetValue(MaximumProperty, value);
    }
    public static readonly DependencyProperty RangeStartProperty =
        DependencyProperty.Register(nameof(RangeStart), typeof(double), typeof(RangeSlider),
            new FrameworkPropertyMetadata(0.0,
                FrameworkPropertyMetadataOptions.BindsTwoWayByDefault,
                OnThumbValueChanged));
    public double RangeStart
    {
        get => (double)GetValue(RangeStartProperty);
        set => SetValue(RangeStartProperty, value);
    }
    public static readonly DependencyProperty RangeEndProperty =
        DependencyProperty.Register(nameof(RangeEnd), typeof(double), typeof(RangeSlider),
            new FrameworkPropertyMetadata(100.0,
                FrameworkPropertyMetadataOptions.BindsTwoWayByDefault,
                OnThumbValueChanged));
    public double RangeEnd
    {
        get => (double)GetValue(RangeEndProperty);
        set => SetValue(RangeEndProperty, value);
    }
    public static readonly DependencyProperty SmallChangeProperty =
        DependencyProperty.Register(nameof(SmallChange), typeof(double), typeof(RangeSlider),
            new PropertyMetadata(1.0));
    public double SmallChange
    {
        get => (double)GetValue(SmallChangeProperty);
        set => SetValue(SmallChangeProperty, value);
    }
    public static readonly DependencyProperty ValueFormatProperty =
        DependencyProperty.Register(nameof(ValueFormat), typeof(string), typeof(RangeSlider),
            new PropertyMetadata("0", OnVisualOptionChanged));
    public string ValueFormat
    {
        get => (string)GetValue(ValueFormatProperty);
        set => SetValue(ValueFormatProperty, value);
    }
    public static readonly DependencyProperty StartValueLabelModeProperty =
        DependencyProperty.Register(nameof(StartValueLabelMode), typeof(RangeSliderValueLabelMode), typeof(RangeSlider),
            new PropertyMetadata(RangeSliderValueLabelMode.Always, OnVisualOptionChanged));
    public RangeSliderValueLabelMode StartValueLabelMode
    {
        get => (RangeSliderValueLabelMode)GetValue(StartValueLabelModeProperty);
        set => SetValue(StartValueLabelModeProperty, value);
    }
    public static readonly DependencyProperty EndValueLabelModeProperty =
        DependencyProperty.Register(nameof(EndValueLabelMode), typeof(RangeSliderValueLabelMode), typeof(RangeSlider),
            new PropertyMetadata(RangeSliderValueLabelMode.Always, OnVisualOptionChanged));
    public RangeSliderValueLabelMode EndValueLabelMode
    {
        get => (RangeSliderValueLabelMode)GetValue(EndValueLabelModeProperty);
        set => SetValue(EndValueLabelModeProperty, value);
    }
    public static readonly DependencyProperty TickFrequencyProperty =
        DependencyProperty.Register(nameof(TickFrequency), typeof(double), typeof(RangeSlider),
            new PropertyMetadata(0.0, OnRangeChanged));
    public double TickFrequency
    {
        get => (double)GetValue(TickFrequencyProperty);
        set => SetValue(TickFrequencyProperty, value);
    }
    public static readonly DependencyProperty IsSnapToTickEnabledProperty =
        DependencyProperty.Register(nameof(IsSnapToTickEnabled), typeof(bool), typeof(RangeSlider),
            new PropertyMetadata(false, OnRangeChanged));
    public bool IsSnapToTickEnabled
    {
        get => (bool)GetValue(IsSnapToTickEnabledProperty);
        set => SetValue(IsSnapToTickEnabledProperty, value);
    }
    public static readonly DependencyProperty ShowStopIndicatorsProperty =
        DependencyProperty.Register(nameof(ShowStopIndicators), typeof(bool), typeof(RangeSlider),
            new PropertyMetadata(false, OnVisualOptionChanged));
    public bool ShowStopIndicators
    {
        get => (bool)GetValue(ShowStopIndicatorsProperty);
        set => SetValue(ShowStopIndicatorsProperty, value);
    }
    private static void OnVisualOptionChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        var ctrl = (RangeSlider)d;
        ctrl.UpdateFormattedValues();
        ctrl.UpdateVisuals();
    }
    private static readonly DependencyPropertyKey FormattedStartKey =
        DependencyProperty.RegisterReadOnly(nameof(FormattedStart), typeof(string), typeof(RangeSlider),
            new PropertyMetadata("0"));
    public static readonly DependencyProperty FormattedStartProperty = FormattedStartKey.DependencyProperty;
    public string FormattedStart => (string)GetValue(FormattedStartProperty);
    private static readonly DependencyPropertyKey FormattedEndKey =
        DependencyProperty.RegisterReadOnly(nameof(FormattedEnd), typeof(string), typeof(RangeSlider),
            new PropertyMetadata("100"));
    public static readonly DependencyProperty FormattedEndProperty = FormattedEndKey.DependencyProperty;
    public string FormattedEnd => (string)GetValue(FormattedEndProperty);
    public static readonly RoutedEvent RangeChangedEvent =
        EventManager.RegisterRoutedEvent(nameof(RangeChanged), RoutingStrategy.Bubble,
            typeof(RoutedEventHandler), typeof(RangeSlider));
    public event RoutedEventHandler RangeChanged
    {
        add => AddHandler(RangeChangedEvent, value);
        remove => RemoveHandler(RangeChangedEvent, value);
    }
    private static void OnRangeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        var ctrl = (RangeSlider)d;
        ctrl.CoerceRangeValues();
        ctrl.UpdateFormattedValues();
        ctrl.UpdateVisuals();
    }
    private static void OnThumbValueChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        var ctrl = (RangeSlider)d;
        ctrl.CoerceRangeValues();
        ctrl.UpdateFormattedValues();
        ctrl.UpdateVisuals();
        ctrl.RaiseEvent(new RoutedEventArgs(RangeChangedEvent, ctrl));
    }
    public override void OnApplyTemplate()
    {
        DetachTemplateHandlers();
        SizeChanged -= OnControlSizeChanged;
        base.OnApplyTemplate();
        _root = GetTemplateChild(PartRoot) as FrameworkElement;
        _track = GetTemplateChild(PartTrack) as FrameworkElement;
        _trackLeft = GetTemplateChild(PartTrackLeft) as FrameworkElement;
        _fill = GetTemplateChild(PartFill) as FrameworkElement;
        _trackRight = GetTemplateChild(PartTrackRight) as FrameworkElement;
        _thumbStart = GetTemplateChild(PartThumbStart) as Thumb;
        _thumbEnd = GetTemplateChild(PartThumbEnd) as Thumb;
        _startValueLabel = GetTemplateChild(PartStartValueLabel) as FrameworkElement;
        _endValueLabel = GetTemplateChild(PartEndValueLabel) as FrameworkElement;
        _stopIndicatorsHost = GetTemplateChild(PartStopIndicatorsHost) as Canvas;
        AttachTemplateHandlers();
        SizeChanged += OnControlSizeChanged;
        CoerceRangeValues();
        UpdateFormattedValues();
        UpdateVisuals();
    }
    private void AttachTemplateHandlers()
    {
        if (_track != null)
            _track.PreviewMouseLeftButtonDown += OnTrackPreviewMouseLeftButtonDown;
        if (_thumbStart != null)
        {
            _thumbStart.DragDelta += OnStartDragDelta;
            _thumbStart.DragStarted += OnThumbStartDragStarted;
            _thumbStart.DragCompleted += OnThumbStartDragCompleted;
            _thumbStart.MouseEnter += OnThumbStartMouseEnter;
            _thumbStart.MouseLeave += OnThumbStartMouseLeave;
        }
        if (_thumbEnd != null)
        {
            _thumbEnd.DragDelta += OnEndDragDelta;
            _thumbEnd.DragStarted += OnThumbEndDragStarted;
            _thumbEnd.DragCompleted += OnThumbEndDragCompleted;
            _thumbEnd.MouseEnter += OnThumbEndMouseEnter;
            _thumbEnd.MouseLeave += OnThumbEndMouseLeave;
        }
    }
    private void DetachTemplateHandlers()
    {
        if (_track != null)
            _track.PreviewMouseLeftButtonDown -= OnTrackPreviewMouseLeftButtonDown;
        if (_thumbStart != null)
        {
            _thumbStart.DragDelta -= OnStartDragDelta;
            _thumbStart.DragStarted -= OnThumbStartDragStarted;
            _thumbStart.DragCompleted -= OnThumbStartDragCompleted;
            _thumbStart.MouseEnter -= OnThumbStartMouseEnter;
            _thumbStart.MouseLeave -= OnThumbStartMouseLeave;
        }
        if (_thumbEnd != null)
        {
            _thumbEnd.DragDelta -= OnEndDragDelta;
            _thumbEnd.DragStarted -= OnThumbEndDragStarted;
            _thumbEnd.DragCompleted -= OnThumbEndDragCompleted;
            _thumbEnd.MouseEnter -= OnThumbEndMouseEnter;
            _thumbEnd.MouseLeave -= OnThumbEndMouseLeave;
        }
    }
    private void OnControlSizeChanged(object sender, SizeChangedEventArgs e) => UpdateVisuals();
    private void OnThumbStartMouseEnter(object sender, MouseEventArgs e)
    {
        _isStartHovered = true;
        UpdateValueLabelStates();
    }
    private void OnThumbStartMouseLeave(object sender, MouseEventArgs e)
    {
        _isStartHovered = false;
        UpdateValueLabelStates();
    }
    private void OnThumbEndMouseEnter(object sender, MouseEventArgs e)
    {
        _isEndHovered = true;
        UpdateValueLabelStates();
    }
    private void OnThumbEndMouseLeave(object sender, MouseEventArgs e)
    {
        _isEndHovered = false;
        UpdateValueLabelStates();
    }
    private void OnThumbStartDragStarted(object sender, DragStartedEventArgs e)
    {
        _isStartDragging = true;
        _thumbStart?.Focus();
        UpdateValueLabelStates();
    }
    private void OnThumbStartDragCompleted(object sender, DragCompletedEventArgs e)
    {
        _isStartDragging = false;
        UpdateValueLabelStates();
    }
    private void OnThumbEndDragStarted(object sender, DragStartedEventArgs e)
    {
        _isEndDragging = true;
        _thumbEnd?.Focus();
        UpdateValueLabelStates();
    }
    private void OnThumbEndDragCompleted(object sender, DragCompletedEventArgs e)
    {
        _isEndDragging = false;
        UpdateValueLabelStates();
    }
    private void OnTrackPreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        if (_track == null || _track.ActualWidth <= 0)
            return;
        if (FindAncestor<Thumb>(e.OriginalSource as DependencyObject) != null)
            return;
        var position = e.GetPosition(_track);
        var fraction = Math.Clamp(position.X / _track.ActualWidth, 0, 1);
        var clickedValue = Minimum + (Maximum - Minimum) * fraction;
        clickedValue = SnapValue(clickedValue);
        if (Math.Abs(clickedValue - RangeStart) <= Math.Abs(clickedValue - RangeEnd))
        {
            RangeStart = Math.Clamp(clickedValue, Minimum, RangeEnd);
            _thumbStart?.Focus();
        }
        else
        {
            RangeEnd = Math.Clamp(clickedValue, RangeStart, Maximum);
            _thumbEnd?.Focus();
        }
        e.Handled = true;
    }
    private void OnStartDragDelta(object sender, DragDeltaEventArgs e)
    {
        if (_track == null || _track.ActualWidth <= 0)
            return;
        var range = Maximum - Minimum;
        if (range <= 0)
            return;
        var delta = e.HorizontalChange / _track.ActualWidth * range;
        var proposed = RangeStart + delta;
        RangeStart = Math.Clamp(SnapValue(proposed), Minimum, RangeEnd);
    }
    private void OnEndDragDelta(object sender, DragDeltaEventArgs e)
    {
        if (_track == null || _track.ActualWidth <= 0)
            return;
        var range = Maximum - Minimum;
        if (range <= 0)
            return;
        var delta = e.HorizontalChange / _track.ActualWidth * range;
        var proposed = RangeEnd + delta;
        RangeEnd = Math.Clamp(SnapValue(proposed), RangeStart, Maximum);
    }
    protected override void OnKeyDown(KeyEventArgs e)
    {
        base.OnKeyDown(e);
        var focused = Keyboard.FocusedElement;
        if (focused == _thumbStart)
        {
            if (e.Key == Key.Left)
            {
                RangeStart = Math.Max(SnapValue(RangeStart - SmallChange), Minimum);
                e.Handled = true;
            }
            else if (e.Key == Key.Right)
            {
                RangeStart = Math.Min(SnapValue(RangeStart + SmallChange), RangeEnd);
                e.Handled = true;
            }
        }
        else if (focused == _thumbEnd)
        {
            if (e.Key == Key.Left)
            {
                RangeEnd = Math.Max(SnapValue(RangeEnd - SmallChange), RangeStart);
                e.Handled = true;
            }
            else if (e.Key == Key.Right)
            {
                RangeEnd = Math.Min(SnapValue(RangeEnd + SmallChange), Maximum);
                e.Handled = true;
            }
        }
    }
    private void UpdateVisuals()
    {
        if (_track == null || _trackLeft == null || _fill == null || _trackRight == null)
            return;
        var trackWidth = _track.ActualWidth;
        var range = Maximum - Minimum;
        if (trackWidth <= 0 || range <= 0)
            return;
        var startFrac = (RangeStart - Minimum) / range;
        var endFrac = (RangeEnd - Minimum) / range;
        var startOffset = startFrac * trackWidth;
        var endOffset = endFrac * trackWidth;
        _trackLeft.Width = Math.Max(0, startOffset);
        _trackLeft.Margin = new Thickness(0, 0, 0, 0);
        _fill.Width = Math.Max(0, endOffset - startOffset);
        _fill.Margin = new Thickness(startOffset, 0, 0, 0);
        _trackRight.Width = Math.Max(0, trackWidth - endOffset);
        _trackRight.Margin = new Thickness(endOffset, 0, 0, 0);
        UpdateStopIndicators(trackWidth, startOffset, endOffset);
        var coordinateRoot = _root ?? this;
        var trackLeft = _track.TranslatePoint(new Point(0, 0), coordinateRoot).X;
        var startCenter = trackLeft + startOffset;
        var endCenter = trackLeft + endOffset;
        if (_thumbStart != null)
            _thumbStart.Margin = new Thickness(startCenter - ThumbRadius, 0, 0, 0);
        if (_thumbEnd != null)
            _thumbEnd.Margin = new Thickness(endCenter - ThumbRadius, 0, 0, 0);
        UpdateValueLabelLayout(startCenter, endCenter);
    }
    private void UpdateValueLabelLayout(double startCenter, double endCenter)
    {
        if (_root == null)
            return;
        var showStart = ShouldShowValueLabel(StartValueLabelMode, _isStartHovered, _isStartDragging);
        var showEnd = ShouldShowValueLabel(EndValueLabelMode, _isEndHovered, _isEndDragging);
        UpdateLabelVisibility(_startValueLabel, showStart);
        UpdateLabelVisibility(_endValueLabel, showEnd);
        if (!showStart && !showEnd)
            return;
        var rootWidth = _root.ActualWidth;
        var startWidth = GetDesiredWidth(_startValueLabel);
        var endWidth = GetDesiredWidth(_endValueLabel);
        var startHeight = GetDesiredHeight(_startValueLabel);
        var endHeight = GetDesiredHeight(_endValueLabel);
        var startLeft = Math.Clamp(startCenter - startWidth / 2, 0, Math.Max(0, rootWidth - startWidth));
        var endLeft = Math.Clamp(endCenter - endWidth / 2, 0, Math.Max(0, rootWidth - endWidth));
        var baseTop = Math.Max(0, LabelHostHeight - Math.Max(startHeight, endHeight) - LabelTrackSpacing);
        var startTop = baseTop;
        var endTop = baseTop;
        if (showStart && showEnd)
        {
            var overlap = (startLeft + startWidth + LabelGap) - endLeft;
            if (overlap > 0)
            {
                startLeft -= overlap / 2;
                endLeft += overlap / 2;
                if (startLeft < 0)
                {
                    endLeft = Math.Min(Math.Max(0, rootWidth - endWidth), endLeft - startLeft);
                    startLeft = 0;
                }
                var maxEndLeft = Math.Max(0, rootWidth - endWidth);
                if (endLeft > maxEndLeft)
                {
                    startLeft = Math.Max(0, startLeft - (endLeft - maxEndLeft));
                    endLeft = maxEndLeft;
                }
            }
            if (startLeft + startWidth + LabelGap > endLeft)
                endTop = Math.Max(0, baseTop - endHeight - LabelStackSpacing);
        }
        if (showStart)
            SetLabelPosition(_startValueLabel!, startLeft, startTop);
        if (showEnd)
            SetLabelPosition(_endValueLabel!, endLeft, endTop);
    }
    private void UpdateValueLabelStates() => Dispatcher.BeginInvoke(DispatcherPriority.Loaded, new Action(UpdateVisuals));
    private void UpdateLabelVisibility(FrameworkElement? label, bool isVisible)
    {
        if (label == null)
            return;
        label.Visibility = isVisible ? Visibility.Visible : Visibility.Collapsed;
    }
    private static void SetLabelPosition(FrameworkElement label, double left, double top)
    {
        Canvas.SetLeft(label, left);
        Canvas.SetTop(label, top);
    }
    private void UpdateStopIndicators(double trackWidth, double startOffset, double endOffset)
    {
        if (_stopIndicatorsHost == null)
            return;
        _stopIndicatorsHost.Children.Clear();
        _stopIndicatorsHost.Visibility = Visibility.Collapsed;
        var frequency = GetEffectiveTickFrequency();
        if (!ShowStopIndicators || frequency <= 0)
            return;
        var range = Maximum - Minimum;
        if (range <= 0)
            return;
        var tickCount = (int)Math.Round(range / frequency) + 1;
        if (tickCount <= 1 || tickCount > 200)
            return;
        _stopIndicatorsHost.Visibility = Visibility.Visible;
        for (var i = 0; i < tickCount; i++)
        {
            var value = Minimum + i * frequency;
            if (value > Maximum)
                value = Maximum;
            var fraction = (value - Minimum) / range;
            var x = fraction * trackWidth - StopIndicatorSize / 2;
            var centerX = x + StopIndicatorSize / 2;
            var isSelected = centerX >= startOffset && centerX <= endOffset;
            var ellipse = new Ellipse
            {
                Width = StopIndicatorSize,
                Height = StopIndicatorSize,
                IsHitTestVisible = false
            };
            ellipse.SetResourceReference(Shape.FillProperty,
                isSelected ? "Mine.Brush.Primary" : "Mine.Brush.OutlineVariant");
            Canvas.SetLeft(ellipse, x);
            Canvas.SetTop(ellipse, ThumbRadius - StopIndicatorSize / 2);
            _stopIndicatorsHost.Children.Add(ellipse);
        }
    }
    private void UpdateFormattedValues()
    {
        SetValue(FormattedStartKey, FormatValue(RangeStart));
        SetValue(FormattedEndKey, FormatValue(RangeEnd));
        Dispatcher.BeginInvoke(DispatcherPriority.Loaded, new Action(UpdateVisuals));
    }
    private string FormatValue(double value)
    {
        try
        {
            if (ValueFormat.Contains("{0"))
                return string.Format(ValueFormat, value);
            return value.ToString(ValueFormat);
        }
        catch
        {
            return value.ToString("0");
        }
    }
    private void CoerceRangeValues()
    {
        if (Maximum < Minimum)
            SetCurrentValue(MaximumProperty, Minimum);
        var start = Math.Clamp(RangeStart, Minimum, Maximum);
        var end = Math.Clamp(RangeEnd, Minimum, Maximum);
        if (IsSnapToTickEnabled)
        {
            start = SnapValue(start);
            end = SnapValue(end);
        }
        if (start > end)
            (start, end) = (end, start);
        if (!AreClose(RangeStart, start))
            SetCurrentValue(RangeStartProperty, start);
        if (!AreClose(RangeEnd, end))
            SetCurrentValue(RangeEndProperty, end);
    }
    private double SnapValue(double value)
    {
        var frequency = GetEffectiveTickFrequency();
        if (!IsSnapToTickEnabled || frequency <= 0)
            return Math.Clamp(value, Minimum, Maximum);
        var snapped = Minimum + Math.Round((value - Minimum) / frequency) * frequency;
        return Math.Clamp(snapped, Minimum, Maximum);
    }
    private double GetEffectiveTickFrequency() => TickFrequency > 0 ? TickFrequency : 0;
    private static bool ShouldShowValueLabel(RangeSliderValueLabelMode mode, bool isHovered, bool isDragging)
        => mode switch
        {
            RangeSliderValueLabelMode.Hidden => false,
            RangeSliderValueLabelMode.Hover => isHovered,
            RangeSliderValueLabelMode.Drag => isDragging,
            RangeSliderValueLabelMode.HoverOrDrag => isHovered || isDragging,
            RangeSliderValueLabelMode.Always => true,
            _ => true
        };
    private static double GetDesiredWidth(FrameworkElement? element)
    {
        if (element == null)
            return 0;
        var width = element.ActualWidth;
        if (width > 0)
            return width;
        element.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));
        return element.DesiredSize.Width;
    }
    private static double GetDesiredHeight(FrameworkElement? element)
    {
        if (element == null)
            return 0;
        var height = element.ActualHeight;
        if (height > 0)
            return height;
        element.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));
        return element.DesiredSize.Height;
    }
    private static T? FindAncestor<T>(DependencyObject? current) where T : DependencyObject
    {
        while (current != null)
        {
            if (current is T match)
                return match;
            current = System.Windows.Media.VisualTreeHelper.GetParent(current);
        }
        return null;
    }
    private static bool AreClose(double a, double b) => Math.Abs(a - b) < 0.0001;
}

/// <summary>
/// 暴露 <see cref="RangeSlider"/> 的区间给屏幕阅读器 / UI 自动化。
/// UIA 无原生双滑块模式，Value 映射到 RangeStart，Name 中附带完整区间描述。
/// </summary>
public class RangeSliderAutomationPeer : FrameworkElementAutomationPeer, IRangeValueProvider
{
    public RangeSliderAutomationPeer(RangeSlider owner) : base(owner) { }

    private RangeSlider Control => (RangeSlider)Owner;

    public override object GetPattern(PatternInterface patternInterface)
        => patternInterface == PatternInterface.RangeValue ? this : base.GetPattern(patternInterface);

    protected override string GetClassNameCore() => nameof(RangeSlider);
    protected override AutomationControlType GetAutomationControlTypeCore() => AutomationControlType.Slider;

    protected override string GetNameCore()
    {
        var name = base.GetNameCore();
        return string.IsNullOrEmpty(name)
            ? $"{Control.RangeStart:G} - {Control.RangeEnd:G}"
            : $"{name}: {Control.RangeStart:G} - {Control.RangeEnd:G}";
    }

    public bool IsReadOnly => !Control.IsEnabled;
    public double Maximum => Control.RangeEnd;
    public double Minimum => Control.Minimum;
    public double LargeChange => Control.SmallChange * 10;
    public double SmallChange => Control.SmallChange;
    public double Value => Control.RangeStart;

    public void SetValue(double value) => Control.RangeStart = Math.Clamp(value, Control.Minimum, Control.RangeEnd);
}
