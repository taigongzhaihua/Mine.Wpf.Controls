using System.Windows;

namespace Mine.Wpf.Controls.Attached;
/// <summary>为控件提供前置图标、图标尺寸和后置图标附加属性。</summary>
public static class IconAssist
{
    public static readonly DependencyProperty IconProperty =
        DependencyProperty.RegisterAttached("Icon", typeof(object), typeof(IconAssist),
            new PropertyMetadata(null));
    public static readonly DependencyProperty IconSizeProperty =
        DependencyProperty.RegisterAttached("IconSize", typeof(double), typeof(IconAssist),
            new PropertyMetadata(18.0));
    public static readonly DependencyProperty TrailingIconProperty =
        DependencyProperty.RegisterAttached("TrailingIcon", typeof(object), typeof(IconAssist),
            new PropertyMetadata(null));
    public static object? GetIcon(DependencyObject o)               => o.GetValue(IconProperty);
    public static void    SetIcon(DependencyObject o, object? v)    => o.SetValue(IconProperty, v);
    public static double GetIconSize(DependencyObject o)            => (double)o.GetValue(IconSizeProperty);
    public static void   SetIconSize(DependencyObject o, double v)  => o.SetValue(IconSizeProperty, v);
    public static object? GetTrailingIcon(DependencyObject o)           => o.GetValue(TrailingIconProperty);
    public static void    SetTrailingIcon(DependencyObject o, object? v) => o.SetValue(TrailingIconProperty, v);
}