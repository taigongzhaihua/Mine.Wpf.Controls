using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace Mine.Wpf.Controls.Controls;

public class CarouselItemVisibilityConverter : IMultiValueConverter
{
    // values: SelectedIndex, ItemIndex, ItemsCount
    public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
    {
        if (values == null || values.Length < 3) return Visibility.Collapsed;
        if (!ConverterUtil.TryGetInt(values[0], out var selected) || !ConverterUtil.TryGetInt(values[1], out var index) || !ConverterUtil.TryGetInt(values[2], out var count)) return Visibility.Collapsed;
        var diff = Math.Abs(index - selected);
        if (count > 0 && diff > count / 2) diff = Math.Abs(diff - count);
        return diff <= 2 ? Visibility.Visible : Visibility.Collapsed;
    }

    public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture) => throw new NotSupportedException();
}
