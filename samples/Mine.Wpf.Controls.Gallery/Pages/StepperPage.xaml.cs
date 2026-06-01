namespace Mine.Wpf.Controls.Gallery.Pages;

public partial class StepperPage
{
    private const int HorzStepCount = 4;
    private const int VertStepCount = 3;

    public StepperPage() => InitializeComponent();

    private void OnHorzPrev(object sender, System.Windows.RoutedEventArgs e)
    {
        if (HorzStepper.ActiveIndex > 0)
            HorzStepper.ActiveIndex--;
    }

    private void OnHorzNext(object sender, System.Windows.RoutedEventArgs e)
    {
        if (HorzStepper.ActiveIndex < HorzStepCount)
            HorzStepper.ActiveIndex++;
    }

    private void OnVertPrev(object sender, System.Windows.RoutedEventArgs e)
    {
        if (VertStepper.ActiveIndex > 0)
            VertStepper.ActiveIndex--;
    }

    private void OnVertNext(object sender, System.Windows.RoutedEventArgs e)
    {
        if (VertStepper.ActiveIndex < VertStepCount)
            VertStepper.ActiveIndex++;
    }
}
