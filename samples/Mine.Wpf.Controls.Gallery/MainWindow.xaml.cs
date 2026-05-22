using System.Reflection;
using System.Windows.Controls;

namespace Mine.Wpf.Controls.Gallery;

[System.Windows.TemplatePart(Name = "PART_ContentArea", Type = typeof(ContentControl))]
public partial class MainWindow : Mine.Wpf.Controls.Controls.Window
{
    public MainWindow()
    {
        InitializeComponent();
        NavList.SelectedIndex = 0;
    }

    private void NavList_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (NavList.SelectedItem is NavItem item)
        {
            ContentArea.Content = Activator.CreateInstance(item.PageType);
            PageTitle.Text = item.Label;
        }
    }

    private void OnToggleTheme(object sender, System.Windows.RoutedEventArgs e)
        => Mine.Wpf.Controls.Theming.ThemeManager.ToggleLightDark();

    private void OnSeedColor(object sender, System.Windows.RoutedEventArgs e)
    {
        if (sender is System.Windows.FrameworkElement fe && fe.Tag is string hex)
        {
            var c = (System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString(hex);
            Mine.Wpf.Controls.Theming.ThemeManager.ApplyTheme(
                Mine.Wpf.Controls.Theming.ThemeManager.Mode, c);
        }
    }
}

public class NavItem
{
    public string Label    { get; set; } = "";
    public string Icon     { get; set; } = "";
    public Type   PageType { get; set; } = typeof(object);
}


