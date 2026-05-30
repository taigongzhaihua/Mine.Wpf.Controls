using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Media;
using System.Windows.Media.Animation;

namespace Mine.Wpf.Controls.Controls;

/// <summary>
/// Material 3 Flyout：点击触发的轻量弹出层，支持任意内容、淡入+缩放动画、
/// 点击外部自动关闭（light-dismiss）。
/// 通常通过 FlyoutService.Flyout 附加属性挂载到触发元素上。
/// </summary>
[TemplatePart(Name = PartPopup, Type = typeof(Popup))]
[TemplatePart(Name = PartContainer, Type = typeof(FrameworkElement))]
public class Flyout : ContentControl
{
    private const string PartPopup = "PART_Popup";
    private const string PartContainer = "PART_Container";
    private const uint MonitorDefaultToNearest = 2;

    private Popup? _popup;
    private FrameworkElement? _container;
    private bool _isAnimatingClose;

    static Flyout()
    {
        DefaultStyleKeyProperty.OverrideMetadata(
            typeof(Flyout),
            new FrameworkPropertyMetadata(typeof(Flyout)));
    }

    public static readonly DependencyProperty IsOpenProperty =
        DependencyProperty.Register(nameof(IsOpen), typeof(bool), typeof(Flyout),
            new FrameworkPropertyMetadata(false,
                FrameworkPropertyMetadataOptions.BindsTwoWayByDefault,
                OnIsOpenChanged));

    public bool IsOpen
    {
        get => (bool)GetValue(IsOpenProperty);
        set => SetValue(IsOpenProperty, value);
    }

    private static void OnIsOpenChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        var flyout = (Flyout)d;
        if (flyout._popup == null)
            flyout.ApplyTemplate();
        if (flyout._popup == null)
            return;

        if ((bool)e.NewValue)
        {
            flyout.PreparePopupLayout();
            flyout._popup.IsOpen = true;
        }
        else
        {
            flyout.BeginCloseAnimation();
        }
    }

    public static readonly DependencyProperty PlacementProperty =
        DependencyProperty.Register(nameof(Placement), typeof(PlacementMode), typeof(Flyout),
            new PropertyMetadata(PlacementMode.Bottom));

    public PlacementMode Placement
    {
        get => (PlacementMode)GetValue(PlacementProperty);
        set => SetValue(PlacementProperty, value);
    }

    public static readonly DependencyProperty PlacementTargetProperty =
        DependencyProperty.Register(nameof(PlacementTarget), typeof(UIElement), typeof(Flyout),
            new PropertyMetadata(null, OnPlacementTargetChanged));

    public UIElement? PlacementTarget
    {
        get => (UIElement?)GetValue(PlacementTargetProperty);
        set => SetValue(PlacementTargetProperty, value);
    }

    private static void OnPlacementTargetChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is Flyout { _popup: not null } flyout)
            flyout._popup.PlacementTarget = e.NewValue as UIElement;
    }

    public static readonly DependencyProperty HorizontalOffsetProperty =
        DependencyProperty.Register(nameof(HorizontalOffset), typeof(double), typeof(Flyout),
            new PropertyMetadata(0.0));

    public double HorizontalOffset
    {
        get => (double)GetValue(HorizontalOffsetProperty);
        set => SetValue(HorizontalOffsetProperty, value);
    }

    public static readonly DependencyProperty VerticalOffsetProperty =
        DependencyProperty.Register(nameof(VerticalOffset), typeof(double), typeof(Flyout),
            new PropertyMetadata(4.0));

    public double VerticalOffset
    {
        get => (double)GetValue(VerticalOffsetProperty);
        set => SetValue(VerticalOffsetProperty, value);
    }

    public static readonly DependencyProperty AutoPlacementProperty =
        DependencyProperty.Register(nameof(AutoPlacement), typeof(bool), typeof(Flyout),
            new PropertyMetadata(false));

    public bool AutoPlacement
    {
        get => (bool)GetValue(AutoPlacementProperty);
        set => SetValue(AutoPlacementProperty, value);
    }

    public static readonly DependencyProperty ConstrainToWorkAreaProperty =
        DependencyProperty.Register(nameof(ConstrainToWorkArea), typeof(bool), typeof(Flyout),
            new PropertyMetadata(false));

    public bool ConstrainToWorkArea
    {
        get => (bool)GetValue(ConstrainToWorkAreaProperty);
        set => SetValue(ConstrainToWorkAreaProperty, value);
    }

    public static readonly DependencyProperty ViewportMarginProperty =
        DependencyProperty.Register(nameof(ViewportMargin), typeof(double), typeof(Flyout),
            new PropertyMetadata(16.0));

    public double ViewportMargin
    {
        get => (double)GetValue(ViewportMarginProperty);
        set => SetValue(ViewportMarginProperty, value);
    }

    public override void OnApplyTemplate()
    {
        base.OnApplyTemplate();

        if (_popup != null)
        {
            _popup.Opened -= OnPopupOpened;
            _popup.Closed -= OnPopupClosed;
        }

        _popup = GetTemplateChild(PartPopup) as Popup;
        _container = GetTemplateChild(PartContainer) as FrameworkElement;

        if (_popup != null)
        {
            _popup.PlacementTarget = PlacementTarget;
            _popup.Placement = Placement;
            _popup.Opened += OnPopupOpened;
            _popup.Closed += OnPopupClosed;
        }

        ResetContainerConstraints();
    }

    private void OnPopupOpened(object? sender, EventArgs e)
    {
        if (_container == null)
            return;

        PreparePopupLayout();

        _container.Opacity = 0;
        _container.RenderTransform = new ScaleTransform(0.95, 0.95);
        _container.RenderTransformOrigin = _popup?.Placement == PlacementMode.Top
            ? new Point(0, 1)
            : new Point(0, 0);

        var ease = new CubicEase { EasingMode = EasingMode.EaseOut };
        var duration = new Duration(TimeSpan.FromMilliseconds(180));

        _container.BeginAnimation(OpacityProperty, new DoubleAnimation(0, 1, duration) { EasingFunction = ease });

        if (_container.RenderTransform is ScaleTransform scale)
        {
            scale.BeginAnimation(ScaleTransform.ScaleXProperty, new DoubleAnimation(0.95, 1.0, duration) { EasingFunction = ease });
            scale.BeginAnimation(ScaleTransform.ScaleYProperty, new DoubleAnimation(0.95, 1.0, duration) { EasingFunction = ease });
        }
    }

    private void BeginCloseAnimation()
    {
        if (_container == null || _popup == null)
        {
            _popup?.SetCurrentValue(Popup.IsOpenProperty, false);
            return;
        }

        if (_isAnimatingClose)
            return;

        _isAnimatingClose = true;

        var ease = new CubicEase { EasingMode = EasingMode.EaseIn };
        var duration = new Duration(TimeSpan.FromMilliseconds(120));
        var fadeOut = new DoubleAnimation(1, 0, duration) { EasingFunction = ease };
        fadeOut.Completed += (_, _) =>
        {
            _isAnimatingClose = false;
            if (_popup != null)
                _popup.IsOpen = false;
        };

        _container.BeginAnimation(OpacityProperty, fadeOut);

        if (_container.RenderTransform is not ScaleTransform scale)
            return;

        scale.BeginAnimation(ScaleTransform.ScaleXProperty, new DoubleAnimation(1.0, 0.95, duration) { EasingFunction = ease });
        scale.BeginAnimation(ScaleTransform.ScaleYProperty, new DoubleAnimation(1.0, 0.95, duration) { EasingFunction = ease });
    }

    private void OnPopupClosed(object? sender, EventArgs e)
    {
        _isAnimatingClose = false;
        ResetContainerConstraints();
        if (IsOpen)
            IsOpen = false;
    }

    private void PreparePopupLayout()
    {
        if (_popup == null)
            return;

        _popup.PlacementTarget = PlacementTarget;

        if (_container == null)
        {
            var fallbackPlacement = Placement;
            _popup.Placement = fallbackPlacement;
            ApplyPopupOffsets(fallbackPlacement);
            return;
        }

        if (!TryGetPlacementMetrics(out var targetBounds, out var workArea))
        {
            var fallbackPlacement = Placement;
            _popup.Placement = fallbackPlacement;
            ApplyPopupOffsets(fallbackPlacement);
            ApplyContainerHeightConstraint(null);
            return;
        }

        var margin = Math.Max(0, ViewportMargin);
        var desiredHeight = MeasureDesiredPopupHeight();
        var availableBelow = Math.Max(0, workArea.Bottom - targetBounds.Bottom - VerticalOffset - margin);
        var availableAbove = Math.Max(0, targetBounds.Top - workArea.Top - VerticalOffset - margin);

        var placement = ResolvePlacement(Placement, desiredHeight, availableBelow, availableAbove);
        _popup.Placement = placement;
        ApplyPopupOffsets(placement);

        var availableHeight = placement == PlacementMode.Top ? availableAbove : availableBelow;
        ApplyContainerHeightConstraint(ConstrainToWorkArea ? availableHeight : null);
    }

    private void ApplyPopupOffsets(PlacementMode placement)
    {
        if (_popup == null)
            return;

        _popup.HorizontalOffset = HorizontalOffset;
        _popup.VerticalOffset = placement switch
        {
            PlacementMode.Top => -Math.Abs(VerticalOffset),
            PlacementMode.Bottom => Math.Abs(VerticalOffset),
            _ => VerticalOffset
        };
    }

    private void ApplyContainerHeightConstraint(double? availableHeight)
    {
        if (_container == null)
            return;

        var maxHeight = availableHeight ?? double.PositiveInfinity;
        if (!double.IsPositiveInfinity(MaxHeight))
            maxHeight = Math.Min(maxHeight, MaxHeight);

        _container.MaxHeight = maxHeight;
    }

    private void ResetContainerConstraints()
    {
        if (_container == null)
            return;

        _container.MaxHeight = MaxHeight;
    }

    private double MeasureDesiredPopupHeight()
    {
        if (_container == null)
            return 0;

        var measureWidth = !double.IsPositiveInfinity(MaxWidth) && MaxWidth > 0
            ? MaxWidth
            : double.PositiveInfinity;

        _container.Measure(new Size(measureWidth, double.PositiveInfinity));
        return _container.DesiredSize.Height;
    }

    private PlacementMode ResolvePlacement(PlacementMode preferredPlacement, double desiredHeight, double availableBelow, double availableAbove)
    {
        if (!AutoPlacement || preferredPlacement is not (PlacementMode.Bottom or PlacementMode.Top))
            return preferredPlacement;

        if (preferredPlacement == PlacementMode.Bottom)
        {
            if (availableBelow >= desiredHeight || availableBelow >= availableAbove)
                return PlacementMode.Bottom;

            return PlacementMode.Top;
        }

        if (availableAbove >= desiredHeight || availableAbove >= availableBelow)
            return PlacementMode.Top;

        return PlacementMode.Bottom;
    }

    private bool TryGetPlacementMetrics(out Rect targetBounds, out Rect workArea)
    {
        targetBounds = Rect.Empty;
        workArea = Rect.Empty;

        if (PlacementTarget is not UIElement targetElement)
            return false;

        if (PresentationSource.FromVisual(targetElement) is not { CompositionTarget: not null } source)
            return false;

        var transformFromDevice = source.CompositionTarget.TransformFromDevice;
        var renderSize = targetElement.RenderSize;
        var topLeftPixels = targetElement.PointToScreen(new Point(0, 0));
        var bottomRightPixels = targetElement.PointToScreen(new Point(renderSize.Width, renderSize.Height));
        var topLeft = transformFromDevice.Transform(topLeftPixels);
        var bottomRight = transformFromDevice.Transform(bottomRightPixels);
        targetBounds = new Rect(topLeft, bottomRight);

        var monitor = MonitorFromPoint(new NativePoint((int)Math.Round(topLeftPixels.X), (int)Math.Round(topLeftPixels.Y)), MonitorDefaultToNearest);
        if (monitor == IntPtr.Zero)
            return false;

        var monitorInfo = new MonitorInfo { cbSize = Marshal.SizeOf<MonitorInfo>() };
        if (!GetMonitorInfo(monitor, ref monitorInfo))
            return false;

        var workTopLeft = transformFromDevice.Transform(new Point(monitorInfo.rcWork.Left, monitorInfo.rcWork.Top));
        var workBottomRight = transformFromDevice.Transform(new Point(monitorInfo.rcWork.Right, monitorInfo.rcWork.Bottom));
        workArea = new Rect(workTopLeft, workBottomRight);
        return true;
    }

    public void Show() => IsOpen = true;
    public void Hide() => IsOpen = false;
    public void Toggle() => IsOpen = !IsOpen;

    [DllImport("user32.dll")]
    private static extern IntPtr MonitorFromPoint(NativePoint pt, uint dwFlags);

    [DllImport("user32.dll", CharSet = CharSet.Auto)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool GetMonitorInfo(IntPtr hMonitor, ref MonitorInfo lpmi);

    [StructLayout(LayoutKind.Sequential)]
    private struct NativePoint
    {
        public int X;
        public int Y;

        public NativePoint(int x, int y)
        {
            X = x;
            Y = y;
        }
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct MonitorInfo
    {
        public int cbSize;
        public NativeRect rcMonitor;
        public NativeRect rcWork;
        public uint dwFlags;
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct NativeRect
    {
        public int Left;
        public int Top;
        public int Right;
        public int Bottom;
    }
}
