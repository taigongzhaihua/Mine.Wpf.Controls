using System.Windows;
namespace Mine.Wpf.Controls.Attached;
/// <summary>遵循 Material 3 规范的海拔等级（0–5）附加属性。</summary>
public static class ShadowAssist
{
    public static readonly DependencyProperty ElevationProperty =
        DependencyProperty.RegisterAttached("Elevation", typeof(int), typeof(ShadowAssist),
            new FrameworkPropertyMetadata(0, FrameworkPropertyMetadataOptions.AffectsRender));
    public static int  GetElevation(DependencyObject o)       => (int)o.GetValue(ElevationProperty);
    public static void SetElevation(DependencyObject o, int v) => o.SetValue(ElevationProperty, v);
}