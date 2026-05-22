using System.Windows;
using System.Windows.Media;
namespace Mine.Wpf.Controls.Attached;
/// <summary>为文本输入控件提供浮动标签、辅助文本、前缀/后缀等附加属性。</summary>
public static class HintAssist
{
    public static readonly DependencyProperty HintProperty =
        DependencyProperty.RegisterAttached("Hint", typeof(string), typeof(HintAssist),
            new PropertyMetadata(null));
    public static readonly DependencyProperty HelperTextProperty =
        DependencyProperty.RegisterAttached("HelperText", typeof(string), typeof(HintAssist),
            new PropertyMetadata(null));
    public static readonly DependencyProperty FloatingScaleProperty =
        DependencyProperty.RegisterAttached("FloatingScale", typeof(double), typeof(HintAssist),
            new PropertyMetadata(0.75));
    public static readonly DependencyProperty HintForegroundProperty =
        DependencyProperty.RegisterAttached("HintForeground", typeof(Brush), typeof(HintAssist),
            new PropertyMetadata(null));
    public static string? GetHint(DependencyObject o)              => (string?)o.GetValue(HintProperty);
    public static void    SetHint(DependencyObject o, string? v)   => o.SetValue(HintProperty, v);
    public static string? GetHelperText(DependencyObject o)            => (string?)o.GetValue(HelperTextProperty);
    public static void    SetHelperText(DependencyObject o, string? v) => o.SetValue(HelperTextProperty, v);
    public static double GetFloatingScale(DependencyObject o)           => (double)o.GetValue(FloatingScaleProperty);
    public static void   SetFloatingScale(DependencyObject o, double v) => o.SetValue(FloatingScaleProperty, v);
    public static Brush? GetHintForeground(DependencyObject o)          => (Brush?)o.GetValue(HintForegroundProperty);
    public static void   SetHintForeground(DependencyObject o, Brush? v) => o.SetValue(HintForegroundProperty, v);
}