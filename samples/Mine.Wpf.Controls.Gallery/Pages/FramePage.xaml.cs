using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using MineControls = Mine.Wpf.Controls.Controls;
using PageTransition = Mine.Wpf.Controls.Controls.PageTransition;

namespace Mine.Wpf.Controls.Gallery.Pages;

public partial class FramePage
{
    // ── 过渡列表 ────────────────────────────────────────────────────
    private static readonly (string Label, PageTransition Transition)[] Transitions =
    [
        ("FadeThrough（默认）", PageTransition.FadeThrough),
        ("Fade",               PageTransition.Fade),
        ("SlideForward →",    PageTransition.SlideForward),
        ("SlideBack ←",       PageTransition.SlideBack),
        ("SlideUp ↑",         PageTransition.SlideUp),
        ("None（无动画）",      PageTransition.None),
    ];

    private PageTransition CurrentTransition =>
        Transitions[TransitionPicker.SelectedIndex < 0 ? 0 : TransitionPicker.SelectedIndex].Transition;

    // 用来统计栈深（Frame 没有暴露栈集合，用计数器模拟显示）
    private int _backCount;
    private int _forwardCount;

    public FramePage()
    {
        InitializeComponent();

        // 绑定过渡选项
        TransitionPicker.ItemsSource = Transitions.Select(t => t.Label).ToArray();
        TransitionPicker.SelectedIndex = 0;

        // 初始页
        Loaded += (_, _) => DemoFrame.Navigate(new DemoSubPage(1), PageTransition.None);
    }

    // ── 导航按钮 ────────────────────────────────────────────────────
    private void OnGoBack(object sender, RoutedEventArgs e)
    {
        DemoFrame.GoBack(CurrentTransition);
        _backCount    = Math.Max(0, _backCount - 1);
        _forwardCount++;
    }

    private void OnGoForward(object sender, RoutedEventArgs e)
    {
        DemoFrame.GoForward(CurrentTransition);
        _forwardCount = Math.Max(0, _forwardCount - 1);
        _backCount++;
    }

    private void OnNavigatePage1(object sender, RoutedEventArgs e) => NavigateTo(1);
    private void OnNavigatePage2(object sender, RoutedEventArgs e) => NavigateTo(2);
    private void OnNavigatePage3(object sender, RoutedEventArgs e) => NavigateTo(3);

    private void OnNavigateRoot(object sender, RoutedEventArgs e)
    {
        DemoFrame.NavigateRoot(new DemoSubPage(1), CurrentTransition);
        _backCount = _forwardCount = 0;
        UpdateStatus("Page 1");
    }

    private void NavigateTo(int index)
    {
        DemoFrame.Navigate(new DemoSubPage(index), CurrentTransition);
        _backCount++;
        _forwardCount = 0;
    }

    // ── Navigated 回调 ──────────────────────────────────────────────
    private void DemoFrame_Navigated(object sender, MineControls.PageNavigatedEventArgs e)
    {
        var title = e.Page is MineControls.Page p ? p.Title : e.Page.ToString() ?? "";
        UpdateStatus(title);
    }

    private void UpdateStatus(string currentTitle)
    {
        RunBack.Text    = _backCount    > 0 ? $"{_backCount} 页" : "空";
        RunForward.Text = _forwardCount > 0 ? $"{_forwardCount} 页" : "空";
        RunCurrent.Text = currentTitle;
    }
}

// ── 演示子页面 ───────────────────────────────────────────────────────
internal class DemoSubPage : MineControls.Page
{
    private static readonly (string Title, string Subtitle, string Icon, Color Bg)[] Pages =
    [
        ("Page 1", "第一个演示页面\n这里可以放任意内容", "\uE88A",
            Color.FromRgb(0xD0, 0xBC, 0xFF)),
        ("Page 2", "第二个演示页面\n展示 Title / Subtitle 属性", "\uE8F4",
            Color.FromRgb(0xB5, 0xCC, 0xFF)),
        ("Page 3", "第三个演示页面\n导航生命周期：OnNavigatedTo / OnNavigatingFrom", "\uE0B7",
            Color.FromRgb(0xB8, 0xF0, 0xD8)),
    ];

    public DemoSubPage(int pageNumber)
    {
        var index = Math.Clamp(pageNumber - 1, 0, Pages.Length - 1);
        var (title, subtitle, icon, bg) = Pages[index];
        Title    = title;
        Subtitle = subtitle;

        Content = new Border
        {
            Background = new SolidColorBrush(bg),
            Child = new StackPanel
            {
                VerticalAlignment   = VerticalAlignment.Center,
                HorizontalAlignment = HorizontalAlignment.Center,
                Children =
                {
                    new MineControls.MaterialIcon
                    {
                        Kind = icon,
                        Size = 56,
                        HorizontalAlignment = HorizontalAlignment.Center,
                        Margin = new Thickness(0, 0, 0, 16),
                    },
                    new TextBlock
                    {
                        Text                = title,
                        FontSize            = 28,
                        FontWeight          = FontWeights.Medium,
                        HorizontalAlignment = HorizontalAlignment.Center,
                        Margin              = new Thickness(0, 0, 0, 8),
                    },
                    new TextBlock
                    {
                        Text                = subtitle,
                        FontSize            = 14,
                        TextAlignment       = TextAlignment.Center,
                        HorizontalAlignment = HorizontalAlignment.Center,
                        Opacity             = 0.7,
                    },
                }
            }
        };
    }

    protected override void OnNavigatedTo()
    {
        base.OnNavigatedTo();
        System.Diagnostics.Debug.WriteLine($"[Frame Demo] OnNavigatedTo: {Title}");
    }

    protected override void OnNavigatingFrom()
    {
        base.OnNavigatingFrom();
        System.Diagnostics.Debug.WriteLine($"[Frame Demo] OnNavigatingFrom: {Title}");
    }
}


