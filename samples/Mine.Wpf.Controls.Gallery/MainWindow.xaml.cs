using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using Mine.Wpf.Controls.Controls;
using Mine.Wpf.Controls.Gallery.Pages;
using Mine.Wpf.Controls.Theming;

namespace Mine.Wpf.Controls.Gallery;

public partial class MainWindow
{
    private static readonly Color DefaultSeedColor = Color.FromRgb(0x67, 0x50, 0xA4);
    private bool _updatingThemeUi;

    // 导航项定义
    private static readonly (string Icon, string Label, Type Page)[] NavItems =
    [
        ("\uE8A0", "Frame / Page",        typeof(FramePage)),
        ("\uE762", "SplitView",            typeof(SplitViewPage)),
        ("\uE762", "NavigationView",       typeof(NavigationViewPage)),
        ("\uF1C1", "Button",               typeof(ButtonPage)),
        ("\uE399", "Badge",                typeof(BadgePage)),
        ("\uE7FD", "Avatar",               typeof(AvatarPage)),
        ("\uE8FE", "Segmented",            typeof(SegmentedPage)),
        ("\uF73C", "Stepper",              typeof(StepperPage)),
        ("\uE262", "Text Field",           typeof(TextFieldPage)),
        ("\uE8B6", "Search Box",           typeof(SearchBoxPage)),
        ("\uE834", "Selection",            typeof(SelectionPage)),
        ("\uE5C6", "ComboBox",             typeof(ComboBoxPage)),
        ("\uEF4F", "Chip",                 typeof(ChipPage)),
        ("\uE5D2", "Drawer",               typeof(DrawerPage)),
        ("\uE262", "Inputs",               typeof(InputsPage)),
        ("\uE916", "DatePicker",           typeof(DatePickerPage)),
        ("\uE192", "TimePicker",           typeof(TimePickerPage)),
        ("\uE8D2", "Tab / List",           typeof(TabPage)),
        ("\uE85A", "Snackbar / Dialog",    typeof(FeedbackPage)),
        ("\uEA22", "Toast / Notification", typeof(ToastPage)),
        ("\uE9D0", "Progress",             typeof(ProgressPage)),
        ("\uE8D2", "ToolTip",              typeof(TooltipPage)),
        ("\uE8CB", "Flyout",               typeof(FlyoutPage)),
        ("\uE8E9", "Card",                 typeof(CardPage)),
        ("\uE245", "Typography",           typeof(TypographyPage)),
        ("\uE40A", "Color",                typeof(ColorPage)),
        ("\uE9B0", "Icons",                typeof(IconPage)),
    ];

    public MainWindow()
    {
        InitializeComponent();

        // 生成导航项
        MainNav.MenuItemsSource = NavItems.Select(n =>
            new NavigationViewItem { Icon = n.Icon, Content = n.Label, Tag = n.Page }).ToArray();

        // 默认选中第一项
        Loaded += (_, _) =>
        {
            if (MainNav.MenuItemsSource is NavigationViewItem[] items && items.Length > 0)
                MainNav.SelectedItem = items[0];

            // 启动时同步 Switch 状态（ThemeManager.Initialize 可能已跟随系统切换）
            SyncDarkModeSwitch();
            SyncThemePanel();
        };

        // 主题变化时同步 Switch 状态
        ThemeManager.ThemeChanged += (_, _) => Dispatcher.BeginInvoke(() =>
        {
            SyncDarkModeSwitch();
            SyncThemePanel();
        });
    }

    private void SyncDarkModeSwitch()
    {
        DarkModeSwitch.Checked   -= OnToggleTheme;
        DarkModeSwitch.Unchecked -= OnToggleTheme;
        DarkModeSwitch.IsChecked  = ThemeManager.IsEffectiveDark();
        DarkModeSwitch.Checked   += OnToggleTheme;
        DarkModeSwitch.Unchecked += OnToggleTheme;
    }

    private void SyncThemePanel()
    {
        _updatingThemeUi = true;
        try
        {
            var isPreset = ThemeManager.Preset != ThemePreset.None;
            ThemeModeSelector.SelectedIndex = isPreset ? 0 : 1;

            PresetThemePanel.Visibility = isPreset ? Visibility.Visible : Visibility.Collapsed;
            CustomThemePanel.Visibility = isPreset ? Visibility.Collapsed : Visibility.Visible;

            CurrentThemeModeText.Text = isPreset ? "预设主题" : "自定义主题";
            CurrentThemeValueText.Text = isPreset
                ? ThemePresetCatalog.GetDisplayName(ThemeManager.Preset)
                : $"种子色 #{ThemeManager.SeedColor.R:X2}{ThemeManager.SeedColor.G:X2}{ThemeManager.SeedColor.B:X2}";

            CurrentThemeSwatch.Background = new SolidColorBrush(
                isPreset ? ThemePresetCatalog.GetSeedColor(ThemeManager.Preset) : ThemeManager.SeedColor);

            ThemeColorPicker.SelectedColor = ThemeManager.SeedColor;
        }
        finally
        {
            _updatingThemeUi = false;
        }
    }

    private void MainNav_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (e.AddedItems.Count == 0) return;
        if (e.AddedItems[0] is not NavigationViewItem { Tag: Type pageType }) return;

        if (Activator.CreateInstance(pageType) is not { } page) return;
        MainFrame.Navigate(page, PageTransition.FadeThrough);

        // Overlay / Minimal 模式选中后自动关闭 Pane（NavigationView 内部已处理，此处备用）
    }

    private void MainFrame_Navigated(object sender, PageNavigatedEventArgs e)
    {
        if (e.Page is Controls.Page page)
            MainNav.Header = page.Title;
        else if (MainNav.SelectedItem is NavigationViewItem item)
            MainNav.Header = item.Content?.ToString();
    }

    private void OnToggleTheme(object sender, RoutedEventArgs e)
    {
        ThemeManager.ToggleLightDark();
        // 同步 Switch 状态（避免递归：暂时解绑再绑）
        DarkModeSwitch.Checked   -= OnToggleTheme;
        DarkModeSwitch.Unchecked -= OnToggleTheme;
        DarkModeSwitch.IsChecked  = ThemeManager.Mode == ThemeMode.Dark;
        DarkModeSwitch.Checked   += OnToggleTheme;
        DarkModeSwitch.Unchecked += OnToggleTheme;
    }

    private void OnSeedColor(object sender, RoutedEventArgs e)
    {
        if (sender is not FrameworkElement { Tag: string hex }) return;
        var c = (Color)ColorConverter.ConvertFromString(hex);
        ApplyCustomSeed(c);
    }

    private void OnPresetTheme(object sender, RoutedEventArgs e)
    {
        if (sender is not FrameworkElement { Tag: string presetText }) return;
        if (!Enum.TryParse<ThemePreset>(presetText, ignoreCase: true, out var preset)) return;

        ThemeManager.ApplyPresetTheme(ThemeManager.Mode, preset);
        SyncThemePanel();
    }

    private void ThemeModeSelector_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (_updatingThemeUi || ThemeModeSelector.SelectedIndex < 0) return;

        if (ThemeModeSelector.SelectedIndex == 0)
        {
            var preset = ThemeManager.Preset != ThemePreset.None ? ThemeManager.Preset : ThemePreset.Purple;
            ThemeManager.ApplyPresetTheme(ThemeManager.Mode, preset);
        }
        else
        {
            ThemeManager.ApplyTheme(ThemeManager.Mode, ThemeManager.SeedColor);
        }

        SyncThemePanel();
    }

    private void ThemeColorPicker_SelectedColorChanged(object sender, RoutedPropertyChangedEventArgs<Color> e)
    {
        if (_updatingThemeUi) return;
        ApplyCustomSeed(e.NewValue);
    }

    private void OnUseSystemAccentSeed(object sender, RoutedEventArgs e)
    {
        ThemeManager.UseSystemAccent();
        SyncThemePanel();
    }

    private void OnUseDefaultSeed(object sender, RoutedEventArgs e)
    {
        ApplyCustomSeed(DefaultSeedColor);
    }

    private void ApplyCustomSeed(Color seed)
    {
        ThemeManager.UseSystemAccent(false);
        ThemeManager.ApplyTheme(ThemeManager.Mode, seed);
        SyncThemePanel();
    }
}

public class NavItem
{
    public string Label    { get; set; } = "";
    public string Icon     { get; set; } = "";
    public Type   PageType { get; set; } = typeof(object);
}
