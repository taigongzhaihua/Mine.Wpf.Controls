using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Animation;

namespace Mine.Wpf.Controls.Controls;

/// <summary>Material Design 3 线性进度条。</summary>
[TemplatePart(Name = PartTrack,     Type = typeof(FrameworkElement))]
[TemplatePart(Name = PartIndicator, Type = typeof(FrameworkElement))]
[TemplatePart(Name = PartInd1,      Type = typeof(FrameworkElement))]
[TemplatePart(Name = PartInd2,      Type = typeof(FrameworkElement))]
[TemplateVisualState(Name = "Determinate",   GroupName = "ProgressStates")]
[TemplateVisualState(Name = "Indeterminate", GroupName = "ProgressStates")]
public class ProgressBar : System.Windows.Controls.ProgressBar
{
    public const string PartTrack     = "PART_Track";
    public const string PartIndicator = "PART_Indicator";
    public const string PartInd1      = "PART_Ind1";
    public const string PartInd2      = "PART_Ind2";

    static ProgressBar()
    {
        DefaultStyleKeyProperty.OverrideMetadata(
            typeof(ProgressBar),
            new FrameworkPropertyMetadata(typeof(ProgressBar)));
    }

    private FrameworkElement? _ind1;
    private FrameworkElement? _ind2;
    private Storyboard?       _indStoryboard;

    public override void OnApplyTemplate()
    {
        base.OnApplyTemplate();
        _ind1 = GetTemplateChild(PartInd1) as FrameworkElement;
        _ind2 = GetTemplateChild(PartInd2) as FrameworkElement;

        SizeChanged -= OnSizeChanged;
        SizeChanged += OnSizeChanged;

        UpdateProgressState(false);
    }

    protected override void OnValueChanged(double oldValue, double newValue)
    {
        base.OnValueChanged(oldValue, newValue);
        UpdateProgressState(true);
    }

    protected override void OnPropertyChanged(DependencyPropertyChangedEventArgs e)
    {
        base.OnPropertyChanged(e);
        if (e.Property == IsIndeterminateProperty)
        {
            UpdateProgressState(true);
            if (IsIndeterminate && ActualWidth > 0)
                StartIndeterminateAnimation();
            else
                StopIndeterminateAnimation();
        }
    }

    private void OnSizeChanged(object sender, SizeChangedEventArgs e)
    {
        if (IsIndeterminate && ActualWidth > 0)
            StartIndeterminateAnimation();
    }

    // ── Indeterminate Storyboard ───────────────────────────────────
    private void StartIndeterminateAnimation()
    {
        if (_ind1 == null) return;

        StopIndeterminateAnimation();

        var w = ActualWidth;
        _indStoryboard = new Storyboard { RepeatBehavior = RepeatBehavior.Forever };

        const double dur = 1.4; // 秒

        // 确保 TranslateTransform
        if (_ind1.RenderTransform is not TranslateTransform tt)
        {
            tt = new TranslateTransform();
            _ind1.RenderTransform = tt;
        }

        // X：从 -barW 到 w（整条穿越轨道）
        // 用 EaseInOut 模拟"加速进入，减速退出"
        var xAnim = new DoubleAnimationUsingKeyFrames { RepeatBehavior = RepeatBehavior.Forever };
        xAnim.KeyFrames.Add(new EasingDoubleKeyFrame(-w * 0.3,
            KeyTime.FromTimeSpan(TimeSpan.Zero)));
        xAnim.KeyFrames.Add(new EasingDoubleKeyFrame(w * 1.1,
            KeyTime.FromTimeSpan(TimeSpan.FromSeconds(dur)))
        {
            EasingFunction = new CubicEase { EasingMode = EasingMode.EaseInOut }
        });
        Storyboard.SetTarget(xAnim, _ind1);
        Storyboard.SetTargetProperty(xAnim,
            new PropertyPath("(UIElement.RenderTransform).(TranslateTransform.X)"));
        _indStoryboard.Children.Add(xAnim);

        // Width：生长→收缩（先快速长到 60%，再慢慢缩回 0）
        var wAnim = new DoubleAnimationUsingKeyFrames { RepeatBehavior = RepeatBehavior.Forever };
        wAnim.KeyFrames.Add(new LinearDoubleKeyFrame(0,
            KeyTime.FromTimeSpan(TimeSpan.Zero)));
        wAnim.KeyFrames.Add(new EasingDoubleKeyFrame(w * 0.6,
            KeyTime.FromTimeSpan(TimeSpan.FromSeconds(dur * 0.4)))
        {
            EasingFunction = new CubicEase { EasingMode = EasingMode.EaseOut }
        });
        wAnim.KeyFrames.Add(new EasingDoubleKeyFrame(0,
            KeyTime.FromTimeSpan(TimeSpan.FromSeconds(dur)))
        {
            EasingFunction = new CubicEase { EasingMode = EasingMode.EaseIn }
        });
        Storyboard.SetTarget(wAnim, _ind1);
        Storyboard.SetTargetProperty(wAnim, new PropertyPath(WidthProperty));
        _indStoryboard.Children.Add(wAnim);

        _indStoryboard.Begin(this, isControllable: true);
    }

    private void StopIndeterminateAnimation()
    {
        _indStoryboard?.Stop(this);
        _indStoryboard = null;
    }

    private void UpdateProgressState(bool useTransitions) =>
        VisualStateManager.GoToState(this,
            IsIndeterminate ? "Indeterminate" : "Determinate",
            useTransitions);
}
