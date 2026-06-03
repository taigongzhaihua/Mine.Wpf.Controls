using Mine.Wpf.Controls.Controls;
using System.Windows;
using System.Windows.Controls;

namespace Mine.Wpf.Controls.Gallery.Pages;

public partial class BottomSheetPage
{
    public BottomSheetPage() => InitializeComponent();

    private void ResetSheet(string title, UIElement content)
    {
        SheetTitle.Text             = title;
        ActionPanel.Visibility      = Visibility.Collapsed;
        SheetContentPanel.Children.Remove(content);
        SheetContentPanel.Children.Add(content);
        ActionPanel.Visibility = Visibility.Collapsed;
    }

    private void OnOpenActionMenu(object sender, RoutedEventArgs e)
    {
        SheetTitle.Text        = "操作菜单";
        ActionPanel.Visibility = Visibility.Visible;
        DemoSheet.Variant          = BottomSheetVariant.Modal;
        DemoSheet.SheetHeight      = double.NaN;
        DemoSheet.ShowDragHandle   = true;
        DemoSheet.EnableDragToClose = true;
        DemoSheet.CloseOnScrimClick = true;
        DemoSheet.IsOpen           = true;
    }

    private void OnOpenForm(object sender, RoutedEventArgs e)
    {
        SheetTitle.Text        = "新建任务";
        ActionPanel.Visibility = Visibility.Collapsed;
        DemoSheet.Variant          = BottomSheetVariant.Modal;
        DemoSheet.SheetHeight      = double.NaN;
        DemoSheet.ShowDragHandle   = true;
        DemoSheet.EnableDragToClose = false;
        DemoSheet.CloseOnScrimClick = false;
        DemoSheet.IsOpen           = true;
    }

    private void OnOpenStandard(object sender, RoutedEventArgs e)
    {
        SheetTitle.Text        = "提示信息";
        ActionPanel.Visibility = Visibility.Visible;
        DemoSheet.Variant          = BottomSheetVariant.Standard;
        DemoSheet.SheetHeight      = double.NaN;
        DemoSheet.ShowDragHandle   = true;
        DemoSheet.EnableDragToClose = true;
        DemoSheet.IsOpen           = true;
    }

    private void OnOpenFixed260(object sender, RoutedEventArgs e)
    {
        SheetTitle.Text        = "固定高度 260";
        ActionPanel.Visibility = Visibility.Visible;
        DemoSheet.Variant          = BottomSheetVariant.Modal;
        DemoSheet.SheetHeight      = 260;
        DemoSheet.ShowDragHandle   = true;
        DemoSheet.EnableDragToClose = true;
        DemoSheet.CloseOnScrimClick = true;
        DemoSheet.IsOpen           = true;
    }

    private void OnOpenFixed480(object sender, RoutedEventArgs e)
    {
        SheetTitle.Text        = "固定高度 480";
        ActionPanel.Visibility = Visibility.Visible;
        DemoSheet.Variant          = BottomSheetVariant.Modal;
        DemoSheet.SheetHeight      = 480;
        DemoSheet.ShowDragHandle   = true;
        DemoSheet.EnableDragToClose = true;
        DemoSheet.CloseOnScrimClick = true;
        DemoSheet.IsOpen           = true;
    }
}
