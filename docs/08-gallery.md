# Gallery 示例应用指南

`Mine.Wpf.Controls.Gallery` 是配套的演示应用，展示了所有控件的实际效果、变体选项和交互行为。  
本章讲解 Gallery 项目的结构、如何在本地运行，以及如何将其作为新控件演示页的开发模板。

---

## 目录

1. [项目结构](#项目结构)
2. [在本地运行 Gallery](#在本地运行-gallery)
3. [导航结构与页面注册](#导航结构与页面注册)
4. [新增演示页面](#新增演示页面)
5. [Gallery 的主题切换面板](#gallery-的主题切换面板)
6. [页面开发约定](#页面开发约定)

---

## 项目结构

```
samples/Mine.Wpf.Controls.Gallery/
├── App.xaml                   # 应用入口，声明 ThemeDictionary
├── App.xaml.cs
├── MainWindow.xaml            # 主窗口：NavigationView + 主题切换面板
├── MainWindow.xaml.cs         # 导航项注册、主题切换逻辑
├── BindingProxy.cs            # 用于 DataTemplate 内穿透 DataContext 的工具类
└── Pages/
	├── ButtonPage.xaml/.cs    # Button 演示页
	├── BadgePage.xaml/.cs
	├── AvatarPage.xaml/.cs
	├── ChipPage.xaml/.cs
	├── CardPage.xaml/.cs
	├── ToastPage.xaml/.cs     # Toast / NotificationCenter 演示页
	├── FeedbackPage.xaml/.cs  # Snackbar / Dialog 演示页
	├── FlyoutPage.xaml/.cs
	├── ColorPage.xaml/.cs     # 颜色令牌可视化
	├── IconPage.xaml/.cs      # 图标库浏览
	├── TypographyPage.xaml/.cs
	└── ...（其余演示页）
```

---

## 在本地运行 Gallery

### 使用 Visual Studio

1. 打开 `Mine.Wpf.Controls.sln`；
2. 将 `Mine.Wpf.Controls.Gallery` 设为启动项目（右键 → 设为启动项目）；
3. 按 `F5` 运行。

### 使用命令行

```powershell
cd samples\Mine.Wpf.Controls.Gallery
dotnet run
```

### 构建注意事项

- Gallery 依赖控件库（`ProjectReference`），首次构建时会同时编译两个项目。
- WPF 项目在 `obj/` 目录下生成临时 XAML 编译文件（`.g.cs`、`.baml`），**不应提交到 Git**（已在 `.gitignore` 中排除）。
- 如果遇到 `BG1002`（缺少 `.baml`）或 `CS2001`（缺少 `.g.cs`）错误，执行以下清理步骤：

```powershell
# 清理并重新构建
dotnet clean
dotnet build
```

---

## 导航结构与页面注册

`MainWindow.xaml.cs` 中通过静态数组 `NavItems` 集中注册所有演示页：

```csharp
private static readonly (string Icon, string Label, Type Page)[] NavItems =
[
	("\uF1C1", "Button",               typeof(ButtonPage)),
	("\uE399", "Badge",                typeof(BadgePage)),
	("\uEA22", "Toast / Notification", typeof(ToastPage)),
	// ... 其他页面
];
```

每个元素包含：

| 字段 | 说明 |
|------|------|
| `Icon` | Material Symbols Unicode 码点（显示在导航菜单图标位置） |
| `Label` | 导航菜单文字 |
| `Page` | 对应演示页面的 `Type`（运行时通过 `Activator.CreateInstance` 实例化） |

页面切换时，`NavigationView` 的 `SelectionChanged` 事件处理器将目标页面的 `Type` 传递给内部 `Frame`，Frame 自动导航到对应页面类型。

---

## 新增演示页面

以新增 `RatingPage`（评分控件演示）为例：

### 步骤一：创建 XAML 页面文件

```xml
<!-- samples/Mine.Wpf.Controls.Gallery/Pages/RatingPage.xaml -->
<Page xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
	  xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
	  xmlns:mine="https://schemas.mine.io/wpf"
	  x:Class="Mine.Wpf.Controls.Gallery.Pages.RatingPage"
	  Title="Rating">

	<ScrollViewer Padding="24">
		<StackPanel Spacing="32" MaxWidth="800">

			<!-- 页面标题 -->
			<TextBlock Text="Rating" Style="{DynamicResource Mine.Style.Text.DisplaySmall}"/>

			<!-- 演示区块 -->
			<StackPanel Spacing="16">
				<TextBlock Text="基础用法" Style="{DynamicResource Mine.Style.Text.TitleMedium}"/>
				<mine:Rating Value="3" Maximum="5"/>
			</StackPanel>

			<StackPanel Spacing="16">
				<TextBlock Text="只读" Style="{DynamicResource Mine.Style.Text.TitleMedium}"/>
				<mine:Rating Value="4" IsReadOnly="True"/>
			</StackPanel>

		</StackPanel>
	</ScrollViewer>
</Page>
```

### 步骤二：创建代码后置文件

```csharp
// samples/Mine.Wpf.Controls.Gallery/Pages/RatingPage.xaml.cs
namespace Mine.Wpf.Controls.Gallery.Pages;

public partial class RatingPage : System.Windows.Controls.Page
{
	public RatingPage() => InitializeComponent();
}
```

### 步骤三：注册到 NavItems

在 `MainWindow.xaml.cs` 的 `NavItems` 数组中追加：

```csharp
("\uE838", "Rating", typeof(RatingPage)),
```

> `\uE838` 是 Material Symbols 中 `star` 图标的 Unicode 码点，可在图标速查表中查找。

---

## Gallery 的主题切换面板

Gallery 主窗口的右上角内置了完整的主题切换面板，提供以下交互：

| 控件 | 功能 |
|------|------|
| `Switch`（亮/暗） | 调用 `ThemeManager.ToggleLightDark()` |
| 种子色色板 | 调用 `ThemeManager.ApplyTheme(mode, seedColor)` |
| 预设色系选择 | 调用 `ThemeManager.ApplyPresetTheme(mode, preset)` |

### 状态同步机制

```csharp
// MainWindow.xaml.cs
ThemeManager.ThemeChanged += (_, _) => Dispatcher.BeginInvoke(() =>
{
	SyncDarkModeSwitch();  // 同步 Switch 控件状态（避免循环触发）
	SyncThemePanel();      // 同步预设高亮状态
});
```

`SyncDarkModeSwitch` 在更新 `IsChecked` 前先解绑事件，更新后再重新绑定，  
确保代码赋值不会触发 `Checked` 事件引发循环调用——这是 WPF 中处理双向同步的常见模式。

---

## 页面开发约定

Gallery 中的每个演示页面遵循以下约定，以保持视觉一致性：

### 布局结构

```xml
<Page ...>
	<ScrollViewer Padding="24">
		<StackPanel Spacing="32" MaxWidth="800">

			<!-- 1. 页面标题 -->
			<TextBlock Text="控件名称" Style="{DynamicResource Mine.Style.Text.DisplaySmall}"/>

			<!-- 2. 简短描述（可选） -->
			<TextBlock Text="描述..." TextWrapping="Wrap"
					   Style="{DynamicResource Mine.Style.Text.BodyLarge}"
					   Foreground="{DynamicResource Mine.Brush.OnSurfaceVariant}"/>

			<!-- 3. 演示区块（可重复） -->
			<mine:Card Padding="24">
				<StackPanel Spacing="16">
					<TextBlock Text="区块标题" Style="{DynamicResource Mine.Style.Text.TitleMedium}"/>
					<!-- 演示内容 -->
				</StackPanel>
			</mine:Card>

		</StackPanel>
	</ScrollViewer>
</Page>
```

### 代码后置约定

- 仅在必要时才有代码后置逻辑（大多数页面只有 `InitializeComponent()`）；
- 事件处理方法保持简洁，复杂逻辑提取到 `ViewModel` 或服务层；
- 不应在 Gallery 页面中修改控件库的全局状态。

### 字体样式速查

| 样式键 | 用途 |
|--------|------|
| `Mine.Style.Text.DisplaySmall` | 页面主标题 |
| `Mine.Style.Text.HeadlineMedium` | 章节标题 |
| `Mine.Style.Text.TitleMedium` | 演示区块标题 |
| `Mine.Style.Text.BodyLarge` | 正文描述 |
| `Mine.Style.Text.LabelSmall` | 注释、标注 |

---

上一章：[图标系统 ←](07-icons.md)　　下一章：[贡献指南 →](09-contributing.md)
