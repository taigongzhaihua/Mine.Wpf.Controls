using Mine.Wpf.Controls.Controls;

namespace Mine.Wpf.Controls.Gallery.Pages;

public partial class InputsPage
{
    public InputsPage()
    {
        InitializeComponent();

        HoverDragRangeSlider.StartValueLabelMode = RangeSliderValueLabelMode.HoverOrDrag;
        HoverDragRangeSlider.EndValueLabelMode = RangeSliderValueLabelMode.Drag;

        DiscreteRangeSlider.TickFrequency = 10;
        DiscreteRangeSlider.IsSnapToTickEnabled = true;
        DiscreteRangeSlider.ShowStopIndicators = true;
        DiscreteRangeSlider.StartValueLabelMode = RangeSliderValueLabelMode.HoverOrDrag;
        DiscreteRangeSlider.EndValueLabelMode = RangeSliderValueLabelMode.HoverOrDrag;
    }
}
