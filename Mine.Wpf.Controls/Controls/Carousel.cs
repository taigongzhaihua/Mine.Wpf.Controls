using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Threading;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Collections.Specialized;


namespace Mine.Wpf.Controls.Controls;

/// <summary>
/// Material Design 3 Carousel 控件。
/// </summary>
public class Carousel : Selector
{
    private DispatcherTimer? _autoPlayTimer;
    private bool _isUpdatingContainers;

    static Carousel()
    {
        DefaultStyleKeyProperty.OverrideMetadata(typeof(Carousel), new FrameworkPropertyMetadata(typeof(Carousel)));
    }

    public Carousel()
    {
        SetCurrentValue(PreviousCommandProperty, new RelayCommand(_ => Previous()));
        SetCurrentValue(NextCommandProperty, new RelayCommand(_ => Next()));
        Loaded += OnLoaded;
    }

    private void OnLoaded(object sender, RoutedEventArgs e)
    {
        UpdateAutoPlay();
        InitializeContainers();
    }

    public override void OnApplyTemplate()
    {
        base.OnApplyTemplate();

        // Subscribe to Items collection changes
        if (Items is INotifyCollectionChanged incc)
            incc.CollectionChanged += Items_CollectionChanged;

        this.SizeChanged += Carousel_SizeChanged;

        // Initialize on next layout pass to ensure containers are generated
        Dispatcher.BeginInvoke(new Action(() =>
        {
            InitializeContainers();
            UpdateContainersTransforms(animated: false);
        }), System.Windows.Threading.DispatcherPriority.Loaded);
    }

    private void Carousel_SizeChanged(object sender, SizeChangedEventArgs e)
    {
        InitializeContainers();
        UpdateContainersTransforms(animated: true);
    }

    private void Items_CollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        InitializeContainers();
    }

    private void InitializeContainers()
    {
        if (_isUpdatingContainers) return;
        _isUpdatingContainers = true;
        try
        {
            var hostWidth = ActualWidth > 0 ? ActualWidth : 600;
            // Ensure containers have transform groups
            for (var i = 0; i < Items.Count; i++)
            {
                var container = ItemContainerGenerator.ContainerFromIndex(i) as FrameworkElement;
                if (container == null) continue;
                // make items narrower than host so neighbors can peek
                try
                {
                    container.HorizontalAlignment = HorizontalAlignment.Center;
                    container.Width = hostWidth * 0.72;
                }
                catch { }
                if (container.RenderTransform is TransformGroup == false)
                {
                    var tg = new TransformGroup();
                    tg.Children.Add(new ScaleTransform(1, 1));
                    tg.Children.Add(new TranslateTransform(0, 0));
                    container.RenderTransform = tg;
                    container.RenderTransformOrigin = new Point(0.5, 0.5);
                }
            }
        }
        finally
        {
            _isUpdatingContainers = false;
        }
        UpdateContainersTransforms(animated: false);
    }

    private void UpdateContainersTransforms(bool animated = true)
    {
        var count = Items.Count;
        if (count == 0) return;

        for (var i = 0; i < count; i++)
        {
            var container = ItemContainerGenerator.ContainerFromIndex(i) as FrameworkElement;
            if (container == null) continue;

            // compute circular diff
            var diff = i - SelectedIndex;
            if (Math.Abs(diff) > count / 2)
            {
                if (diff > 0) diff -= count; else diff += count;
            }

            var hostWidth = ActualWidth > 0 ? ActualWidth : (container.ActualWidth > 0 ? container.ActualWidth : 400);
            // central overlap: main item centered, neighbors peek and slightly stacked
            var offset = diff * (hostWidth * 0.42);
            var scale = diff == 0 ? 1.0 : (Math.Abs(diff) == 1 ? 0.9 : 0.82);
            var opacity = diff == 0 ? 1.0 : (Math.Abs(diff) == 1 ? 0.95 : 0.6);
            // push closer items slightly outward vertically for stacked look
            double vertical = Math.Abs(diff) == 0 ? 0 : (Math.Abs(diff) == 1 ? 6 : 12);
            var z = 100 - Math.Abs(diff);

            var tg = container.RenderTransform as TransformGroup;
            ScaleTransform st = null!;
            TranslateTransform tt = null!;
            if (tg != null)
            {
                st = tg.Children[0] as ScaleTransform;
                tt = tg.Children[1] as TranslateTransform;
            }
            else
            {
                st = new ScaleTransform(1, 1);
                tt = new TranslateTransform(0, 0);
                var newtg = new TransformGroup();
                newtg.Children.Add(st);
                newtg.Children.Add(tt);
                container.RenderTransform = newtg;
            }

            if (animated)
            {
                var ds = new Duration(TimeSpan.FromMilliseconds(300));
                st.BeginAnimation(ScaleTransform.ScaleXProperty, new DoubleAnimation(scale, ds) { EasingFunction = new CubicEase { EasingMode = EasingMode.EaseOut } });
                st.BeginAnimation(ScaleTransform.ScaleYProperty, new DoubleAnimation(scale, ds) { EasingFunction = new CubicEase { EasingMode = EasingMode.EaseOut } });
                tt.BeginAnimation(TranslateTransform.XProperty, new DoubleAnimation(offset, ds) { EasingFunction = new CubicEase { EasingMode = EasingMode.EaseOut } });
                tt.BeginAnimation(TranslateTransform.YProperty, new DoubleAnimation(vertical, ds) { EasingFunction = new CubicEase { EasingMode = EasingMode.EaseOut } });
                container.BeginAnimation(UIElement.OpacityProperty, new DoubleAnimation(opacity, ds));
                Panel.SetZIndex(container, z);
            }
            else
            {
                st.ScaleX = scale; st.ScaleY = scale;
                tt.X = offset; tt.Y = 0;
                container.Opacity = opacity;
                Panel.SetZIndex(container, z);
            }
        }
    }

    // ── 自动播放 ──────────────────────────────────────────────────

    public static readonly DependencyProperty IsAutoPlayProperty =
        DependencyProperty.Register(nameof(IsAutoPlay), typeof(bool), typeof(Carousel),
            new PropertyMetadata(false, OnIsAutoPlayChanged));

    public bool IsAutoPlay
    {
        get => (bool)GetValue(IsAutoPlayProperty);
        set => SetValue(IsAutoPlayProperty, value);
    }

    private static void OnIsAutoPlayChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        ((Carousel)d).UpdateAutoPlay();
    }

    public static readonly DependencyProperty AutoPlayIntervalProperty =
        DependencyProperty.Register(nameof(AutoPlayInterval), typeof(TimeSpan), typeof(Carousel),
            new PropertyMetadata(TimeSpan.FromSeconds(5)));

    public TimeSpan AutoPlayInterval
    {
        get => (TimeSpan)GetValue(AutoPlayIntervalProperty);
        set => SetValue(AutoPlayIntervalProperty, value);
    }

    public static readonly DependencyProperty PreviousCommandProperty =
        DependencyProperty.Register(nameof(PreviousCommand), typeof(System.Windows.Input.ICommand), typeof(Carousel),
            new PropertyMetadata(null));

    public System.Windows.Input.ICommand? PreviousCommand
    {
        get => (System.Windows.Input.ICommand?)GetValue(PreviousCommandProperty);
        set => SetValue(PreviousCommandProperty, value);
    }

    public static readonly DependencyProperty NextCommandProperty =
        DependencyProperty.Register(nameof(NextCommand), typeof(System.Windows.Input.ICommand), typeof(Carousel),
            new PropertyMetadata(null));

    public System.Windows.Input.ICommand? NextCommand
    {
        get => (System.Windows.Input.ICommand?)GetValue(NextCommandProperty);
        set => SetValue(NextCommandProperty, value);
    }

    private void UpdateAutoPlay()
    {
        if (IsAutoPlay && IsLoaded)
        {
            if (_autoPlayTimer == null)
            {
                _autoPlayTimer = new DispatcherTimer();
                _autoPlayTimer.Tick += (s, e) => Next();
            }
            _autoPlayTimer.Interval = AutoPlayInterval;
            _autoPlayTimer.Start();
        }
        else
        {
            _autoPlayTimer?.Stop();
        }
    }

    // ── 导航 ─────────────────────────────────────────────────────

    public void Next()
    {
        if (Items.Count <= 1) return;
        SelectedIndex = (SelectedIndex + 1) % Items.Count;
        UpdateContainersTransforms();
    }

    public void Previous()
    {
        if (Items.Count <= 1) return;
        SelectedIndex = (SelectedIndex - 1 + Items.Count) % Items.Count;
        UpdateContainersTransforms();
    }

    protected override DependencyObject GetContainerForItemOverride() => new CarouselItem();
    protected override bool IsItemItsOwnContainerOverride(object item) => item is CarouselItem;

    protected override void OnSelectionChanged(SelectionChangedEventArgs e)
    {
        base.OnSelectionChanged(e);
        UpdateContainersTransforms();
    }
}

public class RelayCommand : System.Windows.Input.ICommand
{
    private readonly Action<object?> _execute;
    public RelayCommand(Action<object?> execute) => _execute = execute;
    public bool CanExecute(object? parameter) => true;
    public void Execute(object? parameter) => _execute(parameter);
    public event EventHandler? CanExecuteChanged;
}
