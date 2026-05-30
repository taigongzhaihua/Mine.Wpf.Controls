using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Media;
using Mine.Wpf.Controls.Theming;

namespace Mine.Wpf.Controls.Gallery.Pages;

public partial class ColorPage : System.Windows.Controls.UserControl
{
    public ColorPage()
    {
        InitializeComponent();
        DataContext = new ColorPageViewModel();
    }
}

public sealed class ColorPageViewModel : INotifyPropertyChanged
{
    private string _themeLabel = "";
    private string _seedHex    = "";
    private bool   _isPreset;
    private ObservableCollection<ToneSwatch> _primaryTones   = [];
    private ObservableCollection<ToneSwatch> _secondaryTones = [];
    private ObservableCollection<ToneSwatch> _tertiaryTones  = [];

    public string ThemeLabel { get => _themeLabel; set { _themeLabel = value; OnProp(); } }
    public string SeedHex    { get => _seedHex;    set { _seedHex    = value; OnProp(); } }
    public bool   IsPreset   { get => _isPreset;   set { _isPreset   = value; OnProp(); OnProp(nameof(IsCustom)); } }
    public bool   IsCustom   => !_isPreset;

    public ObservableCollection<ToneSwatch> PrimaryTones   { get => _primaryTones;   set { _primaryTones   = value; OnProp(); } }
    public ObservableCollection<ToneSwatch> SecondaryTones { get => _secondaryTones; set { _secondaryTones = value; OnProp(); } }
    public ObservableCollection<ToneSwatch> TertiaryTones  { get => _tertiaryTones;  set { _tertiaryTones  = value; OnProp(); } }

    public ColorPageViewModel()
    {
        Refresh();
        ThemeManager.ThemeChanged += (_, _) =>
            Application.Current?.Dispatcher.BeginInvoke(Refresh);
    }

    private void Refresh()
    {
        var preset = ThemeManager.Preset;
        var seed   = ThemeManager.SeedColor;

        IsPreset   = preset != ThemePreset.None;
        ThemeLabel = IsPreset ? $"预设主题：{preset}" : "自定义种子颜色";
        SeedHex    = IsPreset ? "" : $"#{seed.R:X2}{seed.G:X2}{seed.B:X2}";

        // 只在自定义模式下生成调色板预览（预设时无 ColorScheme 对象）
        if (IsCustom)
        {
            var p  = TonalPaletteGenerator.Generate(seed);
            var sp = TonalPaletteGenerator.Generate(RotateHue(seed, -30));
            var tp = TonalPaletteGenerator.Generate(RotateHue(seed,  60));
            PrimaryTones   = ToSwatches(p,  "P");
            SecondaryTones = ToSwatches(sp, "S");
            TertiaryTones  = ToSwatches(tp, "T");
        }
        else
        {
            PrimaryTones   = [];
            SecondaryTones = [];
            TertiaryTones  = [];
        }
    }

    private static ObservableCollection<ToneSwatch> ToSwatches(Dictionary<int, Color> palette, string prefix)
    {
        var list = new ObservableCollection<ToneSwatch>();
        foreach (var (tone, color) in palette.OrderBy(x => x.Key))
        {
            // 根据明度决定文字颜色
            var brightness = (color.R * 299 + color.G * 587 + color.B * 114) / 1000.0 / 255.0;
            var fg = brightness > 0.45 ? Color.FromRgb(0x1C, 0x1B, 0x1F) : Color.FromRgb(0xFF, 0xFF, 0xFF);
            list.Add(new ToneSwatch
            {
                Label  = $"{prefix}{tone}",
                Bg     = new SolidColorBrush(color),
                Fg     = new SolidColorBrush(fg),
            });
        }
        return list;
    }

    private static Color RotateHue(Color c, double degrees)
    {
        // 简单 HSL 色相旋转用于预览
        RgbToHsl(c, out var h, out var s, out var l);
        h = (h + degrees / 360.0 + 1.0) % 1.0;
        return HslToRgb(h, s, l);
    }
    private static void RgbToHsl(Color c, out double h, out double s, out double l)
    {
        double r = c.R / 255.0, g = c.G / 255.0, b = c.B / 255.0;
        var max = Math.Max(r, Math.Max(g, b)); var min = Math.Min(r, Math.Min(g, b));
        l = (max + min) / 2.0;
        var d = max - min;
        if (d < 1e-10) { h = 0; s = 0; return; }
        s = d / (1.0 - Math.Abs(2 * l - 1));
        if      (max == r) h = ((g - b) / d % 6) / 6.0;
        else if (max == g) h = ((b - r) / d + 2) / 6.0;
        else               h = ((r - g) / d + 4) / 6.0;
        if (h < 0) h += 1.0;
    }
    private static Color HslToRgb(double h, double s, double l)
    {
        var cv = (1 - Math.Abs(2 * l - 1)) * s;
        var x = cv * (1 - Math.Abs(h * 6 % 2 - 1));
        var m = l - cv / 2;
        double r, g, b;
        var sec = (int)(h * 6);
        (r, g, b) = sec switch { 0 => (cv, x, 0.0), 1 => (x, cv, 0.0), 2 => (0.0, cv, x),
            3 => (0.0, x, cv), 4 => (x, 0.0, cv), _ => (cv, 0.0, x) };
        return Color.FromRgb((byte)((r + m) * 255), (byte)((g + m) * 255), (byte)((b + m) * 255));
    }

    public event PropertyChangedEventHandler? PropertyChanged;
    private void OnProp([CallerMemberName] string? n = null)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(n));
}

public sealed class ToneSwatch
{
    public string          Label { get; init; } = "";
    public SolidColorBrush Bg    { get; init; } = new(Colors.Gray);
    public SolidColorBrush Fg    { get; init; } = new(Colors.White);
}
