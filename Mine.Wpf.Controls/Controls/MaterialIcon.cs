using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace Mine.Wpf.Controls.Controls;

/// <summary>
/// 显示 Google Material Symbols Rounded 图标的控件。
/// 图标名称常量见 <see cref="Mine.Wpf.Controls.Icons.MaterialIcons"/>。
/// </summary>
[TemplatePart(Name = PartText, Type = typeof(TextBlock))]
public class MaterialIcon : Control
{
    private const string PartText = "PART_Text";
    private TextBlock? _text;

    internal static readonly FontFamily OutlineFont = new(
        new Uri("pack://application:,,,/Mine.Wpf.Controls;component/Fonts/"),
        "./#Material Symbols Rounded");

    internal static readonly FontFamily FilledFont = new(
        new Uri("pack://application:,,,/Mine.Wpf.Controls;component/Fonts/"),
        "./#Material Symbols Rounded Filled");

    static MaterialIcon()
    {
        DefaultStyleKeyProperty.OverrideMetadata(typeof(MaterialIcon), new FrameworkPropertyMetadata(typeof(MaterialIcon)));
        IsTabStopProperty.OverrideMetadata(typeof(MaterialIcon), new FrameworkPropertyMetadata(false));
        FocusableProperty.OverrideMetadata(typeof(MaterialIcon),  new FrameworkPropertyMetadata(false));
    }

    // ── Kind ─────────────────────────────────────────────────────────────
    public static readonly DependencyProperty KindProperty =
        DependencyProperty.Register(nameof(Kind), typeof(string), typeof(MaterialIcon),
            new FrameworkPropertyMetadata(string.Empty, OnVisualChanged));

    public string Kind
    {
        get => (string)GetValue(KindProperty);
        set => SetValue(KindProperty, value);
    }

    // ── Size ─────────────────────────────────────────────────────────────
    public static readonly DependencyProperty SizeProperty =
        DependencyProperty.Register(nameof(Size), typeof(double), typeof(MaterialIcon),
            new FrameworkPropertyMetadata(24.0, OnSizeChanged));

    public double Size
    {
        get => (double)GetValue(SizeProperty);
        set => SetValue(SizeProperty, value);
    }

    private static void OnSizeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        var icon = (MaterialIcon)d;
        var size = (double)e.NewValue;
        icon.Width  = size;
        icon.Height = size;
        icon.UpdateText();
    }

    // ── Weight ───────────────────────────────────────────────────────────
    public static readonly DependencyProperty WeightProperty =
        DependencyProperty.Register(nameof(Weight), typeof(FontWeight), typeof(MaterialIcon),
            new FrameworkPropertyMetadata(FontWeight.FromOpenTypeWeight(300), OnVisualChanged));

    public FontWeight Weight
    {
        get => (FontWeight)GetValue(WeightProperty);
        set => SetValue(WeightProperty, value);
    }

    // ── Fill（false=线框 true=实心） ──────────────────────────────────────
    public static readonly DependencyProperty FillProperty =
        DependencyProperty.Register(nameof(Fill), typeof(bool), typeof(MaterialIcon),
            new FrameworkPropertyMetadata(false, OnVisualChanged));

    /// <summary>false = 线框（Rounded），true = 实心（Filled）。</summary>
    public bool Fill
    {
        get => (bool)GetValue(FillProperty);
        set => SetValue(FillProperty, value);
    }

    private static void OnVisualChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        => ((MaterialIcon)d).UpdateText();

    // ─────────────────────────────────────────────────────────────────────
    public override void OnApplyTemplate()
    {
        base.OnApplyTemplate();
        _text = GetTemplateChild(PartText) as TextBlock;
        UpdateText();
    }

    private void UpdateText()
    {
        if (_text == null) return;
        _text.Text       = Kind;
        _text.FontSize   = Size;
        _text.FontWeight = Weight;
        _text.FontFamily = Fill ? FilledFont : OutlineFont;
    }
}
