using System.Windows;
using Mine.Wpf.Controls.Controls;

namespace Mine.Wpf.Controls.Gallery.Pages;

public partial class InfoBarPage : Page
{
    public InfoBarPage()
    {
        InitializeComponent();
    }

    private void OnResetClosableBar(object sender, RoutedEventArgs e)
    {
        ClosableBar.IsOpen = true;
    }
}
