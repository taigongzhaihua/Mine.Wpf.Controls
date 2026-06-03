using System.Collections.ObjectModel;
using System.Windows;
using Mine.Wpf.Controls.Controls;

namespace Mine.Wpf.Controls.Gallery.Pages;

public partial class BreadcrumbBarPage : Page
{
    public BreadcrumbBarPage()
    {
        InitializeComponent();
        DataContext = this;

        BasicItems = new ObservableCollection<string>
        {
            "Home",
            "Documents",
            "Projects",
            "WPF"
        };

        LongPathItems = new ObservableCollection<string>
        {
            "Home",
            "Users",
            "Administrator",
            "Documents",
            "Projects",
            "Mine.Wpf.Controls",
            "src",
            "Controls"
        };

        CustomItems = new ObservableCollection<CustomBreadcrumbItem>
        {
            new() { Label = "Home", Icon = "Home" },
            new() { Label = "Folder", Icon = "Folder" },
            new() { Label = "Document", Icon = "Description" }
        };
    }

    public ObservableCollection<string> BasicItems { get; }
    public ObservableCollection<string> LongPathItems { get; }
    public ObservableCollection<CustomBreadcrumbItem> CustomItems { get; }

    private void OnBreadcrumbItemClicked(object sender, Controls.BreadcrumbItemClickedEventArgs e)
    {
        // 暂时禁用 MessageBox 以避免可能的问题
        // MessageBox.Show($"Clicked item at index {e.Index}: {e.Item}", "BreadcrumbBar Demo");
        System.Diagnostics.Debug.WriteLine($"Clicked item at index {e.Index}: {e.Item}");
    }

    private void OnAddItem(object sender, RoutedEventArgs e)
    {
        BasicItems.Add($"Item {BasicItems.Count + 1}");
    }

    private void OnRemoveItem(object sender, RoutedEventArgs e)
    {
        if (BasicItems.Count > 0)
        {
            BasicItems.RemoveAt(BasicItems.Count - 1);
        }
    }
}

public class CustomBreadcrumbItem
{
    public string? Label { get; set; }
    public string? Icon { get; set; }

    public override string ToString() => Label ?? string.Empty;
}
