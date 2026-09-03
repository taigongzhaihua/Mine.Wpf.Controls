using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;

namespace Mine.Wpf.Controls.Controls;

/// <summary>
/// Material 3 顶部菜单栏：横向排列一组 <see cref="MenuBarItem"/>（例如 文件/编辑/视图），
/// 点击展开下拉菜单，展开状态下鼠标悬停到其它项会自动切换。
/// </summary>
[TemplatePart(Name = PartItemsPanel, Type = typeof(Panel))]
public class MenuBar : Control
{
    private const string PartItemsPanel = "PART_ItemsPanel";

    private Panel? _itemsPanel;
    private MenuBarItem? _openItem;
    private System.Windows.Window? _hostWindow;

    static MenuBar()
    {
        DefaultStyleKeyProperty.OverrideMetadata(
            typeof(MenuBar),
            new FrameworkPropertyMetadata(typeof(MenuBar)));
    }

    public MenuBar()
    {
        Items = [];
        Loaded += OnLoaded;
        Unloaded += OnUnloaded;
    }

    private void OnLoaded(object sender, RoutedEventArgs e)
    {
        _hostWindow = System.Windows.Window.GetWindow(this);
        if (_hostWindow != null)
            _hostWindow.PreviewMouseDown += OnHostWindowPreviewMouseDown;
    }

    private void OnUnloaded(object sender, RoutedEventArgs e)
    {
        if (_hostWindow != null)
            _hostWindow.PreviewMouseDown -= OnHostWindowPreviewMouseDown;
        _hostWindow = null;
    }

    private void OnHostWindowPreviewMouseDown(object sender, MouseButtonEventArgs e)
    {
        if (_openItem == null) return;
        if (e.OriginalSource is not DependencyObject source) return;

        // 点击落在菜单栏自身或当前展开的下拉菜单内部时不关闭。
        if (IsDescendant(this, source)) return;
        if (_openItem.IsPointerInsideFlyout(source)) return;

        CloseAll();
    }

    private static bool IsDescendant(DependencyObject ancestor, DependencyObject node)
    {
        var current = node;
        while (current != null)
        {
            if (ReferenceEquals(current, ancestor))
                return true;
            current = System.Windows.Media.VisualTreeHelper.GetParent(current)
                       ?? (current as FrameworkElement)?.Parent;
        }
        return false;
    }

    // ── Items ─────────────────────────────────────────────────────────
    public static readonly DependencyProperty ItemsProperty =
        DependencyProperty.Register(nameof(Items), typeof(ObservableCollection<MenuBarItem>), typeof(MenuBar),
            new PropertyMetadata(null, OnItemsChanged));

    /// <summary>菜单栏项集合。</summary>
    public ObservableCollection<MenuBarItem> Items
    {
        get => (ObservableCollection<MenuBarItem>)GetValue(ItemsProperty);
        set => SetValue(ItemsProperty, value);
    }

    private static void OnItemsChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        var bar = (MenuBar)d;
        if (e.OldValue is ObservableCollection<MenuBarItem> oldItems)
            oldItems.CollectionChanged -= bar.OnItemsCollectionChanged;
        if (e.NewValue is ObservableCollection<MenuBarItem> newItems)
            newItems.CollectionChanged += bar.OnItemsCollectionChanged;
        bar.PopulateItems();
    }

    private void OnItemsCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
        => PopulateItems();

    // ── 模板应用 ──────────────────────────────────────────────────────
    public override void OnApplyTemplate()
    {
        base.OnApplyTemplate();
        _itemsPanel = GetTemplateChild(PartItemsPanel) as Panel;
        PopulateItems();
    }

    private void PopulateItems()
    {
        if (_itemsPanel == null) return;
        _itemsPanel.Children.Clear();

        foreach (var item in Items)
        {
            item.OwnerMenuBar = this;
            _itemsPanel.Children.Add(item);
        }
    }

    /// <summary>由 <see cref="MenuBarItem"/> 调用，通知菜单栏其展开状态发生变化。</summary>
    internal void NotifyItemOpenChanged(MenuBarItem item, bool isOpen)
    {
        if (isOpen)
        {
            if (_openItem != null && _openItem != item)
                _openItem.IsSubMenuOpen = false;
            _openItem = item;
        }
        else if (_openItem == item)
        {
            _openItem = null;
        }
    }

    /// <summary>当前是否有任意菜单项处于展开状态（供其它项判断是否需要响应悬停切换）。</summary>
    internal bool IsAnyItemOpen => _openItem != null;

    /// <summary>关闭所有已展开的菜单项。</summary>
    public void CloseAll()
    {
        _openItem?.SetCurrentValue(MenuBarItem.IsSubMenuOpenProperty, false);
        _openItem = null;
    }
}

/// <summary>
/// MenuBar 中的单个菜单栏项（例如"文件"），点击展开下拉菜单。
/// </summary>
[TemplatePart(Name = PartMenuFlyout, Type = typeof(MenuFlyout))]
public class MenuBarItem : ButtonBase
{
    private const string PartMenuFlyout = "PART_MenuFlyout";

    private MenuFlyout? _menuFlyout;

    internal MenuBar? OwnerMenuBar { get; set; }

    static MenuBarItem()
    {
        DefaultStyleKeyProperty.OverrideMetadata(
            typeof(MenuBarItem),
            new FrameworkPropertyMetadata(typeof(MenuBarItem)));
    }

    public MenuBarItem()
    {
        Items = [];
        Focusable = true;
        MouseEnter += OnMouseEnter;
        PreviewMouseLeftButtonDown += OnPreviewMouseLeftButtonDown;
    }

    // ── Header ────────────────────────────────────────────────────────
    public static readonly DependencyProperty HeaderProperty =
        DependencyProperty.Register(nameof(Header), typeof(string), typeof(MenuBarItem),
            new PropertyMetadata(string.Empty));

    /// <summary>菜单栏项标题文本，例如"文件"。</summary>
    public string Header
    {
        get => (string)GetValue(HeaderProperty);
        set => SetValue(HeaderProperty, value);
    }

    // ── Items ─────────────────────────────────────────────────────────
    public static readonly DependencyProperty ItemsProperty =
        DependencyProperty.Register(nameof(Items), typeof(ObservableCollection<MenuFlyoutItemBase>), typeof(MenuBarItem),
            new PropertyMetadata(null, OnItemsChanged));

    /// <summary>下拉菜单项集合。</summary>
    public ObservableCollection<MenuFlyoutItemBase> Items
    {
        get => (ObservableCollection<MenuFlyoutItemBase>)GetValue(ItemsProperty);
        set => SetValue(ItemsProperty, value);
    }

    private static void OnItemsChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        var menuBarItem = (MenuBarItem)d;
        if (e.OldValue is ObservableCollection<MenuFlyoutItemBase> oldItems)
            oldItems.CollectionChanged -= menuBarItem.OnItemsCollectionChanged;
        if (e.NewValue is ObservableCollection<MenuFlyoutItemBase> newItems)
            newItems.CollectionChanged += menuBarItem.OnItemsCollectionChanged;
        menuBarItem.SyncMenuFlyoutItems();
    }

    private void OnItemsCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
        => SyncMenuFlyoutItems();

    // ── IsSubMenuOpen ─────────────────────────────────────────────────
    public static readonly DependencyProperty IsSubMenuOpenProperty =
        DependencyProperty.Register(nameof(IsSubMenuOpen), typeof(bool), typeof(MenuBarItem),
            new PropertyMetadata(false, OnIsSubMenuOpenChanged));

    /// <summary>下拉菜单是否展开。</summary>
    public bool IsSubMenuOpen
    {
        get => (bool)GetValue(IsSubMenuOpenProperty);
        set => SetValue(IsSubMenuOpenProperty, value);
    }

    private static void OnIsSubMenuOpenChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        var item = (MenuBarItem)d;
        var isOpen = (bool)e.NewValue;
        if (item._menuFlyout != null)
            item._menuFlyout.IsOpen = isOpen;
        item.OwnerMenuBar?.NotifyItemOpenChanged(item, isOpen);
    }

    // ── 模板应用 ──────────────────────────────────────────────────────
    private static readonly DependencyPropertyDescriptor MenuIsOpenDescriptor =
        DependencyPropertyDescriptor.FromProperty(MenuFlyout.IsOpenProperty, typeof(MenuFlyout));

    public override void OnApplyTemplate()
    {
        if (_menuFlyout != null)
            MenuIsOpenDescriptor.RemoveValueChanged(_menuFlyout, OnMenuFlyoutIsOpenChanged);

        base.OnApplyTemplate();

        _menuFlyout = GetTemplateChild(PartMenuFlyout) as MenuFlyout;
        if (_menuFlyout != null)
        {
            _menuFlyout.PlacementTarget = this;
            _menuFlyout.StaysOpen = true;
            MenuIsOpenDescriptor.AddValueChanged(_menuFlyout, OnMenuFlyoutIsOpenChanged);
        }

        SyncMenuFlyoutItems();
    }

    private void OnMenuFlyoutIsOpenChanged(object? sender, EventArgs e)
    {
        if (_menuFlyout != null)
            SetCurrentValue(IsSubMenuOpenProperty, _menuFlyout.IsOpen);
    }

    private void SyncMenuFlyoutItems()
    {
        if (_menuFlyout == null) return;
        _menuFlyout.Items.Clear();
        foreach (var item in Items)
            _menuFlyout.Items.Add(item);
    }

    protected override void OnClick()
    {
        base.OnClick();
    }

    private void OnPreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        IsSubMenuOpen = !IsSubMenuOpen;
        e.Handled = true;
    }

    private void OnMouseEnter(object sender, MouseEventArgs e)
    {
        // 若菜单栏内已有其它项展开，则悬停即切换到当前项。
        if (OwnerMenuBar is { IsAnyItemOpen: true } && !IsSubMenuOpen)
            IsSubMenuOpen = true;
    }

    /// <summary>判断给定元素是否位于本项下拉菜单的弹出内容内。</summary>
    internal bool IsPointerInsideFlyout(DependencyObject? element)
        => _menuFlyout?.ContainsElement(element) == true;
}
