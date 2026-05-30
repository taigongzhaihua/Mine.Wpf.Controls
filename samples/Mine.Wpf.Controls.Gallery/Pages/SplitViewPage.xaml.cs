using System.Windows;
using Mine.Wpf.Controls.Controls;

namespace Mine.Wpf.Controls.Gallery.Pages;

public partial class SplitViewPage
{
    public SplitViewPage() => InitializeComponent();

    private void OnTogglePane(object sender, RoutedEventArgs e)
        => Demo.TogglePane();

    private void OnNavClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
        => Demo.ClosePane(); // Overlay 模式下点导航项自动关闭

    private void OnModeOverlay(object sender, RoutedEventArgs e)
    {
        Demo.DisplayMode = SplitViewDisplayMode.Overlay;
        Demo.IsPaneOpen = false;
    }

    private void OnModeCompactOverlay(object sender, RoutedEventArgs e)
    {
        Demo.DisplayMode = SplitViewDisplayMode.CompactOverlay;
        Demo.IsPaneOpen = false;
    }

    private void OnModeInline(object sender, RoutedEventArgs e)
    {
        Demo.DisplayMode = SplitViewDisplayMode.Inline;
        Demo.IsPaneOpen = true;
    }

    private void OnModeCompactInline(object sender, RoutedEventArgs e)
    {
        Demo.DisplayMode = SplitViewDisplayMode.CompactInline;
        Demo.IsPaneOpen = true;
    }
}

