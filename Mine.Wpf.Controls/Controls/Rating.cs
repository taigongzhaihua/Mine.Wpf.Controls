using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Mine.Wpf.Controls.Controls;

/// <summary>Material 3 星级评分控件。</summary>
public class Rating : Control
{
    static Rating()
    {
        DefaultStyleKeyProperty.OverrideMetadata(
            typeof(Rating), new FrameworkPropertyMetadata(typeof(Rating)));
    }

    // ── Value ────────────────────────────────────────────────────────
    public static readonly DependencyProperty ValueProperty =
        DependencyProperty.Register(nameof(Value), typeof(double), typeof(Rating),
            new FrameworkPropertyMetadata(0.0,
                FrameworkPropertyMetadataOptions.BindsTwoWayByDefault,
                OnValueChanged));

    public double Value
    {
        get => (double)GetValue(ValueProperty);
        set => SetValue(ValueProperty, Math.Clamp(value, 0, Maximum));
    }

    private static void OnValueChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        var r = (Rating)d;
        r.UpdateStars();
        r.RaiseEvent(new RoutedPropertyChangedEventArgs<double>(
            (double)e.OldValue, (double)e.NewValue, ValueChangedEvent));
    }

    // ── Maximum ──────────────────────────────────────────────────────
    public static readonly DependencyProperty MaximumProperty =
        DependencyProperty.Register(nameof(Maximum), typeof(int), typeof(Rating),
            new PropertyMetadata(5, (d, _) => ((Rating)d).RebuildStars()));

    public int Maximum
    {
        get => (int)GetValue(MaximumProperty);
        set => SetValue(MaximumProperty, value);
    }

    // ── IsReadOnly ───────────────────────────────────────────────────
    public static readonly DependencyProperty IsReadOnlyProperty =
        DependencyProperty.Register(nameof(IsReadOnly), typeof(bool), typeof(Rating),
            new PropertyMetadata(false));

    public bool IsReadOnly
    {
        get => (bool)GetValue(IsReadOnlyProperty);
        set => SetValue(IsReadOnlyProperty, value);
    }

    // ── AllowHalfStar ────────────────────────────────────────────────
    public static readonly DependencyProperty AllowHalfStarProperty =
        DependencyProperty.Register(nameof(AllowHalfStar), typeof(bool), typeof(Rating),
            new PropertyMetadata(false));

    public bool AllowHalfStar
    {
        get => (bool)GetValue(AllowHalfStarProperty);
        set => SetValue(AllowHalfStarProperty, value);
    }

    // ── ValueChanged 事件 ─────────────────────────────────────────────
    public static readonly RoutedEvent ValueChangedEvent =
        EventManager.RegisterRoutedEvent(nameof(ValueChanged), RoutingStrategy.Bubble,
            typeof(RoutedPropertyChangedEventHandler<double>), typeof(Rating));

    public event RoutedPropertyChangedEventHandler<double> ValueChanged
    {
        add    => AddHandler(ValueChangedEvent, value);
        remove => RemoveHandler(ValueChangedEvent, value);
    }

    // ── 星星容器 ──────────────────────────────────────────────────────
    private StackPanel? _starPanel;
    private double _hoverValue = -1;

    public override void OnApplyTemplate()
    {
        base.OnApplyTemplate();
        _starPanel = GetTemplateChild("PART_Stars") as StackPanel;
        RebuildStars();
    }

    private void RebuildStars()
    {
        if (_starPanel == null) return;
        _starPanel.Children.Clear();
        for (var i = 1; i <= Maximum; i++)
        {
            var star = CreateStar(i);
            _starPanel.Children.Add(star);
        }
        UpdateStars();
    }

    private UIElement CreateStar(int index)
    {
        var tb = new TextBlock
        {
            FontSize          = 28,
            Text              = "\uE838",
            Cursor            = IsReadOnly ? null : Cursors.Hand,
            Margin            = new Thickness(2, 0, 2, 0),
            VerticalAlignment = VerticalAlignment.Center,
            Tag               = index,
            FontFamily        = MaterialIcon.OutlineFont
        };

        if (IsReadOnly) return tb;
        tb.MouseEnter        += OnStarMouseEnter;
        tb.MouseLeave        += OnStarMouseLeave;
        tb.MouseLeftButtonUp += OnStarClick;

        return tb;
    }

    private void OnStarMouseEnter(object sender, MouseEventArgs e)
    {
        if (sender is not TextBlock { Tag: int idx }) return;
        _hoverValue = idx;
        RenderStars(_hoverValue);
    }

    private void OnStarMouseLeave(object sender, MouseEventArgs e)
    {
        _hoverValue = -1;
        UpdateStars();
    }

    private void OnStarClick(object sender, MouseButtonEventArgs e)
    {
        if (sender is not TextBlock { Tag: int idx }) return;
        // 再次点击同一颗星则清零
        Value = Math.Abs(Value - idx) < 0.01 ? 0 : idx;
    }

    private void UpdateStars() => RenderStars(Value);

    private void RenderStars(double highlightUpTo)
    {
        if (_starPanel == null) return;
        for (var i = 0; i < _starPanel.Children.Count; i++)
        {
            if (_starPanel.Children[i] is not TextBlock tb) continue;
            var starIndex = i + 1;
            var isFilled = starIndex <= highlightUpTo;
            tb.Text = "\uE838";
            tb.FontFamily = isFilled ? MaterialIcon.FilledFont : MaterialIcon.OutlineFont;
            tb.SetResourceReference(TextBlock.ForegroundProperty,
                isFilled ? "Mine.Brush.Primary" : "Mine.Brush.OnSurfaceVariant");
        }
    }
}

