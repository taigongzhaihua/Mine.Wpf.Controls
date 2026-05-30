using System.Windows;
using Mine.Wpf.Controls.Controls;

namespace Mine.Wpf.Controls.Gallery.Pages;

public partial class NavigationViewPage
{
    public NavigationViewPage()
    {
        InitializeComponent();
        Loaded += OnLoaded;
    }

    private void OnLoaded(object sender, RoutedEventArgs e)
    {
        // 添加导航项
        DemoNav.MenuItemsSource = new[]
        {
            new NavigationViewItem { Icon = "\uE88A", Content = "Home" },
            new NavigationViewItem { Icon = "\uE8F4", Content = "Favorites" },
            new NavigationViewItem { Icon = "\uE8B8", Content = "History" },
            new NavigationViewItem { Icon = "\uE8B6", Content = "Search" },
        };

        DemoNav.FooterMenuItemsSource = new[]
        {
            new NavigationViewItem { Icon = "\uE8B8", Content = "Settings" },
        };

        DemoNav.SelectionChanged += OnNavSelectionChanged;
    }

    private void OnNavSelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
    {
        if (e.AddedItems.Count > 0 && e.AddedItems[0] is NavigationViewItem item)
        {
            SelectedLabel.Text = item.Content?.ToString() ?? "(none)";
            DemoNav.Header = item.Content;
        }
    }

    private void OnModeLeft(object sender, RoutedEventArgs e)
        => DemoNav.DisplayMode = NavigationViewDisplayMode.Left;

    private void OnModeLeftCompact(object sender, RoutedEventArgs e)
        => DemoNav.DisplayMode = NavigationViewDisplayMode.LeftCompact;

    private void OnModeLeftCompactInline(object sender, RoutedEventArgs e)
        => DemoNav.DisplayMode = NavigationViewDisplayMode.LeftCompactInline;

    private void OnModeLeftMinimal(object sender, RoutedEventArgs e)
        => DemoNav.DisplayMode = NavigationViewDisplayMode.LeftMinimal;

    private void OnModeAuto(object sender, RoutedEventArgs e)
        => DemoNav.DisplayMode = NavigationViewDisplayMode.Auto;
}

