using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace Mine.Wpf.Controls.Controls;

/// <summary>
/// Material 3 Badge：叠加在宿主控件右上角的小徽标，支持「点状」和「数字」两种变体。
/// </summary>
/// <remarks>
/// 用法一：直接将 BadgeText 设为非 null 值，Badge 控件本身作为内容。
/// 用法二（推荐）：将要显示徽标的控件放在 Badge.Content 中，
///   Badge 自动将数字/点叠加在 Content 右上角。
/// <code>
/// &lt;mine:Badge BadgeText="3"&gt;
///     &lt;mine:MaterialIcon Kind="&#xE88A;" Size="24"/&gt;
/// &lt;/mine:Badge&gt;
/// </code>
/// </remarks>
[TemplatePart(Name = PartBadge, Type = typeof(Border))]
public class Badge : ContentControl
{
    private const string PartBadge = "PART_Badge";

    static Badge()
    {
        DefaultStyleKeyProperty.OverrideMetadata(
            typeof(Badge),
            new FrameworkPropertyMetadata(typeof(Badge)));
    }

    // ── BadgeText ──────────────────────────────────────────────────────────
    // null  → 隐藏徽标
    // ""    → 小圆点（Small Badge，6 dp）
    // "5"   → 数字徽标（Large Badge，自动适应宽度，高度 16 dp）
    public static readonly DependencyProperty BadgeTextProperty =
        DependencyProperty.Register(nameof(BadgeText), typeof(string), typeof(Badge),
            new PropertyMetadata(null, OnBadgeTextChanged));

    /// <summary>
    /// 徽标显示的文字。<br/>
    /// • <c>null</c>  → 隐藏徽标<br/>
    /// • <c>""</c>    → 仅显示小圆点（无文字）<br/>
    /// • 任意文字     → 带文字的大徽标
    /// </summary>
    public string? BadgeText
    {
        get => (string?)GetValue(BadgeTextProperty);
        set => SetValue(BadgeTextProperty, value);
    }

    private static void OnBadgeTextChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        var badge = (Badge)d;
        badge.UpdateBadgeVariant();
    }

    // ── BadgeVariant（只读，由 BadgeText 自动推断）──────────────────────
    private static readonly DependencyPropertyKey BadgeVariantPropertyKey =
        DependencyProperty.RegisterReadOnly(nameof(BadgeVariant), typeof(BadgeVariant), typeof(Badge),
            new PropertyMetadata(BadgeVariant.Hidden));

    public static readonly DependencyProperty BadgeVariantProperty = BadgeVariantPropertyKey.DependencyProperty;

    /// <summary>徽标的外观变体（由 BadgeText 自动推断，只读）。</summary>
    public BadgeVariant BadgeVariant
    {
        get => (BadgeVariant)GetValue(BadgeVariantProperty);
        private set => SetValue(BadgeVariantPropertyKey, value);
    }

    // ── BadgePlacement（水平方向）────────────────────────────────────────
    public static readonly DependencyProperty BadgePlacementProperty =
        DependencyProperty.Register(nameof(BadgePlacement), typeof(BadgePlacement), typeof(Badge),
            new PropertyMetadata(BadgePlacement.TopRight, OnBadgePlacementChanged));

    private static void OnBadgePlacementChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        => (d as Badge)?.UpdateBadgeTransform();

    /// <summary>徽标相对于宿主内容的位置，默认右上角。</summary>
    public BadgePlacement BadgePlacement
    {
        get => (BadgePlacement)GetValue(BadgePlacementProperty);
        set => SetValue(BadgePlacementProperty, value);
    }

    // ── BadgeHorizontalOffset ─────────────────────────────────────────────
    public static readonly DependencyProperty BadgeHorizontalOffsetProperty =
        DependencyProperty.Register(nameof(BadgeHorizontalOffset), typeof(double), typeof(Badge),
            new PropertyMetadata(0.0));

    /// <summary>徽标在水平方向的额外偏移量（正值=向外，负值=向内）。</summary>
    public double BadgeHorizontalOffset
    {
        get => (double)GetValue(BadgeHorizontalOffsetProperty);
        set => SetValue(BadgeHorizontalOffsetProperty, value);
    }

    // ── BadgeVerticalOffset ───────────────────────────────────────────────
    public static readonly DependencyProperty BadgeVerticalOffsetProperty =
        DependencyProperty.Register(nameof(BadgeVerticalOffset), typeof(double), typeof(Badge),
            new PropertyMetadata(0.0));

    /// <summary>徽标在垂直方向的额外偏移量（正值=向外，负值=向内）。</summary>
    public double BadgeVerticalOffset
    {
        get => (double)GetValue(BadgeVerticalOffsetProperty);
        set => SetValue(BadgeVerticalOffsetProperty, value);
    }

    // ── Template parts ────────────────────────────────────────────────────
    private Border? _badgeBorder;

    public override void OnApplyTemplate()
    {
        base.OnApplyTemplate();

        if (_badgeBorder != null)
            _badgeBorder.SizeChanged -= OnBadgeSizeChanged;

        _badgeBorder = GetTemplateChild(PartBadge) as Border;

        if (_badgeBorder != null)
            _badgeBorder.SizeChanged += OnBadgeSizeChanged;

        UpdateBadgeVariant();
    }

    private void OnBadgeSizeChanged(object sender, SizeChangedEventArgs e) => UpdateBadgeTransform();

    /// <summary>
    /// 根据徽标实际尺寸计算 RenderTransform，使徽标中心始终锚定在宿主控件的对应角，
    /// 文字变长时向外侧延伸而不遮挡内容。
    /// </summary>
    private void UpdateBadgeTransform()
    {
        if (_badgeBorder == null) return;

        var w = _badgeBorder.ActualWidth;
        var h = _badgeBorder.ActualHeight;
        if (w <= 0 || h <= 0) return;

        // 对于 Right 对齐：badge 右边缘 = 容器右边缘，中心在 containerRight - w/2
        // 向右平移 w/2，使中心落在容器右边缘（往外延伸）
        // 对于 Left 对齐：同理向左平移 w/2
        double tx, ty;
        switch (BadgePlacement)
        {
            case BadgePlacement.TopRight:
                tx =  w / 2;
                ty = -h / 2;
                break;
            case BadgePlacement.TopLeft:
                tx = -w / 2;
                ty = -h / 2;
                break;
            case BadgePlacement.BottomRight:
                tx =  w / 2;
                ty =  h / 2;
                break;
            case BadgePlacement.BottomLeft:
            default:
                tx = -w / 2;
                ty =  h / 2;
                break;
        }

        _badgeBorder.RenderTransform = new TranslateTransform(tx, ty);
    }

    // ── 内部状态更新 ──────────────────────────────────────────────────────
    private void UpdateBadgeVariant()
    {
        BadgeVariant = BadgeText switch
        {
            null         => BadgeVariant.Hidden,
            ""           => BadgeVariant.Small,
            _            => BadgeVariant.Large,
        };
    }
}

/// <summary>Badge 的外观变体。</summary>
public enum BadgeVariant
{
    /// <summary>隐藏（BadgeText = null）。</summary>
    Hidden,
    /// <summary>小圆点，无文字（BadgeText = ""）。</summary>
    Small,
    /// <summary>带文字的大徽标（BadgeText 有内容）。</summary>
    Large,
}

/// <summary>Badge 相对于宿主内容的位置。</summary>
public enum BadgePlacement
{
    /// <summary>右上角（默认）。</summary>
    TopRight,
    /// <summary>左上角。</summary>
    TopLeft,
    /// <summary>右下角。</summary>
    BottomRight,
    /// <summary>左下角。</summary>
    BottomLeft,
}

