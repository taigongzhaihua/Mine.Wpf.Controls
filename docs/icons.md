# 图标

## `MaterialIcon`(控件)

命名空间:`Mine.Wpf.Controls.Controls`

显示 Google Material Symbols 图标。库内嵌了 Material Symbols Rounded / Filled 两套字体,按 `Fill` 属性自动切换。图标名称与码点见 `MaterialIcons` 常量类。

```xml
<mine:MaterialIcon Kind="home" Size="24"/>
```

| 属性 | 类型 | 默认值 | 说明 |
|---|---|---|---|
| `Kind` | `string` | `""` | 图标名称或 Unicode 字符;名称会被解析为码点字符 |
| `Size` | `double` | `24` | 图标尺寸(同步设置宽高) |
| `Weight` | `FontWeight` | 300 | 图标字重 |
| `Fill` | `bool` | `false` | `false` = 线框(Rounded),`true` = 实心(Filled) |

在 C# 中引用图标:

```csharp
new MaterialIcon { Kind = MaterialIcons.Settings, Size = 20 };
```

## `MaterialIcons`(静态类)

命名空间:`Mine.Wpf.Controls.Icons`

自动生成的 Material Symbols 图标码点常量,共 **501** 个字段,字段名即图标名。

### 命名规律

| 规则 | 示例 |
|---|---|
| 字母开头:直接使用图标名 | `Settings` → `"\ue8b8"` |
| 数字开头:字段名加前缀 `N` | 图标 `10k` → `N10k` → `"\ue951"` |
| 补充平面码点(> U+FFFF):WPF 可能无法正确渲染,字段标注 `[supplementary]` | `N2d2` → `"\U000fff0e"` |
| 别名:多个字段共用同一码点 | `AccessAlarm` / `AccessAlarms` 均为 `"\ue855"` |

### 用法

```xml
<!-- XAML -->
<mine:MaterialIcon Kind="home"/>
```

```csharp
// C#:引用常量字段
button.Icon = MaterialIcons.Delete;                 // "\ue92e"
textBox.LeadingIcon = MaterialIcons.Search;         // "\ue8b6"
```

> 常量值是码点字符而非枚举,可直接赋给任何接受 Material Symbols 字符的属性(如 `Button.Icon`、`NavigationViewItem.Icon`、`Switch.OnIcon` 等)。
