using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Interop;
using System.Windows.Shell;
namespace Mine.Wpf.Controls.Controls;
/// <summary>
/// Material 风格窗口，带自定义标题栏，支持 Mica / Acrylic / None 背景特效。
/// 用法：在 XAML 中将 &lt;Window&gt; 替换为 &lt;mine:Window&gt;。
/// </summary>
[TemplatePart(Name = PartTitleBar,    Type = typeof(Grid))]
[TemplatePart(Name = PartCloseButton, Type = typeof(Button))]
[TemplatePart(Name = PartMaxButton,   Type = typeof(Button))]
[TemplatePart(Name = PartMinButton,   Type = typeof(Button))]
public class Window : System.Windows.Window
{
    private const string PartTitleBar    = "PART_TitleBar";
    private const string PartCloseButton = "PART_Close";
    private const string PartMaxButton   = "PART_Maximize";
    private const string PartMinButton   = "PART_Minimize";
    static Window()
    {
        DefaultStyleKeyProperty.OverrideMetadata(
            typeof(Window),
            new FrameworkPropertyMetadata(typeof(Window)));
    }
    public Window()
    {
        var chrome = new WindowChrome
        {
            CaptionHeight         = 40,
            ResizeBorderThickness = new Thickness(6),
            UseAeroCaptionButtons = false,
            GlassFrameThickness   = new Thickness(-1),
        };
        WindowChrome.SetWindowChrome(this, chrome);
        WindowStyle        = WindowStyle.None;
        AllowsTransparency = false;
    }
    // ── 背景特效类型 ──────────────────────────────────────────────
    public static readonly DependencyProperty BackdropTypeProperty =
        DependencyProperty.Register(nameof(BackdropType), typeof(WindowBackdropType), typeof(Window),
            new PropertyMetadata(WindowBackdropType.Mica, OnBackdropTypeChanged));
    public WindowBackdropType BackdropType
    {
        get => (WindowBackdropType)GetValue(BackdropTypeProperty);
        set => SetValue(BackdropTypeProperty, value);
    }
    // ── 标题栏高度 ────────────────────────────────────────────────
    public static readonly DependencyProperty TitleBarHeightProperty =
        DependencyProperty.Register(nameof(TitleBarHeight), typeof(double), typeof(Window),
            new PropertyMetadata(40.0, OnTitleBarHeightChanged));
    public double TitleBarHeight
    {
        get => (double)GetValue(TitleBarHeightProperty);
        set => SetValue(TitleBarHeightProperty, value);
    }
    // ── 是否显示标题栏 ────────────────────────────────────────────
    public static readonly DependencyProperty ShowTitleBarProperty =
        DependencyProperty.Register(nameof(ShowTitleBar), typeof(bool), typeof(Window),
            new PropertyMetadata(true));
    public bool ShowTitleBar
    {
        get => (bool)GetValue(ShowTitleBarProperty);
        set => SetValue(ShowTitleBarProperty, value);
    }
    // ── 标题栏自定义内容 ──────────────────────────────────────────
    public static readonly DependencyProperty TitleBarContentProperty =
        DependencyProperty.Register(nameof(TitleBarContent), typeof(object), typeof(Window),
            new PropertyMetadata(null));
    public object? TitleBarContent
    {
        get => GetValue(TitleBarContentProperty);
        set => SetValue(TitleBarContentProperty, value);
    }
    // ── 圆角 ─────────────────────────────────────────────────────
    public static readonly DependencyProperty CornerRadiusProperty =
        DependencyProperty.Register(nameof(CornerRadius), typeof(CornerRadius), typeof(Window),
            new PropertyMetadata(new CornerRadius(8)));
    public CornerRadius CornerRadius
    {
        get => (CornerRadius)GetValue(CornerRadiusProperty);
        set => SetValue(CornerRadiusProperty, value);
    }
    public override void OnApplyTemplate()
    {
        base.OnApplyTemplate();
        if (GetTemplateChild(PartCloseButton) is System.Windows.Controls.Button close)
            close.Click += (_, _) => Close();
        if (GetTemplateChild(PartMaxButton) is System.Windows.Controls.Button max)
            max.Click += (_, _) => WindowState = WindowState == WindowState.Maximized
                ? WindowState.Normal : WindowState.Maximized;
        if (GetTemplateChild(PartMinButton) is System.Windows.Controls.Button min)
            min.Click += (_, _) => WindowState = WindowState.Minimized;
        Loaded += (_, _) => ApplyBackdrop();
    }
    private static void OnBackdropTypeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is Window w && w.IsLoaded) w.ApplyBackdrop();
    }
    private static void OnTitleBarHeightChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is Window w)
            WindowChrome.GetWindowChrome(w).CaptionHeight = (double)e.NewValue;
    }
    private void ApplyBackdrop()
    {
        var hwnd = new WindowInteropHelper(this).EnsureHandle();
        if (hwnd == IntPtr.Zero) return;
        switch (BackdropType)
        {
            case WindowBackdropType.Mica:
                TrySetMica(hwnd, MicaKind.Base);
                break;
            case WindowBackdropType.MicaAlt:
                TrySetMica(hwnd, MicaKind.Tabbed);
                break;
            case WindowBackdropType.Acrylic:
                TrySetAcrylic(hwnd);
                break;
        }
    }
    private static void TrySetMica(IntPtr hwnd, MicaKind kind)
    {
        try
        {
            int value = kind == MicaKind.Tabbed ? 4 : 2;
            DwmSetWindowAttribute(hwnd, 38, ref value, sizeof(int));
        }
        catch { /* 不支持时静默忽略 */ }
    }
    private static void TrySetAcrylic(IntPtr hwnd)
    {
        try
        {
            int value = 3;
            DwmSetWindowAttribute(hwnd, 38, ref value, sizeof(int));
        }
        catch { /* 不支持时静默忽略 */ }
    }
    [DllImport("dwmapi.dll")]
    private static extern int DwmSetWindowAttribute(
        IntPtr hwnd, int attr, ref int attrValue, int attrSize);
    private enum MicaKind { Base, Tabbed }
}
/// <summary>窗口背景特效类型枚举。</summary>
public enum WindowBackdropType { None, Mica, MicaAlt, Acrylic }
