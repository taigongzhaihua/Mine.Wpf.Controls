using System;
using System.Globalization;
using System.Windows.Data;

namespace Mine.Wpf.Controls.Converters;

/// <summary>
/// 将层级转换为缩进宽度
/// </summary>
public class LevelToIndentConverter : IValueConverter, IMultiValueConverter
{
    public double IndentSize { get; set; } = 20;

    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is int level)
        {
            return level * IndentSize;
        }
        return 0.0;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }

    public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
    {
        if (values.Length > 0 && values[0] is int level)
        {
            return level * IndentSize;
        }
        return 0.0;
    }

    public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
