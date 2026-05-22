using System.Globalization;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;
namespace Mine.Wpf.Controls.Converters;
/// <summary>将 bool 转换为 Visibility，支持取反和 Hidden 模式。</summary>
[ValueConversion(typeof(bool), typeof(Visibility))]
public sealed class BoolToVisibilityConverter : IValueConverter
{
    public bool Invert   { get; set; }
    public bool UseHidden { get; set; }
    public object Convert(object value, Type t, object p, CultureInfo c)
    {
        bool b = value is true;
        if (Invert) b = !b;
        return b ? Visibility.Visible : (UseHidden ? Visibility.Hidden : Visibility.Collapsed);
    }
    public object ConvertBack(object value, Type t, object p, CultureInfo c)
        => value is Visibility.Visible ? !Invert : Invert;
}
/// <summary>当字符串非空时返回 true。</summary>
[ValueConversion(typeof(string), typeof(bool))]
public sealed class StringIsNotNullOrEmptyConverter : IValueConverter
{
    public object Convert(object value, Type t, object p, CultureInfo c)
        => !string.IsNullOrEmpty(value as string);
    public object ConvertBack(object value, Type t, object p, CultureInfo c)
        => Binding.DoNothing;
}
/// <summary>当字符串非空时返回 Visible，否则 Collapsed，支持取反。</summary>
[ValueConversion(typeof(string), typeof(Visibility))]
public sealed class StringToVisibilityConverter : IValueConverter
{
    public bool Invert { get; set; }
    public object Convert(object value, Type t, object p, CultureInfo c)
    {
        bool hasVal = !string.IsNullOrEmpty(value as string);
        return (Invert ? !hasVal : hasVal) ? Visibility.Visible : Visibility.Collapsed;
    }
    public object ConvertBack(object value, Type t, object p, CultureInfo c) => Binding.DoNothing;
}
/// <summary>将 Color 转换为 SolidColorBrush。</summary>
[ValueConversion(typeof(Color), typeof(SolidColorBrush))]
public sealed class ColorToBrushConverter : IValueConverter
{
    public object Convert(object value, Type t, object p, CultureInfo c)
        => value is Color col ? new SolidColorBrush(col) : Binding.DoNothing;
    public object ConvertBack(object value, Type t, object p, CultureInfo c)
        => value is SolidColorBrush b ? b.Color : Binding.DoNothing;
}
/// <summary>从 SolidColorBrush 中提取 Color（供 RippleDecorator.RippleColor 使用）。</summary>
public sealed class BrushToColorConverter : IValueConverter
{
    public object Convert(object value, Type t, object p, CultureInfo c)
        => value is SolidColorBrush b ? b.Color : Color.FromArgb(40, 255, 255, 255);
    public object ConvertBack(object value, Type t, object p, CultureInfo c)
        => Binding.DoNothing;
}
/// <summary>根据海拔等级返回表面着色叠加层的透明度。</summary>
public sealed class ElevationToOpacityConverter : IValueConverter
{
    private static readonly double[] Opacities = [0, 0.05, 0.08, 0.11, 0.12, 0.14];
    public object Convert(object value, Type t, object p, CultureInfo c)
    {
        int level = value is int i ? Math.Clamp(i, 0, 5) : 0;
        return Opacities[level];
    }
    public object ConvertBack(object value, Type t, object p, CultureInfo c) => Binding.DoNothing;
}
/// <summary>当值为 null 时返回 Collapsed，否则返回 Visible，支持取反。</summary>
public sealed class NullToVisibilityConverter : IValueConverter
{
    public bool Invert { get; set; }
    public object Convert(object value, Type t, object p, CultureInfo c)
    {
        bool isNull = value is null;
        return (Invert ? !isNull : isNull) ? Visibility.Collapsed : Visibility.Visible;
    }
    public object ConvertBack(object value, Type t, object p, CultureInfo c) => Binding.DoNothing;
}
/// <summary>当值非 null 时返回 true，否则返回 false。</summary>
public sealed class NullToBoolConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        => value != null;
    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => throw new NotImplementedException();
}
/// <summary>
/// 使用 CSS border-radius 比例压缩算法限制 CornerRadius，防止超大圆角值导致渲染变形。
/// 对每个角分别计算水平压缩比和垂直压缩比，取两者之小作为该角的最终缩放系数。
///
/// MultiBinding 输入顺序：[0] CornerRadius，[1] ActualWidth，[2] ActualHeight
/// </summary>
public class CornerRadiusFilterConverter : IMultiValueConverter
{
    public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
    {
        if (values.Length < 3
            || values[0] is not CornerRadius cr
            || values[1] is not double width
            || values[2] is not double height
            || width <= 0 || height <= 0)
        {
            return values.Length > 0 && values[0] is CornerRadius r ? r : new CornerRadius(0);
        }
        double tl = cr.TopLeft;
        double tr = cr.TopRight;
        double bl = cr.BottomLeft;
        double br = cr.BottomRight;
        // 水平方向：上边（tl+tr）和下边（bl+br）
        double topH   = tl + tr;
        double botH   = bl + br;
        double hScale = Math.Min(
            topH > width ? width / topH : 1.0,
            botH > width ? width / botH : 1.0);
        // 垂直方向：左边（tl+bl）和右边（tr+br）
        double leftV  = tl + bl;
        double rightV = tr + br;
        double vScale = Math.Min(
            leftV  > height ? height / leftV  : 1.0,
            rightV > height ? height / rightV : 1.0);
        // 取水平与垂直压缩比的最小值
        double scale = Math.Min(hScale, vScale);
        if (scale >= 1.0) return cr; // 无需压缩，直接返回原值
        return new CornerRadius(
            tl * scale,
            tr * scale,
            br * scale,
            bl * scale);
    }
    public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        => throw new NotImplementedException();
}