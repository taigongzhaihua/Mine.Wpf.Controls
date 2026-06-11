using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;

namespace Mine.Wpf.Controls.Controls;

/// <summary>
/// Material 3 颜色选择器。
/// 包含色相滑块、饱和度/亮度面板和透明度滑块，支持 Hex 输入。
/// </summary>
[TemplatePart(Name = PartSbCanvas,     Type = typeof(Canvas))]
[TemplatePart(Name = PartSbThumb,      Type = typeof(Ellipse))]
[TemplatePart(Name = PartHueSlider,    Type = typeof(System.Windows.Controls.Slider))]
[TemplatePart(Name = PartAlphaSlider,  Type = typeof(System.Windows.Controls.Slider))]
[TemplatePart(Name = PartHexBox,       Type = typeof(System.Windows.Controls.TextBox))]
[TemplatePart(Name = PartPreview,      Type = typeof(Border))]
public class ColorPicker : Control
{
    private const string PartSbCanvas    = "PART_SbCanvas";
    private const string PartSbThumb     = "PART_SbThumb";
    private const string PartHueSlider   = "PART_HueSlider";
    private const string PartAlphaSlider = "PART_AlphaSlider";
    private const string PartHexBox      = "PART_HexBox";
    private const string PartPreview     = "PART_Preview";

    private Canvas?  _sbCanvas;
    private Ellipse? _sbThumb;
    private System.Windows.Controls.Slider? _hueSlider;
    private System.Windows.Controls.Slider? _alphaSlider;
    private System.Windows.Controls.TextBox? _hexBox;
    private Border? _preview;

    // HSV 内部状态
    private double _hue;            // [0,360)
    private double _saturation = 1; // [0,1]
    private double _brightness = 1; // [0,1]
    private byte   _alpha      = 255;
    private bool   _updating;

    static ColorPicker()
    {
        DefaultStyleKeyProperty.OverrideMetadata(
            typeof(ColorPicker), new FrameworkPropertyMetadata(typeof(ColorPicker)));
    }

    // ── SelectedColor ─────────────────────────────────────────────────
    public static readonly DependencyProperty SelectedColorProperty =
        DependencyProperty.Register(nameof(SelectedColor), typeof(Color), typeof(ColorPicker),
            new FrameworkPropertyMetadata(Colors.Red,
                FrameworkPropertyMetadataOptions.BindsTwoWayByDefault,
                OnSelectedColorChanged));

    public Color SelectedColor
    {
        get => (Color)GetValue(SelectedColorProperty);
        set => SetValue(SelectedColorProperty, value);
    }

    private static void OnSelectedColorChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        var cp = (ColorPicker)d;
        cp._updating = true;
        try { ColorToHsv((Color)e.NewValue, out cp._hue, out cp._saturation, out cp._brightness); }
        finally { cp._updating = false; }
        cp._alpha = ((Color)e.NewValue).A;
        // 模板未应用时（_sbCanvas==null）跳过，OnApplyTemplate 会初始化
        if (cp is { _updating: false, _sbCanvas: not null }) cp.UpdateAllFromHsv();
        cp.RaiseEvent(new RoutedPropertyChangedEventArgs<Color>(
            (Color)e.OldValue, (Color)e.NewValue, SelectedColorChangedEvent));
    }

    // ── ShowAlpha ─────────────────────────────────────────────────────
    public static readonly DependencyProperty ShowAlphaProperty =
        DependencyProperty.Register(nameof(ShowAlpha), typeof(bool), typeof(ColorPicker),
            new PropertyMetadata(false));

    public bool ShowAlpha
    {
        get => (bool)GetValue(ShowAlphaProperty);
        set => SetValue(ShowAlphaProperty, value);
    }

    // ── HueGradient（只读，供色相滑块背景用） ─────────────────────────
    private static readonly DependencyPropertyKey HueGradientKey =
        DependencyProperty.RegisterReadOnly(nameof(HueGradient), typeof(LinearGradientBrush), typeof(ColorPicker),
            new PropertyMetadata(BuildHueGradient()));
    public static readonly DependencyProperty HueGradientProperty = HueGradientKey.DependencyProperty;
    public LinearGradientBrush HueGradient => (LinearGradientBrush)GetValue(HueGradientProperty);

    // ── SelectedColorChangedEvent ─────────────────────────────────────
    public static readonly RoutedEvent SelectedColorChangedEvent =
        EventManager.RegisterRoutedEvent(nameof(SelectedColorChanged), RoutingStrategy.Bubble,
            typeof(RoutedPropertyChangedEventHandler<Color>), typeof(ColorPicker));

    public event RoutedPropertyChangedEventHandler<Color> SelectedColorChanged
    {
        add    => AddHandler(SelectedColorChangedEvent, value);
        remove => RemoveHandler(SelectedColorChangedEvent, value);
    }

    // ── Template ─────────────────────────────────────────────────────
    public override void OnApplyTemplate()
    {
        base.OnApplyTemplate();

        _sbCanvas    = GetTemplateChild(PartSbCanvas)    as Canvas;
        _sbThumb     = GetTemplateChild(PartSbThumb)     as Ellipse;
        _hueSlider   = GetTemplateChild(PartHueSlider)   as System.Windows.Controls.Slider;
        _alphaSlider = GetTemplateChild(PartAlphaSlider) as System.Windows.Controls.Slider;
        _hexBox      = GetTemplateChild(PartHexBox)      as System.Windows.Controls.TextBox;
        _preview     = GetTemplateChild(PartPreview)     as Border;

        if (_sbCanvas != null)
        {
            _sbCanvas.MouseLeftButtonDown += OnSbMouseDown;
            _sbCanvas.MouseMove           += OnSbMouseMove;
        }

        if (_hueSlider != null)
            _hueSlider.ValueChanged += (_, _) =>
            {
                _hue = _hueSlider.Value;
                CommitHsv();
            };

        if (_alphaSlider != null)
            _alphaSlider.ValueChanged += (_, _) =>
            {
                _alpha = (byte)_alphaSlider.Value;
                CommitHsv();
            };

        if (_hexBox != null)
        {
            _hexBox.LostFocus += (_, _) => TryParseHex(_hexBox.Text);
            _hexBox.KeyDown   += (_, e) => { if (e.Key == Key.Enter) TryParseHex(_hexBox.Text); };
        }

        // 初始化
        ColorToHsv(SelectedColor, out _hue, out _saturation, out _brightness);
        _alpha = SelectedColor.A;
        UpdateAllFromHsv();
    }

    // ── SB 面板交互 ───────────────────────────────────────────────────
    private bool _draggingSb;

    private void OnSbMouseDown(object sender, MouseButtonEventArgs e)
    {
        _draggingSb = true;
        _sbCanvas!.CaptureMouse();
        _sbCanvas.MouseLeftButtonUp += OnSbMouseUp;
        UpdateSbFromPoint(e.GetPosition(_sbCanvas));
    }

    private void OnSbMouseMove(object sender, MouseEventArgs e)
    {
        if (_draggingSb && e.LeftButton == MouseButtonState.Pressed)
            UpdateSbFromPoint(e.GetPosition(_sbCanvas!));
    }

    private void OnSbMouseUp(object sender, MouseButtonEventArgs e)
    {
        _draggingSb = false;
        _sbCanvas?.ReleaseMouseCapture();
        if (_sbCanvas != null) _sbCanvas.MouseLeftButtonUp -= OnSbMouseUp;
    }

    private void UpdateSbFromPoint(Point p)
    {
        if (_sbCanvas == null) return;
        var w = _sbCanvas.ActualWidth;
        var h = _sbCanvas.ActualHeight;
        _saturation = Math.Clamp(p.X / w, 0, 1);
        _brightness = Math.Clamp(1 - p.Y / h, 0, 1);
        CommitHsv();
        MoveThumb(p.X, p.Y);
    }

    private void MoveThumb(double x, double y)
    {
        if (_sbThumb == null || _sbCanvas == null) return;
        var maxX = _sbCanvas.ActualWidth  - _sbThumb.Width;
        var maxY = _sbCanvas.ActualHeight - _sbThumb.Height;
        // 模板尚未布局时跳过，等 SizeChanged 触发后再更新
        if (maxX < 0 || maxY < 0) return;
        Canvas.SetLeft(_sbThumb, Math.Clamp(x - _sbThumb.Width  / 2, 0, maxX));
        Canvas.SetTop (_sbThumb, Math.Clamp(y - _sbThumb.Height / 2, 0, maxY));
    }

    // ── 颜色提交 ──────────────────────────────────────────────────────
    private void CommitHsv()
    {
        _updating = true;
        try
        {
            SelectedColor = HsvToColor(_hue, _saturation, _brightness, _alpha);
        }
        finally { _updating = false; }
        UpdatePreview();
        UpdateHexBox();
    }

    private void UpdateAllFromHsv()
    {
        if (_hueSlider   != null) _hueSlider.Value   = _hue;
        if (_alphaSlider != null) _alphaSlider.Value  = _alpha;
        // 移动 SB 游标
        if (_sbCanvas != null && _sbThumb != null)
        {
            var x = _saturation * _sbCanvas.ActualWidth;
            var y = (1 - _brightness) * _sbCanvas.ActualHeight;
            MoveThumb(x, y);
        }
        // 更新 SB 面板背景色（纯色相）
        UpdateSbBackground();
        UpdatePreview();
        UpdateHexBox();
    }

    private void UpdateSbBackground()
    {
        if (_sbCanvas == null) return;
        var hueColor = HsvToColor(_hue, 1, 1, 255);
        // 背景由 XAML 中的渐变处理（绑定 HsvHue），这里通过 Tag 传递
        _sbCanvas.Tag = hueColor;
        // 更新色相背景 Fill（需要 PART_HueBg Rectangle）
        if (_sbCanvas.Children.Count > 0 &&
            _sbCanvas.FindName("PART_HueBg") is System.Windows.Shapes.Rectangle hueBg)
            hueBg.Fill = new SolidColorBrush(hueColor);
    }

    private void UpdatePreview()
    {
        if (_preview != null)
            _preview.Background = new SolidColorBrush(SelectedColor);
    }

    private void UpdateHexBox()
    {
        if (_hexBox == null) return;
        var c = SelectedColor;
        _hexBox.Text = ShowAlpha
            ? $"#{c.A:X2}{c.R:X2}{c.G:X2}{c.B:X2}"
            : $"#{c.R:X2}{c.G:X2}{c.B:X2}";
    }

    private void TryParseHex(string text)
    {
        try
        {
            var c = (Color)ColorConverter.ConvertFromString(text.StartsWith('#') ? text : "#" + text);
            SelectedColor = c;
        }
        catch { UpdateHexBox(); }
    }

    // ── HSV 转换 ──────────────────────────────────────────────────────
    private static void ColorToHsv(Color c, out double h, out double s, out double v)
    {
        double r = c.R / 255.0, g = c.G / 255.0, b = c.B / 255.0;
        var max = Math.Max(r, Math.Max(g, b));
        var min = Math.Min(r, Math.Min(g, b));
        var delta = max - min;
        v = max;
        s = max == 0 ? 0 : delta / max;
        if (delta == 0) { h = 0; return; }
        if (max == r)      h = 60 * ((g - b) / delta % 6);
        else if (max == g) h = 60 * ((b - r) / delta + 2);
        else               h = 60 * ((r - g) / delta + 4);
        if (h < 0) h += 360;
    }

    private static Color HsvToColor(double h, double s, double v, byte a)
    {
        h = ((h % 360) + 360) % 360;
        var hi = (int)(h / 60) % 6;
        var f  = h / 60 - Math.Floor(h / 60);
        var p  = v * (1 - s);
        var q  = v * (1 - f * s);
        var t  = v * (1 - (1 - f) * s);
        (var r, var g, var b) = hi switch
        {
            0 => (v, t, p),
            1 => (q, v, p),
            2 => (p, v, t),
            3 => (p, q, v),
            4 => (t, p, v),
            _ => (v, p, q),
        };
        return Color.FromArgb(a, (byte)(r * 255), (byte)(g * 255), (byte)(b * 255));
    }

    private static LinearGradientBrush BuildHueGradient()
    {
        var brush = new LinearGradientBrush
        {
            StartPoint = new Point(0, 0.5),
            EndPoint   = new Point(1, 0.5),
        };
        for (var i = 0; i <= 6; i++)
        {
            var h = i * 60.0;
            brush.GradientStops.Add(new GradientStop(HsvToColor(h, 1, 1, 255), i / 6.0));
        }
        return brush;
    }
}



