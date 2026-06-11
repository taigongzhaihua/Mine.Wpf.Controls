using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace Mine.Wpf.Controls.Controls;

public class CarouselItemOpacityConverter : IMultiValueConverter
{
    // values: SelectedIndex, ItemIndex, ItemsCount
    public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
    {
        if (values == null || values.Length < 3) return 0.0;
        if (!ConverterUtil.TryGetInt(values[0], out var selected) || !ConverterUtil.TryGetInt(values[1], out var index) || !ConverterUtil.TryGetInt(values[2], out var count)) return 0.0;
        var diff = Math.Abs(index - selected);
        return diff switch
        {
            0 => 1.0,
            1 => 0.9,
            _ => 0.5
        };
    }

static partial class CarouselItemHelpers
{
    public static bool TryGetInt(object? o, out int v)
    {
        v = 0;
        switch (o)
        {
            case null:
                return false;
            case int i:
                v = i; return true;
            case short s:
                v = s; return true;
            case long l:
                v = (int)l; return true;
            case string str when int.TryParse(str, out var r):
                v = r; return true;
            default:
                try { v = System.Convert.ToInt32(o); return true; } catch { return false; }
        }
    }
}

    public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture) => throw new NotSupportedException();
}

public class CarouselItemZIndexConverter : IMultiValueConverter
{
    public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
    {
        if (values == null || values.Length < 2) return 0;
        if (!ConverterUtil.TryGetInt(values[0], out var selected) || !ConverterUtil.TryGetInt(values[1], out var index)) return 0;
        var diff = Math.Abs(selected - index);
        return 100 - diff;
    }

    public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture) => throw new NotSupportedException();
}
