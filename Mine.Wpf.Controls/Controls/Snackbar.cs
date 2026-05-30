using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Animation;

namespace Mine.Wpf.Controls.Controls;

/// <summary>
/// Material 3 Snackbar — 轻量级消息提示条。
/// 通常由 <see cref="SnackbarService"/> 驱动；也可直接在 XAML 中使用。
/// </summary>
[TemplatePart(Name = PartRoot,           Type = typeof(Border))]
[TemplatePart(Name = PartActionButton,   Type = typeof(Button))]
[TemplatePart(Name = PartCloseButton,    Type = typeof(Border))]
public class Snackbar : ContentControl
{
    private const string PartRoot         = "PART_Root";
    private const string PartActionButton = "PART_ActionButton";
    private const string PartCloseButton  = "PART_CloseButton";

    private Border? _root;
    private Button? _actionBtn;
    private Border? _closeBtn;

    static Snackbar() =>
        DefaultStyleKeyProperty.OverrideMetadata(
            typeof(Snackbar), new FrameworkPropertyMetadata(typeof(Snackbar)));

    // ── ActionLabel ───────────────────────────────────────────────
    public static readonly DependencyProperty ActionLabelProperty =
        DependencyProperty.Register(nameof(ActionLabel), typeof(string), typeof(Snackbar),
            new PropertyMetadata(null));
    public string? ActionLabel
    {
        get => (string?)GetValue(ActionLabelProperty);
        set => SetValue(ActionLabelProperty, value);
    }

    // ── HasAction ─────────────────────────────────────────────────
    private static readonly DependencyPropertyKey HasActionKey =
        DependencyProperty.RegisterReadOnly(nameof(HasAction), typeof(bool), typeof(Snackbar),
            new PropertyMetadata(false));
    public static readonly DependencyProperty HasActionProperty = HasActionKey.DependencyProperty;
    public bool HasAction => (bool)GetValue(HasActionProperty);

    // ── IsOpen ────────────────────────────────────────────────────
    public static readonly DependencyProperty IsOpenProperty =
        DependencyProperty.Register(nameof(IsOpen), typeof(bool), typeof(Snackbar),
            new PropertyMetadata(false, OnIsOpenChanged));
    public bool IsOpen
    {
        get => (bool)GetValue(IsOpenProperty);
        set => SetValue(IsOpenProperty, value);
    }

    // ── Duration ──────────────────────────────────────────────────
    public static readonly DependencyProperty DurationProperty =
        DependencyProperty.Register(nameof(Duration), typeof(TimeSpan), typeof(Snackbar),
            new PropertyMetadata(TimeSpan.FromSeconds(4)));
    /// <summary>自动关闭延迟（默认 4 秒）。设为 <see cref="TimeSpan.Zero"/> 禁用自动关闭。</summary>
    public TimeSpan Duration
    {
        get => (TimeSpan)GetValue(DurationProperty);
        set => SetValue(DurationProperty, value);
    }

    // ── Events ────────────────────────────────────────────────────
    public static readonly RoutedEvent ActionClickedEvent =
        EventManager.RegisterRoutedEvent(nameof(ActionClicked), RoutingStrategy.Bubble,
            typeof(RoutedEventHandler), typeof(Snackbar));
    public event RoutedEventHandler ActionClicked
    {
        add    => AddHandler(ActionClickedEvent, value);
        remove => RemoveHandler(ActionClickedEvent, value);
    }

    // ── Template ──────────────────────────────────────────────────
    public override void OnApplyTemplate()
    {
        base.OnApplyTemplate();
        _root      = GetTemplateChild(PartRoot)         as Border;
        _actionBtn = GetTemplateChild(PartActionButton) as Button;
        _closeBtn  = GetTemplateChild(PartCloseButton)  as Border;

        if (_actionBtn != null)
            _actionBtn.Click += (_, _) =>
            {
                RaiseEvent(new RoutedEventArgs(ActionClickedEvent));
                IsOpen = false;
            };
        if (_closeBtn != null)
            _closeBtn.MouseLeftButtonDown += (_, _) => IsOpen = false;
    }

    // ── IsOpen change ─────────────────────────────────────────────
    private static void OnIsOpenChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is Snackbar sb) sb.OnIsOpenChanged((bool)e.NewValue);
    }

    private System.Windows.Threading.DispatcherTimer? _autoCloseTimer;

    private void OnIsOpenChanged(bool isOpen)
    {
        SetValue(HasActionKey, ActionLabel != null);

        if (isOpen)
        {
            Visibility = Visibility.Visible;
            if (_root != null) AnimateIn();
            StartAutoClose();
        }
        else
        {
            _autoCloseTimer?.Stop();
            _autoCloseTimer = null;
            AnimateOut();
        }
    }

    private void AnimateIn()
    {
        if (_root == null) return;
        _root.BeginAnimation(OpacityProperty, null);
        _root.BeginAnimation(OpacityProperty,
            new DoubleAnimation(0, 1, new Duration(TimeSpan.FromMilliseconds(200)))
            { EasingFunction = new CubicEase { EasingMode = EasingMode.EaseOut } });

        _root.BeginAnimation(MarginProperty,
            new ThicknessAnimation(
                new Thickness(0, 24, 0, -24), new Thickness(0),
                new Duration(TimeSpan.FromMilliseconds(200)))
            { EasingFunction = new CubicEase { EasingMode = EasingMode.EaseOut } });
    }

    private void AnimateOut()
    {
        void RemoveSelf()
        {
            // 在 SnackbarService 的 StackPanel 中自我移除，触发其余项目自动补位；
            // 独立使用时回退到 Collapsed
            if (Parent is Panel panel)
                panel.Children.Remove(this);
            else
                Visibility = Visibility.Collapsed;
        }

        if (_root == null) { RemoveSelf(); return; }

        var anim = new DoubleAnimation(1, 0, new Duration(TimeSpan.FromMilliseconds(150)))
        {
            EasingFunction = new CubicEase { EasingMode = EasingMode.EaseIn }
        };
        anim.Completed += (_, _) => RemoveSelf();
        _root.BeginAnimation(OpacityProperty, anim);
    }

    private void StartAutoClose()
    {
        _autoCloseTimer?.Stop();
        if (Duration == TimeSpan.Zero) return;

        _autoCloseTimer = new System.Windows.Threading.DispatcherTimer { Interval = Duration };
        _autoCloseTimer.Tick += (_, _) =>
        {
            _autoCloseTimer!.Stop();
            _autoCloseTimer = null;
            IsOpen = false;
        };
        _autoCloseTimer.Start();
    }

    /// <summary>显示 Snackbar 消息。</summary>
    public void Show(string message, string? actionLabel = null, TimeSpan? duration = null)
    {
        Content     = message;
        ActionLabel = actionLabel;
        // 始终重置 Duration，避免上次设置的值污染下次调用
        Duration    = duration ?? TimeSpan.FromSeconds(4);
        IsOpen      = true;
    }
}

// ══════════════════════════════════════════════════════════════════
/// <summary>
/// 全局 Snackbar 服务。每次调用 Show 创建一个独立实例并叠加在窗口底部，
/// 各条消息从下往上排列，关闭后自动补位。
/// </summary>
public static class SnackbarService
{
    private const string ContainerTag = "__Mine.SnackbarContainer__";

    public static void Show(string message, string? actionLabel = null,
                            TimeSpan? duration = null, Action? onAction = null)
    {
        var window = Helpers.WindowOverlay.GetActiveWindow();
        if (window == null) return;

        window.Dispatcher.BeginInvoke(() =>
        {
            var container = GetOrCreateContainer(window);

            var snackbar = new Snackbar
            {
                HorizontalAlignment = HorizontalAlignment.Center,
                // 相邻条目间距（第一条无上边距也可，StackPanel 会自然排列）
                Margin = new Thickness(0, 8, 0, 0),
            };

            if (onAction != null)
            {
                RoutedEventHandler? handler = null;
                handler = (_, _) =>
                {
                    onAction();
                    snackbar.ActionClicked -= handler;
                };
                snackbar.ActionClicked += handler;
            }

            // 新消息追加到末尾 → 出现在栈底部（最靠近屏幕底部）
            container.Children.Add(snackbar);
            snackbar.Show(message, actionLabel, duration);
        });
    }

    /// <summary>获取或创建窗口底部的 Snackbar 容器 StackPanel。</summary>
    private static StackPanel GetOrCreateContainer(System.Windows.Window window)
    {
        var overlay = Helpers.WindowOverlay.GetOrCreate(window);
        var existing = overlay.Children.OfType<StackPanel>()
            .FirstOrDefault(p => p.Tag as string == ContainerTag);
        if (existing != null) return existing;

        var container = new StackPanel
        {
            Tag                 = ContainerTag,
            VerticalAlignment   = VerticalAlignment.Bottom,
            HorizontalAlignment = HorizontalAlignment.Center,
            Margin              = new Thickness(0, 0, 0, 24),   // 距屏幕底部
        };
        Panel.SetZIndex(container, 9999);
        overlay.Children.Add(container);
        return container;
    }
}

