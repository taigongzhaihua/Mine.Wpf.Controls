using System.Windows;
using System.Windows.Automation;
using System.Windows.Automation.Peers;
using System.Windows.Automation.Provider;
using System.Windows.Controls;
using System.Windows.Input;

namespace Mine.Wpf.Controls.Controls;

/// <summary>MD3 Chip 变体。</summary>
public enum ChipVariant
{
    /// <summary>辅助芯片：触发单次操作，不保留选中态。</summary>
    Assist,
    /// <summary>过滤芯片：可切换选中/取消，选中时显示勾选图标。</summary>
    Filter,
    /// <summary>输入芯片：表示已输入的标签，带删除按钮。</summary>
    Input,
    /// <summary>建议芯片：点击即触发，不保留选中态。</summary>
    Suggestion,
}

/// <summary>
/// Material Design 3 Chip 控件。
/// 继承 Control（而非 ToggleButton）以避免 ButtonBase 鼠标捕获破坏子元素交互。
/// </summary>
[TemplateVisualState(Name = "Normal",   GroupName = "CommonStates")]
[TemplateVisualState(Name = "Checked",  GroupName = "CommonStates")]
[TemplateVisualState(Name = "Disabled", GroupName = "CommonStates")]
[TemplatePart(Name = "PART_DeleteButton", Type = typeof(UIElement))]
public class Chip : Control
{
    static Chip()
    {
        DefaultStyleKeyProperty.OverrideMetadata(
            typeof(Chip), new FrameworkPropertyMetadata(typeof(Chip)));
    }

    protected override AutomationPeer OnCreateAutomationPeer() => new ChipAutomationPeer(this);

    // ── Variant ───────────────────────────────────────────────────
    public static readonly DependencyProperty VariantProperty =
        DependencyProperty.Register(nameof(Variant), typeof(ChipVariant), typeof(Chip),
            new PropertyMetadata(ChipVariant.Assist, OnVariantChanged));

    public ChipVariant Variant
    {
        get => (ChipVariant)GetValue(VariantProperty);
        set => SetValue(VariantProperty, value);
    }

    // ── Content ──────────────────────────────────────────────────
    public static readonly DependencyProperty ContentProperty =
        DependencyProperty.Register(nameof(Content), typeof(object), typeof(Chip),
            new PropertyMetadata(null));

    public object? Content
    {
        get => GetValue(ContentProperty);
        set => SetValue(ContentProperty, value);
    }

    // ── IsChecked（Filter 变体用）────────────────────────────────
    public static readonly DependencyProperty IsCheckedProperty =
        DependencyProperty.Register(nameof(IsChecked), typeof(bool), typeof(Chip),
            new PropertyMetadata(false, OnIsCheckedChanged));

    public bool IsChecked
    {
        get => (bool)GetValue(IsCheckedProperty);
        set => SetValue(IsCheckedProperty, value);
    }

    // ── Icon ─────────────────────────────────────────────────────
    public static readonly DependencyProperty IconProperty =
        DependencyProperty.Register(nameof(Icon), typeof(string), typeof(Chip),
            new PropertyMetadata(null, OnIconChanged));

    public string? Icon
    {
        get => (string?)GetValue(IconProperty);
        set => SetValue(IconProperty, value);
    }

    // ── Avatar ────────────────────────────────────────────────────
    public static readonly DependencyProperty AvatarProperty =
        DependencyProperty.Register(nameof(Avatar), typeof(object), typeof(Chip),
            new PropertyMetadata(null));

    public object? Avatar
    {
        get => GetValue(AvatarProperty);
        set => SetValue(AvatarProperty, value);
    }

    // ── Click 路由事件 ─────────────────────────────────────────────
    public static readonly RoutedEvent ClickEvent =
        EventManager.RegisterRoutedEvent(nameof(Click), RoutingStrategy.Bubble,
            typeof(RoutedEventHandler), typeof(Chip));

    public event RoutedEventHandler Click
    {
        add    => AddHandler(ClickEvent, value);
        remove => RemoveHandler(ClickEvent, value);
    }

    // ── Deleted 路由事件 ───────────────────────────────────────────
    public static readonly RoutedEvent DeletedEvent =
        EventManager.RegisterRoutedEvent(nameof(Deleted), RoutingStrategy.Bubble,
            typeof(RoutedEventHandler), typeof(Chip));

    public event RoutedEventHandler Deleted
    {
        add    => AddHandler(DeletedEvent, value);
        remove => RemoveHandler(DeletedEvent, value);
    }

    // ── DeleteCommand ─────────────────────────────────────────────
    public static readonly DependencyProperty DeleteCommandProperty =
        DependencyProperty.Register(nameof(DeleteCommand), typeof(ICommand), typeof(Chip),
            new PropertyMetadata(null));

    public ICommand? DeleteCommand
    {
        get => (ICommand?)GetValue(DeleteCommandProperty);
        set => SetValue(DeleteCommandProperty, value);
    }

    public static readonly DependencyProperty DeleteCommandParameterProperty =
        DependencyProperty.Register(nameof(DeleteCommandParameter), typeof(object), typeof(Chip),
            new PropertyMetadata(null));

    public object? DeleteCommandParameter
    {
        get => GetValue(DeleteCommandParameterProperty);
        set => SetValue(DeleteCommandParameterProperty, value);
    }

    // ── Template parts ────────────────────────────────────────────
    private TextBlock? _leadingIcon;
    private UIElement? _deleteButton;

    public override void OnApplyTemplate()
    {
        base.OnApplyTemplate();

        _leadingIcon = GetTemplateChild("LeadingIcon") as TextBlock;

        if (_deleteButton != null)
            _deleteButton.MouseLeftButtonUp -= OnDeleteButtonMouseUp;

        _deleteButton = GetTemplateChild("PART_DeleteButton") as UIElement;

        if (_deleteButton != null)
            _deleteButton.MouseLeftButtonUp += OnDeleteButtonMouseUp;

        UpdateLeadingIcon();
        UpdateVisualState(false);
    }

    // ── 鼠标交互（自行实现，无 ButtonBase 捕获干扰）──────────────
    protected override void OnMouseLeftButtonUp(MouseButtonEventArgs e)
    {
        base.OnMouseLeftButtonUp(e);
        if (!IsEnabled || e.Handled) return;

        if (Variant == ChipVariant.Filter)
            IsChecked = !IsChecked;

        RaiseEvent(new RoutedEventArgs(ClickEvent, this));
        e.Handled = true;
    }

    private void OnDeleteButtonMouseUp(object sender, MouseButtonEventArgs e)
    {
        if (!IsEnabled) return;
        e.Handled = true; // 阻止冒泡到 Chip 触发 Click
        RaiseEvent(new RoutedEventArgs(DeletedEvent, this));
        DeleteCommand?.Execute(DeleteCommandParameter);
    }

    private void UpdateLeadingIcon()
    {
        if (_leadingIcon == null) return;
        _leadingIcon.Visibility = string.IsNullOrEmpty(Icon)
            ? Visibility.Collapsed
            : Visibility.Visible;
    }

    private static void OnVariantChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        => (d as Chip)?.UpdateVisualState(false);

    private static void OnIsCheckedChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        => (d as Chip)?.UpdateVisualState(true);

    private static void OnIconChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        => (d as Chip)?.UpdateLeadingIcon();

    private void UpdateVisualState(bool animate)
    {
        if (!IsEnabled)
            VisualStateManager.GoToState(this, "Disabled", animate);
        else if (IsChecked)
            VisualStateManager.GoToState(this, "Checked", animate);
        else
            VisualStateManager.GoToState(this, "Normal", animate);
    }
}

/// <summary>暴露 <see cref="Chip"/> 的点击 / 选中状态给屏幕阅读器 / UI 自动化。</summary>
public class ChipAutomationPeer : FrameworkElementAutomationPeer, IInvokeProvider, IToggleProvider
{
    public ChipAutomationPeer(Chip owner) : base(owner) { }

    private Chip Control => (Chip)Owner;

    public override object GetPattern(PatternInterface patternInterface)
    {
        if (Control.Variant == ChipVariant.Filter)
        {
            if (patternInterface == PatternInterface.Toggle) return this;
        }
        else if (patternInterface == PatternInterface.Invoke)
        {
            return this;
        }

        return base.GetPattern(patternInterface);
    }

    protected override string GetClassNameCore() => nameof(Chip);
    protected override AutomationControlType GetAutomationControlTypeCore() => AutomationControlType.Button;

    protected override string GetNameCore()
    {
        var name = base.GetNameCore();
        if (!string.IsNullOrEmpty(name)) return name;
        return Control.Content switch
        {
            string s => s,
            null => string.Empty,
            var o => o.ToString() ?? string.Empty,
        };
    }

    void IInvokeProvider.Invoke() => Control.RaiseEvent(new RoutedEventArgs(Chip.ClickEvent, Control));

    ToggleState IToggleProvider.ToggleState => Control.IsChecked ? ToggleState.On : ToggleState.Off;

    void IToggleProvider.Toggle() => Control.IsChecked = !Control.IsChecked;
}
