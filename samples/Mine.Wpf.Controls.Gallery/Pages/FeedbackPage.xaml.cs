using System.Windows;
using Mine.Wpf.Controls.Controls;
using TextBox = Mine.Wpf.Controls.Controls.TextBox;

namespace Mine.Wpf.Controls.Gallery.Pages;

public partial class FeedbackPage
{
    public FeedbackPage() => InitializeComponent();

    // ── Snackbar demos ──────────────────────────────────────────────
    private void OnSnackbarBasic(object sender, RoutedEventArgs e)
        => SnackbarService.Show("文件已保存");

    private void OnSnackbarAction(object sender, RoutedEventArgs e)
        => SnackbarService.Show("邮件已归档", actionLabel: "撤销");

    private void OnSnackbarLong(object sender, RoutedEventArgs e)
        => SnackbarService.Show("无法连接到服务器，请检查网络设置后重试");

    private void OnSnackbarPersist(object sender, RoutedEventArgs e)
        => SnackbarService.Show("此消息不会自动关闭", actionLabel: "关闭",
                                duration: TimeSpan.Zero);

    // ── Dialog demos ────────────────────────────────────────────────
    private async void OnDialogConfirm(object sender, RoutedEventArgs e)
    {
        var result = await DialogService.ShowAsync(
                                                   title:       "删除文件",
                                                   content:     "确定要删除选中的 3 个文件吗？此操作无法撤销。",
                                                   confirmText: "删除",
                                                   cancelText:  "取消");
        ShowResult(result ? "用户点击了【删除】" : "用户点击了【取消】");
    }

    private async void OnDialogAlert(object sender, RoutedEventArgs e)
    {
        await DialogService.ShowAsync(
            title:       "更新提示",
            content:     "已发现新版本 v2.0，请前往官网下载。",
            confirmText: "知道了",
            hasCancel:   false);
        ShowResult("用户已关闭提示");
    }

    private async void OnDialogCustom(object sender, RoutedEventArgs e)
    {
        var input = new TextBox
        {
            Text    = "未命名文档",
            Hint = "请输入新名称",
            Variant = TextFieldVariant.Outlined,
            VerticalAlignment = VerticalAlignment.Center
        };

        var ok = await DialogService.ShowAsync(
                                               title:       "重命名",
                                               content:     input,
                                               confirmText: "确认",
                                               cancelText:  "取消");

        ShowResult(ok ? $"重命名为：{input.Text}" : "已取消重命名");
    }

    private void ShowResult(string text)
    {
        ResultText.Text = text;
        ResultBorder.Visibility = Visibility.Visible;
    }
}

