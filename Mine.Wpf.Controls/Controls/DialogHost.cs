using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;

namespace Mine.Wpf.Controls.Controls;

/// <summary>
/// Material 3 Dialog — 模态对话框。
/// </summary>
    [TemplatePart(Name = PartScrim,      Type = typeof(Border))]
    [TemplatePart(Name = PartScrimImage, Type = typeof(System.Windows.Controls.Image))]
    [TemplatePart(Name = PartContainer,  Type = typeof(Border))]
    [TemplatePart(Name = PartConfirm,    Type = typeof(ButtonBase))]
    [TemplatePart(Name = PartCancel,     Type = typeof(ButtonBase))]
public class DialogHost : ContentControl
{
    private const string PartScrim      = "PART_Scrim";
    private const string PartScrimImage = "PART_ScrimImage";
    private const string PartContainer  = "PART_Container";
    private const string PartConfirm    = "PART_Confirm";
    private const string PartCancel     = "PART_Cancel";

    private Border? _scrim;
    private System.Windows.Controls.Image? _scrimImage;
    private Border? _container;
    private ButtonBase? _confirmBtn;
    private ButtonBase? _cancelBtn;
    private TaskCompletionSource<bool>? _tcs;

    static DialogHost() =>
        DefaultStyleKeyProperty.OverrideMetadata(
            typeof(DialogHost), new FrameworkPropertyMetadata(typeof(DialogHost)));

    // ── Title ─────────────────────────────────────────────────────
    public static readonly DependencyProperty TitleProperty =
        DependencyProperty.Register(nameof(Title), typeof(string), typeof(DialogHost),
            new PropertyMetadata(null));
    public string? Title
    {
        get => (string?)GetValue(TitleProperty);
        set => SetValue(TitleProperty, value);
    }

    // ── ConfirmText ───────────────────────────────────────────────
    public static readonly DependencyProperty ConfirmTextProperty =
        DependencyProperty.Register(nameof(ConfirmText), typeof(string), typeof(DialogHost),
            new PropertyMetadata("确认"));
    public string ConfirmText
    {
        get => (string)GetValue(ConfirmTextProperty);
        set => SetValue(ConfirmTextProperty, value);
    }

    // ── CancelText ────────────────────────────────────────────────
    public static readonly DependencyProperty CancelTextProperty =
        DependencyProperty.Register(nameof(CancelText), typeof(string), typeof(DialogHost),
            new PropertyMetadata("取消"));
    public string CancelText
    {
        get => (string)GetValue(CancelTextProperty);
        set => SetValue(CancelTextProperty, value);
    }

    // ── HasCancel ─────────────────────────────────────────────────
    public static readonly DependencyProperty HasCancelProperty =
        DependencyProperty.Register(nameof(HasCancel), typeof(bool), typeof(DialogHost),
            new PropertyMetadata(true));
    public bool HasCancel
    {
        get => (bool)GetValue(HasCancelProperty);
        set => SetValue(HasCancelProperty, value);
    }

    // ── IsOpen ────────────────────────────────────────────────────
    public static readonly DependencyProperty IsOpenProperty =
        DependencyProperty.Register(nameof(IsOpen), typeof(bool), typeof(DialogHost),
            new PropertyMetadata(false, OnIsOpenChanged));
    public bool IsOpen
    {
        get => (bool)GetValue(IsOpenProperty);
        set => SetValue(IsOpenProperty, value);
    }

    // ── CloseOnScrim ──────────────────────────────────────────────
    public static readonly DependencyProperty CloseOnScrimProperty =
        DependencyProperty.Register(nameof(CloseOnScrim), typeof(bool), typeof(DialogHost),
            new PropertyMetadata(false));
    /// <summary>点击背景遮罩是否关闭（默认 false，符合 M3 规范）。</summary>
    public bool CloseOnScrim
    {
        get => (bool)GetValue(CloseOnScrimProperty);
        set => SetValue(CloseOnScrimProperty, value);
    }

    // ── Template ──────────────────────────────────────────────────
    public override void OnApplyTemplate()
    {
        base.OnApplyTemplate();
        _scrim      = GetTemplateChild(PartScrim)      as Border;
        _scrimImage = GetTemplateChild(PartScrimImage) as System.Windows.Controls.Image;
        _container  = GetTemplateChild(PartContainer)  as Border;
        _confirmBtn = GetTemplateChild(PartConfirm)    as ButtonBase;
        _cancelBtn  = GetTemplateChild(PartCancel)     as ButtonBase;

        if (_confirmBtn != null)
            _confirmBtn.Click += (_, _) => Close(true);
        if (_cancelBtn != null)
            _cancelBtn.Click += (_, _) => Close(false);
        if (_scrim != null)
            _scrim.MouseLeftButtonDown += (_, _) => { if (CloseOnScrim) Close(false); };
    }

    // ── IsOpen ────────────────────────────────────────────────────
    private static void OnIsOpenChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is DialogHost h) h.OnIsOpenChanged((bool)e.NewValue);
    }

    private void OnIsOpenChanged(bool isOpen)
    {
        if (isOpen)
        {
            Visibility = Visibility.Visible;

            // 立即归零，防止在 layout pass 之前闪出对话框内容
            if (_container != null) _container.Opacity = 0;
            if (_scrim     != null) _scrim.Opacity     = 0;

            // 推迟到 Render 优先级（layout pass 完成之后），
            // 此时 DialogHost 已有正确尺寸，截图和动画才能正常工作
            Dispatcher.BeginInvoke(
                System.Windows.Threading.DispatcherPriority.Render,
                new Action(AnimateIn));
        }
        else
        {
            AnimateOut(() => Visibility = Visibility.Collapsed);
        }
    }

    private void AnimateIn()
    {
        if (_scrim == null || _container == null) return;

        // container 在 OnIsOpenChanged 里已归零；再做一次确保 RenderTransform 也就位
        _container.Opacity = 0;
        _container.RenderTransform = new ScaleTransform(0.92, 0.92)
        {
            CenterX = _container.ActualWidth / 2,
            CenterY = _container.ActualHeight / 2,
        };
        _container.RenderTransformOrigin = new System.Windows.Point(0.5, 0.5);

        // 捕获窗口截图用于亚克力模糊（layout 已完成，尺寸正确）
        CaptureScrimBackground();

        _scrim.BeginAnimation(OpacityProperty,
            new DoubleAnimation(0, 1, new Duration(TimeSpan.FromMilliseconds(200))));

        var ease = new CubicEase { EasingMode = EasingMode.EaseOut };
        var dur  = new Duration(TimeSpan.FromMilliseconds(250));

        _container.BeginAnimation(OpacityProperty, new DoubleAnimation(0, 1, dur) { EasingFunction = ease });
        if (_container.RenderTransform is ScaleTransform st)
        {
            st.BeginAnimation(ScaleTransform.ScaleXProperty, new DoubleAnimation(0.92, 1, dur) { EasingFunction = ease });
            st.BeginAnimation(ScaleTransform.ScaleYProperty, new DoubleAnimation(0.92, 1, dur) { EasingFunction = ease });
        }
    }

    private void AnimateOut(Action? onComplete = null)
    {
        if (_scrim == null || _container == null) { onComplete?.Invoke(); return; }

        var dur  = new Duration(TimeSpan.FromMilliseconds(150));
        var ease = new CubicEase { EasingMode = EasingMode.EaseIn };

        _scrim.BeginAnimation(OpacityProperty, new DoubleAnimation(1, 0, dur));

        var fadeOut = new DoubleAnimation(1, 0, dur) { EasingFunction = ease };
        fadeOut.Completed += (_, _) => onComplete?.Invoke();
        _container.BeginAnimation(OpacityProperty, fadeOut);
    }

    /// <summary>捕获窗口当前画面作为亚克力蒙版的模糊底图。</summary>
    private void CaptureScrimBackground()
    {
        if (_scrimImage == null || _scrim == null) return;
        var window = System.Windows.Window.GetWindow(this);
        if (window == null) return;

        // 临时隐藏遮罩，防止截图包含上一次的遮罩残影
        _scrim.Visibility = Visibility.Hidden;

        try
        {
            var dpi = 96.0;
            var src = PresentationSource.FromVisual(window);
            if (src?.CompositionTarget != null)
                dpi = src.CompositionTarget.TransformToDevice.M11 * 96.0;

            var w = (int)(window.ActualWidth  * dpi / 96.0);
            var h = (int)(window.ActualHeight * dpi / 96.0);
            if (w <= 0 || h <= 0) return;

            var rtb = new RenderTargetBitmap(w, h, dpi, dpi, PixelFormats.Pbgra32);
            rtb.Render(window);
            rtb.Freeze();
            _scrimImage.Source = rtb;
        }
        finally
        {
            _scrim.Visibility = Visibility.Visible;
        }
    }

    private void Close(bool result)
    {
        IsOpen = false;
        _tcs?.TrySetResult(result);
        _tcs = null;
    }

    // ── Public API ────────────────────────────────────────────────
    /// <summary>
    /// 异步显示对话框，返回用户是否点击确认。
    /// </summary>
    public Task<bool> ShowAsync(string? title = null, object? content = null,
                                string confirmText = "确认", string cancelText = "取消",
                                bool hasCancel = true)
    {
        _tcs = new TaskCompletionSource<bool>();
        if (title   != null) Title       = title;
        if (content != null) Content     = content;
        ConfirmText = confirmText;
        CancelText  = cancelText;
        HasCancel   = hasCancel;
        IsOpen      = true;
        return _tcs.Task;
    }
}

// ══════════════════════════════════════════════════════════════════
/// <summary>
/// 全局 DialogHost 服务。自动在当前活动 Window 上注入浮层并显示对话框，无需手动注册。
/// </summary>
public static class DialogService
{
    private static DialogHost? _pinned;
    public static void Register(DialogHost host) => _pinned = host;

    public static Task<bool> ShowAsync(string? title = null, object? content = null,
                                       string confirmText = "确认", string cancelText = "取消",
                                       bool hasCancel = true)
    {
        var window = Helpers.WindowOverlay.GetActiveWindow()
                     ?? throw new InvalidOperationException("DialogService: 找不到活动 Window。");

        // 必须在 UI 线程上操作
        return window.Dispatcher.InvokeAsync(() =>
        {
            var host = _pinned ?? GetOrCreate(window);
            return host.ShowAsync(title, content, confirmText, cancelText, hasCancel);
        }).Result;
    }

    private static DialogHost GetOrCreate(System.Windows.Window window)
    {
        var overlay = Helpers.WindowOverlay.GetOrCreate(window);
        var existing = overlay.Children.OfType<DialogHost>().FirstOrDefault();
        if (existing != null) return existing;

        var host = new DialogHost
        {
            HorizontalAlignment = System.Windows.HorizontalAlignment.Stretch,
            VerticalAlignment   = System.Windows.VerticalAlignment.Stretch,
        };


        System.Windows.Controls.Panel.SetZIndex(host, 9998);
        overlay.Children.Add(host);
        return host;
    }
}

