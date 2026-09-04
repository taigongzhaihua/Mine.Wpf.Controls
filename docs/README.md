# Mine.Wpf.Controls API 文档

Material Design 3 风格的 WPF 控件库,包含 80+ 控件、运行时主题切换、动效令牌与全局服务(对话框 / Snackbar / Toast)。

- 目标框架:.NET 10(`net10.0-windows10.0.19041.0`)
- NuGet 包:`Mine.Wpf.Controls`(当前版本 0.1.0)
- 所有公开类型通过统一 XAML 命名空间访问,无需逐类引用程序集命名空间:

```xml
xmlns:mine="https://schemas.mine.io/wpf"
```

---

## 快速开始

### 1. 引入主题字典

在 `App.xaml` 中合并 `ThemeDictionary` 即完成全部引导:颜色字典、字体(Material Symbols)、动效、转换器与全部控件样式都会被自动加载。

```xml
<Application x:Class="MyApp.App"
             xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
             xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
             xmlns:mine="https://schemas.mine.io/wpf"
             StartupUri="MainWindow.xaml">
    <Application.Resources>
        <ResourceDictionary>
            <ResourceDictionary.MergedDictionaries>
                <!-- System 跟随系统深浅色;#6750A4 为 Material 3 默认种子色 -->
                <mine:ThemeDictionary Mode="System" SeedColor="#6750A4"/>
            </ResourceDictionary.MergedDictionaries>
        </ResourceDictionary>
    </Application.Resources>
</Application>
```

`ThemeDictionary` 支持的启动配置:

| 属性 | 类型 | 默认值 | 说明 |
|---|---|---|---|
| `Mode` | `ThemeMode` | `System` | `Light` / `Dark` / `System`(跟随系统) |
| `SeedColor` | `Color` | `#6750A4` | 动态种子色,由色板算法生成整套主题 |
| `Preset` | `ThemePreset` | `None` | 官方预设色系;非 `None` 时优先于 `SeedColor` |

> 两种典型用法:`Mode="System" SeedColor="#6750A4"`(自定义色),或 `Mode="System" Preset="Teal"`(预设色)。详见[主题系统](theming.md)。

### 2. 使用控件

所有控件在 `mine:` 前缀下直接使用:

```xml
<mine:Button Content="保存" Variant="Filled"/>
<mine:Card>
    <mine:TextBox Hint="用户名" Variant="Outlined"/>
</mine:Card>
```

### 3. 替换窗口(可选)

将 `<Window>` 替换为 `<mine:Window>` 可获得带自定义标题栏与系统级背景特效的窗口:

```xml
<mine:Window x:Class="MyApp.MainWindow"
             xmlns:mine="https://schemas.mine.io/wpf"
             Title="My App"
             Width="1200" Height="760"
             BackdropType="Mica">
    ...
</mine:Window>
```

### 4. 运行时切换主题

```csharp
// 亮/暗/跟随系统 + 自定义种子色
Mine.Wpf.Controls.Theming.ThemeManager.ApplyTheme(ThemeMode.Dark, Color.FromRgb(0x00, 0x66, 0x87));

// 官方预设色系
ThemeManager.ApplyPresetTheme(ThemeMode.Light, ThemePreset.Teal);

// 一键明暗切换
ThemeManager.ToggleLightDark();

// 跟随 Windows 系统主题色
ThemeManager.UseSystemAccent();
```

---

## 设计令牌(资源键)

主题资源以 `Mine.` 前缀写入 `Application.Resources`:

| 类别 | 键名示例 | 说明 |
|---|---|---|
| 颜色画笔 | `Mine.Brush.Primary`、`Mine.Brush.Surface`、`Mine.Brush.Error` … | 共 47 个 MD3 颜色角色,见下表 |
| 颜色值 | `Mine.Color.*` | 仅在使用自定义种子色时写入,键名同颜色角色 |
| 图标字体 | `Mine.Font.MaterialSymbols` | Material Symbols Rounded 字体 |

全部颜色角色(与 MD3 规范一一对应):

```
Primary, OnPrimary, PrimaryContainer, OnPrimaryContainer,
Secondary, OnSecondary, SecondaryContainer, OnSecondaryContainer,
Tertiary, OnTertiary, TertiaryContainer, OnTertiaryContainer,
Error, OnError, ErrorContainer, OnErrorContainer,
Surface, OnSurface, SurfaceVariant, OnSurfaceVariant, SurfaceTint,
SurfaceContainerLowest, SurfaceContainerLow, SurfaceContainer,
SurfaceContainerHigh, SurfaceContainerHighest,
Background, OnBackground,
Outline, OutlineVariant,
InverseSurface, InverseOnSurface, InversePrimary,
Shadow, Scrim,
Warning, OnWarning, WarningContainer, OnWarningContainer,
Success, OnSuccess, SuccessContainer, OnSuccessContainer,
Info, OnInfo, InfoContainer, OnInfoContainer
```

在 XAML 中可用标记扩展按需取主题资源,主题切换时自动刷新:

```xml
<Border Background="{mine:ThemeResource Brush.SurfaceContainerHigh}"/>
```

---

## 文档导航

| 文档 | 内容 |
|---|---|
| [主题系统](theming.md) | `ThemeDictionary` / `ThemeManager` / `ColorScheme` / 预设色 |
| [附加属性](attached-properties.md) | `HintAssist` / `IconAssist` / `RippleAssist` / `ShadowAssist` / `ShapeAssist` / `ToolTipAssist` / `FlyoutService` / `ClipHelper` |
| [全局服务](services.md) | `DialogService` / `SnackbarService` / `ToastService` |
| [动画](animations.md) | `MotionTokens` / `CornerRadiusAnimation` |
| [图标](icons.md) | `MaterialIcon` 控件 / `MaterialIcons` 常量 |
| [转换器](converters.md) | 19 个通用转换器 |
| [原语控件](primitives.md) | `RippleDecorator` / `NotchedOutlineBorder` |
| [控件:按钮](controls/buttons.md) | `Button` / `AppBarButton` / `Chip` / `CommandBar` / `SegmentedControl` |
| [控件:输入](controls/inputs.md) | `TextBox` / `AutoCompleteBox` / `ComboBox` / `NumericUpDown` / `SearchBox` / `TagBox` |
| [控件:选择](controls/selectors.md) | `CheckBox` / `RadioButton` / `Switch` / `Slider` / `RangeSlider` / `Rating` / `DatePicker` / `TimePicker` / `ColorPicker` / `Stepper` |
| [控件:展示](controls/display.md) | `Card` / `Badge` / `Avatar` / `Shield` / `ProgressBar` / `ProgressRing` / `ImageViewer` / `InfoBar` / `Expander` |
| [控件:导航与布局](controls/navigation.md) | `Window` / `Page` / `Frame` / `NavigationView` / `BreadcrumbBar` / `MenuBar` / `MenuFlyout` / `SplitView` / `Drawer` |
| [控件:浮层](controls/overlays.md) | `DialogHost` / `Flyout` / `BottomSheet` / `SideSheet` / `Snackbar` / `Toast` / `RichToolTip` / `NotificationCenter` / `FlipView` / `Carousel` / `PaginationControl` |
| [控件:设置与数据](controls/settings-data.md) | `SettingsCard` 系列 / `TreeListView` |

---

## 命名空间对照

XAML 前缀 `mine` 映射到以下 CLR 命名空间(由程序集 `XmlnsDefinition` 声明):

| XAML 前缀 | CLR 命名空间 |
|---|---|
| `mine` | `Mine.Wpf.Controls.Controls`(全部控件) |
| `mine` | `Mine.Wpf.Controls.Theming`(主题) |
| `mine` | `Mine.Wpf.Controls.Attached`(附加属性) |
| `mine` | `Mine.Wpf.Controls.Primitives`(原语控件) |
| `mine` | `Mine.Wpf.Controls.Markup`(`ThemeResourceExtension`) |
| `mine` | `Mine.Wpf.Controls.Animations`(动效令牌) |
| `mine` | `Mine.Wpf.Controls.Converters`(转换器) |

C# 代码中的命名空间:`Mine.Wpf.Controls.Controls`、`Mine.Wpf.Controls.Theming`、`Mine.Wpf.Controls.Icons`、`Mine.Wpf.Controls.Helpers`。

## 无障碍支持

全部控件均实现了 `AutomationPeer` 屏幕阅读器支持(如 `ChipAutomationPeer`、`RatingAutomationPeer`、`DatePickerAutomationPeer`、`TreeListView` 等)。数值类控件暴露 `IRangeValueProvider`(`NumericUpDown` / `RangeSlider` / `Rating` / `PaginationControl`),弹出类控件暴露 `IExpandCollapseProvider`(`DatePicker` / `TimePicker`)。这些 Peer 类不面向常规应用代码,无需直接使用。
