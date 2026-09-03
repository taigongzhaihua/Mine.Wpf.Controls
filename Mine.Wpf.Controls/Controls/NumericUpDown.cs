using System.Windows;
using System.Windows.Automation.Peers;
using System.Windows.Automation.Provider;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Threading;
using Mine.Wpf.Controls.Primitives;

namespace Mine.Wpf.Controls.Controls;

/// <summary>
/// Material 3 数字输入框，支持最小值、最大值、步进、格式化显示。
/// </summary>
[TemplatePart(Name = PartTextBox,   Type = typeof(System.Windows.Controls.TextBox))]
[TemplatePart(Name = PartIncrement, Type = typeof(ButtonBase))]
[TemplatePart(Name = PartDecrement, Type = typeof(ButtonBase))]
[TemplatePart(Name = PartContainer, Type = typeof(NotchedOutlineBorder))]
[TemplatePart(Name = PartHint, Type = typeof(TextBlock))]
public class NumericUpDown : Control
{
    private const string PartTextBox   = "PART_TextBox";
    private const string PartIncrement = "PART_Increment";
    private const string PartDecrement = "PART_Decrement";
    private const string PartContainer = "Container";
    private const string PartHint = "PART_Hint";

    private System.Windows.Controls.TextBox? _textBox;
    private ButtonBase? _incrementButton;
    private ButtonBase? _decrementButton;
    private NotchedOutlineBorder? _notchedBorder;
    private TextBlock? _hintBlock;
    private bool _isUpdatingText;

    static NumericUpDown()
    {
        DefaultStyleKeyProperty.OverrideMetadata(
            typeof(NumericUpDown), new FrameworkPropertyMetadata(typeof(NumericUpDown)));
    }

    public static readonly DependencyProperty VariantProperty =
        DependencyProperty.Register(nameof(Variant), typeof(TextFieldVariant), typeof(NumericUpDown),
            new PropertyMetadata(TextFieldVariant.Filled, OnVisualPropertyChanged));

    public TextFieldVariant Variant
    {
        get => (TextFieldVariant)GetValue(VariantProperty);
        set => SetValue(VariantProperty, value);
    }

    // ── Value ────────────────────────────────────────────────────────
    public static readonly DependencyProperty ValueProperty =
        DependencyProperty.Register(nameof(Value), typeof(double), typeof(NumericUpDown),
            new FrameworkPropertyMetadata(0.0,
                FrameworkPropertyMetadataOptions.BindsTwoWayByDefault,
                OnValueChanged, CoerceValue));

    public double Value
    {
        get => (double)GetValue(ValueProperty);
        set => SetValue(ValueProperty, value);
    }

    private static object CoerceValue(DependencyObject d, object baseValue)
    {
        var ctrl = (NumericUpDown)d;
        var v = (double)baseValue;
        return Math.Clamp(v, ctrl.Minimum, ctrl.Maximum);
    }

    private static void OnValueChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        var ctrl = (NumericUpDown)d;
        ctrl.UpdateText();
        ctrl.RaiseEvent(new RoutedPropertyChangedEventArgs<double>(
            (double)e.OldValue, (double)e.NewValue, ValueChangedEvent));
    }

    // ── Minimum ──────────────────────────────────────────────────────
    public static readonly DependencyProperty MinimumProperty =
        DependencyProperty.Register(nameof(Minimum), typeof(double), typeof(NumericUpDown),
            new PropertyMetadata(double.MinValue, OnRangeChanged));

    public double Minimum
    {
        get => (double)GetValue(MinimumProperty);
        set => SetValue(MinimumProperty, value);
    }

    // ── Maximum ──────────────────────────────────────────────────────
    public static readonly DependencyProperty MaximumProperty =
        DependencyProperty.Register(nameof(Maximum), typeof(double), typeof(NumericUpDown),
            new PropertyMetadata(double.MaxValue, OnRangeChanged));

    public double Maximum
    {
        get => (double)GetValue(MaximumProperty);
        set => SetValue(MaximumProperty, value);
    }

    private static void OnRangeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        => ((NumericUpDown)d).CoerceValue(ValueProperty);

    // ── SmallChange（单步）────────────────────────────────────────────
    public static readonly DependencyProperty SmallChangeProperty =
        DependencyProperty.Register(nameof(SmallChange), typeof(double), typeof(NumericUpDown),
            new PropertyMetadata(1.0));

    public double SmallChange
    {
        get => (double)GetValue(SmallChangeProperty);
        set => SetValue(SmallChangeProperty, value);
    }

    // ── StringFormat ─────────────────────────────────────────────────
    public static readonly DependencyProperty StringFormatProperty =
        DependencyProperty.Register(nameof(StringFormat), typeof(string), typeof(NumericUpDown),
            new PropertyMetadata("G", (d, _) => ((NumericUpDown)d).UpdateText()));

    /// <summary>数值格式化字符串，默认 "G"。例如 "F2" 显示两位小数。</summary>
    public string StringFormat
    {
        get => (string)GetValue(StringFormatProperty);
        set => SetValue(StringFormatProperty, value);
    }

    // ── Hint ─────────────────────────────────────────────────────────
    public static readonly DependencyProperty HintProperty =
        DependencyProperty.Register(nameof(Hint), typeof(string), typeof(NumericUpDown),
            new PropertyMetadata(null, OnVisualPropertyChanged));

    public string? Hint
    {
        get => (string?)GetValue(HintProperty);
        set => SetValue(HintProperty, value);
    }

    // ── ValueChanged 路由事件 ─────────────────────────────────────────
    public static readonly RoutedEvent ValueChangedEvent =
        EventManager.RegisterRoutedEvent(nameof(ValueChanged), RoutingStrategy.Bubble,
            typeof(RoutedPropertyChangedEventHandler<double>), typeof(NumericUpDown));

    public event RoutedPropertyChangedEventHandler<double> ValueChanged
    {
        add    => AddHandler(ValueChangedEvent, value);
        remove => RemoveHandler(ValueChangedEvent, value);
    }

    // ── Template ─────────────────────────────────────────────────────
    public override void OnApplyTemplate()
    {
        DetachTemplateHandlers();
        base.OnApplyTemplate();

        _textBox = GetTemplateChild(PartTextBox) as System.Windows.Controls.TextBox;
        _incrementButton = GetTemplateChild(PartIncrement) as ButtonBase;
        _decrementButton = GetTemplateChild(PartDecrement) as ButtonBase;
        _notchedBorder = GetTemplateChild(PartContainer) as NotchedOutlineBorder;
        _hintBlock = GetTemplateChild(PartHint) as TextBlock;

        AttachTemplateHandlers();

        UpdateText();
        Dispatcher.BeginInvoke(DispatcherPriority.Loaded, new Action(UpdateOutlinedNotch));
    }

    private static void OnVisualPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        => ((NumericUpDown)d).UpdateOutlinedNotch();

    private void AttachTemplateHandlers()
    {
        if (_textBox != null)
        {
            _textBox.LostFocus += OnTextBoxLostFocus;
            _textBox.KeyDown += OnTextBoxKeyDown;
            _textBox.PreviewMouseWheel += OnMouseWheel;
        }

        if (_incrementButton != null)
            _incrementButton.Click += OnIncrementButtonClick;
        if (_decrementButton != null)
            _decrementButton.Click += OnDecrementButtonClick;
        if (_hintBlock != null)
            _hintBlock.SizeChanged += OnHintSizeChanged;
    }

    private void DetachTemplateHandlers()
    {
        if (_textBox != null)
        {
            _textBox.LostFocus -= OnTextBoxLostFocus;
            _textBox.KeyDown -= OnTextBoxKeyDown;
            _textBox.PreviewMouseWheel -= OnMouseWheel;
        }

        if (_incrementButton != null)
            _incrementButton.Click -= OnIncrementButtonClick;
        if (_decrementButton != null)
            _decrementButton.Click -= OnDecrementButtonClick;
        if (_hintBlock != null)
            _hintBlock.SizeChanged -= OnHintSizeChanged;
    }

    private void OnIncrementButtonClick(object sender, RoutedEventArgs e) => Increment();
    private void OnDecrementButtonClick(object sender, RoutedEventArgs e) => Decrement();
    private void OnHintSizeChanged(object sender, SizeChangedEventArgs e) => UpdateOutlinedNotch();

    // ── 键盘与滚轮 ────────────────────────────────────────────────────
    protected override void OnPreviewMouseWheel(MouseWheelEventArgs e)
    {
        base.OnPreviewMouseWheel(e);
        if (IsFocused || (_textBox?.IsFocused == true))
            OnMouseWheel(this, e);
    }

    private void OnMouseWheel(object sender, MouseWheelEventArgs e)
    {
        if (e.Delta > 0) Increment();
        else             Decrement();
        e.Handled = true;
    }

    private void OnTextBoxKeyDown(object sender, KeyEventArgs e)
    {
        if (e.Key == Key.Up)   { Increment(); e.Handled = true; }
        if (e.Key == Key.Down) { Decrement(); e.Handled = true; }
        if (e.Key == Key.Enter) CommitText();
    }

    private void OnTextBoxLostFocus(object sender, RoutedEventArgs e) => CommitText();

    // ── 内部逻辑 ──────────────────────────────────────────────────────
    private void Increment() => Value = Math.Clamp(Value + SmallChange, Minimum, Maximum);
    private void Decrement() => Value = Math.Clamp(Value - SmallChange, Minimum, Maximum);

    private void CommitText()
    {
        if (_textBox == null) return;
        if (double.TryParse(_textBox.Text, out var v))
            Value = Math.Clamp(v, Minimum, Maximum);
        else
            UpdateText(); // 恢复合法值
    }

    private void UpdateText()
    {
        if (_textBox == null || _isUpdatingText) return;
        _isUpdatingText = true;
        try { _textBox.Text = Value.ToString(StringFormat); }
        finally { _isUpdatingText = false; }
    }

    private void UpdateOutlinedNotch()
    {
        if (_notchedBorder == null)
            return;

        if (Variant != TextFieldVariant.Outlined || _hintBlock == null || string.IsNullOrWhiteSpace(Hint))
        {
            _notchedBorder.NotchWidth = 0;
            return;
        }

        if (_hintBlock.ActualWidth <= 0)
        {
            Dispatcher.BeginInvoke(DispatcherPriority.Loaded, new Action(UpdateOutlinedNotch));
            return;
        }

        const double gap = 4.0;
        _notchedBorder.NotchStart = _hintBlock.Margin.Left - gap;
        _notchedBorder.NotchWidth = Math.Max(0, _hintBlock.ActualWidth * 0.75 + gap * 2);
    }

    protected override AutomationPeer OnCreateAutomationPeer() => new NumericUpDownAutomationPeer(this);
}

/// <summary>暴露 <see cref="NumericUpDown"/> 的数值范围给屏幕阅读器 / UI 自动化。</summary>
public class NumericUpDownAutomationPeer : FrameworkElementAutomationPeer, IRangeValueProvider
{
    public NumericUpDownAutomationPeer(NumericUpDown owner) : base(owner) { }

    private NumericUpDown Control => (NumericUpDown)Owner;

    public override object GetPattern(PatternInterface patternInterface)
        => patternInterface == PatternInterface.RangeValue ? this : base.GetPattern(patternInterface);

    protected override string GetClassNameCore() => nameof(NumericUpDown);
    protected override AutomationControlType GetAutomationControlTypeCore() => AutomationControlType.Spinner;

    public bool IsReadOnly => !Control.IsEnabled;
    public double Maximum => Control.Maximum;
    public double Minimum => Control.Minimum;
    public double LargeChange => Control.SmallChange;
    public double SmallChange => Control.SmallChange;
    public double Value => Control.Value;

    public void SetValue(double value) => Control.Value = Math.Clamp(value, Control.Minimum, Control.Maximum);
}

