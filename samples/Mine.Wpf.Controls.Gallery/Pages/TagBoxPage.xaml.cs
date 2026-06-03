using System.Collections.ObjectModel;
using System.Windows;
using Mine.Wpf.Controls.Controls;

namespace Mine.Wpf.Controls.Gallery.Pages;

public partial class TagBoxPage : Page
{
    public TagBoxPage()
    {
        InitializeComponent();
        DataContext = this;

        BasicTags = new ObservableCollection<string>
        {
            "WPF",
            "Material Design",
            "C#"
        };

        MaxTagsList = new ObservableCollection<string>
        {
            "Tag 1",
            "Tag 2"
        };

        NoDuplicatesTags = new ObservableCollection<string>
        {
            "Unique",
            "Tags"
        };

        PredefinedTags = new ObservableCollection<string>
        {
            "Design",
            "Development",
            "Testing",
            "Documentation"
        };

        EventTags = new ObservableCollection<string>
        {
            "Event",
            "Demo"
        };
    }

    public ObservableCollection<string> BasicTags { get; }
    public ObservableCollection<string> MaxTagsList { get; }
    public ObservableCollection<string> NoDuplicatesTags { get; }
    public ObservableCollection<string> PredefinedTags { get; }
    public ObservableCollection<string> EventTags { get; }

    private void OnTagAdded(object sender, TagEventArgs e)
    {
        EventLogTextBlock.Text = $"Tag Added: {e.Tag}";
        System.Diagnostics.Debug.WriteLine($"Tag Added: {e.Tag}");
    }

    private void OnTagRemoved(object sender, TagEventArgs e)
    {
        EventLogTextBlock.Text = $"Tag Removed: {e.Tag}";
        System.Diagnostics.Debug.WriteLine($"Tag Removed: {e.Tag}");
    }
}
