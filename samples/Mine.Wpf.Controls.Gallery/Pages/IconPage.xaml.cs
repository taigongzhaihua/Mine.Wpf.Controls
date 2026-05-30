using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using Mine.Wpf.Controls.Icons;

namespace Mine.Wpf.Controls.Gallery.Pages;

public partial class IconPage : Mine.Wpf.Controls.Controls.Page
{
    // Fill 状态
    private bool _currentFill;

    // 图标原始数据（只含 Kind 和 Name，Fill 在渲染时叠加）
    private record IconItem(string Kind, string Name, bool Fill);

    private static readonly (string Kind, string Name)[] AllIcons = typeof(MaterialIcons)
        .GetFields(BindingFlags.Public | BindingFlags.Static)
        .Where(f => f.IsLiteral && f.FieldType == typeof(string))
        .Select(f => ((string)f.GetRawConstantValue()!, ToSnakeCase(f.Name)))
        // 过滤掉补充私用区字符（代理对，WPF 无法正确渲染）
        // 这类图标的 string.Length == 2（两个 char 组成一个代理对）
        .Where(t => t.Item1.Length == 1)
        .OrderBy(t => t.Item2)
        .ToArray();

    private int _perRow = 12;

    private static string ToSnakeCase(string pascal)
    {
        var sb = new System.Text.StringBuilder(pascal.Length + 8);
        for (var i = 0; i < pascal.Length; i++)
        {
            var c = pascal[i];
            if (i > 0 && char.IsUpper(c)) sb.Append('_');
            sb.Append(char.ToLower(c));
        }
        return sb.ToString();
    }

    public IconPage()
    {
        InitializeComponent();
        Loaded      += (_, _) => ApplyFilter(string.Empty);
        SizeChanged += (_, e) =>
        {
            var n = Math.Max(1, (int)(e.NewSize.Width / 100));
            if (n != _perRow) { _perRow = n; if (IsLoaded) ApplyFilter(SearchBox.Text.Trim()); }
        };
    }

    private void OnSearchChanged(object sender, TextChangedEventArgs e)
        => ApplyFilter(SearchBox.Text.Trim());

    private void ApplyFilter(string query)
    {
        var filtered = string.IsNullOrEmpty(query)
            ? AllIcons
            : AllIcons.Where(t => t.Name.Contains(query, StringComparison.OrdinalIgnoreCase));

        // 每个 IconItem 携带当前 Fill 值——直接绑定，无需跨模板
        var rows = filtered
            .Select(t => new IconItem(t.Kind, t.Name, _currentFill))
            .Chunk(_perRow)
            .ToList();

        IconList.ItemsSource = rows;

        var total = filtered.Count();
        CountLabel.Text = string.IsNullOrEmpty(query)
            ? $"{AllIcons.Length} 个图标"
            : $"{total} / {AllIcons.Length}";
    }

    private void OnFillChanged(object sender, RoutedEventArgs e)
    {
        _currentFill = !_currentFill;
        FillToggle.Content = _currentFill ? "Fill: On" : "Fill: Off";
        FillToggle.Style = _currentFill
            ? (Style)FindResource("Mine.Style.Button.Filled")
            : (Style)FindResource("Mine.Style.Button.Outlined");

        ApplyFilter(SearchBox.Text.Trim());
    }
}
