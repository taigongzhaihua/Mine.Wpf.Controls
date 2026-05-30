using System.Windows;
using System.Windows.Controls;

namespace Mine.Wpf.Controls.Controls;

/// <summary>
/// NavigationView 的导航项，支持图标 + 文本，可在 Expanded / Compact 两种布局之间切换。
/// </summary>
public class NavigationViewItem : ListBoxItem
{
    static NavigationViewItem()
    {
        DefaultStyleKeyProperty.OverrideMetadata(
            typeof(NavigationViewItem),
            new FrameworkPropertyMetadata(typeof(NavigationViewItem)));
    }

    // ── Icon（Material Symbols 字符） ─────────────────────────────────
    public static readonly DependencyProperty IconProperty =
        DependencyProperty.Register(nameof(Icon), typeof(string), typeof(NavigationViewItem),
            new PropertyMetadata(null));

    public string? Icon
    {
        get => (string?)GetValue(IconProperty);
        set => SetValue(IconProperty, value);
    }

    // ── LayoutMode：由父级 NavigationView 设置 ─────────────────────────
    public static readonly DependencyProperty LayoutModeProperty =
        DependencyProperty.Register(nameof(LayoutMode), typeof(NavigationViewItemLayoutMode), typeof(NavigationViewItem),
            new PropertyMetadata(NavigationViewItemLayoutMode.Expanded));

    public NavigationViewItemLayoutMode LayoutMode
    {
        get => (NavigationViewItemLayoutMode)GetValue(LayoutModeProperty);
        set => SetValue(LayoutModeProperty, value);
    }
}

/// <summary>导航项的布局模式。</summary>
public enum NavigationViewItemLayoutMode
{
    /// <summary>展开：图标 + 文字标签并排显示。</summary>
    Expanded,
    /// <summary>紧凑：仅显示图标，隐藏文字标签。</summary>
    Compact,
}

