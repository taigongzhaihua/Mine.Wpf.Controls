using System.Collections;
using System.Collections.ObjectModel;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using Mine.Wpf.Controls.Primitives;

namespace Mine.Wpf.Controls.Controls;

/// <summary>
/// Material Design 3 自动完成输入框控件。
/// 支持 Outlined / Filled 变体、浮动标签、自动过滤建议列表、键盘导航。
/// </summary>
[TemplatePart(Name = PartHint,            Type = typeof(TextBlock))]
[TemplatePart(Name = PartContainer,       Type = typeof(NotchedOutlineBorder))]
[TemplatePart(Name = PartSuggestionsPopup, Type = typeof(Popup))]
[TemplatePart(Name = PartSuggestionsList,  Type = typeof(ListBox))]
[TemplateVisualState(Name = "Normal",       GroupName = "CommonStates")]
[TemplateVisualState(Name = "Focused",      GroupName = "CommonStates")]
[TemplateVisualState(Name = "Disabled",     GroupName = "CommonStates")]
[TemplateVisualState(Name = "LabelNormal",  GroupName = "LabelStates")]
[TemplateVisualState(Name = "LabelFloated", GroupName = "LabelStates")]
public class AutoCompleteBox : TextBox
{
    private const string PartHint            = "PART_Hint";
    private const string PartContainer       = "Container";
    private const string PartSuggestionsPopup = "PART_SuggestionsPopup";
    private const string PartSuggestionsList  = "PART_SuggestionsList";

    static AutoCompleteBox()
    {
        DefaultStyleKeyProperty.OverrideMetadata(
            typeof(AutoCompleteBox),
            new FrameworkPropertyMetadata(typeof(AutoCompleteBox)));
    }

    // 内部过滤结果集，绑定到模板中的 ListBox
    private readonly ObservableCollection<object> _filteredItems = new();

    // ── Variant ────────────────────────────────────────────────────
    public static readonly DependencyProperty VariantProperty =
        DependencyProperty.Register(nameof(Variant), typeof(TextFieldVariant), typeof(AutoCompleteBox),
            new PropertyMetadata(TextFieldVariant.Outlined));

    public TextFieldVariant Variant
    {
        get => (TextFieldVariant)GetValue(VariantProperty);
        set => SetValue(VariantProperty, value);
    }

    // ── Hint (浮动标签) ────────────────────────────────────────────
    public static readonly DependencyProperty HintProperty =
        DependencyProperty.Register(nameof(Hint), typeof(string), typeof(AutoCompleteBox),
            new PropertyMetadata(null));

    public string? Hint
    {
        get => (string?)GetValue(HintProperty);
        set => SetValue(HintProperty, value);
    }

    // ── HelperText ────────────────────────────────────────────────
    public static readonly DependencyProperty HelperTextProperty =
        DependencyProperty.Register(nameof(HelperText), typeof(string), typeof(AutoCompleteBox),
            new PropertyMetadata(null));

    public string? HelperText
    {
        get => (string?)GetValue(HelperTextProperty);
        set => SetValue(HelperTextProperty, value);
    }

    // ── HasError ──────────────────────────────────────────────────
    public static readonly DependencyProperty HasErrorProperty =
        DependencyProperty.Register(nameof(HasError), typeof(bool), typeof(AutoCompleteBox),
            new PropertyMetadata(false, (d, _) => ((AutoCompleteBox)d).UpdateStates(true)));

    public bool HasError
    {
        get => (bool)GetValue(HasErrorProperty);
        set => SetValue(HasErrorProperty, value);
    }

    // ── 建议列表数据源 ────────────────────────────────────────────
    public static readonly DependencyProperty SuggestionsSourceProperty =
        DependencyProperty.Register(nameof(SuggestionsSource), typeof(IEnumerable), typeof(AutoCompleteBox),
            new PropertyMetadata(null, OnSuggestionsSourceChanged));

    public IEnumerable? SuggestionsSource
    {
        get => (IEnumerable?)GetValue(SuggestionsSourceProperty);
        set => SetValue(SuggestionsSourceProperty, value);
    }

    private static void OnSuggestionsSourceChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        => ((AutoCompleteBox)d).UpdateSuggestions();

    // ── 自动过滤 ──────────────────────────────────────────────────
    public static readonly DependencyProperty AutoFilterProperty =
        DependencyProperty.Register(nameof(AutoFilter), typeof(bool), typeof(AutoCompleteBox),
            new PropertyMetadata(true));

    public bool AutoFilter
    {
        get => (bool)GetValue(AutoFilterProperty);
        set => SetValue(AutoFilterProperty, value);
    }

    // ── 用于显示/过滤的成员路径 ───────────────────────────────────
    public static readonly DependencyProperty SuggestionDisplayMemberPathProperty =
        DependencyProperty.Register(nameof(SuggestionDisplayMemberPath), typeof(string), typeof(AutoCompleteBox),
            new PropertyMetadata(null));

    public string? SuggestionDisplayMemberPath
    {
        get => (string?)GetValue(SuggestionDisplayMemberPathProperty);
        set => SetValue(SuggestionDisplayMemberPathProperty, value);
    }

    // ── 自定义建议项模板 ──────────────────────────────────────────
    public static readonly DependencyProperty SuggestionItemTemplateProperty =
        DependencyProperty.Register(nameof(SuggestionItemTemplate), typeof(DataTemplate), typeof(AutoCompleteBox),
            new PropertyMetadata(null));

    public DataTemplate? SuggestionItemTemplate
    {
        get => (DataTemplate?)GetValue(SuggestionItemTemplateProperty);
        set => SetValue(SuggestionItemTemplateProperty, value);
    }

    // ── 最大显示建议条数 ──────────────────────────────────────────
    public static readonly DependencyProperty MaxSuggestionsProperty =
        DependencyProperty.Register(nameof(MaxSuggestions), typeof(int), typeof(AutoCompleteBox),
            new PropertyMetadata(8));

    public int MaxSuggestions
    {
        get => (int)GetValue(MaxSuggestionsProperty);
        set => SetValue(MaxSuggestionsProperty, value);
    }

    // ── 最小触发过滤的字符数 ──────────────────────────────────────
    public static readonly DependencyProperty MinimumPrefixLengthProperty =
        DependencyProperty.Register(nameof(MinimumPrefixLength), typeof(int), typeof(AutoCompleteBox),
            new PropertyMetadata(0));

    public int MinimumPrefixLength
    {
        get => (int)GetValue(MinimumPrefixLengthProperty);
        set => SetValue(MinimumPrefixLengthProperty, value);
    }

    // ── 下拉是否打开 ──────────────────────────────────────────────
    public static readonly DependencyProperty IsDropDownOpenProperty =
        DependencyProperty.Register(nameof(IsDropDownOpen), typeof(bool), typeof(AutoCompleteBox),
            new PropertyMetadata(false, OnIsDropDownOpenChanged));

    public bool IsDropDownOpen
    {
        get => (bool)GetValue(IsDropDownOpenProperty);
        set => SetValue(IsDropDownOpenProperty, value);
    }

    private static void OnIsDropDownOpenChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        var box = (AutoCompleteBox)d;
        if (!(bool)e.NewValue && box._list != null)
            box._list.SelectedIndex = -1;
    }

    // ── 当前选中的建议项 ──────────────────────────────────────────
    public static readonly DependencyProperty SelectedSuggestionProperty =
        DependencyProperty.Register(nameof(SelectedSuggestion), typeof(object), typeof(AutoCompleteBox),
            new PropertyMetadata(null));

    public object? SelectedSuggestion
    {
        get => GetValue(SelectedSuggestionProperty);
        set => SetValue(SelectedSuggestionProperty, value);
    }

    // ── 过滤后的结果集（只读 DP，供模板 ListBox 绑定） ───────────
    private static readonly DependencyPropertyKey FilteredSuggestionsPropertyKey =
        DependencyProperty.RegisterReadOnly(nameof(FilteredSuggestions), typeof(IEnumerable), typeof(AutoCompleteBox),
            new PropertyMetadata(null));

    public static readonly DependencyProperty FilteredSuggestionsProperty =
        FilteredSuggestionsPropertyKey.DependencyProperty;

    public IEnumerable FilteredSuggestions => (IEnumerable)GetValue(FilteredSuggestionsProperty);

    // ── 建议被选中事件 ────────────────────────────────────────────
    public static readonly RoutedEvent SuggestionSelectedEvent =
        EventManager.RegisterRoutedEvent(nameof(SuggestionSelected), RoutingStrategy.Bubble,
            typeof(SuggestionSelectedEventHandler), typeof(AutoCompleteBox));

    public event SuggestionSelectedEventHandler SuggestionSelected
    {
        add => AddHandler(SuggestionSelectedEvent, value);
        remove => RemoveHandler(SuggestionSelectedEvent, value);
    }

    // ── Template parts ────────────────────────────────────────────
    private NotchedOutlineBorder? _notchedBorder;
    private TextBlock? _hintBlock;
    private ScaleTransform? _hintScale;
    private TranslateTransform? _hintTranslate;
    private Popup? _popup;
    private ListBox? _list;

    public override void OnApplyTemplate()
    {
        base.OnApplyTemplate();

        // 解绑旧处理器
        if (_list != null)
        {
            _list.PreviewKeyDown -= OnListPreviewKeyDown;
            _list.MouseLeftButtonUp -= OnListMouseLeftButtonUp;
        }

        _notchedBorder = GetTemplateChild(PartContainer) as NotchedOutlineBorder;
        _hintBlock = GetTemplateChild(PartHint) as TextBlock;
        _popup = GetTemplateChild(PartSuggestionsPopup) as Popup;
        _list = GetTemplateChild(PartSuggestionsList) as ListBox;

        if (_hintBlock != null)
        {
            _hintScale = new ScaleTransform(1, 1);
            _hintTranslate = new TranslateTransform(0, 0);
            _hintBlock.RenderTransform = new TransformGroup
            {
                Children = { _hintScale, _hintTranslate }
            };
        }

        if (_list != null)
        {
            _list.PreviewKeyDown += OnListPreviewKeyDown;
            _list.MouseLeftButtonUp += OnListMouseLeftButtonUp;
        }

        if (_hintBlock != null && !string.IsNullOrEmpty(Text))
            _hintBlock.LayoutUpdated += OnHintFirstLayout;

        SetValue(FilteredSuggestionsPropertyKey, _filteredItems);
        UpdateStates(false);
        UpdateSuggestions();
    }

    private void OnHintFirstLayout(object? sender, EventArgs e)
    {
        if (_hintBlock == null) return;
        _hintBlock.LayoutUpdated -= OnHintFirstLayout;
        UpdateLabelState(false);
    }

    // ── 文本变化 ──────────────────────────────────────────────────
    protected override void OnTextChanged(TextChangedEventArgs e)
    {
        base.OnTextChanged(e);
        UpdateLabelState(true);
        UpdateSuggestions();
    }

    // ── 键盘：AutoCompleteBox 自身 ────────────────────────────────
    protected override void OnPreviewKeyDown(KeyEventArgs e)
    {
        base.OnPreviewKeyDown(e);
        switch (e.Key)
        {
            case Key.Enter:
                if (IsDropDownOpen && _list?.SelectedItem != null)
                {
                    ConfirmSuggestion(_list.SelectedItem);
                    e.Handled = true;
                }
                break;

            case Key.Escape:
                if (IsDropDownOpen)
                {
                    IsDropDownOpen = false;
                    e.Handled = true;
                }
                break;

            case Key.Down:
                if (IsDropDownOpen && _list != null)
                {
                    _list.Focus();
                    if (_list.SelectedIndex < 0 && _list.Items.Count > 0)
                        _list.SelectedIndex = 0;
                    (_list.ItemContainerGenerator.ContainerFromIndex(_list.SelectedIndex)
                        as ListBoxItem)?.Focus();
                    e.Handled = true;
                }
                break;
        }
    }

    // ── 键盘：建议列表 ────────────────────────────────────────────
    private void OnListPreviewKeyDown(object sender, KeyEventArgs e)
    {
        switch (e.Key)
        {
            case Key.Enter:
                if (_list?.SelectedItem != null)
                {
                    ConfirmSuggestion(_list.SelectedItem);
                    e.Handled = true;
                }
                break;

            case Key.Escape:
                IsDropDownOpen = false;
                Focus();
                e.Handled = true;
                break;

            case Key.Up when _list?.SelectedIndex <= 0:
                IsDropDownOpen = false;
                Focus();
                CaretIndex = Text.Length;
                e.Handled = true;
                break;
        }
    }

    private void OnListMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
    {
        if (_list?.SelectedItem != null)
            ConfirmSuggestion(_list.SelectedItem);
    }

    // ── 建议更新 ──────────────────────────────────────────────────
    private void UpdateSuggestions()
    {
        _filteredItems.Clear();

        if (SuggestionsSource == null)
        {
            IsDropDownOpen = false;
            return;
        }

        var text = Text?.Trim() ?? string.Empty;

        // 检查最小前缀长度
        if (text.Length < MinimumPrefixLength)
        {
            IsDropDownOpen = false;
            return;
        }

        var count = 0;

        foreach (var item in SuggestionsSource)
        {
            if (count >= MaxSuggestions) break;

            if (AutoFilter && !string.IsNullOrEmpty(text))
            {
                var display = GetDisplayString(item);
                if (!display.Contains(text, StringComparison.OrdinalIgnoreCase))
                    continue;
            }

            _filteredItems.Add(item);
            count++;
        }

        // 只有真正聚焦时才弹出
        IsDropDownOpen = _filteredItems.Count > 0 && IsKeyboardFocusWithin;
    }

    private void ConfirmSuggestion(object item)
    {
        SelectedSuggestion = item;
        var display = GetDisplayString(item);
        SetCurrentValue(TextProperty, display);
        CaretIndex = display.Length;
        IsDropDownOpen = false;
        Focus();

        var args = new SuggestionSelectedEventArgs(SuggestionSelectedEvent, this, item);
        RaiseEvent(args);
    }

    private string GetDisplayString(object item)
    {
        if (item == null) return string.Empty;
        if (!string.IsNullOrEmpty(SuggestionDisplayMemberPath))
        {
            try
            {
                var prop = item.GetType()
                    .GetProperty(SuggestionDisplayMemberPath,
                        BindingFlags.Public | BindingFlags.Instance);
                return prop?.GetValue(item)?.ToString() ?? item.ToString() ?? string.Empty;
            }
            catch { /* fallthrough */ }
        }
        return item.ToString() ?? string.Empty;
    }

    // ── 焦点 ──────────────────────────────────────────────────────
    protected override void OnGotFocus(RoutedEventArgs e)
    {
        base.OnGotFocus(e);
        UpdateStates(true);
    }

    protected override void OnLostFocus(RoutedEventArgs e)
    {
        base.OnLostFocus(e);
        UpdateStates(true);
    }

    protected override void OnIsKeyboardFocusWithinChanged(DependencyPropertyChangedEventArgs e)
    {
        base.OnIsKeyboardFocusWithinChanged(e);
        if (!(bool)e.NewValue)
        {
            Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Background, () =>
            {
                if (!IsKeyboardFocusWithin)
                    IsDropDownOpen = false;
            });
        }
        else
        {
            if (_filteredItems.Count > 0 && !string.IsNullOrEmpty(Text) && Text.Length >= MinimumPrefixLength)
                IsDropDownOpen = true;
        }
    }

    // ── States ────────────────────────────────────────────────────
    private void UpdateStates(bool animate)
    {
        if (!IsEnabled)
            VisualStateManager.GoToState(this, "Disabled", animate);
        else if (IsFocused)
            VisualStateManager.GoToState(this, "Focused", animate);
        else
            VisualStateManager.GoToState(this, "Normal", animate);

        UpdateLabelState(animate);
    }

    private void UpdateLabelState(bool animate)
    {
        var hasValue = !string.IsNullOrEmpty(Text);
        var floated = hasValue || IsFocused;
        AnimateHint(floated, animate);
        UpdateNotch(floated);
    }

    private static readonly Duration FastDur = new(TimeSpan.FromSeconds(0.15));
    private static readonly IEasingFunction EaseOut = new CubicEase { EasingMode = EasingMode.EaseOut };
    private static readonly IEasingFunction EaseIn = new CubicEase { EasingMode = EasingMode.EaseIn };

    private void AnimateHint(bool floated, bool animate)
    {
        if (_hintScale == null || _hintTranslate == null) return;

        var targetScale = floated ? 0.75 : 1.0;
        var targetY = floated
            ? (Variant == TextFieldVariant.Outlined ? -24.0 : -12.0)
            : 0.0;
        var easing = floated ? EaseOut : EaseIn;

        if (animate)
        {
            var scaleAnim = new DoubleAnimation(targetScale, FastDur) { EasingFunction = easing };
            var yAnim = new DoubleAnimation(targetY, FastDur) { EasingFunction = easing };
            _hintScale.BeginAnimation(ScaleTransform.ScaleXProperty, scaleAnim);
            _hintScale.BeginAnimation(ScaleTransform.ScaleYProperty, scaleAnim);
            _hintTranslate.BeginAnimation(TranslateTransform.YProperty, yAnim);
        }
        else
        {
            _hintScale.ScaleX = targetScale;
            _hintScale.ScaleY = targetScale;
            _hintTranslate.Y = targetY;
        }
    }

    private void UpdateNotch(bool floated)
    {
        if (_notchedBorder == null || _hintBlock == null) return;
        if (Variant != TextFieldVariant.Outlined) return;

        if (floated)
        {
            const double gap = 4.0;
            _notchedBorder.NotchStart = _hintBlock.Margin.Left - gap;
            _notchedBorder.NotchWidth = _hintBlock.ActualWidth * 0.75 + gap * 2;
        }
        else
        {
            _notchedBorder.NotchWidth = 0;
        }
    }
}
