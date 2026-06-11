using System.Windows;
using System.Windows.Controls;

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
