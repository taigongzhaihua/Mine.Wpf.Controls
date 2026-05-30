using System.Windows;
using System.Windows.Controls;

namespace Mine.Wpf.Controls.Helpers;

/// <summary>
/// 在任意 Window 上动态注入透明 Overlay Grid，用于承载 Snackbar、DialogHost 等浮层控件。
/// </summary>
internal static class WindowOverlay
{
    private const string OverlayTag = "__Mine.WindowOverlay__";

    public static Grid GetOrCreate(System.Windows.Window window)
    {
        if (window.Content is Grid g && g.Tag as string == OverlayTag)
            return g;

        var original = window.Content as UIElement;
        window.Content = null;

        var overlay = new Grid { Tag = OverlayTag };
        if (original != null)
            overlay.Children.Add(original);

        window.Content = overlay;
        return overlay;
    }

    public static System.Windows.Window? GetActiveWindow()
    {
        if (Application.Current == null) return null;
        return Application.Current.Windows
                   .OfType<System.Windows.Window>()
                   .FirstOrDefault(w => w.IsActive)
               ?? Application.Current.MainWindow;
    }
}


