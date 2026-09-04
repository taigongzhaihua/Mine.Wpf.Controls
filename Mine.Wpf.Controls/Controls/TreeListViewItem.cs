using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;

namespace Mine.Wpf.Controls.Controls;

/// <summary>
/// TreeListView 的子项容器
/// </summary>
public class TreeListViewItem : TreeViewItem
{
    static TreeListViewItem()
    {
        DefaultStyleKeyProperty.OverrideMetadata(
            typeof(TreeListViewItem),
            new FrameworkPropertyMetadata(typeof(TreeListViewItem)));
    }

    // ── 缩进级别 ─────────────────────────────────────────────────────
    public static readonly DependencyProperty LevelProperty =
        DependencyProperty.Register(nameof(Level), typeof(int), typeof(TreeListViewItem),
            new PropertyMetadata(0));

    /// <summary>节点的层级深度,用于计算缩进</summary>
    public int Level
    {
        get => (int)GetValue(LevelProperty);
        set => SetValue(LevelProperty, value);
    }

    // ── 多选状态 ─────────────────────────────────────────────────────
    public static readonly DependencyProperty IsItemSelectedProperty =
        DependencyProperty.Register(nameof(IsItemSelected), typeof(bool), typeof(TreeListViewItem),
            new PropertyMetadata(false));

    /// <summary>
    /// 是否处于选中状态。独立于原生 <see cref="TreeViewItem.IsSelected"/>（原生选中只支持单选），
    /// 用于驱动 <see cref="TreeListView"/> 的多选视觉效果。
    /// </summary>
    public bool IsItemSelected
    {
        get => (bool)GetValue(IsItemSelectedProperty);
        set => SetValue(IsItemSelectedProperty, value);
    }

    /// <summary>沿逻辑父级链查找拥有本容器的 <see cref="TreeListView"/>。</summary>
    private TreeListView? FindOwnerTreeListView()
    {
        DependencyObject current = this;
        while (ItemsControl.ItemsControlFromItemContainer(current) is { } parent)
        {
            if (parent is TreeListView owner) return owner;
            current = parent;
        }
        return null;
    }

    protected override void OnMouseLeftButtonDown(MouseButtonEventArgs e)
    {
        // 点击落在展开按钮/单元格内部的交互控件（如按钮）上时，保留原生行为，不做选择处理
        if (HasInteractiveAncestor(e.OriginalSource as DependencyObject))
        {
            base.OnMouseLeftButtonDown(e);
            return;
        }

        e.Handled = true;
        Focus();
        FindOwnerTreeListView()?.HandleItemClick(this, Keyboard.Modifiers);
    }

    private bool HasInteractiveAncestor(DependencyObject? source)
    {
        while (source != null && !ReferenceEquals(source, this))
        {
            if (source is ButtonBase) return true;
            source = VisualTreeHelper.GetParent(source);
        }
        return false;
    }

    protected override void OnExpanded(RoutedEventArgs e)
    {
        base.OnExpanded(e);

        // 展开时新生成的子容器需要补上当前排序状态
        if (FindOwnerTreeListView() is { SortMemberPath: { } path, SortDirection: { } direction })
            TreeListView.ApplySort(this, path, direction);
    }

    protected override DependencyObject GetContainerForItemOverride()
    {
        return new TreeListViewItem();
    }

    protected override bool IsItemItsOwnContainerOverride(object item)
    {
        return item is TreeListViewItem;
    }

    protected override void OnVisualParentChanged(DependencyObject oldParent)
    {
        base.OnVisualParentChanged(oldParent);
        UpdateLevel();
    }

    private void UpdateLevel()
    {
        if (ItemsControl.ItemsControlFromItemContainer(this) is TreeListViewItem parentItem)
        {
            Level = parentItem.Level + 1;
        }
        else
        {
            Level = 0;
        }
    }

    protected override void OnItemsChanged(System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
    {
        base.OnItemsChanged(e);

        // 延迟更新子项层级,确保容器已生成
        Dispatcher.BeginInvoke(new Action(() =>
        {
            foreach (var item in Items)
            {
                if (ItemContainerGenerator.ContainerFromItem(item) is TreeListViewItem container)
                {
                    container.UpdateLevel();
                }
            }
        }));
    }
}
