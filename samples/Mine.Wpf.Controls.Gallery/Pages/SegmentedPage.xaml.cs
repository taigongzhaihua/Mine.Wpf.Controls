using System.Windows.Controls;
using Mine.Wpf.Controls.Controls;

namespace Mine.Wpf.Controls.Gallery.Pages;

public partial class SegmentedPage
{
    public SegmentedPage() => InitializeComponent();

    private void OnSegmentedChanged(object sender, SelectionChangedEventArgs e)
    {
        if (SegmentedLabel == null) return;
        if (DemoSegmented.SelectedItem is SegmentedItem item)
            SegmentedLabel.Text = $"Selected: {item.Content}";
        else
            SegmentedLabel.Text = "(none selected)";
    }
}

