# 自定义控件与模板扩展

本章讲解如何在 Mine.Wpf.Controls 的架构体系内**扩展现有控件**或**创建新控件**，  
包括：覆盖控件样式、编写自定义 ControlTemplate、向现有控件添加附加属性，以及实现全新的 Mine 风格控件。

---

## 目录

1. [架构分层回顾](#架构分层回顾)
2. [覆盖控件样式](#覆盖控件样式)
3. [自定义 ControlTemplate](#自定义-controltemplate)
4. [利用 RippleDecorator 添加水波纹](#利用-rippledecorator-添加水波纹)
5. [创建遵循库规范的新控件](#创建遵循库规范的新控件)
6. [在模板中使用主题资源令牌](#在模板中使用主题资源令牌)
7. [TemplatePart 约定](#templatepart-约定)
8. [常见扩展场景示例](#常见扩展场景示例)

---

## 架构分层回顾

```
Controls/Button.cs            ← 行为层：依赖属性、命令、视觉状态逻辑
Themes/Controls/Button.xaml  ← 视觉层：ControlTemplate、Style 变体
Themes/Resources/             ← 令牌层：颜色、形状、阴影、字体（DynamicResource）
```

**黄金法则**：行为层与视觉层通过 `DefaultStyleKey` + 隐式样式**自动绑定**，  
开发者可以在不修改 C# 代码的情况下完全替换视觉层，也可以在不修改 XAML 的情况下扩展行为层。

---

## 覆盖控件样式

### 方式一：在 App.xaml 中全局覆盖

```xml
<Application.Resources>
	<ResourceDictionary>
		<ResourceDictionary.MergedDictionaries>
			<!-- ① Mine 主题字典（必须在前） -->
			<mine:ThemeDictionary Mode="System" SeedColor="#6750A4"/>
		</ResourceDictionary.MergedDictionaries>

		<!-- ② 在合并字典之后覆盖（优先级更高） -->
		<Style TargetType="{x:Type mine:Button}"
			   BasedOn="{StaticResource Mine.Style.Button.Filled}">
			<Setter Property="FontSize" Value="14"/>
			<Setter Property="Height"   Value="44"/>
		</Style>
	</ResourceDictionary>
</Application.Resources>
```

> **关键**：自定义样式必须放在 `MergedDictionaries` 之后（`ResourceDictionary` 内直接声明），  
> 这样 WPF 的资源查找优先级才会优先命中你的覆盖样式。

### 方式二：在页面/控件级别覆盖

```xml
<Page.Resources>
	<Style x:Key="MyButton" TargetType="{x:Type mine:Button}"
		   BasedOn="{StaticResource Mine.Style.Button.Outlined}">
		<Setter Property="CornerRadius" Value="8"/>
	</Style>
</Page.Resources>

<mine:Button Style="{StaticResource MyButton}" Content="自定义按钮"/>
```

### 方式三：通过 Variant 在控件级别切换

对于按钮，`Variant` 属性已经内置了所有官方变体的完整样式切换：

```xml
<mine:Button Variant="Tonal" Content="次要操作"/>
<mine:Button Variant="Text"  Content="文字操作"/>
```

---

## 自定义 ControlTemplate

当你需要完全掌控控件的视觉结构时，可以提供自己的 `ControlTemplate`。  
以下示例为 `Button` 提供一个去掉水波纹、使用渐变背景的自定义模板：

```xml
<ControlTemplate x:Key="MyGradientButtonTemplate"
				 TargetType="{x:Type mine:Button}">
	<Border x:Name="PART_Root"
			CornerRadius="{TemplateBinding CornerRadius}"
			BorderBrush="{TemplateBinding BorderBrush}"
			BorderThickness="{TemplateBinding BorderThickness}"
			SnapsToDevicePixels="True">
		<Border.Background>
			<LinearGradientBrush StartPoint="0,0" EndPoint="1,1">
				<GradientStop Color="{DynamicResource Mine.Color.Primary}"   Offset="0"/>
				<GradientStop Color="{DynamicResource Mine.Color.Tertiary}" Offset="1"/>
			</LinearGradientBrush>
		</Border.Background>
		<ContentPresenter HorizontalAlignment="Center"
						  VerticalAlignment="Center"
						  Margin="{TemplateBinding Padding}"/>
	</Border>
	<ControlTemplate.Triggers>
		<Trigger Property="IsEnabled" Value="False">
			<Setter TargetName="PART_Root" Property="Opacity" Value="0.38"/>
		</Trigger>
	</ControlTemplate.Triggers>
</ControlTemplate>

<mine:Button Template="{StaticResource MyGradientButtonTemplate}"
			 Content="渐变按钮" Foreground="White"/>
```

### 保留 PART_Ripple

若需要保留水波纹效果，在模板中包含 `RippleDecorator`，并命名为 `PART_Ripple`：

```xml
<ControlTemplate TargetType="{x:Type mine:Button}">
	<mine:RippleDecorator x:Name="PART_Ripple"
						  RippleBrush="{TemplateBinding Foreground}"
						  CornerRadius="{TemplateBinding CornerRadius}">
		<!-- 你的自定义内容 -->
	</mine:RippleDecorator>
</ControlTemplate>
```

---

## 利用 RippleDecorator 添加水波纹

`RippleDecorator` 是一个通用装饰器，可以包裹**任意控件**使其获得水波纹效果：

```xml
<mine:RippleDecorator CornerRadius="12">
	<Grid Width="120" Height="60" Background="{DynamicResource Mine.Brush.SurfaceContainerHigh}">
		<TextBlock Text="点击我" HorizontalAlignment="Center" VerticalAlignment="Center"/>
	</Grid>
</mine:RippleDecorator>
```

### RippleDecorator 属性

| 属性 | 类型 | 默认值 | 说明 |
|------|------|--------|------|
| `RippleColor` | `Color` | `rgba(255,255,255,0.12)` | 水波纹颜色 |
| `RippleBrush` | `Brush?` | `null` | 从画笔提取颜色（优先于 `RippleColor`） |
| `IsCentered` | `bool` | `false` | 是否从中心展开 |
| `CornerRadius` | `CornerRadius` | `0` | 圆角（同时裁剪水波纹边界） |

---

## 创建遵循库规范的新控件

以创建一个 `TagLabel`（标签标注）控件为例，展示完整的新控件开发流程。

### 步骤一：编写 C# 行为层

```csharp
// Controls/TagLabel.cs
using System.Windows;
using System.Windows.Controls;

namespace Mine.Wpf.Controls.Controls;

[TemplatePart(Name = "PART_CloseButton", Type = typeof(ButtonBase))]
public class TagLabel : ContentControl
{
	static TagLabel()
	{
		DefaultStyleKeyProperty.OverrideMetadata(
			typeof(TagLabel),
			new FrameworkPropertyMetadata(typeof(TagLabel)));
	}

	// ── Color ────────────────────────────────────────────────────
	public static readonly DependencyProperty LabelColorProperty =
		DependencyProperty.Register(nameof(LabelColor), typeof(System.Windows.Media.Brush),
			typeof(TagLabel), new PropertyMetadata(null));

	public System.Windows.Media.Brush? LabelColor
	{
		get => (System.Windows.Media.Brush?)GetValue(LabelColorProperty);
		set => SetValue(LabelColorProperty, value);
	}

	// ── CanClose ─────────────────────────────────────────────────
	public static readonly DependencyProperty CanCloseProperty =
		DependencyProperty.Register(nameof(CanClose), typeof(bool),
			typeof(TagLabel), new PropertyMetadata(false));

	public bool CanClose
	{
		get => (bool)GetValue(CanCloseProperty);
		set => SetValue(CanCloseProperty, value);
	}

	// ── Closed Event ─────────────────────────────────────────────
	public static readonly RoutedEvent ClosedEvent =
		EventManager.RegisterRoutedEvent(nameof(Closed), RoutingStrategy.Bubble,
			typeof(RoutedEventHandler), typeof(TagLabel));

	public event RoutedEventHandler Closed
	{
		add    => AddHandler(ClosedEvent, value);
		remove => RemoveHandler(ClosedEvent, value);
	}

	public override void OnApplyTemplate()
	{
		base.OnApplyTemplate();
		if (GetTemplateChild("PART_CloseButton") is ButtonBase btn)
			btn.Click += (_, _) => RaiseEvent(new RoutedEventArgs(ClosedEvent));
	}
}
```

### 步骤二：编写 XAML 视觉层

```xml
<!-- Themes/Controls/TagLabel.xaml -->
<ResourceDictionary xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
					xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
					xmlns:controls="clr-namespace:Mine.Wpf.Controls.Controls">

	<Style TargetType="{x:Type controls:TagLabel}">
		<Setter Property="Padding"       Value="8,4"/>
		<Setter Property="Foreground"    Value="{DynamicResource Mine.Brush.OnSecondaryContainer}"/>
		<Setter Property="Background"    Value="{DynamicResource Mine.Brush.SecondaryContainer}"/>
		<Setter Property="FontSize"      Value="12"/>
		<Setter Property="Template">
			<Setter.Value>
				<ControlTemplate TargetType="{x:Type controls:TagLabel}">
					<Border Background="{TemplateBinding Background}"
							Padding="{TemplateBinding Padding}"
							CornerRadius="{DynamicResource Mine.Shape.Full}">
						<StackPanel Orientation="Horizontal" Spacing="4">
							<ContentPresenter VerticalAlignment="Center"/>
							<Button x:Name="PART_CloseButton"
									Style="{StaticResource Mine.Style.Button.Icon}"
									Width="16" Height="16"
									Padding="0"
									Icon="&#xE5CD;"
									Visibility="Collapsed"/>
						</StackPanel>
					</Border>
					<ControlTemplate.Triggers>
						<Trigger Property="CanClose" Value="True">
							<Setter TargetName="PART_CloseButton" Property="Visibility" Value="Visible"/>
						</Trigger>
					</ControlTemplate.Triggers>
				</ControlTemplate>
			</Setter.Value>
		</Setter>
	</Style>

</ResourceDictionary>
```

### 步骤三：注册到 ThemeDictionary

```csharp
// Theming/ThemeDictionary.cs — 在构造函数末尾追加
MergedDictionaries.Add(Load("Themes/Controls/TagLabel.xaml"));
```

### 步骤四：使用

```xml
<mine:TagLabel Content="新功能" CanClose="True" Closed="OnTagClosed"/>
```

---

## 在模板中使用主题资源令牌

所有主题令牌均通过 `{DynamicResource}` 引用，主题切换时自动更新，无需订阅任何事件：

```xml
<!-- ✅ 推荐：使用 DynamicResource，支持运行时主题切换 -->
<Border Background="{DynamicResource Mine.Brush.SurfaceContainerLow}"
		BorderBrush="{DynamicResource Mine.Brush.Outline}"/>

<!-- ❌ 避免：使用 StaticResource 引用动态令牌，主题切换后不会更新 -->
<Border Background="{StaticResource Mine.Brush.SurfaceContainerLow}"/>
```

### 半透明状态层（State Layer）

Material 3 的 Hover / Pressed 状态通过在内容上叠加半透明颜色层实现：

```xml
<!-- 状态层 Border，叠在内容之上 -->
<Border x:Name="StateLayer" Background="Transparent">
	<Border.CornerRadius><!-- 同父容器 --></Border.CornerRadius>
</Border>

<!-- 触发器 -->
<Trigger Property="IsMouseOver" Value="True">
	<Setter TargetName="StateLayer" Property="Background">
		<Setter.Value>
			<SolidColorBrush Color="{DynamicResource Mine.Color.Primary}" Opacity="0.08"/>
		</Setter.Value>
	</Setter>
</Trigger>
<Trigger Property="IsPressed" Value="True">
	<Setter TargetName="StateLayer" Property="Background">
		<Setter.Value>
			<SolidColorBrush Color="{DynamicResource Mine.Color.Primary}" Opacity="0.12"/>
		</Setter.Value>
	</Setter>
</Trigger>
```

状态层不透明度标准（MD3）：

| 状态 | 不透明度 |
|------|---------|
| Hover | 8%（0.08） |
| Pressed | 12%（0.12） |
| Focus | 12%（0.12） |
| Dragged | 16%（0.16） |

---

## TemplatePart 约定

所有具名模板部件必须遵循以下规范：

1. **名称以 `PART_` 开头**，全大写，下划线分隔（如 `PART_CloseButton`）。
2. 在控件类上通过 `[TemplatePart]` 特性声明，以便 XAML 设计器验证。
3. 在 `OnApplyTemplate()` 中通过 `GetTemplateChild(name)` 获取，并判空后再操作。
4. 模板部件是**可选的**——即使 XAML 作者删除某个 `PART_`，控件不应崩溃（仅功能降级）。

```csharp
[TemplatePart(Name = "PART_CloseButton", Type = typeof(ButtonBase))]
public class MyControl : Control
{
	private ButtonBase? _closeBtn;   // 字段存储，避免重复查找

	public override void OnApplyTemplate()
	{
		base.OnApplyTemplate();

		// 先解绑旧实例（防止模板热重载时重复订阅）
		if (_closeBtn != null)
			_closeBtn.Click -= OnClose;

		_closeBtn = GetTemplateChild("PART_CloseButton") as ButtonBase;

		if (_closeBtn != null)
			_closeBtn.Click += OnClose;
	}

	private void OnClose(object sender, RoutedEventArgs e) { /* ... */ }
}
```

---

## 常见扩展场景示例

### 场景一：为 TextBox 添加字数统计

```xml
<Style TargetType="{x:Type mine:TextBox}"
	   BasedOn="{StaticResource {x:Type mine:TextBox}}">
	<Setter Property="Template">
		<!-- 在原有模板基础上，于底部追加字数显示 -->
		...
	</Setter>
</Style>
```

### 场景二：创建带图标的 ListBoxItem

```xml
<Style TargetType="{x:Type ListBoxItem}">
	<Setter Property="Template">
		<Setter.Value>
			<ControlTemplate TargetType="ListBoxItem">
				<mine:RippleDecorator CornerRadius="8">
					<Border Padding="12,10" Background="Transparent">
						<StackPanel Orientation="Horizontal" Spacing="12">
							<mine:MaterialIcon Kind="{Binding Icon}" Size="20"
											   Foreground="{DynamicResource Mine.Brush.OnSurfaceVariant}"/>
							<ContentPresenter VerticalAlignment="Center"/>
						</StackPanel>
					</Border>
				</mine:RippleDecorator>
				<ControlTemplate.Triggers>
					<Trigger Property="IsSelected" Value="True">
						<!-- 选中高亮 -->
					</Trigger>
				</ControlTemplate.Triggers>
			</ControlTemplate>
		</Setter.Value>
	</Setter>
</Style>
```

### 场景三：扩展 Button，添加确认步骤

```csharp
public class ConfirmButton : Button
{
	static ConfirmButton()
		=> DefaultStyleKeyProperty.OverrideMetadata(
			   typeof(ConfirmButton),
			   new FrameworkPropertyMetadata(typeof(ConfirmButton)));

	private bool _waitingConfirm;

	protected override void OnClick()
	{
		if (!_waitingConfirm)
		{
			_waitingConfirm = true;
			Content = "再次点击确认";
			// 3 秒后自动重置
			Task.Delay(3000).ContinueWith(_ =>
				Dispatcher.Invoke(() => { Content = _originalContent; _waitingConfirm = false; }));
		}
		else
		{
			_waitingConfirm = false;
			base.OnClick(); // 真正触发 Click 事件
		}
	}
}
```

---

上一章：[附加行为与服务 ←](05-attached-behaviors.md)　　下一章：[图标系统 →](07-icons.md)
