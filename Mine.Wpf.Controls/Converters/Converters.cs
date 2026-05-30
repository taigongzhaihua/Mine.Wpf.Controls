using System.Globalization;
using System.Windows;
using System.Windows.Controls;
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
        var b = value is true;
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
        var hasVal = !string.IsNullOrEmpty(value as string);
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
        var level = value is int i ? Math.Clamp(i, 0, 5) : 0;
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
        var isNull = value is null;
        return (Invert ? !isNull : isNull) ? Visibility.Collapsed : Visibility.Visible;
    }
    public object ConvertBack(object value, Type t, object p, CultureInfo c) => Binding.DoNothing;
}
/// <summary>当值为 GridView 实例时返回 true，否则 false。</summary>
public sealed class IsGridViewConverter : IValueConverter
{
    public object Convert(object? value, Type t, object? p, CultureInfo c) => value is GridView;
    public object ConvertBack(object? value, Type t, object? p, CultureInfo c) => Binding.DoNothing;
}
/// <summary>
/// MultiBinding 转换器：将 ActualWidth 和 ActualHeight 转换为带圆角的 RectangleGeometry，
/// 用于 UIElement.Clip 绑定，实现真正的圆角裁剪（ClipToBounds 只裁矩形）。
/// 输入顺序：[0] ActualWidth，[1] ActualHeight
/// </summary>
public class RoundedRectClipConverter : IMultiValueConverter
{
    public double RadiusX { get; set; } = 8;
    public double RadiusY { get; set; } = 8;

    /// <summary>
    /// values[0]=ActualWidth, values[1]=ActualHeight, values[2]=CornerRadius(可选)
    /// 当传入第三个值（CornerRadius）时以其 TopLeft 覆盖 RadiusX/Y。
    /// 也可通过 ConverterParameter（double 或字符串）指定圆角半径。
    /// </summary>
    public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
    {
        if (values[0] is double w && values[1] is double h && w > 0 && h > 0)
        {
            double rx = RadiusX, ry = RadiusY;
            if (values.Length > 2 && values[2] is CornerRadius cr)
            {
                rx = cr.TopLeft;
                ry = cr.TopLeft;
            }
            else if (parameter != null &&
                     double.TryParse(parameter.ToString(), System.Globalization.NumberStyles.Any,
                                     CultureInfo.InvariantCulture, out var pr))
            {
                rx = ry = pr;
            }
            return new RectangleGeometry(new Rect(0, 0, w, h), rx, ry);
        }
        return Geometry.Empty;
    }

    public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        => throw new NotImplementedException();
}
/// <summary>当值非 null 时返回 true，否则返回 false。</summary>
public sealed class NullToBoolConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        => value != null;
    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => throw new NotImplementedException();
}
/// <summary>将 double 值除以 2，用于 Avatar 图标 FontSize = Size / 2。</summary>
public sealed class HalfValueConverter : IValueConverter
{
    public static readonly HalfValueConverter Instance = new();
    public object Convert(object value, Type t, object p, CultureInfo c)
        => value is double d ? d / 2.0 : 20.0;
    public object ConvertBack(object value, Type t, object p, CultureInfo c) => Binding.DoNothing;
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
        var tl = cr.TopLeft;
        var tr = cr.TopRight;
        var bl = cr.BottomLeft;
        var br = cr.BottomRight;
        // 水平方向：上边（tl+tr）和下边（bl+br）
        var topH   = tl + tr;
        var botH   = bl + br;
        var hScale = Math.Min(
                              topH > width ? width / topH : 1.0,
                              botH > width ? width / botH : 1.0);
        // 垂直方向：左边（tl+bl）和右边（tr+br）
        var leftV  = tl + bl;
        var rightV = tr + br;
        var vScale = Math.Min(
                              leftV  > height ? height / leftV  : 1.0,
                              rightV > height ? height / rightV : 1.0);
        // 取水平与垂直压缩比的最小值
        var scale = Math.Min(hScale, vScale);
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
