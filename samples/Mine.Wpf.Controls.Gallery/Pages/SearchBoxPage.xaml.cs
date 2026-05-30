using System.Collections.ObjectModel;
using System.Windows.Media;
using Mine.Wpf.Controls.Controls;

namespace Mine.Wpf.Controls.Gallery.Pages;

public partial class SearchBoxPage : Page
{
    // ── 城市列表（字符串，用于自动过滤演示） ─────────────────────
    public IReadOnlyList<string> Cities { get; } =
    [
        "北京", "上海", "广州", "深圳", "杭州", "南京", "成都", "武汉",
        "西安", "重庆", "苏州", "天津", "长沙", "郑州", "青岛", "大连",
        "厦门", "宁波", "合肥", "昆明", "沈阳", "哈尔滨", "济南", "福州"
    ];

    // ── 编程语言列表（对象，含自定义模板演示） ────────────────────
    public IReadOnlyList<LanguageItem> Languages { get; } =
    [
        new("C#",         "#239120", "Microsoft"),
        new("Kotlin",     "#7F52FF", "JetBrains"),
        new("Rust",       "#CE422B", "Mozilla"),
        new("TypeScript", "#3178C6", "Microsoft"),
        new("Go",         "#00ACD7", "Google"),
        new("Swift",      "#F05138", "Apple"),
        new("Python",     "#3776AB", "PSF"),
        new("Java",       "#ED8B00", "Oracle"),
        new("Dart",       "#00B4AB", "Google"),
        new("F#",         "#378BBA", "Microsoft"),
    ];

    // ── 手动模式结果集（模拟远程搜索） ───────────────────────────
    public ObservableCollection<string> ManualResults { get; } = [];

    public SearchBoxPage() => InitializeComponent();

    private void OnSuggestionSelected(object sender, SuggestionSelectedEventArgs e)
        => BarResult.Text = $"已选中：{e.SelectedItem}";

    private void OnSearch(object sender, System.Windows.RoutedEventArgs e)
    {
        if (sender is SearchBox box)
            SearchResult.Text = $"搜索关键字：「{box.Text}」";
    }

    private async void OnManualSearch(object sender, System.Windows.RoutedEventArgs e)
    {
        if (sender is not SearchBox box || string.IsNullOrWhiteSpace(box.Text)) return;

        ManualStatus.Text = "正在搜索...";
        ManualResults.Clear();

        // 模拟网络延迟
        await Task.Delay(600);

        var keyword = box.Text.Trim();
        foreach (var city in Cities)
            if (city.Contains(keyword, StringComparison.OrdinalIgnoreCase))
                ManualResults.Add($"{city}（远程结果）");

        if (ManualResults.Count == 0)
            ManualResults.Add($"无匹配结果：{keyword}");

        box.IsDropDownOpen = true;
        ManualStatus.Text = $"共找到 {ManualResults.Count} 条结果";
    }
}

/// <summary>编程语言演示数据项。</summary>
public record LanguageItem(string Name, string Color, string Tag)
{
    public Brush ColorBrush =>
        new SolidColorBrush((Color)ColorConverter.ConvertFromString(Color)!);
}

