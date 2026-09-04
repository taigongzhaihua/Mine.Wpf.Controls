using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace Mine.Wpf.Controls.Helpers;

/// <summary>
/// 全屏透明遮罩取色器：跟随鼠标实时预览像素颜色，左键确认、Esc 取消。
/// </summary>
internal static class ScreenColorPicker
{
    public static bool TryPickColor(out Color color)
    {
        var overlay = new PickerWindow();
        var confirmed = overlay.ShowDialog() == true;
        color = overlay.PickedColor;
        return confirmed;
    }

    private sealed class PickerWindow : System.Windows.Window
    {
        private readonly Border _swatch;
        private readonly TextBlock _hexText;
        private readonly Border _preview;
        private readonly Canvas _canvas;
        private IntPtr _screenDc;

        public Color PickedColor { get; private set; }

        public PickerWindow()
        {
            WindowStyle = WindowStyle.None;
            AllowsTransparency = true;
            // Alpha 必须 >0，否则 WPF 分层窗口对全透明区域会直接穿透点击，导致鼠标事件收不到
            Background = new SolidColorBrush(Color.FromArgb(1, 0, 0, 0));
            ShowInTaskbar = false;
            Topmost = true;
            ResizeMode = ResizeMode.NoResize;
            Cursor = Cursors.Cross;
            Left = SystemParameters.VirtualScreenLeft;
            Top = SystemParameters.VirtualScreenTop;
            Width = SystemParameters.VirtualScreenWidth;
            Height = SystemParameters.VirtualScreenHeight;

            _swatch = new Border
            {
                Width = 16,
                Height = 16,
                Background = Brushes.Black,
                BorderBrush = Brushes.White,
                BorderThickness = new Thickness(1.5),
                Margin = new Thickness(0, 0, 6, 0),
            };
            _hexText = new TextBlock
            {
                Foreground = Brushes.White,
                FontSize = 12,
                VerticalAlignment = VerticalAlignment.Center,
            };
            var content = new StackPanel { Orientation = Orientation.Horizontal };
            content.Children.Add(_swatch);
            content.Children.Add(_hexText);

            _preview = new Border
            {
                Background = new SolidColorBrush(Color.FromArgb(200, 0, 0, 0)),
                CornerRadius = new CornerRadius(4),
                Padding = new Thickness(6, 4, 8, 4),
                Child = content,
                IsHitTestVisible = false,
            };

            _canvas = new Canvas { Background = Brushes.Transparent };
            _canvas.Children.Add(_preview);
            Content = _canvas;

            PreviewMouseMove += OnPreviewMouseMove;
            PreviewMouseLeftButtonDown += OnPreviewMouseLeftButtonDown;
            PreviewKeyDown += OnPreviewKeyDown;
            Loaded += (_, _) => { _screenDc = GetDC(IntPtr.Zero); Activate(); };
            Closed += (_, _) => { if (_screenDc != IntPtr.Zero) ReleaseDC(IntPtr.Zero, _screenDc); };
        }

        private void OnPreviewMouseMove(object sender, MouseEventArgs e)
        {
            if (_screenDc == IntPtr.Zero || !GetCursorPos(out var pt)) return;

            var colorRef = GetPixel(_screenDc, pt.X, pt.Y);
            if (colorRef == 0xFFFFFFFF) return; // CLR_INVALID

            PickedColor = Color.FromRgb(
                (byte)(colorRef & 0xFF),
                (byte)((colorRef >> 8) & 0xFF),
                (byte)((colorRef >> 16) & 0xFF));

            _swatch.Background = new SolidColorBrush(PickedColor);
            _hexText.Text = $"#{PickedColor.R:X2}{PickedColor.G:X2}{PickedColor.B:X2}";

            var pos = e.GetPosition(this);
            Canvas.SetLeft(_preview, pos.X + 20);
            Canvas.SetTop(_preview, pos.Y + 20);
        }

        private void OnPreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e) => DialogResult = true;

        private void OnPreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape) DialogResult = false;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct NativePoint
        {
            public int X;
            public int Y;
        }

        [DllImport("user32.dll")]
        private static extern bool GetCursorPos(out NativePoint pt);

        [DllImport("user32.dll")]
        private static extern IntPtr GetDC(IntPtr hWnd);

        [DllImport("user32.dll")]
        private static extern int ReleaseDC(IntPtr hWnd, IntPtr hDc);

        [DllImport("gdi32.dll")]
        private static extern uint GetPixel(IntPtr hdc, int x, int y);
    }
}
