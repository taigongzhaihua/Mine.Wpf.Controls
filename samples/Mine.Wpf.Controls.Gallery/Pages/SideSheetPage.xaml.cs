using Mine.Wpf.Controls.Controls;
using System.Windows;

namespace Mine.Wpf.Controls.Gallery.Pages;

public partial class SideSheetPage
{
    public SideSheetPage() => InitializeComponent();

    private void OnCloseSheet(object sender, RoutedEventArgs e)
        => DemoSheet.IsOpen = false;

    private void OnOpenRightModal(object sender, RoutedEventArgs e)
    {
        SheetTitle.Text             = "筛选条件";
        DemoSheet.Variant           = SideSheetVariant.Modal;
        DemoSheet.Placement         = SideSheetPlacement.Right;
        DemoSheet.SheetWidth        = 360;
        DemoSheet.CloseOnScrimClick = true;
        DemoSheet.IsOpen            = true;
    }

    private void OnOpenDetail(object sender, RoutedEventArgs e)
    {
        SheetTitle.Text             = "详情";
        DemoSheet.Variant           = SideSheetVariant.Modal;
        DemoSheet.Placement         = SideSheetPlacement.Right;
        DemoSheet.SheetWidth        = 400;
        DemoSheet.CloseOnScrimClick = true;
        DemoSheet.IsOpen            = true;
    }

    private void OnOpenLeftModal(object sender, RoutedEventArgs e)
    {
        SheetTitle.Text             = "二级导航";
        DemoSheet.Variant           = SideSheetVariant.Modal;
        DemoSheet.Placement         = SideSheetPlacement.Left;
        DemoSheet.SheetWidth        = 320;
        DemoSheet.CloseOnScrimClick = true;
        DemoSheet.IsOpen            = true;
    }

    private void OnOpenStandard(object sender, RoutedEventArgs e)
    {
        SheetTitle.Text             = "辅助信息";
        DemoSheet.Variant           = SideSheetVariant.Standard;
        DemoSheet.Placement         = SideSheetPlacement.Right;
        DemoSheet.SheetWidth        = 360;
        DemoSheet.IsOpen            = true;
    }

    private void OnOpenWidth280(object sender, RoutedEventArgs e)
    {
        SheetTitle.Text             = "宽度 280";
        DemoSheet.Variant           = SideSheetVariant.Modal;
        DemoSheet.Placement         = SideSheetPlacement.Right;
        DemoSheet.SheetWidth        = 280;
        DemoSheet.CloseOnScrimClick = true;
        DemoSheet.IsOpen            = true;
    }

    private void OnOpenWidth480(object sender, RoutedEventArgs e)
    {
        SheetTitle.Text             = "宽度 480";
        DemoSheet.Variant           = SideSheetVariant.Modal;
        DemoSheet.Placement         = SideSheetPlacement.Right;
        DemoSheet.SheetWidth        = 480;
        DemoSheet.CloseOnScrimClick = true;
        DemoSheet.IsOpen            = true;
    }
}
