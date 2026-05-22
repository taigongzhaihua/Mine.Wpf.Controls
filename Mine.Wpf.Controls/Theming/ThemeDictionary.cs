using System.Windows;
using System.Windows.Media;
namespace Mine.Wpf.Controls.Theming;
/// <summary>
/// ResourceDictionary 子类，用于在 App.xaml 中引导 Mine 主题系统。
/// 在 App.xaml 的 MergedDictionaries 中添加：
/// <code>
///   &lt;mine:ThemeDictionary Mode="System" SeedColor="#6750A4"/&gt;
/// </code>
/// </summary>
public sealed class ThemeDictionary : ResourceDictionary
{
    private ThemeMode _mode      = ThemeMode.System;
    private Color     _seedColor = Color.FromRgb(0x67, 0x50, 0xA4);
    private bool      _initialized;
    public ThemeMode Mode
    {
        get => _mode;
        set { _mode = value; if (_initialized) Apply(); }
    }
    public Color SeedColor
    {
        get => _seedColor;
        set { _seedColor = value; if (_initialized) Apply(); }
    }
    public ThemeDictionary()
    {
        // 合并令牌字典与转换器字典。
        // 转换器必须位于 App.Resources，以使控件模板内的 StaticResource 在运行时能找到它们。
        MergedDictionaries.Add(Load("Themes/Resources/Colors.Light.xaml"));
        MergedDictionaries.Add(Load("Themes/Resources/Typography.xaml"));
        MergedDictionaries.Add(Load("Themes/Resources/Shape.xaml"));
        MergedDictionaries.Add(Load("Themes/Resources/Elevation.xaml"));
        MergedDictionaries.Add(Load("Themes/Resources/Motion.xaml"));
        MergedDictionaries.Add(Load("Themes/Resources/Converters.xaml"));
        // 延迟初始化，确保属性设置后再应用主题
        if (Application.Current != null)
            Application.Current.Dispatcher.BeginInvoke(Initialize);
    }
    private void Initialize()
    {
        _initialized = true;
        Apply();
        ThemeManager.Initialize();
    }
    private void Apply() => ThemeManager.ApplyTheme(_mode, _seedColor);
    private static ResourceDictionary Load(string relPath)
    {
        var uri = new Uri($"pack://application:,,,/Mine.Wpf.Controls;component/{relPath}",
                          UriKind.Absolute);
        return new ResourceDictionary { Source = uri };
    }
}