using System.Windows;
using System.Windows.Media;

namespace Mine.Wpf.Controls.Primitives;

/// <summary>
/// 顶部带缺口的描边边框，用于 Outlined TextField 的浮动标签效果。
/// 缺口区域留给浮动标签，无需任何背景遮挡技巧。
/// </summary>
public class NotchedOutlineBorder : System.Windows.Controls.Decorator
{
    // ── 依赖属性 ──────────────────────────────────────────────────────

    public static readonly DependencyProperty BorderBrushProperty =
        DependencyProperty.Register(nameof(BorderBrush), typeof(Brush), typeof(NotchedOutlineBorder),
            new FrameworkPropertyMetadata(Brushes.Gray,
                FrameworkPropertyMetadataOptions.AffectsRender));

    public static readonly DependencyProperty BorderThicknessProperty =
        DependencyProperty.Register(nameof(BorderThickness), typeof(double), typeof(NotchedOutlineBorder),
            new FrameworkPropertyMetadata(1.0,
                FrameworkPropertyMetadataOptions.AffectsRender | FrameworkPropertyMetadataOptions.AffectsMeasure));

    public static readonly DependencyProperty CornerRadiusProperty =
        DependencyProperty.Register(nameof(CornerRadius), typeof(double), typeof(NotchedOutlineBorder),
            new FrameworkPropertyMetadata(4.0,
                FrameworkPropertyMetadataOptions.AffectsRender));

    /// <summary>缺口起始 X（相对于控件左边）</summary>
    public static readonly DependencyProperty NotchStartProperty =
        DependencyProperty.Register(nameof(NotchStart), typeof(double), typeof(NotchedOutlineBorder),
            new FrameworkPropertyMetadata(12.0,
                FrameworkPropertyMetadataOptions.AffectsRender));

    /// <summary>缺口宽度；为 0 时不绘制缺口</summary>
    public static readonly DependencyProperty NotchWidthProperty =
        DependencyProperty.Register(nameof(NotchWidth), typeof(double), typeof(NotchedOutlineBorder),
            new FrameworkPropertyMetadata(0.0,
                FrameworkPropertyMetadataOptions.AffectsRender));

    // ── CLR 属性 ─────────────────────────────────────────────────────

    public Brush BorderBrush
    {
        get => (Brush)GetValue(BorderBrushProperty);
        set => SetValue(BorderBrushProperty, value);
    }

    public double BorderThickness
    {
        get => (double)GetValue(BorderThicknessProperty);
        set => SetValue(BorderThicknessProperty, value);
    }

    public double CornerRadius
    {
        get => (double)GetValue(CornerRadiusProperty);
        set => SetValue(CornerRadiusProperty, value);
    }

    public double NotchStart
    {
        get => (double)GetValue(NotchStartProperty);
        set => SetValue(NotchStartProperty, value);
    }

    public double NotchWidth
    {
        get => (double)GetValue(NotchWidthProperty);
        set => SetValue(NotchWidthProperty, value);
    }

    // ── 布局 ─────────────────────────────────────────────────────────

    protected override Size MeasureOverride(Size availableSize)
    {
        if (Child != null)
        {
            var thick = BorderThickness;
            var inner = new Size(
                Math.Max(0, availableSize.Width - thick * 2),
                Math.Max(0, availableSize.Height - thick * 2));
            Child.Measure(inner);
            return new Size(Child.DesiredSize.Width + thick * 2,
                            Child.DesiredSize.Height + thick * 2);
        }
        return base.MeasureOverride(availableSize);
    }

    protected override Size ArrangeOverride(Size finalSize)
    {
        if (Child != null)
        {
            var thick = BorderThickness;
            Child.Arrange(new Rect(thick, thick,
                Math.Max(0, finalSize.Width - thick * 2),
                Math.Max(0, finalSize.Height - thick * 2)));
        }
        return finalSize;
    }

    // ── 绘制 ─────────────────────────────────────────────────────────

    protected override void OnRender(DrawingContext dc)
    {
        var w = ActualWidth;
        var h = ActualHeight;
        if (w <= 0 || h <= 0) return;

        var brush = BorderBrush;
        if (brush == null) return;

        var thick = BorderThickness;
        var r = Math.Min(CornerRadius, Math.Min(w / 2, h / 2));
        var half = thick / 2;          // 画笔中心偏移
        var pen = new Pen(brush, thick);

        var notchStart = NotchStart;
        var notchWidth = NotchWidth;
        var hasNotch = notchWidth > 0.5;

        // 用 StreamGeometry 绘制整个轮廓（顺时针，从缺口左端开始）
        var geo = new StreamGeometry();
        using (var ctx = geo.Open())
        {
            // 起点：顶边缺口左端（或左上角弧线结束后）
            var rightX = w - half;
            var bottomY = h - half;

            if (hasNotch)
            {
                var nx1 = notchStart + notchWidth; // 缺口右端 X

                // 从缺口左端起，顺时针绕一圈
                ctx.BeginFigure(new Point(notchStart, half), false, false);
                // 顶边左段（缺口左 → 左上角弧起点）
                ctx.LineTo(new Point(half + r, half), true, false);
                // 左上圆角
                if (r > 0)
                    ctx.ArcTo(new Point(half, half + r), new Size(r, r), 0, false,
                               SweepDirection.Counterclockwise, true, false);
                // 左边
                ctx.LineTo(new Point(half, bottomY - r), true, false);
                // 左下圆角
                if (r > 0)
                    ctx.ArcTo(new Point(half + r, bottomY), new Size(r, r), 0, false,
                               SweepDirection.Counterclockwise, true, false);
                // 底边
                ctx.LineTo(new Point(rightX - r, bottomY), true, false);
                // 右下圆角
                if (r > 0)
                    ctx.ArcTo(new Point(rightX, bottomY - r), new Size(r, r), 0, false,
                               SweepDirection.Counterclockwise, true, false);
                // 右边
                ctx.LineTo(new Point(rightX, half + r), true, false);
                // 右上圆角
                if (r > 0)
                    ctx.ArcTo(new Point(rightX - r, half), new Size(r, r), 0, false,
                               SweepDirection.Counterclockwise, true, false);
                // 顶边右段（右上角弧结束 → 缺口右端）
                ctx.LineTo(new Point(nx1, half), true, false);
                // 缺口处不连线（结束即可）
            }
            else
            {
                // 无缺口：完整圆角矩形
                ctx.BeginFigure(new Point(half + r, half), false, false);
                if (r > 0)
                    ctx.ArcTo(new Point(half, half + r), new Size(r, r), 0, false,
                               SweepDirection.Counterclockwise, true, false);
                ctx.LineTo(new Point(half, bottomY - r), true, false);
                if (r > 0)
                    ctx.ArcTo(new Point(half + r, bottomY), new Size(r, r), 0, false,
                               SweepDirection.Counterclockwise, true, false);
                ctx.LineTo(new Point(rightX - r, bottomY), true, false);
                if (r > 0)
                    ctx.ArcTo(new Point(rightX, bottomY - r), new Size(r, r), 0, false,
                               SweepDirection.Counterclockwise, true, false);
                ctx.LineTo(new Point(rightX, half + r), true, false);
                if (r > 0)
                    ctx.ArcTo(new Point(rightX - r, half), new Size(r, r), 0, false,
                               SweepDirection.Counterclockwise, true, false);
                ctx.LineTo(new Point(half + r, half), true, false);
            }
        }
        dc.DrawGeometry(null, pen, geo);
    }
}

