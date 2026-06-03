using System.Collections;
using System.Collections.Specialized;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Mine.Wpf.Controls.Controls;

/// <summary>
/// 面包屑导航控件，显示层级导航路径。
/// </summary>
[TemplatePart(Name = "PART_ItemsControl", Type = typeof(ItemsControl))]
public class BreadcrumbBar : Control
{
    private ItemsControl? _itemsControl;

    static BreadcrumbBar()
    {
        DefaultStyleKeyProperty.OverrideMetadata(
            typeof(BreadcrumbBar),
            new FrameworkPropertyMetadata(typeof(BreadcrumbBar)));
    }

    public BreadcrumbBar()
    {
        CommandBindings.Add(new CommandBinding(NavigateCommand, OnNavigateExecuted));
    }

    // ── ItemsSource ──────────────────────────────────────────────────
    public static readonly DependencyProperty ItemsSourceProperty =
        DependencyProperty.Register(nameof(ItemsSource), typeof(IEnumerable), typeof(BreadcrumbBar),
            new PropertyMetadata(null, OnItemsSourceChanged));

    public IEnumerable? ItemsSource
    {
        get => (IEnumerable?)GetValue(ItemsSourceProperty);
        set => SetValue(ItemsSourceProperty, value);
    }

    private static void OnItemsSourceChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        var breadcrumb = (BreadcrumbBar)d;
        if (e.OldValue is INotifyCollectionChanged oldCollection)
        {
            oldCollection.CollectionChanged -= breadcrumb.OnItemsCollectionChanged;
        }

        if (e.NewValue is INotifyCollectionChanged newCollection)
        {
            newCollection.CollectionChanged += breadcrumb.OnItemsCollectionChanged;
        }

        breadcrumb.UpdateItems();
    }

    private void OnItemsCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        UpdateItems();
    }

    // ── ItemTemplate ────────────────────────────────────────────────
    public static readonly DependencyProperty ItemTemplateProperty =
        DependencyProperty.Register(nameof(ItemTemplate), typeof(DataTemplate), typeof(BreadcrumbBar),
            new PropertyMetadata(null));

    public DataTemplate? ItemTemplate
    {
        get => (DataTemplate?)GetValue(ItemTemplateProperty);
        set => SetValue(ItemTemplateProperty, value);
    }

    // ── MaxItems ────────────────────────────────────────────────────
    public static readonly DependencyProperty MaxItemsProperty =
        DependencyProperty.Register(nameof(MaxItems), typeof(int), typeof(BreadcrumbBar),
            new PropertyMetadata(0, OnMaxItemsChanged));

    public int MaxItems
    {
        get => (int)GetValue(MaxItemsProperty);
        set => SetValue(MaxItemsProperty, value);
    }

    private static void OnMaxItemsChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        ((BreadcrumbBar)d).UpdateItems();
    }

    // ── Navigate Command ────────────────────────────────────────────
    public static readonly RoutedCommand NavigateCommand = new(
        nameof(NavigateCommand),
        typeof(BreadcrumbBar));

    private void OnNavigateExecuted(object sender, ExecutedRoutedEventArgs e)
    {
        if (e.Parameter is BreadcrumbItem item)
        {
            RaiseItemClickedEvent(item);
        }
    }

    // ── ItemClicked Event ───────────────────────────────────────────
    public static readonly RoutedEvent ItemClickedEvent =
        EventManager.RegisterRoutedEvent(
            nameof(ItemClicked),
            RoutingStrategy.Bubble,
            typeof(BreadcrumbItemClickedEventHandler),
            typeof(BreadcrumbBar));

    public event BreadcrumbItemClickedEventHandler ItemClicked
    {
        add => AddHandler(ItemClickedEvent, value);
        remove => RemoveHandler(ItemClickedEvent, value);
    }

    private void RaiseItemClickedEvent(BreadcrumbItem item)
    {
        var args = new BreadcrumbItemClickedEventArgs(ItemClickedEvent, this)
        {
            Item = item.Data,
            Index = item.Index
        };
        RaiseEvent(args);
    }

    // ── Template Application ────────────────────────────────────────
    public override void OnApplyTemplate()
    {
        base.OnApplyTemplate();
        _itemsControl = GetTemplateChild("PART_ItemsControl") as ItemsControl;
        UpdateItems();
    }

    // ── Update Items Logic ──────────────────────────────────────────
    private void UpdateItems()
    {
        if (_itemsControl == null)
            return;

        if (ItemsSource == null)
        {
            _itemsControl.ItemsSource = null;
            return;
        }

        var sourceList = new List<object>();
        foreach (var item in ItemsSource)
        {
            sourceList.Add(item);
        }

        var displayList = new List<BreadcrumbItem>();
        var totalCount = sourceList.Count;

        if (totalCount == 0)
        {
            _itemsControl.ItemsSource = displayList;
            return;
        }

        if (MaxItems > 0 && totalCount > MaxItems)
        {
            displayList.Add(new BreadcrumbItem
            {
                Data = sourceList[0],
                Index = 0,
                IsEllipsis = false
            });

            displayList.Add(new BreadcrumbItem
            {
                Data = null,
                Index = -1,
                IsEllipsis = true
            });

            for (var i = totalCount - (MaxItems - 2); i < totalCount; i++)
            {
                displayList.Add(new BreadcrumbItem
                {
                    Data = sourceList[i],
                    Index = i,
                    IsEllipsis = false
                });
            }
        }
        else
        {
            for (var i = 0; i < totalCount; i++)
            {
                displayList.Add(new BreadcrumbItem
                {
                    Data = sourceList[i],
                    Index = i,
                    IsEllipsis = false
                });
            }
        }

        foreach (var item in displayList)
        {
            item.IsLast = item == displayList[^1];
        }

        _itemsControl.ItemsSource = displayList;
    }
}

/// <summary>
/// 面包屑项的内部容器。
/// </summary>
public class BreadcrumbItem : DependencyObject
{
    public int Index { get; set; }
    public bool IsEllipsis { get; set; }
    public bool IsLast { get; set; }
    public object? Data { get; set; }
}

/// <summary>
/// 面包屑项点击事件参数。
/// </summary>
public class BreadcrumbItemClickedEventArgs : RoutedEventArgs
{
    public BreadcrumbItemClickedEventArgs(RoutedEvent routedEvent, object source)
        : base(routedEvent, source)
    {
    }

    public object? Item { get; set; }
    public int Index { get; set; }
}

public delegate void BreadcrumbItemClickedEventHandler(object sender, BreadcrumbItemClickedEventArgs e);
