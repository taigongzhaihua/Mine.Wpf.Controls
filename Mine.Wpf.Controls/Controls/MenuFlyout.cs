using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Collections.ObjectModel;

namespace Mine.Wpf.Controls.Controls;

/// <summary>
/// Material 3 MenuFlyout：轻量级上下文菜单，支持图标、快捷键、分隔符和子菜单。
/// </summary>
[TemplatePart(Name = PartPopup, Type = typeof(Popup))]
[TemplatePart(Name = PartContainer, Type = typeof(FrameworkElement))]
[TemplatePart(Name = PartItemsPanel, Type = typeof(Panel))]
public class MenuFlyout : Control
{
    private const string PartPopup = "PART_Popup";
    private const string PartContainer = "PART_Container";
    private const string PartItemsPanel = "PART_ItemsPanel";

    private Popup? _popup;
    private FrameworkElement? _container;
    private Panel? _itemsPanel;
    private bool _isAnimatingClose;

    static MenuFlyout()
    {
        DefaultStyleKeyProperty.OverrideMetadata(
            typeof(MenuFlyout),
            new FrameworkPropertyMetadata(typeof(MenuFlyout)));
    }

    public MenuFlyout()
    {
        Items = [];
    }

    // ── Items ─────────────────────────────────────────────────────────
    public static readonly DependencyProperty ItemsProperty =
        DependencyProperty.Register(nameof(Items), typeof(ObservableCollection<MenuFlyoutItemBase>), typeof(MenuFlyout),
            new PropertyMetadata(null, OnItemsChanged));

    /// <summary>菜单项集合。</summary>
    public ObservableCollection<MenuFlyoutItemBase> Items
    {
        get => (ObservableCollection<MenuFlyoutItemBase>)GetValue(ItemsProperty);
        set => SetValue(ItemsProperty, value);
    }

    private static void OnItemsChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        var menu = (MenuFlyout)d;
        if (e.OldValue is ObservableCollection<MenuFlyoutItemBase> oldItems)
            oldItems.CollectionChanged -= menu.OnItemsCollectionChanged;
        if (e.NewValue is ObservableCollection<MenuFlyoutItemBase> newItems)
            newItems.CollectionChanged += menu.OnItemsCollectionChanged;
        menu.UpdateItems();
    }

    private void OnItemsCollectionChanged(object? sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        => UpdateItems();

    // ── IsOpen ────────────────────────────────────────────────────────
    public static readonly DependencyProperty IsOpenProperty =
        DependencyProperty.Register(nameof(IsOpen), typeof(bool), typeof(MenuFlyout),
            new FrameworkPropertyMetadata(false, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, OnIsOpenChanged));

    /// <summary>菜单是否打开（双向绑定）。</summary>
    public bool IsOpen
    {
        get => (bool)GetValue(IsOpenProperty);
        set => SetValue(IsOpenProperty, value);
    }

    private static void OnIsOpenChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        var menu = (MenuFlyout)d;
        if (menu._popup == null)
            menu.ApplyTemplate();
        if (menu._popup == null)
            return;

        if ((bool)e.NewValue)
        {
            menu._popup.IsOpen = true;
        }
        else
        {
            menu.BeginCloseAnimation();
        }
    }

    // ── PlacementTarget ───────────────────────────────────────────────
    public static readonly DependencyProperty PlacementTargetProperty =
        DependencyProperty.Register(nameof(PlacementTarget), typeof(UIElement), typeof(MenuFlyout),
            new PropertyMetadata(null, OnPlacementTargetChanged));

    /// <summary>菜单的相对定位目标元素。</summary>
    public UIElement? PlacementTarget
    {
        get => (UIElement?)GetValue(PlacementTargetProperty);
        set => SetValue(PlacementTargetProperty, value);
    }

    private static void OnPlacementTargetChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is MenuFlyout { _popup: not null } menu)
            menu._popup.PlacementTarget = e.NewValue as UIElement;
    }

    // ── Placement ─────────────────────────────────────────────────────
    public static readonly DependencyProperty PlacementProperty =
        DependencyProperty.Register(nameof(Placement), typeof(PlacementMode), typeof(MenuFlyout),
            new PropertyMetadata(PlacementMode.Bottom, OnPlacementChanged));

    /// <summary>菜单的放置模式。</summary>
    public PlacementMode Placement
    {
        get => (PlacementMode)GetValue(PlacementProperty);
        set => SetValue(PlacementProperty, value);
    }

    private static void OnPlacementChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is MenuFlyout { _popup: not null } menu)
            menu._popup.Placement = (PlacementMode)e.NewValue;
    }

    // ── MinWidth ──────────────────────────────────────────────────────
    public static readonly DependencyProperty MenuMinWidthProperty =
        DependencyProperty.Register(nameof(MenuMinWidth), typeof(double), typeof(MenuFlyout),
            new PropertyMetadata(112.0));

    /// <summary>菜单的最小宽度。</summary>
    public double MenuMinWidth
    {
        get => (double)GetValue(MenuMinWidthProperty);
        set => SetValue(MenuMinWidthProperty, value);
    }

    // ── MaxWidth ──────────────────────────────────────────────────────
    public static readonly DependencyProperty MenuMaxWidthProperty =
        DependencyProperty.Register(nameof(MenuMaxWidth), typeof(double), typeof(MenuFlyout),
            new PropertyMetadata(280.0));

    /// <summary>菜单的最大宽度。</summary>
    public double MenuMaxWidth
    {
        get => (double)GetValue(MenuMaxWidthProperty);
        set => SetValue(MenuMaxWidthProperty, value);
    }

    // ── 模板应用 ──────────────────────────────────────────────────────
    public override void OnApplyTemplate()
    {
        base.OnApplyTemplate();

        if (_popup != null)
        {
            _popup.Opened -= OnPopupOpened;
            _popup.Closed -= OnPopupClosed;
        }

        _popup = GetTemplateChild(PartPopup) as Popup;
        _container = GetTemplateChild(PartContainer) as FrameworkElement;
        _itemsPanel = GetTemplateChild(PartItemsPanel) as Panel;

        if (_popup != null)
        {
            _popup.PlacementTarget = PlacementTarget;
            _popup.Placement = Placement;
            _popup.StaysOpen = false;
            _popup.AllowsTransparency = true;
            _popup.Opened += OnPopupOpened;
            _popup.Closed += OnPopupClosed;
        }

        UpdateItems();
    }

    // ── 内部方法 ──────────────────────────────────────────────────────
    private void UpdateItems()
    {
        if (_itemsPanel == null) return;
        _itemsPanel.Children.Clear();

        foreach (var item in Items)
        {
            item.ParentMenu = this;
            _itemsPanel.Children.Add(item);
        }
    }

    private void OnPopupOpened(object? sender, EventArgs e)
    {
        if (_container == null) return;

        _container.Opacity = 0;
        _container.RenderTransform = new ScaleTransform(0.9, 0.9);
        _container.RenderTransformOrigin = new Point(0, 0);

        var ease = new CubicEase { EasingMode = EasingMode.EaseOut };
        var duration = new Duration(TimeSpan.FromMilliseconds(150));

        _container.BeginAnimation(OpacityProperty, 
            new DoubleAnimation(0, 1, duration) { EasingFunction = ease });

        if (_container.RenderTransform is ScaleTransform scale)
        {
            scale.BeginAnimation(ScaleTransform.ScaleXProperty, 
                new DoubleAnimation(0.9, 1.0, duration) { EasingFunction = ease });
            scale.BeginAnimation(ScaleTransform.ScaleYProperty, 
                new DoubleAnimation(0.9, 1.0, duration) { EasingFunction = ease });
        }
    }

    private void BeginCloseAnimation()
    {
        if (_container == null || _popup == null)
        {
            _popup?.SetCurrentValue(Popup.IsOpenProperty, false);
            return;
        }

        if (_isAnimatingClose) return;
        _isAnimatingClose = true;

        var ease = new CubicEase { EasingMode = EasingMode.EaseIn };
        var duration = new Duration(TimeSpan.FromMilliseconds(100));

        var fadeOut = new DoubleAnimation(1, 0, duration) { EasingFunction = ease };
        fadeOut.Completed += (_, _) =>
        {
            _popup.IsOpen = false;
            _isAnimatingClose = false;
        };

        _container.BeginAnimation(OpacityProperty, fadeOut);

        if (_container.RenderTransform is ScaleTransform scale)
        {
            scale.BeginAnimation(ScaleTransform.ScaleXProperty, 
                new DoubleAnimation(1.0, 0.9, duration) { EasingFunction = ease });
            scale.BeginAnimation(ScaleTransform.ScaleYProperty, 
                new DoubleAnimation(1.0, 0.9, duration) { EasingFunction = ease });
        }
    }

    private void OnPopupClosed(object? sender, EventArgs e)
    {
        if (_container != null)
        {
            _container.RenderTransform = null;
            _container.Opacity = 1;
        }
        _isAnimatingClose = false;
    }

    internal void CloseMenu()
    {
        IsOpen = false;
    }
}

/// <summary>
/// MenuFlyout 项的基类。
/// </summary>
public abstract class MenuFlyoutItemBase : Control
{
    internal MenuFlyout? ParentMenu { get; set; }
}

/// <summary>
/// MenuFlyout 菜单项。
/// </summary>
public class MenuFlyoutItem : MenuFlyoutItemBase
{
    static MenuFlyoutItem()
    {
        DefaultStyleKeyProperty.OverrideMetadata(
            typeof(MenuFlyoutItem),
            new FrameworkPropertyMetadata(typeof(MenuFlyoutItem)));
    }

    public MenuFlyoutItem()
    {
        MouseLeftButtonUp += OnMouseLeftButtonUp;
    }

    // ── Text ──────────────────────────────────────────────────────────
    public static readonly DependencyProperty TextProperty =
        DependencyProperty.Register(nameof(Text), typeof(string), typeof(MenuFlyoutItem),
            new PropertyMetadata(string.Empty));

    /// <summary>菜单项文本。</summary>
    public string Text
    {
        get => (string)GetValue(TextProperty);
        set => SetValue(TextProperty, value);
    }

    // ── Icon ──────────────────────────────────────────────────────────
    public static readonly DependencyProperty IconProperty =
        DependencyProperty.Register(nameof(Icon), typeof(object), typeof(MenuFlyoutItem),
            new PropertyMetadata(null));

    /// <summary>菜单项图标（支持字符串或 UIElement）。</summary>
    public object? Icon
    {
        get => GetValue(IconProperty);
        set => SetValue(IconProperty, value);
    }

    // ── Command ───────────────────────────────────────────────────────
    public static readonly DependencyProperty CommandProperty =
        DependencyProperty.Register(nameof(Command), typeof(ICommand), typeof(MenuFlyoutItem),
            new PropertyMetadata(null));

    /// <summary>菜单项命令。</summary>
    public ICommand? Command
    {
        get => (ICommand?)GetValue(CommandProperty);
        set => SetValue(CommandProperty, value);
    }

    // ── CommandParameter ──────────────────────────────────────────────
    public static readonly DependencyProperty CommandParameterProperty =
        DependencyProperty.Register(nameof(CommandParameter), typeof(object), typeof(MenuFlyoutItem),
            new PropertyMetadata(null));

    /// <summary>命令参数。</summary>
    public object? CommandParameter
    {
        get => GetValue(CommandParameterProperty);
        set => SetValue(CommandParameterProperty, value);
    }

    // ── KeyboardAcceleratorTextOverride ───────────────────────────────
    public static readonly DependencyProperty KeyboardAcceleratorTextOverrideProperty =
        DependencyProperty.Register(nameof(KeyboardAcceleratorTextOverride), typeof(string), typeof(MenuFlyoutItem),
            new PropertyMetadata(string.Empty));

    /// <summary>快捷键文本（显示用，不执行实际绑定）。</summary>
    public string KeyboardAcceleratorTextOverride
    {
        get => (string)GetValue(KeyboardAcceleratorTextOverrideProperty);
        set => SetValue(KeyboardAcceleratorTextOverrideProperty, value);
    }

    // ── IsEnabled ─────────────────────────────────────────────────────
    public static new readonly DependencyProperty IsEnabledProperty =
        DependencyProperty.Register(nameof(IsEnabled), typeof(bool), typeof(MenuFlyoutItem),
            new PropertyMetadata(true));

    /// <summary>菜单项是否启用。</summary>
    public new bool IsEnabled
    {
        get => (bool)GetValue(IsEnabledProperty);
        set => SetValue(IsEnabledProperty, value);
    }

    // ── Click 事件 ────────────────────────────────────────────────────
    public static readonly RoutedEvent ClickEvent =
        EventManager.RegisterRoutedEvent(nameof(Click), RoutingStrategy.Bubble,
            typeof(RoutedEventHandler), typeof(MenuFlyoutItem));

    /// <summary>菜单项点击事件。</summary>
    public event RoutedEventHandler Click
    {
        add => AddHandler(ClickEvent, value);
        remove => RemoveHandler(ClickEvent, value);
    }

    private void OnMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
    {
        if (!IsEnabled) return;

        RaiseEvent(new RoutedEventArgs(ClickEvent, this));
        Command?.Execute(CommandParameter);
        ParentMenu?.CloseMenu();
    }
}

/// <summary>
/// MenuFlyout 分隔符。
/// </summary>
public class MenuFlyoutSeparator : MenuFlyoutItemBase
{
    static MenuFlyoutSeparator()
    {
        DefaultStyleKeyProperty.OverrideMetadata(
            typeof(MenuFlyoutSeparator),
            new FrameworkPropertyMetadata(typeof(MenuFlyoutSeparator)));
    }
}

/// <summary>
/// MenuFlyout 子菜单项。
/// </summary>
public class MenuFlyoutSubItem : MenuFlyoutItemBase
{
    private Popup? _subMenuPopup;
    private Panel? _subItemsPanel;

    static MenuFlyoutSubItem()
    {
        DefaultStyleKeyProperty.OverrideMetadata(
            typeof(MenuFlyoutSubItem),
            new FrameworkPropertyMetadata(typeof(MenuFlyoutSubItem)));
    }

    public MenuFlyoutSubItem()
    {
        Items = [];
        MouseEnter += OnMouseEnter;
        MouseLeave += OnMouseLeave;
    }

    // ── Text ──────────────────────────────────────────────────────────
    public static readonly DependencyProperty TextProperty =
        DependencyProperty.Register(nameof(Text), typeof(string), typeof(MenuFlyoutSubItem),
            new PropertyMetadata(string.Empty));

    /// <summary>子菜单项文本。</summary>
    public string Text
    {
        get => (string)GetValue(TextProperty);
        set => SetValue(TextProperty, value);
    }

    // ── Icon ──────────────────────────────────────────────────────────
    public static readonly DependencyProperty IconProperty =
        DependencyProperty.Register(nameof(Icon), typeof(object), typeof(MenuFlyoutSubItem),
            new PropertyMetadata(null));

    /// <summary>子菜单项图标（支持字符串或 UIElement）。</summary>
    public object? Icon
    {
        get => GetValue(IconProperty);
        set => SetValue(IconProperty, value);
    }

    // ── Items ─────────────────────────────────────────────────────────
    public static readonly DependencyProperty ItemsProperty =
        DependencyProperty.Register(nameof(Items), typeof(ObservableCollection<MenuFlyoutItemBase>), typeof(MenuFlyoutSubItem),
            new PropertyMetadata(null, OnItemsChanged));

    /// <summary>子菜单项集合。</summary>
    public ObservableCollection<MenuFlyoutItemBase> Items
    {
        get => (ObservableCollection<MenuFlyoutItemBase>)GetValue(ItemsProperty);
        set => SetValue(ItemsProperty, value);
    }

    private static void OnItemsChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        var subItem = (MenuFlyoutSubItem)d;
        if (e.OldValue is ObservableCollection<MenuFlyoutItemBase> oldItems)
            oldItems.CollectionChanged -= subItem.OnItemsCollectionChanged;
        if (e.NewValue is ObservableCollection<MenuFlyoutItemBase> newItems)
            newItems.CollectionChanged += subItem.OnItemsCollectionChanged;
        subItem.UpdateItems();
    }

    private void OnItemsCollectionChanged(object? sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        => UpdateItems();

    // ── IsSubMenuOpen ─────────────────────────────────────────────────
    public static readonly DependencyProperty IsSubMenuOpenProperty =
        DependencyProperty.Register(nameof(IsSubMenuOpen), typeof(bool), typeof(MenuFlyoutSubItem),
            new PropertyMetadata(false, OnIsSubMenuOpenChanged));

    /// <summary>子菜单是否打开。</summary>
    public bool IsSubMenuOpen
    {
        get => (bool)GetValue(IsSubMenuOpenProperty);
        set => SetValue(IsSubMenuOpenProperty, value);
    }

    private static void OnIsSubMenuOpenChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        var subItem = (MenuFlyoutSubItem)d;
        if (subItem._subMenuPopup != null)
            subItem._subMenuPopup.IsOpen = (bool)e.NewValue;
    }

    public override void OnApplyTemplate()
    {
        base.OnApplyTemplate();
        _subMenuPopup = GetTemplateChild("PART_SubMenuPopup") as Popup;
        _subItemsPanel = GetTemplateChild("PART_SubItemsPanel") as Panel;
        UpdateItems();
    }

    private void UpdateItems()
    {
        if (_subItemsPanel == null) return;
        _subItemsPanel.Children.Clear();

        foreach (var item in Items)
        {
            item.ParentMenu = ParentMenu;
            _subItemsPanel.Children.Add(item);
        }
    }

    private void OnMouseEnter(object sender, MouseEventArgs e)
        => IsSubMenuOpen = true;

    private void OnMouseLeave(object sender, MouseEventArgs e)
        => IsSubMenuOpen = false;
}
