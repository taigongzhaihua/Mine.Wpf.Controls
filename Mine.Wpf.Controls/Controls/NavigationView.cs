using System.Collections;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Threading;

namespace Mine.Wpf.Controls.Controls;

/// <summary>
/// NavigationView：基于 SplitView 实现的左侧导航框架控件。
/// </summary>
/// <remarks>
/// DisplayMode 与 SplitView 的映射关系：
/// <list type="table">
///   <item><term>Left</term>            <description>SplitView.Inline        — 始终展开，并排显示</description></item>
///   <item><term>LeftCompact</term>     <description>SplitView.CompactOverlay — 图标条 + 叠加展开</description></item>
///   <item><term>LeftCompactInline</term><description>SplitView.CompactInline  — 图标条 + 并排推挤展开</description></item>
///   <item><term>LeftMinimal</term>     <description>SplitView.Overlay        — 完全隐藏 + 叠加展开</description></item>
///   <item><term>Auto</term>            <description>根据控件宽度自动在以上模式间切换</description></item>
/// </list>
/// </remarks>
[TemplatePart(Name = PartSplitView,        Type = typeof(SplitView))]
[TemplatePart(Name = PartMenuList,         Type = typeof(ListBox))]
[TemplatePart(Name = PartFooterList,       Type = typeof(ListBox))]
[TemplatePart(Name = PartPaneToggleButton, Type = typeof(ButtonBase))]
[TemplatePart(Name = PartAutoSuggestBox,   Type = typeof(SearchBox))]
public class NavigationView : ContentControl
{
    private const string PartSplitView        = "PART_SplitView";
    private const string PartMenuList         = "PART_MenuList";
    private const string PartFooterList       = "PART_FooterList";
    private const string PartPaneToggleButton = "PART_PaneToggleButton";
    private const string PartAutoSuggestBox   = "PART_AutoSuggestBox";

    /// <summary>Auto 模式宽度阈值（与 WinUI 一致）：≥1008 → Left。</summary>
    private const double ExpandedThreshold = 1008;
    /// <summary>Auto 模式宽度阈值（与 WinUI 一致）：≥641 → LeftCompact。</summary>
    private const double CompactThreshold  = 641;

    static NavigationView()
    {
        DefaultStyleKeyProperty.OverrideMetadata(
            typeof(NavigationView),
            new FrameworkPropertyMetadata(typeof(NavigationView)));
    }

    private SplitView?    _splitView;
    private ListBox?      _menuList;
    private ListBox?      _footerList;
    private ButtonBase?   _paneToggleButton;
    private SearchBox?    _autoSuggestBox;
    private DispatcherTimer? _layoutModeTimer;

    // SplitView.IsPaneOpen 的属性描述符，用于监听 Scrim 点击等外部关闭行为
    private static readonly DependencyPropertyDescriptor SplitViewIsPaneOpenDescriptor =
        DependencyPropertyDescriptor.FromProperty(SplitView.IsPaneOpenProperty, typeof(SplitView));

    // ════════════════════════════════════════════════════════════════════
    //  依赖属性
    // ════════════════════════════════════════════════════════════════════

    // ── MenuItemsSource ───────────────────────────────────────────────
    public static readonly DependencyProperty MenuItemsSourceProperty =
        DependencyProperty.Register(nameof(MenuItemsSource), typeof(IEnumerable), typeof(NavigationView),
            new PropertyMetadata(null, (d, e) =>
            {
                var nv = (NavigationView)d;
                if (nv._menuList != null) nv._menuList.ItemsSource = e.NewValue as IEnumerable;
            }));

    /// <summary>主导航列表的数据源。</summary>
    public IEnumerable? MenuItemsSource
    {
        get => (IEnumerable?)GetValue(MenuItemsSourceProperty);
        set => SetValue(MenuItemsSourceProperty, value);
    }

    // ── FooterMenuItemsSource ─────────────────────────────────────────
    public static readonly DependencyProperty FooterMenuItemsSourceProperty =
        DependencyProperty.Register(nameof(FooterMenuItemsSource), typeof(IEnumerable), typeof(NavigationView),
            new PropertyMetadata(null, (d, e) =>
            {
                var nv = (NavigationView)d;
                if (nv._footerList != null) nv._footerList.ItemsSource = e.NewValue as IEnumerable;
            }));

    /// <summary>底部 Footer 导航列表的数据源。</summary>
    public IEnumerable? FooterMenuItemsSource
    {
        get => (IEnumerable?)GetValue(FooterMenuItemsSourceProperty);
        set => SetValue(FooterMenuItemsSourceProperty, value);
    }

    // ── SelectedItem ──────────────────────────────────────────────────
    public static readonly DependencyProperty SelectedItemProperty =
        DependencyProperty.Register(nameof(SelectedItem), typeof(object), typeof(NavigationView),
            new FrameworkPropertyMetadata(null,
                FrameworkPropertyMetadataOptions.BindsTwoWayByDefault,
                (d, e) =>
                {
                    var nv = (NavigationView)d;
                    if (nv._menuList   != null) nv._menuList.SelectedItem   = e.NewValue;
                    if (nv._footerList != null) nv._footerList.SelectedItem = e.NewValue;
                }));

    /// <summary>当前选中的导航项（双向绑定）。</summary>
    public object? SelectedItem
    {
        get => GetValue(SelectedItemProperty);
        set => SetValue(SelectedItemProperty, value);
    }

    // ── DisplayMode ───────────────────────────────────────────────────
    public static readonly DependencyProperty DisplayModeProperty =
        DependencyProperty.Register(nameof(DisplayMode), typeof(NavigationViewDisplayMode), typeof(NavigationView),
            new PropertyMetadata(NavigationViewDisplayMode.Auto,
                (d, _) => ((NavigationView)d).ApplyDisplayMode()));

    /// <summary>导航面板的显示模式。</summary>
    public NavigationViewDisplayMode DisplayMode
    {
        get => (NavigationViewDisplayMode)GetValue(DisplayModeProperty);
        set => SetValue(DisplayModeProperty, value);
    }

    // ── IsPaneOpen ────────────────────────────────────────────────────
    public static readonly DependencyProperty IsPaneOpenProperty =
        DependencyProperty.Register(nameof(IsPaneOpen), typeof(bool), typeof(NavigationView),
            new FrameworkPropertyMetadata(true,
                FrameworkPropertyMetadataOptions.BindsTwoWayByDefault,
                (d, e) =>
                {
                    var nv = (NavigationView)d;
                    if (nv._splitView != null) nv._splitView.IsPaneOpen = (bool)e.NewValue;
                    nv.UpdateItemLayoutModes(animate: true);
                    nv.UpdateHeaderPadding();
                }));

    /// <summary>Pane 是否处于展开状态（双向绑定）。</summary>
    public bool IsPaneOpen
    {
        get => (bool)GetValue(IsPaneOpenProperty);
        set => SetValue(IsPaneOpenProperty, value);
    }

    // ── PaneTitle ─────────────────────────────────────────────────────
    public static readonly DependencyProperty PaneTitleProperty =
        DependencyProperty.Register(nameof(PaneTitle), typeof(string), typeof(NavigationView),
            new PropertyMetadata(null));

    /// <summary>Pane 顶部显示的应用名称（仅展开状态可见）。</summary>
    public string? PaneTitle
    {
        get => (string?)GetValue(PaneTitleProperty);
        set => SetValue(PaneTitleProperty, value);
    }

    // ── Header ────────────────────────────────────────────────────────
    public static readonly DependencyProperty HeaderProperty =
        DependencyProperty.Register(nameof(Header), typeof(object), typeof(NavigationView),
            new PropertyMetadata(null));

    /// <summary>内容区顶部标题，为 null 时自动隐藏标题行。</summary>
    public object? Header
    {
        get => GetValue(HeaderProperty);
        set => SetValue(HeaderProperty, value);
    }

    // ── OpenPaneLength ────────────────────────────────────────────────
    public static readonly DependencyProperty OpenPaneLengthProperty =
        DependencyProperty.Register(nameof(OpenPaneLength), typeof(double), typeof(NavigationView),
            new PropertyMetadata(260.0));

    /// <summary>Pane 完全展开时的宽度（px），默认 260。</summary>
    public double OpenPaneLength
    {
        get => (double)GetValue(OpenPaneLengthProperty);
        set => SetValue(OpenPaneLengthProperty, value);
    }

    // ── CompactPaneLength ─────────────────────────────────────────────
    public static readonly DependencyProperty CompactPaneLengthProperty =
        DependencyProperty.Register(nameof(CompactPaneLength), typeof(double), typeof(NavigationView),
            new PropertyMetadata(50.0));

    /// <summary>图标条模式下 Pane 的宽度（px），默认 48。</summary>
    public double CompactPaneLength
    {
        get => (double)GetValue(CompactPaneLengthProperty);
        set => SetValue(CompactPaneLengthProperty, value);
    }

    // ── IsPaneToggleButtonVisible ─────────────────────────────────────
    public static readonly DependencyProperty IsPaneToggleButtonVisibleProperty =
        DependencyProperty.Register(nameof(IsPaneToggleButtonVisible), typeof(bool), typeof(NavigationView),
            new PropertyMetadata(true, (d, e) =>
            {
                var nv = (NavigationView)d;
                if (nv._paneToggleButton != null)
                    nv._paneToggleButton.Visibility = (bool)e.NewValue
                        ? Visibility.Visible : Visibility.Collapsed;
                nv.UpdateHeaderPadding();
            }));

    /// <summary>是否显示汉堡开关按钮，默认 true。</summary>
    public bool IsPaneToggleButtonVisible
    {
        get => (bool)GetValue(IsPaneToggleButtonVisibleProperty);
        set => SetValue(IsPaneToggleButtonVisibleProperty, value);
    }

    // ── PaneFooter ────────────────────────────────────────────────────
    public static readonly DependencyProperty PaneFooterProperty =
        DependencyProperty.Register(nameof(PaneFooter), typeof(object), typeof(NavigationView),
            new PropertyMetadata(null));

    /// <summary>固定在 Pane 底部的自定义内容（Footer 列表下方）。</summary>
    public object? PaneFooter
    {
        get => GetValue(PaneFooterProperty);
        set => SetValue(PaneFooterProperty, value);
    }

    // ── IsSearchBoxVisible ────────────────────────────────────────────
    public static readonly DependencyProperty IsSearchBoxVisibleProperty =
        DependencyProperty.Register(nameof(IsSearchBoxVisible), typeof(bool), typeof(NavigationView),
            new PropertyMetadata(false, (d, e) =>
            {
                var nv = (NavigationView)d;
                if (nv._autoSuggestBox != null)
                    nv._autoSuggestBox.Visibility = (bool)e.NewValue
                        ? Visibility.Visible : Visibility.Collapsed;
            }));

    /// <summary>是否在 Pane 内显示搜索框（仅展开状态可见，Compact 时自动隐藏）。</summary>
    public bool IsSearchBoxVisible
    {
        get => (bool)GetValue(IsSearchBoxVisibleProperty);
        set => SetValue(IsSearchBoxVisibleProperty, value);
    }

    // ── AutoSuggestBoxText ────────────────────────────────────────────
    public static readonly DependencyProperty AutoSuggestBoxTextProperty =
        DependencyProperty.Register(nameof(AutoSuggestBoxText), typeof(string), typeof(NavigationView),
            new FrameworkPropertyMetadata(string.Empty,
                FrameworkPropertyMetadataOptions.BindsTwoWayByDefault,
                (d, e) =>
                {
                    var nv = (NavigationView)d;
                    if (nv._autoSuggestBox != null && nv._autoSuggestBox.Text != (string)e.NewValue)
                        nv._autoSuggestBox.Text = (string)e.NewValue ?? string.Empty;
                }));

    /// <summary>搜索框的文字内容（双向绑定）。</summary>
    public string AutoSuggestBoxText
    {
        get => (string)GetValue(AutoSuggestBoxTextProperty);
        set => SetValue(AutoSuggestBoxTextProperty, value);
    }

    // ── AutoSuggestBoxPlaceholder ─────────────────────────────────────
    public static readonly DependencyProperty AutoSuggestBoxPlaceholderProperty =
        DependencyProperty.Register(nameof(AutoSuggestBoxPlaceholder), typeof(string), typeof(NavigationView),
            new PropertyMetadata("搜索"));

    /// <summary>搜索框的占位符文字，默认"搜索"。</summary>
    public string AutoSuggestBoxPlaceholder
    {
        get => (string)GetValue(AutoSuggestBoxPlaceholderProperty);
        set => SetValue(AutoSuggestBoxPlaceholderProperty, value);
    }

    // ── AutoSuggestBoxSuggestionsSource ───────────────────────────────
    public static readonly DependencyProperty AutoSuggestBoxSuggestionsSourceProperty =
        DependencyProperty.Register(nameof(AutoSuggestBoxSuggestionsSource), typeof(IEnumerable), typeof(NavigationView),
            new PropertyMetadata(null, (d, e) =>
            {
                var nv = (NavigationView)d;
                if (nv._autoSuggestBox != null)
                    nv._autoSuggestBox.SuggestionsSource = e.NewValue as IEnumerable;
            }));

    /// <summary>搜索框的建议列表数据源。</summary>
    public IEnumerable? AutoSuggestBoxSuggestionsSource
    {
        get => (IEnumerable?)GetValue(AutoSuggestBoxSuggestionsSourceProperty);
        set => SetValue(AutoSuggestBoxSuggestionsSourceProperty, value);
    }

    // ── HeaderPadding（只读，由代码计算后写入，XAML 通过 TemplateBinding 消费）──
    public static readonly DependencyProperty HeaderPaddingProperty =
        DependencyProperty.Register(nameof(HeaderPadding), typeof(Thickness), typeof(NavigationView),
            new PropertyMetadata(default(Thickness)));

    /// <summary>
    /// Header 区域的 Padding，由控件根据当前模式自动计算。
    /// <br/>Overlay 模式下左侧加 CompactPaneLength 的偏移，避免被汉堡按钮遮挡。
    /// </summary>
    public Thickness HeaderPadding
    {
        get => (Thickness)GetValue(HeaderPaddingProperty);
        private set => SetValue(HeaderPaddingProperty, value);
    }

    // ════════════════════════════════════════════════════════════════════
    //  事件
    // ════════════════════════════════════════════════════════════════════

    public static readonly RoutedEvent SelectionChangedEvent =
        EventManager.RegisterRoutedEvent(nameof(SelectionChanged), RoutingStrategy.Bubble,
            typeof(SelectionChangedEventHandler), typeof(NavigationView));

    /// <summary>导航项选中变化时触发（冒泡路由事件）。</summary>
    public event SelectionChangedEventHandler SelectionChanged
    {
        add    => AddHandler(SelectionChangedEvent, value);
        remove => RemoveHandler(SelectionChangedEvent, value);
    }

    public static readonly RoutedEvent QuerySubmittedEvent =
        EventManager.RegisterRoutedEvent(nameof(QuerySubmitted), RoutingStrategy.Bubble,
            typeof(RoutedEventHandler), typeof(NavigationView));

    /// <summary>用户在搜索框中提交搜索（按 Enter 或点击搜索按钮）时触发。</summary>
    public event RoutedEventHandler QuerySubmitted
    {
        add    => AddHandler(QuerySubmittedEvent, value);
        remove => RemoveHandler(QuerySubmittedEvent, value);
    }

    // ════════════════════════════════════════════════════════════════════
    //  模板 & 生命周期
    // ════════════════════════════════════════════════════════════════════

    public override void OnApplyTemplate()
    {
        base.OnApplyTemplate();

        // 解绑旧事件
        if (_paneToggleButton != null) _paneToggleButton.Click      -= OnPaneToggleClick;
        if (_menuList         != null)
        {
            _menuList.SelectionChanged -= OnListSelectionChanged;
            _menuList.ItemContainerGenerator.StatusChanged -= OnMenuContainersGenerated;
        }
        if (_footerList != null)
        {
            _footerList.SelectionChanged -= OnListSelectionChanged;
            _footerList.ItemContainerGenerator.StatusChanged -= OnFooterContainersGenerated;
        }
        if (_autoSuggestBox != null)
        {
            _autoSuggestBox.TextChanged -= OnAutoSuggestBoxTextChanged;
            _autoSuggestBox.Search      -= OnAutoSuggestBoxSearch;
        }
        if (_splitView        != null)
        {
            _splitView.SizeChanged -= OnSplitViewSizeChanged;
            // 取消监听 SplitView.IsPaneOpen（Scrim 点击等外部驱动的关闭）
            SplitViewIsPaneOpenDescriptor.RemoveValueChanged(_splitView, OnSplitViewIsPaneOpenChanged);
        }

        // 获取模板子元素
        _splitView        = GetTemplateChild(PartSplitView)        as SplitView;
        _menuList         = GetTemplateChild(PartMenuList)         as ListBox;
        _footerList       = GetTemplateChild(PartFooterList)       as ListBox;
        _paneToggleButton = GetTemplateChild(PartPaneToggleButton) as ButtonBase;
        _autoSuggestBox   = GetTemplateChild(PartAutoSuggestBox)   as SearchBox;

        // 绑定新事件
        if (_paneToggleButton != null)
        {
            _paneToggleButton.Click      += OnPaneToggleClick;
            _paneToggleButton.Visibility  =
                IsPaneToggleButtonVisible ? Visibility.Visible : Visibility.Collapsed;
        }
        if (_menuList   != null)
        {
            _menuList.SelectionChanged += OnListSelectionChanged;
            _menuList.ItemContainerGenerator.StatusChanged += OnMenuContainersGenerated;
        }
        if (_footerList != null)
        {
            _footerList.SelectionChanged += OnListSelectionChanged;
            _footerList.ItemContainerGenerator.StatusChanged += OnFooterContainersGenerated;
        }
        if (_autoSuggestBox != null)
        {
            _autoSuggestBox.Visibility        = IsSearchBoxVisible ? Visibility.Visible : Visibility.Collapsed;
            _autoSuggestBox.Text              = AutoSuggestBoxText;
            _autoSuggestBox.SuggestionsSource = AutoSuggestBoxSuggestionsSource;
            _autoSuggestBox.TextChanged       += OnAutoSuggestBoxTextChanged;
            _autoSuggestBox.Search            += OnAutoSuggestBoxSearch;
        }
        if (_splitView  != null)
        {
            _splitView.SizeChanged += OnSplitViewSizeChanged;
            // 监听 SplitView.IsPaneOpen 变化，以便 Scrim 点击等外部关闭能同步回 NavigationView
            SplitViewIsPaneOpenDescriptor.AddValueChanged(_splitView, OnSplitViewIsPaneOpenChanged);
            _splitView.OpenPaneLength    = OpenPaneLength;
            _splitView.CompactPaneLength = CompactPaneLength;
        }

        // 同步数据源
        if (_menuList   != null) _menuList.ItemsSource   = MenuItemsSource;
        if (_footerList != null) _footerList.ItemsSource = FooterMenuItemsSource;

        // 应用初始模式
        ApplyDisplayMode();

        // Loaded 后再矫正一次：SplitView 模板在 NavigationView.OnApplyTemplate 时尚未 apply，
        // ApplyState 是 no-op；等 Loaded 时视觉树完全就绪，确保状态正确。
        Loaded -= OnLoaded;
        Loaded += OnLoaded;
    }

    private void OnLoaded(object sender, RoutedEventArgs e) => ApplyDisplayMode();

    // ════════════════════════════════════════════════════════════════════
    //  事件处理
    // ════════════════════════════════════════════════════════════════════

    private void OnPaneToggleClick(object sender, RoutedEventArgs e)
        => IsPaneOpen = !IsPaneOpen;

    /// <summary>
    /// 主列表容器生成完毕后重新应用 LayoutMode（解决初始加载时 ContainerFromItem 返回 null 的问题）。
    /// </summary>
    private void OnMenuContainersGenerated(object? sender, EventArgs e)
    {
        if (_menuList?.ItemContainerGenerator.Status == GeneratorStatus.ContainersGenerated)
            UpdateItemLayoutModes();
    }

    private void OnFooterContainersGenerated(object? sender, EventArgs e)
    {
        if (_footerList?.ItemContainerGenerator.Status == GeneratorStatus.ContainersGenerated)
            UpdateItemLayoutModes();
    }

    /// <summary>搜索框文字变化时同步到 NavigationView.AutoSuggestBoxText。</summary>
    private void OnAutoSuggestBoxTextChanged(object sender, TextChangedEventArgs e)
    {
        if (AutoSuggestBoxText != _autoSuggestBox!.Text)
            AutoSuggestBoxText = _autoSuggestBox.Text;
    }

    /// <summary>搜索框提交时冒泡 QuerySubmitted 路由事件。</summary>
    private void OnAutoSuggestBoxSearch(object sender, RoutedEventArgs e)
        => RaiseEvent(new RoutedEventArgs(QuerySubmittedEvent, this));

    /// <summary>
    /// SplitView.IsPaneOpen 被外部（Scrim 点击）改变时，反向同步到 NavigationView.IsPaneOpen。
    /// 避免"点击 Scrim 关闭后汉堡按钮失效"的 Bug。
    /// </summary>
    private void OnSplitViewIsPaneOpenChanged(object? sender, EventArgs e)
    {
        if (_splitView == null) return;
        // 仅当值真正不一致时才写入，避免循环触发
        if (IsPaneOpen != _splitView.IsPaneOpen)
        {
            IsPaneOpen = _splitView.IsPaneOpen;
        }
    }

    private void OnListSelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (e.AddedItems.Count == 0) return;

        var list  = (ListBox)sender;
        var other = ReferenceEquals(list, _menuList) ? _footerList : _menuList;

        // 两列表互斥
        if (other?.SelectedItem != null) other.SelectedItem = null;

        SelectedItem = list.SelectedItem;

        // Overlay 模式选中后自动关闭 Pane
        if (_splitView?.DisplayMode == SplitViewDisplayMode.Overlay)
            IsPaneOpen = false;

        RaiseEvent(new SelectionChangedEventArgs(SelectionChangedEvent, e.RemovedItems, e.AddedItems));
    }

    private void OnSplitViewSizeChanged(object sender, SizeChangedEventArgs e)
    {
        if (DisplayMode == NavigationViewDisplayMode.Auto)
            ApplyDisplayMode();
    }

    // ════════════════════════════════════════════════════════════════════
    //  核心逻辑
    // ════════════════════════════════════════════════════════════════════

    /// <summary>
    /// 根据当前 DisplayMode（含 Auto 自动推断）配置 SplitView 并更新所有子项状态。
    /// </summary>
    private void ApplyDisplayMode()
    {
        if (_splitView == null) return;

        // 解析实际模式（Auto → 根据宽度推断）
        var mode = DisplayMode;
        if (mode == NavigationViewDisplayMode.Auto)
        {
            var width = _splitView.ActualWidth;
            mode = width >= ExpandedThreshold ? NavigationViewDisplayMode.Left
                 : width >= CompactThreshold  ? NavigationViewDisplayMode.LeftCompact
                 :                              NavigationViewDisplayMode.LeftMinimal;
        }

        // 同步尺寸到 SplitView
        _splitView.OpenPaneLength    = OpenPaneLength;
        _splitView.CompactPaneLength = CompactPaneLength;

        // 映射到 SplitView.DisplayMode
        switch (mode)
        {
            case NavigationViewDisplayMode.Left:
                _splitView.DisplayMode = SplitViewDisplayMode.Inline;
                _splitView.IsPaneOpen  = true;
                IsPaneOpen             = true;
                break;

            case NavigationViewDisplayMode.LeftCompact:
                _splitView.DisplayMode = SplitViewDisplayMode.CompactOverlay;
                // 切入 Compact 模式时默认显示图标条（关闭状态）
                _splitView.IsPaneOpen  = false;
                IsPaneOpen             = false;
                break;

            case NavigationViewDisplayMode.LeftCompactInline:
                _splitView.DisplayMode = SplitViewDisplayMode.CompactInline;
                // 切入 CompactInline 模式时默认显示图标条（关闭状态）
                _splitView.IsPaneOpen  = false;
                IsPaneOpen             = false;
                break;

            case NavigationViewDisplayMode.LeftMinimal:
                _splitView.DisplayMode = SplitViewDisplayMode.Overlay;
                _splitView.IsPaneOpen  = false;
                IsPaneOpen             = false;
                break;
        }

        // 显式触发一次最终状态应用，消除中间过渡状态和 WPF 动画 HoldEnd 残留的干扰。
        // 各属性赋值时均会触发 ApplyState，但 DisplayMode 改变前的中间调用使用旧 DisplayMode，
        // 此处确保所有属性就位后以最终正确状态重新应用一次。
        _splitView.ApplyState(animate: false);

        UpdateItemLayoutModes();
        UpdateHeaderPadding();
    }

    /// <summary>
    /// 更新所有导航项的 LayoutMode。
    /// <param name="animate">true = 等 SplitView 动画（250ms）结束后再切换，避免视觉跳变；
    ///                        false = 立即切换（初始化、模式切换等无动画场景）。</param>
    /// </summary>
    private void UpdateItemLayoutModes(bool animate = false)
    {
        var isCompactMode = _splitView?.DisplayMode is SplitViewDisplayMode.CompactOverlay
                                                       or SplitViewDisplayMode.CompactInline;
        var paneOpen = _splitView?.IsPaneOpen ?? false;

        var targetMode = (isCompactMode && !paneOpen)
            ? NavigationViewItemLayoutMode.Compact
            : NavigationViewItemLayoutMode.Expanded;

        // 取消上一个待执行的切换
        _layoutModeTimer?.Stop();
        _layoutModeTimer = null;

        if (animate && isCompactMode)
        {
            // 有动画时延迟切换：等 SplitView 动画（250ms）完成后再改变外观，避免跳变
            _layoutModeTimer = new DispatcherTimer(DispatcherPriority.Background)
            {
                Interval = TimeSpan.FromMilliseconds(260)
            };
            _layoutModeTimer.Tick += (_, _) =>
            {
                _layoutModeTimer!.Stop();
                _layoutModeTimer = null;
                ApplyLayoutMode(_menuList,   targetMode);
                ApplyLayoutMode(_footerList, targetMode);
                UpdateSearchBoxForLayoutMode(targetMode);
                UpdatePaneScrollBarVisibility(targetMode);
            };
            _layoutModeTimer.Start();
        }
        else
        {
            // 无动画：直接切换（初始加载 / 模式切换 / 非 Compact 模式）
            ApplyLayoutMode(_menuList,   targetMode);
            ApplyLayoutMode(_footerList, targetMode);
            UpdateSearchBoxForLayoutMode(targetMode);
            UpdatePaneScrollBarVisibility(targetMode);
        }
    }

    private static void ApplyLayoutMode(ListBox? list, NavigationViewItemLayoutMode mode)
    {
        if (list == null) return;
        foreach (var item in list.Items)
        {
            var container = list.ItemContainerGenerator.ContainerFromItem(item) as NavigationViewItem;
            container?.SetValue(NavigationViewItem.LayoutModeProperty, mode);
        }
    }

    /// <summary>
    /// 根据当前 LayoutMode 控制搜索框可见性：
    /// Compact 时隐藏（Pane 太窄放不下），Expanded 时根据 IsSearchBoxVisible 决定。
    /// </summary>
    private void UpdateSearchBoxForLayoutMode(NavigationViewItemLayoutMode mode)
    {
        if (_autoSuggestBox == null) return;
        _autoSuggestBox.Visibility = (mode == NavigationViewItemLayoutMode.Compact || !IsSearchBoxVisible)
            ? Visibility.Collapsed
            : Visibility.Visible;
    }

    /// <summary>
    /// 根据当前 LayoutMode 控制 MenuList 内部 ScrollViewer 的滚动条可见性：
    /// - Compact：Hidden（滚动条宽度=0，不挤压 item；鼠标滚轮仍然有效）
    /// - Expanded：Auto（正常显示滚动条）
    /// 注：ScrollBarVisibility.Hidden ≠ Disabled，Hidden 仅隐藏视觉，滚动功能完好。
    /// </summary>
    private void UpdatePaneScrollBarVisibility(NavigationViewItemLayoutMode mode)
    {
        var vis = mode == NavigationViewItemLayoutMode.Compact
            ? ScrollBarVisibility.Hidden
            : ScrollBarVisibility.Auto;

        if (_menuList   != null) ScrollViewer.SetVerticalScrollBarVisibility(_menuList,   vis);
        if (_footerList != null) ScrollViewer.SetVerticalScrollBarVisibility(_footerList, vis);
    }

    /// <summary>
    /// 计算 Header 区域左侧 Padding：
    /// Overlay 模式时 Pane 不占用水平空间，内容从 x=0 开始，汉堡按钮浮在左上角，
    /// 需要给 Header 加 CompactPaneLength 的左偏移避免被按钮遮挡。
    /// </summary>
    private void UpdateHeaderPadding()
    {
        var buttonVisible = IsPaneToggleButtonVisible;
        var paneOpen      = _splitView?.IsPaneOpen ?? IsPaneOpen;
        var  svMode        = _splitView?.DisplayMode;

        // 需要额外左偏移的情况（汉堡按钮浮于左上角，内容从 x=0 开始）：
        //   1. Overlay 模式（Pane 不占水平空间）
        //   2. Inline 模式且 Pane 已关闭（Pane 收起后宽度为 0，内容从 x=0 开始）
        var needPad = buttonVisible &&
                      (svMode == SplitViewDisplayMode.Overlay ||
                       (svMode == SplitViewDisplayMode.Inline && !paneOpen));

        var leftPad = needPad ? CompactPaneLength : 0;
        HeaderPadding = new Thickness(leftPad, 0, 0, 0);
    }

    // ════════════════════════════════════════════════════════════════════
    //  公共 API
    // ════════════════════════════════════════════════════════════════════

    public void OpenPane()   => IsPaneOpen = true;
    public void ClosePane()  => IsPaneOpen = false;
    public void TogglePane() => IsPaneOpen = !IsPaneOpen;
}

// ════════════════════════════════════════════════════════════════════════
//  辅助类 & 枚举
// ════════════════════════════════════════════════════════════════════════

/// <summary>
/// NavigationView 内部 ListBox，强制生成 <see cref="NavigationViewItem"/> 容器。
/// 不重写 DefaultStyleKey，直接复用 ListBox 主题样式。
/// </summary>
public class NavigationViewList : ListBox
{
    protected override DependencyObject GetContainerForItemOverride()
        => new NavigationViewItem();

    protected override bool IsItemItsOwnContainerOverride(object item)
        => item is NavigationViewItem;
}

/// <summary>NavigationView 的显示模式。</summary>
public enum NavigationViewDisplayMode
{
    /// <summary>根据控件宽度自动在 Left / LeftCompact / LeftMinimal 之间切换（WinUI 同款阈值）。</summary>
    Auto,

    /// <summary>左侧展开面板，始终可见，与内容并排推挤（SplitView.Inline）。</summary>
    Left,

    /// <summary>左侧紧凑图标条（CompactPaneLength），点击汉堡按钮叠加浮层展开（SplitView.CompactOverlay）。</summary>
    LeftCompact,

    /// <summary>左侧紧凑图标条（CompactPaneLength），点击汉堡按钮并排推挤内容展开（SplitView.CompactInline）。</summary>
    LeftCompactInline,

    /// <summary>左侧面板完全隐藏，点击汉堡按钮叠加浮层展开（SplitView.Overlay）。</summary>
    LeftMinimal,
}
