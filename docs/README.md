# Mine.Wpf.Controls 开发者文档

基于 **Material Design 3** 规范的 WPF 控件库，支持动态颜色、亮/暗主题、Material Symbols 图标。

---

## 文档目录

| 章节 | 内容摘要 |
|------|----------|
| [01 — 快速入门](01-getting-started.md) | 环境要求、添加引用、引导主题、渲染第一个控件 |
| [02 — 主题系统详解](02-theming.md) | 动态颜色原理、ThemeDictionary、ThemeManager、资源令牌参考 |
| [03 — 控件参考](03-controls.md) | 所有控件的属性表、变体说明与完整用法示例 |
| [04 — Toast 与 NotificationCenter](04-toast.md) | 通知卡片架构、ToastService API、动画行为、集成示例 |
| [05 — 附加行为与服务](05-attached-behaviors.md) | FlyoutService、RippleAssist、HintAssist、IconAssist 等 |
| [06 — 自定义控件与模板扩展](06-customization.md) | 覆盖样式、自定义 ControlTemplate、创建新控件的规范流程 |
| [07 — 图标系统](07-icons.md) | Material Symbols 字体、MaterialIcon 控件、常量类与码点速查 |
| [08 — Gallery 示例应用指南](08-gallery.md) | 项目结构、运行方法、页面注册、演示页开发约定 |
| [09 — 贡献指南](09-contributing.md) | 分支策略、编码规范、新控件完整流程、提交信息规范 |

---

## 快速上手（一页纸版）

### 1. 在 App.xaml 中引导主题

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

### 2. 在 XAML 中使用控件

```xml
xmlns:mine="https://schemas.mine.io/wpf"

<mine:Button Content="开始使用"/>
<mine:Button Content="下载" Icon="&#xF090;" Variant="Tonal"/>
<mine:MaterialIcon Kind="Favorite" Size="24"/>
```

### 3. 运行时切换主题

```csharp
ThemeManager.ApplyTheme(ThemeMode.Dark, Color.FromRgb(0xFF, 0x57, 0x22));
ThemeManager.ToggleLightDark();
ThemeManager.ApplyPresetTheme(ThemeMode.System, ThemePreset.Teal);
```

### 4. 发送通知

```csharp
ToastService.Success("文件已保存");
ToastService.Error("连接失败", ex.Message);
ToastService.Warning("存储空间不足", "剩余低于 1 GB");
```

---

## 关键概念速查

| 概念 | 入口 |
|------|------|
| 所有主题令牌（颜色/形状/阴影） | [02 — 主题系统](02-theming.md#动态资源令牌参考) |
| Button 变体（Filled/Outlined/FAB 等） | [03 — 控件参考 · Button](03-controls.md#button按钮) |
| 将 Flyout 附加到按钮 | [05 — 附加行为 · FlyoutService](05-attached-behaviors.md#flyoutservice) |
| 为文本框添加浮动标签 | [05 — 附加行为 · HintAssist](05-attached-behaviors.md#hintassist) |
| 创建新控件的规范步骤 | [06 — 自定义](06-customization.md#创建遵循库规范的新控件) |
| 图标名称与码点对照 | [07 — 图标](07-icons.md#图标查找与常用图标速查) |
| 向 Gallery 添加演示页 | [08 — Gallery](08-gallery.md#新增演示页面) |

---

## 设计原则

- **主题令牌优先**：所有颜色、形状、阴影必须通过 `{DynamicResource Mine.xxx}` 引用，不硬编码。
- **行为与视觉分离**：C# 代码只处理逻辑和依赖属性，XAML 只处理视觉和动画。
- **渐进式扩展**：优先通过附加属性或变体属性扩展现有控件；仅在功能确实无法复用时才新增控件类。
- **向下兼容**：对任何已公开的依赖属性或路由事件的改动，必须保持向下兼容性。
