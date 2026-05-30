using Mine.Wpf.Controls.Controls;
using System.Windows;

namespace Mine.Wpf.Controls.Gallery.Pages;

public partial class DrawerPage
{
    public DrawerPage() => InitializeComponent();

    private void OnOpenLeft(object sender, RoutedEventArgs e)
    {
        DemoDrawer.DrawerPlacement = DrawerPlacement.Left;
        DemoDrawer.DrawerWidth     = 280;
        DemoDrawer.IsOpen          = true;
    }

    private void OnOpenRight(object sender, RoutedEventArgs e)
    {
        DemoDrawer.DrawerPlacement = DrawerPlacement.Right;
        DemoDrawer.DrawerWidth     = 320;
        DemoDrawer.IsOpen          = true;
    }

    private void OnOpenBottom(object sender, RoutedEventArgs e)
    {
        DemoDrawer.DrawerPlacement = DrawerPlacement.Bottom;
        DemoDrawer.DrawerHeight    = 260;
        DemoDrawer.IsOpen          = true;
    }
}

