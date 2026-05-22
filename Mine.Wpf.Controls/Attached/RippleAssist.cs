using System.Windows;
using System.Windows.Media;
namespace Mine.Wpf.Controls.Attached;
/// <summary>为任意控件提供水波纹（Ripple）效果控制的附加属性。</summary>
public static class RippleAssist
{
    public static readonly DependencyProperty IsEnabledProperty =
        DependencyProperty.RegisterAttached("IsEnabled", typeof(bool), typeof(RippleAssist),
            new PropertyMetadata(true));
    public static readonly DependencyProperty RippleColorProperty =
        DependencyProperty.RegisterAttached("RippleColor", typeof(Color), typeof(RippleAssist),
            new PropertyMetadata(Colors.White));
    public static readonly DependencyProperty IsCenteredProperty =
        DependencyProperty.RegisterAttached("IsCentered", typeof(bool), typeof(RippleAssist),
            new PropertyMetadata(false));
    public static readonly DependencyProperty ClipToBoundsProperty =
        DependencyProperty.RegisterAttached("ClipToBounds", typeof(bool), typeof(RippleAssist),
            new PropertyMetadata(true));
    public static bool  GetIsEnabled(DependencyObject o)            => (bool)o.GetValue(IsEnabledProperty);
    public static void  SetIsEnabled(DependencyObject o, bool v)    => o.SetValue(IsEnabledProperty, v);
    public static Color GetRippleColor(DependencyObject o)              => (Color)o.GetValue(RippleColorProperty);
    public static void  SetRippleColor(DependencyObject o, Color v)     => o.SetValue(RippleColorProperty, v);
    public static bool  GetIsCentered(DependencyObject o)           => (bool)o.GetValue(IsCenteredProperty);
    public static void  SetIsCentered(DependencyObject o, bool v)   => o.SetValue(IsCenteredProperty, v);
    public static bool  GetClipToBounds(DependencyObject o)         => (bool)o.GetValue(ClipToBoundsProperty);
    public static void  SetClipToBounds(DependencyObject o, bool v) => o.SetValue(ClipToBoundsProperty, v);
}