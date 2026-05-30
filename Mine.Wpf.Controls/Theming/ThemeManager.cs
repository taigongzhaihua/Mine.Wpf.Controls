using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Animation;
using Microsoft.Win32;
namespace Mine.Wpf.Controls.Theming;
/// <summary>
/// 主题管理器——运行时主题切换的核心 API。
/// </summary>
public static class ThemeManager
{
    // ── 颜色角色列表 ──────────────────────────────────────────────
    private static readonly string[] ColorRoles =
    [
        "Primary", "OnPrimary", "PrimaryContainer", "OnPrimaryContainer",
        "Secondary", "OnSecondary", "SecondaryContainer", "OnSecondaryContainer",
        "Tertiary", "OnTertiary", "TertiaryContainer", "OnTertiaryContainer",
        "Error", "OnError", "ErrorContainer", "OnErrorContainer",
        "Surface", "OnSurface", "SurfaceVariant", "OnSurfaceVariant", "SurfaceTint",
        "SurfaceContainerLowest", "SurfaceContainerLow", "SurfaceContainer",
        "SurfaceContainerHigh", "SurfaceContainerHighest",
        "Background", "OnBackground",
        "Outline", "OutlineVariant",
        "InverseSurface", "InverseOnSurface", "InversePrimary",
        "Shadow", "Scrim",
        "Warning", "OnWarning", "WarningContainer", "OnWarningContainer",
        "Success", "OnSuccess", "SuccessContainer", "OnSuccessContainer",
        "Info", "OnInfo", "InfoContainer", "OnInfoContainer",
    ];

    // ── 托管画笔（惰性创建：首次用户触发主题切换时才初始化）────────
    // 写入 Application.Resources["Mine.Brush.*"]，优先级高于合并字典。
    // 后续切换只需对画笔 Color 属性做 ColorAnimation，实现平滑过渡。
    private static readonly Dictionary<string, SolidColorBrush> _brushes = new(48);

    // 写入 Application.Resources 的 Mine.Color.* key，切换到预设时需清除
    private static readonly HashSet<string> _colorOverrideKeys = new(48);

    // ── 状态 ──────────────────────────────────────────────────────
    private static ThemeMode    _mode      = ThemeMode.System;
    private static Color        _seedColor = Color.FromRgb(0x67, 0x50, 0xA4);
    private static ThemePreset  _preset    = ThemePreset.None;
    private static ColorScheme? _current;
    private static bool         _followSystemAccent;
    private static bool         _initialized;

    private static WeakReference<ThemeDictionary>? _themeDictRef;
    private static readonly Color DefaultSeedColor = Color.FromRgb(0x67, 0x50, 0xA4);

    public static ThemeMode    Mode      => _mode;
    public static Color        SeedColor => _seedColor;
    public static ThemePreset  Preset    => _preset;
    public static ColorScheme? Current   => _preset == ThemePreset.None ? (_current ??= BuildScheme()) : null;
    public static event EventHandler<ThemeChangedEventArgs>? ThemeChanged;

    // ── 注册 ──────────────────────────────────────────────────────
    internal static void Register(ThemeDictionary dict)
        => _themeDictRef = new WeakReference<ThemeDictionary>(dict);

    // ── 公共 API ──────────────────────────────────────────────────
    public static void ApplyTheme(ThemeMode mode, Color? seedColor = null)
    {
        _mode      = mode;
        _seedColor = seedColor ?? _seedColor;
        _preset    = ThemePreset.None;
        _current   = null;
        WriteToResources(animate: _initialized);
        ThemeChanged?.Invoke(null, new ThemeChangedEventArgs(Current, IsEffectiveDark()));
    }

    public static void ApplyPresetTheme(ThemeMode mode, ThemePreset preset)
    {
        _mode    = mode;
        _preset  = preset;
        _current = null;
        WriteToResources(animate: _initialized);
        ThemeChanged?.Invoke(null, new ThemeChangedEventArgs(null, IsEffectiveDark()));
    }

    public static void ToggleLightDark()
    {
        var newMode = IsEffectiveDark() ? ThemeMode.Light : ThemeMode.Dark;
        if (_preset != ThemePreset.None)
            ApplyPresetTheme(newMode, _preset);
        else
            ApplyTheme(newMode, _seedColor);
    }

    public static void UseSystemAccent(bool follow = true)
    {
        _followSystemAccent = follow;
        if (follow) UpdateFromSystemAccent();
    }

    internal static void Initialize()
    {
        if (_initialized) return;
        _initialized = true;
        SystemEvents.UserPreferenceChanged += OnUserPreferenceChanged;
        // 初始化阶段：只切字典 + 写颜色，不做动画，不创建托管画笔
        WriteToResources(animate: false);
        ThemeChanged?.Invoke(null, new ThemeChangedEventArgs(Current, IsEffectiveDark()));
    }

    // ── 内部方法 ──────────────────────────────────────────────────
    public static bool IsEffectiveDark() => _mode switch
    {
        ThemeMode.Dark  => true,
        ThemeMode.Light => false,
        _               => IsSystemDark()
    };

    private static bool IsSystemDark()
    {
        try
        {
            using var key = Registry.CurrentUser.OpenSubKey(
                @"Software\Microsoft\Windows\CurrentVersion\Themes\Personalize");
            return key?.GetValue("AppsUseLightTheme") is int and 0;
        }
        catch { return false; }
    }

    private static Color GetSystemAccentColor()
    {
        try
        {
            using var key = Registry.CurrentUser.OpenSubKey(@"Software\Microsoft\Windows\DWM");
            if (key?.GetValue("AccentColor") is int abgr)
                return Color.FromRgb((byte)(abgr & 0xFF), (byte)((abgr >> 8) & 0xFF), (byte)((abgr >> 16) & 0xFF));
        }
        catch { }
        return DefaultSeedColor;
    }

    private static void UpdateFromSystemAccent()
    {
        _seedColor = GetSystemAccentColor();
        _preset    = ThemePreset.None;
        _current   = null;
        WriteToResources(animate: _initialized);
    }

    private static void OnUserPreferenceChanged(object sender, UserPreferenceChangedEventArgs e)
    {
        if (e.Category != UserPreferenceCategory.General) return;
        if (_followSystemAccent) UpdateFromSystemAccent();
        if (_mode == ThemeMode.System)
        {
            _current = null;
            Application.Current?.Dispatcher.BeginInvoke(() =>
            {
                WriteToResources(animate: true);
                ThemeChanged?.Invoke(null, new ThemeChangedEventArgs(Current, IsEffectiveDark()));
            });
        }
    }

    private static ColorScheme BuildScheme()
        => IsEffectiveDark()
            ? TonalPaletteGenerator.BuildDark(_seedColor)
            : TonalPaletteGenerator.BuildLight(_seedColor);

    // ── 资源写入 ──────────────────────────────────────────────────
    /// <summary>
    /// 主题切换核心：
    /// 1. 切换颜色字典；
    /// 2. 自定义种子色时写入 Mine.Color.* 覆盖值；
    /// 3. 动画模式下对 ThemeDictionary.AnimatedBrushes 中的托管画笔执行 ColorAnimation。
    ///    画笔存放于 ThemeDictionary.AnimatedBrushes（独立 ResourceDictionary），
    ///    避免 Application.Resources 被 WPF 密封后画笔遭到冻结导致 BeginAnimation 崩溃。
    /// </summary>
    private static void WriteToResources(bool animate)
    {
        var app = Application.Current;
        if (app == null) return;

        void Write()
        {
            var dark = IsEffectiveDark();
            var res  = app.Resources;

            // ── 1. 切换颜色字典 ──────────────────────────────────────
            ThemeDictionary? themeDict = null;
            _themeDictRef?.TryGetTarget(out themeDict);
            themeDict?.SwapColorDictionary(_preset, dark);

            // ── 2. 处理 Color 覆盖值 ─────────────────────────────────
            ColorScheme? scheme = null;
            if (_preset == ThemePreset.None && _seedColor != DefaultSeedColor)
            {
                scheme = _current ??= BuildScheme();
                foreach (var role in ColorRoles)
                {
                    var key = $"Mine.Color.{role}";
                    res[key] = SchemeColor(scheme, role);
                    _colorOverrideKeys.Add(key);
                }
            }
            else
            {
                foreach (var key in _colorOverrideKeys) res.Remove(key);
                _colorOverrideKeys.Clear();
            }

            // ── 3. 画笔动画 ───────────────────────────────────────────
            if (!animate || themeDict == null) return;

            var brushHost = themeDict.AnimatedBrushes;

            // 惰性初始化：首次动画调用时创建托管画笔，写入 AnimatedBrushes
            if (_brushes.Count == 0)
            {
                foreach (var role in ColorRoles)
                {
                    var color = GetTargetColor(res, scheme, role);
                    var brush = new SolidColorBrush(color);
                    _brushes[role] = brush;
                    brushHost[$"Mine.Brush.{role}"] = brush;
                }
                return; // 首次直接设置颜色，无动画
            }

            // 后续切换：ColorAnimation 平滑过渡
            var duration = new Duration(TimeSpan.FromMilliseconds(300));
            var easing   = new CubicEase { EasingMode = EasingMode.EaseOut };
            foreach (var role in ColorRoles)
            {
                var target = GetTargetColor(res, scheme, role);
                if (!_brushes.TryGetValue(role, out var brush)) continue;

                if (brush.IsFrozen)
                {
                    // 画笔意外被冻结时重新创建（防御性处理）
                    brush = new SolidColorBrush(target);
                    _brushes[role] = brush;
                    brushHost[$"Mine.Brush.{role}"] = brush;
                    continue;
                }

                var from = brush.Color; // 当前有效颜色（可能是上一次动画的值）
                var anim = new ColorAnimation(from, target, duration)
                {
                    EasingFunction = new CubicEase { EasingMode = EasingMode.EaseOut }
                };
                brush.BeginAnimation(SolidColorBrush.ColorProperty, anim);
            }
        }

        if (app.Dispatcher.CheckAccess())
            Write();
        else
            app.Dispatcher.BeginInvoke(Write);
    }

    /// <summary>获取指定角色的目标颜色：自定义种子用 scheme，其余从已切换字典读取。</summary>
    private static Color GetTargetColor(ResourceDictionary res, ColorScheme? scheme, string role)
    {
        if (scheme != null) return SchemeColor(scheme, role);
        var key = $"Mine.Color.{role}";
        try { return (Color)res[key]; }
        catch { return Colors.Transparent; }
    }

    private static Color SchemeColor(ColorScheme s, string role) => role switch
    {
        "Primary"               => s.Primary,
        "OnPrimary"             => s.OnPrimary,
        "PrimaryContainer"      => s.PrimaryContainer,
        "OnPrimaryContainer"    => s.OnPrimaryContainer,
        "Secondary"             => s.Secondary,
        "OnSecondary"           => s.OnSecondary,
        "SecondaryContainer"    => s.SecondaryContainer,
        "OnSecondaryContainer"  => s.OnSecondaryContainer,
        "Tertiary"              => s.Tertiary,
        "OnTertiary"            => s.OnTertiary,
        "TertiaryContainer"     => s.TertiaryContainer,
        "OnTertiaryContainer"   => s.OnTertiaryContainer,
        "Error"                 => s.Error,
        "OnError"               => s.OnError,
        "ErrorContainer"        => s.ErrorContainer,
        "OnErrorContainer"      => s.OnErrorContainer,
        "Surface"               => s.Surface,
        "OnSurface"             => s.OnSurface,
        "SurfaceVariant"        => s.SurfaceVariant,
        "OnSurfaceVariant"      => s.OnSurfaceVariant,
        "SurfaceTint"           => s.SurfaceTint,
        "SurfaceContainerLowest"  => s.SurfaceContainerLowest,
        "SurfaceContainerLow"     => s.SurfaceContainerLow,
        "SurfaceContainer"        => s.SurfaceContainer,
        "SurfaceContainerHigh"    => s.SurfaceContainerHigh,
        "SurfaceContainerHighest" => s.SurfaceContainerHighest,
        "Background"            => s.Background,
        "OnBackground"          => s.OnBackground,
        "Outline"               => s.Outline,
        "OutlineVariant"        => s.OutlineVariant,
        "InverseSurface"        => s.InverseSurface,
        "InverseOnSurface"      => s.InverseOnSurface,
        "InversePrimary"        => s.InversePrimary,
        "Shadow"                => s.Shadow,
        "Scrim"                 => s.Scrim,
        "Warning"               => s.Warning,
        "OnWarning"             => s.OnWarning,
        "WarningContainer"      => s.WarningContainer,
        "OnWarningContainer"    => s.OnWarningContainer,
        "Success"               => s.Success,
        "OnSuccess"             => s.OnSuccess,
        "SuccessContainer"      => s.SuccessContainer,
        "OnSuccessContainer"    => s.OnSuccessContainer,
        "Info"                  => s.Info,
        "OnInfo"                => s.OnInfo,
        "InfoContainer"         => s.InfoContainer,
        "OnInfoContainer"       => s.OnInfoContainer,
        _                       => Colors.Transparent,
    };
}

public sealed class ThemeChangedEventArgs(ColorScheme? scheme, bool isDark) : EventArgs
{
    public ColorScheme? Scheme { get; } = scheme;
    public bool         IsDark  { get; } = isDark;
}
