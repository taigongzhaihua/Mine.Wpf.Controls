using System;
using System.Windows;

namespace Mine.Wpf.Controls.Controls;

internal static class ConverterUtil
{
    public static bool TryGetInt(object? o, out int v)
    {
        v = 0;
        if (o == null) return false;
        if (o == DependencyProperty.UnsetValue) return false;
        if (o is int i) { v = i; return true; }
        if (o is short s) { v = s; return true; }
        if (o is long l) { v = (int)l; return true; }
        if (o is string str && int.TryParse(str, out var r)) { v = r; return true; }
        try { v = Convert.ToInt32(o); return true; } catch { return false; }
    }

    public static bool TryGetDouble(object? o, out double v)
    {
        v = 0;
        if (o == null) return false;
        if (o == DependencyProperty.UnsetValue) return false;
        if (o is double d) { v = d; return true; }
        if (o is float f) { v = f; return true; }
        if (o is int i) { v = i; return true; }
        if (o is string str && double.TryParse(str, out var r)) { v = r; return true; }
        try { v = Convert.ToDouble(o); return true; } catch { return false; }
    }
}
