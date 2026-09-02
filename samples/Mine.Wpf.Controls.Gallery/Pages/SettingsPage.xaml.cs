using System.Windows;
using System.Windows.Media;
using Mine.Wpf.Controls.Controls;

namespace Mine.Wpf.Controls.Gallery.Pages;

public partial class SettingsPage : Page
{
    public SettingsPage()
    {
        InitializeComponent();
    }

    private void OnSettingsCardButtonClick(object sender, RoutedEventArgs e)
    {
        // 演示用：实际项目中可在此导航到详情页
    }

    private void OnAccentColorItemClick(object sender, RoutedEventArgs e)
    {
        // 演示用：可在此弹出 ColorPicker Flyout 供用户选择新颜色
        var colors = new[] { Colors.Purple, Colors.Teal, Colors.Orange, Colors.Green };
        var current = AccentColorItem.SelectedColor;
        var index = Array.IndexOf(colors, current);
        AccentColorItem.SelectedColor = colors[(index + 1) % colors.Length];
    }
}
