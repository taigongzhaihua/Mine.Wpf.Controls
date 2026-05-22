# Mine.Wpf.Controls 设计文档

> 一套基于 Material Design 语言、面向 .NET 10 (net10.0-windows) 的 WPF 控件库。
> 目标：扩展 / 美化默认控件、补齐其他框架已有但 WPF 缺失的控件、提供多主题（亮暗 + 多色）系统、原生支持毛玻璃 (Acrylic / Mica) 与流畅动画。

---

## 1. 项目愿景与目标

| 维度 | 目标 |
| --- | --- |
| 设计语言 | 遵循 **Material Design 3 (Material You)**，同时吸收 Fluent 中的 Acrylic / Mica |
| 控件覆盖 | 100% 覆盖 WPF 原生控件 + 30+ 扩展控件（参考 MaterialDesignInXAML / HandyControl / MahApps / WPF-UI / Syncfusion / DevExpress） |
| 属性扩展 | 默认控件通过附加属性 + 派生控件方式扩展，新增圆角、阴影、Ripple、Hint、Validation、Loading 等常用项 |
| 主题系统 | Light / Dark / System 三种模式 × 任意主色（Primary / Secondary / Tertiary）动态切换，运行时无闪烁 |
| 视觉效果 | 默认支持 Acrylic、Mica、Blur-Behind；阴影使用 DropShadow + Composition |
| 动效 | 所有交互（Hover / Press / Focus / Selection / Open / Close）默认带过渡；曲线遵循 Material Motion |
| 兼容性 | .NET 8 / .NET 9 / .NET 10 (net*-windows)；Windows 10 1809+ |
| 性能 | 启动 < 200ms 主题加载；样式合并使用 ResourceDictionary 按需懒加载 |

---

## 2. 解决方案与项目结构

```
Mine.Wpf.Controls.sln
├── src/
│   ├── Mine.Wpf.Controls/              # 核心控件库（输出 NuGet）
│   │   ├── Controls/                   # 自定义控件 (.cs)
│   │   ├── Primitives/                 # 低层基元控件
│   │   ├── Attached/                   # 附加属性 (RippleAssist, ShadowAssist...)
│   │   ├── Converters/
│   │   ├── Behaviors/
│   │   ├── Effects/                    # AcrylicBrush / BlurEffect / Shaders
│   │   ├── Animations/                 # Easing、Storyboard 工厂、Motion Tokens
│   │   ├── Theming/
│   │   │   ├── ThemeManager.cs
│   │   │   ├── ColorScheme.cs          # M3 ColorRoles
│   │   │   ├── Palette/                # Tonal Palette 生成
│   │   │   └── Resources/
│   │   │       ├── Colors.Light.xaml
│   │   │       ├── Colors.Dark.xaml
│   │   │       ├── Typography.xaml
│   │   │       ├── Elevation.xaml
│   │   │       ├── Shape.xaml
│   │   │       └── Motion.xaml
│   │   ├── Themes/
│   │   │   ├── Generic.xaml            # 必需入口
│   │   │   └── Controls/*.xaml         # 每个控件一份样式
│   │   └── Mine.Wpf.Controls.csproj
│   ├── Mine.Wpf.Controls.Icons/        # MaterialSymbols 字体图标
│   └── Mine.Wpf.Controls.Markup/       # MarkupExtensions (DynamicColor, ThemeResource)
├── samples/
│   └── Mine.Wpf.Controls.Demo/         # 展示 App（类似 Material Gallery）
├── tests/
│   ├── Mine.Wpf.Controls.UnitTests/
│   └── Mine.Wpf.Controls.UITests/      # 基于 FlaUI
└── docs/
    ├── DESIGN.md
    ├── Theming.md
    ├── Controls/*.md
    └── images/
```

---

## 3. 设计语言基础 (Design Tokens)

所有视觉常量都以 **Design Token** 形式集中管理，便于换肤与主题切换。

### 3.1 Color Roles (Material 3)
- Primary / OnPrimary / PrimaryContainer / OnPrimaryContainer
- Secondary / Tertiary（含同结构 4 个 Role）
- Error / Warning / Success / Info
- Surface / SurfaceVariant / SurfaceTint / SurfaceContainer (Lowest…Highest)
- Background / OnBackground / Outline / OutlineVariant
- Inverse Surface / Inverse OnSurface / Inverse Primary
- Scrim / Shadow

> 通过 HCT ��彩空间从单一 Seed Color 生成 13 阶 Tonal Palette，再映射到 Light / Dark Role。

### 3.2 Typography Scale
Display L/M/S、Headline L/M/S、Title L/M/S、Body L/M/S、Label L/M/S。
默认字体：`Segoe UI Variable`（Win11）→ `Microsoft YaHei UI` 回退；CJK 字重映射。

### 3.3 Shape Scale
None(0) / ExtraSmall(4) / Small(8) / Medium(12) / Large(16) / ExtraLarge(28) / Full(9999)。
通过附加属性 `ShapeAssist.CornerRadius="Medium"` 引用。

### 3.4 Elevation
0 / 1 / 2 / 3 / 4 / 5 六级。实现：
- 阴影：`DropShadowEffect` + 颜色取自 `Shadow` Role
- 表面色叠加：`SurfaceTint` 按高度透明度叠加（M3 规范）

### 3.5 Motion Tokens
| Token | Duration | Easing |
| --- | --- | --- |
| `Motion.Short1` | 50ms | StandardAccelerate |
| `Motion.Short2` | 100ms | StandardDecelerate |
| `Motion.Medium1` | 200ms | Standard |
| `Motion.Long1` | 450ms | Emphasized |
| `Motion.ExtraLong1` | 700ms | EmphasizedDecelerate |

Easing 使用自定义 `CubicBezierEase`：
- Standard: `(0.2, 0.0, 0.0, 1.0)`
- Emphasized: `(0.2, 0.0, 0, 1)` + DecelerateRatio

---

## 4. 主题系统 (Theming)

### 4.1 ThemeManager API

```csharp
public static class ThemeManager
{
    public static ThemeMode Mode { get; set; }            // Light / Dark / System
    public static Color SeedColor { get; set; }           // 任意主色
    public static ColorScheme CurrentScheme { get; }
    public static event EventHandler ThemeChanged;

    public static void ApplyTheme(ThemeMode mode, Color seed);
    public static void UseSystemAccent(bool follow = true);
}
```

### 4.2 资源访问
- XAML：`{DynamicResource Mine.Color.Primary}`、`{ThemeResource Brush.Surface}`
- 自定义 MarkupExtension `ThemeResourceExtension` 在 ThemeChanged 时自动刷新（避免使用 DynamicResource 的开销）

### 4.3 多色主题切换流程
1. 用户调用 `ApplyTheme(Dark, #6750A4)`
2. `TonalPaletteGenerator` 基于 HCT 生成 13 档色阶
3. 写入 `Application.Current.Resources` 中的 Color Role
4. 触发 `CompositionTarget.Rendering` 一帧补间动画，让主色平滑过渡（150ms ColorAnimation）
5. 持久化到 `%AppData%/Mine.Wpf.Controls/theme.json`

### 4.4 跟随系统
监听：
- `SystemEvents.UserPreferenceChanged` → 亮暗
- 注册表 `HKCU\Software\Microsoft\Windows\DWM\AccentColor` → 系统强调色

---

## 5. 毛玻璃 / 背景效果

| 效果 | 实现 |
| --- | --- |
| **Acrylic** | `AcrylicBrush`：NoiseTexture + TintColor + TintOpacity + FallbackColor；底层 `HostBackdropBrush` Win10/11 通过 `DwmSetWindowAttribute` (`SystemBackdropType = Acrylic`) |
| **Mica** | Win11 `DWMWA_SYSTEMBACKDROP_TYPE = Mica / MicaAlt` |
| **Blur-Behind** | 不支持 Mica 时回退：`SetWindowCompositionAttribute` + `AccentState.ACCENT_ENABLE_BLURBEHIND` |
| **In-App Blur** | Shader 实现 `GaussianBlurEffect`（HLSL ps_3_0），支持 Radius 动画 |

封装控件：`MineWindow`、`AcrylicCard`、`BlurPanel`。

---

## 6. 动画体系

### 6.1 通用约定
- 所有可视状态切换通过 `VisualStateManager` + `VisualTransition` 默认 200ms
- Ripple 由 `RippleAssist.IsEnabled` 附加属性挂载到任意 `Control`
- 进入/退出动画统一通过 `TransitionPresenter`（类似 WinUI）

### 6.2 内置动画库
- `FadeIn / FadeOut`
- `SlideIn (Direction, Distance)`
- `Scale (FromScale)`
- `SharedAxis (X/Y/Z)`：参考 Material Motion
- `ContainerTransform`：跨控件元素过渡（MorphingPresenter）
- `RippleEffect`：Touch / Mouse 位置精确扩散
- `LoadingShimmer`：Skeleton 占位

### 6.3 性能策略
- 使用 `CompositionTarget` + `DoubleAnimationUsingKeyFrames` 优于 `Storyboard.Begin`
- 大量重复动画使用 `Freeze()`
- 复杂模糊使用 `RenderOptions.BitmapScalingMode=LowQuality` + 缓存

---

## 7. 控件清单

### 7.1 基础控件（样式覆盖 + 属性扩展）
Button / RepeatButton / ToggleButton / CheckBox / RadioButton / TextBox / PasswordBox / RichTextBox / ComboBox / ListBox / ListView / TreeView / TabControl / Slider / ProgressBar / ScrollViewer / Menu / ContextMenu / ToolTip / GroupBox / Expander / Calendar / DatePicker

**扩展项示例（Button）**：
- `Variant`：Filled / Tonal / Outlined / Text / Elevated / FAB / ExtendedFAB
- `Icon` / `IconPlacement` / `IsLoading` / `CornerRadius` / `Elevation` / `RippleColor` / `IsBusy`

### 7.2 新增 / 扩展控件
| 控件 | 说明 |
| --- | --- |
| `MineWindow` | 自定义窗口，支持 Mica/Acrylic、自定义标题栏 |
| `NavigationView` | 顶部 / 左侧 / 抽屉式导航（参考 WinUI） |
| `Card` / `OutlinedCard` / `ElevatedCard` | M3 Card |
| `Chip` / `FilterChip` / `InputChip` / `AssistChip` |
| `Badge` | 数字 / 点状徽标 |
| `Avatar` / `AvatarGroup` |
| `SegmentedControl` |
| `Stepper` (Horizontal / Vertical) |
| `Snackbar` / `Toast` / `NotificationCenter` |
| `Dialog` / `BottomSheet` / `SideSheet` |
| `Drawer` |
| `TimePicker` / `DateRangePicker` / `ColorPicker` |
| `NumericUpDown` / `RangeSlider` |
| `Rating` |
| `TagBox` / `AutoCompleteBox` |
| `BreadcrumbBar` |
| `PaginationControl` |
| `DataGrid+` | 扩展原生 DataGrid：列拖动、固定列、汇总、空状态 |
| `PropertyGrid` |
| `TreeListView` |
| `Carousel` / `FlipView` |
| `ImageViewer` (缩放、平移、缩略图) |
| `Loading` / `Skeleton` / `ProgressRing` |
| `CircularProgress` |
| `Shield` / `GlassCard` (毛玻璃容器) |
| `CommandBar` / `MenuFlyout` |
| `TransitioningContentControl` |
| `MarkdownViewer` |
| `CodeEditor` (基于 AvalonEdit 二次封装) |
| `Terminal` (轻量) |

### 7.3 附加属性（Assist）
- `RippleAssist.IsEnabled / Color / Centered`
- `ShadowAssist.Elevation`
- `ShapeAssist.CornerRadius`
- `HintAssist.Hint / HelperText / FloatingScale`
- `ValidationAssist.HasError / ErrorMessage / ShowErrorOnTouched`
- `IconAssist.Icon / Placement / Size`
- `ScrollViewerAssist.IsSmoothScrolling / ShowSeparator`
- `TextFieldAssist.PrefixText / SuffixText / Clearable / CharacterCounter`
- `WindowAssist.BackdropType / CaptionHeight`

---

## 8. 控件实现规范

### 8.1 代码结构
```csharp
[TemplatePart(Name = PartRoot, Type = typeof(Border))]
[TemplateVisualState(Name = "Normal",   GroupName = "CommonStates")]
[TemplateVisualState(Name = "PointerOver", GroupName = "CommonStates")]
public class MineButton : ButtonBase
{
    public const string PartRoot = "PART_Root";

    static MineButton() =>
        DefaultStyleKeyProperty.OverrideMetadata(
            typeof(MineButton),
            new FrameworkPropertyMetadata(typeof(MineButton)));

    // DependencyProperties...
}
```

### 8.2 样式拆分
- `Themes/Generic.xaml` 仅作 `MergedDictionaries` 入口
- 每个控件一个 `Themes/Controls/MineButton.xaml`
- 颜色 / 字体 / 形状不写死，全部 `{DynamicResource}`

### 8.3 命名规范
- 资源 Key：`Mine.{Category}.{Name}`，如 `Mine.Brush.Primary`、`Mine.Style.Button.Filled`
- 控件类前缀：`Mine`（避免与 BCL 冲突）
- XML 命名空间：`xmlns:mine="https://schemas.mine.io/wpf"`

---

## 9. 开发者使用示例

### 9.1 App.xaml 注入
```xml
<Application xmlns:mine="https://schemas.mine.io/wpf">
  <Application.Resources>
    <ResourceDictionary>
      <ResourceDictionary.MergedDictionaries>
        <mine:ThemeDictionary Mode="System" SeedColor="#6750A4"/>
      </ResourceDictionary.MergedDictionaries>
    </ResourceDictionary>
  </Application.Resources>
</Application>
```

### 9.2 运行时切换
```csharp
ThemeManager.ApplyTheme(ThemeMode.Dark, Color.FromRgb(0x00, 0x6C, 0x4E));
```

### 9.3 Button 示例
```xml
<mine:MineButton Variant="Filled"
                 Icon="{x:Static mine:Symbol.Save}"
                 IsLoading="{Binding Saving}"
                 mine:ShadowAssist.Elevation="2"
                 mine:ShapeAssist.CornerRadius="Large"
                 Content="保存"/>
```

---

## 10. 质量保证

- **单元测试**：Tokens、ColorScheme 生成、ThemeManager 切换
- **UI 自动化**：FlaUI 驱动 Demo，截图对比 (Verify.ImageMagick)
- **可访问性**：AutomationPeer 全覆盖；对比度 ≥ WCAG AA
- **本地化**：内置 zh-CN / en-US；资源走 `.resx`
- **高 DPI**：所有矢量 / 字体图标；像素级对齐
- **RTL**：FlowDirection 全控件验证

---

## 11. 性能预算

| 指标 | 目标 |
| --- | --- |
| 主题首次加载 | < 200 ms |
| 主题切换 | < 16 ms (单帧) + 150 ms 动画 |
| Ripple 一次触发 | < 1 ms CPU |
| 1000 行 ListView 滚动 | 60 fps |
| Demo App 启动 | < 1.5 s（冷启动） |

---

## 12. 路线图 (Roadmap)

| 版本 | 内容 |
| --- | --- |
| **0.1 (M1)** | 基础设施：Tokens / ThemeManager / MineWindow / Button / TextBox / CheckBox / RadioButton / Card / Ripple |
| **0.2 (M2)** | ComboBox / ListView / TabControl / Snackbar / Dialog / ProgressBar / Slider / Tooltip |
| **0.3 (M3)** | NavigationView / Drawer / Chip / Badge / Avatar / SegmentedControl |
| **0.4 (M4)** | DatePicker / TimePicker / ColorPicker / NumericUpDown / RangeSlider / Rating |
| **0.5 (M5)** | DataGrid+ / TreeListView / PropertyGrid / Pagination |
| **0.6 (M6)** | Carousel / ImageViewer / Markdown / CodeEditor |
| **0.9 (Beta)** | 性能 / 可访问性 / 本地化完善；Demo Gallery 上线 |
| **1.0** | API 冻结、文档站、NuGet 正式发布 |

---

## 13. 参考与致谢

- Material Design 3 Guidelines — https://m3.material.io
- WinUI 3 / Windows App SDK 源码
- MaterialDesignInXAML Toolkit
- HandyControl / MahApps.Metro / WPF-UI / Panuon.WPF.UI
- Avalonia Fluent / Semi Design
- HCT Color Space (Google)

---

> 本设计文档作为 v0.1 立项基线，后续将在 `docs/Controls/` 下补充每个控件的详细 API、模板结构与可视状态图。


