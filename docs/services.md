# 全局服务

命名空间:`Mine.Wpf.Controls.Controls`

三个静态服务类提供「无宿主」的全局浮层能力:自动在当前活动 `Window` 上注入覆盖层承载内容,无需在 XAML 中手动放置宿主控件。

---

## `DialogService`

全局模态对话框服务。

| 签名 | 说明 |
|---|---|
| `Register(DialogHost host)` | 注册固定 `DialogHost` 实例(可选) |
| `Task<bool> ShowAsync(string? title = null, object? content = null, string confirmText = "确认", string cancelText = "取消", bool hasCancel = true)` | 显示对话框,返回用户是否点击确认 |

```csharp
var ok = await DialogService.ShowAsync(
    title: "删除确认",
    content: "确定要删除这些文件吗?此操作不可撤销。",
    confirmText: "删除",
    cancelText: "取消");
if (ok) { /* 执行删除 */ }
```

`DialogHost` 控件 API 见[浮层](controls/overlays.md)。

---

## `SnackbarService`

全局 Snackbar 服务。每次调用在活动窗口底部叠放一条消息,多条从下往上排列,自动关闭后补位。

| 签名 | 说明 |
|---|---|
| `Show(string message, string? actionLabel = null, TimeSpan? duration = null, Action? onAction = null)` | 显示 Snackbar;`actionLabel` 非空时显示操作按钮,`onAction` 为点击回调 |

```csharp
SnackbarService.Show("已保存 3 项更改");
SnackbarService.Show("文件已删除", actionLabel: "撤销", onAction: () => RestoreFile());
```

`Snackbar` 控件 API 见[浮层](controls/overlays.md)。

---

## `ToastService`

全局 Toast 通知服务。从窗口右上角叠放通知卡片,同时维护历史通知列表供 `NotificationCenter` 显示。

### 属性

| 成员 | 类型 | 说明 |
|---|---|---|
| `Notifications` | `ObservableCollection<NotificationItem>` | 历史通知(最多保留最近 50 条) |
| `UnreadCount` | `int` | 未读通知数量 |

### 方法

| 签名 | 说明 |
|---|---|
| `Show(string title, string? message = null, ToastLevel level = ToastLevel.Info, TimeSpan? duration = null)` | 主入口,显示一条通知 |
| `Info(string title, ...)` / `Success(...)` / `Warning(...)` / `Error(...)` | 按级别显示通知的便捷方法 |
| `MarkAllRead()` | 全部标记为已读 |
| `ClearAll()` | 清空历史通知 |

```csharp
ToastService.Success("导出完成", "report.xlsx 已生成");
ToastService.Error("同步失败", "网络不可用,请稍后重试");

// 通知中心配合使用(示例)
var vm = new { Items = ToastService.Notifications, Unread = ... };
```

### `ToastLevel` 枚举

| 值 | 说明 |
|---|---|
| `Info` | 信息(默认) |
| `Success` | 成功 |
| `Warning` | 警告 |
| `Error` | 错误 |

### `NotificationItem`

历史通知条目(非 UI 类型,供绑定):

| 属性 | 类型 | 说明 |
|---|---|---|
| `Level` | `ToastLevel` | 级别 |
| `Title` | `string` | 标题 |
| `Message` | `string?` | 正文 |
| `Timestamp` | `DateTime` | 时间戳 |
| `IsRead` | `bool` | 是否已读(可写) |
| `TimeAgo` | `string` | 时间友好文案(刚刚 / N 分钟前 / …) |
| `LevelIcon` | `string` | 按级别返回的 Material Symbols 图标码点 |

---

## `NotificationCenter`(控件)

通知中心面板:绑定 `ToastService.Notifications`,带未读徽标、全部已读与清空按钮。

| 属性 | 类型 | 默认值 | 说明 |
|---|---|---|---|
| `UnreadCount` | `int` | `0` | 未读数量(只读,由服务自动同步) |
| `HasUnread` | `bool` | `false` | 是否存在未读(只读) |

```xml
<mine:NotificationCenter/>
```

> `ToastService` / `NotificationCenter` 之间通过静态 `ObservableCollection` 联动,无需额外绑定代码。
