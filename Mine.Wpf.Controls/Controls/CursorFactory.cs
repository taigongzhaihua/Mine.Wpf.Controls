using System.IO;
using System.Windows;
using System.Windows.Input;

namespace Mine.Wpf.Controls.Controls;

/// <summary>
/// 从内嵌 .cur 资源加载拖拽手形光标。
/// .cur 文件由 Tools/ConvertCursors.ps1 从 PNG 一次性生成后随源码提交。
/// </summary>
internal static class CursorFactory
{
    private const string Base = "pack://application:,,,/Mine.Wpf.Controls;component/Assets/Cursors/";

    /// <summary>张开手掌（悬停）。</summary>
    public static Cursor? CreateGrab() => Load(Base + "grab.cur");

    /// <summary>握拳（按下拖拽）。</summary>
    public static Cursor? CreateGrabbing() => Load(Base + "grabbing.cur");

    private static Cursor? Load(string packUri)
    {
        try
        {
            var info = Application.GetResourceStream(new Uri(packUri));
            if (info is null) return null;
            var ms = new MemoryStream();
            info.Stream.CopyTo(ms);
            ms.Position = 0;
            return new Cursor(ms);
        }
        catch
        {
            return null;
        }
    }
}
