# 附加行为与服务

Mine.Wpf.Controls 提供一组**附加属性（Attached Properties）**和**静态服务**，  
让你无需修改控件类型就能为现有元素注入行为或扩展视觉效果。

---

## 目录

1. [FlyoutService](#flyoutservice)
2. [RippleAssist](#rippleassist)
3. [HintAssist](#hintassist)
4. [IconAssist](#iconassist)
5. [ShapeAssist](#shapeassist)
6. [ShadowAssist](#shadowassist)
7. [ToolTipAssist](#tooltoopassist)
8. [WindowOverlay（内部工具）](#windowoverlay内部工具)

---

## FlyoutService

**命名空间**：`Mine.Wpf.Controls.Attached`

将任意 `Flyout` 控件绑定到触发元素。点击触发元素时自动切换 Flyout 的展开/收起状态。

### 工作原理

1. 检测触发元素类型：若为 `ButtonBase` 子类，订阅 `ButtonBase.ClickEvent`（冒泡）；否则订阅 `MouseLeftButtonUp`。
2. 当元素 `Loaded` 后，将 `Flyout` 实例插入最近的 `Panel` 祖先的 `Children` 集合，使其进入 WPF 逻辑/视觉树（资源和模板才能正确解析）。
3. 设置 `Flyout.PlacementTarget` 为触发元素，保证弹出方向正确对齐。
4. 点击时调用 `Flyout.Toggle()` 切换状态。

### 附加属性

| 属性 | 类型 | 说明 |
|------|------|------|
| `FlyoutService.Flyout` | `Flyout?` | 要绑定的 Flyout 实例 |

### 用法一：内联定义（最常用）

```xml
<mine:Button Content="更多操作">
	<mine:FlyoutService.Flyout>
		<mine:Flyout MinWidth="180">
			<StackPanel Padding="4">
				<mine:Button Content="编辑"   Variant="Text" HorizontalAlignment="Stretch"/>
				<mine:Button Content="复制"   Variant="Text" HorizontalAlignment="Stretch"/>
				<mine:Button Content="删除"   Variant="Text" HorizontalAlignment="Stretch"/>
			</StackPanel>
		</mine:Flyout>
	</mine:FlyoutService.Flyout>
</mine:Button>
```

### 用法二：ElementName 绑定（跨层引用）

```xml
<!-- 触发按钮 -->
<mine:Button Content="筛选"
			 mine:FlyoutService.Flyout="{Binding ElementName=FilterFlyout}"/>

<!-- Flyout 定义可以放在任意位置 -->
<mine:Flyout x:Name="FilterFlyout" Placement="Bottom" MinWidth="240">
	<local:FilterPanel/>
</mine:Flyout>
```

### 用法三：代码绑定

```csharp
var flyout = new Flyout { MinWidth = 200 };
flyout.Content = new TextBlock { Text = "Flyout 内容" };
FlyoutService.SetFlyout(myButton, flyout);
```

### 注意事项

- 一个触发元素只能绑定一个 `Flyout`；重复设置会自动清除旧绑定。
- `Flyout` 被插入父 `Panel` 而非 `Popup`，因此它受父元素的 `DataContext` 和资源字典影响，绑定更简单。
- 若触发元素不在任何 `Panel` 内（罕见），`FlyoutService` 将无法自动插入，需手动将 Flyout 添加到视觉树。

---

## RippleAssist

**命名空间**：`Mine.Wpf.Controls.Attached`

为任意控件启用/禁用或自定义 Material 3 水波纹（Ripple）效果。

> `Button`、`Card`、`Chip` 等控件的模板内部已集成 `RippleDecorator`；  
> `RippleAssist` 适用于需要**从外部控制**水波纹参数的场景。

### 附加属性

| 属性 | 类型 | 默认值 | 说明 |
|------|------|--------|------|
| `RippleAssist.IsEnabled` | `bool` | `true` | 是否启用水波纹 |
| `RippleAssist.RippleColor` | `Color` | `White` | 水波纹颜色 |
| `RippleAssist.IsCentered` | `bool` | `false` | 是否从中心展开（否则从点击点展开） |
| `RippleAssist.ClipToBounds` | `bool` | `true` | 水波纹是否裁剪到边界 |

### 用法

```xml
<!-- 禁用某个按钮的水波纹 -->
<mine:Button Content="无水波纹" mine:RippleAssist.IsEnabled="False"/>

<!-- 自定义水波纹颜色 -->
<mine:Button Content="深色水波纹"
			 mine:RippleAssist.RippleColor="#FF000000"/>

<!-- 居中展开（适合 FAB 等圆形控件） -->
<mine:Button Variant="Fab" Icon="&#xE145;"
			 mine:RippleAssist.IsCentered="True"/>
```

---

## HintAssist

**命名空间**：`Mine.Wpf.Controls.Attached`

为文本输入控件（`TextBox`、`ComboBox` 等）附加浮动标签（Label）和辅助文本。

### 附加属性

| 属性 | 类型 | 默认值 | 说明 |
|------|------|--------|------|
| `HintAssist.Hint` | `string?` | `null` | 浮动标签文字（聚焦时上浮） |
| `HintAssist.HelperText` | `string?` | `null` | 输入框下方的辅助说明文字 |
| `HintAssist.HintForeground` | `Brush?` | `null` | 标签颜色（不设则跟随主题） |
| `HintAssist.FloatingScale` | `double` | `0.75` | 浮动标签的缩放比例（默认 75%） |

### 用法

```xml
<!-- 带浮动标签 -->
<mine:TextBox mine:HintAssist.Hint="用户名"/>

<!-- 带辅助文本 -->
<mine:TextBox mine:HintAssist.Hint="密码"
			  mine:HintAssist.HelperText="至少 8 个字符，包含字母和数字"/>

<!-- 自定义标签颜色 -->
<mine:TextBox mine:HintAssist.Hint="品牌色标签"
			  mine:HintAssist.HintForeground="{DynamicResource Mine.Brush.Tertiary}"/>
```

---

## IconAssist

**命名空间**：`Mine.Wpf.Controls.Attached`

为支持图标的控件（`TextBox`、`SearchBox` 等）附加前置图标。

### 附加属性

| 属性 | 类型 | 说明 |
|------|------|------|
| `IconAssist.Icon` | `object?` | 图标内容（字符串或 `UIElement`） |

### 用法

```xml
<!-- 前置图标文本框 -->
<mine:TextBox mine:HintAssist.Hint="搜索"
			  mine:IconAssist.Icon="&#xE8B6;"/>

<!-- 使用 MaterialIcon 作为图标 -->
<mine:TextBox mine:HintAssist.Hint="邮件">
	<mine:IconAssist.Icon>
		<mine:MaterialIcon Kind="Mail" Size="18"/>
	</mine:IconAssist.Icon>
</mine:TextBox>
```

---

## ShapeAssist

**命名空间**：`Mine.Wpf.Controls.Attached`

为任意 `Border` 或支持 `CornerRadius` 的元素统一设置圆角，使其遵循 MD3 形状令牌。

### 附加属性

| 属性 | 类型 | 说明 |
|------|------|------|
| `ShapeAssist.CornerRadius` | `CornerRadius` | 覆盖目标控件的圆角 |

### 用法

```xml
<Border mine:ShapeAssist.CornerRadius="{DynamicResource Mine.Shape.Medium}"
		Background="{DynamicResource Mine.Brush.SurfaceContainerLow}"
		Padding="16">
	<TextBlock Text="自定义圆角卡片"/>
</Border>
```

---

## ShadowAssist

**命名空间**：`Mine.Wpf.Controls.Attached`

为任意元素附加 Material 3 Elevation 阴影。

### 附加属性

| 属性 | 类型 | 说明 |
|------|------|------|
| `ShadowAssist.ShadowDepth` | `int`（0–5） | 阴影等级 |

### 用法

```xml
<Border mine:ShadowAssist.ShadowDepth="2"
		Background="{DynamicResource Mine.Brush.Surface}"
		Padding="16" CornerRadius="12">
	<TextBlock Text="带阴影的卡片"/>
</Border>
```

对应的 `DropShadowEffect` 资源键为 `Mine.Elevation.0` ~ `Mine.Elevation.5`，可直接在 `Effect` 属性中引用：

```xml
<Border Effect="{DynamicResource Mine.Elevation.3}"/>
```

---

## ToolTipAssist

**命名空间**：`Mine.Wpf.Controls.Attached`

为任意元素附加富内容工具提示（超越原生 `ToolTip` 纯文本限制）。

### 附加属性

| 属性 | 类型 | 说明 |
|------|------|------|
| `ToolTipAssist.ToolTip` | `object?` | 工具提示内容（字符串或任意 UIElement） |
| `ToolTipAssist.Placement` | `PlacementMode` | 工具提示方向（默认 `Top`） |

### 用法

```xml
<!-- 文字提示 -->
<mine:Button Content="删除"
			 mine:ToolTipAssist.ToolTip="永久删除选中项目"/>

<!-- 富内容提示 -->
<mine:Button Icon="&#xE88A;">
	<mine:ToolTipAssist.ToolTip>
		<StackPanel Orientation="Horizontal" Spacing="8">
			<mine:MaterialIcon Kind="Info" Size="16"/>
			<TextBlock Text="点击查看详情"/>
		</StackPanel>
	</mine:ToolTipAssist.ToolTip>
</mine:Button>
```

---

## WindowOverlay（内部工具）

**命名空间**：`Mine.Wpf.Controls.Helpers`  
**访问级别**：`internal`（不对外公开）

`WindowOverlay` 是 `ToastService` 和 `DialogHost` 的基础设施，  
它在目标 `Window` 的根视觉树上动态叠加一个透明 `Canvas`，用于放置覆盖全屏的控件（Toast、对话框背景等），同时不影响原有布局。

> 开发者通常不需要直接使用此类；了解其原理有助于排查 Toast / Dialog 显示异常问题。

### 关键特性

- 使用 `AdornerLayer` 或直接替换 `Window.Content` 为 `Grid` 的方式插入覆盖层。
- 每个 `Window` 最多创建一个覆盖层（惰性单例）。
- Toast 容器（`StackPanel`）以固定 `Tag = "__Mine.ToastContainer__"` 标识，避免重复创建。

---

上一章：[Toast 与 NotificationCenter ←](04-toast.md)　　下一章：[自定义控件与模板扩展 →](06-customization.md)
