using System.Windows;

namespace Mine.Wpf.Controls.Attached;

/// <summary>为 RichToolTip 提供可选标题附加属性。</summary>
public static class ToolTipAssist
{
    public static readonly DependencyProperty TitleProperty =
        DependencyProperty.RegisterAttached("Title", typeof(string), typeof(ToolTipAssist),
            new FrameworkPropertyMetadata(null));

    public static string? GetTitle(DependencyObject o) => (string?)o.GetValue(TitleProperty);
    public static void SetTitle(DependencyObject o, string? v) => o.SetValue(TitleProperty, v);
}
