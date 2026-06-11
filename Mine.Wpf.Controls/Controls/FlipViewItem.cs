using System.Windows;
using System.Windows.Controls;

namespace Mine.Wpf.Controls.Controls
{
    public class FlipViewItem : ContentControl
    {
        static FlipViewItem()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(FlipViewItem), new FrameworkPropertyMetadata(typeof(FlipViewItem)));
        }
    }
}
