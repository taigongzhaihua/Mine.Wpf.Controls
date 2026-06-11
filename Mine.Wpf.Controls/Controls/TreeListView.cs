using System.Windows;
using System.Windows.Controls;

namespace Mine.Wpf.Controls.Controls;

/// <summary>
/// Material Design 3 树形列表视图
/// 结合 TreeView 的层级展示和 ListView 的多列显示
/// </summary>
[TemplatePart(Name = PartScrollViewer, Type = typeof(ScrollViewer))]
public class TreeListView : TreeView
{
    private const string PartScrollViewer = "PART_ScrollViewer";

    static TreeListView()
    {
        DefaultStyleKeyProperty.OverrideMetadata(
            typeof(TreeListView),
            new FrameworkPropertyMetadata(typeof(TreeListView)));
    }

    // ── 列定义集合 ─────────────────────────────────────────────────────
    public static readonly DependencyProperty ColumnsProperty =
        DependencyProperty.Register(nameof(Columns), typeof(GridViewColumnCollection), typeof(TreeListView),
            new PropertyMetadata(null));

    /// <summary>列定义集合</summary>
    public GridViewColumnCollection Columns
    {
        get => (GridViewColumnCollection)GetValue(ColumnsProperty);
        set => SetValue(ColumnsProperty, value);
    }

    public TreeListView()
    {
        Columns = new GridViewColumnCollection();
    }

    protected override DependencyObject GetContainerForItemOverride()
    {
        return new TreeListViewItem();
    }

    protected override bool IsItemItsOwnContainerOverride(object item)
    {
        return item is TreeListViewItem;
    }
}
