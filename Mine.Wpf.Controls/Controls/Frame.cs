using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;

namespace Mine.Wpf.Controls.Controls;

/// <summary>
/// Material 风格导航帧，管理 <see cref="Page"/> 的导航栈与过渡动画。
/// 支持 Back / Forward 历史、可自定义过渡类型。
/// </summary>
[TemplatePart(Name = PartOldContent, Type = typeof(ContentPresenter))]
[TemplatePart(Name = PartNewContent, Type = typeof(ContentPresenter))]
public class Frame : ContentControl
{
    private const string PartOldContent = "PART_OldContent";
    private const string PartNewContent = "PART_NewContent";

    private ContentPresenter? _old;
    private ContentPresenter? _new;
    private bool _animating;

    // Pending navigation when template not yet applied
    private (object? OldContent, object NewContent, PageTransition Transition)? _pending;

    private readonly Stack<object> _backStack    = new();
    private readonly Stack<object> _forwardStack = new();
    private object? _current;

    static Frame() =>
        DefaultStyleKeyProperty.OverrideMetadata(
            typeof(Frame), new FrameworkPropertyMetadata(typeof(Frame)));

    // ── DefaultTransition ─────────────────────────────────────────
    public static readonly DependencyProperty DefaultTransitionProperty =
        DependencyProperty.Register(nameof(DefaultTransition), typeof(PageTransition), typeof(Frame),
            new PropertyMetadata(PageTransition.FadeThrough));
    public PageTransition DefaultTransition
    {
        get => (PageTransition)GetValue(DefaultTransitionProperty);
        set => SetValue(DefaultTransitionProperty, value);
    }

    // ── TransitionDuration ────────────────────────────────────────
    public static readonly DependencyProperty TransitionDurationProperty =
        DependencyProperty.Register(nameof(TransitionDuration), typeof(Duration), typeof(Frame),
            new PropertyMetadata(new Duration(TimeSpan.FromMilliseconds(240))));
    public Duration TransitionDuration
    {
        get => (Duration)GetValue(TransitionDurationProperty);
        set => SetValue(TransitionDurationProperty, value);
    }

    // ── CanGoBack / CanGoForward (readonly) ───────────────────────
    private static readonly DependencyPropertyKey CanGoBackKey =
        DependencyProperty.RegisterReadOnly(nameof(CanGoBack), typeof(bool), typeof(Frame),
            new PropertyMetadata(false));
    public static readonly DependencyProperty CanGoBackProperty = CanGoBackKey.DependencyProperty;
    public bool CanGoBack => (bool)GetValue(CanGoBackProperty);

    private static readonly DependencyPropertyKey CanGoForwardKey =
        DependencyProperty.RegisterReadOnly(nameof(CanGoForward), typeof(bool), typeof(Frame),
            new PropertyMetadata(false));
    public static readonly DependencyProperty CanGoForwardProperty = CanGoForwardKey.DependencyProperty;
    public bool CanGoForward => (bool)GetValue(CanGoForwardProperty);

    // ── CurrentPage (readonly) ───��────────────────────────────────
    private static readonly DependencyPropertyKey CurrentPageKey =
        DependencyProperty.RegisterReadOnly(nameof(CurrentPage), typeof(object), typeof(Frame),
            new PropertyMetadata(null));
    public static readonly DependencyProperty CurrentPageProperty = CurrentPageKey.DependencyProperty;
    public object? CurrentPage => GetValue(CurrentPageProperty);

    // ── Events ────────────────────────────────────────────────────
    public event EventHandler<PageNavigatedEventArgs>? Navigated;

    // ── Template ──────────────────────────────────────────────────
    public override void OnApplyTemplate()
    {
        base.OnApplyTemplate();
        _old = GetTemplateChild(PartOldContent) as ContentPresenter;
        _new = GetTemplateChild(PartNewContent) as ContentPresenter;

        // Apply any navigation that was requested before template was ready
        if (_pending.HasValue)
        {
            var p = _pending.Value;
            _pending = null;
            DoNavigate(p.OldContent, p.NewContent, p.Transition);
        }
    }

    // ── Public Navigation API ─────────────────────────────────────
    /// <summary>导航到新内容，清空 Forward 栈。</summary>
    public void Navigate(object content, PageTransition? transition = null)
    {
        if (_animating) return;
        if (_current != null)
            _backStack.Push(_current);
        _forwardStack.Clear();
        DoNavigate(_current, content, transition ?? DefaultTransition);
    }

    /// <summary>返回上一页。</summary>
    public void GoBack(PageTransition? transition = null)
    {
        if (!CanGoBack || _animating) return;
        if (_current != null) _forwardStack.Push(_current);
        var prev = _backStack.Pop();
        DoNavigate(_current, prev, transition ?? PageTransition.SlideBack);
    }

    /// <summary>前进到下一页（GoBack 后可用）。</summary>
    public void GoForward(PageTransition? transition = null)
    {
        if (!CanGoForward || _animating) return;
        if (_current != null) _backStack.Push(_current);
        var next = _forwardStack.Pop();
        DoNavigate(_current, next, transition ?? PageTransition.SlideForward);
    }

    /// <summary>清空导航历史并导航到初始页。</summary>
    public void NavigateRoot(object content, PageTransition? transition = null)
    {
        _backStack.Clear();
        _forwardStack.Clear();
        DoNavigate(_current, content, transition ?? PageTransition.Fade);
    }

    // ── Core ─��────────────────────────────────────────────────────
    private void DoNavigate(object? oldContent, object newContent, PageTransition transition)
    {
        if (oldContent is Page oldPage) oldPage.InternalOnNavigatingFrom();

        _current = newContent;
        SetValue(CurrentPageKey, newContent);
        UpdateCanNavigation();

        // Template not ready yet — defer
        if (_old == null || _new == null)
        {
            _pending = (oldContent, newContent, transition);
            return;
        }

        if (transition == PageTransition.None)
        {
            DirectSwap(newContent);
            return;
        }

        _animating = true;

        // Stop any in-progress animations cleanly
        StopAnimations();

        _old.Content = oldContent;
        _new.Content = newContent;
        _old.Opacity = oldContent == null ? 0 : 1;
        _new.Opacity = 0;
        _old.RenderTransform = null;
        _new.RenderTransform = null;

        switch (transition)
        {
            case PageTransition.Fade:
            case PageTransition.FadeThrough:
                AnimateCrossFade();
                break;
            case PageTransition.SlideForward:
                AnimateSlide(fromRight: true);
                break;
            case PageTransition.SlideBack:
                AnimateSlide(fromRight: false);
                break;
            case PageTransition.SlideUp:
                AnimateSlideUp();
                break;
        }

        var dur = TransitionDuration.HasTimeSpan
            ? TransitionDuration.TimeSpan
            : TimeSpan.FromMilliseconds(240);

        var timer = new System.Windows.Threading.DispatcherTimer { Interval = dur };
        timer.Tick += (_, _) =>
        {
            timer.Stop();
            // Clear completed animations so property is clean
            StopAnimations();
            _old.Content = null;
            _old.Opacity = 0;
            _new.Opacity = 1;
            _animating = false;
            OnNavigationComplete(newContent);
        };
        timer.Start();
    }

    private void DirectSwap(object newContent)
    {
        StopAnimations();
        _old!.Content = null;
        _old.Opacity = 0;
        _new!.Content = newContent;
        _new.Opacity = 1;
        OnNavigationComplete(newContent);
    }

    private void StopAnimations()
    {
        if (_old != null)
        {
            _old.BeginAnimation(OpacityProperty, null);
            if (_old.RenderTransform is TranslateTransform ot)
            {
                ot.BeginAnimation(TranslateTransform.XProperty, null);
                ot.BeginAnimation(TranslateTransform.YProperty, null);
            }
        }
        if (_new != null)
        {
            _new.BeginAnimation(OpacityProperty, null);
            if (_new.RenderTransform is TranslateTransform nt)
            {
                nt.BeginAnimation(TranslateTransform.XProperty, null);
                nt.BeginAnimation(TranslateTransform.YProperty, null);
            }
        }
    }

    private void OnNavigationComplete(object newContent)
    {
        if (newContent is Page p) p.InternalOnNavigatedTo(this);
        Navigated?.Invoke(this, new PageNavigatedEventArgs(newContent));
    }

    private void UpdateCanNavigation()
    {
        SetValue(CanGoBackKey,    _backStack.Count > 0);
        SetValue(CanGoForwardKey, _forwardStack.Count > 0);
    }

    // ── Animation helpers ─────────────────────────────────────���───
    private Duration Dur => TransitionDuration;
    private static readonly IEasingFunction Ease = new CubicEase { EasingMode = EasingMode.EaseInOut };

    /// <summary>
    /// 无间隙交叉淡入淡出：旧页 1→0，新页 0→1，同步进行，无黑屏。
    /// </summary>
    private void AnimateCrossFade()
    {
        if (_old!.Content != null)
            _old.BeginAnimation(OpacityProperty,
                new DoubleAnimation(1, 0, Dur) { EasingFunction = Ease });

        _new!.BeginAnimation(OpacityProperty,
            new DoubleAnimation(0, 1, Dur) { EasingFunction = Ease });
    }

    private void AnimateSlide(bool fromRight)
    {
        var width = ActualWidth > 0 ? ActualWidth : 600;
        var startX = fromRight ? width * 0.25 : -width * 0.25;

        var oldT = new TranslateTransform();
        var newT = new TranslateTransform();
        _old!.RenderTransform = oldT;
        _new!.RenderTransform = newT;

        var exitX = fromRight ? -width * 0.1 : width * 0.1;
        oldT.BeginAnimation(TranslateTransform.XProperty,
            new DoubleAnimation(0, exitX, Dur) { EasingFunction = Ease });
        newT.BeginAnimation(TranslateTransform.XProperty,
            new DoubleAnimation(startX, 0, Dur) { EasingFunction = Ease });

        if (_old.Content != null)
            _old.BeginAnimation(OpacityProperty,
                new DoubleAnimation(1, 0, Dur) { EasingFunction = Ease });
        _new.BeginAnimation(OpacityProperty,
            new DoubleAnimation(0, 1, Dur) { EasingFunction = Ease });
    }

    private void AnimateSlideUp()
    {
        var height = ActualHeight > 0 ? ActualHeight : 800;

        var oldT = new TranslateTransform();
        var newT = new TranslateTransform();
        _old!.RenderTransform = oldT;
        _new!.RenderTransform = newT;

        oldT.BeginAnimation(TranslateTransform.YProperty,
            new DoubleAnimation(0, -height * 0.1, Dur) { EasingFunction = Ease });
        newT.BeginAnimation(TranslateTransform.YProperty,
            new DoubleAnimation(height * 0.1, 0, Dur) { EasingFunction = Ease });

        if (_old.Content != null)
            _old.BeginAnimation(OpacityProperty,
                new DoubleAnimation(1, 0, Dur) { EasingFunction = Ease });
        _new.BeginAnimation(OpacityProperty,
            new DoubleAnimation(0, 1, Dur) { EasingFunction = Ease });
    }
}

public enum NavigationDirection { Forward, Back }

public class PageNavigatedEventArgs(object page) : EventArgs
{
    public object Page { get; } = page;
}

