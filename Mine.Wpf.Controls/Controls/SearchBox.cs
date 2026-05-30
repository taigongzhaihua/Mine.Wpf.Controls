using System.Collections;
using System.Collections.ObjectModel;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;

namespace Mine.Wpf.Controls.Controls;

/// <summary>
/// Material 3 搜索框控件——在基础文本框上扩展搜索专用功能：
/// 内置搜索图标、清空按钮、占位符、搜索命令（Enter 触发）、
/// 建议列表 Popup（支持自动过滤、键盘导航、点击选中）。
/// 支持 Bar（带阴影、胶囊型）和 Field（边框型）两种变体。
/// </summary>
[TemplatePart(Name = PartClearButton,      Type = typeof(FrameworkElement))]
[TemplatePart(Name = PartSearchButton,     Type = typeof(FrameworkElement))]
[TemplatePart(Name = PartSuggestionsPopup, Type = typeof(Popup))]
[TemplatePart(Name = PartSuggestionsList,  Type = typeof(ListBox))]
public class SearchBox : System.Windows.Controls.TextBox
{
    private const string PartClearButton      = "PART_ClearButton";
    private const string PartSearchButton     = "PART_SearchButton";
    private const string PartSuggestionsPopup = "PART_SuggestionsPopup";
    private const string PartSuggestionsList  = "PART_SuggestionsList";

    private Popup?  _popup;
    private ListBox? _list;
    private FrameworkElement? _clearButton;
    private FrameworkElement? _searchButton;

    // 内部过滤结果集，绑定到模板中的 ListBox
    private readonly ObservableCollection<object> _filteredItems = new();

    static SearchBox()
    {
        DefaultStyleKeyProperty.OverrideMetadata(
            typeof(SearchBox),
            new FrameworkPropertyMetadata(typeof(SearchBox)));
    }

    // ── 变体 ──────────────────────────────────────────────────────
    public static readonly DependencyProperty VariantProperty =
        DependencyProperty.Register(nameof(Variant), typeof(SearchBoxVariant), typeof(SearchBox),
            new PropertyMetadata(SearchBoxVariant.Bar));
    public SearchBoxVariant Variant
    {
        get => (SearchBoxVariant)GetValue(VariantProperty);
        set => SetValue(VariantProperty, value);
    }

    // ── 占位符文字 ────────────────────────────────────────────────
    public static readonly DependencyProperty PlaceholderProperty =
        DependencyProperty.Register(nameof(Placeholder), typeof(string), typeof(SearchBox),
            new PropertyMetadata("搜索"));
    public string Placeholder
    {
        get => (string)GetValue(PlaceholderProperty);
        set => SetValue(PlaceholderProperty, value);
    }

    // ── 是否允许清空 ──────────────────────────────────────────────
    public static readonly DependencyProperty IsClearableProperty =
        DependencyProperty.Register(nameof(IsClearable), typeof(bool), typeof(SearchBox),
            new PropertyMetadata(true));
    public bool IsClearable
    {
        get => (bool)GetValue(IsClearableProperty);
        set => SetValue(IsClearableProperty, value);
    }

    // ── 是否显示搜索按钮（右侧） ──────────────────────────────────
    public static readonly DependencyProperty ShowSearchButtonProperty =
        DependencyProperty.Register(nameof(ShowSearchButton), typeof(bool), typeof(SearchBox),
            new PropertyMetadata(false));
    public bool ShowSearchButton
    {
        get => (bool)GetValue(ShowSearchButtonProperty);
        set => SetValue(ShowSearchButtonProperty, value);
    }

    // ── 前置内容（默认为搜索图标，可替换为头像等） ──────────────
    public static readonly DependencyProperty LeadingContentProperty =
        DependencyProperty.Register(nameof(LeadingContent), typeof(object), typeof(SearchBox),
            new PropertyMetadata(null));
    public object? LeadingContent
    {
        get => GetValue(LeadingContentProperty);
        set => SetValue(LeadingContentProperty, value);
    }

    // ── 搜索命令 ──────────────────────────────────────────────────
    public static readonly DependencyProperty SearchCommandProperty =
        DependencyProperty.Register(nameof(SearchCommand), typeof(ICommand), typeof(SearchBox),
            new PropertyMetadata(null));
    public ICommand? SearchCommand
    {
        get => (ICommand?)GetValue(SearchCommandProperty);
        set => SetValue(SearchCommandProperty, value);
    }

    public static readonly DependencyProperty SearchCommandParameterProperty =
        DependencyProperty.Register(nameof(SearchCommandParameter), typeof(object), typeof(SearchBox),
            new PropertyMetadata(null));
    public object? SearchCommandParameter
    {
        get => GetValue(SearchCommandParameterProperty);
        set => SetValue(SearchCommandParameterProperty, value);
    }

    // ── 建议列表数据源 ────────────────────────────────────────────
    public static readonly DependencyProperty SuggestionsSourceProperty =
        DependencyProperty.Register(nameof(SuggestionsSource), typeof(IEnumerable), typeof(SearchBox),
            new PropertyMetadata(null, OnSuggestionsSourceChanged));
    public IEnumerable? SuggestionsSource
    {
        get => (IEnumerable?)GetValue(SuggestionsSourceProperty);
        set => SetValue(SuggestionsSourceProperty, value);
    }
    private static void OnSuggestionsSourceChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        => ((SearchBox)d).UpdateSuggestions();

    // ── 自动过滤（true=根据输入文字自动过滤；false=直接展示 SuggestionsSource） ──
    public static readonly DependencyProperty AutoFilterProperty =
        DependencyProperty.Register(nameof(AutoFilter), typeof(bool), typeof(SearchBox),
            new PropertyMetadata(true));
    public bool AutoFilter
    {
        get => (bool)GetValue(AutoFilterProperty);
        set => SetValue(AutoFilterProperty, value);
    }

    // ── 用于显示/过滤的成员路径（留空则用 ToString()） ───────────
    public static readonly DependencyProperty SuggestionDisplayMemberPathProperty =
        DependencyProperty.Register(nameof(SuggestionDisplayMemberPath), typeof(string), typeof(SearchBox),
            new PropertyMetadata(null));
    public string? SuggestionDisplayMemberPath
    {
        get => (string?)GetValue(SuggestionDisplayMemberPathProperty);
        set => SetValue(SuggestionDisplayMemberPathProperty, value);
    }

    // ── 自定义建议项模板 ──────────────────────────────────────────
    public static readonly DependencyProperty SuggestionItemTemplateProperty =
        DependencyProperty.Register(nameof(SuggestionItemTemplate), typeof(DataTemplate), typeof(SearchBox),
            new PropertyMetadata(null));
    public DataTemplate? SuggestionItemTemplate
    {
        get => (DataTemplate?)GetValue(SuggestionItemTemplateProperty);
        set => SetValue(SuggestionItemTemplateProperty, value);
    }

    // ── 最大显示建议条数 ──────────────────────────────────────────
    public static readonly DependencyProperty MaxSuggestionsProperty =
        DependencyProperty.Register(nameof(MaxSuggestions), typeof(int), typeof(SearchBox),
            new PropertyMetadata(8));
    public int MaxSuggestions
    {
        get => (int)GetValue(MaxSuggestionsProperty);
        set => SetValue(MaxSuggestionsProperty, value);
    }

    // ── 下拉是否打开 ──────────────────────────────────────────────
    public static readonly DependencyProperty IsDropDownOpenProperty =
        DependencyProperty.Register(nameof(IsDropDownOpen), typeof(bool), typeof(SearchBox),
            new PropertyMetadata(false, OnIsDropDownOpenChanged));
    public bool IsDropDownOpen
    {
        get => (bool)GetValue(IsDropDownOpenProperty);
        set => SetValue(IsDropDownOpenProperty, value);
    }
    private static void OnIsDropDownOpenChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        var box = (SearchBox)d;
        if (!(bool)e.NewValue && box._list != null)
            box._list.SelectedIndex = -1; // 关闭时清除列表选中
    }

    // ── 当前选中的建议项 ──────────────────────────────────────────
    public static readonly DependencyProperty SelectedSuggestionProperty =
        DependencyProperty.Register(nameof(SelectedSuggestion), typeof(object), typeof(SearchBox),
            new PropertyMetadata(null));
    public object? SelectedSuggestion
    {
        get => GetValue(SelectedSuggestionProperty);
        set => SetValue(SelectedSuggestionProperty, value);
    }

    // ── 过滤后的结果集（只读 DP，供模板 ListBox 绑定） ───────────
    private static readonly DependencyPropertyKey FilteredSuggestionsPropertyKey =
        DependencyProperty.RegisterReadOnly(nameof(FilteredSuggestions), typeof(IEnumerable), typeof(SearchBox),
            new PropertyMetadata(null));
    public static readonly DependencyProperty FilteredSuggestionsProperty =
        FilteredSuggestionsPropertyKey.DependencyProperty;
    public IEnumerable FilteredSuggestions => (IEnumerable)GetValue(FilteredSuggestionsProperty);

    // ── HasText（只读） ───────────────────────────────────────────
    private static readonly DependencyPropertyKey HasTextPropertyKey =
        DependencyProperty.RegisterReadOnly(nameof(HasText), typeof(bool), typeof(SearchBox),
            new PropertyMetadata(false));
    public static readonly DependencyProperty HasTextProperty = HasTextPropertyKey.DependencyProperty;
    public bool HasText => (bool)GetValue(HasTextProperty);

    // ── 搜索事件 ──────────────────────────────────────────────────
    public static readonly RoutedEvent SearchEvent =
        EventManager.RegisterRoutedEvent(nameof(Search), RoutingStrategy.Bubble,
            typeof(RoutedEventHandler), typeof(SearchBox));
    public event RoutedEventHandler Search
    {
        add    => AddHandler(SearchEvent, value);
        remove => RemoveHandler(SearchEvent, value);
    }

    // ── 建议被选中事件 ────────────────────────────────────────────
    public static readonly RoutedEvent SuggestionSelectedEvent =
        EventManager.RegisterRoutedEvent(nameof(SuggestionSelected), RoutingStrategy.Bubble,
            typeof(SuggestionSelectedEventHandler), typeof(SearchBox));
    public event SuggestionSelectedEventHandler SuggestionSelected
    {
        add    => AddHandler(SuggestionSelectedEvent, value);
        remove => RemoveHandler(SuggestionSelectedEvent, value);
    }

    // ── 模板 ──────────────────────────────────────────────────────
    public override void OnApplyTemplate()
    {
        base.OnApplyTemplate();

        // 解绑旧处理器
        if (_clearButton  != null) _clearButton.MouseLeftButtonDown  -= OnClearMouseDown;
        if (_searchButton != null) _searchButton.MouseLeftButtonDown -= OnSearchMouseDown;
        if (_list != null)
        {
            _list.PreviewKeyDown    -= OnListPreviewKeyDown;
            _list.MouseLeftButtonUp -= OnListMouseLeftButtonUp;
        }

        // 清空/搜索按钮现在是 Border（FrameworkElement），而非 Button，
        // 避免 ButtonBase 与 TextBox 抢鼠标捕获
        _clearButton  = GetTemplateChild(PartClearButton)  as FrameworkElement;
        _searchButton = GetTemplateChild(PartSearchButton) as FrameworkElement;
        _popup        = GetTemplateChild(PartSuggestionsPopup) as Popup;
        _list         = GetTemplateChild(PartSuggestionsList)  as ListBox;

        if (_clearButton  != null) _clearButton.MouseLeftButtonDown  += OnClearMouseDown;
        if (_searchButton != null) _searchButton.MouseLeftButtonDown += OnSearchMouseDown;

        if (_list != null)
        {
            _list.PreviewKeyDown    += OnListPreviewKeyDown;
            _list.MouseLeftButtonUp += OnListMouseLeftButtonUp;
        }

        SetValue(FilteredSuggestionsPropertyKey, _filteredItems);
        UpdateSuggestions();
    }

    // Border.MouseLeftButtonDown 在气泡阶段触发，e.Handled=true 后
    // TextBox 的类处理器（handledEventsToo=false）直接跳过，不再 CaptureMouse
    private void OnClearMouseDown(object sender, MouseButtonEventArgs e)
    {
        e.Handled = true;
        Clear();
        Focus();
    }

    private void OnSearchMouseDown(object sender, MouseButtonEventArgs e)
    {
        e.Handled = true;
        ExecuteSearch();
    }


    protected override void OnTextChanged(TextChangedEventArgs e)
    {
        base.OnTextChanged(e);
        SetValue(HasTextPropertyKey, !string.IsNullOrEmpty(Text));
        UpdateSuggestions();
    }

    // ── 键盘：SearchBox 自身 ──────────────────────────────────────
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
                else
                {
                    ExecuteSearch();
                    e.Handled = true;
                }
                break;

            case Key.Escape:
                if (IsDropDownOpen) { IsDropDownOpen = false; e.Handled = true; }
                else if (HasText)   { Clear();                e.Handled = true; }
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

    protected override void OnKeyDown(KeyEventArgs e)
    {
        base.OnKeyDown(e);
        // 阻止 Enter 在 TextBox 内产生换行
        if (e.Key == Key.Enter) e.Handled = true;
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
                // 恢复光标到末尾
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
        // 将选中项的显示文字填回输入框
        var display = GetDisplayString(item);
        SetCurrentValue(System.Windows.Controls.TextBox.TextProperty, display);
        CaretIndex = display.Length;
        IsDropDownOpen = false;
        Focus();

        var args = new SuggestionSelectedEventArgs(SuggestionSelectedEvent, this, item);
        RaiseEvent(args);
    }

    private void ExecuteSearch()
    {
        IsDropDownOpen = false;
        var param = SearchCommandParameter ?? Text;
        if (SearchCommand?.CanExecute(param) == true)
            SearchCommand.Execute(param);
        RaiseEvent(new RoutedEventArgs(SearchEvent, this));
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

    // ── 失去焦点时关闭 Popup ──────────────────────────────────────
    protected override void OnIsKeyboardFocusWithinChanged(DependencyPropertyChangedEventArgs e)
    {
        base.OnIsKeyboardFocusWithinChanged(e);
        if (!(bool)e.NewValue)
        {
            // 延迟判断，防止焦点转移到 Popup 内的 ListBox 时误关闭
            Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Background, () =>
            {
                if (!IsKeyboardFocusWithin)
                    IsDropDownOpen = false;
            });
        }
        else
        {
            // 重新聚焦时如果有过滤结果，重新弹出
            if (_filteredItems.Count > 0)
                IsDropDownOpen = true;
        }
    }
}

/// <summary>搜索框变体枚举。</summary>
public enum SearchBoxVariant
{
    /// <summary>搜索栏（胶囊型，带阴影，Material 3 Search Bar）</summary>
    Bar,
    /// <summary>搜索字段（紧凑边框型，嵌入式场景）</summary>
    Field
}

/// <summary>建议选中事件参数。</summary>
public class SuggestionSelectedEventArgs(RoutedEvent e, object source, object selectedItem) : RoutedEventArgs(e, source)
{
    public object SelectedItem { get; } = selectedItem;
}
public delegate void SuggestionSelectedEventHandler(object sender, SuggestionSelectedEventArgs e);
