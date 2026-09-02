using System.Collections.Specialized;
using System.Windows;
using System.Windows.Controls;

namespace Mine.Wpf.Controls.Controls;

/// <summary>
/// 设置项分组容器：带 Header 标题，内部承载多个 <see cref="SettingsCard"/>（或任意元素），
/// 整体呈现为一个圆角卡片外观，项之间自动绘制分隔线。
/// </summary>
public class SettingsGroup : ItemsControl
{
    static SettingsGroup()
    {
        DefaultStyleKeyProperty.OverrideMetadata(
            typeof(SettingsGroup),
            new FrameworkPropertyMetadata(typeof(SettingsGroup)));
    }

    // ── Header ───────────────────────────────────────────────────
    public static readonly DependencyProperty HeaderProperty =
        DependencyProperty.Register(nameof(Header), typeof(object), typeof(SettingsGroup),
            new PropertyMetadata(null));
    public object? Header
    {
        get => GetValue(HeaderProperty);
        set => SetValue(HeaderProperty, value);
    }

    // ── CornerRadius ─────────────────────────────────────────────
    public static readonly DependencyProperty CornerRadiusProperty =
        DependencyProperty.Register(nameof(CornerRadius), typeof(CornerRadius), typeof(SettingsGroup),
            new PropertyMetadata(new CornerRadius(12)));
    public CornerRadius CornerRadius
    {
        get => (CornerRadius)GetValue(CornerRadiusProperty);
        set => SetValue(CornerRadiusProperty, value);
    }

    protected override DependencyObject GetContainerForItemOverride()
        => new SettingsGroupItem();

    protected override bool IsItemItsOwnContainerOverride(object item)
        => item is SettingsGroupItem;

    protected override void OnItemsChanged(NotifyCollectionChangedEventArgs e)
    {
        base.OnItemsChanged(e);
        UpdateItemPositions();
    }

    public override void OnApplyTemplate()
    {
        base.OnApplyTemplate();
        UpdateItemPositions();
    }

    /// <summary>标记末项以隐藏其分隔线。</summary>
    private void UpdateItemPositions()
    {
        Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Loaded, () =>
        {
            var count = Items.Count;
            for (var i = 0; i < count; i++)
            {
                if (ItemContainerGenerator.ContainerFromIndex(i) is not SettingsGroupItem item) continue;
                item.IsLast = i == count - 1;
            }
        });
    }
}

/// <summary>SettingsGroup 的项容器，负责底部分隔线的显隐。</summary>
public class SettingsGroupItem : ContentControl
{
    static SettingsGroupItem()
    {
        DefaultStyleKeyProperty.OverrideMetadata(
            typeof(SettingsGroupItem),
            new FrameworkPropertyMetadata(typeof(SettingsGroupItem)));
    }

    public static readonly DependencyProperty IsLastProperty =
        DependencyProperty.Register(nameof(IsLast), typeof(bool), typeof(SettingsGroupItem),
            new PropertyMetadata(false));
    public bool IsLast
    {
        get => (bool)GetValue(IsLastProperty);
        set => SetValue(IsLastProperty, value);
    }
}
