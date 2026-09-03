using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;

namespace Mine.Wpf.Controls.Controls;

/// <summary>
/// Material 3 命令栏：水平排列一组主要命令（<see cref="AppBarButton"/> / <see cref="AppBarToggleButton"/> / <see cref="AppBarSeparator"/>），
/// 次要命令通过溢出按钮以 <see cref="MenuFlyout"/> 形式展示。
/// </summary>
[TemplatePart(Name = PartPrimaryPanel, Type = typeof(Panel))]
[TemplatePart(Name = PartOverflowButton, Type = typeof(ButtonBase))]
[TemplatePart(Name = PartOverflowMenu, Type = typeof(MenuFlyout))]
public class CommandBar : Control
{
    private const string PartPrimaryPanel = "PART_PrimaryPanel";
    private const string PartOverflowButton = "PART_OverflowButton";
    private const string PartOverflowMenu = "PART_OverflowMenu";

    private Panel? _primaryPanel;
    private ButtonBase? _overflowButton;
    private MenuFlyout? _overflowMenu;

    static CommandBar()
    {
        DefaultStyleKeyProperty.OverrideMetadata(
            typeof(CommandBar),
            new FrameworkPropertyMetadata(typeof(CommandBar)));
    }

    public CommandBar()
    {
        PrimaryCommands = [];
        SecondaryCommands = [];
    }

    // ── PrimaryCommands ──────────────────────────────────────────────
    public static readonly DependencyProperty PrimaryCommandsProperty =
        DependencyProperty.Register(nameof(PrimaryCommands), typeof(ObservableCollection<UIElement>), typeof(CommandBar),
            new PropertyMetadata(null, OnPrimaryCommandsChanged));

    /// <summary>主命令集合，始终直接显示在命令栏中（<see cref="AppBarButton"/>、<see cref="AppBarToggleButton"/>、<see cref="AppBarSeparator"/> 等）。</summary>
    public ObservableCollection<UIElement> PrimaryCommands
    {
        get => (ObservableCollection<UIElement>)GetValue(PrimaryCommandsProperty);
        set => SetValue(PrimaryCommandsProperty, value);
    }

    private static void OnPrimaryCommandsChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        var bar = (CommandBar)d;
        if (e.OldValue is ObservableCollection<UIElement> oldItems)
            oldItems.CollectionChanged -= bar.OnPrimaryCommandsCollectionChanged;
        if (e.NewValue is ObservableCollection<UIElement> newItems)
            newItems.CollectionChanged += bar.OnPrimaryCommandsCollectionChanged;
        bar.PopulatePrimaryCommands();
    }

    private void OnPrimaryCommandsCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
        => PopulatePrimaryCommands();

    // ── SecondaryCommands ────────────────────────────────────────────
    public static readonly DependencyProperty SecondaryCommandsProperty =
        DependencyProperty.Register(nameof(SecondaryCommands), typeof(ObservableCollection<MenuFlyoutItemBase>), typeof(CommandBar),
            new PropertyMetadata(null, OnSecondaryCommandsChanged));

    /// <summary>次要命令集合，收纳到溢出菜单（<see cref="MenuFlyoutItem"/>、<see cref="MenuFlyoutSeparator"/> 等）中。</summary>
    public ObservableCollection<MenuFlyoutItemBase> SecondaryCommands
    {
        get => (ObservableCollection<MenuFlyoutItemBase>)GetValue(SecondaryCommandsProperty);
        set => SetValue(SecondaryCommandsProperty, value);
    }

    private static void OnSecondaryCommandsChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        var bar = (CommandBar)d;
        if (e.OldValue is ObservableCollection<MenuFlyoutItemBase> oldItems)
            oldItems.CollectionChanged -= bar.OnSecondaryCommandsCollectionChanged;
        if (e.NewValue is ObservableCollection<MenuFlyoutItemBase> newItems)
            newItems.CollectionChanged += bar.OnSecondaryCommandsCollectionChanged;
        bar.SyncOverflowMenuItems();
        bar.UpdateOverflowButtonVisibility();
    }

    private void OnSecondaryCommandsCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        SyncOverflowMenuItems();
        UpdateOverflowButtonVisibility();
    }

    // ── OverflowButtonVisibility ─────────────────────────────────────
    public static readonly DependencyProperty OverflowButtonVisibilityProperty =
        DependencyProperty.Register(nameof(OverflowButtonVisibility), typeof(CommandBarOverflowButtonVisibility), typeof(CommandBar),
            new PropertyMetadata(CommandBarOverflowButtonVisibility.Auto, OnOverflowButtonVisibilityChanged));

    /// <summary>溢出按钮的可见性策略。</summary>
    public CommandBarOverflowButtonVisibility OverflowButtonVisibility
    {
        get => (CommandBarOverflowButtonVisibility)GetValue(OverflowButtonVisibilityProperty);
        set => SetValue(OverflowButtonVisibilityProperty, value);
    }

    private static void OnOverflowButtonVisibilityChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        => ((CommandBar)d).UpdateOverflowButtonVisibility();

    // ── IsOverflowOpen（只读，供模板绑定溢出菜单状态） ──────────────────
    private static readonly DependencyPropertyKey IsOverflowOpenPropertyKey =
        DependencyProperty.RegisterReadOnly(nameof(IsOverflowOpen), typeof(bool), typeof(CommandBar),
            new PropertyMetadata(false));

    public static readonly DependencyProperty IsOverflowOpenProperty = IsOverflowOpenPropertyKey.DependencyProperty;

    /// <summary>溢出菜单当前是否展开。</summary>
    public bool IsOverflowOpen => (bool)GetValue(IsOverflowOpenProperty);

    // ── 模板应用 ──────────────────────────────────────────────────────
    private static readonly DependencyPropertyDescriptor MenuIsOpenDescriptor =
        DependencyPropertyDescriptor.FromProperty(MenuFlyout.IsOpenProperty, typeof(MenuFlyout));

    public override void OnApplyTemplate()
    {
        if (_overflowButton is not null)
            _overflowButton.Click -= OnOverflowButtonClick;
        if (_overflowMenu is not null)
            MenuIsOpenDescriptor.RemoveValueChanged(_overflowMenu, OnOverflowMenuIsOpenChanged);

        base.OnApplyTemplate();

        _primaryPanel = GetTemplateChild(PartPrimaryPanel) as Panel;
        _overflowButton = GetTemplateChild(PartOverflowButton) as ButtonBase;
        _overflowMenu = GetTemplateChild(PartOverflowMenu) as MenuFlyout;

        if (_overflowButton is not null)
            _overflowButton.Click += OnOverflowButtonClick;
        if (_overflowMenu is not null)
            MenuIsOpenDescriptor.AddValueChanged(_overflowMenu, OnOverflowMenuIsOpenChanged);

        PopulatePrimaryCommands();
        SyncOverflowMenuItems();
        UpdateOverflowButtonVisibility();
    }

    private void OnOverflowButtonClick(object sender, RoutedEventArgs e)
    {
        if (_overflowMenu is null || _overflowButton is null)
            return;
        _overflowMenu.PlacementTarget = _overflowButton;
        _overflowMenu.IsOpen = true;
    }

    private void OnOverflowMenuIsOpenChanged(object? sender, EventArgs e)
        => SetValue(IsOverflowOpenPropertyKey, _overflowMenu?.IsOpen ?? false);

    private void PopulatePrimaryCommands()
    {
        if (_primaryPanel is null)
            return;
        _primaryPanel.Children.Clear();
        foreach (var element in PrimaryCommands)
            _primaryPanel.Children.Add(element);
    }

    private void SyncOverflowMenuItems()
    {
        if (_overflowMenu is null)
            return;
        _overflowMenu.Items.Clear();
        foreach (var item in SecondaryCommands)
            _overflowMenu.Items.Add(item);
    }

    private void UpdateOverflowButtonVisibility()
    {
        if (_overflowButton is null)
            return;
        _overflowButton.Visibility = OverflowButtonVisibility switch
        {
            CommandBarOverflowButtonVisibility.Collapsed => Visibility.Collapsed,
            CommandBarOverflowButtonVisibility.Visible => Visibility.Visible,
            _ => SecondaryCommands.Count > 0 ? Visibility.Visible : Visibility.Collapsed,
        };
    }
}

/// <summary>溢出按钮可见性策略。</summary>
public enum CommandBarOverflowButtonVisibility
{
    /// <summary>存在次要命令时自动显示。</summary>
    Auto,
    Visible,
    Collapsed,
}

/// <summary>
/// 命令栏中的图标命令按钮（图标 + 标签竖排）。
/// </summary>
public class AppBarButton : ButtonBase
{
    static AppBarButton()
    {
        DefaultStyleKeyProperty.OverrideMetadata(
            typeof(AppBarButton),
            new FrameworkPropertyMetadata(typeof(AppBarButton)));
    }

    // ── Icon ─────────────────────────────────────────────────────────
    public static readonly DependencyProperty IconProperty =
        DependencyProperty.Register(nameof(Icon), typeof(object), typeof(AppBarButton),
            new PropertyMetadata(null));

    /// <summary>按钮图标，支持任意 UIElement，例如 MaterialIcon。</summary>
    public object? Icon
    {
        get => GetValue(IconProperty);
        set => SetValue(IconProperty, value);
    }

    // ── Label ────────────────────────────────────────────────────────
    public static readonly DependencyProperty LabelProperty =
        DependencyProperty.Register(nameof(Label), typeof(string), typeof(AppBarButton),
            new PropertyMetadata(null));

    /// <summary>按钮下方的文本标签。</summary>
    public string? Label
    {
        get => (string?)GetValue(LabelProperty);
        set => SetValue(LabelProperty, value);
    }

    // ── IsCompact（隐藏 Label，仅显示图标） ─────────────────────────────
    public static readonly DependencyProperty IsCompactProperty =
        DependencyProperty.Register(nameof(IsCompact), typeof(bool), typeof(AppBarButton),
            new PropertyMetadata(false));

    /// <summary>是否为紧凑模式（仅显示图标，不显示标签）。</summary>
    public bool IsCompact
    {
        get => (bool)GetValue(IsCompactProperty);
        set => SetValue(IsCompactProperty, value);
    }
}

/// <summary>
/// 命令栏中的图标切换命令按钮（图标 + 标签竖排，支持选中状态）。
/// </summary>
public class AppBarToggleButton : System.Windows.Controls.Primitives.ToggleButton
{
    static AppBarToggleButton()
    {
        DefaultStyleKeyProperty.OverrideMetadata(
            typeof(AppBarToggleButton),
            new FrameworkPropertyMetadata(typeof(AppBarToggleButton)));
    }

    public static readonly DependencyProperty IconProperty =
        DependencyProperty.Register(nameof(Icon), typeof(object), typeof(AppBarToggleButton),
            new PropertyMetadata(null));

    /// <summary>按钮图标，支持任意 UIElement，例如 MaterialIcon。</summary>
    public object? Icon
    {
        get => GetValue(IconProperty);
        set => SetValue(IconProperty, value);
    }

    public static readonly DependencyProperty LabelProperty =
        DependencyProperty.Register(nameof(Label), typeof(string), typeof(AppBarToggleButton),
            new PropertyMetadata(null));

    /// <summary>按钮下方的文本标签。</summary>
    public string? Label
    {
        get => (string?)GetValue(LabelProperty);
        set => SetValue(LabelProperty, value);
    }

    public static readonly DependencyProperty IsCompactProperty =
        DependencyProperty.Register(nameof(IsCompact), typeof(bool), typeof(AppBarToggleButton),
            new PropertyMetadata(false));

    /// <summary>是否为紧凑模式（仅显示图标，不显示标签）。</summary>
    public bool IsCompact
    {
        get => (bool)GetValue(IsCompactProperty);
        set => SetValue(IsCompactProperty, value);
    }
}

/// <summary>
/// 命令栏中的分隔符（竖线）。
/// </summary>
public class AppBarSeparator : Control
{
    static AppBarSeparator()
    {
        DefaultStyleKeyProperty.OverrideMetadata(
            typeof(AppBarSeparator),
            new FrameworkPropertyMetadata(typeof(AppBarSeparator)));
    }
}
