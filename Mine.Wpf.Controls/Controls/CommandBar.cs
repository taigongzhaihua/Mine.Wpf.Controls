using System.Windows;
using System.Windows.Controls;
using System.Collections;
using System.Collections.Specialized;
using System.Windows.Controls.Primitives;

namespace Mine.Wpf.Controls.Controls;

/// <summary>
/// Material 3 命令栏：用于显示一组操作按钮的工具栏容器。
/// 支持主要命令 (PrimaryCommands) 和次要命令 (SecondaryCommands) 的组织管理。
/// </summary>
[TemplatePart(Name = PartPrimaryPanel, Type = typeof(Panel))]
[TemplatePart(Name = PartSecondaryPanel, Type = typeof(Panel))]
[TemplatePart(Name = PartMoreButton, Type = typeof(ButtonBase))]
public class CommandBar : Control
{
    private const string PartPrimaryPanel = "PART_PrimaryPanel";
    private const string PartSecondaryPanel = "PART_SecondaryPanel";
    private const string PartMoreButton = "PART_MoreButton";

    private Panel? _primaryPanel;
    private Panel? _secondaryPanel;
    private ButtonBase? _moreButton;
    private MenuFlyout? _secondaryMenuFlyout;

    static CommandBar()
    {
        DefaultStyleKeyProperty.OverrideMetadata(
            typeof(CommandBar),
            new FrameworkPropertyMetadata(typeof(CommandBar)));
    }

    public CommandBar()
    {
        PrimaryCommands = new System.Collections.ObjectModel.ObservableCollection<object>();
        SecondaryCommands = new System.Collections.ObjectModel.ObservableCollection<MenuFlyoutItemBase>();
        Loaded += OnLoaded;
    }

    private void OnLoaded(object sender, RoutedEventArgs e)
    {
        UpdateCommandLayout();
    }

    // ── PrimaryCommands ───────────────────────────────────────────────
    public static readonly DependencyProperty PrimaryCommandsProperty =
        DependencyProperty.Register(nameof(PrimaryCommands), typeof(IList), typeof(CommandBar),
            new PropertyMetadata(null, OnPrimaryCommandsChanged));

    /// <summary>主要命令集合（显示在工具栏中）。</summary>
    public IList PrimaryCommands
    {
        get => (IList)GetValue(PrimaryCommandsProperty);
        set => SetValue(PrimaryCommandsProperty, value);
    }

    private static void OnPrimaryCommandsChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        var bar = (CommandBar)d;
        if (e.OldValue is INotifyCollectionChanged oldCollection)
            oldCollection.CollectionChanged -= bar.OnPrimaryCommandsCollectionChanged;
        if (e.NewValue is INotifyCollectionChanged newCollection)
            newCollection.CollectionChanged += bar.OnPrimaryCommandsCollectionChanged;
        bar.UpdatePrimaryCommands();
    }

    private void OnPrimaryCommandsCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
        => UpdatePrimaryCommands();

    // ── SecondaryCommands ─────────────────────────────────────────────
    public static readonly DependencyProperty SecondaryCommandsProperty =
        DependencyProperty.Register(nameof(SecondaryCommands), typeof(IList), typeof(CommandBar),
            new PropertyMetadata(null, OnSecondaryCommandsChanged));

    /// <summary>次要命令集合（显示在更多菜单中）。</summary>
    public IList SecondaryCommands
    {
        get => (IList)GetValue(SecondaryCommandsProperty);
        set => SetValue(SecondaryCommandsProperty, value);
    }

    private static void OnSecondaryCommandsChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        var bar = (CommandBar)d;
        if (e.OldValue is INotifyCollectionChanged oldCollection)
            oldCollection.CollectionChanged -= bar.OnSecondaryCommandsCollectionChanged;
        if (e.NewValue is INotifyCollectionChanged newCollection)
            newCollection.CollectionChanged += bar.OnSecondaryCommandsCollectionChanged;
        bar.UpdateSecondaryCommands();
    }

    private void OnSecondaryCommandsCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
        => UpdateSecondaryCommands();

    // ── IsSecondaryMenuOpen ───────────────────────────────────────────
    public static readonly DependencyProperty IsSecondaryMenuOpenProperty =
        DependencyProperty.Register(nameof(IsSecondaryMenuOpen), typeof(bool), typeof(CommandBar),
            new FrameworkPropertyMetadata(false, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));

    /// <summary>次要命令菜单是否打开（双向绑定）。</summary>
    public bool IsSecondaryMenuOpen
    {
        get => (bool)GetValue(IsSecondaryMenuOpenProperty);
        set => SetValue(IsSecondaryMenuOpenProperty, value);
    }

    // ── DefaultLabelPosition ──────────────────────────────────────────
    public static readonly DependencyProperty DefaultLabelPositionProperty =
        DependencyProperty.Register(nameof(DefaultLabelPosition), typeof(CommandBarLabelPosition), typeof(CommandBar),
            new PropertyMetadata(CommandBarLabelPosition.Right));

    /// <summary>命令按钮的默认标签位置。</summary>
    public CommandBarLabelPosition DefaultLabelPosition
    {
        get => (CommandBarLabelPosition)GetValue(DefaultLabelPositionProperty);
        set => SetValue(DefaultLabelPositionProperty, value);
    }

    // ── CompactMode ───────────────────────────────────────────────────
    public static readonly DependencyProperty CompactModeProperty =
        DependencyProperty.Register(nameof(CompactMode), typeof(bool), typeof(CommandBar),
            new PropertyMetadata(false, OnCompactModeChanged));

    /// <summary>紧凑模式：仅显示图标，隐藏标签。</summary>
    public bool CompactMode
    {
        get => (bool)GetValue(CompactModeProperty);
        set => SetValue(CompactModeProperty, value);
    }

    private static void OnCompactModeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        => ((CommandBar)d).UpdateCommandLayout();

    // ── 模板应用 ──────────────────────────────────────────────────────
    public override void OnApplyTemplate()
    {
        base.OnApplyTemplate();

        if (_moreButton != null)
            _moreButton.Click -= OnMoreButtonClick;

        _primaryPanel = GetTemplateChild(PartPrimaryPanel) as Panel;
        _secondaryPanel = GetTemplateChild(PartSecondaryPanel) as Panel;
        _moreButton = GetTemplateChild(PartMoreButton) as ButtonBase;

        if (_moreButton != null)
            _moreButton.Click += OnMoreButtonClick;

        // 将 MenuFlyout 添加到次要面板（确保在视觉树中）
        if (_secondaryPanel != null && _secondaryMenuFlyout == null)
        {
            _secondaryMenuFlyout = new MenuFlyout
            {
                PlacementTarget = _moreButton,
                Placement = System.Windows.Controls.Primitives.PlacementMode.Bottom
            };
            _secondaryPanel.Children.Add(_secondaryMenuFlyout);
        }

        UpdatePrimaryCommands();
        UpdateSecondaryCommands();
        UpdateCommandLayout();
    }

    // ── 内部方法 ──────────────────────────────────────────────────────
    private void UpdatePrimaryCommands()
    {
        if (_primaryPanel == null) return;
        _primaryPanel.Children.Clear();

        if (PrimaryCommands == null) return;

        foreach (var item in PrimaryCommands)
        {
            if (item is UIElement element)
                _primaryPanel.Children.Add(element);
        }
    }

    private void UpdateSecondaryCommands()
    {
        if (_moreButton == null) return;

        if (SecondaryCommands?.Count > 0)
        {
            _moreButton.Visibility = Visibility.Visible;

            // 创建或更新 MenuFlyout
            if (_secondaryMenuFlyout == null)
            {
                _secondaryMenuFlyout = new MenuFlyout
                {
                    PlacementTarget = _moreButton,
                    Placement = System.Windows.Controls.Primitives.PlacementMode.Bottom
                };
            }

            _secondaryMenuFlyout.Items.Clear();
            foreach (var item in SecondaryCommands)
            {
                if (item is MenuFlyoutItemBase menuItem)
                    _secondaryMenuFlyout.Items.Add(menuItem);
            }
        }
        else
        {
            _moreButton.Visibility = Visibility.Collapsed;
        }
    }

    private void UpdateCommandLayout()
    {
        // 可以在这里实现响应式布局逻辑
        // 例如根据可用宽度自动将部分主要命令移到次要菜单
    }

    private void OnMoreButtonClick(object sender, RoutedEventArgs e)
    {
        if (_secondaryMenuFlyout != null)
        {
            _secondaryMenuFlyout.IsOpen = true;
        }
    }
}

/// <summary>命令栏标签位置枚举。</summary>
public enum CommandBarLabelPosition
{
    /// <summary>标签在图标下方。</summary>
    Bottom,
    /// <summary>标签在图标右侧。</summary>
    Right,
    /// <summary>不显示标签（仅图标）。</summary>
    Collapsed
}
