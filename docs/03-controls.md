# 控件参考

本章是控件库的核心参考手册，按控件类别组织，每个控件涵盖：  
属性表、变体说明、XAML / C# 用法示例，以及与 Material Design 3 规范的对应关系。

---

## 目录

- [Button（按钮）](#button按钮)
- [Badge（徽标）](#badge徽标)
- [Card（卡片）](#card卡片)
- [Chip（筹码标签）](#chip筹码标签)
- [CheckBox / RadioButton / Switch（选择控件）](#checkbox--radiobutton--switch选择控件)
- [TextBox（文本输入框）](#textbox文本输入框)
- [ComboBox（下拉框）](#combobox下拉框)
- [Slider / RangeSlider（滑动条）](#slider--rangeslider滑动条)
- [ProgressBar / ProgressRing（进度指示器）](#progressbar--progressring进度指示器)
- [NavigationView（导航视图）](#navigationview导航视图)
- [Flyout（弹出层）](#flyout弹出层)
- [DialogHost（对话框）](#dialoghost对话框)
- [Snackbar（消息条）](#snackbar消息条)
- [Toast / NotificationCenter（通知）](#toast--notificationcenter通知)
- [MaterialIcon（图标）](#materialicon图标)
- [Avatar / AvatarGroup（头像）](#avatar--avatargroup头像)
- [其他控件速查](#其他控件速查)

---

## Button（按钮）

**类**：`Mine.Wpf.Controls.Controls.Button`  
**基类**：`System.Windows.Controls.Primitives.ButtonBase`  
**MD3 规范**：Common buttons、FAB、Icon button

### 变体（Variant）

| 变体枚举值 | 外观 | 典型用途 |
|-----------|------|----------|
| `Filled`（默认） | 实心主色背景 | 最主要的操作 |
| `Tonal` | 次要容器背景 | 次重要操作 |
| `Outlined` | 透明背景 + 边框 | 可选操作 |
| `Elevated` | 带阴影的浅色背景 | 视觉隔离场景 |
| `Text` | 无背景无边框 | 内联文字操作 |
| `Fab` | 圆形浮动按钮（56×56） | 屏幕主要操作 |
| `ExtendedFab` | 宽版浮动按钮（可含图标+文字） | 需要文字说明的主要操作 |
| `Icon`（样式） | 40×40 透明圆形 | 纯图标操作 |

> 有两种使用方式：  
> ① 通过 `Variant` 属性切换（隐式样式；`Width`/`Height` 自动适配）；  
> ② 直接引用具名样式，如 `Style="{StaticResource Mine.Style.Button.Icon}"`。

### 属性表

| 属性 | 类型 | 默认值 | 说明 |
|------|------|--------|------|
| `Variant` | `ButtonVariant` | `Filled` | 按钮变体 |
| `Icon` | `object?` | `null` | 图标（Material Symbols 字符串或 `UIElement`） |
| `IsLoading` | `bool` | `false` | 显示旋转加载指示器，同时保留原始内容 |
| `CornerRadius` | `CornerRadius` | `20,20,20,20` | 圆角半径（胶囊形） |
| `Content` | `object` | — | 按钮文字（继承自 `ContentControl`） |

### 模板部件（TemplatePart）

| 名称 | 类型 | 说明 |
|------|------|------|
| `PART_Ripple` | `RippleDecorator` | 水波纹动画容器 |
| `PART_Content` | `ContentPresenter` | 内容呈现器 |

### 用法示例

```xml
<!-- 基础用法 -->
<mine:Button Content="保存"/>

<!-- 带图标（Material Symbols Unicode 码点） -->
<mine:Button Content="下载" Icon="&#xF090;"/>

<!-- 带图标控件 -->
<mine:Button Content="收藏">
	<mine:Button.Icon>
		<mine:MaterialIcon Kind="Favorite" Size="18"/>
	</mine:Button.Icon>
</mine:Button>

<!-- Outlined 变体 -->
<mine:Button Content="取消" Variant="Outlined"/>

<!-- FAB（浮动操作按钮）-->
<mine:Button Variant="Fab" Icon="&#xE145;"/>

<!-- Extended FAB -->
<mine:Button Variant="ExtendedFab" Content="新建文档" Icon="&#xE145;"/>

<!-- 加载中 -->
<mine:Button Content="提交" IsLoading="{Binding IsBusy}"/>

<!-- 纯图标按钮（使用 Icon 样式） -->
<mine:Button Style="{StaticResource Mine.Style.Button.Icon}" Icon="&#xE5CD;"/>

<!-- 禁用 -->
<mine:Button Content="不可用" IsEnabled="False"/>
```

### 代码触发加载

```csharp
private async void OnSubmit(object sender, RoutedEventArgs e)
{
	submitBtn.IsLoading = true;
	await Task.Delay(2000); // 模拟网络请求
	submitBtn.IsLoading = false;
}
```

### 图标说明

`Icon` 属性支持两种内容：

1. **Material Symbols 字符串**：直接传入 Unicode 码点字符串（如 `"&#xE145;"`），
   模板会使用 `Mine.Font.MaterialSymbols` 字体渲染。
2. **任意 `UIElement`**：例如 `MaterialIcon` 控件、`Image`、`Path` 等。

推荐使用 `MaterialIcon` 控件以获得完整的填充/描边切换能力（见 [MaterialIcon](#materialicon图标)）。

---

## Badge（徽标）

**类**：`Mine.Wpf.Controls.Controls.Badge`  
**基类**：`ContentControl`  
**MD3 规范**：Badges

Badge 将数字或小圆点**叠加**在宿主内容的角落，适用于导航图标未读数、头像状态等场景。

### 属性表

| 属性 | 类型 | 默认值 | 说明 |
|------|------|--------|------|
| `BadgeText` | `string?` | `null` | `null`=隐藏；`""`=小圆点；文字=带字徽标 |
| `BadgeVariant` | `BadgeVariant` | 只读，自动推断 | `Hidden` / `Small` / `Large` |
| `BadgePlacement` | `BadgePlacement` | `TopRight` | 徽标相对内容的位置 |
| `BadgeHorizontalOffset` | `double` | `0` | 水平微调偏移量（px） |
| `BadgeVerticalOffset` | `double` | `0` | 垂直微调偏移量（px） |
| `Content` | `object` | — | 宿主内容（继承自 `ContentControl`） |

### BadgePlacement 枚举

`TopRight`（默认）/ `TopLeft` / `BottomRight` / `BottomLeft`

### 用法示例

```xml
<!-- 数字徽标 -->
<mine:Badge BadgeText="3">
	<mine:MaterialIcon Kind="Notifications" Size="24"/>
</mine:Badge>

<!-- 小圆点（空字符串） -->
<mine:Badge BadgeText="">
	<mine:MaterialIcon Kind="Chat" Size="24"/>
</mine:Badge>

<!-- 隐藏（null） -->
<mine:Badge BadgeText="{Binding UnreadCount, 
						 Converter={StaticResource ZeroToNullStringConverter}}">
	<mine:MaterialIcon Kind="Mail" Size="24"/>
</mine:Badge>

<!-- 左上角放置 -->
<mine:Badge BadgeText="!" BadgePlacement="TopLeft">
	<mine:Avatar Source="/avatar.png" Size="40"/>
</mine:Badge>
```

---

## Card（卡片）

**类**：`Mine.Wpf.Controls.Controls.Card`  
**基类**：`ContentControl`  
**MD3 规范**：Cards

### 变体

| 样式键 | 描述 |
|--------|------|
| `Mine.Style.Card.Elevated` | 带阴影的浮动卡片（默认） |
| `Mine.Style.Card.Filled` | 实色填充卡片 |
| `Mine.Style.Card.Outlined` | 带边框的卡片 |

### 用法示例

```xml
<!-- 默认 Elevated 卡片 -->
<mine:Card Width="300" Padding="16">
	<StackPanel Spacing="8">
		<TextBlock Text="卡片标题" Style="{DynamicResource Mine.Style.Text.TitleMedium}"/>
		<TextBlock Text="这是卡片描述内容" TextWrapping="Wrap"/>
		<mine:Button Content="操作" HorizontalAlignment="Right"/>
	</StackPanel>
</mine:Card>

<!-- Filled 卡片 -->
<mine:Card Style="{StaticResource Mine.Style.Card.Filled}" Padding="16">
	<TextBlock Text="Filled 卡片"/>
</mine:Card>

<!-- Outlined 卡片 -->
<mine:Card Style="{StaticResource Mine.Style.Card.Outlined}" Padding="16">
	<TextBlock Text="Outlined 卡片"/>
</mine:Card>
```

---

## Chip（筹码标签）

**类**：`Mine.Wpf.Controls.Controls.Chip`  
**基类**：`ButtonBase`  
**MD3 规范**：Chips

### 变体（ChipVariant）

| 变体 | 说明 |
|------|------|
| `Assist` | 辅助提示类（常显示动作图标） |
| `Filter` | 可勾选过滤类（选中时显示勾选标记） |
| `Input` | 可删除的输入标签（附带删除按钮） |
| `Suggestion` | 建议类（无需选中状态） |

### 属性表

| 属性 | 类型 | 默认值 | 说明 |
|------|------|--------|------|
| `Variant` | `ChipVariant` | `Assist` | Chip 变体 |
| `Icon` | `object?` | `null` | 前置图标 |
| `IsSelected` | `bool` | `false` | 选中状态（Filter 变体有效） |

### 用法示例

```xml
<!-- Assist Chip -->
<mine:Chip Content="日历" Icon="&#xE916;" Variant="Assist"/>

<!-- Filter Chip（可多选） -->
<mine:Chip Content="全部" Variant="Filter" IsSelected="True"/>
<mine:Chip Content="已读" Variant="Filter"/>

<!-- Input Chip（可删除） -->
<mine:Chip Content="标签名" Variant="Input" 
		   DeleteCommand="{Binding RemoveTagCommand}"/>
```

---

## CheckBox / RadioButton / Switch（选择控件）

这三个控件均直接替换对应的 WPF 原生控件，遵循相同的 API，无需任何额外属性配置即可自动应用 Material 3 样式。

```xml
<!-- CheckBox -->
<mine:CheckBox Content="同意服务条款" IsChecked="{Binding Agreed}"/>

<!-- RadioButton -->
<StackPanel>
	<mine:RadioButton Content="选项 A" GroupName="Demo"/>
	<mine:RadioButton Content="选项 B" GroupName="Demo"/>
</StackPanel>

<!-- Switch -->
<mine:Switch IsChecked="{Binding IsEnabled}" Content="启用通知"/>
```

---

## TextBox（文本输入框）

**类**：`Mine.Wpf.Controls.Controls.TextBox`  
**基类**：`System.Windows.Controls.TextBox`  
**MD3 规范**：Text fields（Filled / Outlined）

### 附加属性

| 附加属性 | 类型 | 说明 |
|----------|------|------|
| `HintAssist.Hint` | `string` | 浮动标签文字（Label） |
| `HintAssist.HelperText` | `string` | 输入框下方的辅助说明 |
| `IconAssist.Icon` | `object` | 前置图标 |

### 变体

| 样式键 | 描述 |
|--------|------|
| `Mine.Style.TextBox.Filled`（默认） | 底部边框 + 填充背景 |
| `Mine.Style.TextBox.Outlined` | 带 Notched 边框 |

```xml
<!-- Filled 文本框 -->
<mine:TextBox mine:HintAssist.Hint="电子邮件"
			  mine:HintAssist.HelperText="请输入有效的邮件地址"/>

<!-- Outlined 文本框 + 前置图标 -->
<mine:TextBox Style="{StaticResource Mine.Style.TextBox.Outlined}"
			  mine:HintAssist.Hint="搜索"
			  mine:IconAssist.Icon="&#xE8B6;"/>

<!-- 多行 -->
<mine:TextBox mine:HintAssist.Hint="备注"
			  AcceptsReturn="True"
			  Height="120"
			  TextWrapping="Wrap"
			  VerticalScrollBarVisibility="Auto"/>
```

---

## ComboBox（下拉框）

无需额外属性，直接替换：

```xml
<mine:ComboBox SelectedIndex="0" Width="200">
	<ComboBoxItem Content="选项一"/>
	<ComboBoxItem Content="选项二"/>
	<ComboBoxItem Content="选项三"/>
</mine:ComboBox>
```

绑定：

```xml
<mine:ComboBox ItemsSource="{Binding Items}"
			   DisplayMemberPath="Name"
			   SelectedValuePath="Id"
			   SelectedValue="{Binding SelectedId}"/>
```

---

## Slider / RangeSlider（滑动条）

### Slider

```xml
<mine:Slider Minimum="0" Maximum="100" Value="{Binding Volume}"/>
```

### RangeSlider（双滑块）

```xml
<mine:RangeSlider Minimum="0" Maximum="1000"
				  LowerValue="{Binding MinPrice}"
				  UpperValue="{Binding MaxPrice}"/>
```

---

## ProgressBar / ProgressRing（进度指示器）

### ProgressBar（线形进度条）

```xml
<!-- 确定值 -->
<mine:ProgressBar Value="75" Maximum="100"/>

<!-- 不确定（循环动画） -->
<mine:ProgressBar IsIndeterminate="True"/>
```

### ProgressRing（环形进度） 

```xml
<!-- 不确定 -->
<mine:ProgressRing IsIndeterminate="True" Width="48" Height="48"/>

<!-- 确定值 -->
<mine:ProgressRing Value="60" Maximum="100" Width="48" Height="48"/>
```

---

## NavigationView（导航视图）

**类**：`Mine.Wpf.Controls.Controls.NavigationView`  
实现 MD3 Navigation drawer / Navigation rail 的导航容器。

```xml
<mine:NavigationView>
	<mine:NavigationView.MenuItems>
		<mine:NavigationViewItem Content="首页"   Icon="&#xE88A;"/>
		<mine:NavigationViewItem Content="探索"   Icon="&#xE8B6;"/>
		<mine:NavigationViewItem Content="设置"   Icon="&#xE8B8;"/>
	</mine:NavigationView.MenuItems>
	<mine:NavigationView.Content>
		<Frame x:Name="ContentFrame"/>
	</mine:NavigationView.Content>
</mine:NavigationView>
```

---

## Flyout（弹出层）

**类**：`Mine.Wpf.Controls.Controls.Flyout`  
**MD3 规范**：Menus、Tooltips（富内容场景）

`Flyout` 是点击触发的轻量弹出层，支持任意内容、淡入+缩放动画，并在点击外部时自动关闭（light-dismiss）。

### 属性表

| 属性 | 类型 | 默认值 | 说明 |
|------|------|--------|------|
| `IsOpen` | `bool` | `false` | 是否展开（支持双向绑定） |
| `Placement` | `PlacementMode` | `Bottom` | 弹出方向 |
| `PlacementTarget` | `UIElement?` | `null` | 对齐目标元素 |
| `Content` | `object` | — | 弹出内容 |

### 通过 FlyoutService 附加到按钮（推荐）

```xml
<mine:Button Content="打开菜单">
	<mine:FlyoutService.Flyout>
		<mine:Flyout>
			<StackPanel Width="200" Padding="8">
				<mine:Button Content="编辑"   Variant="Text" HorizontalAlignment="Stretch"/>
				<mine:Button Content="删除"   Variant="Text" HorizontalAlignment="Stretch"/>
				<mine:Button Content="分享"   Variant="Text" HorizontalAlignment="Stretch"/>
			</StackPanel>
		</mine:Flyout>
	</mine:FlyoutService.Flyout>
</mine:Button>
```

### 通过 ElementName 绑定

```xml
<mine:Button Content="筛选"
			 mine:FlyoutService.Flyout="{Binding ElementName=FilterFlyout}"/>

<mine:Flyout x:Name="FilterFlyout" Placement="Bottom">
	<local:FilterPanel/>
</mine:Flyout>
```

### 代码控制

```csharp
myFlyout.Toggle();   // 切换开关
myFlyout.IsOpen = true;
myFlyout.IsOpen = false;
```

---

## DialogHost（对话框）

**类**：`Mine.Wpf.Controls.Controls.DialogHost`  
**MD3 规范**：Dialogs（Basic、Full-screen）

DialogHost 以模态方式覆盖当前窗口，提供 `async/await` 调用模式。

### 属性表

| 属性 | 类型 | 默认值 | 说明 |
|------|------|--------|------|
| `Title` | `string?` | `null` | 对话框标题 |
| `ConfirmText` | `string` | `"确认"` | 确认按钮文字 |
| `CancelText` | `string` | `"取消"` | 取消按钮文字 |
| `IsOpen` | `bool` | `false` | 是否显示 |

### 用法

```xml
<!-- 在窗口/页面根元素下声明 -->
<mine:DialogHost x:Name="Dialog" Title="确认删除" ConfirmText="删除" CancelText="取消">
	<TextBlock Text="此操作不可撤销，确定要删除所选项目吗？" TextWrapping="Wrap"/>
</mine:DialogHost>
```

```csharp
// 等待用户操作，返回 true=确认，false=取消
bool confirmed = await Dialog.ShowAsync();
if (confirmed)
	await DeleteItemAsync();
```

---

## Snackbar（消息条）

**类**：`Mine.Wpf.Controls.Controls.Snackbar`  
**MD3 规范**：Snackbar

### 属性表

| 属性 | 类型 | 默认值 | 说明 |
|------|------|--------|------|
| `Content` | `object` | — | 消息内容 |
| `ActionLabel` | `string?` | `null` | 操作按钮文字（为 `null` 时隐藏） |
| `IsOpen` | `bool` | `false` | 是否显示 |
| `Duration` | `TimeSpan` | `4s` | 自动关闭时长，`Zero` 禁用 |

### 通过 SnackbarService（推荐）

```csharp
// App.xaml.cs — 注册 Snackbar 实例
SnackbarService.Register(MySnackbar);
```

```csharp
// 任意位置调用
SnackbarService.Show("文件已保存");
SnackbarService.Show("操作已撤销", actionLabel: "重做", onAction: () => Redo());
```

### 直接在 XAML 中使用

```xml
<mine:Snackbar x:Name="MySnackbar" ActionLabel="撤销"
			   ActionClicked="OnUndo"/>
```

---

## Toast / NotificationCenter（通知）

Toast 是从右上角滑入的富文本通知卡片，NotificationCenter 是带历史记录的通知中心。  
详见独立章节：[Toast 与 NotificationCenter →](04-toast.md)

---

## MaterialIcon（图标）

**类**：`Mine.Wpf.Controls.Controls.MaterialIcon`

### 属性表

| 属性 | 类型 | 默认值 | 说明 |
|------|------|--------|------|
| `Kind` | `string` | `""` | 图标名（与 `MaterialIcons` 常量类字段名一致） |
| `Size` | `double` | `24` | 图标尺寸（同时设置 Width / Height） |
| `IsFilled` | `bool` | `false` | `true` 使用填充变体字体 |
| `Weight` | `FontWeight` | `Regular` | 字重（影响线条粗细） |

### 用法

```xml
<!-- 基础图标 -->
<mine:MaterialIcon Kind="Favorite" Size="24"/>

<!-- 填充变体 -->
<mine:MaterialIcon Kind="Favorite" Size="24" IsFilled="True"/>

<!-- 大号图标 -->
<mine:MaterialIcon Kind="Settings" Size="48"/>
```

### 代码方式（常量类）

```csharp
using Mine.Wpf.Controls.Icons;

// 使用常量类中的字段名
icon.Kind = MaterialIcons.Favorite;   // "\uE87D"
icon.Kind = MaterialIcons.Settings;   // "\uE8B8"
```

> `MaterialIcons` 静态类包含所有 Material Symbols Rounded 图标名称到 Unicode 码点的映射，  
> IDE 可以提供完整的自动补全。

---

## Avatar / AvatarGroup（头像）

### Avatar

```xml
<!-- 图片头像 -->
<mine:Avatar Source="/Assets/avatar.png" Size="40"/>

<!-- 文字头像（自动取首字母） -->
<mine:Avatar Content="张三" Size="40"/>

<!-- 图标头像 -->
<mine:Avatar Size="40">
	<mine:Avatar.Icon>
		<mine:MaterialIcon Kind="Person" Size="24"/>
	</mine:Avatar.Icon>
</mine:Avatar>
```

### AvatarGroup（头像组）

```xml
<mine:AvatarGroup MaxCount="3" Size="36" Overlap="12">
	<mine:Avatar Source="/a.png"/>
	<mine:Avatar Source="/b.png"/>
	<mine:Avatar Source="/c.png"/>
	<mine:Avatar Source="/d.png"/>
	<!-- 超出 MaxCount 的头像自动折叠为 "+N" -->
</mine:AvatarGroup>
```

---

## 其他控件速查

| 控件 | 类名 | 简述 |
|------|------|------|
| `ColorPicker` | `ColorPicker` | Material 颜色拾取器 |
| `DatePicker` | `DatePicker` | 日期选择器（日历弹出） |
| `TimePicker` | `TimePicker` | 时间选择器（时钟盘弹出） |
| `NumericUpDown` | `NumericUpDown` | 数值步进输入框 |
| `Rating` | `Rating` | 星级评分 |
| `SearchBox` | `SearchBox` | 带搜索图标和清除按钮的输入框 |
| `SegmentedControl` | `SegmentedControl` | 分段选择器（类 Tab） |
| `Stepper` | `Stepper` | 步骤指示器（水平/垂直） |
| `SplitView` | `SplitView` | 主/从分栏布局 |
| `Drawer` | `Drawer` | 侧边抽屉 |
| `RangeSlider` | `RangeSlider` | 双滑块区间选择 |
| `RichToolTip` | `RichToolTip` | 富内容工具提示（支持标题+图标） |
| `Frame` | `Frame` | 带导航栈的内容帧 |

---

上一章：[主题系统详解 ←](02-theming.md)　　下一章：[Toast 与 NotificationCenter →](04-toast.md)
