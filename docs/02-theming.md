# 主题系统详解

Mine.Wpf.Controls 的主题系统完整实现了 **Material Design 3 动态颜色（Dynamic Color）** 规范。  
本章从颜色生成原理出发，逐层介绍运行时切换、系统跟随、预设色系及资源令牌的使用方式。

---

## 目录

1. [设计原则：色调调色板与色彩角色](#设计原则色调调色板与色彩角色)
2. [ThemeDictionary：引导入口](#themedictionary引导入口)
3. [ThemeManager：运行时主题 API](#thememanager运行时主题-api)
4. [ThemeMode：亮色 / 暗色 / 跟随系统](#thememode亮色--暗色--跟随系统)
5. [SeedColor：自定义品牌色](#seedcolor自定义品牌色)
6. [ThemePreset：官方预设色系](#themepreset官方预设色系)
7. [动态资源令牌参考](#动态资源令牌参考)
8. [监听主题变化](#监听主题变化)
9. [常见用法示例](#常见用法示例)

---

## 设计原则：色调调色板与色彩角色

MD3 不要求设计师手工指定所有颜色，而是从单一的**种子色（Seed Color）** 通过 HCT 色彩空间算法自动生成一套完整的**色调调色板（Tonal Palette）**，再从调色板中提取约 40 个**颜色角色（Color Role）**，分别用于背景、前景、容器、轮廓等语义场景。

库在 `TonalPaletteGenerator` 中内置了完整的 HCT 生成算法，无需联网或外部依赖。

**色彩角色示例**（部分）：

| 角色 | 含义 |
|------|------|
| `Primary` | 主色，用于最重要的 UI 元素 |
| `OnPrimary` | 叠加在 Primary 上的内容色（通常为白/黑） |
| `PrimaryContainer` | 较浅的主色容器背景 |
| `OnPrimaryContainer` | 叠加在 PrimaryContainer 上的内容色 |
| `Surface` | 页面/卡片背景 |
| `SurfaceVariant` | 略有区别的 Surface 变体 |
| `Outline` | 控件边框色 |
| `Error` / `OnError` | 错误状态色对 |

完整角色列表见 [动态资源令牌参考](#动态资源令牌参考)。

---

## ThemeDictionary：引导入口

`ThemeDictionary` 继承自 `ResourceDictionary`，是整个主题系统的**唯一引导点**。  
在 `App.xaml` 中声明后，它自动完成以下工作：

1. 将 Material Symbols Rounded 字体注册到 `Mine.Font.MaterialSymbols` 资源键；
2. 按顺序合并颜色（Light/Dark）、字体、形状、阴影、动效、转换器字典；
3. 合并所有控件模板字典（Button、Card、TextBox 等共 30+ 个）；
4. 创建一套**可动画的托管画笔**（`Mine.Brush.*`），初次切换主题时以颜色动画平滑过渡；
5. 向 `ThemeManager` 注册自身，使后续 API 调用能找到资源写入目标。

```xml
<!-- App.xaml -->
<mine:ThemeDictionary Mode="System" SeedColor="#6750A4"/>
```

### 属性详解

```csharp
// 读写均有效；赋值后立即重新应用主题
dict.Mode      = ThemeMode.Dark;
dict.SeedColor = Color.FromRgb(0x00, 0x70, 0xF3);
dict.Preset    = ThemePreset.Teal;
```

| 属性 | 类型 | 默认值 | 说明 |
|------|------|--------|------|
| `Mode` | `ThemeMode` | `System` | 亮/暗/跟随系统 |
| `SeedColor` | `Color` | `#6750A4` | 品牌种子色（`Preset = None` 时有效） |
| `Preset` | `ThemePreset` | `None` | 官方预设（优先于 `SeedColor`） |

> **注意**：`Preset` 与 `SeedColor` 互斥。设置 `Preset` 后，库会直接读取预生成的预设颜色文件，不再调用 HCT 生成算法，渲染性能更高。

---

## ThemeManager：运行时主题 API

`ThemeManager` 是静态类，提供所有运行时主题切换能力。

### 主要 API

```csharp
// 切换到自定义种子色主题
ThemeManager.ApplyTheme(ThemeMode.Dark, seedColor: Color.FromRgb(0xFF, 0x57, 0x22));

// 切换到官方预设主题
ThemeManager.ApplyPresetTheme(ThemeMode.Light, ThemePreset.Blue);

// 亮/暗一键切换（智能判断当前状态并反转）
ThemeManager.ToggleLightDark();

// 启用系统强调色跟随（跟随 Windows 个性化颜色设置）
ThemeManager.UseSystemAccent(follow: true);

// 查询当前是否为暗色模式
bool isDark = ThemeManager.IsEffectiveDark();

// 获取当前生成的颜色方案对象（使用预设时为 null）
ColorScheme? scheme = ThemeManager.Current;
```

### 状态属性（只读）

| 属性 | 类型 | 说明 |
|------|------|------|
| `Mode` | `ThemeMode` | 当前主题模式 |
| `SeedColor` | `Color` | 当前种子色 |
| `Preset` | `ThemePreset` | 当前预设（`None` 表示使用种子色） |
| `Current` | `ColorScheme?` | 当前生成的颜色方案（预设模式下为 `null`） |

### 切换动画

首次调用 `ApplyTheme` 或 `ApplyPresetTheme` 时，若界面已经渲染完毕（即 `_initialized == true`），库会对托管画笔执行 **200 ms 颜色动画**，使主题切换平滑过渡，无闪烁。

---

## ThemeMode：亮色 / 暗色 / 跟随系统

```csharp
public enum ThemeMode
{
	Light,   // 强制亮色
	Dark,    // 强制暗色
	System   // 读取 Windows 注册表 AppsUseLightTheme，跟随系统偏好
}
```

`System` 模式下，库订阅 `SystemEvents.UserPreferenceChanged`，当用户在 Windows 设置中切换亮/暗模式时，应用会**自动响应**，无需开发者任何额外代码。

---

## SeedColor：自定义品牌色

只需要一个颜色值，库自动生成完整的 Material 3 调色板：

```xml
<!-- XAML（在 ThemeDictionary 上设置） -->
<mine:ThemeDictionary Mode="System" SeedColor="#FF5722"/>
```

```csharp
// 代码（运行时切换）
ThemeManager.ApplyTheme(ThemeMode.System, Color.FromRgb(0xFF, 0x57, 0x22));
```

**生成过程**：`SeedColor` → HCT 空间 → `TonalPaletteGenerator` 生成 13 条色调曲线 → 从曲线上提取约 40 个颜色角色 → 写入 `Mine.Color.*` 和 `Mine.Brush.*` 资源。

---

## ThemePreset：官方预设色系

库内置 28 套官方预设主题，每套均提供亮/暗两个颜色文件（位于 `Themes/Resources/Presets/`），无需运行时生成算法，切换更快。

```csharp
public enum ThemePreset
{
	None,        // 使用动态种子色（默认）
	Violet, Iris, Purple, Indigo, Cobalt, Blue, Sky, Cyan,
	Teal, Jade, Green, Forest, Lime, Olive,
	Gold, Amber, Orange, DeepOrange, Coral,
	Red, Crimson, Pink, Rose, Magenta,
	Brown, Cocoa, Slate, Gray
}
```

用法：

```xml
<mine:ThemeDictionary Mode="System" Preset="Teal"/>
```

```csharp
ThemeManager.ApplyPresetTheme(ThemeMode.Dark, ThemePreset.Cobalt);
```

---

## 动态资源令牌参考

以下资源键在 XAML 中可通过 `{DynamicResource ...}` 引用，主题切换时自动更新。

### 颜色令牌（`Mine.Color.*`）

返回 `Color` 结构，适合与 `SolidColorBrush` 配合使用：

```xml
<SolidColorBrush Color="{DynamicResource Mine.Color.Primary}" Opacity="0.5"/>
```

完整颜色角色列表：

```
Primary / OnPrimary / PrimaryContainer / OnPrimaryContainer
Secondary / OnSecondary / SecondaryContainer / OnSecondaryContainer
Tertiary / OnTertiary / TertiaryContainer / OnTertiaryContainer
Error / OnError / ErrorContainer / OnErrorContainer
Surface / OnSurface / SurfaceVariant / OnSurfaceVariant / SurfaceTint
SurfaceContainerLowest / SurfaceContainerLow / SurfaceContainer
SurfaceContainerHigh / SurfaceContainerHighest
Background / OnBackground
Outline / OutlineVariant
InverseSurface / InverseOnSurface / InversePrimary
Shadow / Scrim
Warning / OnWarning / WarningContainer / OnWarningContainer
Success / OnSuccess / SuccessContainer / OnSuccessContainer
Info / OnInfo / InfoContainer / OnInfoContainer
```

### 画笔令牌（`Mine.Brush.*`）

返回 `SolidColorBrush`，是控件模板的主要引用方式：

```xml
<Border Background="{DynamicResource Mine.Brush.SurfaceContainerLow}"/>
<TextBlock Foreground="{DynamicResource Mine.Brush.OnSurface}"/>
```

每个颜色角色都有对应的 `Mine.Brush.*` 键，命名规则与颜色令牌相同。

### 形状令牌（`Mine.Shape.*`）

返回 `CornerRadius`：

| 键 | 典型值 | 适用场景 |
|----|--------|----------|
| `Mine.Shape.None` | `0` | 无圆角 |
| `Mine.Shape.ExtraSmall` | `4` | 输入框、菜单项 |
| `Mine.Shape.Small` | `8` | 卡片角标 |
| `Mine.Shape.Medium` | `12` | 卡片 |
| `Mine.Shape.Large` | `16` | FAB、抽屉 |
| `Mine.Shape.ExtraLarge` | `28` | 底部弹出层 |
| `Mine.Shape.Full` | `50` | 胶囊形按钮 |

### 阴影令牌（`Mine.Elevation.*`）

返回 `DropShadowEffect`，共 0–5 级：

```xml
<Border Effect="{DynamicResource Mine.Elevation.2}"/>
```

### 字体令牌（`Mine.FontSize.*` / `Mine.FontFamily.*`）

```xml
<TextBlock FontSize="{DynamicResource Mine.FontSize.BodyLarge}"
		   FontFamily="{DynamicResource Mine.FontFamily.Default}"/>
```

---

## 监听主题变化

```csharp
ThemeManager.ThemeChanged += (_, args) =>
{
	// args.IsDark       — 切换后是否为暗色
	// args.ColorScheme  — 新的颜色方案对象（预设模式下为 null）
	Console.WriteLine($"主题已切换，暗色模式：{args.IsDark}");
};
```

---

## 常见用法示例

### 切换按钮（Toggle）

```xml
<mine:Button Content="切换主题" Click="OnToggleTheme"/>
```

```csharp
private void OnToggleTheme(object sender, RoutedEventArgs e)
	=> ThemeManager.ToggleLightDark();
```

### 系统颜色跟随

```csharp
// App.xaml.cs（OnStartup）
protected override void OnStartup(StartupEventArgs e)
{
	base.OnStartup(e);
	// 启用跟随 Windows 强调色（注册表读取）
	ThemeManager.UseSystemAccent(follow: true);
}
```

### 在自定义控件中引用主题色

```xml
<Style TargetType="local:MyCard">
	<Setter Property="Background" Value="{DynamicResource Mine.Brush.SurfaceContainerLow}"/>
	<Setter Property="BorderBrush" Value="{DynamicResource Mine.Brush.Outline}"/>
</Style>
```

这样无论用户如何切换主题，你的自定义控件都会自动获得正确颜色，无需任何额外代码。

---

上一章：[快速入门 ←](01-getting-started.md)　　下一章：[控件参考 →](03-controls.md)
