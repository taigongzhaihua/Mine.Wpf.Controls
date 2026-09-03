using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Threading;

namespace Mine.Wpf.Controls.Controls;

/// <summary>
/// Material 3 菜单：轻量级弹出式菜单，支持图标、快捷键提示、分隔符与子菜单。
/// 支持鼠标与键盘（方向键/Enter/Esc）操作。
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
    private readonly List<MenuFlyoutSubItem> _openSubItems = [];

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
        menu.PopulateItems();
    }

    private void OnItemsCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
        => PopulateItems();

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
            menu._isAnimatingClose = false;
            menu._popup.IsOpen = true;
        }
        else
        {
            menu.CloseAllSubMenus();
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

    // ── MenuMinWidth ──────────────────────────────────────────────────
    public static readonly DependencyProperty MenuMinWidthProperty =
        DependencyProperty.Register(nameof(MenuMinWidth), typeof(double), typeof(MenuFlyout),
            new PropertyMetadata(112.0));

    /// <summary>菜单的最小宽度。</summary>
    public double MenuMinWidth
    {
        get => (double)GetValue(MenuMinWidthProperty);
        set => SetValue(MenuMinWidthProperty, value);
    }

    // ── StaysOpen ─────────────────────────────────────────────────────
    public static readonly DependencyProperty StaysOpenProperty =
        DependencyProperty.Register(nameof(StaysOpen), typeof(bool), typeof(MenuFlyout),
            new PropertyMetadata(false, OnStaysOpenChanged));

    /// <summary>
    /// 是否保持打开状态（不随点击外部/失去激活自动关闭）。
    /// 默认 <see langword="false"/>：点击菜单外部会自动关闭（适用于独立菜单/上下文菜单场景）。
    /// 设为 <see langword="true"/> 时需要宿主（如 MenuBar）自行处理外部点击关闭逻辑。
    /// </summary>
    public bool StaysOpen
    {
        get => (bool)GetValue(StaysOpenProperty);
        set => SetValue(StaysOpenProperty, value);
    }

    private static void OnStaysOpenChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is MenuFlyout { _popup: not null } menu)
            menu._popup.StaysOpen = (bool)e.NewValue;
    }

    // ── MenuMaxWidth ──────────────────────────────────────────────────
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
        if (_container != null)
            _container.PreviewKeyDown -= OnContainerPreviewKeyDown;

        _popup = GetTemplateChild(PartPopup) as Popup;
        _container = GetTemplateChild(PartContainer) as FrameworkElement;
        _itemsPanel = GetTemplateChild(PartItemsPanel) as Panel;

        if (_popup != null)
        {
            _popup.PlacementTarget = PlacementTarget;
            _popup.Placement = Placement;
            _popup.StaysOpen = StaysOpen;
            _popup.AllowsTransparency = true;
            _popup.Opened += OnPopupOpened;
            _popup.Closed += OnPopupClosed;
        }
        if (_container != null)
            _container.PreviewKeyDown += OnContainerPreviewKeyDown;

        PopulateItems();
    }

    // ── 内部方法 ──────────────────────────────────────────────────────
    private void PopulateItems()
    {
        if (_itemsPanel == null) return;
        _itemsPanel.Children.Clear();

        if (Items == null) return;

        foreach (var item in Items)
        {
            item.RootMenu = this;
            item.ParentSubItem = null;
            _itemsPanel.Children.Add(item);
        }
    }

    private void OnPopupOpened(object? sender, EventArgs e)
    {
        AnimateOpen();
        FocusFirstItem();
    }

    private void AnimateOpen()
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
            if (_popup != null)
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
        CloseAllSubMenus();
        if (_container != null)
        {
            _container.RenderTransform = null;
            _container.Opacity = 1;
        }
        _isAnimatingClose = false;

        if (IsOpen)
            IsOpen = false;
    }

    private void OnContainerPreviewKeyDown(object sender, KeyEventArgs e)
    {
        switch (e.Key)
        {
            case Key.Escape:
                CloseMenu();
                e.Handled = true;
                break;
            case Key.Up:
                MoveFocus(-1);
                e.Handled = true;
                break;
            case Key.Down:
                MoveFocus(1);
                e.Handled = true;
                break;
        }
    }

    private void FocusFirstItem()
    {
        if (_itemsPanel == null) return;
        foreach (var child in _itemsPanel.Children)
        {
            if (child is MenuFlyoutItemBase { IsEnabled: true, Focusable: true } item and not MenuFlyoutSeparator)
            {
                item.Focus();
                return;
            }
        }
    }

    private void MoveFocus(int direction)
    {
        if (_itemsPanel == null) return;
        var focusable = _itemsPanel.Children.OfType<MenuFlyoutItemBase>()
            .Where(i => i is not MenuFlyoutSeparator && i.IsEnabled && i.Focusable)
            .ToList();
        if (focusable.Count == 0) return;

        var currentIndex = focusable.FindIndex(i => i.IsKeyboardFocusWithin);
        var nextIndex = currentIndex < 0
            ? (direction > 0 ? 0 : focusable.Count - 1)
            : (currentIndex + direction + focusable.Count) % focusable.Count;

        focusable[nextIndex].Focus();
    }

    internal void RegisterOpenSubItem(MenuFlyoutSubItem subItem)
    {
        if (!_openSubItems.Contains(subItem))
            _openSubItems.Add(subItem);
    }

    internal void UnregisterOpenSubItem(MenuFlyoutSubItem subItem)
        => _openSubItems.Remove(subItem);

    private void CloseAllSubMenus()
    {
        for (var i = _openSubItems.Count - 1; i >= 0; i--)
            _openSubItems[i].IsSubMenuOpen = false;
        _openSubItems.Clear();
    }

    /// <summary>关闭菜单（含所有已展开的子菜单）。</summary>
    public void CloseMenu() => IsOpen = false;

    public void Show() => IsOpen = true;

    /// <summary>判断给定元素是否位于本菜单弹出内容的可视化树内（Popup 内容独立于宿主树）。</summary>
    public bool ContainsElement(DependencyObject? element)
    {
        if (element == null || _container == null) return false;
        var current = element;
        while (current != null)
        {
            if (ReferenceEquals(current, _container))
                return true;
            current = System.Windows.Media.VisualTreeHelper.GetParent(current)
                       ?? (current as FrameworkElement)?.Parent;
        }
        return false;
    }
}

/// <summary>
/// MenuFlyout 项的基类。
/// </summary>
public abstract class MenuFlyoutItemBase : Control
{
    internal MenuFlyout? RootMenu { get; set; }
    internal MenuFlyoutSubItem? ParentSubItem { get; set; }
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
        Focusable = true;
        MouseLeftButtonUp += OnMouseLeftButtonUp;
        KeyDown += OnKeyDown;
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

    // ── InputGestureText ──────────────────────────────────────────────
    public static readonly DependencyProperty InputGestureTextProperty =
        DependencyProperty.Register(nameof(InputGestureText), typeof(string), typeof(MenuFlyoutItem),
            new PropertyMetadata(string.Empty));

    /// <summary>快捷键提示文本（仅展示，不执行实际按键绑定）。</summary>
    public string InputGestureText
    {
        get => (string)GetValue(InputGestureTextProperty);
        set => SetValue(InputGestureTextProperty, value);
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
        Activate();
        e.Handled = true;
    }

    private void OnKeyDown(object sender, KeyEventArgs e)
    {
        if (e.Key is not (Key.Enter or Key.Space)) return;
        Activate();
        e.Handled = true;
    }

    private void Activate()
    {
        if (!IsEnabled) return;

        RaiseEvent(new RoutedEventArgs(ClickEvent, this));
        if (Command?.CanExecute(CommandParameter) == true)
            Command.Execute(CommandParameter);
        RootMenu?.CloseMenu();
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

    public MenuFlyoutSeparator()
    {
        Focusable = false;
        IsHitTestVisible = false;
    }
}

/// <summary>
/// MenuFlyout 子菜单项，鼠标悬停或按右方向键展开子菜单。
/// </summary>
[TemplatePart(Name = PartSubMenuPopup, Type = typeof(Popup))]
[TemplatePart(Name = PartSubItemsPanel, Type = typeof(Panel))]
public class MenuFlyoutSubItem : MenuFlyoutItemBase
{
    private const string PartSubMenuPopup = "PART_SubMenuPopup";
    private const string PartSubItemsPanel = "PART_SubItemsPanel";

    private static readonly TimeSpan OpenDelay = TimeSpan.FromMilliseconds(150);
    private static readonly TimeSpan CloseDelay = TimeSpan.FromMilliseconds(250);

    private Popup? _subMenuPopup;
    private Panel? _subItemsPanel;
    private DispatcherTimer? _openTimer;
    private DispatcherTimer? _closeTimer;

    static MenuFlyoutSubItem()
    {
        DefaultStyleKeyProperty.OverrideMetadata(
            typeof(MenuFlyoutSubItem),
            new FrameworkPropertyMetadata(typeof(MenuFlyoutSubItem)));
    }

    public MenuFlyoutSubItem()
    {
        Focusable = true;
        Items = [];
        MouseEnter += (_, _) => ScheduleOpen();
        MouseLeave += (_, _) => ScheduleClose();
        MouseLeftButtonUp += (_, e) => { OpenSubMenu(); e.Handled = true; };
        KeyDown += OnKeyDown;
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
        subItem.PopulateItems();
    }

    private void OnItemsCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
        => PopulateItems();

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
        var isOpen = (bool)e.NewValue;
        if (subItem._subMenuPopup != null)
            subItem._subMenuPopup.IsOpen = isOpen;

        if (isOpen)
            subItem.RootMenu?.RegisterOpenSubItem(subItem);
        else
            subItem.RootMenu?.UnregisterOpenSubItem(subItem);
    }

    public override void OnApplyTemplate()
    {
        base.OnApplyTemplate();
        _subMenuPopup = GetTemplateChild(PartSubMenuPopup) as Popup;
        _subItemsPanel = GetTemplateChild(PartSubItemsPanel) as Panel;
        PopulateItems();
    }

    private void PopulateItems()
    {
        if (_subItemsPanel == null) return;
        _subItemsPanel.Children.Clear();

        foreach (var item in Items)
        {
            item.RootMenu = RootMenu;
            item.ParentSubItem = this;
            _subItemsPanel.Children.Add(item);
        }
    }

    private void ScheduleOpen()
    {
        _closeTimer?.Stop();
        if (IsSubMenuOpen) return;

        _openTimer ??= new DispatcherTimer { Interval = OpenDelay };
        _openTimer.Tick -= OnOpenTimerTick;
        _openTimer.Tick += OnOpenTimerTick;
        _openTimer.Start();
    }

    private void OnOpenTimerTick(object? sender, EventArgs e)
    {
        _openTimer?.Stop();
        OpenSubMenu();
    }

    private void ScheduleClose()
    {
        _openTimer?.Stop();

        _closeTimer ??= new DispatcherTimer { Interval = CloseDelay };
        _closeTimer.Tick -= OnCloseTimerTick;
        _closeTimer.Tick += OnCloseTimerTick;
        _closeTimer.Start();
    }

    private void OnCloseTimerTick(object? sender, EventArgs e)
    {
        _closeTimer?.Stop();
        if (!IsMouseOver && (_subMenuPopup?.Child is not { IsMouseOver: true }))
            IsSubMenuOpen = false;
    }

    private void OpenSubMenu()
    {
        if (Items.Count == 0) return;
        IsSubMenuOpen = true;
    }

    private void OnKeyDown(object sender, KeyEventArgs e)
    {
        switch (e.Key)
        {
            case Key.Right or Key.Enter:
                OpenSubMenu();
                FocusFirstChild();
                e.Handled = true;
                break;
            case Key.Left:
                IsSubMenuOpen = false;
                Focus();
                e.Handled = true;
                break;
        }
    }

    private void FocusFirstChild()
    {
        if (_subItemsPanel == null) return;
        foreach (var child in _subItemsPanel.Children)
        {
            if (child is MenuFlyoutItemBase { IsEnabled: true, Focusable: true } item and not MenuFlyoutSeparator)
            {
                item.Focus();
                return;
            }
        }
    }
}
