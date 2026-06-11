using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Media;
using System.Windows.Media.Animation;
using SWWindow = System.Windows.Window;

namespace Mine.Wpf.Controls.Controls;

// ══════════════════════════════════════════════════════════════════
//  Enums & Data Model
// ══════════════════════════════════════════════════════════════════

/// <summary>Toast 通知级别。</summary>
public enum ToastLevel { Info, Success, Warning, Error }

/// <summary>通知历史记录项，用于 NotificationCenter 列表。</summary>
public class NotificationItem
{
    public ToastLevel   Level       { get; init; }
    public string       Title       { get; init; } = string.Empty;
    public string?      Message     { get; init; }
    public DateTime     Timestamp   { get; init; } = DateTime.Now;
    public bool         IsRead      { get; set;  }

    /// <summary>时间的友好显示文字（刚刚 / N 分钟前 / 日期）。</summary>
    public string TimeAgo
    {
        get
        {
            var diff = DateTime.Now - Timestamp;
            if (diff.TotalSeconds < 60)  return "刚刚";
            if (diff.TotalMinutes < 60)  return $"{(int)diff.TotalMinutes} 分钟前";
            if (diff.TotalHours   < 24)  return $"{(int)diff.TotalHours} 小时前";
            return Timestamp.ToString("M月d日");
        }
    }

    /// <summary>Material Symbols 图标码点（按级别）。</summary>
    public string LevelIcon => Level switch
    {
        ToastLevel.Success => "\uE86C",  // check_circle
        ToastLevel.Warning => "\uE002",  // warning
        ToastLevel.Error   => "\uE000",  // error
        _                  => "\uE88E",  // info
    };
}

// ══════════════════════════════════════════════════════════════════
//  Toast Control
// ══════════════════════════════════════════════════════════════════

/// <summary>
/// Material Design 3 Toast — 从右上角滑入的富文本通知卡片。
/// 通常由 <see cref="ToastService"/> 驱动；也可在 XAML 中直接使用。
/// </summary>
[TemplatePart(Name = PartRoot,        Type = typeof(Border))]
[TemplatePart(Name = PartCloseButton, Type = typeof(ButtonBase))]
public class Toast : ContentControl
{
    private const string PartRoot        = "PART_Root";
    private const string PartCloseButton = "PART_Close";

    private Border?     _root;
    private ButtonBase? _closeBtn;

    static Toast() =>
        DefaultStyleKeyProperty.OverrideMetadata(
            typeof(Toast), new FrameworkPropertyMetadata(typeof(Toast)));

    // ── Title ─────────────────────────────────────────────────────
    public static readonly DependencyProperty TitleProperty =
        DependencyProperty.Register(nameof(Title), typeof(string), typeof(Toast),
            new PropertyMetadata(string.Empty));
    public string Title
    {
        get => (string)GetValue(TitleProperty);
        set => SetValue(TitleProperty, value);
    }

    // ── Level ─────────────────────────────────────────────────────
    public static readonly DependencyProperty LevelProperty =
        DependencyProperty.Register(nameof(Level), typeof(ToastLevel), typeof(Toast),
            new PropertyMetadata(ToastLevel.Info));
    public ToastLevel Level
    {
        get => (ToastLevel)GetValue(LevelProperty);
        set => SetValue(LevelProperty, value);
    }

    // ── Duration ──────────────────────────────────────────────────
    public static readonly DependencyProperty DurationProperty =
        DependencyProperty.Register(nameof(Duration), typeof(TimeSpan), typeof(Toast),
            new PropertyMetadata(TimeSpan.FromSeconds(5)));
    /// <summary>自动关闭延迟（默认 5 秒）。设为 <see cref="TimeSpan.Zero"/> 禁用自动关闭。</summary>
    public TimeSpan Duration
    {
        get => (TimeSpan)GetValue(DurationProperty);
        set => SetValue(DurationProperty, value);
    }

    // ── IsOpen ────────────────────────────────────────────────────
    public static readonly DependencyProperty IsOpenProperty =
        DependencyProperty.Register(nameof(IsOpen), typeof(bool), typeof(Toast),
            new PropertyMetadata(false, OnIsOpenChanged));
    public bool IsOpen
    {
        get => (bool)GetValue(IsOpenProperty);
        set => SetValue(IsOpenProperty, value);
    }

    // ── Events ────────────────────────────────────────────────────
    public static readonly RoutedEvent ClosedEvent =
        EventManager.RegisterRoutedEvent(nameof(Closed), RoutingStrategy.Bubble,
            typeof(RoutedEventHandler), typeof(Toast));
    public event RoutedEventHandler Closed
    {
        add    => AddHandler(ClosedEvent, value);
        remove => RemoveHandler(ClosedEvent, value);
    }

    // ── Template ──────────────────────────────────────────────────
    public override void OnApplyTemplate()
    {
        base.OnApplyTemplate();
        _root     = GetTemplateChild(PartRoot)        as Border;
        _closeBtn = GetTemplateChild(PartCloseButton) as ButtonBase;

        if (_closeBtn != null)
            _closeBtn.Click += (_, _) => Close();
    }

    private static void OnIsOpenChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is Toast t) t.OnIsOpenChanged((bool)e.NewValue);
    }

    private System.Windows.Threading.DispatcherTimer? _timer;

    private void OnIsOpenChanged(bool isOpen)
    {
        if (isOpen)
        {
            Visibility = Visibility.Visible;
            if (_root != null) AnimateIn();
            StartAutoClose();
        }
        else
        {
            _timer?.Stop();
            _timer = null;
            AnimateOut();
        }
    }

    /// <summary>关闭此 Toast。</summary>
    public void Close() => IsOpen = false;

    private void AnimateIn()
    {
        if (_root == null) return;
        _root.RenderTransform = new System.Windows.Media.TranslateTransform(60, 0);
        _root.BeginAnimation(OpacityProperty, null);

        var slide = new DoubleAnimation(60, 0, new Duration(TimeSpan.FromMilliseconds(280)))
            { EasingFunction = new CubicEase { EasingMode = EasingMode.EaseOut } };
        _root.RenderTransform.BeginAnimation(
            System.Windows.Media.TranslateTransform.XProperty, slide);

        _root.BeginAnimation(OpacityProperty,
            new DoubleAnimation(0, 1, new Duration(TimeSpan.FromMilliseconds(200)))
            { EasingFunction = new CubicEase { EasingMode = EasingMode.EaseOut } });
    }

    private void AnimateOut()
    {
        void RemoveSelf()
        {
            Dispatcher.BeginInvoke(() =>
            {
                RaiseEvent(new RoutedEventArgs(ClosedEvent));
                if (Parent is Panel panel)
                    panel.Children.Remove(this);
                else
                    Visibility = Visibility.Collapsed;
            });
        }

        if (_root == null) { RemoveSelf(); return; }

        var fade = new DoubleAnimation(1, 0, new Duration(TimeSpan.FromMilliseconds(180)))
            { EasingFunction = new CubicEase { EasingMode = EasingMode.EaseIn } };
        fade.Completed += (_, _) => RemoveSelf();
        _root.BeginAnimation(OpacityProperty, fade);
    }

    private void StartAutoClose()
    {
        _timer?.Stop();
        if (Duration == TimeSpan.Zero) return;

        _timer = new System.Windows.Threading.DispatcherTimer { Interval = Duration };
        _timer.Tick += (_, _) =>
        {
            _timer!.Stop();
            _timer = null;
            IsOpen = false;
        };
        _timer.Start();
    }

    /// <summary>显示此 Toast。</summary>
    public void Show(string title, string? message = null,
                     ToastLevel level = ToastLevel.Info, TimeSpan? duration = null)
    {
        Title    = title;
        Content  = message;
        Level    = level;
        Duration = duration ?? TimeSpan.FromSeconds(5);
        IsOpen   = true;
    }
}

// ══════════════════════════════════════════════════════════════════
//  ToastService  —  全局静态服务
// ══════════════════════════════════════════════════════════════════

/// <summary>
/// 全局 Toast 通知服务。从右上角叠加显示通知卡片，并将历史保存到
/// <see cref="Notifications"/> 供 <see cref="NotificationCenter"/> 显示。
/// </summary>
public static class ToastService
{
    private const string ContainerTag = "__Mine.ToastContainer__";
    private const int    MaxHistory   = 50;

    /// <summary>历史通知列表（最近 50 条），供 NotificationCenter 绑定。</summary>
    public static ObservableCollection<NotificationItem> Notifications { get; } = [];

    /// <summary>未读通知数量。</summary>
    public static int UnreadCount => Notifications.Count(n => !n.IsRead);

    // ── Show helpers ──────────────────────────────────────────────

    public static void Info(string title, string? message = null, TimeSpan? duration = null)
        => Show(title, message, ToastLevel.Info, duration);

    public static void Success(string title, string? message = null, TimeSpan? duration = null)
        => Show(title, message, ToastLevel.Success, duration);

    public static void Warning(string title, string? message = null, TimeSpan? duration = null)
        => Show(title, message, ToastLevel.Warning, duration);

    public static void Error(string title, string? message = null, TimeSpan? duration = null)
        => Show(title, message, ToastLevel.Error, duration);

    /// <summary>主调用入口：创建 Toast 并将其追加到窗口覆盖层右上角。</summary>
    public static void Show(string title, string? message = null,
                            ToastLevel level = ToastLevel.Info, TimeSpan? duration = null)
    {
        // 写入历史（UI 线程）
        var item = new NotificationItem { Level = level, Title = title, Message = message };

        var window = Helpers.WindowOverlay.GetActiveWindow();
        if (window == null) return;

        window.Dispatcher.BeginInvoke(() =>
        {
            // 历史记录（最近 50 条，新的在前）
            Notifications.Insert(0, item);
            while (Notifications.Count > MaxHistory)
                Notifications.RemoveAt(Notifications.Count - 1);

            // 通知所有 NotificationCenter 刷新 Badge
            NotificationCenter.RaiseBadgeChanged();

            var container = GetOrCreateContainer(window);

            var toast = new Toast();
            container.Children.Insert(0, toast);   // 新消息插到顶部
            toast.Show(title, message, level, duration);
        });
    }

    private static StackPanel GetOrCreateContainer(SWWindow window)
    {
        var overlay = Helpers.WindowOverlay.GetOrCreate(window);
        var existing = overlay.Children.OfType<StackPanel>()
            .FirstOrDefault(p => p.Tag as string == ContainerTag);
        if (existing != null) return existing;

        var container = new StackPanel
        {
            Tag                 = ContainerTag,
            VerticalAlignment   = VerticalAlignment.Top,
            HorizontalAlignment = HorizontalAlignment.Right,
            Margin              = new Thickness(0, 16, 16, 0),
        };
        Panel.SetZIndex(container, 9999);
        overlay.Children.Add(container);
        return container;
    }

    /// <summary>将所有通知标记为已读。</summary>
    public static void MarkAllRead()
    {
        foreach (var n in Notifications)
            n.IsRead = true;
        NotificationCenter.RaiseBadgeChanged();
    }

    /// <summary>清空历史通知。</summary>
    public static void ClearAll()
    {
        Notifications.Clear();
        NotificationCenter.RaiseBadgeChanged();
    }
}

// ══════════════════════════════════════════════════════════════════
//  NotificationCenter  —  历史通知面板控件
// ══════════════════════════════════════════════════════════════════

/// <summary>
/// 通知中心面板控件：显示 <see cref="ToastService.Notifications"/> 的历史列表，
/// 带未读 Badge、全部已读和清空按钮。
/// </summary>
[TemplatePart(Name = PartMarkAllRead,  Type = typeof(ButtonBase))]
[TemplatePart(Name = PartClearAll,     Type = typeof(ButtonBase))]
[TemplatePart(Name = PartItemsList,    Type = typeof(ItemsControl))]
public class NotificationCenter : Control
{
    private const string PartBellButton  = "PART_Bell";
    private const string PartMarkAllRead = "PART_MarkAllRead";
    private const string PartClearAll    = "PART_ClearAll";
    private const string PartItemsList   = "PART_Items";

    // 全局弱引用列表，供 ToastService 广播刷新
    private static readonly List<WeakReference<NotificationCenter>> _instances = [];

    static NotificationCenter() =>
        DefaultStyleKeyProperty.OverrideMetadata(
            typeof(NotificationCenter), new FrameworkPropertyMetadata(typeof(NotificationCenter)));

    public NotificationCenter()
    {
        _instances.Add(new WeakReference<NotificationCenter>(this));
        Unloaded += (_, _) => _instances.RemoveAll(r => !r.TryGetTarget(out _));
        RefreshBadge();
    }

    // ── UnreadCount ───────────────────────────────────────────────
    private static readonly DependencyPropertyKey UnreadCountKey =
        DependencyProperty.RegisterReadOnly(nameof(UnreadCount), typeof(int),
            typeof(NotificationCenter), new PropertyMetadata(0));
    public static readonly DependencyProperty UnreadCountProperty = UnreadCountKey.DependencyProperty;
    public int UnreadCount
    {
        get => (int)GetValue(UnreadCountProperty);
        private set => SetValue(UnreadCountKey, value);
    }

    // ── HasUnread ──────────────────────────────────────────────────
    private static readonly DependencyPropertyKey HasUnreadKey =
        DependencyProperty.RegisterReadOnly(nameof(HasUnread), typeof(bool),
            typeof(NotificationCenter), new PropertyMetadata(false));
    public static readonly DependencyProperty HasUnreadProperty = HasUnreadKey.DependencyProperty;
    public bool HasUnread
    {
        get => (bool)GetValue(HasUnreadProperty);
        private set => SetValue(HasUnreadKey, value);
    }

    // ── Template ──────────────────────────────────────────────────
    public override void OnApplyTemplate()
    {
        base.OnApplyTemplate();
        Loaded += OnLoaded;
    }

    private void OnLoaded(object sender, RoutedEventArgs e)
    {
        Loaded -= OnLoaded;

        if (GetTemplateChild(PartBellButton) is not ButtonBase bell) return;

        // 编程式创建 Flyout（避免 XAML 资源内联解析问题）
        var flyout = BuildFlyout();
        Attached.FlyoutService.SetFlyout(bell, flyout);
    }

    private Flyout BuildFlyout()
    {
        // 通知列表
        var items = new ItemsControl { Name = PartItemsList };
        items.ItemsSource = ToastService.Notifications;
        items.ClearValue(ItemsControl.StyleProperty);

        var nullToVis = (System.Windows.Data.IValueConverter?)TryFindResource("Toast.NullToVis");

        var msgBinding = new System.Windows.Data.Binding(nameof(NotificationItem.Message))
        {
            Converter = nullToVis
        };

        var itemTemplate = new DataTemplate(typeof(NotificationItem));
        var borderFactory = new FrameworkElementFactory(typeof(Border));
        borderFactory.SetValue(Border.PaddingProperty, new Thickness(16, 10, 16, 10));
        borderFactory.SetResourceReference(Border.BorderBrushProperty, "Mine.Brush.OutlineVariant");
        borderFactory.SetValue(Border.BorderThicknessProperty, new Thickness(0, 0, 0, 1));

        var gridFactory = new FrameworkElementFactory(typeof(Grid));

        var col0 = new FrameworkElementFactory(typeof(System.Windows.Controls.ColumnDefinition));
        col0.SetValue(System.Windows.Controls.ColumnDefinition.WidthProperty, GridLength.Auto);
        var col1 = new FrameworkElementFactory(typeof(System.Windows.Controls.ColumnDefinition));
        col1.SetValue(System.Windows.Controls.ColumnDefinition.WidthProperty, new GridLength(1, GridUnitType.Star));
        var col2 = new FrameworkElementFactory(typeof(System.Windows.Controls.ColumnDefinition));
        col2.SetValue(System.Windows.Controls.ColumnDefinition.WidthProperty, GridLength.Auto);
        gridFactory.AppendChild(col0);
        gridFactory.AppendChild(col1);
        gridFactory.AppendChild(col2);

        // 图标
        var icon = new FrameworkElementFactory(typeof(TextBlock));
        icon.SetBinding(TextBlock.TextProperty, new System.Windows.Data.Binding(nameof(NotificationItem.LevelIcon)));
        icon.SetResourceReference(TextBlock.FontFamilyProperty, "Mine.Font.MaterialSymbols");
        icon.SetValue(TextBlock.FontSizeProperty, 18d);
        icon.SetValue(TextBlock.VerticalAlignmentProperty, VerticalAlignment.Top);
        icon.SetValue(TextBlock.MarginProperty, new Thickness(0, 1, 10, 0));
        icon.SetResourceReference(TextBlock.ForegroundProperty, "Mine.Brush.OnSurfaceVariant");
        icon.SetValue(Grid.ColumnProperty, 0);
        gridFactory.AppendChild(icon);

        // 标题+消息
        var stack = new FrameworkElementFactory(typeof(StackPanel));
        stack.SetValue(StackPanel.OrientationProperty, Orientation.Vertical);
        stack.SetValue(Grid.ColumnProperty, 1);

        var title = new FrameworkElementFactory(typeof(TextBlock));
        title.SetBinding(TextBlock.TextProperty, new System.Windows.Data.Binding(nameof(NotificationItem.Title)));
        title.SetResourceReference(TextBlock.FontFamilyProperty, "Mine.FontFamily.Default");
        title.SetResourceReference(TextBlock.FontSizeProperty, "Mine.FontSize.LabelLarge");
        title.SetValue(TextBlock.FontWeightProperty, FontWeights.Medium);
        title.SetResourceReference(TextBlock.ForegroundProperty, "Mine.Brush.OnSurface");
        title.SetValue(TextBlock.TextWrappingProperty, TextWrapping.Wrap);
        stack.AppendChild(title);

        var msg = new FrameworkElementFactory(typeof(TextBlock));
        msg.SetBinding(TextBlock.TextProperty, new System.Windows.Data.Binding(nameof(NotificationItem.Message)));
        msg.SetResourceReference(TextBlock.FontFamilyProperty, "Mine.FontFamily.Default");
        msg.SetResourceReference(TextBlock.FontSizeProperty, "Mine.FontSize.BodySmall");
        msg.SetResourceReference(TextBlock.ForegroundProperty, "Mine.Brush.OnSurfaceVariant");
        msg.SetValue(TextBlock.TextWrappingProperty, TextWrapping.Wrap);
        msg.SetValue(TextBlock.MarginProperty, new Thickness(0, 2, 0, 0));
        if (nullToVis != null)
            msg.SetBinding(UIElement.VisibilityProperty, msgBinding);
        stack.AppendChild(msg);
        gridFactory.AppendChild(stack);

        // 时间
        var time = new FrameworkElementFactory(typeof(TextBlock));
        time.SetBinding(TextBlock.TextProperty, new System.Windows.Data.Binding(nameof(NotificationItem.TimeAgo)));
        time.SetResourceReference(TextBlock.FontFamilyProperty, "Mine.FontFamily.Default");
        time.SetResourceReference(TextBlock.FontSizeProperty, "Mine.FontSize.BodySmall");
        time.SetResourceReference(TextBlock.ForegroundProperty, "Mine.Brush.OnSurfaceVariant");
        time.SetValue(TextBlock.VerticalAlignmentProperty, VerticalAlignment.Top);
        time.SetValue(TextBlock.MarginProperty, new Thickness(8, 0, 0, 0));
        time.SetValue(Grid.ColumnProperty, 2);
        gridFactory.AppendChild(time);

        borderFactory.AppendChild(gridFactory);
        itemTemplate.VisualTree = borderFactory;
        itemTemplate.Seal();
        items.ItemTemplate = itemTemplate;

        var scrollViewer = new ScrollViewer
        {
            VerticalScrollBarVisibility   = ScrollBarVisibility.Auto,
            HorizontalScrollBarVisibility = ScrollBarVisibility.Disabled,
            Content                       = items
        };

        // 标题栏
        var markAllBtn = new Button { Name = PartMarkAllRead, Content = "全部已读",
                                     HorizontalAlignment = HorizontalAlignment.Right };
        markAllBtn.SetResourceReference(Button.StyleProperty, "Mine.Style.Button.Text");
        markAllBtn.SetResourceReference(Button.ForegroundProperty, "Mine.Brush.Primary");
        markAllBtn.Click += (_, _) => { ToastService.MarkAllRead(); RefreshBadge(); };

        var headerTitle = new TextBlock { Text = "通知中心", VerticalAlignment = VerticalAlignment.Center,
                                          FontWeight = FontWeights.SemiBold };
        headerTitle.SetResourceReference(TextBlock.FontFamilyProperty, "Mine.FontFamily.Default");
        headerTitle.SetResourceReference(TextBlock.FontSizeProperty, "Mine.FontSize.TitleMedium");
        headerTitle.SetResourceReference(TextBlock.ForegroundProperty, "Mine.Brush.OnSurface");

        var headerGrid = new Grid { Margin = new Thickness(16, 12, 12, 8) };
        headerGrid.Children.Add(headerTitle);
        headerGrid.Children.Add(markAllBtn);

        // 底部工具栏
        var clearBtn = new Button { Name = PartClearAll, Content = "清空所有通知",
                                    HorizontalAlignment = HorizontalAlignment.Center };
        clearBtn.SetResourceReference(Button.StyleProperty, "Mine.Style.Button.Text");
        clearBtn.SetResourceReference(Button.ForegroundProperty, "Mine.Brush.Error");
        clearBtn.Click += (_, _) => { ToastService.ClearAll(); RefreshBadge(); };

        var footerBorder = new Border { Padding = new Thickness(16, 8, 16, 8) };
        footerBorder.SetResourceReference(Border.BorderBrushProperty, "Mine.Brush.OutlineVariant");
        footerBorder.BorderThickness = new Thickness(0, 1, 0, 0);
        footerBorder.Child = clearBtn;

        // 组装内容 Grid
        var contentGrid = new Grid();
        contentGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
        contentGrid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) });
        contentGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
        Grid.SetRow(headerGrid,   0);
        Grid.SetRow(scrollViewer, 1);
        Grid.SetRow(footerBorder, 2);
        contentGrid.Children.Add(headerGrid);
        contentGrid.Children.Add(scrollViewer);
        contentGrid.Children.Add(footerBorder);

        var flyout = new Flyout
        {
            Placement        = System.Windows.Controls.Primitives.PlacementMode.Bottom,
            HorizontalOffset = -280,
            VerticalOffset   = 4,
            MinWidth         = 320,
            Width            = 320,
            MaxHeight        = 480,
            Content          = contentGrid
        };

        // Flyout 打开时标记全部已读
        DependencyPropertyDescriptor
            .FromProperty(Flyout.IsOpenProperty, typeof(Flyout))
            .AddValueChanged(flyout, (_, _) =>
            {
                if (flyout.IsOpen) { ToastService.MarkAllRead(); RefreshBadge(); }
            });

        return flyout;
    }

    private static T? FindNamedChild<T>(DependencyObject parent, string name) where T : FrameworkElement
    {
        for (var i = 0; i < VisualTreeHelper.GetChildrenCount(parent); i++)
        {
            var child = VisualTreeHelper.GetChild(parent, i);
            if (child is T fe && fe.Name == name) return fe;
            var found = FindNamedChild<T>(child, name);
            if (found != null) return found;
        }
        return null;
    }

    private void RefreshBadge()
    {
        var count   = ToastService.UnreadCount;
        UnreadCount = count;
        HasUnread   = count > 0;
    }


    internal static void RaiseBadgeChanged()
    {
        foreach (var wr in _instances.ToList())
            if (wr.TryGetTarget(out var nc))
                nc.RefreshBadge();
    }
}
