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

public sealed class TimePickerClockItem
{
    public int Value { get; init; }
    public string Label { get; init; } = string.Empty;
    public bool IsSelected { get; init; }
    public bool IsMajorLabel { get; init; }
    public bool IsMajorTick { get; init; }
    public double X { get; init; }
    public double Y { get; init; }
    public double Size { get; init; }
}

public enum TimePickerClockMode
{
    Hour,
    Minute,
    Second
}

public enum TimePickerPrecision
{
    Minute,
    Second,
    Millisecond
}

/// <summary>
/// Material Design 3 时间选择器。支持 Filled / Outlined、12/24 小时制、环形 clock 选择时/分/秒以及毫秒输入。
/// </summary>
[TemplatePart(Name = PartInputBorder, Type = typeof(FrameworkElement))]
[TemplatePart(Name = PartContainer, Type = typeof(NotchedOutlineBorder))]
[TemplatePart(Name = PartHint, Type = typeof(TextBlock))]
[TemplatePart(Name = PartFlyout, Type = typeof(Flyout))]
[TemplatePart(Name = PartPopupInput, Type = typeof(WpfTextBox))]
[TemplatePart(Name = PartClockItems, Type = typeof(ItemsControl))]
[TemplatePart(Name = PartHourHeader, Type = typeof(WpfButton))]
[TemplatePart(Name = PartMinuteHeader, Type = typeof(WpfButton))]
[TemplatePart(Name = PartSecondHeader, Type = typeof(WpfButton))]
[TemplatePart(Name = PartMillisecondInput, Type = typeof(NumericUpDown))]
[TemplatePart(Name = PartAmButton, Type = typeof(WpfButton))]
[TemplatePart(Name = PartPmButton, Type = typeof(WpfButton))]
[TemplatePart(Name = PartNowButton, Type = typeof(WpfButton))]
[TemplatePart(Name = PartClearButton, Type = typeof(WpfButton))]
[TemplatePart(Name = PartCancelButton, Type = typeof(WpfButton))]
[TemplatePart(Name = PartConfirmButton, Type = typeof(WpfButton))]
[TemplateVisualState(Name = "LabelNormal", GroupName = "LabelStates")]
[TemplateVisualState(Name = "LabelFloated", GroupName = "LabelStates")]
public class TimePicker : Control
{
    private const string PartInputBorder = "PART_InputBorder";
    private const string PartContainer = "Container";
    private const string PartHint = "PART_Hint";
    private const string PartFlyout = "PART_Flyout";
    private const string PartPopupInput = "PART_PopupInput";
    private const string PartClockItems = "PART_ClockItems";
    private const string PartHourHeader = "PART_HourHeader";
    private const string PartMinuteHeader = "PART_MinuteHeader";
    private const string PartSecondHeader = "PART_SecondHeader";
    private const string PartMillisecondInput = "PART_MillisecondInput";
    private const string PartAmButton = "PART_AmButton";
    private const string PartPmButton = "PART_PmButton";
    private const string PartNowButton = "PART_NowButton";
    private const string PartClearButton = "PART_ClearButton";
    private const string PartCancelButton = "PART_CancelButton";
    private const string PartConfirmButton = "PART_ConfirmButton";

    private const double ClockFaceSize = 280;
    private const double ClockCenter = ClockFaceSize / 2;

    private static readonly DependencyPropertyDescriptor? FlyoutIsOpenDescriptor =
        DependencyPropertyDescriptor.FromProperty(Flyout.IsOpenProperty, typeof(Flyout));

    private Flyout? _flyout;
    private FrameworkElement? _inputBorder;
    private NotchedOutlineBorder? _notchedBorder;
    private TextBlock? _hintBlock;
    private WpfTextBox? _popupInput;
    private ItemsControl? _clockItems;
    private NumericUpDown? _millisecondInput;
    private DispatcherTimer? _deferredOpenTimer;
    private TimeSpan? _pendingTime;
    private bool? _lastOutlinedHintFloated;
    private double _lastNotchStart = double.NaN;
    private double _lastNotchWidth = double.NaN;
    private bool _suppressMillisecondSync;

    static TimePicker()
    {
        DefaultStyleKeyProperty.OverrideMetadata(typeof(TimePicker), new FrameworkPropertyMetadata(typeof(TimePicker)));
        FocusableProperty.OverrideMetadata(typeof(TimePicker), new FrameworkPropertyMetadata(true));
    }

    public static readonly DependencyProperty VariantProperty =
        DependencyProperty.Register(nameof(Variant), typeof(TextFieldVariant), typeof(TimePicker),
            new PropertyMetadata(TextFieldVariant.Outlined));

    public TextFieldVariant Variant
    {
        get => (TextFieldVariant)GetValue(VariantProperty);
        set => SetValue(VariantProperty, value);
    }

    public static readonly DependencyProperty SelectedTimeProperty =
        DependencyProperty.Register(nameof(SelectedTime), typeof(TimeSpan?), typeof(TimePicker),
            new FrameworkPropertyMetadata(null,
                FrameworkPropertyMetadataOptions.BindsTwoWayByDefault,
                OnSelectedTimeChanged,
                CoerceSelectedTime));

    public TimeSpan? SelectedTime
    {
        get => (TimeSpan?)GetValue(SelectedTimeProperty);
        set => SetValue(SelectedTimeProperty, value);
    }

    private static void OnSelectedTimeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        var picker = (TimePicker)d;
        picker.UpdateDisplayText();
        picker.BuildClock();
        picker.RaiseEvent(new RoutedPropertyChangedEventArgs<TimeSpan?>((TimeSpan?)e.OldValue, (TimeSpan?)e.NewValue, SelectedTimeChangedEvent));
    }

    private static object? CoerceSelectedTime(DependencyObject d, object? baseValue)
    {
        if (baseValue is not TimeSpan time)
            return null;

        return ((TimePicker)d).ApplyPrecision(time);
    }

    public static readonly DependencyProperty DisplayFormatProperty =
        DependencyProperty.Register(nameof(DisplayFormat), typeof(string), typeof(TimePicker),
            new PropertyMetadata("HH:mm:ss.fff", OnDisplayFormatChanged));

    public string DisplayFormat
    {
        get => (string)GetValue(DisplayFormatProperty);
        set => SetValue(DisplayFormatProperty, value);
    }

    private static void OnDisplayFormatChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        var picker = (TimePicker)d;
        picker.UpdateDisplayText();
        picker.SyncPopupInput();
    }

    public static readonly DependencyProperty HintProperty =
        DependencyProperty.Register(nameof(Hint), typeof(string), typeof(TimePicker),
            new PropertyMetadata("选择时间"));

    public string? Hint
    {
        get => (string?)GetValue(HintProperty);
        set => SetValue(HintProperty, value);
    }

    public static readonly DependencyProperty HelperTextProperty =
        DependencyProperty.Register(nameof(HelperText), typeof(string), typeof(TimePicker),
            new PropertyMetadata(string.Empty));

    public string? HelperText
    {
        get => (string?)GetValue(HelperTextProperty);
        set => SetValue(HelperTextProperty, value);
    }

    public static readonly DependencyProperty Is24HoursProperty =
        DependencyProperty.Register(nameof(Is24Hours), typeof(bool), typeof(TimePicker),
            new PropertyMetadata(true, OnClockSettingsChanged));

    public bool Is24Hours
    {
        get => (bool)GetValue(Is24HoursProperty);
        set => SetValue(Is24HoursProperty, value);
    }

    public static readonly DependencyProperty MinuteIntervalProperty =
        DependencyProperty.Register(nameof(MinuteInterval), typeof(int), typeof(TimePicker),
            new FrameworkPropertyMetadata(1, OnClockSettingsChanged, CoerceClockInterval));

    public int MinuteInterval
    {
        get => (int)GetValue(MinuteIntervalProperty);
        set => SetValue(MinuteIntervalProperty, value);
    }

    public static readonly DependencyProperty SecondIntervalProperty =
        DependencyProperty.Register(nameof(SecondInterval), typeof(int), typeof(TimePicker),
            new FrameworkPropertyMetadata(1, OnClockSettingsChanged, CoerceClockInterval));

    public int SecondInterval
    {
        get => (int)GetValue(SecondIntervalProperty);
        set => SetValue(SecondIntervalProperty, value);
    }

    public static readonly DependencyProperty MillisecondIntervalProperty =
        DependencyProperty.Register(nameof(MillisecondInterval), typeof(int), typeof(TimePicker),
            new FrameworkPropertyMetadata(1, OnClockSettingsChanged, CoerceMillisecondInterval));

    public int MillisecondInterval
    {
        get => (int)GetValue(MillisecondIntervalProperty);
        set => SetValue(MillisecondIntervalProperty, value);
    }

    private static object CoerceClockInterval(DependencyObject d, object baseValue)
        => Math.Clamp((int)baseValue, 1, 59);

    private static object CoerceMillisecondInterval(DependencyObject d, object baseValue)
        => Math.Clamp((int)baseValue, 1, 999);

    private static void OnClockSettingsChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        var picker = (TimePicker)d;
        picker.BuildClock();
        picker.SyncPopupInput();
    }

    public static readonly DependencyProperty PrecisionProperty =
        DependencyProperty.Register(nameof(Precision), typeof(TimePickerPrecision), typeof(TimePicker),
            new FrameworkPropertyMetadata(TimePickerPrecision.Millisecond, OnPrecisionChanged));

    public TimePickerPrecision Precision
    {
        get => (TimePickerPrecision)GetValue(PrecisionProperty);
        set => SetValue(PrecisionProperty, value);
    }

    private static void OnPrecisionChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        var picker = (TimePicker)d;
        picker.CoerceValue(SelectedTimeProperty);
        if (picker._pendingTime.HasValue)
            picker._pendingTime = picker.ApplyPrecision(picker._pendingTime.Value);

        picker.UpdateDisplayText();
        picker.BuildClock();
        picker.SyncPopupInput();
        picker.SyncMillisecondInput();
    }

    public static readonly DependencyProperty ClockModeProperty =
        DependencyProperty.Register(nameof(ClockMode), typeof(TimePickerClockMode), typeof(TimePicker),
            new PropertyMetadata(TimePickerClockMode.Hour, OnClockModeChanged));

    public TimePickerClockMode ClockMode
    {
        get => (TimePickerClockMode)GetValue(ClockModeProperty);
        set => SetValue(ClockModeProperty, value);
    }

    private static void OnClockModeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        => ((TimePicker)d).BuildClock();

    public static readonly DependencyProperty IsOpenProperty =
        DependencyProperty.Register(nameof(IsOpen), typeof(bool), typeof(TimePicker),
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
        var picker = (TimePicker)d;
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

    private static readonly DependencyPropertyKey DisplayTextKey =
        DependencyProperty.RegisterReadOnly(nameof(DisplayText), typeof(string), typeof(TimePicker),
            new PropertyMetadata(string.Empty));
    public static readonly DependencyProperty DisplayTextProperty = DisplayTextKey.DependencyProperty;
    public string DisplayText => (string)GetValue(DisplayTextProperty);

    private static readonly DependencyPropertyKey HasSelectedTimeKey =
        DependencyProperty.RegisterReadOnly(nameof(HasSelectedTime), typeof(bool), typeof(TimePicker),
            new PropertyMetadata(false));
    public static readonly DependencyProperty HasSelectedTimeProperty = HasSelectedTimeKey.DependencyProperty;
    public bool HasSelectedTime => (bool)GetValue(HasSelectedTimeProperty);

    private static readonly DependencyPropertyKey ClockItemsKey =
        DependencyProperty.RegisterReadOnly(nameof(ClockItems), typeof(IReadOnlyList<TimePickerClockItem>), typeof(TimePicker),
            new PropertyMetadata(Array.Empty<TimePickerClockItem>()));
    public static readonly DependencyProperty ClockItemsProperty = ClockItemsKey.DependencyProperty;
    public IReadOnlyList<TimePickerClockItem> ClockItems => (IReadOnlyList<TimePickerClockItem>)GetValue(ClockItemsProperty);

    private static readonly DependencyPropertyKey IsAmPendingKey =
        DependencyProperty.RegisterReadOnly(nameof(IsAmPending), typeof(bool), typeof(TimePicker),
            new PropertyMetadata(false));
    public static readonly DependencyProperty IsAmPendingProperty = IsAmPendingKey.DependencyProperty;
    public bool IsAmPending => (bool)GetValue(IsAmPendingProperty);

    private static readonly DependencyPropertyKey IsPmPendingKey =
        DependencyProperty.RegisterReadOnly(nameof(IsPmPending), typeof(bool), typeof(TimePicker),
            new PropertyMetadata(false));
    public static readonly DependencyProperty IsPmPendingProperty = IsPmPendingKey.DependencyProperty;
    public bool IsPmPending => (bool)GetValue(IsPmPendingProperty);

    private static readonly DependencyPropertyKey PendingHourTextKey =
        DependencyProperty.RegisterReadOnly(nameof(PendingHourText), typeof(string), typeof(TimePicker),
            new PropertyMetadata("--"));
    public static readonly DependencyProperty PendingHourTextProperty = PendingHourTextKey.DependencyProperty;
    public string PendingHourText => (string)GetValue(PendingHourTextProperty);

    private static readonly DependencyPropertyKey PendingMinuteTextKey =
        DependencyProperty.RegisterReadOnly(nameof(PendingMinuteText), typeof(string), typeof(TimePicker),
            new PropertyMetadata("--"));
    public static readonly DependencyProperty PendingMinuteTextProperty = PendingMinuteTextKey.DependencyProperty;
    public string PendingMinuteText => (string)GetValue(PendingMinuteTextProperty);

    private static readonly DependencyPropertyKey PendingSecondTextKey =
        DependencyProperty.RegisterReadOnly(nameof(PendingSecondText), typeof(string), typeof(TimePicker),
            new PropertyMetadata("--"));
    public static readonly DependencyProperty PendingSecondTextProperty = PendingSecondTextKey.DependencyProperty;
    public string PendingSecondText => (string)GetValue(PendingSecondTextProperty);

    private static readonly DependencyPropertyKey PendingMillisecondTextKey =
        DependencyProperty.RegisterReadOnly(nameof(PendingMillisecondText), typeof(string), typeof(TimePicker),
            new PropertyMetadata("---"));
    public static readonly DependencyProperty PendingMillisecondTextProperty = PendingMillisecondTextKey.DependencyProperty;
    public string PendingMillisecondText => (string)GetValue(PendingMillisecondTextProperty);

    private static readonly DependencyPropertyKey PendingPeriodTextKey =
        DependencyProperty.RegisterReadOnly(nameof(PendingPeriodText), typeof(string), typeof(TimePicker),
            new PropertyMetadata(string.Empty));
    public static readonly DependencyProperty PendingPeriodTextProperty = PendingPeriodTextKey.DependencyProperty;
    public string PendingPeriodText => (string)GetValue(PendingPeriodTextProperty);

    public static readonly RoutedEvent SelectedTimeChangedEvent =
        EventManager.RegisterRoutedEvent(nameof(SelectedTimeChanged), RoutingStrategy.Bubble,
            typeof(RoutedPropertyChangedEventHandler<TimeSpan?>), typeof(TimePicker));

    public event RoutedPropertyChangedEventHandler<TimeSpan?> SelectedTimeChanged
    {
        add => AddHandler(SelectedTimeChangedEvent, value);
        remove => RemoveHandler(SelectedTimeChangedEvent, value);
    }

    public override void OnApplyTemplate()
    {
        CancelDeferredOpen();
        if (_flyout != null && FlyoutIsOpenDescriptor != null)
            FlyoutIsOpenDescriptor.RemoveValueChanged(_flyout, OnFlyoutIsOpenChanged);
        if (_inputBorder != null)
        {
            _inputBorder.PreviewMouseLeftButtonDown -= OnInputBorderPreviewMouseLeftButtonDown;
            _inputBorder.MouseLeftButtonUp -= OnInputBorderMouseLeftButtonUp;
        }
        if (_popupInput != null)
            _popupInput.PreviewKeyDown -= OnPopupInputPreviewKeyDown;
        if (_clockItems != null)
            _clockItems.RemoveHandler(ButtonBase.ClickEvent, (RoutedEventHandler)OnClockItemClick);
        if (_millisecondInput != null)
            _millisecondInput.ValueChanged -= OnMillisecondValueChanged;
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

        _clockItems = GetTemplateChild(PartClockItems) as ItemsControl;
        _clockItems?.AddHandler(ButtonBase.ClickEvent, (RoutedEventHandler)OnClockItemClick);

        _millisecondInput = GetTemplateChild(PartMillisecondInput) as NumericUpDown;
        if (_millisecondInput != null)
            _millisecondInput.ValueChanged += OnMillisecondValueChanged;

        if (GetTemplateChild(PartHourHeader) is WpfButton hourHeader)
            hourHeader.Click += (_, _) => ClockMode = TimePickerClockMode.Hour;
        if (GetTemplateChild(PartMinuteHeader) is WpfButton minuteHeader)
            minuteHeader.Click += (_, _) => ClockMode = TimePickerClockMode.Minute;
        if (GetTemplateChild(PartSecondHeader) is WpfButton secondHeader)
            secondHeader.Click += (_, _) => ClockMode = TimePickerClockMode.Second;
        if (GetTemplateChild(PartAmButton) is WpfButton amButton)
            amButton.Click += (_, _) => SelectMeridiem(false);
        if (GetTemplateChild(PartPmButton) is WpfButton pmButton)
            pmButton.Click += (_, _) => SelectMeridiem(true);
        if (GetTemplateChild(PartNowButton) is WpfButton nowButton)
            nowButton.Click += (_, _) => SelectNow();
        if (GetTemplateChild(PartClearButton) is WpfButton clearButton)
            clearButton.Click += (_, _) => ClearSelection();
        if (GetTemplateChild(PartCancelButton) is WpfButton cancelButton)
            cancelButton.Click += (_, _) => CancelSelection();
        if (GetTemplateChild(PartConfirmButton) is WpfButton confirmButton)
            confirmButton.Click += (_, _) => ConfirmSelection();

        UpdateDisplayText();
        BuildClock();
        UpdateLabelState(false);
        if (IsOpen)
            ScheduleFlyoutOpen();
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

        var key = e.Key == Key.System ? e.SystemKey : e.Key;

        if (key is Key.Enter or Key.Space)
        {
            SetCurrentValue(IsOpenProperty, true);
            e.Handled = true;
        }
        else if (key == Key.Escape && IsOpen)
        {
            SetCurrentValue(IsOpenProperty, false);
            e.Handled = true;
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
        _pendingTime = SelectedTime.HasValue ? ApplyPrecision(SelectedTime.Value) : null;
        ClockMode = TimePickerClockMode.Hour;
        BuildClock();
        SyncPopupInput();
        SyncMillisecondInput();
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

    private void OnClockItemClick(object sender, RoutedEventArgs e)
    {
        if (e.OriginalSource is not DependencyObject source)
            return;

        var button = FindAncestorButton(source);
        if (button?.Tag is not TimePickerClockItem item)
            return;

        switch (ClockMode)
        {
            case TimePickerClockMode.Hour:
                SelectHour(item.Value);
                break;
            case TimePickerClockMode.Minute:
                SelectMinute(item.Value);
                break;
            case TimePickerClockMode.Second:
                SelectSecond(item.Value);
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }

        e.Handled = true;
    }

    private void OnMillisecondValueChanged(object? sender, RoutedPropertyChangedEventArgs<double> e)
    {
        if (_suppressMillisecondSync)
            return;

        SetMilliseconds((int)Math.Round(e.NewValue));
    }

    private void SelectHour(int value)
    {
        var current = GetEditingTime();
        var hour = Is24Hours ? value : ConvertTo24Hour(value, current.Hours >= 12);
        _pendingTime = ApplyPrecision(CreateTime(hour, current.Minutes, current.Seconds, current.Milliseconds));
        ClockMode = TimePickerClockMode.Minute;
        RefreshPendingState();
    }

    private void SelectMinute(int value)
    {
        var current = GetEditingTime();
        _pendingTime = ApplyPrecision(CreateTime(current.Hours, value, current.Seconds, current.Milliseconds));
        if (SupportsSeconds)
            ClockMode = TimePickerClockMode.Second;
        RefreshPendingState();
    }

    private void SelectSecond(int value)
    {
        var current = GetEditingTime();
        _pendingTime = ApplyPrecision(CreateTime(current.Hours, current.Minutes, value, current.Milliseconds));
        RefreshPendingState();
    }

    private void SetMilliseconds(int value)
    {
        if (!SupportsMilliseconds)
            return;

        var current = GetEditingTime();
        _pendingTime = ApplyPrecision(CreateTime(current.Hours, current.Minutes, current.Seconds, Math.Clamp(value, 0, 999)));
        RefreshPendingState(syncMillisecondsInput: false);
    }

    private void SelectMeridiem(bool isPm)
    {
        var current = GetEditingTime();
        var hour = current.Hours;
        if (isPm && hour < 12)
            hour += 12;
        else if (!isPm && hour >= 12)
            hour -= 12;

        _pendingTime = ApplyPrecision(CreateTime(hour, current.Minutes, current.Seconds, current.Milliseconds));
        RefreshPendingState();
    }

    private void SelectNow()
    {
        _pendingTime = ApplyPrecision(NormalizeToMilliseconds(DateTime.Now.TimeOfDay));
        RefreshPendingState();
    }

    private void ClearSelection()
    {
        _pendingTime = null;
        SetCurrentValue(SelectedTimeProperty, null);
        SetCurrentValue(IsOpenProperty, false);
    }

    private void CancelSelection()
    {
        _pendingTime = SelectedTime;
        SetCurrentValue(IsOpenProperty, false);
    }

    private void ConfirmSelection()
    {
        SetCurrentValue(SelectedTimeProperty, _pendingTime);
        SetCurrentValue(IsOpenProperty, false);
    }

    private void ParsePopupInput(string? text)
    {
        if (string.IsNullOrWhiteSpace(text))
        {
            _pendingTime = null;
            RefreshPendingState();
            return;
        }

        if (!TryParseTime(text, out var time))
        {
            SyncPopupInput();
            SyncMillisecondInput();
            return;
        }

        _pendingTime = ApplyPrecision(time);
        RefreshPendingState();
    }

    private bool TryParseTime(string text, out TimeSpan time)
    {
        var culture = CultureInfo.CurrentCulture;
        if (DateTime.TryParseExact(text, DisplayFormat, culture, DateTimeStyles.NoCurrentDateDefault, out var exactTime)
            || DateTime.TryParse(text, culture, DateTimeStyles.NoCurrentDateDefault, out exactTime))
        {
            time = NormalizeToMilliseconds(exactTime.TimeOfDay);
            return true;
        }

        if (TimeSpan.TryParse(text, culture, out var parsed))
        {
            time = NormalizeToMilliseconds(parsed);
            return true;
        }

        time = TimeSpan.Zero;
        return false;
    }

    private void RefreshPendingState(bool syncMillisecondsInput = true)
    {
        BuildClock();
        SyncPopupInput();
        if (syncMillisecondsInput)
            SyncMillisecondInput();
    }

    private void BuildClock()
    {
        var coercedClockMode = CoerceClockModeForPrecision(ClockMode);
        if (coercedClockMode != ClockMode)
        {
            SetCurrentValue(ClockModeProperty, coercedClockMode);
            return;
        }

        SetValue(ClockItemsKey,  BuildClockItems());
        SetValue(IsAmPendingKey, _pendingTime is { Hours: < 12 });
        SetValue(IsPmPendingKey, _pendingTime is { Hours: >= 12 });
        UpdatePendingHeaderText();
        SyncMillisecondInput();
    }

    private IReadOnlyList<TimePickerClockItem> BuildClockItems()
    {
        return ClockMode switch
        {
            TimePickerClockMode.Hour => BuildHourClockItems(),
            TimePickerClockMode.Minute => BuildMinuteSecondClockItems(GetEditingTime().Minutes),
            TimePickerClockMode.Second => BuildMinuteSecondClockItems(GetEditingTime().Seconds),
            _ => Array.Empty<TimePickerClockItem>()
        };
    }

    private IReadOnlyList<TimePickerClockItem> BuildHourClockItems()
    {
        var currentHour = GetEditingTime().Hours;
        var items = new List<TimePickerClockItem>(Is24Hours ? 24 : 12);

        if (Is24Hours)
        {
            for (var index = 0; index < 12; index++)
            {
                var outerValue = index == 0 ? 12 : index;
                items.Add(CreateClockItem(outerValue, outerValue.ToString("00", CultureInfo.CurrentCulture), index * 30 - 90, 104, 40, currentHour == outerValue, true));

                var innerValue = index == 0 ? 0 : index + 12;
                items.Add(CreateClockItem(innerValue, innerValue.ToString("00", CultureInfo.CurrentCulture), index * 30 - 90, 68, 34, currentHour == innerValue, true));
            }
        }
        else
        {
            for (var hour = 1; hour <= 12; hour++)
            {
                var angle = (hour % 12) * 30 - 90;
                items.Add(CreateClockItem(hour, hour.ToString("00", CultureInfo.CurrentCulture), angle, 100, 40, ToTwelveHour(currentHour) == hour, true));
            }
        }

        return items;
    }

    private IReadOnlyList<TimePickerClockItem> BuildMinuteSecondClockItems(int selectedValue)
    {
        const double tickRadius = 106;
        const double labelRadius = 84;

        var items = new List<TimePickerClockItem>(72);
        for (var value = 0; value < 60; value++)
        {
            var angle = value * 6 - 90;
            var isMajorTick = value % 5 == 0;

            // 完整外圈刻度始终保留；无论是否为 5 的倍数，选中时对应刻度都需要同步高亮。
            items.Add(CreateClockItem(
                value,
                string.Empty,
                angle,
                tickRadius,
                isMajorTick ? 22 : 16,
                selectedValue == value,
                false,
                isMajorTick));

            if (!isMajorTick)
                continue;

            // 5 的倍数数字放到独立内圈，因此外圈对应刻度不再缺失，同时数字与刻度都可参与选中高亮。
            items.Add(CreateClockItem(
                value,
                value.ToString("00", CultureInfo.CurrentCulture),
                angle,
                labelRadius,
                40,
                selectedValue == value,
                true));
        }

        return items;
    }

    private static TimePickerClockItem CreateClockItem(int value, string label, double angleDegrees, double radius, double size, bool isSelected, bool isMajorLabel, bool isMajorTick = false)
    {
        var radians = angleDegrees * Math.PI / 180.0;
        var centerX = ClockCenter + Math.Cos(radians) * radius;
        var centerY = ClockCenter + Math.Sin(radians) * radius;

        return new TimePickerClockItem
        {
            Value = value,
            Label = label,
            IsSelected = isSelected,
            IsMajorLabel = isMajorLabel,
            IsMajorTick = isMajorTick,
            Size = size,
            X = centerX - size / 2,
            Y = centerY - size / 2
        };
    }

    private void UpdatePendingHeaderText()
    {
        if (!_pendingTime.HasValue)
        {
            SetValue(PendingHourTextKey, "--");
            SetValue(PendingMinuteTextKey, "--");
            SetValue(PendingSecondTextKey, SupportsSeconds ? "--" : string.Empty);
            SetValue(PendingMillisecondTextKey, SupportsMilliseconds ? "---" : string.Empty);
            SetValue(PendingPeriodTextKey, Is24Hours ? string.Empty : "AM");
            return;
        }

        var current = ApplyPrecision(_pendingTime.Value);
        var hours = current.Hours;
        SetValue(PendingHourTextKey, (Is24Hours ? hours : ToTwelveHour(hours)).ToString("00", CultureInfo.CurrentCulture));
        SetValue(PendingMinuteTextKey, current.Minutes.ToString("00", CultureInfo.CurrentCulture));
        SetValue(PendingSecondTextKey, SupportsSeconds ? current.Seconds.ToString("00", CultureInfo.CurrentCulture) : string.Empty);
        SetValue(PendingMillisecondTextKey, SupportsMilliseconds ? current.Milliseconds.ToString("000", CultureInfo.CurrentCulture) : string.Empty);
        SetValue(PendingPeriodTextKey, Is24Hours ? string.Empty : (hours >= 12 ? "PM" : "AM"));
    }

    private TimeSpan GetEditingTime() => _pendingTime ?? NormalizeToMilliseconds(DateTime.Now.TimeOfDay);

    private void SyncPopupInput()
    {
        if (_popupInput == null) return;
        _popupInput.Text = _pendingTime.HasValue ? FormatTime(_pendingTime.Value) : string.Empty;
        _popupInput.SelectAll();
    }

    private void SyncMillisecondInput()
    {
        if (_millisecondInput == null)
            return;

        _suppressMillisecondSync = true;
        try
        {
            _millisecondInput.Minimum = 0;
            _millisecondInput.Maximum = 999;
            _millisecondInput.SmallChange = MillisecondInterval;
            _millisecondInput.StringFormat = "F0";
            _millisecondInput.Value = SupportsMilliseconds ? _pendingTime?.Milliseconds ?? 0 : 0;
            _millisecondInput.IsEnabled = SupportsMilliseconds;
        }
        finally
        {
            _suppressMillisecondSync = false;
        }
    }

    private void UpdateDisplayText()
    {
        SetValue(DisplayTextKey, SelectedTime.HasValue ? FormatTime(SelectedTime.Value) : string.Empty);
        SetValue(HasSelectedTimeKey, SelectedTime.HasValue);
        UpdateLabelState(true);
    }

    private string FormatTime(TimeSpan time) => DateTime.Today.Add(ApplyPrecision(time)).ToString(GetEffectiveDisplayFormat(), CultureInfo.CurrentCulture);

    private string GetEffectiveDisplayFormat()
    {
        if (string.IsNullOrWhiteSpace(DisplayFormat))
            return GetDefaultDisplayFormat();

        return Precision switch
        {
            TimePickerPrecision.Minute when DisplayFormat is "HH:mm:ss.fff" or "HH:mm:ss" => Is24Hours ? "HH:mm" : "hh:mm tt",
            TimePickerPrecision.Second when DisplayFormat == "HH:mm:ss.fff" => Is24Hours ? "HH:mm:ss" : "hh:mm:ss tt",
            _ => DisplayFormat
        };
    }

    private string GetDefaultDisplayFormat() => Precision switch
    {
        TimePickerPrecision.Minute => Is24Hours ? "HH:mm" : "hh:mm tt",
        TimePickerPrecision.Second => Is24Hours ? "HH:mm:ss" : "hh:mm:ss tt",
        _ => Is24Hours ? "HH:mm:ss.fff" : "hh:mm:ss.fff tt"
    };

    private bool SupportsSeconds => Precision is TimePickerPrecision.Second or TimePickerPrecision.Millisecond;

    private bool SupportsMilliseconds => Precision == TimePickerPrecision.Millisecond;

    private TimePickerClockMode CoerceClockModeForPrecision(TimePickerClockMode mode)
        => !SupportsSeconds && mode == TimePickerClockMode.Second ? TimePickerClockMode.Minute : mode;

    private TimeSpan ApplyPrecision(TimeSpan time)
    {
        var normalized = NormalizeToMilliseconds(time);
        return Precision switch
        {
            TimePickerPrecision.Minute => CreateTime(normalized.Hours, normalized.Minutes, 0, 0),
            TimePickerPrecision.Second => CreateTime(normalized.Hours, normalized.Minutes, normalized.Seconds, 0),
            _ => normalized
        };
    }

    private void UpdateLabelState(bool useTransitions)
    {
        var floated = IsKeyboardFocusWithin || IsOpen || HasSelectedTime;
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

        var hasFrozenChild = group.Children.Any(child => child.IsFrozen);
        if (!group.IsFrozen && !hasFrozenChild)
            return group;

        var writableGroup = group.CloneCurrentValue();
        _hintBlock!.RenderTransform = writableGroup;
        return writableGroup;
    }

    private void UpdateOutlinedNotch(bool? floatedOverride = null)
    {
        if (_notchedBorder == null || _hintBlock == null || Variant != TextFieldVariant.Outlined)
            return;

        var floated = floatedOverride ?? IsKeyboardFocusWithin || IsOpen || HasSelectedTime;
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

    private static WpfButton? FindAncestorButton(DependencyObject? current)
    {
        while (current != null)
        {
            if (current is WpfButton button)
                return button;
            current = VisualTreeHelper.GetParent(current);
        }

        return null;
    }

    private static TimeSpan NormalizeToMilliseconds(TimeSpan time)
    {
        var totalMilliseconds = (long)Math.Round(time.TotalMilliseconds, MidpointRounding.AwayFromZero);
        totalMilliseconds %= 24L * 60 * 60 * 1000;
        if (totalMilliseconds < 0)
            totalMilliseconds += 24L * 60 * 60 * 1000;
        return TimeSpan.FromMilliseconds(totalMilliseconds);
    }

    private static TimeSpan CreateTime(int hour, int minute, int second, int millisecond)
        => new(0, hour, minute, second, millisecond);

    private static bool AreClose(double a, double b) => Math.Abs(a - b) < 0.1;

    private static int ToTwelveHour(int hour)
    {
        var result = hour % 12;
        return result == 0 ? 12 : result;
    }

    private static int ConvertTo24Hour(int twelveHour, bool isPm)
    {
        var baseHour = twelveHour % 12;
        return isPm ? baseHour + 12 : baseHour;
    }
}

/// <summary>暴露 <see cref="TimePicker"/> 的展开状态与显示文本给屏幕阅读器 / UI 自动化。</summary>
public class TimePickerAutomationPeer : FrameworkElementAutomationPeer, IExpandCollapseProvider, IValueProvider
{
    public TimePickerAutomationPeer(TimePicker owner) : base(owner) { }

    private TimePicker Control => (TimePicker)Owner;

    public override object GetPattern(PatternInterface patternInterface)
        => patternInterface is PatternInterface.ExpandCollapse or PatternInterface.Value
            ? this
            : base.GetPattern(patternInterface);

    protected override string GetClassNameCore() => nameof(TimePicker);
    protected override AutomationControlType GetAutomationControlTypeCore() => AutomationControlType.ComboBox;

    public ExpandCollapseState ExpandCollapseState => Control.IsOpen ? ExpandCollapseState.Expanded : ExpandCollapseState.Collapsed;
    public void Expand() => Control.IsOpen = true;
    public void Collapse() => Control.IsOpen = false;

    public bool IsReadOnly => true;
    public string Value => Control.DisplayText;
    public void SetValue(string value) => throw new InvalidOperationException("TimePicker 仅支持通过时钟盘或输入框设置时间。");
}

