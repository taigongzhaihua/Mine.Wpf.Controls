# Toast 与 NotificationCenter

本章详细介绍 **Toast**（即时通知卡片）和 **NotificationCenter**（通知中心）的设计原理、  
API 参考及最佳实践。

---

## 目录

1. [概念与架构](#概念与架构)
2. [Toast 控件](#toast-控件)
3. [ToastService：全局静态服务](#toastservice全局静态服务)
4. [NotificationCenter：通知中心](#notificationcenter通知中心)
5. [NotificationItem：通知数据模型](#notificationitem通知数据模型)
6. [动画行为](#动画行为)
7. [完整集成示例](#完整集成示例)
8. [常见问题](#常见问题)

---

## 概念与架构

```
用户代码
  └── ToastService.Info / Success / Warning / Error
		  │
		  ├── 创建 NotificationItem，写入 ToastService.Notifications
		  │
		  ├── 调用 WindowOverlay.GetOrCreate(window) 获取覆盖 Canvas
		  │
		  └── 在覆盖层右上角创建 Toast 控件 → 播放入场动画 → 计时自动关闭

NotificationCenter（可选）
  └── 绑定 ToastService.Notifications → 以 Flyout 列表展示历史通知
	  └── PART_Bell 按钮（含未读 Badge）→ 点击触发 FlyoutService
```

两个组件协作但**相互独立**：`ToastService` 负责即时弹出，`NotificationCenter` 负责历史查阅。  
你可以只用 `ToastService` 而不使用 `NotificationCenter`，也可以反之。

---

## Toast 控件

**类**：`Mine.Wpf.Controls.Controls.Toast`  
**基类**：`ContentControl`

通常**不需要**直接在 XAML 中放置 `Toast` 实例，而是通过 `ToastService` 动态创建。  
如有特殊需求（如内嵌提示），可按如下方式手动使用。

### 属性表

| 属性 | 类型 | 默认值 | 说明 |
|------|------|--------|------|
| `Title` | `string` | `""` | 通知标题（加粗显示） |
| `Content` | `object?` | `null` | 通知正文（可为字符串或任意 UIElement） |
| `Level` | `ToastLevel` | `Info` | 通知级别，影响图标颜色 |
| `Duration` | `TimeSpan` | `5s` | 自动关闭延迟；`TimeSpan.Zero` 禁用自动关闭 |
| `IsOpen` | `bool` | `false` | 控制显示/关闭（设为 `false` 触发退场动画） |

### ToastLevel 枚举

| 值 | 图标 | 语义 |
|----|------|------|
| `Info` | info（圆形 i） | 普通信息 |
| `Success` | check_circle | 操作成功 |
| `Warning` | warning | 警告 |
| `Error` | error | 错误 |

### 模板部件（TemplatePart）

| 名称 | 类型 | 说明 |
|------|------|------|
| `PART_Root` | `Border` | 卡片根容器（动画目标） |
| `PART_Close` | `ButtonBase` | 关闭按钮（`controls:Button` + `Mine.Style.Button.Icon`） |

### 事件

| 事件 | 说明 |
|------|------|
| `Closed` | Toast 退场动画完成并从视觉树移除后触发 |

### 手动使用示例

```xml
<!-- 静态内嵌 Toast（不常见，用于 UI 演示） -->
<mine:Toast Title="文件已保存"
			Content="MyDocument.docx 已保存到桌面"
			Level="Success"
			IsOpen="True"
			Duration="0:0:0"/>
```

```csharp
// 代码创建（通常通过 ToastService 更简便）
var toast = new Toast();
myPanel.Children.Add(toast);
toast.Show("提示", "操作完成", ToastLevel.Success, TimeSpan.FromSeconds(3));
```

---

## ToastService：全局静态服务

`ToastService` 是驱动 Toast 系统的核心入口。它会自动定位当前活动窗口，在其右上角叠加显示通知卡片，并维护历史通知列表。

### 快捷方法

```csharp
// 信息通知
ToastService.Info("标题");
ToastService.Info("标题", "可选的正文描述");

// 成功通知
ToastService.Success("文件已保存");
ToastService.Success("上传完成", $"共上传 {count} 个文件");

// 警告通知
ToastService.Warning("存储空间不足", "剩余空间低于 1 GB，请及时清理");

// 错误通知
ToastService.Error("连接失败", ex.Message);
```

### 完整方法签名

```csharp
public static void Show(
	string     title,
	string?    message  = null,
	ToastLevel level    = ToastLevel.Info,
	TimeSpan?  duration = null);   // null 使用默认 5 秒
```

### 持久通知（不自动关闭）

```csharp
ToastService.Show("正在同步...", level: ToastLevel.Info, duration: TimeSpan.Zero);
```

### 访问通知历史

```csharp
// ObservableCollection<NotificationItem>，可直接绑定到 ItemsControl
var history = ToastService.Notifications;

// 未读数量
int unread = ToastService.UnreadCount;
```

### 清除操作

```csharp
// 标记全部已读（在 NotificationCenter 中调用）
ToastService.MarkAllRead();    // 由 NotificationCenter 内部使用

// 清空全部历史
ToastService.ClearAll();       // 由 NotificationCenter 内部使用
```

> **线程安全**：`ToastService.Show` 系列方法可在后台线程调用，内部已使用 `Dispatcher.BeginInvoke` 切换到 UI 线程。

---

## NotificationCenter：通知中心

**类**：`Mine.Wpf.Controls.Controls.NotificationCenter`  
**基类**：`Control`

NotificationCenter 提供一个铃铛图标按钮，点击后以 **Flyout** 形式弹出历史通知列表，并通过 **Badge** 显示未读数量。

### 属性表

| 属性 | 类型 | 默认值 | 说明 |
|------|------|--------|------|
| `UnreadBadgeText` | `string?` | 只读，自动推断 | 未读数 Badge 文字；`0` 时为 `null`（隐藏） |

### 模板部件

| 名称 | 类型 | 说明 |
|------|------|------|
| `PART_Bell` | `Button`（`controls:Button`） | 铃铛按钮，点击触发 Flyout |

### 内置操作（Flyout 内）

通知 Flyout 内部包含两个操作按钮：

| 按钮 | 说明 |
|------|------|
| 标记全部已读 | 调用 `ToastService.MarkAllRead()`，Badge 清零 |
| 清空全部 | 调用 `ToastService.ClearAll()`，列表和 Badge 均清零 |

### 在 XAML 中使用

```xml
<!-- 典型用法：放在应用标题栏/工具栏右侧 -->
<mine:NotificationCenter HorizontalAlignment="Right"
						 VerticalAlignment="Center"/>
```

无需任何额外绑定，`NotificationCenter` 在内部订阅 `ToastService.Notifications` 的变化，全自动更新 Badge 和列表。

### 集成到标题栏

```xml
<DockPanel>
	<!-- 左侧：导航标题 -->
	<TextBlock Text="我的应用" DockPanel.Dock="Left" VerticalAlignment="Center"/>

	<!-- 右侧：通知中心 -->
	<mine:NotificationCenter DockPanel.Dock="Right" Margin="0,0,8,0"/>

	<!-- 中间：其余内容 -->
	<ContentControl/>
</DockPanel>
```

---

## NotificationItem：通知数据模型

`NotificationItem` 是存储在 `ToastService.Notifications` 列表中的数据对象。

```csharp
public class NotificationItem
{
	public ToastLevel Level     { get; init; }   // 通知级别
	public string     Title     { get; init; }   // 标题
	public string?    Message   { get; init; }   // 正文（可为 null）
	public DateTime   Timestamp { get; init; }   // 创建时间
	public bool       IsRead    { get; set;  }   // 是否已读（可变）

	// 计算属性（每次访问实时计算）
	public string TimeAgo   { get; }   // "刚刚" / "N 分钟前" / "N 小时前" / "M月D日"
	public string LevelIcon { get; }   // Material Symbols Unicode 码点
}
```

### 自定义通知列表 UI

如需自定义通知历史的呈现方式，可直接绑定 `ToastService.Notifications`：

```xml
<ItemsControl ItemsSource="{x:Static controls:ToastService.Notifications}">
	<ItemsControl.ItemTemplate>
		<DataTemplate DataType="{x:Type controls:NotificationItem}">
			<Border Padding="12,8" Margin="0,2">
				<StackPanel>
					<TextBlock Text="{Binding Title}" FontWeight="SemiBold"/>
					<TextBlock Text="{Binding Message}" Opacity="0.7"/>
					<TextBlock Text="{Binding TimeAgo}" FontSize="11" Opacity="0.5"/>
				</StackPanel>
			</Border>
		</DataTemplate>
	</ItemsControl.ItemTemplate>
</ItemsControl>
```

---

## 动画行为

### 入场动画（AnimateIn）

Toast 出现时执行**右侧滑入 + 淡入**：
- X 轴位移：`60px → 0`，280 ms，`CubicEase.EaseOut`
- 透明度：`0 → 1`，200 ms，`CubicEase.EaseOut`

### 退场动画（AnimateOut）

关闭时执行**淡出**后从视觉树移除：
- 透明度：`1 → 0`，180 ms，`CubicEase.EaseIn`
- 动画完成后：触发 `Closed` 事件 → 从父 `Panel.Children` 移除控件（或 `Visibility = Collapsed`）

> **安全移除**：退场动画完成回调通过 `Dispatcher.BeginInvoke` 执行，确保不在动画栈中直接操作视觉树，避免 WPF 布局异常。

### 堆叠布局

多条 Toast 同时存在时，以 `StackPanel`（垂直）从上到下堆叠，新通知插入顶部（`Insert(0, toast)`），视觉上最新的消息始终显示在最上方。

---

## 完整集成示例

### 1. App.xaml — 引导主题

```xml
<Application xmlns:mine="https://schemas.mine.io/wpf" ...>
	<Application.Resources>
		<ResourceDictionary>
			<ResourceDictionary.MergedDictionaries>
				<mine:ThemeDictionary Mode="System" SeedColor="#6750A4"/>
			</ResourceDictionary.MergedDictionaries>
		</ResourceDictionary>
	</Application.Resources>
</Application>
```

### 2. MainWindow.xaml — 放置 NotificationCenter

```xml
<Window xmlns:mine="https://schemas.mine.io/wpf" ...>
	<DockPanel>
		<ToolBar DockPanel.Dock="Top" Height="48">
			<TextBlock Text="我的应用" VerticalAlignment="Center" Margin="8,0"/>
			<mine:NotificationCenter HorizontalAlignment="Right" Margin="0,0,8,0"/>
		</ToolBar>
		<Frame x:Name="MainFrame" DockPanel.Dock="Bottom"/>
	</DockPanel>
</Window>
```

### 3. 任意页面 — 发送通知

```csharp
// 信息提示
private void OnSave(object sender, RoutedEventArgs e)
{
	await SaveAsync();
	ToastService.Success("保存成功", "数据已写入数据库");
}

// 错误提示
private async Task LoadDataAsync()
{
	try
	{
		await _service.LoadAsync();
		ToastService.Info("数据加载完成");
	}
	catch (Exception ex)
	{
		ToastService.Error("加载失败", ex.Message);
	}
}

// 持久通知（需要用户手动关闭）
private void OnStartSync()
{
	ToastService.Show("正在同步数据...",
		level: ToastLevel.Info,
		duration: TimeSpan.Zero);   // 不自动关闭
}
```

---

## 常见问题

### Q：Toast 显示在哪个窗口？

`ToastService` 通过 `WindowOverlay.GetActiveWindow()` 获取当前激活的顶层窗口，并在其覆盖层上显示。  
如果应用有多个窗口同时存在，Toast 会出现在当前有焦点的窗口上。

### Q：如何控制 Toast 最大同时显示数量？

库默认不限制同时显示数量，但历史记录最多保留 50 条（`MaxHistory = 50`）。  
如需限制显示数量，在调用 `ToastService.Show` 前自行检查 `ToastService.Notifications.Count`。

### Q：NotificationCenter 必须和 ToastService 配合使用吗？

不必须，但推荐。`NotificationCenter` 内部绑定 `ToastService.Notifications`；  
如果你有自己的通知数据源，可以继承 `NotificationCenter` 并覆盖 Flyout 内容。

### Q：后台线程能调用 ToastService.Show 吗？

可以。`ToastService.Show` 内部使用 `Dispatcher.BeginInvoke` 将操作切换到 UI 线程执行。

### Q：如何自定义 Toast 的外观？

在你的应用资源中覆盖 `Toast` 的隐式样式（`TargetType="{x:Type controls:Toast}"`），  
或自定义 `ControlTemplate` 中的 `Mine.Internal.Toast.Template` 资源键。

---

上一章：[控件参考 ←](03-controls.md)　　下一章：[附加行为与服务 →](05-attached-behaviors.md)
