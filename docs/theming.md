# 主题系统

命名空间:`Mine.Wpf.Controls.Theming`

主题系统采用 Material Design 3 的「种子色 + 色调板算法」:给定一个种子色,自动生成亮/暗两套完整配色方案。切换主题时对托管画笔执行 300ms `ColorAnimation`,实现平滑过渡。

## 引导:`ThemeDictionary`

`ResourceDictionary` 子类,在 `App.xaml` 的 `MergedDictionaries` 中引用一次即完成全部引导:合并颜色/字体/形状/海拔/动效资源与全部控件样式,并向 `ThemeManager` 注册。

```xml
<Application.Resources>
    <ResourceDictionary>
        <ResourceDictionary.MergedDictionaries>
            <mine:ThemeDictionary Mode="System" SeedColor="#6750A4"/>
            <!-- 或使用预设: <mine:ThemeDictionary Mode="System" Preset="Teal"/> -->
        </ResourceDictionary.MergedDictionaries>
    </ResourceDictionary>
</Application.Resources>
```

### 属性

| 属性 | 类型 | 默认值 | 说明 |
|---|---|---|---|
| `Mode` | `ThemeMode` | `System` | 亮/暗/跟随系统 |
| `SeedColor` | `Color` | `#6750A4` | 动态种子色 |
| `Preset` | `ThemePreset` | `None` | 官方预设色系;非 `None` 时优先于 `SeedColor` |

---

## 核心:`ThemeManager`(静态类)

运行时主题切换的唯一入口。

### 属性

| 成员 | 类型 | 说明 |
|---|---|---|
| `Mode` | `ThemeMode` | 当前主题模式(只读) |
| `SeedColor` | `Color` | 当前种子色(只读) |
| `Preset` | `ThemePreset` | 当前预设(只读) |
| `Current` | `ColorScheme?` | 当前配色方案;使用预设时返回 `null` |
| `ThemeChanged` | `EventHandler<ThemeChangedEventArgs>` | 主题切换后触发(静态事件) |

### 方法

| 签名 | 说明 |
|---|---|
| `ApplyTheme(ThemeMode mode, Color? seedColor = null)` | 应用主题;`seedColor` 为 null 时沿用当前种子色 |
| `ApplyPresetTheme(ThemeMode mode, ThemePreset preset)` | 应用预设色系主题 |
| `ToggleLightDark()` | 在当前明暗状态间切换 |
| `UseSystemAccent(bool follow = true)` | 开启后跟随 Windows 系统主题色(系统主题变化时自动更新) |
| `IsEffectiveDark()` | 当前是否处于暗色(考虑 `System` 模式下的系统设置) |

```csharp
using Mine.Wpf.Controls.Theming;

ThemeManager.ApplyTheme(ThemeMode.Dark);                 // 暗色,保持种子色
ThemeManager.ApplyTheme(ThemeMode.Light, Colors.Red);    // 亮色 + 红色种子
ThemeManager.ApplyPresetTheme(ThemeMode.System, ThemePreset.Teal);
ThemeManager.ToggleLightDark();
ThemeManager.UseSystemAccent();                          // 跟随系统主题色
```

> 提示:主题切换需要应用资源已通过 `ThemeDictionary` 完成初始化。切换后订阅 `ThemeChanged` 可获知新方案:

```csharp
ThemeManager.ThemeChanged += (_, e) =>
{
    // e.Scheme: 新配色方案(预设模式下为 null);e.IsDark: 是否暗色
};
```

---

## 枚举

### `ThemeMode`

| 值 | 说明 |
|---|---|
| `Light` | 亮色 |
| `Dark` | 暗色 |
| `System` | 跟随操作系统设置 |

### `ThemePreset`

29 个官方预设色系;`None` 表示使用动态种子色。`ThemePresetCatalog.All` 返回除 `None` 外的全部预设。

| 值 | 种子色 | 值 | 种子色 |
|---|---|---|---|
| `Violet` | `#6E56CF` | `Gold` | `#8A6300` |
| `Iris` | `#5A67D8` | `Amber` | `#9A6700` |
| `Purple` | `#6750A4` | `Orange` | `#B05A00` |
| `Indigo` | `#394AB6` | `DeepOrange` | `#B5270F` |
| `Cobalt` | `#0057B8` | `Coral` | `#C24F3D` |
| `Blue` | `#005FAF` | `Red` | `#BA1A1A` |
| `Sky` | `#007CC7` | `Crimson` | `#A61D48` |
| `Cyan` | `#0076A3` | `Pink` | `#B02F60` |
| `Teal` | `#006874` | `Rose` | `#C2185B` |
| `Jade` | `#007A5A` | `Magenta` | `#9C27B0` |
| `Green` | `#006C4E` | `Brown` | `#7D5246` |
| `Forest` | `#2E7D32` | `Cocoa` | `#6D4C41` |
| `Lime` | `#758F00` | `Slate` | `#526373` |
| `Olive` | `#6B7331` | `Gray` | `#5F6369` |

---

## 预设目录:`ThemePresetCatalog`(静态类)

| 成员 | 类型 | 说明 |
|---|---|---|
| `All` | `IReadOnlyList<ThemePreset>` | 全部预设(不含 `None`) |
| `GetDisplayName(ThemePreset preset)` | `string` | 预设显示名(如 `DeepOrange` → `"Deep Orange"`,`None` → `"Custom"`) |
| `GetSeedColor(ThemePreset preset)` | `Color` | 预设对应种子色 |

```csharp
foreach (var preset in ThemePresetCatalog.All)
{
    var chip = new SuggestionChip
    {
        Content = ThemePresetCatalog.GetDisplayName(preset),
    };
    chip.Click += (_, _) => ThemeManager.ApplyPresetTheme(ThemeManager.Mode, preset);
}
```

---

## 配色方案:`ColorScheme`

47 个颜色角色的普通 POCO 类,每个角色一个 `Color` 属性,全部可读写。属性名即角色名:

`Primary`、`OnPrimary`、`PrimaryContainer`、`OnPrimaryContainer`、`Secondary`、`OnSecondary`、`SecondaryContainer`、`OnSecondaryContainer`、`Tertiary`、`OnTertiary`、`TertiaryContainer`、`OnTertiaryContainer`、`Error`、`OnError`、`ErrorContainer`、`OnErrorContainer`、`Surface`、`OnSurface`、`SurfaceVariant`、`OnSurfaceVariant`、`SurfaceTint`、`SurfaceContainerLowest`、`SurfaceContainerLow`、`SurfaceContainer`、`SurfaceContainerHigh`、`SurfaceContainerHighest`、`Background`、`OnBackground`、`Outline`、`OutlineVariant`、`InverseSurface`、`InverseOnSurface`、`InversePrimary`、`Shadow`、`Scrim`、`Warning`、`OnWarning`、`WarningContainer`、`OnWarningContainer`、`Success`、`OnSuccess`、`SuccessContainer`、`OnSuccessContainer`、`Info`、`OnInfo`、`InfoContainer`、`OnInfoContainer`。

---

## 事件参数:`ThemeChangedEventArgs`

| 属性 | 类型 | 说明 |
|---|---|---|
| `Scheme` | `ColorScheme` | 新配色方案(预设模式下为 `null`) |
| `IsDark` | `bool` | 是否暗色 |

---

## 色板算法:`TonalPaletteGenerator`(静态类)

高级用法,直接生成配色方案:

| 签名 | 说明 |
|---|---|
| `BuildLight(Color seed, Color? secondarySeed = null, Color? tertiarySeed = null)` | 生成亮色方案 |
| `BuildDark(Color seed, Color? secondarySeed = null, Color? tertiarySeed = null)` | 生成暗色方案 |
| `Generate(Color seed)` | 生成种子色的完整色调板:`Dictionary<int, Color>`(键为色调 0–100) |

> 绝大多数场景无需直接调用——`ThemeManager.ApplyTheme` 已内置该流程。

---

## 标记扩展:`ThemeResourceExtension`

命名空间:`Mine.Wpf.Controls.Markup`

按键名获取主题资源,并在 `ThemeChanged` 触发时自动刷新(等价于带自动刷新的 `DynamicResource`)。

| 成员 | 说明 |
|---|---|
| `ResourceKey`(`string?`) | 资源键,不含 `Mine.` 前缀(带前缀亦可),例如 `"Brush.Primary"` |

```xml
<Border Background="{mine:ThemeResource Brush.SurfaceContainerHigh}"/>
```

等价于 `{DynamicResource Mine.Brush.SurfaceContainerHigh}`,但对主题切换的刷新有明确语义,推荐在自绘模板中使用。
