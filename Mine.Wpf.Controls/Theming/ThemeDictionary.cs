using System.Windows;
using System.Windows.Media;
namespace Mine.Wpf.Controls.Theming;
/// <summary>
/// ResourceDictionary 子类，用于在 App.xaml 中引导 Mine 主题系统。
/// 在 App.xaml 的 MergedDictionaries 中添加：
/// <code>
///   &lt;mine:ThemeDictionary Mode="System" SeedColor="#6750A4"/&gt;
///   &lt;mine:ThemeDictionary Mode="System" Preset="Teal"/&gt;
/// </code>
/// </summary>
public sealed class ThemeDictionary : ResourceDictionary
{
    private ThemeMode   _mode      = ThemeMode.System;
    private Color       _seedColor = Color.FromRgb(0x67, 0x50, 0xA4);
    private ThemePreset _preset    = ThemePreset.None;
    private bool        _initialized;

    // 颜色字典在 MergedDictionaries 中的固定索引（始终为第 0 项）
    private const int ColorDictIndex = 0;

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
    public ThemePreset Preset
    {
        get => _preset;
        set { _preset = value; if (_initialized) Apply(); }
    }

    /// <summary>
    /// 存放运行时托管的动画画笔（Mine.Brush.*）。
    /// 作为 MergedDictionaries 最后一项，查找优先级最高，覆盖 ColorBrushes.xaml 中的静态画笔。
    /// 使用独立字典而非写入 Application.Resources，避免 Application.Resources 被 WPF 密封后画笔被冻结。
    /// </summary>
    internal readonly ResourceDictionary AnimatedBrushes = new();

    public ThemeDictionary()
    {
        // Material Symbols 字体
        this["Mine.Font.MaterialSymbols"] = new System.Windows.Media.FontFamily(
            new Uri("pack://application:,,,/Mine.Wpf.Controls;component/Fonts/"),
            "./#Material Symbols Rounded");

        // 颜色字典（索引 0，运行时通过 SwapColorDictionary 切换 Light/Dark）
        MergedDictionaries.Add(Load("Themes/Resources/Colors.Light.xaml"));
        MergedDictionaries.Add(Load("Themes/Resources/Typography.xaml"));
        MergedDictionaries.Add(Load("Themes/Resources/Shape.xaml"));
        MergedDictionaries.Add(Load("Themes/Resources/Elevation.xaml"));
        MergedDictionaries.Add(Load("Themes/Resources/Motion.xaml"));
        MergedDictionaries.Add(Load("Themes/Resources/Converters.xaml"));
        // 控件样式（隐式样式需在 App.Resources 中才能覆盖系统控件）
        MergedDictionaries.Add(Load("Themes/Controls/Button.xaml"));
        MergedDictionaries.Add(Load("Themes/Controls/Card.xaml"));
        MergedDictionaries.Add(Load("Themes/Controls/Window.xaml"));
        MergedDictionaries.Add(Load("Themes/Controls/TextBox.xaml"));
        MergedDictionaries.Add(Load("Themes/Controls/CheckBox.xaml"));
        MergedDictionaries.Add(Load("Themes/Controls/RadioButton.xaml"));
        MergedDictionaries.Add(Load("Themes/Controls/Slider.xaml"));
        MergedDictionaries.Add(Load("Themes/Controls/Progress.xaml"));
        MergedDictionaries.Add(Load("Themes/Controls/ComboBox.xaml"));
        MergedDictionaries.Add(Load("Themes/Controls/ScrollBar.xaml"));
        MergedDictionaries.Add(Load("Themes/Controls/Frame.xaml"));
        MergedDictionaries.Add(Load("Themes/Controls/Page.xaml"));
        MergedDictionaries.Add(Load("Themes/Controls/ToolTip.xaml"));
        MergedDictionaries.Add(Load("Themes/Controls/TabControl.xaml"));
        MergedDictionaries.Add(Load("Themes/Controls/ListView.xaml"));
        MergedDictionaries.Add(Load("Themes/Controls/SearchBox.xaml"));
        MergedDictionaries.Add(Load("Themes/Controls/Snackbar.xaml"));
        MergedDictionaries.Add(Load("Themes/Controls/Dialog.xaml"));
        MergedDictionaries.Add(Load("Themes/Controls/Switch.xaml"));
        MergedDictionaries.Add(Load("Themes/Controls/Chip.xaml"));
        MergedDictionaries.Add(Load("Themes/Controls/Flyout.xaml"));
        MergedDictionaries.Add(Load("Themes/Controls/SplitView.xaml"));
        MergedDictionaries.Add(Load("Themes/Controls/NavigationView.xaml"));
        MergedDictionaries.Add(Load("Themes/Controls/Badge.xaml"));
        MergedDictionaries.Add(Load("Themes/Controls/Avatar.xaml"));
        MergedDictionaries.Add(Load("Themes/Controls/Drawer.xaml"));
        MergedDictionaries.Add(Load("Themes/Controls/SegmentedControl.xaml"));
        MergedDictionaries.Add(Load("Themes/Controls/NumericUpDown.xaml"));
        MergedDictionaries.Add(Load("Themes/Controls/Rating.xaml"));
        MergedDictionaries.Add(Load("Themes/Controls/RangeSlider.xaml"));
        MergedDictionaries.Add(Load("Themes/Controls/DatePicker.xaml"));
        MergedDictionaries.Add(Load("Themes/Controls/TimePicker.xaml"));
        MergedDictionaries.Add(Load("Themes/Controls/ColorPicker.xaml"));
        MergedDictionaries.Add(Load("Themes/Controls/Stepper.xaml"));
        MergedDictionaries.Add(Load("Themes/Controls/Toast.xaml"));

        // 动画画笔字典：最后添加，查找优先级最高，覆盖 ColorBrushes.xaml 中的静态画笔
        MergedDictionaries.Add(AnimatedBrushes);

        // 注册到 ThemeManager，用 Send（最高优先级）确保在第一帧渲染前完成主题初始化
        ThemeManager.Register(this);
        if (Application.Current != null)
            Application.Current.Dispatcher.BeginInvoke(Initialize, System.Windows.Threading.DispatcherPriority.Send);
    }

    /// <summary>切换颜色资源字典（Light/Dark/Preset）。由 ThemeManager 调用。</summary>
    internal void SwapColorDictionary(ThemePreset preset, bool dark)
    {
        string path;
        if (preset != ThemePreset.None)
        {
            var mode = dark ? "Dark" : "Light";
            path = $"Themes/Resources/Presets/{preset}.{mode}.xaml";
        }
        else
        {
            path = dark ? "Themes/Resources/Colors.Dark.xaml" : "Themes/Resources/Colors.Light.xaml";
        }
        MergedDictionaries[ColorDictIndex] = Load(path);
    }

    private void Initialize()
    {
        _initialized = true;
        Apply();
        ThemeManager.Initialize();
    }
    private void Apply()
    {
        if (_preset != ThemePreset.None)
            ThemeManager.ApplyPresetTheme(_mode, _preset);
        else
            ThemeManager.ApplyTheme(_mode, _seedColor);
    }
    private static ResourceDictionary Load(string relPath)
    {
        var uri = new Uri($"pack://application:,,,/Mine.Wpf.Controls;component/{relPath}",
                          UriKind.Absolute);
        return new ResourceDictionary { Source = uri };
    }
}
