using System.Windows.Media;

namespace Mine.Wpf.Controls.Theming;

/// <summary>
/// 基于 OKLCH 感知均匀色彩空间，从种子颜色生成 Material 3 色调调色板。
/// OKLCH 确保不同色相在相同 Tone 下具有一致的感知明度，支持全色相范围。
/// 产生 21 个色调：0, 4, 6, 10, 12, 17, 20, 22, 30, 40, 50, 60, 70, 80, 90, 92, 94, 95, 96, 99, 100。
/// </summary>
public static class TonalPaletteGenerator
{
    private static readonly int[] Tones = [0, 4, 6, 10, 12, 17, 20, 22, 30, 40, 50, 60, 70, 80, 90, 92, 94, 95, 96, 99, 100];

    /// <summary>从种子颜色生成色调调色板（色调 → Color 映射）。</summary>
    public static Dictionary<int, Color> Generate(Color seed)
    {
        RgbToOklch(seed, out var _, out var c, out var h);

        // 确保最低彩度，防止近似无彩色种子生成灰色调色板
        c = Math.Max(c, 0.04);

        var palette = new Dictionary<int, Color>(Tones.Length);
        foreach (var tone in Tones)
        {
            var l = ToneToOklabL(tone);
            // 在该明度下将彩度限制在 sRGB 色域内
            var clampedC = ClampChromaToGamut(l, c, h);
            palette[tone] = OklchToRgb(l, clampedC, h);
        }
        return palette;
    }

    /// <summary>从种子颜色构建亮色 ColorScheme，可选次要色/第三色种子。</summary>
    public static ColorScheme BuildLight(Color seed, Color? secondarySeed = null, Color? tertiarySeed = null)
    {
        var p  = Generate(seed);
        var sp = Generate(secondarySeed ?? RotateHue(seed, -30));
        var tp = Generate(tertiarySeed  ?? RotateHue(seed,  60));
        var np  = GenerateNeutral(seed, chromaScale: 0.10);
        var nvp = GenerateNeutral(seed, chromaScale: 0.06);
        var ep  = GenerateError();
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
            Surface                 = np[99],
            OnSurface               = np[10],
            SurfaceVariant          = nvp[90],
            OnSurfaceVariant        = nvp[30],
            SurfaceTint             = p[40],
            SurfaceContainerLowest  = np[100],
            SurfaceContainerLow     = np[96],
            SurfaceContainer        = np[94],
            SurfaceContainerHigh    = np[92],
            SurfaceContainerHighest = np[90],
            Background   = np[99],
            OnBackground = np[10],
            Outline        = nvp[50],
            OutlineVariant = nvp[80],
            InverseSurface   = np[20],
            InverseOnSurface = np[95],
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
        var np  = GenerateNeutral(seed, chromaScale: 0.10);
        var nvp = GenerateNeutral(seed, chromaScale: 0.06);
        var ep  = GenerateError();
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
            Surface                 = np[6],
            OnSurface               = np[90],
            SurfaceVariant          = nvp[30],
            OnSurfaceVariant        = nvp[80],
            SurfaceTint             = p[80],
            SurfaceContainerLowest  = np[4],
            SurfaceContainerLow     = np[10],
            SurfaceContainer        = np[12],
            SurfaceContainerHigh    = np[17],
            SurfaceContainerHighest = np[22],
            Background   = np[6],
            OnBackground = np[90],
            Outline        = nvp[60],
            OutlineVariant = nvp[30],
            InverseSurface   = np[90],
            InverseOnSurface = np[20],
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

    /// <summary>
    /// 生成中性调色板：保留种子色相，但将彩度按比例缩减（用于 Surface/Neutral 角色）。
    /// chromaScale：种子彩度的保留比例（0.0 = 纯灰，1.0 = 完整彩度）。
    /// </summary>
    private static Dictionary<int, Color> GenerateNeutral(Color seed, double chromaScale)
    {
        RgbToOklch(seed, out _, out var c, out var h);
        // 中性调板：低彩度，保留色相以产生微妙的色彩感
        var neutralC = Math.Min(c * chromaScale, 0.04);

        var palette = new Dictionary<int, Color>(Tones.Length);
        foreach (var tone in Tones)
        {
            var l = ToneToOklabL(tone);
            var clampedC = ClampChromaToGamut(l, neutralC, h);
            palette[tone] = OklchToRgb(l, clampedC, h);
        }
        return palette;
    }

    /// <summary>旋转种子颜色的色相（OKLCH 空间，单位：度）。</summary>
    private static Color RotateHue(Color c, double degrees)
    {
        RgbToOklch(c, out var l, out var chroma, out var h);
        h = (h + degrees + 360.0) % 360.0;
        var clampedC = ClampChromaToGamut(l, chroma, h);
        return OklchToRgb(l, clampedC, h);
    }

    // ── OKLCH ↔ sRGB 转换 ──────────────────────────────────────────

    /// <summary>将 M3 Tone (0-100) 转换为 Oklab 明度 L（非线性感知映射）。</summary>
    private static double ToneToOklabL(int tone)
    {
        // Oklab L≈0 为黑，L≈1 为白；与感知明度 Tone 的关系近似线性
        // 使用精确映射：Tone=0→L=0, Tone=100→L=1，中间段与感知一致
        return tone / 100.0;
    }

    /// <summary>sRGB → OKLCH（L:明度, C:彩度, H:色相°）</summary>
    private static void RgbToOklch(Color color, out double L, out double C, out double H)
    {
        // Step 1: sRGB → Linear sRGB
        double r = SrgbToLinear(color.R / 255.0);
        double g = SrgbToLinear(color.G / 255.0);
        double b = SrgbToLinear(color.B / 255.0);

        // Step 2: Linear sRGB → OKLab
        LinearToOklab(r, g, b, out var labL, out var a, out var labB);

        // Step 3: OKLab → OKLCH
        L = labL;
        C = Math.Sqrt(a * a + labB * labB);
        H = Math.Atan2(labB, a) * (180.0 / Math.PI);
        if (H < 0) H += 360.0;
    }

    /// <summary>OKLCH → sRGB Color（自动钳制到 [0,255]）</summary>
    private static Color OklchToRgb(double L, double C, double H)
    {
        // Step 1: OKLCH → OKLab
        double hRad = H * (Math.PI / 180.0);
        double a    = C * Math.Cos(hRad);
        double b    = C * Math.Sin(hRad);

        // Step 2: OKLab → Linear sRGB
        OklabToLinear(L, a, b, out var r, out var g, out var bl);

        // Step 3: Linear sRGB → sRGB（含钳制）
        return Color.FromRgb(
            (byte)Math.Round(Math.Clamp(LinearToSrgb(r), 0.0, 1.0) * 255),
            (byte)Math.Round(Math.Clamp(LinearToSrgb(g), 0.0, 1.0) * 255),
            (byte)Math.Round(Math.Clamp(LinearToSrgb(bl), 0.0, 1.0) * 255));
    }

    /// <summary>在给定 (L, H) 下，用二分法找出最大彩度 C 使颜色仍在 sRGB 色域内。</summary>
    private static double ClampChromaToGamut(double L, double requestedC, double H)
    {
        if (requestedC <= 0) return 0;
        if (IsInGamut(L, requestedC, H)) return requestedC;

        // 二分查找最大可用彩度
        double lo = 0, hi = requestedC;
        for (int i = 0; i < 20; i++)
        {
            double mid = (lo + hi) / 2.0;
            if (IsInGamut(L, mid, H))
                lo = mid;
            else
                hi = mid;
        }
        return lo;
    }

    private static bool IsInGamut(double L, double C, double H)
    {
        double hRad = H * (Math.PI / 180.0);
        double a    = C * Math.Cos(hRad);
        double b    = C * Math.Sin(hRad);
        OklabToLinear(L, a, b, out var r, out var g, out var bl);
        const double eps = 1e-6;
        return r >= -eps && r <= 1 + eps &&
               g >= -eps && g <= 1 + eps &&
               bl >= -eps && bl <= 1 + eps;
    }

    // ── 低级色彩空间转换 ──────────────────────────────────────────

    private static double SrgbToLinear(double c)
        => c <= 0.04045 ? c / 12.92 : Math.Pow((c + 0.055) / 1.055, 2.4);

    private static double LinearToSrgb(double c)
        => c <= 0.0031308 ? 12.92 * c : 1.055 * Math.Pow(c, 1.0 / 2.4) - 0.055;

    private static void LinearToOklab(double r, double g, double b,
        out double L, out double a, out double labB)
    {
        // Linear sRGB → LMS (Oklab M1)
        double l = 0.4122214708 * r + 0.5363325363 * g + 0.0514459929 * b;
        double m = 0.2119034982 * r + 0.6806995451 * g + 0.1073969566 * b;
        double s = 0.0883024619 * r + 0.2817188376 * g + 0.6299787005 * b;

        // LMS → LMS' (cube root)
        double l_ = Math.Cbrt(l);
        double m_ = Math.Cbrt(m);
        double s_ = Math.Cbrt(s);

        // LMS' → OKLab (M2)
        L    = 0.2104542553 * l_ + 0.7936177850 * m_ - 0.0040720468 * s_;
        a    = 1.9779984951 * l_ - 2.4285922050 * m_ + 0.4505937099 * s_;
        labB = 0.0259040371 * l_ + 0.7827717662 * m_ - 0.8086757660 * s_;
    }

    private static void OklabToLinear(double L, double a, double b,
        out double r, out double g, out double bl)
    {
        // OKLab → LMS' (M2 inverse)
        double l_ = L + 0.3963377774 * a + 0.2158037573 * b;
        double m_ = L - 0.1055613458 * a - 0.0638541728 * b;
        double s_ = L - 0.0894841775 * a - 1.2914855480 * b;

        // LMS' → LMS (cube)
        double lv = l_ * l_ * l_;
        double mv = m_ * m_ * m_;
        double sv = s_ * s_ * s_;

        // LMS → Linear sRGB (M1 inverse)
        r  =  4.0767416621 * lv - 3.3077115913 * mv + 0.2309699292 * sv;
        g  = -1.2684380046 * lv + 2.6097574011 * mv - 0.3413193965 * sv;
        bl = -0.0041960863 * lv - 0.7034186147 * mv + 1.7076147010 * sv;
    }

    /// <summary>插值获取非标准色调的颜色。</summary>
    private static Color GetTone(Dictionary<int, Color> palette, int tone)
    {
        if (palette.TryGetValue(tone, out var exact)) return exact;
        var lo = Tones.Where(t => t <= tone).DefaultIfEmpty(0).Max();
        var hi = Tones.Where(t => t >= tone).DefaultIfEmpty(100).Min();
        if (lo == hi) return palette[lo];
        var t = (double)(tone - lo) / (hi - lo);
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
        var lo = d.Keys.Where(k => k <= tone).DefaultIfEmpty(0).Max();
        var hi = d.Keys.Where(k => k >= tone).DefaultIfEmpty(100).Min();
        if (lo == hi) return d[lo];
        var t = (double)(tone - lo) / (hi - lo);
        var a = d[lo]; var b = d[hi];
        return Color.FromRgb(
            (byte)(a.R + t * (b.R - a.R)),
            (byte)(a.G + t * (b.G - a.G)),
            (byte)(a.B + t * (b.B - a.B)));
    }
}
