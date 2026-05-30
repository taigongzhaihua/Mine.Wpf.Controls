using System.Windows;
using System.Windows.Controls;

namespace Mine.Wpf.Controls.Controls;

/// <summary>
/// Material 风格页面基类，配合 <see cref="Frame"/> 使用。
/// 默认提供内置 ScrollViewer（可通过 IsScrollable=False 关闭）。
/// </summary>
public class Page : ContentControl
{
    static Page() =>
        DefaultStyleKeyProperty.OverrideMetadata(
            typeof(Page), new FrameworkPropertyMetadata(typeof(Page)));

    // ── Title ──────────────────────────────────────────────────────
    public static readonly DependencyProperty TitleProperty =
        DependencyProperty.Register(nameof(Title), typeof(string), typeof(Page),
            new PropertyMetadata(string.Empty));
    public string Title
    {
        get => (string)GetValue(TitleProperty);
        set => SetValue(TitleProperty, value);
    }

    // ── Subtitle ───────────────────────────────────────────────────
    public static readonly DependencyProperty SubtitleProperty =
        DependencyProperty.Register(nameof(Subtitle), typeof(string), typeof(Page),
            new PropertyMetadata(string.Empty));
    public string Subtitle
    {
        get => (string)GetValue(SubtitleProperty);
        set => SetValue(SubtitleProperty, value);
    }

    // ── IsScrollable ───────────────────────────────────────────────
    /// <summary>
    /// True（默认）：页面内容包裹在 ScrollViewer 中自动滚动。
    /// False：内容直接铺满，由页面内部自行管理滚动。
    /// </summary>
    public static readonly DependencyProperty IsScrollableProperty =
        DependencyProperty.Register(nameof(IsScrollable), typeof(bool), typeof(Page),
            new PropertyMetadata(true));
    public bool IsScrollable
    {
        get => (bool)GetValue(IsScrollableProperty);
        set => SetValue(IsScrollableProperty, value);
    }

    // ── Parent Frame ───────────────────────────────────────────────
    /// <summary>当前页面所在的 <see cref="Frame"/>，由 Frame 在导航时注入。</summary>
    public Frame? Frame { get; internal set; }

    // ── Navigation lifecycle ───────────────────────────────────────
    /// <summary>页面被导航进来时调用（Frame 已完成内容切换）。</summary>
    protected virtual void OnNavigatedTo() { }

    /// <summary>页面即将被导航离开时调用（新页面尚未显示）。</summary>
    protected virtual void OnNavigatingFrom() { }

    // Internal dispatch called by Frame
    internal void InternalOnNavigatedTo(Frame frame)
    {
        Frame = frame;
        OnNavigatedTo();
    }

    internal void InternalOnNavigatingFrom()
    {
        OnNavigatingFrom();
        Frame = null;
    }
}

/// <summary>页面过渡动画类型。</summary>
public enum PageTransition
{
    /// <summary>无动画，直接切换。</summary>
    None,
    /// <summary>淡入淡出。</summary>
    Fade,
    /// <summary>Material FadeThrough：旧页淡出缩小，新页淡入放大。</summary>
    FadeThrough,
    /// <summary>向前导航：新页从右侧滑入。</summary>
    SlideForward,
    /// <summary>向后导航：新页从左侧滑入。</summary>
    SlideBack,
    /// <summary>新页从下方滑入（BottomSheet / 详情页风格）。</summary>
    SlideUp,
}
