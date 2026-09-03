using System.Windows;
using System.Windows.Automation.Peers;
using System.Windows.Automation.Provider;
using System.Windows.Controls;
using System.Windows.Input;

namespace Mine.Wpf.Controls.Controls;

/// <summary>
/// Material Design 3 分页控件
/// 支持页码跳转、上一页/下一页、首页/末页导航
/// </summary>
[TemplatePart(Name = PartPreviousButton, Type = typeof(Button))]
[TemplatePart(Name = PartNextButton, Type = typeof(Button))]
[TemplatePart(Name = PartFirstButton, Type = typeof(Button))]
[TemplatePart(Name = PartLastButton, Type = typeof(Button))]
[TemplatePart(Name = PartPageButtonsPanel, Type = typeof(Panel))]
public class PaginationControl : Control
{
    private const string PartPreviousButton = "PART_PreviousButton";
    private const string PartNextButton = "PART_NextButton";
    private const string PartFirstButton = "PART_FirstButton";
    private const string PartLastButton = "PART_LastButton";
    private const string PartPageButtonsPanel = "PART_PageButtonsPanel";

    private Button? _previousButton;
    private Button? _nextButton;
    private Button? _firstButton;
    private Button? _lastButton;
    private Panel? _pageButtonsPanel;

    static PaginationControl()
    {
        DefaultStyleKeyProperty.OverrideMetadata(
            typeof(PaginationControl),
            new FrameworkPropertyMetadata(typeof(PaginationControl)));
    }

    protected override AutomationPeer OnCreateAutomationPeer() => new PaginationControlAutomationPeer(this);

    // ── 当前页码 ─────────────────────────────────────────────────────
    public static readonly DependencyProperty CurrentPageProperty =
        DependencyProperty.Register(nameof(CurrentPage), typeof(int), typeof(PaginationControl),
            new FrameworkPropertyMetadata(1, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault,
                OnCurrentPageChanged, CoerceCurrentPage));

    public int CurrentPage
    {
        get => (int)GetValue(CurrentPageProperty);
        set => SetValue(CurrentPageProperty, value);
    }

    private static void OnCurrentPageChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is PaginationControl pagination)
        {
            pagination.RaisePageChangedEvent();
            pagination.UpdatePageButtons();
            pagination.UpdateNavigationButtons();
        }
    }

    private static object CoerceCurrentPage(DependencyObject d, object baseValue)
    {
        if (d is PaginationControl pagination)
        {
            var page = (int)baseValue;
            return Math.Max(1, Math.Min(page, pagination.TotalPages));
        }
        return baseValue;
    }

    // ── 总页数 ───────────────────────────────────────────────────────
    public static readonly DependencyProperty TotalPagesProperty =
        DependencyProperty.Register(nameof(TotalPages), typeof(int), typeof(PaginationControl),
            new PropertyMetadata(1, OnTotalPagesChanged, CoerceTotalPages));

    public int TotalPages
    {
        get => (int)GetValue(TotalPagesProperty);
        set => SetValue(TotalPagesProperty, value);
    }

    private static void OnTotalPagesChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is PaginationControl pagination)
        {
            pagination.CoerceValue(CurrentPageProperty);
            pagination.UpdatePageButtons();
            pagination.UpdateNavigationButtons();
        }
    }

    private static object CoerceTotalPages(DependencyObject d, object baseValue)
    {
        return Math.Max(1, (int)baseValue);
    }

    // ── 显示的页码按钮数量 ───────────────────────────────────────────
    public static readonly DependencyProperty MaxPageButtonsProperty =
        DependencyProperty.Register(nameof(MaxPageButtons), typeof(int), typeof(PaginationControl),
            new PropertyMetadata(7, OnMaxPageButtonsChanged));

    public int MaxPageButtons
    {
        get => (int)GetValue(MaxPageButtonsProperty);
        set => SetValue(MaxPageButtonsProperty, value);
    }

    private static void OnMaxPageButtonsChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is PaginationControl pagination)
        {
            pagination.UpdatePageButtons();
        }
    }

    // ── 是否显示首页/末页按钮 ────────────────────────────────────────
    public static readonly DependencyProperty ShowFirstLastButtonsProperty =
        DependencyProperty.Register(nameof(ShowFirstLastButtons), typeof(bool), typeof(PaginationControl),
            new PropertyMetadata(true));

    public bool ShowFirstLastButtons
    {
        get => (bool)GetValue(ShowFirstLastButtonsProperty);
        set => SetValue(ShowFirstLastButtonsProperty, value);
    }

    // ── 圆角 ─────────────────────────────────────────────────────────
    public static readonly DependencyProperty CornerRadiusProperty =
        DependencyProperty.Register(nameof(CornerRadius), typeof(CornerRadius), typeof(PaginationControl),
            new PropertyMetadata(new CornerRadius(20)));

    public CornerRadius CornerRadius
    {
        get => (CornerRadius)GetValue(CornerRadiusProperty);
        set => SetValue(CornerRadiusProperty, value);
    }

    // ── 页码改变事件 ─────────────────────────────────────────────────
    public static readonly RoutedEvent PageChangedEvent =
        EventManager.RegisterRoutedEvent(
            nameof(PageChanged),
            RoutingStrategy.Bubble,
            typeof(RoutedPropertyChangedEventHandler<int>),
            typeof(PaginationControl));

    public event RoutedPropertyChangedEventHandler<int> PageChanged
    {
        add => AddHandler(PageChangedEvent, value);
        remove => RemoveHandler(PageChangedEvent, value);
    }

    private void RaisePageChangedEvent()
    {
        var args = new RoutedPropertyChangedEventArgs<int>(CurrentPage, CurrentPage)
        {
            RoutedEvent = PageChangedEvent
        };
        RaiseEvent(args);
    }

    // ── 命令 ─────────────────────────────────────────────────────────
    public static readonly DependencyProperty GoToPageCommandProperty =
        DependencyProperty.Register(nameof(GoToPageCommand), typeof(ICommand), typeof(PaginationControl));

    public ICommand GoToPageCommand
    {
        get => (ICommand)GetValue(GoToPageCommandProperty);
        private set => SetValue(GoToPageCommandProperty, value);
    }

    public PaginationControl()
    {
        GoToPageCommand = new RelayCommand<int>(GoToPage);
    }

    // ── 模板应用 ─────────────────────────────────────────────────────
    public override void OnApplyTemplate()
    {
        base.OnApplyTemplate();

        // 清理旧的事件处理
        if (_previousButton != null)
            _previousButton.Click -= OnPreviousButtonClick;
        if (_nextButton != null)
            _nextButton.Click -= OnNextButtonClick;
        if (_firstButton != null)
            _firstButton.Click -= OnFirstButtonClick;
        if (_lastButton != null)
            _lastButton.Click -= OnLastButtonClick;

        // 获取模板部件
        _previousButton = GetTemplateChild(PartPreviousButton) as Button;
        _nextButton = GetTemplateChild(PartNextButton) as Button;
        _firstButton = GetTemplateChild(PartFirstButton) as Button;
        _lastButton = GetTemplateChild(PartLastButton) as Button;
        _pageButtonsPanel = GetTemplateChild(PartPageButtonsPanel) as Panel;

        System.Diagnostics.Debug.WriteLine($"[PaginationControl] OnApplyTemplate:");
        System.Diagnostics.Debug.WriteLine($"  PartPreviousButton = {GetTemplateChild(PartPreviousButton)?.GetType().Name}");
        System.Diagnostics.Debug.WriteLine($"  PartNextButton = {GetTemplateChild(PartNextButton)?.GetType().Name}");
        System.Diagnostics.Debug.WriteLine($"  PartFirstButton = {GetTemplateChild(PartFirstButton)?.GetType().Name}");
        System.Diagnostics.Debug.WriteLine($"  PartLastButton = {GetTemplateChild(PartLastButton)?.GetType().Name}");
        System.Diagnostics.Debug.WriteLine($"  PartPageButtonsPanel = {GetTemplateChild(PartPageButtonsPanel)?.GetType().Name}");
        System.Diagnostics.Debug.WriteLine($"  _previousButton={_previousButton != null}, _nextButton={_nextButton != null}, _pageButtonsPanel={_pageButtonsPanel != null}");

        // 绑定事件
        if (_previousButton != null)
            _previousButton.Click += OnPreviousButtonClick;
        if (_nextButton != null)
            _nextButton.Click += OnNextButtonClick;
        if (_firstButton != null)
            _firstButton.Click += OnFirstButtonClick;
        if (_lastButton != null)
            _lastButton.Click += OnLastButtonClick;

        UpdatePageButtons();
        UpdateNavigationButtons();
    }

    // ── 导航方法 ─────────────────────────────────────────────────────
    private void OnPreviousButtonClick(object sender, RoutedEventArgs e)
    {
        System.Diagnostics.Debug.WriteLine($"[PaginationControl] Previous clicked, CurrentPage={CurrentPage}");
        if (CurrentPage > 1)
            CurrentPage--;
    }

    private void OnNextButtonClick(object sender, RoutedEventArgs e)
    {
        System.Diagnostics.Debug.WriteLine($"[PaginationControl] Next clicked, CurrentPage={CurrentPage}");
        if (CurrentPage < TotalPages)
            CurrentPage++;
    }

    private void OnFirstButtonClick(object sender, RoutedEventArgs e)
    {
        System.Diagnostics.Debug.WriteLine($"[PaginationControl] First clicked");
        CurrentPage = 1;
    }

    private void OnLastButtonClick(object sender, RoutedEventArgs e)
    {
        System.Diagnostics.Debug.WriteLine($"[PaginationControl] Last clicked");
        CurrentPage = TotalPages;
    }

    private void GoToPage(int page)
    {
        System.Diagnostics.Debug.WriteLine($"[PaginationControl] GoToPage({page})");
        if (page >= 1 && page <= TotalPages)
            CurrentPage = page;
    }

    // ── 更新页码按钮 ─────────────────────────────────────────────────
    private void UpdatePageButtons()
    {
        System.Diagnostics.Debug.WriteLine($"[PaginationControl] UpdatePageButtons: CurrentPage={CurrentPage}, TotalPages={TotalPages}");

        if (_pageButtonsPanel == null)
        {
            System.Diagnostics.Debug.WriteLine("[PaginationControl] _pageButtonsPanel is null");
            return;
        }

        _pageButtonsPanel.Children.Clear();

        if (TotalPages <= 1)
        {
            System.Diagnostics.Debug.WriteLine("[PaginationControl] TotalPages <= 1, skipping");
            return;
        }

        var pages = CalculatePageNumbers();
        System.Diagnostics.Debug.WriteLine($"[PaginationControl] Page numbers: {string.Join(", ", pages)}");

        for (var i = 0; i < pages.Length; i++)
        {
            var page = pages[i];

            if (page == -1)
            {
                // 显示省略号
                var ellipsis = new TextBlock
                {
                    Text = "...",
                    VerticalAlignment = VerticalAlignment.Center,
                    HorizontalAlignment = HorizontalAlignment.Center,
                    Margin = new Thickness(4, 0, 4, 0),
                    FontSize = 14
                };

                if (TryFindResource("Mine.Brush.OnSurface") is System.Windows.Media.Brush brush)
                    ellipsis.Foreground = brush;

                _pageButtonsPanel.Children.Add(ellipsis);
            }
            else
            {
                var button = new Button
                {
                    Content = page.ToString(),
                    Width = 40,
                    Height = 40,
                    Padding = new Thickness(0),
                    Margin = new Thickness(2, 0, 2, 0),
                    CommandParameter = page,
                    Command = GoToPageCommand,
                    Variant = page == CurrentPage ? ButtonVariant.Filled : ButtonVariant.Text
                };

                System.Diagnostics.Debug.WriteLine($"[PaginationControl] Button {page}: Variant={button.Variant}, Content={button.Content}");

                _pageButtonsPanel.Children.Add(button);
            }
        }
    }

    // ── 计算要显示的页码 ─────────────────────────────────────────────
    private int[] CalculatePageNumbers()
    {
        if (TotalPages <= MaxPageButtons)
        {
            // 页数不多,全部显示
            return Enumerable.Range(1, TotalPages).ToArray();
        }

        var pages = new List<int>();
        var half = MaxPageButtons / 2;

        // 始终显示第一页
        pages.Add(1);

        int start, end;

        if (CurrentPage <= half + 1)
        {
            // 靠近开始
            start = 2;
            end = MaxPageButtons - 1;
        }
        else if (CurrentPage >= TotalPages - half)
        {
            // 靠近末尾
            start = TotalPages - MaxPageButtons + 2;
            end = TotalPages - 1;
        }
        else
        {
            // 在中间
            start = CurrentPage - half + 1;
            end = CurrentPage + half - 1;
        }

        // 添加省略号
        if (start > 2)
            pages.Add(-1);

        // 添加中间页码
        for (var i = start; i <= end; i++)
        {
            if (i > 1 && i < TotalPages)
                pages.Add(i);
        }

        // 添加省略号
        if (end < TotalPages - 1)
            pages.Add(-1);

        // 始终显示最后一页
        if (TotalPages > 1)
            pages.Add(TotalPages);

        return pages.ToArray();
    }

    // ── 更新导航按钮状态 ─────────────────────────────────────────────
    private void UpdateNavigationButtons()
    {
        if (_previousButton != null)
            _previousButton.IsEnabled = CurrentPage > 1;

        if (_nextButton != null)
            _nextButton.IsEnabled = CurrentPage < TotalPages;

        if (_firstButton != null)
            _firstButton.IsEnabled = CurrentPage > 1;

        if (_lastButton != null)
            _lastButton.IsEnabled = CurrentPage < TotalPages;
    }
}

/// <summary>暴露 <see cref="PaginationControl"/> 的当前页码给屏幕阅读器 / UI 自动化。</summary>
public class PaginationControlAutomationPeer : FrameworkElementAutomationPeer, IRangeValueProvider
{
    public PaginationControlAutomationPeer(PaginationControl owner) : base(owner) { }

    private PaginationControl Control => (PaginationControl)Owner;

    public override object GetPattern(PatternInterface patternInterface)
        => patternInterface == PatternInterface.RangeValue ? this : base.GetPattern(patternInterface);

    protected override string GetClassNameCore() => nameof(PaginationControl);
    protected override AutomationControlType GetAutomationControlTypeCore() => AutomationControlType.Pane;

    public bool IsReadOnly => !Control.IsEnabled;
    public double Maximum => Control.TotalPages;
    public double Minimum => 1;
    public double LargeChange => 1;
    public double SmallChange => 1;
    public double Value => Control.CurrentPage;

    public void SetValue(double value) => Control.CurrentPage = (int)Math.Clamp(value, 1, Control.TotalPages);
}

/// <summary>简单的命令实现</summary>
internal class RelayCommand<T> : ICommand
{
    private readonly Action<T> _execute;
    private readonly Func<T, bool>? _canExecute;

    public RelayCommand(Action<T> execute, Func<T, bool>? canExecute = null)
    {
        _execute = execute ?? throw new ArgumentNullException(nameof(execute));
        _canExecute = canExecute;
    }

    public event EventHandler? CanExecuteChanged
    {
        add => CommandManager.RequerySuggested += value;
        remove => CommandManager.RequerySuggested -= value;
    }

    public bool CanExecute(object? parameter)
    {
        if (parameter is T typedParameter)
            return _canExecute == null || _canExecute(typedParameter);
        return false;
    }

    public void Execute(object? parameter)
    {
        if (parameter is T typedParameter)
            _execute(typedParameter);
    }
}
