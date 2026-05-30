using System.Windows;
using System.Windows.Media;

namespace Mine.Wpf.Controls.Helpers;

/// <summary>
/// 为任意 FrameworkElement 动态设置圆角矩形裁剪区（UIElement.Clip），
/// 解决 Border.ClipToBounds 只裁剪到矩形边界而不裁剪到圆角的问题。
/// 用法：helpers:ClipHelper.CornerRadius="8"
/// </summary>
public static class ClipHelper
{
    public static readonly DependencyProperty CornerRadiusProperty =
        DependencyProperty.RegisterAttached(
            "CornerRadius", typeof(double), typeof(ClipHelper),
            new PropertyMetadata(0d, OnCornerRadiusChanged));

    public static double GetCornerRadius(UIElement element)
        => (double)element.GetValue(CornerRadiusProperty);

    public static void SetCornerRadius(UIElement element, double value)
        => element.SetValue(CornerRadiusProperty, value);

    private static void OnCornerRadiusChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is not FrameworkElement fe) return;

        fe.SizeChanged -= OnSizeChanged;

        if ((double)e.NewValue > 0)
        {
            fe.SizeChanged += OnSizeChanged;
            ApplyClip(fe);
        }
        else
        {
            fe.Clip = null;
        }
    }

    private static void OnSizeChanged(object sender, SizeChangedEventArgs e)
        => ApplyClip((FrameworkElement)sender);

    private static void ApplyClip(FrameworkElement fe)
    {
        var r = GetCornerRadius(fe);
        if (fe.ActualWidth <= 0 || fe.ActualHeight <= 0) return;
        fe.Clip = new RectangleGeometry(
            new Rect(0, 0, fe.ActualWidth, fe.ActualHeight), r, r);
    }
}

