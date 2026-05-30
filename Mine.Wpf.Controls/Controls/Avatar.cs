using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace Mine.Wpf.Controls.Controls;

/// <summary>
/// Material 3 Avatar：圆形头像控件。
/// 内容优先级：ImageSource > Icon（Material Symbols 字符）> Initials（姓名首字母）> 默认人形图标。
/// </summary>
[TemplatePart(Name = PartImage,    Type = typeof(System.Windows.Controls.Image))]
[TemplatePart(Name = PartIconText, Type = typeof(TextBlock))]
[TemplatePart(Name = PartInitials, Type = typeof(TextBlock))]
public class Avatar : Control
{
    private const string PartImage    = "PART_Image";
    private const string PartIconText = "PART_IconText";
    private const string PartInitials = "PART_Initials";

    static Avatar()
    {
        DefaultStyleKeyProperty.OverrideMetadata(
            typeof(Avatar), new FrameworkPropertyMetadata(typeof(Avatar)));
    }

    // ── Size ─────────────────────────────────────────────────────────
    public static readonly DependencyProperty SizeProperty =
        DependencyProperty.Register(nameof(Size), typeof(double), typeof(Avatar),
            new PropertyMetadata(40.0, OnSizeChanged));

    /// <summary>头像直径（默认 40）。</summary>
    public double Size
    {
        get => (double)GetValue(SizeProperty);
        set => SetValue(SizeProperty, value);
    }

    private static void OnSizeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        => (d as Avatar)?.UpdateLayout();

    // ── ImageSource ───────────────────────────────────────────────────
    public static readonly DependencyProperty ImageSourceProperty =
        DependencyProperty.Register(nameof(ImageSource), typeof(ImageSource), typeof(Avatar),
            new PropertyMetadata(null, OnContentChanged));

    /// <summary>头像图片源。</summary>
    public ImageSource? ImageSource
    {
        get => (ImageSource?)GetValue(ImageSourceProperty);
        set => SetValue(ImageSourceProperty, value);
    }

    // ── Icon（Material Symbols 字符）─────────────────────────────────
    public static readonly DependencyProperty IconProperty =
        DependencyProperty.Register(nameof(Icon), typeof(string), typeof(Avatar),
            new PropertyMetadata(null, OnContentChanged));

    /// <summary>Material Symbols 图标字符（当 ImageSource 为空时显示）。</summary>
    public string? Icon
    {
        get => (string?)GetValue(IconProperty);
        set => SetValue(IconProperty, value);
    }

    // ── Initials（姓名首字母）────────────────────────────────────────
    public static readonly DependencyProperty InitialsProperty =
        DependencyProperty.Register(nameof(Initials), typeof(string), typeof(Avatar),
            new PropertyMetadata(null, OnContentChanged));

    /// <summary>文字头像（当 ImageSource 和 Icon 均为空时显示，建议 1-2 个字符）。</summary>
    public string? Initials
    {
        get => (string?)GetValue(InitialsProperty);
        set => SetValue(InitialsProperty, value);
    }

    // ── AvatarVariant（只读，由内容属性自动推断）───────────────────────
    private static readonly DependencyPropertyKey AvatarVariantPropertyKey =
        DependencyProperty.RegisterReadOnly(nameof(AvatarVariant), typeof(AvatarVariant), typeof(Avatar),
            new PropertyMetadata(AvatarVariant.Default));

    public static readonly DependencyProperty AvatarVariantProperty = AvatarVariantPropertyKey.DependencyProperty;

    public AvatarVariant AvatarVariant
    {
        get => (AvatarVariant)GetValue(AvatarVariantProperty);
        private set => SetValue(AvatarVariantPropertyKey, value);
    }

    private static void OnContentChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        => (d as Avatar)?.UpdateVariant();

    private void UpdateVariant()
    {
        if (ImageSource != null)
            AvatarVariant = AvatarVariant.Image;
        else if (!string.IsNullOrEmpty(Icon))
            AvatarVariant = AvatarVariant.Icon;
        else if (!string.IsNullOrEmpty(Initials))
            AvatarVariant = AvatarVariant.Initials;
        else
            AvatarVariant = AvatarVariant.Default;
    }

    public override void OnApplyTemplate()
    {
        base.OnApplyTemplate();
        UpdateVariant();
    }
}

/// <summary>Avatar 的内容变体。</summary>
public enum AvatarVariant
{
    /// <summary>显示图片。</summary>
    Image,
    /// <summary>显示 Material Symbols 图标。</summary>
    Icon,
    /// <summary>显示文字首字母。</summary>
    Initials,
    /// <summary>显示默认人形图标（无内容时）。</summary>
    Default,
}

