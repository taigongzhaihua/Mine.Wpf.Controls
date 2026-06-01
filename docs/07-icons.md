# 图标系统

Mine.Wpf.Controls 内置完整的 **Google Material Symbols Rounded** 图标字体（含普通版和填充版），  
并提供两种使用方式：`MaterialIcon` 控件（推荐）和直接使用 Unicode 字符串。

---

## 目录

1. [图标字体说明](#图标字体说明)
2. [MaterialIcon 控件](#materialicon-控件)
3. [MaterialIcons 常量类](#materialicons-常量类)
4. [在按钮中使用图标](#在按钮中使用图标)
5. [在任意 TextBlock 中使用图标](#在任意-textblock-中使用图标)
6. [图标查找与常用图标速查](#图标查找与常用图标速查)

---

## 图标字体说明

库内嵌了两个字体文件（`pack://application:,,,/Mine.Wpf.Controls;component/Fonts/`）：

| 字体文件 | 资源键 | 说明 |
|---------|--------|------|
| `MaterialSymbols.ttf` | `Mine.Font.MaterialSymbols` | 轮廓（Outline）风格，默认 |
| `MaterialSymbolsFilled.ttf` | ——（仅 `MaterialIcon` 内部使用） | 填充（Filled）风格 |

这两个资源在 `ThemeDictionary` 的构造函数中自动注册，无需额外引用。

---

## MaterialIcon 控件

**类**：`Mine.Wpf.Controls.Controls.MaterialIcon`  
**基类**：`Control`

### 属性表

| 属性 | 类型 | 默认值 | 说明 |
|------|------|--------|------|
| `Kind` | `string` | `""` | 图标名称（与 `MaterialIcons` 常量字段名一致） |
| `Size` | `double` | `24` | 图标尺寸（像素），同时设置 `Width` 和 `Height` |
| `IsFilled` | `bool` | `false` | `true` 时使用填充变体字体 |
| `Weight` | `FontWeight` | `Regular` | 字重（影响图标线条粗细） |
| `Foreground` | `Brush` | 继承 | 图标颜色 |

### 用法示例

```xml
<!-- 基础图标（24px，轮廓风格） -->
<mine:MaterialIcon Kind="Favorite"/>

<!-- 指定尺寸 -->
<mine:MaterialIcon Kind="Settings" Size="32"/>

<!-- 填充风格 -->
<mine:MaterialIcon Kind="Favorite" IsFilled="True" Foreground="Red"/>

<!-- 自定义颜色 -->
<mine:MaterialIcon Kind="CheckCircle"
				   Foreground="{DynamicResource Mine.Brush.Success}"/>

<!-- 大号图标，加粗 -->
<mine:MaterialIcon Kind="CloudUpload" Size="48" Weight="Bold"/>
```

### Kind 属性的两种赋值方式

**方式一：常量类字段名（推荐，有 IDE 补全）**

```xml
<mine:MaterialIcon Kind="Favorite"/>
<mine:MaterialIcon Kind="Settings"/>
<mine:MaterialIcon Kind="Notifications"/>
```

**方式二：Unicode 码点字符串（直接硬编码）**

```xml
<mine:MaterialIcon Kind="&#xE87D;"/>   <!-- Favorite -->
<mine:MaterialIcon Kind="&#xE8B8;"/>   <!-- Settings -->
```

> `MaterialIcon` 内部通过反射将字段名映射到 Unicode 码点，并缓存查找结果，性能开销可以忽略不计。

---

## MaterialIcons 常量类

**命名空间**：`Mine.Wpf.Controls.Icons`

`MaterialIcons` 是一个纯常量类，包含所有 Material Symbols 图标的名称到 Unicode 字符串的映射：

```csharp
using Mine.Wpf.Controls.Icons;

// 在代码中引用图标
icon.Kind = MaterialIcons.Favorite;          // "\uE87D"
icon.Kind = MaterialIcons.Settings;          // "\uE8B8"
icon.Kind = MaterialIcons.Notifications;     // "\uE7F4"
icon.Kind = MaterialIcons.CheckCircle;       // "\uE86C"
icon.Kind = MaterialIcons.Warning;           // "\uE002"
icon.Kind = MaterialIcons.Error;             // "\uE000"
icon.Kind = MaterialIcons.Info;              // "\uE88E"
```

### 在 XAML 中使用常量（通过 x:Static）

```xml
xmlns:icons="clr-namespace:Mine.Wpf.Controls.Icons;assembly=Mine.Wpf.Controls"

<mine:MaterialIcon Kind="{x:Static icons:MaterialIcons.Favorite}"/>
```

---

## 在按钮中使用图标

`Button.Icon` 属性接受 `object?` 类型，支持两种内容：

### 方式一：`MaterialIcon` 控件（推荐）

```xml
<mine:Button Content="收藏">
	<mine:Button.Icon>
		<mine:MaterialIcon Kind="Favorite" Size="18"/>
	</mine:Button.Icon>
</mine:Button>
```

优势：支持 `IsFilled`、`Weight` 等完整属性控制。

### 方式二：Unicode 字符串（简洁）

```xml
<!-- 模板会自动用 Mine.Font.MaterialSymbols 渲染 -->
<mine:Button Content="下载" Icon="&#xF090;"/>
<mine:Button Icon="&#xE5CD;" Style="{StaticResource Mine.Style.Button.Icon}"/>
```

### FAB（浮动操作按钮）图标

```xml
<!-- 图标居中，无文字 -->
<mine:Button Variant="Fab" Icon="&#xE145;"/>

<!-- Extended FAB：图标 + 文字 -->
<mine:Button Variant="ExtendedFab" Content="新建" Icon="&#xE145;"/>
```

---

## 在任意 TextBlock 中使用图标

若不使用 `MaterialIcon` 控件，可直接在 `TextBlock` 中内联图标字符：

```xml
<TextBlock FontFamily="{DynamicResource Mine.Font.MaterialSymbols}"
		   FontSize="24"
		   Text="&#xE87D;"
		   Foreground="{DynamicResource Mine.Brush.Primary}"/>
```

这适合极简场景（如 `DataTemplate` 内的轻量图标），但不支持填充变体切换。

---

## 图标查找与常用图标速查

### 在线查找

访问 [fonts.google.com/icons](https://fonts.google.com/icons)，搜索图标名称，  
选择 **Material Symbols Rounded**，复制图标名（即 `MaterialIcons` 中的字段名，驼峰格式）。

> **注意**：`MaterialIcons` 字段名使用**大驼峰命名**（PascalCase），与 Google Fonts 网站显示的 snake_case 名称不同。  
> 例如：网站上的 `check_circle` 对应常量类中的 `CheckCircle`。

### 常用图标速查

| 图标名（Kind） | Unicode | 含义 |
|---------------|---------|------|
| `Add` | `&#xE145;` | 添加/加号 |
| `Close` | `&#xE5CD;` | 关闭/×号 |
| `Search` | `&#xE8B6;` | 搜索 |
| `Settings` | `&#xE8B8;` | 设置 |
| `Favorite` | `&#xE87D;` | 收藏/心形 |
| `Home` | `&#xE88A;` | 首页 |
| `Person` | `&#xE7FD;` | 用户/人物 |
| `Notifications` | `&#xE7F4;` | 通知/铃铛 |
| `Mail` | `&#xE158;` | 邮件 |
| `Download` | `&#xF090;` | 下载 |
| `Upload` | `&#xF09B;` | 上传 |
| `Delete` | `&#xE872;` | 删除 |
| `Edit` | `&#xE3C9;` | 编辑 |
| `Share` | `&#xE80D;` | 分享 |
| `CheckCircle` | `&#xE86C;` | 成功/勾选圆 |
| `Warning` | `&#xE002;` | 警告三角 |
| `Error` | `&#xE000;` | 错误圆 |
| `Info` | `&#xE88E;` | 信息/i圆 |
| `ArrowBack` | `&#xE5C4;` | 返回 |
| `ArrowForward` | `&#xE5C8;` | 前进 |
| `Menu` | `&#xE5D2;` | 汉堡菜单 |
| `MoreVert` | `&#xE5D4;` | 更多（垂直三点） |
| `MoreHoriz` | `&#xE5D3;` | 更多（水平三点） |
| `Refresh` | `&#xE5D5;` | 刷新 |
| `CloudUpload` | `&#xE2C3;` | 云上传 |
| `Visibility` | `&#xE8F4;` | 可见/眼睛 |
| `VisibilityOff` | `&#xE8F5;` | 隐藏 |
| `Lock` | `&#xE897;` | 锁定 |
| `FilterList` | `&#xE152;` | 筛选 |
| `Sort` | `&#xE164;` | 排序 |

---

上一章：[自定义控件与模板扩展 ←](06-customization.md)　　下一章：[Gallery 示例应用指南 →](08-gallery.md)
