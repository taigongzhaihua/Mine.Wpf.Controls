using System.Windows;

namespace Mine.Wpf.Controls.Gallery.Pages;

public partial class BadgePage
{
    public BadgePage() => InitializeComponent();

    private void OnSetDot(object sender, RoutedEventArgs e)
        => DemoBadge.BadgeText = "";

    private void OnSetNumber(object sender, RoutedEventArgs e)
        => DemoBadge.BadgeText = "5";

    private void OnSetLarge(object sender, RoutedEventArgs e)
        => DemoBadge.BadgeText = "99+";

    private void OnHide(object sender, RoutedEventArgs e)
        => DemoBadge.BadgeText = null;
}

