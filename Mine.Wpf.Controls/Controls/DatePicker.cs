using System.ComponentModel;
using System.Globalization;
using System.Windows;
using System.Windows.Automation;
using System.Windows.Automation.Peers;
using System.Windows.Automation.Provider;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Threading;
using Mine.Wpf.Controls.Primitives;
using WpfButton = System.Windows.Controls.Button;
using WpfTextBox = System.Windows.Controls.TextBox;

namespace Mine.Wpf.Controls.Controls;

/// <summary>日期选择器中的日历单元格。</summary>
public sealed class CalendarDayItem
{
    public DateTime Date { get; init; }
    public string Label { get; init; } = string.Empty;
    public bool IsToday { get; init; }
    public bool IsSelected { get; init; }
    public bool IsConfirmed { get; init; }
    public bool IsOutsideMonth { get; init; }
    public bool IsEnabled { get; init; }
    public bool IsInRange { get; init; }
    public bool IsRangeStart { get; init; }
    public bool IsRangeEnd { get; init; }
}

/// <summary>
/// Material Design 3 日期选择器。使用 Flyout 弹出日历，支持单日期/日期范围、light-dismiss、确认/取消和手动输入。
/// </summary>
[TemplatePart(Name = PartInputBorder, Type = typeof(FrameworkElement))]
[TemplatePart(Name = PartContainer, Type = typeof(NotchedOutlineBorder))]
[TemplatePart(Name = PartHint, Type = typeof(TextBlock))]
[TemplatePart(Name = PartFlyout, Type = typeof(Flyout))]
[TemplatePart(Name = PartPopupInput, Type = typeof(WpfTextBox))]
[TemplatePart(Name = PartDayItems, Type = typeof(ItemsControl))]
[TemplatePart(Name = PartPrevMonth, Type = typeof(WpfButton))]
[TemplatePart(Name = PartNextMonth, Type = typeof(WpfButton))]
[TemplatePart(Name = PartTodayButton, Type = typeof(WpfButton))]
[TemplatePart(Name = PartClearButton, Type = typeof(WpfButton))]
[TemplatePart(Name = PartCancelButton, Type = typeof(WpfButton))]
[TemplatePart(Name = PartConfirmButton, Type = typeof(WpfButton))]
[TemplateVisualState(Name = "LabelNormal", GroupName = "LabelStates")]
[TemplateVisualState(Name = "LabelFloated", GroupName = "LabelStates")]
public class DatePicker : Control
{
    private const string PartInputBorder = "PART_InputBorder";
    private const string PartContainer = "Container";
    private const string PartHint = "PART_Hint";
    private const string PartFlyout = "PART_Flyout";
    private const string PartPopupInput = "PART_PopupInput";
    private const string PartDayItems = "PART_DayItems";
    private const string PartPrevMonth = "PART_PrevMonth";
    private const string PartNextMonth = "PART_NextMonth";
    private const string PartTodayButton = "PART_TodayButton";
    private const string PartClearButton = "PART_ClearButton";
    private const string PartCancelButton = "PART_CancelButton";
    private const string PartConfirmButton = "PART_ConfirmButton";

    private static readonly DependencyPropertyDescriptor? FlyoutIsOpenDescriptor =
        DependencyPropertyDescriptor.FromProperty(Flyout.IsOpenProperty, typeof(Flyout));

    private Flyout? _flyout;
    private FrameworkElement? _inputBorder;
    private NotchedOutlineBorder? _notchedBorder;
    private TextBlock? _hintBlock;
    private WpfTextBox? _popupInput;
    private ItemsControl? _dayItems;
    private DispatcherTimer? _deferredOpenTimer;
    private DateTime _viewDate = new(DateTime.Today.Year, DateTime.Today.Month, 1);
    private DateTime? _pendingDate;
    private DateTime? _pendingStartDate;
    private DateTime? _pendingEndDate;
    private bool? _lastOutlinedHintFloated;
    private double _lastNotchStart = double.NaN;
    private double _lastNotchWidth = double.NaN;

    static DatePicker()
    {
        DefaultStyleKeyProperty.OverrideMetadata(typeof(DatePicker), new FrameworkPropertyMetadata(typeof(DatePicker)));
        FocusableProperty.OverrideMetadata(typeof(DatePicker), new FrameworkPropertyMetadata(true));
    }

    protected override AutomationPeer OnCreateAutomationPeer() => new DatePickerAutomationPeer(this);

    // ── Variant ────────────────────────────────────────────────────
    public static readonly DependencyProperty VariantProperty =
        DependencyProperty.Register(nameof(Variant), typeof(TextFieldVariant), typeof(DatePicker),
            new PropertyMetadata(TextFieldVariant.Outlined));

    public TextFieldVariant Variant
    {
        get => (TextFieldVariant)GetValue(VariantProperty);
        set => SetValue(VariantProperty, value);
    }

    // ── SelectionMode ──────────────────────────────────────────────
    public static readonly DependencyProperty SelectionModeProperty =
        DependencyProperty.Register(nameof(SelectionMode), typeof(DatePickerSelectionMode), typeof(DatePicker),
            new PropertyMetadata(DatePickerSelectionMode.Single, OnSelectionModeChanged));

    public DatePickerSelectionMode SelectionMode
    {
        get => (DatePickerSelectionMode)GetValue(SelectionModeProperty);
        set => SetValue(SelectionModeProperty, value);
    }

    private static void OnSelectionModeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        var picker = (DatePicker)d;
        picker.UpdateDisplayText();
        picker.BuildCalendar();
        picker.SyncPopupInput();
    }

    // ── SelectedDate（单日期模式）───────────────────────────────────
    public static readonly DependencyProperty SelectedDateProperty =
        DependencyProperty.Register(nameof(SelectedDate), typeof(DateTime?), typeof(DatePicker),
            new FrameworkPropertyMetadata(null,
                FrameworkPropertyMetadataOptions.BindsTwoWayByDefault,
                OnSelectedDateChanged));

    public DateTime? SelectedDate
    {
        get => (DateTime?)GetValue(SelectedDateProperty);
        set => SetValue(SelectedDateProperty, value);
    }

    private static void OnSelectedDateChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        var picker = (DatePicker)d;
        if (e.NewValue is DateTime date)
            picker._viewDate = FirstOfMonth(date);

        picker.UpdateDisplayText();
        picker.BuildCalendar();
        picker.RaiseEvent(new RoutedPropertyChangedEventArgs<DateTime?>((DateTime?)e.OldValue, (DateTime?)e.NewValue, SelectedDateChangedEvent));
    }

    // ── StartDate / EndDate（范围模式）───────────────────────────────
    public static readonly DependencyProperty StartDateProperty =
        DependencyProperty.Register(nameof(StartDate), typeof(DateTime?), typeof(DatePicker),
            new FrameworkPropertyMetadata(null,
                FrameworkPropertyMetadataOptions.BindsTwoWayByDefault,
                OnRangeDateChanged));

    public DateTime? StartDate
    {
        get => (DateTime?)GetValue(StartDateProperty);
        set => SetValue(StartDateProperty, value);
    }

    public static readonly DependencyProperty EndDateProperty =
        DependencyProperty.Register(nameof(EndDate), typeof(DateTime?), typeof(DatePicker),
            new FrameworkPropertyMetadata(null,
                FrameworkPropertyMetadataOptions.BindsTwoWayByDefault,
                OnRangeDateChanged));

    public DateTime? EndDate
    {
        get => (DateTime?)GetValue(EndDateProperty);
        set => SetValue(EndDateProperty, value);
    }

    private static void OnRangeDateChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        var picker = (DatePicker)d;
        if (picker.StartDate is { } start)
            picker._viewDate = FirstOfMonth(start);
        else if (picker.EndDate is { } end)
            picker._viewDate = FirstOfMonth(end);

        picker.CoerceCommittedRange();
        picker.UpdateDisplayText();
        picker.BuildCalendar();
        picker.RaiseEvent(new RoutedEventArgs(DateRangeChangedEvent, picker));
    }

    private void CoerceCommittedRange()
    {
        if (StartDate.HasValue && EndDate.HasValue && EndDate.Value.Date < StartDate.Value.Date)
        {
            var start = StartDate.Value.Date;
            var end = EndDate.Value.Date;
            SetCurrentValue(StartDateProperty, end);
            SetCurrentValue(EndDateProperty, start);
        }
    }

    // ── DisplayFormat ──────────────────────────────────────────────
    public static readonly DependencyProperty DisplayFormatProperty =
        DependencyProperty.Register(nameof(DisplayFormat), typeof(string), typeof(DatePicker),
            new PropertyMetadata("yyyy-MM-dd", (d, _) => ((DatePicker)d).UpdateDisplayText()));

    public string DisplayFormat
    {
        get => (string)GetValue(DisplayFormatProperty);
        set => SetValue(DisplayFormatProperty, value);
    }

    // ── Hint / HelperText ──────────────────────────────────────────
    public static readonly DependencyProperty HintProperty =
        DependencyProperty.Register(nameof(Hint), typeof(string), typeof(DatePicker),
            new PropertyMetadata("选择日期"));

    public string? Hint
    {
        get => (string?)GetValue(HintProperty);
        set => SetValue(HintProperty, value);
    }

    public static readonly DependencyProperty HelperTextProperty =
        DependencyProperty.Register(nameof(HelperText), typeof(string), typeof(DatePicker),
            new PropertyMetadata(string.Empty));

    public string? HelperText
    {
        get => (string?)GetValue(HelperTextProperty);
        set => SetValue(HelperTextProperty, value);
    }

    // ── Date range limit ───────────────────────────────────────────
    public static readonly DependencyProperty MinimumDateProperty =
        DependencyProperty.Register(nameof(MinimumDate), typeof(DateTime?), typeof(DatePicker),
            new PropertyMetadata(null, OnDateRangeChanged));

    public DateTime? MinimumDate
    {
        get => (DateTime?)GetValue(MinimumDateProperty);
        set => SetValue(MinimumDateProperty, value);
    }

    public static readonly DependencyProperty MaximumDateProperty =
        DependencyProperty.Register(nameof(MaximumDate), typeof(DateTime?), typeof(DatePicker),
            new PropertyMetadata(null, OnDateRangeChanged));

    public DateTime? MaximumDate
    {
        get => (DateTime?)GetValue(MaximumDateProperty);
        set => SetValue(MaximumDateProperty, value);
    }

    private static void OnDateRangeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        => ((DatePicker)d).BuildCalendar();

    // ── FirstDayOfWeek ─────────────────────────────────────────────
    public static readonly DependencyProperty FirstDayOfWeekProperty =
        DependencyProperty.Register(nameof(FirstDayOfWeek), typeof(DayOfWeek), typeof(DatePicker),
            new PropertyMetadata(DayOfWeek.Monday, (d, _) => ((DatePicker)d).BuildCalendar()));

    public DayOfWeek FirstDayOfWeek
    {
        get => (DayOfWeek)GetValue(FirstDayOfWeekProperty);
        set => SetValue(FirstDayOfWeekProperty, value);
    }

    // ── IsOpen ─────────────────────────────────────────────────────
    public static readonly DependencyProperty IsOpenProperty =
        DependencyProperty.Register(nameof(IsOpen), typeof(bool), typeof(DatePicker),
            new FrameworkPropertyMetadata(false,
                FrameworkPropertyMetadataOptions.BindsTwoWayByDefault,
                OnIsOpenChanged));

    public bool IsOpen
    {
        get => (bool)GetValue(IsOpenProperty);
        set => SetValue(IsOpenProperty, value);
    }

    private static void OnIsOpenChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        var picker = (DatePicker)d;
        if ((bool)e.NewValue)
        {
            picker.UpdateLabelState(true);
            picker.ScheduleFlyoutOpen();
        }
        else
        {
            picker.CancelDeferredOpen();
            picker._flyout?.Hide();
            picker.UpdateLabelState(true);
        }
    }

    // ── Readonly display properties ────────────────────────────────
    private static readonly DependencyPropertyKey DisplayTextKey =
        DependencyProperty.RegisterReadOnly(nameof(DisplayText), typeof(string), typeof(DatePicker),
            new PropertyMetadata(string.Empty));
    public static readonly DependencyProperty DisplayTextProperty = DisplayTextKey.DependencyProperty;
    public string DisplayText => (string)GetValue(DisplayTextProperty);

    private static readonly DependencyPropertyKey HasSelectedDateKey =
        DependencyProperty.RegisterReadOnly(nameof(HasSelectedDate), typeof(bool), typeof(DatePicker),
            new PropertyMetadata(false));
    public static readonly DependencyProperty HasSelectedDateProperty = HasSelectedDateKey.DependencyProperty;
    public bool HasSelectedDate => (bool)GetValue(HasSelectedDateProperty);

    private static readonly DependencyPropertyKey ViewMonthYearKey =
        DependencyProperty.RegisterReadOnly(nameof(ViewMonthYear), typeof(string), typeof(DatePicker),
            new PropertyMetadata(string.Empty));
    public static readonly DependencyProperty ViewMonthYearProperty = ViewMonthYearKey.DependencyProperty;
    public string ViewMonthYear => (string)GetValue(ViewMonthYearProperty);

    private static readonly DependencyPropertyKey CalendarDaysKey =
        DependencyProperty.RegisterReadOnly(nameof(CalendarDays), typeof(IReadOnlyList<CalendarDayItem>), typeof(DatePicker),
            new PropertyMetadata(Array.Empty<CalendarDayItem>()));
    public static readonly DependencyProperty CalendarDaysProperty = CalendarDaysKey.DependencyProperty;
    public IReadOnlyList<CalendarDayItem> CalendarDays => (IReadOnlyList<CalendarDayItem>)GetValue(CalendarDaysProperty);

    private static readonly DependencyPropertyKey WeekdayNamesKey =
        DependencyProperty.RegisterReadOnly(nameof(WeekdayNames), typeof(IReadOnlyList<string>), typeof(DatePicker),
            new PropertyMetadata(Array.Empty<string>()));
    public static readonly DependencyProperty WeekdayNamesProperty = WeekdayNamesKey.DependencyProperty;
    public IReadOnlyList<string> WeekdayNames => (IReadOnlyList<string>)GetValue(WeekdayNamesProperty);

    // ── Events ─────────────────────────────────────────────────────
    public static readonly RoutedEvent SelectedDateChangedEvent =
        EventManager.RegisterRoutedEvent(nameof(SelectedDateChanged), RoutingStrategy.Bubble,
            typeof(RoutedPropertyChangedEventHandler<DateTime?>), typeof(DatePicker));

    public event RoutedPropertyChangedEventHandler<DateTime?> SelectedDateChanged
    {
        add => AddHandler(SelectedDateChangedEvent, value);
        remove => RemoveHandler(SelectedDateChangedEvent, value);
    }

    public static readonly RoutedEvent DateRangeChangedEvent =
        EventManager.RegisterRoutedEvent(nameof(DateRangeChanged), RoutingStrategy.Bubble,
            typeof(RoutedEventHandler), typeof(DatePicker));

    public event RoutedEventHandler DateRangeChanged
    {
        add => AddHandler(DateRangeChangedEvent, value);
        remove => RemoveHandler(DateRangeChangedEvent, value);
    }

    public override void OnApplyTemplate()
    {
        if (_flyout != null && FlyoutIsOpenDescriptor != null)
            FlyoutIsOpenDescriptor.RemoveValueChanged(_flyout, OnFlyoutIsOpenChanged);
        if (_inputBorder != null)
        {
            _inputBorder.PreviewMouseLeftButtonDown -= OnInputBorderPreviewMouseLeftButtonDown;
            _inputBorder.MouseLeftButtonUp -= OnInputBorderMouseLeftButtonUp;
        }
        if (_popupInput != null)
            _popupInput.PreviewKeyDown -= OnPopupInputPreviewKeyDown;
        if (_dayItems != null)
            _dayItems.RemoveHandler(ButtonBase.ClickEvent, (RoutedEventHandler)OnDayButtonClick);
        if (_hintBlock != null)
            _hintBlock.LayoutUpdated -= OnHintLayoutUpdated;

        base.OnApplyTemplate();

        _notchedBorder = GetTemplateChild(PartContainer) as NotchedOutlineBorder;
        _hintBlock = GetTemplateChild(PartHint) as TextBlock;
        _lastOutlinedHintFloated = null;
        _lastNotchStart = double.NaN;
        _lastNotchWidth = double.NaN;
        if (_hintBlock != null)
            _hintBlock.LayoutUpdated += OnHintLayoutUpdated;

        _flyout = GetTemplateChild(PartFlyout) as Flyout;
        _inputBorder = GetTemplateChild(PartInputBorder) as FrameworkElement;
        if (_inputBorder != null)
        {
            _inputBorder.PreviewMouseLeftButtonDown += OnInputBorderPreviewMouseLeftButtonDown;
            _inputBorder.MouseLeftButtonUp += OnInputBorderMouseLeftButtonUp;
            if (_flyout != null)
                _flyout.PlacementTarget = _inputBorder;
        }

        if (_flyout != null && FlyoutIsOpenDescriptor != null)
            FlyoutIsOpenDescriptor.AddValueChanged(_flyout, OnFlyoutIsOpenChanged);

        _popupInput = GetTemplateChild(PartPopupInput) as WpfTextBox;
        if (_popupInput != null)
            _popupInput.PreviewKeyDown += OnPopupInputPreviewKeyDown;

        _dayItems = GetTemplateChild(PartDayItems) as ItemsControl;
        _dayItems?.AddHandler(ButtonBase.ClickEvent, (RoutedEventHandler)OnDayButtonClick);

        if (GetTemplateChild(PartPrevMonth) is WpfButton prevMonth)
            prevMonth.Click += (_, _) => NavigateMonth(-1);
        if (GetTemplateChild(PartNextMonth) is WpfButton nextMonth)
            nextMonth.Click += (_, _) => NavigateMonth(1);
        if (GetTemplateChild(PartTodayButton) is WpfButton today)
            today.Click += (_, _) => SelectToday();
        if (GetTemplateChild(PartClearButton) is WpfButton clear)
            clear.Click += (_, _) => ClearSelection();
        if (GetTemplateChild(PartCancelButton) is WpfButton cancel)
            cancel.Click += (_, _) => CancelSelection();
        if (GetTemplateChild(PartConfirmButton) is WpfButton confirm)
            confirm.Click += (_, _) => ConfirmSelection();

        UpdateDisplayText();
        BuildCalendar();
        UpdateLabelState(false);
        if (IsOpen)
            _flyout?.Show();
    }

    private void OnHintLayoutUpdated(object? sender, EventArgs e) => UpdateOutlinedNotch();

    private void OnInputBorderPreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        if (!IsEnabled || IsKeyboardFocusWithin)
            return;

        Focus();
    }

    private void OnInputBorderMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
    {
        if (!IsEnabled)
            return;

        if (!IsKeyboardFocusWithin)
            Focus();

        if (!IsOpen)
        {
            Dispatcher.BeginInvoke(DispatcherPriority.Input, new Action(() =>
            {
                if (IsEnabled && !IsOpen)
                    SetCurrentValue(IsOpenProperty, true);
            }));
        }

        e.Handled = true;
    }

    protected override void OnKeyDown(KeyEventArgs e)
    {
        base.OnKeyDown(e);
        if (e.Handled) return;

        switch (e.Key)
        {
            case Key.Enter:
            case Key.Space:
                SetCurrentValue(IsOpenProperty, true);
                e.Handled = true;
                break;
            case Key.Escape when IsOpen:
                SetCurrentValue(IsOpenProperty, false);
                e.Handled = true;
                break;
        }
    }

    protected override void OnGotKeyboardFocus(KeyboardFocusChangedEventArgs e)
    {
        base.OnGotKeyboardFocus(e);
        UpdateLabelState(true);
    }

    protected override void OnLostKeyboardFocus(KeyboardFocusChangedEventArgs e)
    {
        base.OnLostKeyboardFocus(e);
        UpdateLabelState(true);
    }

    private void OnFlyoutIsOpenChanged(object? sender, EventArgs e)
    {
        if (_flyout == null) return;

        if (_flyout.IsOpen && !IsOpen)
            SetCurrentValue(IsOpenProperty, true);
        else if (!_flyout.IsOpen && IsOpen)
            SetCurrentValue(IsOpenProperty, false);
        UpdateLabelState(true);
    }

    private void ScheduleFlyoutOpen()
    {
        if (_flyout == null || _flyout.IsOpen)
            return;

        _deferredOpenTimer ??= new DispatcherTimer(DispatcherPriority.Background, Dispatcher)
        {
            Interval = TimeSpan.FromMilliseconds(90)
        };

        _deferredOpenTimer.Tick -= OnDeferredOpenTimerTick;
        _deferredOpenTimer.Tick += OnDeferredOpenTimerTick;
        _deferredOpenTimer.Stop();
        _deferredOpenTimer.Start();
    }

    private void CancelDeferredOpen() => _deferredOpenTimer?.Stop();

    private void OnDeferredOpenTimerTick(object? sender, EventArgs e)
    {
        CancelDeferredOpen();

        if (!IsOpen || !IsEnabled || _flyout == null || _flyout.IsOpen)
            return;

        PrepareForOpen();
        _flyout.Show();
    }

    private void PrepareForOpen()
    {
        _pendingDate = SelectedDate;
        _pendingStartDate = StartDate;
        _pendingEndDate = EndDate;
        _viewDate = FirstOfMonth(SelectionMode == DatePickerSelectionMode.Range
            ? (_pendingStartDate ?? _pendingEndDate ?? DateTime.Today)
            : (_pendingDate ?? DateTime.Today));
        BuildCalendar();
        SyncPopupInput();
        Focus();
    }

    private void OnPopupInputPreviewKeyDown(object sender, KeyEventArgs e)
    {
        switch (e.Key)
        {
            case Key.Enter:
                ParsePopupInput(_popupInput?.Text);
                e.Handled = true;
                break;
            case Key.Escape:
                CancelSelection();
                e.Handled = true;
                break;
        }
    }

    private void OnDayButtonClick(object sender, RoutedEventArgs e)
    {
        if (e.OriginalSource is not WpfButton { Tag: CalendarDayItem day }) return;
        SelectDay(day);
        e.Handled = true;
    }

    private void SelectDay(CalendarDayItem day)
    {
        if (!day.IsEnabled) return;

        var date = day.Date.Date;
        if (SelectionMode == DatePickerSelectionMode.Range)
        {
            if (!_pendingStartDate.HasValue || (_pendingStartDate.HasValue && _pendingEndDate.HasValue))
            {
                _pendingStartDate = date;
                _pendingEndDate = null;
            }
            else if (date < _pendingStartDate.Value.Date)
            {
                _pendingEndDate = _pendingStartDate.Value.Date;
                _pendingStartDate = date;
            }
            else
            {
                _pendingEndDate = date;
            }
        }
        else
        {
            _pendingDate = date;
        }

        _viewDate = FirstOfMonth(date);
        BuildCalendar();
        SyncPopupInput();
    }

    private void NavigateMonth(int months)
    {
        _viewDate = _viewDate.AddMonths(months);
        BuildCalendar();
    }

    private void SelectToday()
    {
        var today = DateTime.Today;
        if (!IsDateSelectable(today)) return;

        if (SelectionMode == DatePickerSelectionMode.Range)
        {
            _pendingStartDate = today;
            _pendingEndDate = null;
        }
        else
        {
            _pendingDate = today;
        }

        _viewDate = FirstOfMonth(today);
        BuildCalendar();
        SyncPopupInput();
    }

    private void ClearSelection()
    {
        _pendingDate = null;
        _pendingStartDate = null;
        _pendingEndDate = null;
        SetCurrentValue(SelectedDateProperty, null);
        SetCurrentValue(StartDateProperty, null);
        SetCurrentValue(EndDateProperty, null);
        SetCurrentValue(IsOpenProperty, false);
    }

    private void CancelSelection()
    {
        _pendingDate = SelectedDate;
        _pendingStartDate = StartDate;
        _pendingEndDate = EndDate;
        SetCurrentValue(IsOpenProperty, false);
    }

    private void ConfirmSelection()
    {
        if (SelectionMode == DatePickerSelectionMode.Range)
        {
            if (_pendingStartDate.HasValue && !IsDateSelectable(_pendingStartDate.Value)) return;
            if (_pendingEndDate.HasValue && !IsDateSelectable(_pendingEndDate.Value)) return;
            SetCurrentValue(StartDateProperty, _pendingStartDate);
            SetCurrentValue(EndDateProperty, _pendingEndDate);
        }
        else
        {
            if (_pendingDate.HasValue && !IsDateSelectable(_pendingDate.Value)) return;
            SetCurrentValue(SelectedDateProperty, _pendingDate);
        }

        SetCurrentValue(IsOpenProperty, false);
    }

    private void ParsePopupInput(string? text)
    {
        if (SelectionMode == DatePickerSelectionMode.Range)
        {
            ParseRangePopupInput(text);
            return;
        }

        if (string.IsNullOrWhiteSpace(text))
        {
            _pendingDate = null;
            BuildCalendar();
            SyncPopupInput();
            return;
        }

        if (!TryParseDate(text, out var date) || !IsDateSelectable(date))
        {
            SyncPopupInput();
            return;
        }

        _pendingDate = date.Date;
        _viewDate = FirstOfMonth(date);
        BuildCalendar();
        SyncPopupInput();
    }

    private void ParseRangePopupInput(string? text)
    {
        if (string.IsNullOrWhiteSpace(text))
        {
            _pendingStartDate = null;
            _pendingEndDate = null;
            BuildCalendar();
            SyncPopupInput();
            return;
        }

        var parts = SplitRangeText(text);
        if (parts.Length == 0 || !TryParseDate(parts[0], out var start) || !IsDateSelectable(start))
        {
            SyncPopupInput();
            return;
        }

        DateTime? end = null;
        if (parts.Length > 1)
        {
            if (!TryParseDate(parts[1], out var parsedEnd) || !IsDateSelectable(parsedEnd))
            {
                SyncPopupInput();
                return;
            }
            end = parsedEnd.Date;
        }

        _pendingStartDate = start.Date;
        _pendingEndDate = end;
        NormalizePendingRange();
        _viewDate = FirstOfMonth(_pendingStartDate ?? _pendingEndDate ?? DateTime.Today);
        BuildCalendar();
        SyncPopupInput();
    }

    private string[] SplitRangeText(string text)
    {
        var separators = new[] { " 至 ", " — ", " – ", " ~ ", " to ", " TO " };
        foreach (var separator in separators)
        {
            var parts = text.Split(new[] { separator }, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
            if (parts.Length > 1) return parts;
        }
        return new[] { text.Trim() };
    }

    private bool TryParseDate(string text, out DateTime date)
    {
        var culture = CultureInfo.CurrentCulture;
        return DateTime.TryParseExact(text, DisplayFormat, culture, DateTimeStyles.None, out date)
               || DateTime.TryParse(text, culture, DateTimeStyles.None, out date);
    }

    private void BuildCalendar()
    {
        SetValue(WeekdayNamesKey, BuildWeekdayNames());
        SetValue(ViewMonthYearKey, _viewDate.ToString("yyyy年 M月", CultureInfo.CurrentCulture));

        var firstDay = FirstOfMonth(_viewDate);
        var offset = ((int)firstDay.DayOfWeek - (int)FirstDayOfWeek + 7) % 7;
        var daysInMonth = DateTime.DaysInMonth(firstDay.Year, firstDay.Month);
        var today = DateTime.Today;
        var days = new List<CalendarDayItem>(42);

        NormalizePendingRange();

        for (var i = 0; i < offset; i++)
            days.Add(CreateDay(firstDay.AddDays(i - offset), today, isOutsideMonth: true));

        for (var day = 1; day <= daysInMonth; day++)
            days.Add(CreateDay(new DateTime(firstDay.Year, firstDay.Month, day), today, isOutsideMonth: false));

        var nextMonthStart = firstDay.AddMonths(1);
        for (var i = 0; days.Count < 42; i++)
            days.Add(CreateDay(nextMonthStart.AddDays(i), today, isOutsideMonth: true));

        SetValue(CalendarDaysKey, days);
    }

    private CalendarDayItem CreateDay(DateTime date, DateTime today, bool isOutsideMonth)
    {
        var day = date.Date;
        var selectable = !isOutsideMonth && IsDateSelectable(day);
        var isRange = SelectionMode == DatePickerSelectionMode.Range;

        var pendingStart = _pendingStartDate?.Date;
        var pendingEnd = _pendingEndDate?.Date;
        var committedStart = StartDate?.Date;
        var committedEnd = EndDate?.Date;

        var isRangeStart = isRange && pendingStart.HasValue && day == pendingStart.Value;
        var isRangeEnd = isRange && pendingEnd.HasValue && day == pendingEnd.Value;
        var isInRange = isRange && pendingStart.HasValue && pendingEnd.HasValue && day > pendingStart.Value && day < pendingEnd.Value;
        var isSelected = isRange ? isRangeStart || isRangeEnd : _pendingDate.HasValue && day == _pendingDate.Value.Date;
        var isConfirmed = isRange
            ? (committedStart.HasValue && day == committedStart.Value) || (committedEnd.HasValue && day == committedEnd.Value)
            : SelectedDate.HasValue && day == SelectedDate.Value.Date;

        return new CalendarDayItem
        {
            Date = day,
            Label = day.Day.ToString(CultureInfo.CurrentCulture),
            IsToday = day == today,
            IsSelected = isSelected,
            IsConfirmed = isConfirmed,
            IsOutsideMonth = isOutsideMonth,
            IsEnabled = selectable,
            IsInRange = isInRange,
            IsRangeStart = isRangeStart,
            IsRangeEnd = isRangeEnd
        };
    }

    private IReadOnlyList<string> BuildWeekdayNames()
    {
        var names = CultureInfo.CurrentCulture.DateTimeFormat.AbbreviatedDayNames;
        var result = new string[7];
        for (var i = 0; i < 7; i++)
            result[i] = names[((int)FirstDayOfWeek + i) % 7];
        return result;
    }

    private bool IsDateSelectable(DateTime date)
    {
        var d = date.Date;
        if (MinimumDate.HasValue && d < MinimumDate.Value.Date) return false;
        if (MaximumDate.HasValue && d > MaximumDate.Value.Date) return false;
        return true;
    }

    private void SyncPopupInput()
    {
        if (_popupInput == null) return;
        _popupInput.Text = SelectionMode == DatePickerSelectionMode.Range
            ? FormatRange(_pendingStartDate, _pendingEndDate)
            : (_pendingDate.HasValue ? FormatDate(_pendingDate.Value) : string.Empty);
        _popupInput.SelectAll();
    }

    private void UpdateDisplayText()
    {
        var text = SelectionMode == DatePickerSelectionMode.Range
            ? FormatRange(StartDate, EndDate)
            : (SelectedDate.HasValue ? FormatDate(SelectedDate.Value) : string.Empty);
        SetValue(DisplayTextKey, text);
        SetValue(HasSelectedDateKey, SelectionMode == DatePickerSelectionMode.Range
            ? StartDate.HasValue || EndDate.HasValue
            : SelectedDate.HasValue);
        UpdateLabelState(true);
    }

    private void UpdateLabelState(bool useTransitions)
    {
        var floated = IsKeyboardFocusWithin || IsOpen || HasSelectedDate;
        if (_lastOutlinedHintFloated != floated)
        {
            ApplyHintTransform(floated, useTransitions);
            _lastOutlinedHintFloated = floated;
        }

        UpdateOutlinedNotch(floated);
    }

    private void ApplyHintTransform(bool floated, bool useTransitions)
    {
        if (EnsureWritableHintTransform() is not { } group)
            return;

        ScaleTransform? scale = null;
        TranslateTransform? translate = null;
        foreach (var child in group.Children)
        {
            switch (child)
            {
                case ScaleTransform s:
                    scale = s;
                    break;
                case TranslateTransform t:
                    translate = t;
                    break;
            }
        }

        if (scale == null || translate == null)
            return;

        var scaleValue = floated ? 0.75 : 1.0;
        var translateValue = floated ? -20.0 : -1.0;

        if (!useTransitions)
        {
            scale.BeginAnimation(ScaleTransform.ScaleXProperty, null);
            scale.BeginAnimation(ScaleTransform.ScaleYProperty, null);
            translate.BeginAnimation(TranslateTransform.YProperty, null);
            scale.ScaleX = scaleValue;
            scale.ScaleY = scaleValue;
            translate.Y = translateValue;
            return;
        }

        var easing = new CubicEase { EasingMode = floated ? EasingMode.EaseOut : EasingMode.EaseIn };
        var duration = TimeSpan.FromMilliseconds(150);
        scale.BeginAnimation(ScaleTransform.ScaleXProperty, new DoubleAnimation(scaleValue, duration) { EasingFunction = easing });
        scale.BeginAnimation(ScaleTransform.ScaleYProperty, new DoubleAnimation(scaleValue, duration) { EasingFunction = easing });
        translate.BeginAnimation(TranslateTransform.YProperty, new DoubleAnimation(translateValue, duration) { EasingFunction = easing });
    }

    private TransformGroup? EnsureWritableHintTransform()
    {
        if (_hintBlock?.RenderTransform is not TransformGroup group)
            return null;

        var hasFrozenChild = false;
        foreach (var child in group.Children)
        {
            if (!child.IsFrozen) continue;
            hasFrozenChild = true;
            break;
        }

        if (!group.IsFrozen && !hasFrozenChild)
            return group;

        var writableGroup = group.CloneCurrentValue();
        _hintBlock.RenderTransform = writableGroup;
        return writableGroup;
    }

    private void UpdateOutlinedNotch(bool? floatedOverride = null)
    {
        if (_notchedBorder == null || _hintBlock == null || Variant != TextFieldVariant.Outlined)
            return;

        var floated = floatedOverride ?? IsKeyboardFocusWithin || IsOpen || HasSelectedDate;

        if (floated)
        {
            const double gap = 4.0;
            var notchStart = _hintBlock.Margin.Left - gap;
            var notchWidth = Math.Max(0, _hintBlock.ActualWidth * 0.75 + gap * 2);
            if (!AreClose(_lastNotchStart, notchStart))
            {
                _notchedBorder.NotchStart = notchStart;
                _lastNotchStart = notchStart;
            }
            if (!AreClose(_lastNotchWidth, notchWidth))
            {
                _notchedBorder.NotchWidth = notchWidth;
                _lastNotchWidth = notchWidth;
            }
        }
        else if (!AreClose(_lastNotchWidth, 0))
        {
            _notchedBorder.NotchWidth = 0;
            _lastNotchWidth = 0;
        }
    }


    private static bool AreClose(double a, double b) => Math.Abs(a - b) < 0.1;

    private string FormatDate(DateTime date) => date.ToString(DisplayFormat, CultureInfo.CurrentCulture);

    private string FormatRange(DateTime? start, DateTime? end)
    {
        if (start.HasValue && end.HasValue) return $"{FormatDate(start.Value)} – {FormatDate(end.Value)}";
        if (start.HasValue) return $"{FormatDate(start.Value)} –";
        if (end.HasValue) return $"– {FormatDate(end.Value)}";
        return string.Empty;
    }

    private void NormalizePendingRange()
    {
        if (_pendingStartDate.HasValue && _pendingEndDate.HasValue && _pendingEndDate.Value.Date < _pendingStartDate.Value.Date)
            (_pendingStartDate, _pendingEndDate) = (_pendingEndDate.Value.Date, _pendingStartDate.Value.Date);
    }

    private static DateTime FirstOfMonth(DateTime date) => new(date.Year, date.Month, 1);
}

/// <summary>暴露 <see cref="DatePicker"/> 的展开状态与显示文本给屏幕阅读器 / UI 自动化。</summary>
public class DatePickerAutomationPeer : FrameworkElementAutomationPeer, IExpandCollapseProvider, IValueProvider
{
    public DatePickerAutomationPeer(DatePicker owner) : base(owner) { }

    private DatePicker Control => (DatePicker)Owner;

    public override object GetPattern(PatternInterface patternInterface)
        => patternInterface is PatternInterface.ExpandCollapse or PatternInterface.Value
            ? this
            : base.GetPattern(patternInterface);

    protected override string GetClassNameCore() => nameof(DatePicker);
    protected override AutomationControlType GetAutomationControlTypeCore() => AutomationControlType.ComboBox;

    public ExpandCollapseState ExpandCollapseState => Control.IsOpen ? ExpandCollapseState.Expanded : ExpandCollapseState.Collapsed;
    public void Expand() => Control.IsOpen = true;
    public void Collapse() => Control.IsOpen = false;

    public bool IsReadOnly => true;
    public string Value => Control.DisplayText;
    public void SetValue(string value) => throw new InvalidOperationException("DatePicker 仅支持通过日历或输入框设置日期。");
}

/// <summary>DatePicker 选择模式。</summary>
public enum DatePickerSelectionMode
{
    Single,
    Range
}
