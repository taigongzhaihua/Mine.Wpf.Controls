# 快速入门

本章带你从零完成 **Mine.Wpf.Controls** 的引用、引导配置，并渲染出第一个 Material 3 按钮。

---

## 目录

1. [环境要求](#环境要求)
2. [添加项目引用](#添加项目引用)
3. [引导主题系统](#引导主题系统)
4. [添加命名空间声明](#添加命名空间声明)
5. [渲染第一个控件](#渲染第一个控件)
6. [项目结构一览](#项目结构一览)

---

## 环境要求

| 项目 | 最低要求 |
|------|----------|
| .NET | 8.0-windows |
| Windows | 10 1809+ |
| WPF | 随 .NET 8 附带 |

> 库本身不依赖任何第三方 NuGet 包，所有渲染逻辑均基于 WPF 原生 API。

---

## 添加项目引用

如果你正在同一解决方案下开发，直接添加项目引用：

```xml
<!-- YourApp.csproj -->
<ItemGroup>
  <ProjectReference Include="..\Mine.Wpf.Controls\Mine.Wpf.Controls.csproj" />
</ItemGroup>
```

发布为 NuGet 包后，改为：

```xml
<PackageReference Include="Mine.Wpf.Controls" Version="*" />
```

---

## 引导主题系统

Mine.Wpf.Controls 的全部样式都托管在 `ThemeDictionary` 这个特殊的 `ResourceDictionary` 子类中。  
必须在 `App.xaml` 的 `Application.Resources` 内将其声明为第一个合并字典，才能让隐式样式和动态资源正确解析。

```xml
<!-- App.xaml -->
<Application xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
			 xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
			 xmlns:mine="https://schemas.mine.io/wpf"
			 StartupUri="MainWindow.xaml">
	<Application.Resources>
		<ResourceDictionary>
			<ResourceDictionary.MergedDictionaries>

				<!-- ① Mine 主题字典——必须是第一项 -->
				<mine:ThemeDictionary Mode="System" SeedColor="#6750A4"/>

				<!-- ② 你自己的资源字典（可选）-->
				<!-- <ResourceDictionary Source="Themes/MyStyles.xaml"/> -->

			</ResourceDictionary.MergedDictionaries>
		</ResourceDictionary>
	</Application.Resources>
</Application>
```

### `ThemeDictionary` 关键属性

| 属性 | 类型 | 说明 |
|------|------|------|
| `Mode` | `ThemeMode` | `Light` / `Dark` / `System`（跟随系统，默认） |
| `SeedColor` | `Color` | MD3 种子色，库会自动生成完整色调调色板 |
| `Preset` | `ThemePreset` | 使用官方预设色系（设置后 `SeedColor` 失效） |

两种用法互斥：

```xml
<!-- 方案 A：自定义种子色 -->
<mine:ThemeDictionary Mode="System" SeedColor="#0070F3"/>

<!-- 方案 B：使用预设主题 -->
<mine:ThemeDictionary Mode="Light" Preset="Teal"/>
```

> **内部机制**：`ThemeDictionary` 在构造时加载字体、颜色、形状、阴影、动效、转换器等共享资源，
> 以及所有控件模板字典。它同时向 `ThemeManager` 注册自身，使后续运行时主题切换能够正常工作。

---

## 添加命名空间声明

在每个需要使用库控件的 XAML 文件顶部，声明命名空间别名：

```xml
xmlns:mine="https://schemas.mine.io/wpf"
```

该 URI 在程序集的 `AssemblyInfo.cs` 中通过 `XmlnsDefinition` 映射到以下 CLR 命名空间：

- `Mine.Wpf.Controls.Controls` — 所有控件类
- `Mine.Wpf.Controls.Theming` — `ThemeDictionary`、`ThemeManager` 等
- `Mine.Wpf.Controls.Attached` — 附加行为（`FlyoutService` 等）
- `Mine.Wpf.Controls.Primitives` — `RippleDecorator` 等原语

---

## 渲染第一个控件

```xml
<Window xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
		xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
		xmlns:mine="https://schemas.mine.io/wpf"
		Title="Hello Mine" Width="400" Height="300">
	<StackPanel HorizontalAlignment="Center" VerticalAlignment="Center" Spacing="16">

		<!-- 默认 Filled 按钮 -->
		<mine:Button Content="开始使用"/>

		<!-- 带图标的 Filled 按钮 -->
		<mine:Button Content="下载" Icon="&#xF090;"/>

		<!-- Outlined 变体 -->
		<mine:Button Content="取消" Variant="Outlined"/>

		<!-- Text 变体 -->
		<mine:Button Content="了解更多" Variant="Text"/>

		<!-- 加载中状态 -->
		<mine:Button Content="提交中" IsLoading="True"/>

	</StackPanel>
</Window>
```

无需任何额外 Style 声明——控件的隐式样式已由 `ThemeDictionary` 注入到 `Application.Resources`。

---

## 项目结构一览

```
Mine.Wpf.Controls/
├── Animations/          # 动效时间常数（MotionTokens）
├── Attached/            # 附加行为（FlyoutService、RippleAssist 等）
├── Controls/            # 所有控件的 C# 逻辑层
├── Converters/          # 值转换器（由 Themes/Resources/Converters.xaml 注册）
├── Fonts/               # Material Symbols Rounded 字体文件（.ttf）
├── Helpers/             # 内部工具类（WindowOverlay、ClipHelper）
├── Icons/               # MaterialIcons 常量类（图标名 → Unicode 码点）
├── Markup/              # XAML 标记扩展
├── Primitives/          # 底层渲染原语（RippleDecorator、NotchedOutlineBorder）
├── Theming/             # 主题系统（ThemeManager、ThemeDictionary、ColorScheme 等）
└── Themes/
	├── Generic.xaml         # WPF 隐式样式入口（合并所有控件模板）
	├── Controls/            # 每个控件独立的 .xaml 模板文件
	└── Resources/           # 颜色、形状、阴影、字体、动效等共享令牌
		├── Colors.Light.xaml
		├── Colors.Dark.xaml
		├── ColorBrushes.xaml
		├── Elevation.xaml
		├── Shape.xaml
		├── Typography.xaml
		├── Motion.xaml
		└── Presets/         # 官方预设主题的颜色文件（每个色系各一 Light/Dark 对）
```

> **关键区分**：`Controls/` 目录存放行为逻辑（依赖属性、命令、动画），  
> `Themes/Controls/` 目录存放视觉模板（XAML ControlTemplate、Style）。  
> 两者通过 `DefaultStyleKey` 和隐式样式自动绑定，无需开发者手工关联。

---

下一章：[主题系统详解 →](02-theming.md)
