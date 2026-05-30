namespace Mine.Wpf.Controls.Gallery.Pages;

public partial class FlyoutPage
{
    public FlyoutPage() => InitializeComponent();

    private void OnCodeOpen(object sender, System.Windows.RoutedEventArgs e)
        => CodeFlyout.Show();

    private void OnCodeClose(object sender, System.Windows.RoutedEventArgs e)
        => CodeFlyout.Hide();
}

