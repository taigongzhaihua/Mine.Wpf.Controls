using System.Windows;
using Mine.Wpf.Controls.Controls;

namespace Mine.Wpf.Controls.Gallery.Pages;

public partial class ChipPage
{
    public ChipPage() => InitializeComponent();

    private void OnChipDeleted(object sender, RoutedEventArgs e)
    {
        if (sender is Chip chip)
            InputChipPanel.Children.Remove(chip);
    }

    private void OnChipDeleted2(object sender, RoutedEventArgs e)
    {
        if (sender is Chip chip)
            InputChipPanel2.Children.Remove(chip);
    }
}


