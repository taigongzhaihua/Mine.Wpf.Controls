using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Media;
using System.Windows.Media.Animation;
using Mine.Wpf.Controls.Primitives;

namespace Mine.Wpf.Controls.Controls;

/// <summary>
/// Material Design 3 下拉选择框。
/// 支持 Outlined / Filled 变体、浮动标签、辅助文本，
/// 以及多选模式（IsMultiSelect）。
/// </summary>
[TemplatePart(Name = PartHint,          Type = typeof(TextBlock))]
[TemplatePart(Name = PartContainer,     Type = typeof(NotchedOutlineBorder))]
[TemplatePart(Name = PartSelectionText, Type = typeof(TextBlock))]
[TemplateVisualState(Name = "Normal",       GroupName = "CommonStates")]
[TemplateVisualState(Name = "Focused",      GroupName = "CommonStates")]
[TemplateVisualState(Name = "Disabled",     GroupName = "CommonStates")]
[TemplateVisualState(Name = "LabelNormal",  GroupName = "LabelStates")]
[TemplateVisualState(Name = "LabelFloated", GroupName = "LabelStates")]
public class ComboBox : System.Windows.Controls.ComboBox
{
    private const string PartHint          = "PART_Hint";
    private const string PartContainer     = "Container";
    private const string PartSelectionText = "PART_SelectionText";

    static ComboBox() =>
        DefaultStyleKeyProperty.OverrideMetadata(
            typeof(ComboBox), new FrameworkPropertyMetadata(typeof(ComboBox)));

    // ── Variant ─────────────────��─────────────────────────────────
    public static readonly DependencyProperty VariantProperty =
        DependencyProperty.Register(nameof(Variant), typeof(TextFieldVariant), typeof(ComboBox),
            new PropertyMetadata(TextFieldVariant.Outlined));
    public TextFieldVariant Variant
    {
        get => (TextFieldVariant)GetValue(VariantProperty);
        set => SetValue(VariantProperty, value);
    }

    // ── Hint ──────────────────────────────────────────────────────
    public static readonly DependencyProperty HintProperty =
        DependencyProperty.Register(nameof(Hint), typeof(string), typeof(ComboBox),
            new PropertyMetadata(null));
    public string? Hint
    {
        get => (string?)GetValue(HintProperty);
        set => SetValue(HintProperty, value);
    }

    // ── HelperText ────────────────────────────────────────────────
    public static readonly DependencyProperty HelperTextProperty =
        DependencyProperty.Register(nameof(HelperText), typeof(string), typeof(ComboBox),
            new PropertyMetadata(null));
    public string? HelperText
    {
        get => (string?)GetValue(HelperTextProperty);
        set => SetValue(HelperTextProperty, value);
    }

    // ── HasError ──────────────────────────────────────────────────
    public static readonly DependencyProperty HasErrorProperty =
        DependencyProperty.Register(nameof(HasError), typeof(bool), typeof(ComboBox),
            new PropertyMetadata(false, (d, _) => ((ComboBox)d).UpdateStates(true)));
    public bool HasError
    {
        get => (bool)GetValue(HasErrorProperty);
        set => SetValue(HasErrorProperty, value);
    }

    // ── IsMultiSelect ─────────────────────────────────────────────
    public static readonly DependencyProperty IsMultiSelectProperty =
        DependencyProperty.Register(nameof(IsMultiSelect), typeof(bool), typeof(ComboBox),
            new PropertyMetadata(false));
    public bool IsMultiSelect
    {
        get => (bool)GetValue(IsMultiSelectProperty);
        set => SetValue(IsMultiSelectProperty, value);
    }

    // ── SelectionText (readonly, computed from multi-selection) ───
    private static readonly DependencyPropertyKey SelectionTextPropertyKey =
        DependencyProperty.RegisterReadOnly(nameof(SelectionText), typeof(string), typeof(ComboBox),
            new PropertyMetadata(string.Empty));
    public static readonly DependencyProperty SelectionTextProperty = SelectionTextPropertyKey.DependencyProperty;
    public string SelectionText => (string)GetValue(SelectionTextProperty);

    // ── SelectedObjects (multi-select result collection) ──────────
    private readonly ObservableCollection<object> _multiSelected = new();
    public IReadOnlyList<object> SelectedObjects => _multiSelected;

    // ── Attached: IsMultiChecked (set on each ComboBoxItem) ───────
    internal static readonly DependencyProperty IsMultiCheckedProperty =
        DependencyProperty.RegisterAttached("IsMultiChecked", typeof(bool), typeof(ComboBox),
            new PropertyMetadata(false));
    internal static bool GetIsMultiChecked(DependencyObject o) => (bool)o.GetValue(IsMultiCheckedProperty);
    internal static void SetIsMultiChecked(DependencyObject o, bool v) => o.SetValue(IsMultiCheckedProperty, v);

    // ── Attached: IsMultiSelectMode (set on each ComboBoxItem) ────
    internal static readonly DependencyProperty IsMultiSelectModeProperty =
        DependencyProperty.RegisterAttached("IsMultiSelectMode", typeof(bool), typeof(ComboBox),
            new PropertyMetadata(false));
    internal static bool GetIsMultiSelectMode(DependencyObject o) => (bool)o.GetValue(IsMultiSelectModeProperty);
    internal static void SetIsMultiSelectMode(DependencyObject o, bool v) => o.SetValue(IsMultiSelectModeProperty, v);

    // ── Template parts ────────────────────────────────────────────
    private NotchedOutlineBorder? _notchedBorder;
    private TextBlock?            _hintBlock;
    private ScaleTransform?       _hintScale;
    private TranslateTransform?   _hintTranslate;

    public override void OnApplyTemplate()
    {
        base.OnApplyTemplate();
        _notchedBorder = GetTemplateChild(PartContainer)     as NotchedOutlineBorder;
        _hintBlock     = GetTemplateChild(PartHint)          as TextBlock;

        if (_hintBlock != null)
        {
            _hintScale     = new ScaleTransform(1, 1);
            _hintTranslate = new TranslateTransform(0, 0);
            _hintBlock.RenderTransform = new TransformGroup
            {
                Children = { _hintScale, _hintTranslate }
            };
        }

        if (_hintBlock != null && (SelectedIndex >= 0 || IsDropDownOpen))
            _hintBlock.LayoutUpdated += OnHintFirstLayout;

        UpdateStates(false);
    }

    private void OnHintFirstLayout(object? sender, EventArgs e)
    {
        if (_hintBlock == null) return;
        _hintBlock.LayoutUpdated -= OnHintFirstLayout;
        UpdateLabelState(false);
    }

    // ── Multi-select item container ───────────────────────────────
    protected override void PrepareContainerForItemOverride(DependencyObject element, object item)
    {
        base.PrepareContainerForItemOverride(element, item);
        if (element is not ComboBoxItem container) return;
        SetIsMultiSelectMode(container, IsMultiSelect);
        SetIsMultiChecked(container, _multiSelected.Contains(item));
        if (IsMultiSelect)
            container.PreviewMouseLeftButtonDown += OnMultiItemMouseDown;
    }

    protected override void ClearContainerForItemOverride(DependencyObject element, object item)
    {
        base.ClearContainerForItemOverride(element, item);
        if (element is ComboBoxItem container)
            container.PreviewMouseLeftButtonDown -= OnMultiItemMouseDown;
    }

    private void OnMultiItemMouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
    {
        if (!IsMultiSelect || sender is not ComboBoxItem container) return;
        var item = ItemContainerGenerator.ItemFromContainer(container);
        if (item == DependencyProperty.UnsetValue) return;

        if (_multiSelected.Contains(item))
            _multiSelected.Remove(item);
        else
            _multiSelected.Add(item);

        SetIsMultiChecked(container, _multiSelected.Contains(item));
        RefreshSelectionText();

        // 阻止 ComboBox 基类关闭下拉并更改 SelectedItem
        e.Handled = true;
    }

    private void RefreshSelectionText()
    {
        var text = _multiSelected.Count == 0
            ? string.Empty
            : string.Join(", ", _multiSelected.Select(GetItemDisplayText));
        SetValue(SelectionTextPropertyKey, text);
        UpdateLabelState(false);
    }

    private string GetItemDisplayText(object item)
    {
        if (DisplayMemberPath is { Length: > 0 } path)
            return item.GetType().GetProperty(path)?.GetValue(item)?.ToString() ?? item.ToString() ?? string.Empty;
        return item.ToString() ?? string.Empty;
    }

    // ── Dropdown ──────────────────────────────────────────────────
    protected override void OnDropDownOpened(EventArgs e)
    {
        base.OnDropDownOpened(e);
        UpdateStates(true);
    }

    protected override void OnDropDownClosed(EventArgs e)
    {
        base.OnDropDownClosed(e);
        UpdateStates(true);
    }

    protected override void OnSelectionChanged(SelectionChangedEventArgs e)
    {
        base.OnSelectionChanged(e);
        if (!IsMultiSelect)
            UpdateLabelState(true);
    }

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

    // ��─ States ────────────────────────────────────────────────────
    private void UpdateStates(bool animate)
    {
        if (!IsEnabled)
            VisualStateManager.GoToState(this, "Disabled", animate);
        else if (IsDropDownOpen || IsFocused)
            VisualStateManager.GoToState(this, "Focused", animate);
        else
            VisualStateManager.GoToState(this, "Normal", animate);

        UpdateLabelState(animate);
    }

    private void UpdateLabelState(bool animate)
    {
        var hasValue = IsMultiSelect ? _multiSelected.Count > 0 : SelectedIndex >= 0;
        var floated  = hasValue || IsDropDownOpen;
        AnimateHint(floated, animate);
        UpdateNotch(floated);
    }

    private static readonly Duration FastDur = new(TimeSpan.FromSeconds(0.15));
    private static readonly IEasingFunction EaseOut = new CubicEase { EasingMode = EasingMode.EaseOut };
    private static readonly IEasingFunction EaseIn  = new CubicEase { EasingMode = EasingMode.EaseIn };

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
            var yAnim     = new DoubleAnimation(targetY,     FastDur) { EasingFunction = easing };
            _hintScale.BeginAnimation(ScaleTransform.ScaleXProperty, scaleAnim);
            _hintScale.BeginAnimation(ScaleTransform.ScaleYProperty, scaleAnim);
            _hintTranslate.BeginAnimation(TranslateTransform.YProperty, yAnim);
        }
        else
        {
            _hintScale.ScaleX = targetScale;
            _hintScale.ScaleY = targetScale;
            _hintTranslate.Y  = targetY;
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
