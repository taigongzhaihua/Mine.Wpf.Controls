# 控件:浮层

命名空间:`Mine.Wpf.Controls.Controls`

> 全局服务用法见[全局服务](../services.md)。本页为浮层控件自身 API。

## `DialogHost`

模态对话框宿主。

```xml
<mine:DialogHost x:Name="Dialog" Title="确认" ConfirmText="确定" CancelText="取消">
    <TextBlock Text="对话框正文内容"/>
</mine:DialogHost>
```

| 属性 | 类型 | 默认值 | 说明 |
|---|---|---|---|
| `Title` | `string?` | `null` | 标题 |
| `ConfirmText` | `string` | `"确认"` | 确认按钮文字 |
| `CancelText` | `string` | `"取消"` | 取消按钮文字 |
| `HasCancel` | `bool` | `true` | 显示取消按钮 |
| `IsOpen` | `bool` | `false` | 是否打开 |
| `CloseOnScrim` | `bool` | `false` | 点遮罩关闭(默认关,符合 M3 规范) |

### 方法

| 签名 | 说明 |
|---|---|
| `Task<bool> ShowAsync(string? title = null, object? content = null, string confirmText = "确认", string cancelText = "取消", bool hasCancel = true)` | 异步显示,返回是否确认 |

```csharp
var ok = await Dialog.ShowAsync("退出", "确定要退出吗?");
```

---

## `Flyout`

轻量弹出层:点击触发、淡入缩放动画、点击外部自动关闭。

```xml
<mine:Button Content="菜单" mine:FlyoutService.Flyout="{Binding ElementName=MyFlyout}"/>
<mine:Flyout x:Name="MyFlyout" Placement="Bottom" VerticalOffset="4">
    <StackPanel>...弹出内容...</StackPanel>
</mine:Flyout>
```

| 属性 | 类型 | 默认值 | 说明 |
|---|---|---|---|
| `IsOpen` | `bool` | `false` | 是否打开(双向) |
| `Placement` | `PlacementMode` | `Bottom` | 放置模式 |
| `PlacementTarget` | `UIElement?` | `null` | 定位目标 |
| `HorizontalOffset` | `double` | `0` | 水平偏移 |
| `VerticalOffset` | `double` | `4` | 垂直偏移 |
| `AutoPlacement` | `bool` | `false` | 自动上下选位 |
| `ConstrainToWorkArea` | `bool` | `false` | 高度约束到工作区 |
| `ViewportMargin` | `double` | `16` | 与视口边缘安全边距 |

### 方法

`Show()` / `Hide()` / `Toggle()`

---

## `BottomSheet` / `SideSheet`

底部 / 侧边面板,各支持 `Modal`(遮罩)与 `Standard`(无遮罩)变体。

```xml
<mine:BottomSheet IsOpen="{Binding IsOpen}" Variant="Modal">
    <mine:BottomSheet.SheetContent>
        <StackPanel>...面板内容...</StackPanel>
    </mine:BottomSheet.SheetContent>
    <Grid>...页面内容...</Grid>
</mine:BottomSheet>

<mine:SideSheet Placement="Right" SheetWidth="360" .../>
```

### `BottomSheet` 属性

| 属性 | 类型 | 默认值 | 说明 |
|---|---|---|---|
| `IsOpen` | `bool` | `false` | 是否打开 |
| `Variant` | `BottomSheetVariant` | `Modal` | 变体 |
| `SheetContent` | `object?` | `null` | 面板内容 |
| `SheetContentTemplate` | `DataTemplate?` | `null` | 内容模板 |
| `SheetHeight` | `double` | `NaN` | 高度(NaN 自适应) |
| `MaxSheetHeight` | `double` | `+∞` | 最大高度 |
| `ShowDragHandle` | `bool` | `true` | 拖拽手柄 |
| `EnableDragToClose` | `bool` | `true` | 下拉关闭 |
| `CloseOnScrimClick` | `bool` | `true` | 点遮罩关闭 |
| `ScrimOpacity` | `double` | `0.4` | 遮罩不透明度(仅 Modal) |
| `CornerRadius` | `CornerRadius` | `(28,28,0,0)` | 顶部圆角 |

### `SideSheet` 属性

| 属性 | 类型 | 默认值 | 说明 |
|---|---|---|---|
| `IsOpen` | `bool` | `false` | 是否打开 |
| `Variant` | `SideSheetVariant` | `Modal` | 变体 |
| `Placement` | `SideSheetPlacement` | `Right` | 左侧 / 右侧 |
| `SheetContent` / `SheetContentTemplate` | `object?` / `DataTemplate?` | `null` | 面板内容 |
| `SheetWidth` | `double` | `360` | 宽度 |
| `CloseOnScrimClick` | `bool` | `true` | 点遮罩关闭(仅 Modal) |
| `ScrimOpacity` | `double` | `0.4` | 遮罩不透明度(仅 Modal) |
| `CornerRadius` | `CornerRadius` | `0` | 圆角 |

### 事件与枚举

两者均有 `Opened` / `Closed`(`RoutedEventHandler`)。

`BottomSheetVariant` / `SideSheetVariant`:`Modal`(模态,有遮罩)/ `Standard`(非模态,与内容共存)
`SideSheetPlacement`:`Left` / `Right`(默认)

---

## `Snackbar`

消息提示条。通常使用 `SnackbarService`;也可直接放置:

```xml
<mine:Snackbar x:Name="Bar" ActionLabel="撤销"/>
```

| 属性 | 类型 | 默认值 | 说明 |
|---|---|---|---|
| `ActionLabel` | `string?` | `null` | 操作按钮文字 |
| `HasAction` | `bool`(只读) | `false` | 是否有操作按钮 |
| `IsOpen` | `bool` | `false` | 是否显示 |
| `Duration` | `TimeSpan` | `4s` | 自动关闭延迟(`TimeSpan.Zero` 禁用) |

### 方法 / 事件

`Show(string message, string? actionLabel = null, TimeSpan? duration = null)`
事件:`ActionClicked`(`RoutedEventHandler`)。

---

## `Toast`

右上角滑入的富文本通知卡片。通常使用 `ToastService`;也可直接使用:

```xml
<mine:Toast x:Name="Toast" Title="标题" Level="Success"/>
```

| 属性 | 类型 | 默认值 | 说明 |
|---|---|---|---|
| `Title` | `string` | `""` | 标题 |
| `Level` | `ToastLevel` | `Info` | 级别 |
| `Duration` | `TimeSpan` | `5s` | 自动关闭延迟(`Zero` 禁用) |
| `IsOpen` | `bool` | `false` | 是否显示 |

### 方法 / 事件

`Show(string title, string? message = null, ToastLevel level = Info, TimeSpan? duration = null)` / `Close()`
事件:`Closed`(`RoutedEventHandler`,关闭动画结束后)。

---

## `RichToolTip`

Material 3 富文本提示(继承 `ToolTip`,无自有公开属性):

```xml
<mine:Button Content="帮助">
    <mine:Button.ToolTip>
        <mine:RichToolTip mine:ToolTipAssist.Title="快捷键">
            按 Ctrl+H 打开帮助。
        </mine:RichToolTip>
    </mine:Button.ToolTip>
</mine:Button>
```

---

## `FlipView`

循环翻页视图(继承 `Selector`),支持上一页/下一页与位移动画。

```xml
<mine:FlipView ItemsSource="{Binding Images}"/>
```

| 属性 | 类型 | 默认值 | 说明 |
|---|---|---|---|
| `NextCommand` / `PreviousCommand` | `ICommand`(只读) | — | 翻页命令 |
| `TransitionDuration` | `Duration` | `320ms` | 动画时长 |
| `TransitionOffsetRatio` | `double` | `0.6` | 位移比例(相对宽度) |
| `TransitionEasingMode` | `EasingMode` | `EaseOut` | 缓动模式 |

### 方法

`Next()` / `Previous()` — 循环切换。

---

## `Carousel`

Material 3 轮播(继承 `Selector`):3D 缩放位移排列,支持自动播放。

```xml
<mine:Carousel IsAutoPlay="True" AutoPlayInterval="0:0:5"
               ItemsSource="{Binding Banners}"/>
```

| 属性 | 类型 | 默认值 | 说明 |
|---|---|---|---|
| `IsAutoPlay` | `bool` | `false` | 自动播放 |
| `AutoPlayInterval` | `TimeSpan` | `5s` | 播放间隔 |
| `NextCommand` / `PreviousCommand` | `ICommand?` | — | 切换命令 |

### 方法

`Next()` / `Previous()` — 循环切换。

> 项容器 `CarouselItem`(继承 `ContentControl`)无自有公开成员。

---

## `PaginationControl`

分页控件:页码、上一页/下一页、首页/末页。

```xml
<mine:PaginationControl CurrentPage="{Binding Page}" TotalPages="{Binding Total}"
                        PageChanged="OnPageChanged"/>
```

| 属性 | 类型 | 默认值 | 说明 |
|---|---|---|---|
| `CurrentPage` | `int` | `1` | 当前页(钳制到 `[1, TotalPages]`) |
| `TotalPages` | `int` | `1` | 总页数(不小于 1) |
| `MaxPageButtons` | `int` | `7` | 页码按钮数 |
| `ShowFirstLastButtons` | `bool` | `true` | 首页/末页按钮 |
| `CornerRadius` | `CornerRadius` | `20` | 圆角 |
| `GoToPageCommand` | `ICommand`(只读) | — | 跳转命令(参数为页码) |

### 事件

| 事件 | 类型 | 说明 |
|---|---|---|
| `PageChanged` | `RoutedPropertyChangedEventHandler<int>` | 页码变化 |

---

## `RelayCommand`

简单 `ICommand` 实现(库内 `Carousel` / `FlipView` 等导航命令所用),可在应用代码中直接复用:

```csharp
var cmd = new RelayCommand(o => DoWork());
```

| 成员 | 说明 |
|---|---|
| `RelayCommand(Action<object?> execute)` | 构造函数 |
| `Execute(object? parameter)` | 执行委托 |
| `CanExecute(object? parameter)` | 始终返回 `true` |
| `CanExecuteChanged` | 标准 `ICommand` 事件 |
