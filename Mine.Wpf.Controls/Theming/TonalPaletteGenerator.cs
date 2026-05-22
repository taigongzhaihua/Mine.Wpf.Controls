using System.Windows.Media;
namespace Mine.Wpf.Controls.Theming;
/// <summary>
/// 基于 HCT 近似算法，从种子颜色生成 Material 3 色调调色板。
/// 产生 21 个色调：0, 4, 6, 10, 12, 17, 20, 22, 30, 40, 50, 60, 70, 80, 90, 92, 94, 95, 96, 99, 100。
/// </summary>
public static class TonalPaletteGenerator
{
    private static readonly int[] Tones = [0, 4, 6, 10, 12, 17, 20, 22, 30, 40, 50, 60, 70, 80, 90, 92, 94, 95, 96, 99, 100];
    /// <summary>从种子颜色生成色调调色板（色调 → Color 映射）。</summary>
    public static Dictionary<int, Color> Generate(Color seed)
    {
        ColorToHsl(seed, out double h, out double s, out _);
        // 限制饱和度下限，防止近似无彩色种子生成灰色调色板
        s = Math.Max(s, 0.12);
        var palette = new Dictionary<int, Color>(Tones.Length);
        foreach (var tone in Tones)
        {
            double l = tone / 100.0;
            palette[tone] = HslToColor(h, s, l);
        }
        return palette;
    }
    /// <summary>从种子颜色构建亮色 ColorScheme，可选次要色/第三色种子。</summary>
    public static ColorScheme BuildLight(Color seed, Color? secondarySeed = null, Color? tertiarySeed = null)
    {
        var p  = Generate(seed);
        var sp = Generate(secondarySeed ?? RotateHue(seed, -30));
        var tp = Generate(tertiarySeed  ?? RotateHue(seed,  60));
        var ep = GenerateError();
        return new ColorScheme
        {
            Primary             = p[40],
            OnPrimary           = p[100],
            PrimaryContainer    = p[90],
            OnPrimaryContainer  = p[10],
            Secondary            = sp[40],
            OnSecondary          = sp[100],
            SecondaryContainer   = sp[90],
            OnSecondaryContainer = sp[10],
            Tertiary            = tp[40],
            OnTertiary          = tp[100],
            TertiaryContainer   = tp[90],
            OnTertiaryContainer = tp[10],
            Error            = ep[40],
            OnError          = ep[100],
            ErrorContainer   = ep[90],
            OnErrorContainer = ep[10],
            Surface                 = p[99],
            OnSurface               = p[10],
            SurfaceVariant          = sp[90],
            OnSurfaceVariant        = sp[30],
            SurfaceTint             = p[40],
            SurfaceContainerLowest  = p[100],
            SurfaceContainerLow     = p[96],
            SurfaceContainer        = p[94],
            SurfaceContainerHigh    = p[92],
            SurfaceContainerHighest = p[90],
            Background   = p[99],
            OnBackground = p[10],
            Outline        = sp[50],
            OutlineVariant = sp[80],
            InverseSurface   = p[20],
            InverseOnSurface = p[95],
            InversePrimary   = p[80],
            Shadow = Color.FromRgb(0, 0, 0),
            Scrim  = Color.FromRgb(0, 0, 0),
            Warning            = Color.FromRgb(0x79, 0x55, 0x00),
            OnWarning          = Color.FromRgb(0xFF, 0xFF, 0xFF),
            WarningContainer   = Color.FromRgb(0xFF, 0xDF, 0x99),
            OnWarningContainer = Color.FromRgb(0x26, 0x1A, 0x00),
            Success            = Color.FromRgb(0x18, 0x6C, 0x2B),
            OnSuccess          = Color.FromRgb(0xFF, 0xFF, 0xFF),
            SuccessContainer   = Color.FromRgb(0xB5, 0xF2, 0xBB),
            OnSuccessContainer = Color.FromRgb(0x00, 0x21, 0x09),
            Info               = Color.FromRgb(0x00, 0x63, 0x8A),
            OnInfo             = Color.FromRgb(0xFF, 0xFF, 0xFF),
            InfoContainer      = Color.FromRgb(0xC5, 0xE7, 0xFF),
            OnInfoContainer    = Color.FromRgb(0x00, 0x1E, 0x2C),
        };
    }
    /// <summary>从种子颜色构建暗色 ColorScheme。</summary>
    public static ColorScheme BuildDark(Color seed, Color? secondarySeed = null, Color? tertiarySeed = null)
    {
        var p  = Generate(seed);
        var sp = Generate(secondarySeed ?? RotateHue(seed, -30));
        var tp = Generate(tertiarySeed  ?? RotateHue(seed,  60));
        var ep = GenerateError();
        return new ColorScheme
        {
            Primary             = p[80],
            OnPrimary           = p[20],
            PrimaryContainer    = p[30],
            OnPrimaryContainer  = p[90],
            Secondary            = sp[80],
            OnSecondary          = sp[20],
            SecondaryContainer   = sp[30],
            OnSecondaryContainer = sp[90],
            Tertiary            = tp[80],
            OnTertiary          = tp[20],
            TertiaryContainer   = tp[30],
            OnTertiaryContainer = tp[90],
            Error            = ep[80],
            OnError          = ep[20],
            ErrorContainer   = ep[30],
            OnErrorContainer = ep[90],
            Surface                 = p[6],
            OnSurface               = p[90],
            SurfaceVariant          = sp[30],
            OnSurfaceVariant        = sp[80],
            SurfaceTint             = p[80],
            SurfaceContainerLowest  = p[4],
            SurfaceContainerLow     = p[10],
            SurfaceContainer        = p[12],
            SurfaceContainerHigh    = p[17],
            SurfaceContainerHighest = p[22],
            Background   = p[6],
            OnBackground = p[90],
            Outline        = sp[60],
            OutlineVariant = sp[30],
            InverseSurface   = p[90],
            InverseOnSurface = p[20],
            InversePrimary   = p[40],
            Shadow = Color.FromRgb(0, 0, 0),
            Scrim  = Color.FromRgb(0, 0, 0),
            Warning            = Color.FromRgb(0xF5, 0xBF, 0x15),
            OnWarning          = Color.FromRgb(0x3F, 0x2E, 0x00),
            WarningContainer   = Color.FromRgb(0x5C, 0x43, 0x00),
            OnWarningContainer = Color.FromRgb(0xFF, 0xDF, 0x99),
            Success            = Color.FromRgb(0x9A, 0xD6, 0xA0),
            OnSuccess          = Color.FromRgb(0x00, 0x39, 0x14),
            SuccessContainer   = Color.FromRgb(0x00, 0x52, 0x1F),
            OnSuccessContainer = Color.FromRgb(0xB5, 0xF2, 0xBB),
            Info               = Color.FromRgb(0x86, 0xCE, 0xFF),
            OnInfo             = Color.FromRgb(0x00, 0x34, 0x49),
            InfoContainer      = Color.FromRgb(0x00, 0x4C, 0x68),
            OnInfoContainer    = Color.FromRgb(0xC5, 0xE7, 0xFF),
        };
    }
    // ── 辅助方法 ──────────────────────────────────────────────────
    private static Dictionary<int, Color> GenerateError()
        => Generate(Color.FromRgb(0xB3, 0x26, 0x1E));
    private static Color RotateHue(Color c, double degrees)
    {
        ColorToHsl(c, out double h, out double s, out double l);
        h = (h + degrees / 360.0 + 1.0) % 1.0;
        return HslToColor(h, s, l);
    }
    private static void ColorToHsl(Color c, out double h, out double s, out double l)
    {
        double r = c.R / 255.0, g = c.G / 255.0, b = c.B / 255.0;
        double max = Math.Max(r, Math.Max(g, b));
        double min = Math.Min(r, Math.Min(g, b));
        l = (max + min) / 2.0;
        double delta = max - min;
        if (delta < 1e-10) { h = 0; s = 0; return; }
        s = delta / (1.0 - Math.Abs(2 * l - 1));
        if      (max == r) h = ((g - b) / delta % 6) / 6.0;
        else if (max == g) h = ((b - r) / delta + 2) / 6.0;
        else               h = ((r - g) / delta + 4) / 6.0;
        if (h < 0) h += 1.0;
    }
    private static Color HslToColor(double h, double s, double l)
    {
        double c = (1 - Math.Abs(2 * l - 1)) * s;
        double x = c * (1 - Math.Abs(h * 6 % 2 - 1));
        double m = l - c / 2;
        double r, g, b;
        int sector = (int)(h * 6);
        (r, g, b) = sector switch
        {
            0 => (c, x, 0.0),
            1 => (x, c, 0.0),
            2 => (0.0, c, x),
            3 => (0.0, x, c),
            4 => (x, 0.0, c),
            _ => (c, 0.0, x),
        };
        return Color.FromRgb(
            (byte)Math.Round(Math.Clamp(r + m, 0, 1) * 255),
            (byte)Math.Round(Math.Clamp(g + m, 0, 1) * 255),
            (byte)Math.Round(Math.Clamp(b + m, 0, 1) * 255));
    }
    /// <summary>插值获取非标准色调的颜色。</summary>
    private static Color GetTone(Dictionary<int, Color> palette, int tone)
    {
        if (palette.TryGetValue(tone, out var exact)) return exact;
        // 查找相邻色调
        int lo = Tones.Where(t => t <= tone).DefaultIfEmpty(0).Max();
        int hi = Tones.Where(t => t >= tone).DefaultIfEmpty(100).Min();
        if (lo == hi) return palette[lo];
        double t = (double)(tone - lo) / (hi - lo);
        var cLo = palette[lo]; var cHi = palette[hi];
        return Color.FromRgb(
            (byte)(cLo.R + t * (cHi.R - cLo.R)),
            (byte)(cLo.G + t * (cHi.G - cLo.G)),
            (byte)(cLo.B + t * (cHi.B - cLo.B)));
    }
}
// 扩展类：允许 Dictionary<int,Color> 以非标准色调索引访问
file static class PaletteExtensions
{
    internal static Color this2(this Dictionary<int, Color> d, int tone)
    {
        if (d.TryGetValue(tone, out var c)) return c;
        // 钳制到有效范围
        tone = Math.Clamp(tone, 0, 100);
        int lo = d.Keys.Where(k => k <= tone).DefaultIfEmpty(0).Max();
        int hi = d.Keys.Where(k => k >= tone).DefaultIfEmpty(100).Min();
        if (lo == hi) return d[lo];
        double t = (double)(tone - lo) / (hi - lo);
        var a = d[lo]; var b = d[hi];
        return Color.FromRgb(
            (byte)(a.R + t * (b.R - a.R)),
            (byte)(a.G + t * (b.G - a.G)),
            (byte)(a.B + t * (b.B - a.B)));
    }
}