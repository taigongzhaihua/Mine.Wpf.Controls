namespace Mine.Wpf.Controls.Gallery.Pages;

public partial class TabPage
{
    private record FileItem(string Name, string Type, string Size, string Date);

    public TabPage()
    {
        InitializeComponent();
        DemoListView.ItemsSource = new[]
        {
            new FileItem("设计文档.docx",   "Word 文档",    "248 KB",  "2026-05-20 14:32"),
            new FileItem("原型设计.fig",     "Figma 文件",   "12.4 MB", "2026-05-22 09:15"),
            new FileItem("控件演示.mp4",     "视频文件",     "88.2 MB", "2026-05-23 18:00"),
            new FileItem("主题配置.json",    "JSON 文件",    "4 KB",    "2026-05-24 08:30"),
            new FileItem("Gallery.exe",      "应用程序",     "2.1 MB",  "2026-05-24 10:00"),
            new FileItem("README.md",        "Markdown 文档","16 KB",   "2026-05-24 11:22"),
        };
    }
}


