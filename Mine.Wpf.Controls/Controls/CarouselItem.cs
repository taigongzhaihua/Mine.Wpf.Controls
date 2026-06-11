using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;

namespace Mine.Wpf.Controls.Controls;

public class CarouselItem : ContentControl
{
    static CarouselItem()
    {
        DefaultStyleKeyProperty.OverrideMetadata(
            typeof(CarouselItem), new FrameworkPropertyMetadata(typeof(CarouselItem)));
    }
}
