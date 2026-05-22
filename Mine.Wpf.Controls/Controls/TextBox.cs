using System.Windows;
using System.Windows.Controls;
using Mine.Wpf.Controls.Primitives;
namespace Mine.Wpf.Controls.Controls;
/// <summary>
/// Material 3 文本框，支持 Filled / Outlined 变体、
/// 浮动标签、辅助文本、前缀/后缀、可清空、字符计数器。
/// </summary>
[TemplatePart(Name = PartHint,        Type = typeof(TextBlock))]
[TemplatePart(Name = PartHelper,      Type = typeof(TextBlock))]
[TemplatePart(Name = PartClearButton, Type = typeof(System.Windows.Controls.Button))]
[TemplateVisualState(Name = "Normal",   GroupName = "CommonStates")]
[TemplateVisualState(Name = "Focused",  GroupName = "CommonStates")]
[TemplateVisualState(Name = "Disabled", GroupName = "CommonStates")]
[TemplateVisualState(Name = "Error",    GroupName = "ValidationStates")]
[TemplateVisualState(Name = "Valid",    GroupName = "ValidationStates")]
public class TextBox : System.Windows.Controls.TextBox
{
    private const string PartHint        = "PART_Hint";
    private const string PartHelper      = "PART_HelperText";
    private const string PartClearButton = "PART_ClearButton";
    private const string PartContainer   = "Container";

    private NotchedOutlineBorder? _notchedBorder;
    private TextBlock?            _hintBlock;
    static TextBox()
    {
        DefaultStyleKeyProperty.OverrideMetadata(
            typeof(TextBox),
            new FrameworkPropertyMetadata(typeof(TextBox)));
    }
    // ── 变体 ─────────────────────────────────────────────────────
    public static readonly DependencyProperty VariantProperty =
        DependencyProperty.Register(nameof(Variant), typeof(TextFieldVariant), typeof(TextBox),
            new PropertyMetadata(TextFieldVariant.Filled));
    public TextFieldVariant Variant
    {
        get => (TextFieldVariant)GetValue(VariantProperty);
        set => SetValue(VariantProperty, value);
    }
    // ── 浮动标签（Hint） ──────────────────────────────────────────
    public static readonly DependencyProperty HintProperty =
        DependencyProperty.Register(nameof(Hint), typeof(string), typeof(TextBox),
            new PropertyMetadata(null));
    public string? Hint
    {
        get => (string?)GetValue(HintProperty);
        set => SetValue(HintProperty, value);
    }
    // ── 辅助文本 ──────────────────────────────────────────────────
    public static readonly DependencyProperty HelperTextProperty =
        DependencyProperty.Register(nameof(HelperText), typeof(string), typeof(TextBox),
            new PropertyMetadata(null));
    public string? HelperText
    {
        get => (string?)GetValue(HelperTextProperty);
        set => SetValue(HelperTextProperty, value);
    }
    // ── 前缀 / 后缀 ───────────────────────────────────────────────
    public static readonly DependencyProperty PrefixTextProperty =
        DependencyProperty.Register(nameof(PrefixText), typeof(string), typeof(TextBox),
            new PropertyMetadata(null));
    public string? PrefixText
    {
        get => (string?)GetValue(PrefixTextProperty);
        set => SetValue(PrefixTextProperty, value);
    }
    public static readonly DependencyProperty SuffixTextProperty =
        DependencyProperty.Register(nameof(SuffixText), typeof(string), typeof(TextBox),
            new PropertyMetadata(null));
    public string? SuffixText
    {
        get => (string?)GetValue(SuffixTextProperty);
        set => SetValue(SuffixTextProperty, value);
    }
    // ── 图标 ─────────────────────────────────────────────────────
    public static readonly DependencyProperty LeadingIconProperty =
        DependencyProperty.Register(nameof(LeadingIcon), typeof(object), typeof(TextBox),
            new PropertyMetadata(null));
    public object? LeadingIcon
    {
        get => GetValue(LeadingIconProperty);
        set => SetValue(LeadingIconProperty, value);
    }
    public static readonly DependencyProperty TrailingIconProperty =
        DependencyProperty.Register(nameof(TrailingIcon), typeof(object), typeof(TextBox),
            new PropertyMetadata(null));
    public object? TrailingIcon
    {
        get => GetValue(TrailingIconProperty);
        set => SetValue(TrailingIconProperty, value);
    }
    // ── 可清空 ────────────────────────────────────────────────────
    public static readonly DependencyProperty IsClearableProperty =
        DependencyProperty.Register(nameof(IsClearable), typeof(bool), typeof(TextBox),
            new PropertyMetadata(false));
    public bool IsClearable
    {
        get => (bool)GetValue(IsClearableProperty);
        set => SetValue(IsClearableProperty, value);
    }
    // ── 错误状态 / 错误信息 ───────────────────────────────────────
    public static readonly DependencyProperty HasErrorProperty =
        DependencyProperty.Register(nameof(HasError), typeof(bool), typeof(TextBox),
            new PropertyMetadata(false, OnHasErrorChanged));
    public bool HasError
    {
        get => (bool)GetValue(HasErrorProperty);
        set => SetValue(HasErrorProperty, value);
    }
    public static readonly DependencyProperty ErrorMessageProperty =
        DependencyProperty.Register(nameof(ErrorMessage), typeof(string), typeof(TextBox),
            new PropertyMetadata(null));
    public string? ErrorMessage
    {
        get => (string?)GetValue(ErrorMessageProperty);
        set => SetValue(ErrorMessageProperty, value);
    }
    // ── 字符计数器 ────────────────────────────────────────────────
    public static readonly DependencyProperty ShowCharacterCounterProperty =
        DependencyProperty.Register(nameof(ShowCharacterCounter), typeof(bool), typeof(TextBox),
            new PropertyMetadata(false));
    public bool ShowCharacterCounter
    {
        get => (bool)GetValue(ShowCharacterCounterProperty);
        set => SetValue(ShowCharacterCounterProperty, value);
    }
    // ── 圆角 ─────────────────────────────────────────────────────
    public static readonly DependencyProperty CornerRadiusProperty =
        DependencyProperty.Register(nameof(CornerRadius), typeof(CornerRadius), typeof(TextBox),
            new PropertyMetadata(new CornerRadius(4, 4, 0, 0)));
    public CornerRadius CornerRadius
    {
        get => (CornerRadius)GetValue(CornerRadiusProperty);
        set => SetValue(CornerRadiusProperty, value);
    }
    // ── 是否有文本（只读） ────────────────────────────────────────
    private static readonly DependencyPropertyKey HasTextPropertyKey =
        DependencyProperty.RegisterReadOnly(nameof(HasText), typeof(bool), typeof(TextBox),
            new PropertyMetadata(false));
    public static readonly DependencyProperty HasTextProperty = HasTextPropertyKey.DependencyProperty;
    public bool HasText => (bool)GetValue(HasTextProperty);
    protected override void OnTextChanged(TextChangedEventArgs e)
    {
        base.OnTextChanged(e);
        SetValue(HasTextPropertyKey, !string.IsNullOrEmpty(Text));
        UpdateLabelState(true);
    }
    // ── 模板 / 状态 ───────────────────────────────────────────────
    public override void OnApplyTemplate()
    {
        base.OnApplyTemplate();
        if (GetTemplateChild(PartClearButton) is System.Windows.Controls.Button clear)
            clear.Click += (_, _) => { Clear(); Focus(); };

        _notchedBorder = GetTemplateChild(PartContainer) as NotchedOutlineBorder;
        _hintBlock     = GetTemplateChild(PartHint)      as TextBlock;

        // 初始状态不带过度；如果已有文字，等首次布局完成后再同步缺口
        UpdateStates(false);
        if (_hintBlock != null && (IsFocused || HasText))
        {
            _hintBlock.LayoutUpdated += OnHintFirstLayout;
        }
    }

    private void OnHintFirstLayout(object? sender, EventArgs e)
    {
        if (_hintBlock == null) return;
        _hintBlock.LayoutUpdated -= OnHintFirstLayout;
        UpdateLabelState(false);
    }
    protected override void OnGotFocus(RoutedEventArgs e)
    {
        base.OnGotFocus(e);
        VisualStateManager.GoToState(this, "Focused", true);
        UpdateLabelState(true);
    }
    protected override void OnLostFocus(RoutedEventArgs e)
    {
        base.OnLostFocus(e);
        VisualStateManager.GoToState(this, IsEnabled ? "Normal" : "Disabled", true);
        UpdateLabelState(true);
    }
    protected override void OnMouseEnter(System.Windows.Input.MouseEventArgs e)
    {
        base.OnMouseEnter(e);
        SyncCommonState(false);
    }
    protected override void OnMouseLeave(System.Windows.Input.MouseEventArgs e)
    {
        base.OnMouseLeave(e);
        SyncCommonState(false);
    }
    // 拦截 WPF 内部通过 IsMouseOver 属性变更触发的 UpdateVisualState
    protected override void OnPropertyChanged(DependencyPropertyChangedEventArgs e)
    {
        base.OnPropertyChanged(e);
        if (e.Property == IsMouseOverProperty)
            SyncCommonState(false);
    }
    /// <summary>根据当前真实状态同步 CommonStates，防止基类鼠标事件干扰 VSM。</summary>
    private void SyncCommonState(bool useTransitions)
    {
        if (!IsEnabled)
            VisualStateManager.GoToState(this, "Disabled", useTransitions);
        else if (IsFocused)
            VisualStateManager.GoToState(this, "Focused", useTransitions);
        else
            VisualStateManager.GoToState(this, "Normal", useTransitions);
    }
    /// <summary>根据 IsFocused / HasText 切换标签浮动状态，并同步缺口宽度。</summary>
    private void UpdateLabelState(bool useTransitions)
    {
        bool floated = IsFocused || HasText;
        VisualStateManager.GoToState(this, floated ? "LabelFloated" : "LabelNormal", useTransitions);

        // Outlined 变体：更新缺口宽度
        if (_notchedBorder != null && _hintBlock != null && Variant == TextFieldVariant.Outlined)
        {
            if (floated)
            {
                const double gap = 4.0; // 标签两���各留 4px 间隙
                // NotchStart：从标签左边距往左 gap 个像素
                _notchedBorder.NotchStart = _hintBlock.Margin.Left - gap;
                // NotchWidth：标签缩放后视觉宽度 + 两侧间隙
                // ActualWidth 是布局宽度（不受 RenderTransform 影响），始终为原始尺寸
                _notchedBorder.NotchWidth = _hintBlock.ActualWidth * 0.75 + gap * 2;
            }
            else
            {
                _notchedBorder.NotchWidth = 0;
            }
        }
    }
    private void UpdateStates(bool useTransitions)
    {
        SyncCommonState(useTransitions);
        VisualStateManager.GoToState(this, HasError ? "Error" : "Valid", useTransitions);
        UpdateLabelState(useTransitions);
    }
    private static void OnHasErrorChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is TextBox tb)
            VisualStateManager.GoToState(tb, (bool)e.NewValue ? "Error" : "Valid", true);
    }
}
/// <summary>文本框变体枚举。</summary>
public enum TextFieldVariant { Filled, Outlined }
