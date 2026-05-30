using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Media;
using Mine.Wpf.Controls.Controls;

namespace Mine.Wpf.Controls.Attached;

/// <summary>
/// 将 Flyout 附加到任意元素，点击该元素时自动打开/关闭 Flyout。
/// 用法：
///   <mine:Button Content="菜单" mine:FlyoutService.Flyout="{Binding ElementName=MyFlyout}"/>
///   <mine:Flyout x:Name="MyFlyout" .../>
///
/// 或内联写法（将 Flyout 作为附加属性值直接定义）：
///   <mine:Button Content="打开">
///     <mine:FlyoutService.Flyout>
///       <mine:Flyout>...content...</mine:Flyout>
///     </mine:FlyoutService.Flyout>
///   </mine:Button>
/// </summary>
public static class FlyoutService
{
    private static readonly DependencyProperty IsServiceHostedProperty =
        DependencyProperty.RegisterAttached("IsServiceHosted", typeof(bool), typeof(FlyoutService),
            new PropertyMetadata(false));

    // ── Flyout 附加属性 ───────────────────────────────────────────
    public static readonly DependencyProperty FlyoutProperty =
        DependencyProperty.RegisterAttached("Flyout", typeof(Flyout), typeof(FlyoutService),
            new PropertyMetadata(null, OnFlyoutChanged));

    public static Flyout? GetFlyout(DependencyObject o) => (Flyout?)o.GetValue(FlyoutProperty);
    public static void SetFlyout(DependencyObject o, Flyout? v) => o.SetValue(FlyoutProperty, v);

    // ── 属性变更 ─────────────────────────────────────────────────
    private static void OnFlyoutChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is not FrameworkElement element) return;

        if (e.OldValue is Flyout oldFlyout)
        {
            element.RemoveHandler(ButtonBase.ClickEvent, (RoutedEventHandler)OnClick);
            element.MouseLeftButtonUp -= OnMouseLeftButtonUp;
            element.Loaded -= OnElementLoaded;
            // 从父 Panel 移除
            if ((bool)oldFlyout.GetValue(IsServiceHostedProperty) && oldFlyout.Parent is Panel oldPanel)
            {
                oldPanel.Children.Remove(oldFlyout);
                oldFlyout.ClearValue(IsServiceHostedProperty);
            }
        }

        if (e.NewValue is Flyout newFlyout)
        {
            if (element is ButtonBase)
                element.AddHandler(ButtonBase.ClickEvent, (RoutedEventHandler)OnClick, handledEventsToo: true);
            else
                element.MouseLeftButtonUp += OnMouseLeftButtonUp;

            if (element.IsLoaded)
                SetupFlyout(element, newFlyout);
            else
                element.Loaded += OnElementLoaded;
        }
    }

    private static void OnElementLoaded(object sender, RoutedEventArgs e)
    {
        if (sender is not FrameworkElement element) return;
        element.Loaded -= OnElementLoaded;
        if (GetFlyout(element) is { } flyout)
            SetupFlyout(element, flyout);
    }

    private static void SetupFlyout(FrameworkElement element, Flyout flyout)
    {
        // 设置放置目标
        flyout.PlacementTarget = element;

        // 将 Flyout 插入最近的 Panel 祖先，使其进入视觉树（资源/模板才能解析）
        if (flyout.Parent == null)
        {
            var parent = VisualTreeHelper.GetParent(element) as DependencyObject
                         ?? LogicalTreeHelper.GetParent(element) as DependencyObject;
            while (parent != null)
            {
                if (parent is Panel panel)
                {
                    panel.Children.Add(flyout);
                    flyout.SetValue(IsServiceHostedProperty, true);
                    break;
                }
                var next = VisualTreeHelper.GetParent(parent) as DependencyObject
                           ?? LogicalTreeHelper.GetParent(parent) as DependencyObject;
                parent = next;
            }
        }
    }

    private static void OnClick(object sender, RoutedEventArgs e)
    {
        if (sender is FrameworkElement fe && GetFlyout(fe) is { } flyout)
            flyout.Toggle();
    }

    private static void OnMouseLeftButtonUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
    {
        if (sender is FrameworkElement fe && GetFlyout(fe) is { } flyout)
        {
            flyout.Toggle();
            e.Handled = true;
        }
    }
}

