using System.Windows;

namespace Mine.Wpf.Controls.Gallery.Pages;

public partial class MenuFlyoutPage
{
    public MenuFlyoutPage()
    {
        InitializeComponent();
    }

    private void OnOpenMenuClick(object sender, RoutedEventArgs e)
    {
        if (sender is not FrameworkElement element) return;
        StandaloneMenu.PlacementTarget = element;
        StandaloneMenu.IsOpen = true;
    }

    private void OnOpenSubMenuClick(object sender, RoutedEventArgs e)
    {
        if (sender is not FrameworkElement element) return;
        SubMenuFlyout.PlacementTarget = element;
        SubMenuFlyout.IsOpen = true;
    }

    private void OnOpenStatesMenuClick(object sender, RoutedEventArgs e)
    {
        if (sender is not FrameworkElement element) return;
        StatesMenu.PlacementTarget = element;
        StatesMenu.IsOpen = true;
    }
}
