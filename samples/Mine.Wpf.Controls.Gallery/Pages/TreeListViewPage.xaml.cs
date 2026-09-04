using System.Collections.ObjectModel;
using Mine.Wpf.Controls.Controls;

namespace Mine.Wpf.Controls.Gallery.Pages;

public partial class TreeListViewPage : Page
{
    public ObservableCollection<FileNode> FileTree { get; } =
    [
        new FileNode
        {
            Name = "文档", Size = "-", Modified = "2024-01-02", Children =
            [
                new FileNode { Name = "报告.docx", Size = "128 KB", Modified = "2024-03-11" },
                new FileNode { Name = "预算.xlsx", Size = "64 KB", Modified = "2024-02-20" },
            ],
        },
        new FileNode
        {
            Name = "图片", Size = "-", Modified = "2023-11-05", Children =
            [
                new FileNode { Name = "照片1.jpg", Size = "2.1 MB", Modified = "2023-11-05" },
                new FileNode { Name = "照片2.png", Size = "3.4 MB", Modified = "2024-01-18" },
            ],
        },
        new FileNode { Name = "readme.md", Size = "2 KB", Modified = "2024-04-01" },
    ];

    public TreeListViewPage()
    {
        InitializeComponent();
        DataContext = this;
    }
}

/// <summary>TreeListView 多列/排序/多选示例用的数据节点。</summary>
public sealed class FileNode
{
    public string Name { get; set; } = "";
    public string Size { get; set; } = "";
    public string Modified { get; set; } = "";
    public ObservableCollection<FileNode> Children { get; set; } = [];
}
