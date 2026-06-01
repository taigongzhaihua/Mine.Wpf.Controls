using System.Windows;
using Mine.Wpf.Controls.Controls;

namespace Mine.Wpf.Controls.Gallery.Pages;

public partial class ToastPage
{
    public ToastPage() => InitializeComponent();

    // ── Toast 级别演示 ────────────────────────────────────────────
    private void OnToastInfo(object sender, RoutedEventArgs e)
        => ToastService.Info("操作完成", "文件已成功同步到云端。");

    private void OnToastSuccess(object sender, RoutedEventArgs e)
        => ToastService.Success("上传成功", "共 12 张图片已上传。");

    private void OnToastWarning(object sender, RoutedEventArgs e)
        => ToastService.Warning("存储空间不足", "剩余空间低于 500 MB，请清理后重试。");

    private void OnToastError(object sender, RoutedEventArgs e)
        => ToastService.Error("连接失败", "无法连接到服务器，请检查网络设置。");

    private void OnToastWithMessage(object sender, RoutedEventArgs e)
        => ToastService.Info("新版本可用", "v2.1.0 已发布，包含性能改进和 Bug 修复。");

    private void OnToastPersist(object sender, RoutedEventArgs e)
        => ToastService.Show("此通知不会自动关闭", duration: TimeSpan.Zero);

    // ── NotificationCenter 演示 ───────────────────────────────────
    private void OnSendMultiple(object sender, RoutedEventArgs e)
    {
        ToastService.Success("部署完成",   "生产环境部署成功，版本 v3.5.2。");
        ToastService.Warning("证书即将过期", "SSL 证书将在 7 天后过期，请及时续签。");
        ToastService.Error("任务失败",     "数据库备份任务执行失败，请查看日志。");
    }

    private void OnMarkAllRead(object sender, RoutedEventArgs e)
        => ToastService.MarkAllRead();

    private void OnClearAll(object sender, RoutedEventArgs e)
        => ToastService.ClearAll();
}
