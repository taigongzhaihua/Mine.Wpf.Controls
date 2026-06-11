using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;

namespace Mine.Wpf.Controls.Controls;

/// <summary>
/// Material Design 3 图片查看器控件。
/// 支持缩放、拖拽、旋转等交互操作。
/// </summary>
[TemplatePart(Name = PART_Image, Type = typeof(Image))]
[TemplatePart(Name = PART_ScrollViewer, Type = typeof(ScrollViewer))]
[TemplatePart(Name = PART_Container, Type = typeof(Border))]
public class ImageViewer : Control
{
    private const string PART_Image = "PART_Image";
    private const string PART_ScrollViewer = "PART_ScrollViewer";
    private const string PART_Container = "PART_Container";

    private Image? _image;
    private ScrollViewer? _scrollViewer;
    private Border? _container;
    private Point _lastMousePosition;
    private bool _isDragging;
    private ScaleTransform? _scaleTransform;
    private RotateTransform? _rotateTransform;
    private TranslateTransform? _translateTransform;

    static ImageViewer()
    {
        DefaultStyleKeyProperty.OverrideMetadata(
            typeof(ImageViewer),
            new FrameworkPropertyMetadata(typeof(ImageViewer)));
    }

    public ImageViewer()
    {
        SetCurrentValue(ZoomInCommandProperty, new RelayCommand(_ => ZoomIn()));
        SetCurrentValue(ZoomOutCommandProperty, new RelayCommand(_ => ZoomOut()));
        SetCurrentValue(ResetCommandProperty, new RelayCommand(_ => Reset()));
        SetCurrentValue(RotateLeftCommandProperty, new RelayCommand(_ => RotateLeft()));
        SetCurrentValue(RotateRightCommandProperty, new RelayCommand(_ => RotateRight()));
        SetCurrentValue(FitToWindowCommandProperty, new RelayCommand(_ => FitToWindow()));
    }

    // ══════════════════════════════════════════════════════════════
    // 依赖属性
    // ══════════════════════════════════════════════════════════════

    #region Source - 图片源

    public static readonly DependencyProperty SourceProperty =
        DependencyProperty.Register(nameof(Source), typeof(ImageSource), typeof(ImageViewer),
            new PropertyMetadata(null, OnSourceChanged));

    public ImageSource? Source
    {
        get => (ImageSource?)GetValue(SourceProperty);
        set => SetValue(SourceProperty, value);
    }

    private static void OnSourceChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is ImageViewer viewer)
        {
            viewer.Reset();
        }
    }

    #endregion

    #region Zoom - 缩放级别

    public static readonly DependencyProperty ZoomProperty =
        DependencyProperty.Register(nameof(Zoom), typeof(double), typeof(ImageViewer),
            new PropertyMetadata(1.0, OnZoomChanged, CoerceZoom));

    public double Zoom
    {
        get => (double)GetValue(ZoomProperty);
        set => SetValue(ZoomProperty, value);
    }

    private static void OnZoomChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is ImageViewer viewer)
        {
            viewer.UpdateTransform();
        }
    }

    private static object CoerceZoom(DependencyObject d, object baseValue)
    {
        if (d is ImageViewer viewer && baseValue is double value)
        {
            return Math.Clamp(value, viewer.MinZoom, viewer.MaxZoom);
        }
        return baseValue;
    }

    #endregion

    #region MinZoom / MaxZoom - 缩放范围

    public static readonly DependencyProperty MinZoomProperty =
        DependencyProperty.Register(nameof(MinZoom), typeof(double), typeof(ImageViewer),
            new PropertyMetadata(0.1));

    public double MinZoom
    {
        get => (double)GetValue(MinZoomProperty);
        set => SetValue(MinZoomProperty, value);
    }

    public static readonly DependencyProperty MaxZoomProperty =
        DependencyProperty.Register(nameof(MaxZoom), typeof(double), typeof(ImageViewer),
            new PropertyMetadata(10.0));

    public double MaxZoom
    {
        get => (double)GetValue(MaxZoomProperty);
        set => SetValue(MaxZoomProperty, value);
    }

    #endregion

    #region ZoomStep - 缩放步长

    public static readonly DependencyProperty ZoomStepProperty =
        DependencyProperty.Register(nameof(ZoomStep), typeof(double), typeof(ImageViewer),
            new PropertyMetadata(0.1));

    public double ZoomStep
    {
        get => (double)GetValue(ZoomStepProperty);
        set => SetValue(ZoomStepProperty, value);
    }

    #endregion

    #region Rotation - 旋转角度

    public static readonly DependencyProperty RotationProperty =
        DependencyProperty.Register(nameof(Rotation), typeof(double), typeof(ImageViewer),
            new PropertyMetadata(0.0, OnRotationChanged));

    public double Rotation
    {
        get => (double)GetValue(RotationProperty);
        set => SetValue(RotationProperty, value);
    }

    private static void OnRotationChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is ImageViewer viewer)
        {
            viewer.UpdateTransform();
        }
    }

    #endregion

    #region ShowToolbar - 是否显示工具栏

    public static readonly DependencyProperty ShowToolbarProperty =
        DependencyProperty.Register(nameof(ShowToolbar), typeof(bool), typeof(ImageViewer),
            new PropertyMetadata(true));

    public bool ShowToolbar
    {
        get => (bool)GetValue(ShowToolbarProperty);
        set => SetValue(ShowToolbarProperty, value);
    }

    #endregion

    #region EnableZoom - 启用缩放

    public static readonly DependencyProperty EnableZoomProperty =
        DependencyProperty.Register(nameof(EnableZoom), typeof(bool), typeof(ImageViewer),
            new PropertyMetadata(true));

    public bool EnableZoom
    {
        get => (bool)GetValue(EnableZoomProperty);
        set => SetValue(EnableZoomProperty, value);
    }

    #endregion

    #region EnablePan - 启用拖拽

    public static readonly DependencyProperty EnablePanProperty =
        DependencyProperty.Register(nameof(EnablePan), typeof(bool), typeof(ImageViewer),
            new PropertyMetadata(true));

    public bool EnablePan
    {
        get => (bool)GetValue(EnablePanProperty);
        set => SetValue(EnablePanProperty, value);
    }

    #endregion

    #region Commands - 命令

    public static readonly DependencyProperty ZoomInCommandProperty =
        DependencyProperty.Register(nameof(ZoomInCommand), typeof(ICommand), typeof(ImageViewer));

    public ICommand? ZoomInCommand
    {
        get => (ICommand?)GetValue(ZoomInCommandProperty);
        set => SetValue(ZoomInCommandProperty, value);
    }

    public static readonly DependencyProperty ZoomOutCommandProperty =
        DependencyProperty.Register(nameof(ZoomOutCommand), typeof(ICommand), typeof(ImageViewer));

    public ICommand? ZoomOutCommand
    {
        get => (ICommand?)GetValue(ZoomOutCommandProperty);
        set => SetValue(ZoomOutCommandProperty, value);
    }

    public static readonly DependencyProperty ResetCommandProperty =
        DependencyProperty.Register(nameof(ResetCommand), typeof(ICommand), typeof(ImageViewer));

    public ICommand? ResetCommand
    {
        get => (ICommand?)GetValue(ResetCommandProperty);
        set => SetValue(ResetCommandProperty, value);
    }

    public static readonly DependencyProperty RotateLeftCommandProperty =
        DependencyProperty.Register(nameof(RotateLeftCommand), typeof(ICommand), typeof(ImageViewer));

    public ICommand? RotateLeftCommand
    {
        get => (ICommand?)GetValue(RotateLeftCommandProperty);
        set => SetValue(RotateLeftCommandProperty, value);
    }

    public static readonly DependencyProperty RotateRightCommandProperty =
        DependencyProperty.Register(nameof(RotateRightCommand), typeof(ICommand), typeof(ImageViewer));

    public ICommand? RotateRightCommand
    {
        get => (ICommand?)GetValue(RotateRightCommandProperty);
        set => SetValue(RotateRightCommandProperty, value);
    }

    public static readonly DependencyProperty FitToWindowCommandProperty =
        DependencyProperty.Register(nameof(FitToWindowCommand), typeof(ICommand), typeof(ImageViewer));

    public ICommand? FitToWindowCommand
    {
        get => (ICommand?)GetValue(FitToWindowCommandProperty);
        set => SetValue(FitToWindowCommandProperty, value);
    }

    #endregion

    // ══════════════════════════════════════════════════════════════
    // 模板应用
    // ══════════════════════════════════════════════════════════════

    public override void OnApplyTemplate()
    {
        base.OnApplyTemplate();

        // 清理旧事件
        if (_image != null)
        {
            _image.MouseLeftButtonDown -= OnImageMouseLeftButtonDown;
            _image.MouseLeftButtonUp -= OnImageMouseLeftButtonUp;
            _image.MouseMove -= OnImageMouseMove;
            _image.MouseWheel -= OnImageMouseWheel;
        }

        if (_container != null)
        {
            _container.MouseLeftButtonDown -= OnContainerMouseLeftButtonDown;
        }

        // 获取模板部件
        _image = GetTemplateChild(PART_Image) as Image;
        _scrollViewer = GetTemplateChild(PART_ScrollViewer) as ScrollViewer;
        _container = GetTemplateChild(PART_Container) as Border;

        // 初始化变换
        if (_image != null)
        {
            var transformGroup = new TransformGroup();
            _scaleTransform = new ScaleTransform(1, 1);
            _rotateTransform = new RotateTransform(0);
            _translateTransform = new TranslateTransform(0, 0);

            transformGroup.Children.Add(_scaleTransform);
            transformGroup.Children.Add(_rotateTransform);
            transformGroup.Children.Add(_translateTransform);

            _image.RenderTransform = transformGroup;
            _image.RenderTransformOrigin = new Point(0.5, 0.5);

            // 绑定事件
            _image.MouseLeftButtonDown += OnImageMouseLeftButtonDown;
            _image.MouseLeftButtonUp += OnImageMouseLeftButtonUp;
            _image.MouseMove += OnImageMouseMove;
            _image.MouseWheel += OnImageMouseWheel;
        }

        if (_container != null)
        {
            _container.MouseLeftButtonDown += OnContainerMouseLeftButtonDown;
        }

        UpdateTransform();
    }

    // ══════════════════════════════════════════════════════════════
    // 鼠标交互
    // ══════════════════════════════════════════════════════════════

    private void OnImageMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        if (!EnablePan) return;

        _isDragging = true;
        _lastMousePosition = e.GetPosition(_container);
        _image?.CaptureMouse();

        if (_image != null)
        {
            _image.Cursor = CursorFactory.CreateGrabbing() ?? Cursors.Hand;
        }

        e.Handled = true;
    }

    private void OnImageMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
    {
        if (_isDragging)
        {
            _isDragging = false;
            _image?.ReleaseMouseCapture();

            if (_image != null)
            {
                _image.Cursor = CursorFactory.CreateGrab() ?? Cursors.Hand;
            }
        }
    }

    private void OnImageMouseMove(object sender, MouseEventArgs e)
    {
        if (!_isDragging || _translateTransform == null || _container == null) return;

        var currentPosition = e.GetPosition(_container);
        var delta = currentPosition - _lastMousePosition;

        _translateTransform.X += delta.X;
        _translateTransform.Y += delta.Y;

        _lastMousePosition = currentPosition;
    }

    private void OnImageMouseWheel(object sender, MouseWheelEventArgs e)
    {
        if (!EnableZoom) return;

        var delta = e.Delta > 0 ? ZoomStep : -ZoomStep;
        var newZoom = Zoom + delta;

        // 以鼠标位置为中心缩放
        if (_image != null && _translateTransform != null)
        {
            var mousePos = e.GetPosition(_image);
            var imageCenter = new Point(_image.ActualWidth / 2, _image.ActualHeight / 2);

            var offsetX = mousePos.X - imageCenter.X;
            var offsetY = mousePos.Y - imageCenter.Y;

            var oldZoom = Zoom;
            Zoom = newZoom;

            var zoomFactor = Zoom / oldZoom;
            _translateTransform.X = (_translateTransform.X - offsetX) * zoomFactor + offsetX;
            _translateTransform.Y = (_translateTransform.Y - offsetY) * zoomFactor + offsetY;
        }
        else
        {
            Zoom = newZoom;
        }

        e.Handled = true;
    }

    private void OnContainerMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        // 双击重置
        if (e.ClickCount == 2)
        {
            Reset();
            e.Handled = true;
        }
    }

    // ══════════════════════════════════════════════════════════════
    // 公共方法
    // ══════════════════════════════════════════════════════════════

    /// <summary>放大</summary>
    public void ZoomIn()
    {
        if (EnableZoom)
        {
            Zoom += ZoomStep;
        }
    }

    /// <summary>缩小</summary>
    public void ZoomOut()
    {
        if (EnableZoom)
        {
            Zoom -= ZoomStep;
        }
    }

    /// <summary>重置所有变换</summary>
    public void Reset()
    {
        AnimateTransform(1.0, 0.0, 0.0, 0.0);
    }

    /// <summary>左旋转 90°</summary>
    public void RotateLeft()
    {
        Rotation -= 90;
        if (Rotation <= -360) Rotation += 360;
    }

    /// <summary>右旋转 90°</summary>
    public void RotateRight()
    {
        Rotation += 90;
        if (Rotation >= 360) Rotation -= 360;
    }

    /// <summary>适应窗口</summary>
    public void FitToWindow()
    {
        if (_image?.Source == null || _container == null) return;

        var imageWidth = _image.Source.Width;
        var imageHeight = _image.Source.Height;
        var containerWidth = _container.ActualWidth;
        var containerHeight = _container.ActualHeight;

        if (imageWidth == 0 || imageHeight == 0 || containerWidth == 0 || containerHeight == 0)
            return;

        var scaleX = containerWidth / imageWidth;
        var scaleY = containerHeight / imageHeight;
        var scale = Math.Min(scaleX, scaleY) * 0.9; // 留 10% 边距

        AnimateTransform(scale, Rotation, 0.0, 0.0);
    }

    // ══════════════════════════════════════════════════════════════
    // 私有方法
    // ══════════════════════════════════════════════════════════════

    private void UpdateTransform()
    {
        if (_scaleTransform != null)
        {
            _scaleTransform.ScaleX = Zoom;
            _scaleTransform.ScaleY = Zoom;
        }

        if (_rotateTransform != null)
        {
            _rotateTransform.Angle = Rotation;
        }

        // 更新光标
        if (_image != null && EnablePan)
        {
            _image.Cursor = CursorFactory.CreateGrab() ?? Cursors.Hand;
        }
    }

    private void AnimateTransform(double zoom, double rotation, double translateX, double translateY)
    {
        var duration = new Duration(TimeSpan.FromMilliseconds(300));
        var ease = new CubicEase { EasingMode = EasingMode.EaseOut };

        // 缩放动画
        var scaleAnimation = new DoubleAnimation(zoom, duration) { EasingFunction = ease };

        // 旋转动画
        var rotateAnimation = new DoubleAnimation(rotation, duration) { EasingFunction = ease };

        // 平移动画
        var translateXAnimation = new DoubleAnimation(translateX, duration) { EasingFunction = ease };
        var translateYAnimation = new DoubleAnimation(translateY, duration) { EasingFunction = ease };

        _scaleTransform?.BeginAnimation(ScaleTransform.ScaleXProperty, scaleAnimation);
        _scaleTransform?.BeginAnimation(ScaleTransform.ScaleYProperty, scaleAnimation);
        _rotateTransform?.BeginAnimation(RotateTransform.AngleProperty, rotateAnimation);
        _translateTransform?.BeginAnimation(TranslateTransform.XProperty, translateXAnimation);
        _translateTransform?.BeginAnimation(TranslateTransform.YProperty, translateYAnimation);

        // 更新属性（动画完成后的值）
        Zoom = zoom;
        Rotation = rotation;
        if (_translateTransform != null)
        {
            _translateTransform.X = translateX;
            _translateTransform.Y = translateY;
        }
    }
}
