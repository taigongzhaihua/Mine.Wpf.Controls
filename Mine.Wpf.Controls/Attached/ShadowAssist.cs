using System.Windows;
using System.Windows.Media.Effects;
namespace Mine.Wpf.Controls.Attached;
/// <summary>遵循 Material 3 规范的海拔等级(0-5)附加属性。可应用于任意 FrameworkElement，
/// 设置后会自动查找 Mine.Elevation.{N} 资源并应用为 Effect。</summary>
public static class ShadowAssist
{
    public static readonly DependencyProperty ElevationProperty =
        DependencyProperty.RegisterAttached("Elevation", typeof(int), typeof(ShadowAssist),
            new FrameworkPropertyMetadata(0, FrameworkPropertyMetadataOptions.AffectsRender, OnElevationChanged));
    public static int  GetElevation(DependencyObject o)       => (int)o.GetValue(ElevationProperty);
    public static void SetElevation(DependencyObject o, int v) => o.SetValue(ElevationProperty, v);

    private static void OnElevationChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is not FrameworkElement element) return;

        var level = (int)e.NewValue;
        level = level switch { < 0 => 0, > 5 => 5, _ => level };

        element.Effect = element.TryFindResource($"Mine.Elevation.{level}") as Effect;
    }
}
