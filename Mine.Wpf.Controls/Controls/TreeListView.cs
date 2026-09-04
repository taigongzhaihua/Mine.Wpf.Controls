using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;

namespace Mine.Wpf.Controls.Controls;

/// <summary>TreeListView 的选择模式。</summary>
public enum TreeListViewSelectionMode
{
    /// <summary>单选（默认，与原生 TreeView 行为一致）。</summary>
    Single,

    /// <summary>支持 Ctrl/Shift 多选。</summary>
    Multiple,
}

/// <summary>
/// Material Design 3 树形列表视图
/// 结合 TreeView 的层级展示和 ListView 的多列显示，支持多选、列宽拖拽（表头与行共享同一 Columns 集合，天然同步）
/// 与点击表头排序。
/// </summary>
[TemplatePart(Name = PartScrollViewer, Type = typeof(ScrollViewer))]
[TemplatePart(Name = PartHeaderRow,    Type = typeof(GridViewHeaderRowPresenter))]
public class TreeListView : TreeView
{
    private const string PartScrollViewer = "PART_ScrollViewer";
    private const string PartHeaderRow    = "PART_HeaderRow";

    private GridViewHeaderRowPresenter? _headerRow;
    private readonly List<TreeListViewItem> _selectedContainers = new();
    private readonly ObservableCollection<object> _selectedItemsCore = new();
    private TreeListViewItem? _selectionAnchor;

    static TreeListView()
    {
        DefaultStyleKeyProperty.OverrideMetadata(
            typeof(TreeListView),
            new FrameworkPropertyMetadata(typeof(TreeListView)));
    }

    public TreeListView()
    {
        Columns = new GridViewColumnCollection();
        SelectedItems = new ReadOnlyObservableCollection<object>(_selectedItemsCore);
    }

    // ── 列定义集合 ─────────────────────────────────────────────────────
    public static readonly DependencyProperty ColumnsProperty =
        DependencyProperty.Register(nameof(Columns), typeof(GridViewColumnCollection), typeof(TreeListView),
            new PropertyMetadata(null));

    /// <summary>列定义集合，表头与每一行共享同一实例，拖拽列宽会自动同步到所有行。</summary>
    public GridViewColumnCollection Columns
    {
        get => (GridViewColumnCollection)GetValue(ColumnsProperty);
        set => SetValue(ColumnsProperty, value);
    }

    // ── SelectionMode ─────────────────────────────────────────────────
    public static readonly DependencyProperty SelectionModeProperty =
        DependencyProperty.Register(nameof(SelectionMode), typeof(TreeListViewSelectionMode), typeof(TreeListView),
            new PropertyMetadata(TreeListViewSelectionMode.Single));

    public TreeListViewSelectionMode SelectionMode
    {
        get => (TreeListViewSelectionMode)GetValue(SelectionModeProperty);
        set => SetValue(SelectionModeProperty, value);
    }

    /// <summary>当前选中的数据项集合（只读）。多选模式下由 Ctrl/Shift 点击维护。</summary>
    public ReadOnlyObservableCollection<object> SelectedItems { get; }

    public static readonly RoutedEvent SelectionChangedEvent =
        EventManager.RegisterRoutedEvent(nameof(SelectionChanged), RoutingStrategy.Bubble,
            typeof(SelectionChangedEventHandler), typeof(TreeListView));

    public event SelectionChangedEventHandler SelectionChanged
    {
        add => AddHandler(SelectionChangedEvent, value);
        remove => RemoveHandler(SelectionChangedEvent, value);
    }

    // ── 排序状态（只读，供表头排序箭头绑定） ───────────────────────────
    private static readonly DependencyPropertyKey SortMemberPathPropertyKey =
        DependencyProperty.RegisterReadOnly(nameof(SortMemberPath), typeof(string), typeof(TreeListView),
            new PropertyMetadata(null));
    public static readonly DependencyProperty SortMemberPathProperty = SortMemberPathPropertyKey.DependencyProperty;

    /// <summary>当前排序依据的列绑定路径（未排序时为 null）。</summary>
    public string? SortMemberPath => (string?)GetValue(SortMemberPathProperty);

    private static readonly DependencyPropertyKey SortDirectionPropertyKey =
        DependencyProperty.RegisterReadOnly(nameof(SortDirection), typeof(ListSortDirection?), typeof(TreeListView),
            new PropertyMetadata(null));
    public static readonly DependencyProperty SortDirectionProperty = SortDirectionPropertyKey.DependencyProperty;

    /// <summary>当前排序方向（未排序时为 null）。</summary>
    public ListSortDirection? SortDirection => (ListSortDirection?)GetValue(SortDirectionProperty);

    protected override DependencyObject GetContainerForItemOverride() => new TreeListViewItem();

    protected override bool IsItemItsOwnContainerOverride(object item) => item is TreeListViewItem;

    public override void OnApplyTemplate()
    {
        if (_headerRow != null)
            _headerRow.RemoveHandler(GridViewColumnHeader.ClickEvent, (RoutedEventHandler)OnColumnHeaderClick);

        base.OnApplyTemplate();

        _headerRow = GetTemplateChild(PartHeaderRow) as GridViewHeaderRowPresenter;
        _headerRow?.AddHandler(GridViewColumnHeader.ClickEvent, (RoutedEventHandler)OnColumnHeaderClick);
    }

    // ── 排序 ──────────────────────────────────────────────────────────
    private void OnColumnHeaderClick(object sender, RoutedEventArgs e)
    {
        if (e.OriginalSource is not GridViewColumnHeader { Column.DisplayMemberBinding: Binding binding } header
            || string.IsNullOrEmpty(binding.Path?.Path))
            return;

        var path = binding.Path.Path;
        var direction = SortMemberPath == path && SortDirection == ListSortDirection.Ascending
            ? ListSortDirection.Descending
            : ListSortDirection.Ascending;

        SetValue(SortMemberPathPropertyKey, path);
        SetValue(SortDirectionPropertyKey, direction);
        ApplySort(this, path, direction);
    }

    /// <summary>对 <paramref name="itemsControl"/> 当前层级及所有已生成的子层级递归排序。</summary>
    internal static void ApplySort(ItemsControl itemsControl, string propertyPath, ListSortDirection direction)
    {
        var view = itemsControl.ItemsSource != null
            ? CollectionViewSource.GetDefaultView(itemsControl.ItemsSource)
            : (ICollectionView)itemsControl.Items;

        using (view.DeferRefresh())
        {
            view.SortDescriptions.Clear();
            view.SortDescriptions.Add(new SortDescription(propertyPath, direction));
        }

        foreach (var item in itemsControl.Items)
        {
            if (itemsControl.ItemContainerGenerator.ContainerFromItem(item) is TreeListViewItem child)
                ApplySort(child, propertyPath, direction);
        }
    }

    // ── 多选 ──────────────────────────────────────────────────────────
    internal void HandleItemClick(TreeListViewItem item, ModifierKeys modifiers)
    {
        var multi = SelectionMode == TreeListViewSelectionMode.Multiple;

        if (multi && modifiers == ModifierKeys.Control)
            ToggleSelection(item);
        else if (multi && modifiers == ModifierKeys.Shift && _selectionAnchor != null)
            SelectRange(_selectionAnchor, item);
        else
            ReplaceSelection(item);
    }

    private void ToggleSelection(TreeListViewItem item)
    {
        var oldItems = _selectedItemsCore.ToArray();

        if (_selectedContainers.Remove(item))
        {
            item.IsItemSelected = false;
            _selectedItemsCore.Remove(item.Header);
        }
        else
        {
            _selectedContainers.Add(item);
            item.IsItemSelected = true;
            _selectedItemsCore.Add(item.Header);
            _selectionAnchor = item;
        }

        item.IsSelected = item.IsItemSelected;
        RaiseSelectionChanged(oldItems, _selectedItemsCore.ToArray());
    }

    private void ReplaceSelection(TreeListViewItem item)
    {
        var oldItems = _selectedItemsCore.ToArray();
        ClearSelectionCore();

        _selectedContainers.Add(item);
        item.IsItemSelected = true;
        _selectedItemsCore.Add(item.Header);
        _selectionAnchor = item;

        item.IsSelected = true;
        RaiseSelectionChanged(oldItems, _selectedItemsCore.ToArray());
    }

    private void SelectRange(TreeListViewItem anchor, TreeListViewItem target)
    {
        var flat = FlattenVisibleItems().ToList();
        var i1 = flat.IndexOf(anchor);
        var i2 = flat.IndexOf(target);
        if (i1 < 0 || i2 < 0)
        {
            ReplaceSelection(target);
            return;
        }

        var oldItems = _selectedItemsCore.ToArray();
        ClearSelectionCore();

        var (lo, hi) = i1 <= i2 ? (i1, i2) : (i2, i1);
        for (var i = lo; i <= hi; i++)
        {
            var container = flat[i];
            container.IsItemSelected = true;
            _selectedContainers.Add(container);
            _selectedItemsCore.Add(container.Header);
        }

        target.IsSelected = true;
        RaiseSelectionChanged(oldItems, _selectedItemsCore.ToArray());
    }

    private void ClearSelectionCore()
    {
        foreach (var container in _selectedContainers)
            container.IsItemSelected = false;
        _selectedContainers.Clear();
        _selectedItemsCore.Clear();
    }

    private IEnumerable<TreeListViewItem> FlattenVisibleItems()
    {
        IEnumerable<TreeListViewItem> Walk(ItemsControl parent)
        {
            foreach (var data in parent.Items)
            {
                if (parent.ItemContainerGenerator.ContainerFromItem(data) is not TreeListViewItem container)
                    continue;
                yield return container;
                if (container.IsExpanded)
                    foreach (var child in Walk(container))
                        yield return child;
            }
        }

        return Walk(this);
    }

    private void RaiseSelectionChanged(object[] oldItems, object[] newItems)
    {
        var added   = newItems.Except(oldItems).ToList();
        var removed = oldItems.Except(newItems).ToList();
        if (added.Count == 0 && removed.Count == 0) return;
        RaiseEvent(new SelectionChangedEventArgs(SelectionChangedEvent, removed, added));
    }
}
