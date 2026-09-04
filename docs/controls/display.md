# 控件:展示

命名空间:`Mine.Wpf.Controls.Controls`

## `Card`

Material 3 卡片,三种变体。

```xml
<mine:Card Variant="Elevated" Elevation="2" CornerRadius="12">
    <StackPanel>
        <TextBlock Text="标题"/>
        <TextBlock Text="正文内容"/>
    </StackPanel>
</mine:Card>
```

| 属性 | 类型 | 默认值 | 说明 |
|---|---|---|---|
| `Variant` | `CardVariant` | `Elevated` | 变体 |
| `CornerRadius` | `CornerRadius` | `12` | 圆角 |
| `IsClickable` | `bool` | `false` | 可点击(悬停/按下视觉反馈) |
| `Elevation` | `int` | `1` | 海拔等级 |

### `CardVariant`

| 值 | 说明 |
|---|---|
| `Elevated` | 带阴影(默认) |
| `Filled` | 填充色 |
| `Outlined` | 描边 |

---

## `Badge`

徽标:叠加在宿主内容角落的小圆点 / 数字标记。

```xml
<mine:Badge BadgeText="3">
    <mine:MaterialIcon Kind="notifications" Size="24"/>
</mine:Badge>

<mine:Badge BadgeText="" BadgePlacement="TopRight"/>   <!-- 无文字圆点 -->
```

| 属性 | 类型 | 默认值 | 说明 |
|---|---|---|---|
| `BadgeText` | `string?` | `null` | 徽标文字;`null` 隐藏,`""` 显示圆点,有文字显示大徽标 |
| `BadgeVariant` | `BadgeVariant`(只读) | `Hidden` | 由 `BadgeText` 自动推断 |
| `BadgePlacement` | `BadgePlacement` | `TopRight` | 位置 |
| `BadgeHorizontalOffset` | `double` | `0` | 水平偏移(正值向外) |
| `BadgeVerticalOffset` | `double` | `0` | 垂直偏移(正值向外) |

### `BadgeVariant` / `BadgePlacement`

`BadgeVariant`:`Hidden` / `Small`(圆点)/ `Large`(带文字)
`BadgePlacement`:`TopRight`(默认)/ `TopLeft` / `BottomRight` / `BottomLeft`

---

## `Avatar` / `AvatarGroup`

圆形头像。内容优先级:`ImageSource` > `Icon`(Material Symbols)> `Initials`(首字母)> 默认人形图标。

```xml
<mine:Avatar Size="48" ImageSource="/assets/me.png"/>
<mine:Avatar Size="48" Icon="person"/>
<mine:Avatar Size="48" Initials="ZQ"/>

<mine:AvatarGroup AvatarSize="40" Spacing="-8" MaxCount="4"
                  ItemsSource="{Binding Members}"/>
```

### `Avatar` 属性

| 属性 | 类型 | 默认值 | 说明 |
|---|---|---|---|
| `Size` | `double` | `40` | 直径 |
| `ImageSource` | `ImageSource?` | `null` | 图片 |
| `Icon` | `string?` | `null` | Material Symbols 图标字符 |
| `Initials` | `string?` | `null` | 文字(建议 1–2 字符) |
| `AvatarVariant` | `AvatarVariant`(只读) | `Default` | 内容变体,自动推断 |

### `AvatarGroup` 属性

| 属性 | 类型 | 默认值 | 说明 |
|---|---|---|---|
| `AvatarSize` | `double` | `40` | 成员头像直径 |
| `Spacing` | `double` | `-8` | 间距(负值重叠) |
| `MaxCount` | `int` | `5` | 最多显示数量,超出显示 `+N`(-1 不限) |
| `OverflowCount` | `int`(只读) | `0` | 隐藏数量 |
| `OverflowText` | `string`(只读) | `""` | 溢出气泡文字(如 `"+3"`) |
| `AvatarBorderBrush` | `Brush?` | `null` | 头像描边 |
| `AvatarBorderThickness` | `double` | `2` | 描边粗细 |

> 内部布局面板 `AvatarGroupPanel`(继承 `Panel`,公开属性 `Spacing` 默认 `-8`)负责头像水平重叠排列,一般无需直接使用。

---

## `Shield`

徽章控件(GitHub Badge 风格):左侧标签 + 右侧内容。

```xml
<mine:Shield Label="version" Value="0.1.0"/>
```

| 属性 | 类型 | 默认值 | 说明 |
|---|---|---|---|
| `Label` | `string` | `""` | 左侧标签 |
| `Value` | `string` | `""` | 右侧内容 |
| `Color` | `Brush?` | `null` | 内容区颜色 |
| `LabelBackground` | `Brush?` | `null` | 标签区背景 |
| `CornerRadius` | `CornerRadius` | `4` | 圆角 |

---

## `ProgressBar` / `ProgressRing`

线形 / 环形进度指示器,支持确定与不定态。

```xml
<mine:ProgressBar Value="{Binding Percent}" Maximum="100"/>
<mine:ProgressBar IsIndeterminate="True"/>

<mine:ProgressRing Value="{Binding Percent}" StrokeThickness="4"/>
<mine:ProgressRing IsIndeterminate="True"/>
```

### `ProgressBar`

继承 `System.Windows.Controls.ProgressBar` 全部 API(`Value` / `Maximum` / `IsIndeterminate`),无自有公开属性。

### `ProgressRing` 属性

| 属性 | 类型 | 默认值 | 说明 |
|---|---|---|---|
| `Value` | `double` | `0` | 当前值 |
| `Minimum` | `double` | `0` | 最小值 |
| `Maximum` | `double` | `100` | 最大值 |
| `IsIndeterminate` | `bool` | `false` | 不定态 |
| `StrokeThickness` | `double` | `4` | 描边粗细 |
| `StrokeDashArray` | `DoubleCollection`(只读) | — | 确定态虚线(模板应用后计算) |
| `StrokeDashOffset` | `double`(只读) | `0` | 虚线偏移 |

---

## `ImageViewer`

图片查看器:缩放、拖拽平移、旋转,内置工具栏。

```xml
<mine:ImageViewer Source="/assets/photo.jpg" Zoom="1.0"/>
```

### 属性

| 属性 | 类型 | 默认值 | 说明 |
|---|---|---|---|
| `Source` | `ImageSource?` | `null` | 图片(变更时自动重置) |
| `Zoom` | `double` | `1.0` | 缩放级别(钳制到 `[MinZoom, MaxZoom]`) |
| `MinZoom` | `double` | `0.1` | 最小缩放 |
| `MaxZoom` | `double` | `10` | 最大缩放 |
| `ZoomStep` | `double` | `0.1` | 缩放步长 |
| `Rotation` | `double` | `0` | 旋转角度(度) |
| `ShowToolbar` | `bool` | `true` | 显示工具栏 |
| `EnableZoom` | `bool` | `true` | 启用缩放 |
| `EnablePan` | `bool` | `true` | 启用拖拽 |
| `ZoomInCommand` 等 | `ICommand?` | — | 工具栏命令(已初始化,可替换) |

### 方法

| 方法 | 说明 |
|---|---|
| `ZoomIn()` / `ZoomOut()` | 放大 / 缩小 |
| `Reset()` | 重置全部变换 |
| `RotateLeft()` / `RotateRight()` | 左旋 / 右旋 90° |
| `FitToWindow()` | 适应窗口(等比缩放留 10% 边距) |

---

## `InfoBar`

内联信息提示条:页面/表单内的状态消息,常驻布局,不悬浮、不自动消失。

```xml
<mine:InfoBar Severity="Warning" Title="磁盘空间不足"
              Message="剩余空间小于 5%,请及时清理。"/>
```

| 属性 | 类型 | 默认值 | 说明 |
|---|---|---|---|
| `Severity` | `InfoBarSeverity` | `Info` | 级别(语义色) |
| `Title` | `object?` | `null` | 标题 |
| `Message` | `object?` | `null` | 描述文本 |
| `ActionContent` | `object?` | `null` | 右侧操作区 |
| `IsOpen` | `bool` | `true` | 是否显示(置 `false` 触发 `Closed`) |
| `IsClosable` | `bool` | `true` | 显示关闭按钮 |
| `CornerRadius` | `CornerRadius` | `8` | 圆角 |

### 事件

| 事件 | 类型 | 说明 |
|---|---|---|
| `Closed` | `RoutedEventHandler` | 关闭(`IsOpen` 变 `false`) |

### `InfoBarSeverity`

| 值 | 说明 |
|---|---|
| `Info` | 信息(默认) |
| `Success` | 成功 |
| `Warning` | 警告 |
| `Error` | 错误 |

---

## `Expander`

可展开控件,带流畅展开/收起动画。

```xml
<mine:Expander Header="高级设置" IsExpanded="True">
    <StackPanel>...</StackPanel>
</mine:Expander>
```

| 属性 | 类型 | 默认值 | 说明 |
|---|---|---|---|
| `IsExpanded` | `bool` | `false` | 是否展开 |
| `ExpandDirection` | `ExpandDirection` | `Down` | 展开方向 |
| `CornerRadius` | `CornerRadius` | `12` | 圆角 |
| `Elevation` | `int` | `0` | 海拔等级 |
| `AnimationDuration` | `Duration` | `300ms` | 动画时长 |

### 事件

| 事件 | 类型 | 说明 |
|---|---|---|
| `Expanded` | `RoutedEventHandler` | 展开 |
| `Collapsed` | `RoutedEventHandler` | 收起 |
