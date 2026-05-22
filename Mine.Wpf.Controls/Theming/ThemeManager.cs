using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Animation;
using Microsoft.Win32;
namespace Mine.Wpf.Controls.Theming;
/// <summary>
/// 主题管理器——运行时主题切换的核心 API。
/// 调用 <see cref="ApplyTheme"/> 来切换亮/暗模式和种子颜色。
/// </summary>
public static class ThemeManager
{
    // ── 状态 ──────────────────────────────────────────────────────
    private static ThemeMode    _mode      = ThemeMode.System;
    private static Color        _seedColor = Color.FromRgb(0x67, 0x50, 0xA4);
    private static ColorScheme? _current;
    private static bool         _followSystemAccent;
    public static ThemeMode   Mode      => _mode;
    public static Color       SeedColor => _seedColor;
    public static ColorScheme Current   => _current ??= BuildScheme();
    public static event EventHandler<ThemeChangedEventArgs>? ThemeChanged;
    // ── 公共 API ──────────────────────────────────────────────────
    /// <summary>立即应用新主题（带平滑颜色过渡动画）。</summary>
    public static void ApplyTheme(ThemeMode mode, Color? seedColor = null)
    {
        _mode      = mode;
        _seedColor = seedColor ?? _seedColor;
        _current   = null;
        var scheme = Current;
        WriteToResources(scheme, animated: true);
        ThemeChanged?.Invoke(null, new ThemeChangedEventArgs(scheme, IsEffectiveDark()));
    }
    /// <summary>在亮色与暗色之间切换（基于当前实际视觉状态）。</summary>
    public static void ToggleLightDark()
        => ApplyTheme(IsEffectiveDark() ? ThemeMode.Light : ThemeMode.Dark, _seedColor);
    /// <summary>跟随 Windows 主题色作为种子颜色。</summary>
    public static void UseSystemAccent(bool follow = true)
    {
        _followSystemAccent = follow;
        if (follow) UpdateFromSystemAccent();
    }
    /// <summary>初始化——从 App.xaml.cs 或 ThemeDictionary 调用一次即可。</summary>
    internal static void Initialize()
    {
        SystemEvents.UserPreferenceChanged += OnUserPreferenceChanged;
        ApplyTheme(_mode, _seedColor);
    }
    // ── 内部方法 ──────────────────────────────────────────────────
    internal static bool IsEffectiveDark()
    {
        if (_mode == ThemeMode.Dark)  return true;
        if (_mode == ThemeMode.Light) return false;
        return IsSystemDark();
    }
    private static bool IsSystemDark()
    {
        try
        {
            using var key = Registry.CurrentUser.OpenSubKey(
                @"Software\Microsoft\Windows\CurrentVersion\Themes\Personalize");
            var val = key?.GetValue("AppsUseLightTheme");
            return val is int i && i == 0;
        }
        catch { return false; }
    }
    private static Color GetSystemAccentColor()
    {
        try
        {
            using var key = Registry.CurrentUser.OpenSubKey(@"Software\Microsoft\Windows\DWM");
            var raw = key?.GetValue("AccentColor");
            if (raw is int abgr)
            {
                byte r = (byte)(abgr & 0xFF);
                byte g = (byte)((abgr >>  8) & 0xFF);
                byte b = (byte)((abgr >> 16) & 0xFF);
                return Color.FromRgb(r, g, b);
            }
        }
        catch { /* 忽略注册表读取异常 */ }
        return Color.FromRgb(0x67, 0x50, 0xA4);
    }
    private static void UpdateFromSystemAccent()
    {
        _seedColor = GetSystemAccentColor();
        _current   = null;
        WriteToResources(Current, animated: true);
    }
    private static void OnUserPreferenceChanged(object sender, UserPreferenceChangedEventArgs e)
    {
        if (e.Category == UserPreferenceCategory.General)
        {
            if (_followSystemAccent) UpdateFromSystemAccent();
            if (_mode == ThemeMode.System)
            {
                _current = null;
                var scheme = Current;
                Application.Current?.Dispatcher.BeginInvoke(() =>
                {
                    WriteToResources(scheme, animated: true);
                    ThemeChanged?.Invoke(null, new ThemeChangedEventArgs(scheme, IsEffectiveDark()));
                });
            }
        }
    }
    private static ColorScheme BuildScheme()
    {
        bool dark = IsEffectiveDark();
        return dark
            ? TonalPaletteGenerator.BuildDark(_seedColor)
            : TonalPaletteGenerator.BuildLight(_seedColor);
    }
    // ── 资源写入 ──────────────────────────────────────────────────
    /// <summary>
    /// 将所有颜色角色写入 Application.Current.Resources。
    /// SolidColorBrush 对象在原处更新（避免重新布局开销）。
    /// </summary>
    private static void WriteToResources(ColorScheme scheme, bool animated)
    {
        var app = Application.Current;
        if (app == null) return;
        app.Dispatcher.BeginInvoke(() =>
        {
            var res = app.Resources;
            WriteRole(res, "Primary",             scheme.Primary,             animated);
            WriteRole(res, "OnPrimary",           scheme.OnPrimary,           animated);
            WriteRole(res, "PrimaryContainer",    scheme.PrimaryContainer,    animated);
            WriteRole(res, "OnPrimaryContainer",  scheme.OnPrimaryContainer,  animated);
            WriteRole(res, "Secondary",            scheme.Secondary,            animated);
            WriteRole(res, "OnSecondary",          scheme.OnSecondary,          animated);
            WriteRole(res, "SecondaryContainer",   scheme.SecondaryContainer,   animated);
            WriteRole(res, "OnSecondaryContainer", scheme.OnSecondaryContainer, animated);
            WriteRole(res, "Tertiary",            scheme.Tertiary,            animated);
            WriteRole(res, "OnTertiary",          scheme.OnTertiary,          animated);
            WriteRole(res, "TertiaryContainer",   scheme.TertiaryContainer,   animated);
            WriteRole(res, "OnTertiaryContainer", scheme.OnTertiaryContainer, animated);
            WriteRole(res, "Error",            scheme.Error,            animated);
            WriteRole(res, "OnError",          scheme.OnError,          animated);
            WriteRole(res, "ErrorContainer",   scheme.ErrorContainer,   animated);
            WriteRole(res, "OnErrorContainer", scheme.OnErrorContainer, animated);
            WriteRole(res, "Surface",                 scheme.Surface,                 animated);
            WriteRole(res, "OnSurface",               scheme.OnSurface,               animated);
            WriteRole(res, "SurfaceVariant",          scheme.SurfaceVariant,          animated);
            WriteRole(res, "OnSurfaceVariant",        scheme.OnSurfaceVariant,        animated);
            WriteRole(res, "SurfaceTint",             scheme.SurfaceTint,             animated);
            WriteRole(res, "SurfaceContainerLowest",  scheme.SurfaceContainerLowest,  animated);
            WriteRole(res, "SurfaceContainerLow",     scheme.SurfaceContainerLow,     animated);
            WriteRole(res, "SurfaceContainer",        scheme.SurfaceContainer,        animated);
            WriteRole(res, "SurfaceContainerHigh",    scheme.SurfaceContainerHigh,    animated);
            WriteRole(res, "SurfaceContainerHighest", scheme.SurfaceContainerHighest, animated);
            WriteRole(res, "Background",   scheme.Background,   animated);
            WriteRole(res, "OnBackground", scheme.OnBackground, animated);
            WriteRole(res, "Outline",        scheme.Outline,        animated);
            WriteRole(res, "OutlineVariant", scheme.OutlineVariant, animated);
            WriteRole(res, "InverseSurface",   scheme.InverseSurface,   animated);
            WriteRole(res, "InverseOnSurface", scheme.InverseOnSurface, animated);
            WriteRole(res, "InversePrimary",   scheme.InversePrimary,   animated);
            WriteRole(res, "Shadow", scheme.Shadow, false);
            WriteRole(res, "Scrim",  scheme.Scrim,  false);
            WriteRole(res, "Warning",            scheme.Warning,            animated);
            WriteRole(res, "OnWarning",          scheme.OnWarning,          animated);
            WriteRole(res, "WarningContainer",   scheme.WarningContainer,   animated);
            WriteRole(res, "OnWarningContainer", scheme.OnWarningContainer, animated);
            WriteRole(res, "Success",            scheme.Success,            animated);
            WriteRole(res, "OnSuccess",          scheme.OnSuccess,          animated);
            WriteRole(res, "SuccessContainer",   scheme.SuccessContainer,   animated);
            WriteRole(res, "OnSuccessContainer", scheme.OnSuccessContainer, animated);
            WriteRole(res, "Info",               scheme.Info,               animated);
            WriteRole(res, "OnInfo",             scheme.OnInfo,             animated);
            WriteRole(res, "InfoContainer",      scheme.InfoContainer,      animated);
            WriteRole(res, "OnInfoContainer",    scheme.OnInfoContainer,    animated);
        });
    }
    private static void WriteRole(ResourceDictionary res, string role, Color color, bool animated)
    {
        string colorKey = $"Mine.Color.{role}";
        string brushKey = $"Mine.Brush.{role}";
        // 更新 Color 资源
        if (res.Contains(colorKey))
        {
            if (animated && res[colorKey] is Color oldColor && oldColor != color)
                AnimateBrushColor(res, brushKey, oldColor, color);
            res[colorKey] = color;
        }
        else
        {
            res[colorKey] = color;
        }
        // 更新 Brush 资源（原处变更，避免触发重新布局）
        if (res[brushKey] is SolidColorBrush existing)
        {
            if (existing.IsFrozen)
            {
                res[brushKey] = new SolidColorBrush(color);
            }
            else if (animated)
            {
                var anim = new ColorAnimation(color, new Duration(TimeSpan.FromMilliseconds(200)))
                {
                    EasingFunction = new CubicEase { EasingMode = EasingMode.EaseInOut }
                };
                existing.BeginAnimation(SolidColorBrush.ColorProperty, anim);
            }
            else
            {
                existing.Color = color;
            }
        }
        else
        {
            res[brushKey] = new SolidColorBrush(color);
        }
    }
    private static void AnimateBrushColor(ResourceDictionary res, string key, Color from, Color to)
    {
        if (res[key] is not SolidColorBrush brush || brush.IsFrozen) return;
        var anim = new ColorAnimation(from, to, new Duration(TimeSpan.FromMilliseconds(200)))
        {
            EasingFunction = new CubicEase { EasingMode = EasingMode.EaseInOut }
        };
        brush.BeginAnimation(SolidColorBrush.ColorProperty, anim);
    }
}
public sealed class ThemeChangedEventArgs(ColorScheme scheme, bool isDark) : EventArgs
{
    public ColorScheme Scheme { get; } = scheme;
    public bool        IsDark  { get; } = isDark;
}
