using System.Collections.Specialized;
using System.Windows;
using System.Windows.Controls;

namespace Mine.Wpf.Controls.Controls;

/// <summary>
/// Material 3 Segmented Button：多个互斥选项排成一行的选择控件。
/// 支持单选（SingleSelect）和多选（MultiSelect）两种模式。
/// </summary>
/// <remarks>
/// 用法：
/// <code>
/// &lt;mine:SegmentedControl SelectionMode="Single"&gt;
///     &lt;mine:SegmentedItem Content="Day"/&gt;
///     &lt;mine:SegmentedItem Content="Week"/&gt;
///     &lt;mine:SegmentedItem Content="Month"/&gt;
/// &lt;/mine:SegmentedControl&gt;
/// </code>
/// </remarks>
[TemplatePart(Name = PartItemsHost, Type = typeof(Panel))]
public class SegmentedControl : ListBox
{
    private const string PartItemsHost = "PART_ItemsHost";

    static SegmentedControl()
    {
        DefaultStyleKeyProperty.OverrideMetadata(
            typeof(SegmentedControl),
            new FrameworkPropertyMetadata(typeof(SegmentedControl)));

        // 覆盖 SelectionMode 默认值为 Single
        SelectionModeProperty.OverrideMetadata(
            typeof(SegmentedControl),
            new FrameworkPropertyMetadata(SelectionMode.Single));
    }

    protected override DependencyObject GetContainerForItemOverride()
        => new SegmentedItem();

    protected override bool IsItemItsOwnContainerOverride(object item)
        => item is SegmentedItem;

    protected override void OnItemsChanged(NotifyCollectionChangedEventArgs e)
    {
        base.OnItemsChanged(e);
        UpdateItemPositions();
    }

    public override void OnApplyTemplate()
    {
        base.OnApplyTemplate();
        UpdateItemPositions();
    }

    /// <summary>
    /// 为首、末项及中间项设置位置标记，以便 XAML 触发器渲染正确的圆角。
    /// </summary>
    private void UpdateItemPositions()
    {
        Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Loaded, () =>
        {
            var count = Items.Count;
            for (var i = 0; i < count; i++)
            {
                if (ItemContainerGenerator.ContainerFromIndex(i) is not SegmentedItem item) continue;

                item.Position = count == 1
                    ? SegmentedItemPosition.Only
                    : i == 0
                        ? SegmentedItemPosition.First
                        : i == count - 1
                            ? SegmentedItemPosition.Last
                            : SegmentedItemPosition.Middle;
            }
        });
    }
}

/// <summary>SegmentedControl 中的单个分段按钮项。</summary>
public class SegmentedItem : ListBoxItem
{
    static SegmentedItem()
    {
        DefaultStyleKeyProperty.OverrideMetadata(
            typeof(SegmentedItem),
            new FrameworkPropertyMetadata(typeof(SegmentedItem)));
    }

    // ── Icon ─────────────────────────────────────────────────────────
    public static readonly DependencyProperty IconProperty =
        DependencyProperty.Register(nameof(Icon), typeof(string), typeof(SegmentedItem),
            new PropertyMetadata(null));

    /// <summary>Material Symbols 字符（可选）。</summary>
    public string? Icon
    {
        get => (string?)GetValue(IconProperty);
        set => SetValue(IconProperty, value);
    }

    // ── Position（由父级 SegmentedControl 写入，只读）────────────────
    internal static readonly DependencyPropertyKey PositionPropertyKey =
        DependencyProperty.RegisterReadOnly(nameof(Position), typeof(SegmentedItemPosition), typeof(SegmentedItem),
            new PropertyMetadata(SegmentedItemPosition.Only));

    public static readonly DependencyProperty PositionProperty = PositionPropertyKey.DependencyProperty;

    /// <summary>该项在 SegmentedControl 中的位置（首/尾/中/独）。</summary>
    public SegmentedItemPosition Position
    {
        get => (SegmentedItemPosition)GetValue(PositionProperty);
        internal set => SetValue(PositionPropertyKey, value);
    }
}

/// <summary>SegmentedItem 在组中的位置，用于控制圆角渲染。</summary>
public enum SegmentedItemPosition
{
    /// <summary>仅此一项。</summary>
    Only,
    /// <summary>第一项（左端圆角）。</summary>
    First,
    /// <summary>中间项（无圆角）。</summary>
    Middle,
    /// <summary>最后一项（右端圆角）。</summary>
    Last,
}

