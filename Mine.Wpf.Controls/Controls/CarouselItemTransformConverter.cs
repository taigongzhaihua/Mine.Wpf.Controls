using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;

namespace Mine.Wpf.Controls.Controls;

/// <summary>
/// Calculate RenderTransform (Scale + Translate) for carousel items based on SelectedIndex.
/// MultiBinding values: 0=SelectedIndex (int), 1=ItemIndex (int), 2=ItemsCount (int), 3=HostWidth (double)
/// </summary>
public class CarouselItemTransformConverter : IMultiValueConverter
{
    public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
    {
        if (values == null || values.Length < 4) return Transform.Identity;

        var ok0 = ConverterUtil.TryGetInt(values[0], out var selected);
        var ok1 = ConverterUtil.TryGetInt(values[1], out var index);
        var ok2 = ConverterUtil.TryGetInt(values[2], out var count);
        var ok3 = ConverterUtil.TryGetDouble(values[3], out var width);
        if (!ok0 || !ok1 || !ok2) return Transform.Identity;
        if (count <= 0) return Transform.Identity;

        var diff = index - selected;
        // normalize to shortest circular distance
        if (Math.Abs(diff) > count / 2)
        {
            if (diff > 0) diff -= count; else diff += count;
        }

        // position factor: use width fraction
        var offset = diff * (width * 0.35); // neighbor offset = 35% of host width

        // scale based on distance
        double abs = Math.Abs(diff);
        var scale = abs == 0 ? 1.0 : (abs == 1 ? 0.86 : 0.75);

        var tg = new TransformGroup();
        tg.Children.Add(new ScaleTransform(scale, scale));
        tg.Children.Add(new TranslateTransform(offset, 0));
        return tg;
    }

    private static bool TryGetInt(object? o, out int v)
    {
        v = 0;
        if (o == null) return false;
        if (o is int i) { v = i; return true; }
        if (o is short s) { v = s; return true; }
        if (o is long l) { v = (int)l; return true; }
        if (o is string str && int.TryParse(str, out var r)) { v = r; return true; }
        if (o is System.Windows.DependencyProperty dp) return false;
        try { v = System.Convert.ToInt32(o); return true; } catch { return false; }
    }

    private static bool TryGetDouble(object? o, out double v)
    {
        v = 0;
        if (o == null) return false;
        if (o is double d) { v = d; return true; }
        if (o is float f) { v = f; return true; }
        if (o is string str && double.TryParse(str, out var r)) { v = r; return true; }
        try { v = System.Convert.ToDouble(o); return true; } catch { return false; }
    }

    public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
    {
        throw new NotSupportedException();
    }
}
