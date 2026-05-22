using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
namespace Mine.Wpf.Controls.Primitives;
/// <summary>
/// 内容装饰器，在鼠标点击时显示 Material 3 水波纹动画。
/// 可直接将任意控件包裹于此获得水波纹效果：
/// <code>&lt;mine:RippleDecorator&gt;&lt;Button .../&gt;&lt;/mine:RippleDecorator&gt;</code>
/// 或在 MineButton / Card 控件模板中内部使用。
/// </summary>
[TemplatePart(Name = PartRippleCanvas, Type = typeof(Canvas))]
public class RippleDecorator : ContentControl
{
    private const string PartRippleCanvas = "PART_RippleCanvas";
    private Canvas? _canvas;
    // ── 依赖属性 ──────────────────────────────────────────────────
    public static readonly DependencyProperty RippleColorProperty =
        DependencyProperty.Register(nameof(RippleColor), typeof(Color), typeof(RippleDecorator),
            new PropertyMetadata(Color.FromArgb(30, 255, 255, 255)));
    /// <summary>若设置，则从画笔中提取颜色覆盖 RippleColor。</summary>
    public static readonly DependencyProperty RippleBrushProperty =
        DependencyProperty.Register(nameof(RippleBrush), typeof(Brush), typeof(RippleDecorator),
            new PropertyMetadata(null));
    public static readonly DependencyProperty IsCenteredProperty =
        DependencyProperty.Register(nameof(IsCentered), typeof(bool), typeof(RippleDecorator),
            new PropertyMetadata(false));
    public static readonly DependencyProperty CornerRadiusProperty =
        DependencyProperty.Register(nameof(CornerRadius), typeof(CornerRadius), typeof(RippleDecorator),
            new PropertyMetadata(new CornerRadius(0), (d, _) => ((RippleDecorator)d).UpdateClip()));
    public Color RippleColor
    {
        get => (Color)GetValue(RippleColorProperty);
        set => SetValue(RippleColorProperty, value);
    }
    public Brush? RippleBrush
    {
        get => (Brush?)GetValue(RippleBrushProperty);
        set => SetValue(RippleBrushProperty, value);
    }
    public bool IsCentered
    {
        get => (bool)GetValue(IsCenteredProperty);
        set => SetValue(IsCenteredProperty, value);
    }
    public CornerRadius CornerRadius
    {
        get => (CornerRadius)GetValue(CornerRadiusProperty);
        set => SetValue(CornerRadiusProperty, value);
    }
    // ── 静态构造函数 ──────────────────────────────────────────────
    static RippleDecorator()
    {
        DefaultStyleKeyProperty.OverrideMetadata(
            typeof(RippleDecorator),
            new FrameworkPropertyMetadata(typeof(RippleDecorator)));
        // 不在控件级别默认 ClipToBounds=true，避免 WPF 按矩形布局框裁剪整个控件渲染输出。
        // 圆角裁剪由模板内的 Border（ClipToBounds="True" + CornerRadius）负责。
    }
    // ── 模板应用 ──────────────────────────────────────────────────
    public override void OnApplyTemplate()
    {
        base.OnApplyTemplate();
        _canvas = GetTemplateChild(PartRippleCanvas) as Canvas;
        UpdateClip();
    }

    // ── 尺寸变化时更新圆角裁剪 ───────────────────────────────────
    protected override void OnRenderSizeChanged(SizeChangedInfo sizeInfo)
    {
        base.OnRenderSizeChanged(sizeInfo);
        UpdateClip();
    }

    /// <summary>
    /// 根据当前尺寸和 CornerRadius 更新控件的 Clip 几何体，实现圆角裁剪。
    /// WPF Border.ClipToBounds 只做矩形裁剪，故须手动管理 Clip。
    /// </summary>
    private void UpdateClip()
    {
        var w = ActualWidth;
        var h = ActualHeight;
        if (w <= 0 || h <= 0) { Clip = null; return; }

        var cr = CornerRadius;
        // 对非均匀圆角取最小值用于 RectangleGeometry（四角均匀时精确）
        double r = Math.Min(Math.Min(cr.TopLeft, cr.TopRight), Math.Min(cr.BottomLeft, cr.BottomRight));
        Clip = new RectangleGeometry(new Rect(0, 0, w, h), r, r);
    }
    // ── 水波纹逻辑 ────────────────────────────────────────────────
    protected override void OnMouseDown(MouseButtonEventArgs e)
    {
        base.OnMouseDown(e);
        if (e.LeftButton == MouseButtonState.Pressed)
            StartRipple(e.GetPosition(this));
    }
    public void StartRipple(Point origin)
    {
        if (_canvas is null) return;
        double w = ActualWidth;
        double h = ActualHeight;
        if (w <= 0 || h <= 0) return;
        // 优先使用 RippleBrush 颜色，否则回退到 RippleColor
        Color rippleColor = RippleColor;
        if (RippleBrush is SolidColorBrush scb)
        {
            var c = scb.Color;
            rippleColor = Color.FromArgb(40, c.R, c.G, c.B);
        }
        var center = IsCentered ? new Point(w / 2, h / 2) : origin;
        // 半径 = 到最远角的距离
        double r = Math.Sqrt(
            Math.Max(center.X, w - center.X) * Math.Max(center.X, w - center.X) +
            Math.Max(center.Y, h - center.Y) * Math.Max(center.Y, h - center.Y));
        var ellipse = new Ellipse
        {
            Width                 = 0,
            Height                = 0,
            Fill                  = new SolidColorBrush(rippleColor),
            IsHitTestVisible      = false,
            RenderTransformOrigin = new Point(0.5, 0.5),
        };
        // 初始位于点击中心，通过 Width/Height + 偏移动画向外扩散
        Canvas.SetLeft(ellipse, center.X);
        Canvas.SetTop(ellipse, center.Y);
        _canvas.Children.Add(ellipse);
        // 扩散动画
        double target = r * 2;
        var dur  = new Duration(TimeSpan.FromMilliseconds(400));
        var ease = new CubicEase { EasingMode = EasingMode.EaseOut };
        var wAnim = new DoubleAnimation(0, target, dur) { EasingFunction = ease };
        var hAnim = new DoubleAnimation(0, target, dur) { EasingFunction = ease };
        var lAnim = new DoubleAnimation(center.X, center.X - r, dur) { EasingFunction = ease };
        var tAnim = new DoubleAnimation(center.Y, center.Y - r, dur) { EasingFunction = ease };
        // 透明度淡出
        var opacityAnim = new DoubleAnimationUsingKeyFrames();
        opacityAnim.KeyFrames.Add(new LinearDoubleKeyFrame(1, KeyTime.FromTimeSpan(TimeSpan.Zero)));
        opacityAnim.KeyFrames.Add(new LinearDoubleKeyFrame(1, KeyTime.FromTimeSpan(TimeSpan.FromMilliseconds(200))));
        opacityAnim.KeyFrames.Add(new LinearDoubleKeyFrame(0, KeyTime.FromTimeSpan(TimeSpan.FromMilliseconds(400))));
        EventHandler? cleanup = null;
        cleanup = (_, _) =>
        {
            ellipse.BeginAnimation(WidthProperty, null);
            _canvas.Children.Remove(ellipse);
            opacityAnim.Completed -= cleanup;
        };
        opacityAnim.Completed += cleanup;
        var board = new Storyboard();
        Storyboard.SetTarget(wAnim,       ellipse); Storyboard.SetTargetProperty(wAnim,       new PropertyPath(WidthProperty));
        Storyboard.SetTarget(hAnim,       ellipse); Storyboard.SetTargetProperty(hAnim,       new PropertyPath(HeightProperty));
        Storyboard.SetTarget(lAnim,       ellipse); Storyboard.SetTargetProperty(lAnim,       new PropertyPath(Canvas.LeftProperty));
        Storyboard.SetTarget(tAnim,       ellipse); Storyboard.SetTargetProperty(tAnim,       new PropertyPath(Canvas.TopProperty));
        Storyboard.SetTarget(opacityAnim, ellipse); Storyboard.SetTargetProperty(opacityAnim, new PropertyPath(OpacityProperty));
        board.Children.Add(wAnim);
        board.Children.Add(hAnim);
        board.Children.Add(lAnim);
        board.Children.Add(tAnim);
        board.Children.Add(opacityAnim);
        board.Begin();
    }
}
