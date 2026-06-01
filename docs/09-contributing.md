# 贡献指南

本章说明如何向 Mine.Wpf.Controls 贡献代码：从开发环境搭建，到编码规范、提交约定，  
再到 PR 审查流程。

---

## 目录

1. [开发环境准备](#开发环境准备)
2. [分支策略](#分支策略)
3. [编码规范](#编码规范)
4. [新增控件的完整流程](#新增控件的完整流程)
5. [提交信息规范](#提交信息规范)
6. [常见问题排查](#常见问题排查)

---

## 开发环境准备

| 工具 | 版本 |
|------|------|
| Visual Studio | 2022 17.x 或 2026 18.x（含 WPF 工作负载） |
| .NET SDK | 8.0+ |
| Git | 2.x |

克隆仓库：

```powershell
git clone <仓库地址>
cd Mine.Wpf.Controls
```

打开解决方案：

```powershell
start Mine.Wpf.Controls.sln
```

或直接用 VS 打开 `.sln` 文件。首次构建会自动还原（无外部 NuGet 依赖）。

---

## 分支策略

| 分支 | 用途 |
|------|------|
| `main` | 稳定发布分支，不直接提交 |
| `develop` | 日常开发合并目标 |
| `feature/<控件名>` | 新控件开发 |
| `fix/<问题简述>` | Bug 修复 |
| `docs/<文档范围>` | 文档更新 |
| `chore/<任务简述>` | 工程化/配置/依赖管理 |

新功能从 `develop` 切出，完成后向 `develop` 提 PR；稳定后再从 `develop` 合并到 `main`。

---

## 编码规范

### C# 部分

1. **作用域最小化**：依赖属性的 `PropertyChangedCallback` 优先写为私有静态方法。
2. **空安全**：启用 `Nullable`，所有可为 null 的引用类型显式标注 `?`。
3. **模板部件获取**：在 `OnApplyTemplate()` 中获取模板部件，获取前先解绑旧实例，获取后判空再操作。
4. **不得修改共享控件**：为某个特定消费场景修改共享控件（`Badge`、`Button` 等）时，  
   必须确保改动对所有使用方向下兼容。如不能保证，改为添加新的可选属性或附加行为。
5. **事件引发**：Routed Event 使用 `RaiseEvent`；CLR 事件使用标准 `?.Invoke`。

### XAML 部分

1. **始终使用 `{DynamicResource}` 引用主题令牌**，禁止使用 `{StaticResource}` 引用颜色/形状类令牌（会导致主题切换失效）。
2. **模板部件命名**：必须以 `PART_` 前缀开头，PascalCase，全程大写前缀（如 `PART_CloseButton`）。
3. **状态层（State Layer）不透明度**：Hover=0.08，Pressed=0.12，遵循 MD3 规范。
4. **样式键命名规则**：
   - 控件模板：`Mine.Template.<ControlName>`
   - 控件样式变体：`Mine.Style.<ControlName>.<Variant>`
   - 内部共享样式（不对外）：`Mine.Internal.<描述>`
5. **不得在控件模板中硬编码颜色值**，所有颜色必须通过动态资源引用。

### 资源令牌命名

| 类别 | 格式 | 示例 |
|------|------|------|
| 颜色 | `Mine.Color.<Role>` | `Mine.Color.Primary` |
| 画笔 | `Mine.Brush.<Role>` | `Mine.Brush.OnSurface` |
| 形状 | `Mine.Shape.<Size>` | `Mine.Shape.Medium` |
| 阴影 | `Mine.Elevation.<0-5>` | `Mine.Elevation.2` |
| 字体大小 | `Mine.FontSize.<Scale>` | `Mine.FontSize.BodyLarge` |
| 字体族 | `Mine.FontFamily.<Name>` | `Mine.FontFamily.Default` |
| 动效 | `Mine.Motion.<Token>` | `Mine.Motion.EasingStandard` |

---

## 新增控件的完整流程

以下是向库添加新控件的标准步骤，缺一不可。

### 1. 创建 C# 行为层

路径：`Mine.Wpf.Controls/Controls/<ControlName>.cs`

```csharp
namespace Mine.Wpf.Controls.Controls;

[TemplatePart(Name = "PART_Root", Type = typeof(Border))]
public class MyControl : Control
{
	static MyControl()
	{
		DefaultStyleKeyProperty.OverrideMetadata(
			typeof(MyControl),
			new FrameworkPropertyMetadata(typeof(MyControl)));
	}

	// 在此声明依赖属性、路由事件、模板逻辑
}
```

### 2. 创建 XAML 视觉层

路径：`Mine.Wpf.Controls/Themes/Controls/<ControlName>.xaml`

```xml
<ResourceDictionary xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
					xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
					xmlns:controls="clr-namespace:Mine.Wpf.Controls.Controls">
	<Style TargetType="{x:Type controls:MyControl}">
		<!-- ... -->
	</Style>
</ResourceDictionary>
```

### 3. 注册到 ThemeDictionary

在 `Mine.Wpf.Controls/Theming/ThemeDictionary.cs` 的构造函数末尾追加：

```csharp
MergedDictionaries.Add(Load("Themes/Controls/MyControl.xaml"));
```

### 4. 注册到 Generic.xaml（双保险）

`Mine.Wpf.Controls/Themes/Generic.xaml` 也需合并该字典：

```xml
<ResourceDictionary.MergedDictionaries>
	<!-- ... 现有条目 ... -->
	<ResourceDictionary Source="/Mine.Wpf.Controls;component/Themes/Controls/MyControl.xaml"/>
</ResourceDictionary.MergedDictionaries>
```

> `Generic.xaml` 是 WPF 运行时按 `DefaultStyleKey` 自动查找样式的入口；  
> `ThemeDictionary` 是应用级引导路径。两者都需要注册，确保在所有场景下样式均可找到。

### 5. 创建 Gallery 演示页

路径：`samples/Mine.Wpf.Controls.Gallery/Pages/<ControlName>Page.xaml/.cs`  
并在 `MainWindow.xaml.cs` 的 `NavItems` 中注册。

### 6. 编写文档

在 `docs/03-controls.md` 中添加对应小节，参照现有格式提供：
- 属性表
- 变体/枚举说明（如适用）
- XAML 用法示例
- 代码用法示例（如适用）

### 7. 构建验证

```powershell
dotnet build Mine.Wpf.Controls.sln
```

确认零错误、零警告后提交。

---

## 提交信息规范

遵循 [Conventional Commits](https://www.conventionalcommits.org/) 规范：

```
<type>(<scope>): <简短描述>

[可选正文：详细说明]

[可选 footer：Breaking Change / 关联 Issue]
```

### 类型（type）

| 类型 | 说明 |
|------|------|
| `feat` | 新功能（新控件、新属性） |
| `fix` | Bug 修复 |
| `docs` | 文档更新 |
| `style` | 代码格式调整（不影响逻辑） |
| `refactor` | 重构（不修改功能或 Bug） |
| `perf` | 性能优化 |
| `test` | 测试相关 |
| `chore` | 工程化/构建/依赖管理 |

### 作用域（scope）

使用控件名或模块名：`button`、`toast`、`theming`、`badge`、`docs`、`gallery` 等。

### 示例

```
feat(toast): add NotificationCenter with flyout-based history panel

fix(button): remove noisy DataTrigger bindings for Icon/Content null check

docs(theming): add dynamic resource token reference table

chore: ignore WPF temp project files and screenshots
```

### Breaking Change

```
feat(thememanager)!: rename ApplySeedColor to ApplyTheme

BREAKING CHANGE: ApplySeedColor 已重命名为 ApplyTheme，调用方需更新。
```

---

## 常见问题排查

### 构建错误：`CS2001` / `BG1002`（缺少 .g.cs 或 .baml）

这是 WPF MSBuild 生成步骤在 `obj/` 目录下的临时文件被锁定或损坏导致的。

解决方案：

```powershell
# 方案 A：清理重建
dotnet clean
dotnet build

# 方案 B：手动删除 obj 目录
Remove-Item -Recurse -Force Mine.Wpf.Controls\obj
Remove-Item -Recurse -Force samples\Mine.Wpf.Controls.Gallery\obj
dotnet build
```

### 运行时警告：`Cannot find resource named 'Mine.Brush.xxx'`

原因：`ThemeDictionary` 未在 `App.xaml` 的 `Application.Resources` 中声明，或声明顺序不正确。

检查清单：
1. `App.xaml` 中 `<mine:ThemeDictionary .../>` 是 `MergedDictionaries` 中的**第一项**。
2. 自定义资源字典在 `ThemeDictionary` **之后**声明。
3. 使用的是 `{DynamicResource}` 而非 `{StaticResource}`。

### 主题切换后颜色未更新

原因：使用了 `{StaticResource}` 引用颜色/画笔令牌，静态资源在首次解析后不再响应资源变化。

解决：将所有 `Mine.Color.*` 和 `Mine.Brush.*` 的引用改为 `{DynamicResource}`。

### Toast 显示在错误的窗口

`ToastService` 使用 `WindowOverlay.GetActiveWindow()` 获取当前活动窗口。  
如果应用同时存在多个顶层窗口且焦点不在预期窗口上，Toast 可能显示在错误位置。

临时解决：在调用 `ToastService.Show` 前先激活目标窗口：

```csharp
targetWindow.Activate();
ToastService.Success("操作完成");
```

### NavigationView 页面切换后内容为空

原因：`Page` 类型未在 `NavItems` 数组中正确注册，或页面类的命名空间/类名拼写错误。

检查：确认 `typeof(YourPage)` 与实际类名完全一致，并且 XAML 的 `x:Class` 属性正确对应。

---

上一章：[Gallery 示例应用指南 ←](08-gallery.md)
