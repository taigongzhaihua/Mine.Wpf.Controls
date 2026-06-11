using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace Mine.Wpf.Controls.Controls;

/// <summary>
/// 将多个 <see cref="Avatar"/> 水平重叠排列的面板。
/// 后一个头像覆盖在前一个头像之上，间距由 <see cref="Spacing"/>（负值 = 重叠）控制。
/// </summary>
public class AvatarGroupPanel : Panel
{
    // ── Spacing ────────────────────────────────────────────────────
    public static readonly DependencyProperty SpacingProperty =
        DependencyProperty.Register(nameof(Spacing), typeof(double), typeof(AvatarGroupPanel),
            new FrameworkPropertyMetadata(-8.0,
                FrameworkPropertyMetadataOptions.AffectsMeasure | FrameworkPropertyMetadataOptions.AffectsArrange));

    /// <summary>相邻头像之间的间距（负值表示重叠像素数，默认 -8）。</summary>
    public double Spacing
    {
        get => (double)GetValue(SpacingProperty);
        set => SetValue(SpacingProperty, value);
    }

    protected override Size MeasureOverride(Size availableSize)
    {
        double totalWidth = 0;
        double maxHeight  = 0;

        foreach (UIElement child in InternalChildren)
        {
            if (child.Visibility == Visibility.Collapsed) continue;
            child.Measure(availableSize);
            if (totalWidth > 0) totalWidth += Spacing;
            totalWidth += child.DesiredSize.Width;
            maxHeight = Math.Max(maxHeight, child.DesiredSize.Height);
        }

        return new Size(Math.Max(0, totalWidth), maxHeight);
    }

    protected override Size ArrangeOverride(Size finalSize)
    {
        double x = 0;
        var zIndex = 0;

        foreach (UIElement child in InternalChildren)
        {
            if (child.Visibility == Visibility.Collapsed)
            {
                child.Arrange(new Rect(0, 0, 0, 0));
                continue;
            }

            var w = child.DesiredSize.Width;
            var h = child.DesiredSize.Height;
            var y = (finalSize.Height - h) / 2;

            child.Arrange(new Rect(x, y, w, h));
            Panel.SetZIndex(child, zIndex++);
            x += w + Spacing;
        }

        return finalSize;
    }
}

/// <summary>
/// Material Design 3 AvatarGroup：将多个 <see cref="Avatar"/> 水平重叠显示，
/// 超出 <see cref="MaxCount"/> 时在末尾显示 "+N" 溢出气泡。
/// </summary>
[TemplatePart(Name = PartOverflow, Type = typeof(UIElement))]
public class AvatarGroup : ItemsControl
{
    private const string PartOverflow = "PART_Overflow";

    private UIElement? _overflow;

    static AvatarGroup()
    {
        DefaultStyleKeyProperty.OverrideMetadata(
            typeof(AvatarGroup), new FrameworkPropertyMetadata(typeof(AvatarGroup)));
    }

    // ── AvatarSize ─────────────────────────────────────────────────
    public static readonly DependencyProperty AvatarSizeProperty =
        DependencyProperty.Register(nameof(AvatarSize), typeof(double), typeof(AvatarGroup),
            new PropertyMetadata(40.0, OnAvatarSettingsChanged));

    /// <summary>每个头像的直径（默认 40）。</summary>
    public double AvatarSize
    {
        get => (double)GetValue(AvatarSizeProperty);
        set => SetValue(AvatarSizeProperty, value);
    }

    // ── Spacing ────────────────────────────────────────────────────
    public static readonly DependencyProperty SpacingProperty =
        DependencyProperty.Register(nameof(Spacing), typeof(double), typeof(AvatarGroup),
            new PropertyMetadata(-8.0, OnLayoutChanged));

    /// <summary>头像间距（负值 = 重叠，默认 -8）。</summary>
    public double Spacing
    {
        get => (double)GetValue(SpacingProperty);
        set => SetValue(SpacingProperty, value);
    }

    // ── MaxCount ───────────────────────────────────────────────────
    public static readonly DependencyProperty MaxCountProperty =
        DependencyProperty.Register(nameof(MaxCount), typeof(int), typeof(AvatarGroup),
            new PropertyMetadata(5, OnLayoutChanged));

    /// <summary>最多显示的头像数量，超出时显示 "+N" 气泡（默认 5，-1 = 不限）。</summary>
    public int MaxCount
    {
        get => (int)GetValue(MaxCountProperty);
        set => SetValue(MaxCountProperty, value);
    }

    // ── OverflowCount（只读）───────────────────────────────────────
    private static readonly DependencyPropertyKey OverflowCountPropertyKey =
        DependencyProperty.RegisterReadOnly(nameof(OverflowCount), typeof(int), typeof(AvatarGroup),
            new PropertyMetadata(0));

    public static readonly DependencyProperty OverflowCountProperty = OverflowCountPropertyKey.DependencyProperty;

    /// <summary>被隐藏的头像数量。</summary>
    public int OverflowCount
    {
        get => (int)GetValue(OverflowCountProperty);
        private set => SetValue(OverflowCountPropertyKey, value);
    }

    // ── OverflowText（只读）───────────────────────────────────────
    private static readonly DependencyPropertyKey OverflowTextPropertyKey =
        DependencyProperty.RegisterReadOnly(nameof(OverflowText), typeof(string), typeof(AvatarGroup),
            new PropertyMetadata(string.Empty));

    public static readonly DependencyProperty OverflowTextProperty = OverflowTextPropertyKey.DependencyProperty;

    /// <summary>溢出气泡显示的文字，如 "+3"。</summary>
    public string OverflowText
    {
        get => (string)GetValue(OverflowTextProperty);
        private set => SetValue(OverflowTextPropertyKey, value);
    }

    // ── BorderBrush / BorderThickness（头像边框，用于分隔重叠）───
    public static readonly DependencyProperty AvatarBorderBrushProperty =
        DependencyProperty.Register(nameof(AvatarBorderBrush), typeof(Brush), typeof(AvatarGroup),
            new PropertyMetadata(null, OnAvatarSettingsChanged));

    /// <summary>每个头像的外描边颜色（通常与背景色相同以体现分隔感）。</summary>
    public Brush? AvatarBorderBrush
    {
        get => (Brush?)GetValue(AvatarBorderBrushProperty);
        set => SetValue(AvatarBorderBrushProperty, value);
    }

    public static readonly DependencyProperty AvatarBorderThicknessProperty =
        DependencyProperty.Register(nameof(AvatarBorderThickness), typeof(double), typeof(AvatarGroup),
            new PropertyMetadata(2.0, OnAvatarSettingsChanged));

    /// <summary>每个头像外描边粗细（默认 2）。</summary>
    public double AvatarBorderThickness
    {
        get => (double)GetValue(AvatarBorderThicknessProperty);
        set => SetValue(AvatarBorderThicknessProperty, value);
    }

    // ── 内部回调 ───────────────────────────────────────────────────
    private static void OnLayoutChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        => (d as AvatarGroup)?.RefreshItems();

    private static void OnAvatarSettingsChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        => (d as AvatarGroup)?.ApplySettingsToAllAvatars();

    public override void OnApplyTemplate()
    {
        base.OnApplyTemplate();
        _overflow = GetTemplateChild(PartOverflow) as UIElement;

        // 让溢出气泡与末尾头像产生相同的重叠效果
        if (_overflow is FrameworkElement fe)
            fe.Margin = new Thickness(Spacing, 0, 0, 0);

        // 此时样式和动态资源已完全解析，重新应用头像设置（修复 BorderBrush 首次为 null 的问题）
        ApplySettingsToAllAvatars();
        RefreshItems();
    }

    protected override void OnItemsChanged(System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
    {
        base.OnItemsChanged(e);
        RefreshItems();
    }

    private void RefreshItems()
    {
        var total  = Items.Count;
        var max    = MaxCount < 0 ? total : MaxCount;
        var hidden = Math.Max(0, total - max);

        OverflowCount = hidden;
        OverflowText  = hidden > 0 ? $"+{hidden}" : string.Empty;

        // IsItemItsOwnContainerOverride 返回 true，Items[i] 本身就是 Avatar 容器
        for (var i = 0; i < total; i++)
        {
            if (Items[i] is Avatar avatar)
                avatar.Visibility = i < max ? Visibility.Visible : Visibility.Collapsed;
        }

        if (_overflow != null)
            _overflow.Visibility = hidden > 0 ? Visibility.Visible : Visibility.Collapsed;
    }

    protected override void PrepareContainerForItemOverride(DependencyObject element, object item)
    {
        base.PrepareContainerForItemOverride(element, item);
        if (element is Avatar avatar)
            BindAvatarSettings(avatar);
    }

    /// <summary>将尺寸和边框设置同步到所有已生成的 Avatar 容器（在 OnApplyTemplate 及属性变更时调用）。</summary>
    private void ApplySettingsToAllAvatars()
    {
        foreach (var item in Items)
        {
            if (item is Avatar avatar)
                BindAvatarSettings(avatar);
        }
    }

    /// <summary>将本控件的尺寸/边框属性绑定到单个 Avatar，使其跟随动态资源变化。</summary>
    private void BindAvatarSettings(Avatar avatar)
    {
        avatar.Size = AvatarSize;
        avatar.BorderThickness = new Thickness(AvatarBorderThickness);
        // 用 Binding 而非直接赋值，确保 AvatarBorderBrush（跟随主题的 DynamicResource）实时更新
        var brush = new System.Windows.Data.Binding(nameof(AvatarBorderBrush))
        {
            Source = this,
            Mode   = System.Windows.Data.BindingMode.OneWay
        };
        avatar.SetBinding(Avatar.BorderBrushProperty, brush);
    }

    protected override bool IsItemItsOwnContainerOverride(object item) => item is Avatar;
}
