using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace Mine.Wpf.Controls.Controls;

public class SelectedIndexToVisibilityConverter : IMultiValueConverter
{
    public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
    {
        if (values.Length >= 2 && values[0] is int selectedIndex && values[1] is int itemIndex)
        {
            return selectedIndex == itemIndex ? Visibility.Visible : Visibility.Collapsed;
        }
        return Visibility.Collapsed;
    }

    public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
    {
        throw new NotSupportedException();
    }
}
