using System.Windows;
namespace Mine.Wpf.Controls.Attached;
/// <summary>为任意 FrameworkElement 提供形状比例令牌或显式圆角附加属性。</summary>
public static class ShapeAssist
{
    public static readonly DependencyProperty CornerRadiusProperty =
        DependencyProperty.RegisterAttached("CornerRadius", typeof(CornerRadius), typeof(ShapeAssist),
            new FrameworkPropertyMetadata(new CornerRadius(0)));
    public static readonly DependencyProperty ShapeScaleProperty =
        DependencyProperty.RegisterAttached("ShapeScale", typeof(ShapeScale), typeof(ShapeAssist),
            new FrameworkPropertyMetadata(ShapeScale.None, OnShapeScaleChanged));
    public static CornerRadius GetCornerRadius(DependencyObject o)               => (CornerRadius)o.GetValue(CornerRadiusProperty);
    public static void         SetCornerRadius(DependencyObject o, CornerRadius v) => o.SetValue(CornerRadiusProperty, v);
    public static ShapeScale GetShapeScale(DependencyObject o)             => (ShapeScale)o.GetValue(ShapeScaleProperty);
    public static void       SetShapeScale(DependencyObject o, ShapeScale v) => o.SetValue(ShapeScaleProperty, v);
    private static void OnShapeScaleChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (e.NewValue is ShapeScale scale)
            SetCornerRadius(d, ScaleToRadius(scale));
    }
    /// <summary>将形状比例枚举转换为对应的 CornerRadius。</summary>
    public static CornerRadius ScaleToRadius(ShapeScale scale) => scale switch
    {
        ShapeScale.None        => new CornerRadius(0),
        ShapeScale.ExtraSmall  => new CornerRadius(4),
        ShapeScale.Small       => new CornerRadius(8),
        ShapeScale.Medium      => new CornerRadius(12),
        ShapeScale.Large       => new CornerRadius(16),
        ShapeScale.ExtraLarge  => new CornerRadius(28),
        ShapeScale.Full        => new CornerRadius(9999),
        _                      => new CornerRadius(0)
    };
}
/// <summary>Material 3 形状比例枚举。</summary>
public enum ShapeScale { None, ExtraSmall, Small, Medium, Large, ExtraLarge, Full }