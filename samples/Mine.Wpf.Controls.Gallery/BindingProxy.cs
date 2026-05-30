using System.Windows;

namespace Mine.Wpf.Controls.Gallery;

/// <summary>
/// WPF 数据绑定代理：将值桥接到 DataTemplate 等无法直接访问父级的场景。
/// 用法：在 Resources 中定义，DataTemplate 内通过 Source={StaticResource key} 访问。
/// </summary>
public class BindingProxy : Freezable
{
    protected override Freezable CreateInstanceCore() => new BindingProxy();

    public static readonly DependencyProperty DataProperty =
        DependencyProperty.Register(nameof(Data), typeof(object), typeof(BindingProxy),
            new FrameworkPropertyMetadata(null));

    public object? Data
    {
        get => GetValue(DataProperty);
        set => SetValue(DataProperty, value);
    }
}

