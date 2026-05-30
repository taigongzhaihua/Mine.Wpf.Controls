using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;

namespace Mine.Wpf.Controls.Controls;

/// <summary>Material Design 3 圆形进度环。</summary>
[TemplatePart(Name = PartIndicator,            Type = typeof(Ellipse))]
[TemplatePart(Name = PartIndeterminateRing,    Type = typeof(Path))]
[TemplateVisualState(Name = "Determinate",     GroupName = "ProgressStates")]
[TemplateVisualState(Name = "Indeterminate",   GroupName = "ProgressStates")]
public class ProgressRing : System.Windows.Controls.Control
{
    public const string PartIndicator         = "PART_Indicator";
    public const string PartIndeterminateRing = "IndeterminateRing";

    static ProgressRing()
    {
        DefaultStyleKeyProperty.OverrideMetadata(
            typeof(ProgressRing),
            new FrameworkPropertyMetadata(typeof(ProgressRing)));
    }

    // ── Value ──────────────────────────────────────────────────────
    public static readonly DependencyProperty ValueProperty =
        DependencyProperty.Register(nameof(Value), typeof(double), typeof(ProgressRing),
            new PropertyMetadata(0d, OnProgressChanged));

    public double Value
    {
        get => (double)GetValue(ValueProperty);
        set => SetValue(ValueProperty, value);
    }

    // ── Minimum ────────────────────────────────────────────────────
    public static readonly DependencyProperty MinimumProperty =
        DependencyProperty.Register(nameof(Minimum), typeof(double), typeof(ProgressRing),
            new PropertyMetadata(0d, OnProgressChanged));

    public double Minimum
    {
        get => (double)GetValue(MinimumProperty);
        set => SetValue(MinimumProperty, value);
    }

    // ── Maximum ────────────────────────────────────────────────────
    public static readonly DependencyProperty MaximumProperty =
        DependencyProperty.Register(nameof(Maximum), typeof(double), typeof(ProgressRing),
            new PropertyMetadata(100d, OnProgressChanged));

    public double Maximum
    {
        get => (double)GetValue(MaximumProperty);
        set => SetValue(MaximumProperty, value);
    }

    // ── IsIndeterminate ────────────────────────────────────────────
    public static readonly DependencyProperty IsIndeterminateProperty =
        DependencyProperty.Register(nameof(IsIndeterminate), typeof(bool), typeof(ProgressRing),
            new PropertyMetadata(false, OnIndeterminateChanged));

    public bool IsIndeterminate
    {
        get => (bool)GetValue(IsIndeterminateProperty);
        set => SetValue(IsIndeterminateProperty, value);
    }

    // ── StrokeThickness ───────────────────────────────────────────
    public static readonly DependencyProperty StrokeThicknessProperty =
        DependencyProperty.Register(nameof(StrokeThickness), typeof(double), typeof(ProgressRing),
            new PropertyMetadata(4d, OnProgressChanged));

    public double StrokeThickness
    {
        get => (double)GetValue(StrokeThicknessProperty);
        set => SetValue(StrokeThicknessProperty, value);
    }

    // ── StrokeDashArray (readonly, computed) ──────────────────────
    private static readonly DependencyPropertyKey StrokeDashArrayPropertyKey =
        DependencyProperty.RegisterReadOnly(nameof(StrokeDashArray), typeof(DoubleCollection), typeof(ProgressRing),
            new PropertyMetadata(null));

    public static readonly DependencyProperty StrokeDashArrayProperty = StrokeDashArrayPropertyKey.DependencyProperty;
    public DoubleCollection StrokeDashArray => (DoubleCollection)GetValue(StrokeDashArrayProperty);

    // ── StrokeDashOffset (readonly, computed) ─────────────────────
    private static readonly DependencyPropertyKey StrokeDashOffsetPropertyKey =
        DependencyProperty.RegisterReadOnly(nameof(StrokeDashOffset), typeof(double), typeof(ProgressRing),
            new PropertyMetadata(0d));

    public static readonly DependencyProperty StrokeDashOffsetProperty = StrokeDashOffsetPropertyKey.DependencyProperty;
    public double StrokeDashOffset => (double)GetValue(StrokeDashOffsetProperty);

    // ── 内部 DP：尾端位置（0→1 匀速，对应 0°→360°） ─────────────
    private static readonly DependencyProperty IndTailFractionProperty =
        DependencyProperty.Register("IndTailFraction", typeof(double), typeof(ProgressRing),
            new PropertyMetadata(0d, OnIndAnimChanged));

    // ── 内部 DP：弧长比例（-1→+1 匀速，-360°→+360°） ────────────
    private static readonly DependencyProperty IndArcFractionProperty =
        DependencyProperty.Register("IndArcFraction", typeof(double), typeof(ProgressRing),
            new PropertyMetadata(-1d, OnIndAnimChanged));

    private static void OnIndAnimChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        => ((ProgressRing)d).ApplyIndAnimation();

    // ── Callbacks ─────────────────────────────────────────────────
    private static void OnProgressChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        => ((ProgressRing)d).UpdateDashArray();

    private static void OnIndeterminateChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        var ring = (ProgressRing)d;
        ring.UpdateDashArray();
        ring.UpdateProgressState(true);
        if ((bool)e.NewValue) ring.StartIndAnimation();
        else                  ring.StopIndAnimation();
    }

    // ── Template references ─────────────────────────────��─────────
    private Ellipse?      _detRing;
    private Path?         _indPath;
    private PathFigure?   _indFigure;
    private ArcSegment?   _indArc;
    private Storyboard?   _indStoryboard;

    // ── Lifecycle ──────────────────────��──────────────────────────
    public override void OnApplyTemplate()
    {
        base.OnApplyTemplate();

        _detRing = GetTemplateChild("DeterminateRing") as Ellipse;
        _indPath = GetTemplateChild(PartIndeterminateRing) as Path;

        if (_indPath != null)
        {
            _indFigure = new PathFigure { IsClosed = false };
            _indArc    = new ArcSegment { SweepDirection = SweepDirection.Clockwise };
            _indFigure.Segments.Add(_indArc);
            var geo = new PathGeometry();
            geo.Figures.Add(_indFigure);
            _indPath.Data = geo;
        }

        UpdateDashArray();
        UpdateProgressState(false);
        if (IsIndeterminate) StartIndAnimation();
    }

    protected override void OnRenderSizeChanged(SizeChangedInfo sizeInfo)
    {
        base.OnRenderSizeChanged(sizeInfo);
        UpdateDashArray();
        if (IsIndeterminate) ApplyIndAnimation(); // 尺寸变化时重算弧几何
    }

    // ── Determinate dash array ────────────────────────────────────
    private void UpdateDashArray()
    {
        if (IsIndeterminate) return;

        var size = Math.Min(ActualWidth, ActualHeight);
        if (size <= 0) size = Width > 0 ? Width : Height > 0 ? Height : 48;
        var t = StrokeThickness;
        var radius = (size - t) / 2.0;
        if (radius <= 0) return;

        var circumference = 2 * Math.PI * radius;
        var range    = Maximum - Minimum;
        var progress = range <= 0 ? 0 : (Value - Minimum) / range;
        progress = Math.Max(0, Math.Min(1, progress));

        var dashLen = circumference * progress / t;
        var gapLen  = circumference / t;

        SetValue(StrokeDashArrayPropertyKey,  new DoubleCollection { dashLen, gapLen });
        SetValue(StrokeDashOffsetPropertyKey, circumference / 4.0 / t);
    }

    // ── Indeterminate: Path geometry ──────────────────────────────
    private void ApplyIndAnimation()
    {
        if (_indPath == null || _indFigure == null || _indArc == null) return;

        var tailFrac = (double)GetValue(IndTailFractionProperty); // 0→1
        var arcFrac  = (double)GetValue(IndArcFractionProperty);  // -1→+1

        var size = Math.Min(ActualWidth, ActualHeight);
        if (size <= 0) size = Width > 0 ? Width : Height > 0 ? Height : 48;
        var t      = StrokeThickness;
        var radius = (size - t) / 2.0;
        if (radius <= 0) return;
        var cx = size / 2.0;
        var cy = size / 2.0;

        // tail angle from 12 o'clock (degrees)
        var tailDeg = tailFrac * 360.0;
        var arcDeg  = arcFrac  * 360.0;

        // 始终顺时针绘制弧，clamp 避免整圆退化
        var arcAbsDeg = Math.Abs(arcDeg);
        if (arcAbsDeg < 0.1)   arcAbsDeg = 0.1;
        if (arcAbsDeg > 359.9) arcAbsDeg = 359.9;
        // arcFrac≥0���弧从尾端向前延伸；arcFrac<0：弧在尾端之前（逆���针方向）
        var startDeg = arcDeg >= 0 ? tailDeg : tailDeg - arcAbsDeg;
        var endDeg   = startDeg + arcAbsDeg;

        static Point AngleToPoint(double cx, double cy, double r, double deg)
        {
            var rad = (deg - 90) * Math.PI / 180.0;
            return new Point(cx + r * Math.Cos(rad), cy + r * Math.Sin(rad));
        }

        _indFigure.StartPoint = AngleToPoint(cx, cy, radius, startDeg);
        _indArc.Point       = AngleToPoint(cx, cy, radius, endDeg);
        _indArc.Size        = new Size(radius, radius);
        _indArc.IsLargeArc  = arcAbsDeg > 180;
    }

    private void StartIndAnimation()
    {
        StopIndAnimation();
        if (_indPath == null) return;

        const double period = 2.0;

        var tailAnim = new DoubleAnimation(0, 1, new Duration(TimeSpan.FromSeconds(period)))
            { RepeatBehavior = RepeatBehavior.Forever };
        var arcAnim  = new DoubleAnimation(-1, 1, new Duration(TimeSpan.FromSeconds(period)))
            { RepeatBehavior = RepeatBehavior.Forever };

        var sb = new Storyboard();
        Storyboard.SetTarget(tailAnim, this);
        Storyboard.SetTargetProperty(tailAnim, new PropertyPath(IndTailFractionProperty));
        Storyboard.SetTarget(arcAnim, this);
        Storyboard.SetTargetProperty(arcAnim, new PropertyPath(IndArcFractionProperty));
        sb.Children.Add(tailAnim);
        sb.Children.Add(arcAnim);

        _indStoryboard = sb;
        sb.Begin(this, isControllable: true);
    }

    private void StopIndAnimation()
    {
        _indStoryboard?.Stop(this);
        _indStoryboard = null;
    }

    private void UpdateProgressState(bool useTransitions) =>
        VisualStateManager.GoToState(this,
            IsIndeterminate ? "Indeterminate" : "Determinate",
            useTransitions);
}

