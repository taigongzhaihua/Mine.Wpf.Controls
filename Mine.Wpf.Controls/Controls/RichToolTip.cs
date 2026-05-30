using System.Windows;
using System.Windows.Controls;

namespace Mine.Wpf.Controls.Controls;

/// <summary>
/// Material 3 Rich ToolTip：可选标题 + 正文，纯展示，无交互。
/// </summary>
public class RichToolTip : ToolTip
{
    static RichToolTip()
    {
        DefaultStyleKeyProperty.OverrideMetadata(
            typeof(RichToolTip),
            new FrameworkPropertyMetadata(typeof(RichToolTip)));
    }
}
