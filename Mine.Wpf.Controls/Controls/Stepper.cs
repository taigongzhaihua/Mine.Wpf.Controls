using System.Windows;
using System.Windows.Controls;

namespace Mine.Wpf.Controls.Controls;

/// <summary>步骤条方向。</summary>
public enum StepperOrientation { Horizontal, Vertical }

/// <summary>单个步骤项的状态。</summary>
public enum StepperItemStatus { Pending, Active, Completed }

/// <summary>
/// Material Design 3 Stepper：显示一组有序步骤的进度条控件。
/// 通过 <see cref="ActiveIndex"/> 控制当前激活的步骤（从 0 开始）。
/// </summary>
public class Stepper : ItemsControl
{
    static Stepper()
    {
        DefaultStyleKeyProperty.OverrideMetadata(
            typeof(Stepper), new FrameworkPropertyMetadata(typeof(Stepper)));
    }

    // ── Orientation ────────────────────────────────────────────────
    public static readonly DependencyProperty OrientationProperty =
        DependencyProperty.Register(nameof(Orientation), typeof(StepperOrientation), typeof(Stepper),
            new PropertyMetadata(StepperOrientation.Horizontal, OnActiveIndexChanged));

    /// <summary>步骤条方向（默认 Horizontal）。</summary>
    public StepperOrientation Orientation
    {
        get => (StepperOrientation)GetValue(OrientationProperty);
        set => SetValue(OrientationProperty, value);
    }

    // ── ActiveIndex ────────────────────────────────────────────────
    public static readonly DependencyProperty ActiveIndexProperty =
        DependencyProperty.Register(nameof(ActiveIndex), typeof(int), typeof(Stepper),
            new PropertyMetadata(0, OnActiveIndexChanged));

    /// <summary>当前激活步骤的索引（0 为第一步）。</summary>
    public int ActiveIndex
    {
        get => (int)GetValue(ActiveIndexProperty);
        set => SetValue(ActiveIndexProperty, value);
    }

    private static void OnActiveIndexChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        => ((Stepper)d).RefreshItemStatuses();

    public override void OnApplyTemplate()
    {
        base.OnApplyTemplate();
        RefreshItemStatuses();
    }

    protected override void OnItemsChanged(System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
    {
        base.OnItemsChanged(e);
        RefreshItemStatuses();
    }

    protected override DependencyObject GetContainerForItemOverride() => new StepperItem();

    protected override bool IsItemItsOwnContainerOverride(object item) => item is StepperItem;

    protected override void PrepareContainerForItemOverride(DependencyObject element, object item)
    {
        base.PrepareContainerForItemOverride(element, item);
        if (element is StepperItem si)
        {
            // 绑定方向
            var orientationBinding = new System.Windows.Data.Binding(nameof(Orientation))
            {
                Source = this,
                Mode = System.Windows.Data.BindingMode.OneWay
            };
            si.SetBinding(StepperItem.OrientationProperty, orientationBinding);
        }
    }

    /// <summary>根据 <see cref="ActiveIndex"/> 刷新所有子项的状态和连接线可见性。</summary>
    private void RefreshItemStatuses()
    {
        var total = Items.Count;
        for (var i = 0; i < total; i++)
        {
            var container = ItemContainerGenerator.ContainerFromIndex(i) as StepperItem
                            ?? Items[i] as StepperItem;
            if (container == null) continue;

            container.StepIndex   = i + 1;
            container.IsLastStep  = i == total - 1;
            container.Status      = i < ActiveIndex  ? StepperItemStatus.Completed
                                  : i == ActiveIndex ? StepperItemStatus.Active
                                                     : StepperItemStatus.Pending;
        }
    }
}

/// <summary>Stepper 中的单个步骤项。</summary>
public class StepperItem : ContentControl
{
    static StepperItem()
    {
        DefaultStyleKeyProperty.OverrideMetadata(
            typeof(StepperItem), new FrameworkPropertyMetadata(typeof(StepperItem)));
    }

    // ── Label ──────────────────────────────────────────────────────
    public static readonly DependencyProperty LabelProperty =
        DependencyProperty.Register(nameof(Label), typeof(string), typeof(StepperItem),
            new PropertyMetadata(string.Empty));

    /// <summary>步骤标题文字。</summary>
    public string Label
    {
        get => (string)GetValue(LabelProperty);
        set => SetValue(LabelProperty, value);
    }

    // ── Description ────────────────────────────────────────────────
    public static readonly DependencyProperty DescriptionProperty =
        DependencyProperty.Register(nameof(Description), typeof(string), typeof(StepperItem),
            new PropertyMetadata(string.Empty));

    /// <summary>步骤说明文字（可选）。</summary>
    public string Description
    {
        get => (string)GetValue(DescriptionProperty);
        set => SetValue(DescriptionProperty, value);
    }

    // ── Status（只读，由父级 Stepper 设置）─────────────────────────
    private static readonly DependencyPropertyKey StatusPropertyKey =
        DependencyProperty.RegisterReadOnly(nameof(Status), typeof(StepperItemStatus), typeof(StepperItem),
            new PropertyMetadata(StepperItemStatus.Pending));

    public static readonly DependencyProperty StatusProperty = StatusPropertyKey.DependencyProperty;

    /// <summary>步骤状态（Pending / Active / Completed），由父级 Stepper 自动设置。</summary>
    public StepperItemStatus Status
    {
        get => (StepperItemStatus)GetValue(StatusProperty);
        internal set => SetValue(StatusPropertyKey, value);
    }

    // ── StepIndex（只读，由父级设置，从 1 开始）────────────────────
    private static readonly DependencyPropertyKey StepIndexPropertyKey =
        DependencyProperty.RegisterReadOnly(nameof(StepIndex), typeof(int), typeof(StepperItem),
            new PropertyMetadata(1));

    public static readonly DependencyProperty StepIndexProperty = StepIndexPropertyKey.DependencyProperty;

    /// <summary>步骤序号（从 1 开始），由父级 Stepper 自动设置。</summary>
    public int StepIndex
    {
        get => (int)GetValue(StepIndexProperty);
        internal set => SetValue(StepIndexPropertyKey, value);
    }

    // ── IsLastStep（只读，用于隐藏末尾连接线）──────────────────────
    private static readonly DependencyPropertyKey IsLastStepPropertyKey =
        DependencyProperty.RegisterReadOnly(nameof(IsLastStep), typeof(bool), typeof(StepperItem),
            new PropertyMetadata(false));

    public static readonly DependencyProperty IsLastStepProperty = IsLastStepPropertyKey.DependencyProperty;

    /// <summary>是否为最后一个步骤（用于隐藏末尾连接线）。</summary>
    public bool IsLastStep
    {
        get => (bool)GetValue(IsLastStepProperty);
        internal set => SetValue(IsLastStepPropertyKey, value);
    }

    // ── Orientation（由父级绑定）────────────────────────────────────
    public static readonly DependencyProperty OrientationProperty =
        DependencyProperty.Register(nameof(Orientation), typeof(StepperOrientation), typeof(StepperItem),
            new PropertyMetadata(StepperOrientation.Horizontal));

    /// <summary>步骤条方向，跟随父级 Stepper。</summary>
    public StepperOrientation Orientation
    {
        get => (StepperOrientation)GetValue(OrientationProperty);
        set => SetValue(OrientationProperty, value);
    }
}
