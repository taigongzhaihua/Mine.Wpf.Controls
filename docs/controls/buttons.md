# 控件:按钮

命名空间:`Mine.Wpf.Controls.Controls`

## `Button`

Material 3 按钮,支持 7 种变体、图标、加载状态与海拔。

```xml
<mine:Button Content="保存" Variant="Filled" Icon="save"/>
<mine:Button Content="删除" Variant="Text" Icon="delete"/>
<mine:Button Content="登录" Variant="Filled" IsLoading="{Binding IsBusy}"/>
<mine:Button Variant="Fab" Icon="add"/>            <!-- 圆形悬浮按钮 -->
<mine:Button Content="创建" Variant="ExtendedFab" Icon="add"/>
```

### 属性

| 属性 | 类型 | 默认值 | 说明 |
|---|---|---|---|
| `Variant` | `ButtonVariant` | `Filled` | 按钮变体 |
| `Icon` | `object?` | `null` | 图标:Material Symbols 字符串或任意 UIElement |
| `IsLoading` | `bool` | `false` | 加载状态(显示加载动画) |
| `CornerRadius` | `CornerRadius` | `20` | 圆角 |
| `Elevation` | `int` | `0` | 海拔等级(FAB 默认阴影) |

### `ButtonVariant` 枚举

| 值 | 说明 |
|---|---|
| `Filled` | 实心按钮(MD3 主按钮) |
| `Tonal` | 次级容器色实心 |
| `FilledTonal` | `Tonal` 的别名,与 MD3 规范命名一致 |
| `Outlined` | 描边按钮 |
| `Text` | 文本按钮 |
| `Elevated` | 带阴影的浅色按钮 |
| `Fab` | 圆形悬浮按钮(自动保持正圆) |
| `ExtendedFab` | 加宽悬浮按钮(图标 + 文字) |

---

## `AppBarButton` / `AppBarToggleButton` / `AppBarSeparator`

命令栏元素:图标 + 标签竖排。`AppBarToggleButton` 支持选中态,`AppBarSeparator` 为竖线分隔符。

| 属性 | 类型 | 默认值 | 说明 |
|---|---|---|---|
| `Icon` | `object?` | `null` | 图标(任意 UIElement,如 `MaterialIcon`) |
| `Label` | `string?` | `null` | 按钮下方文本 |
| `IsCompact` | `bool` | `false` | 紧凑模式(仅图标) |

```xml
<mine:CommandBar>
    <mine:CommandBar.PrimaryCommands>
        <mine:AppBarButton Icon="add" Label="新建" Command="{Binding NewCommand}"/>
        <mine:AppBarToggleButton Icon="view_agenda" Label="列表视图"/>
        <mine:AppBarSeparator/>
        <mine:AppBarButton Icon="delete" Label="删除"/>
    </mine:CommandBar.PrimaryCommands>
</mine:CommandBar>
```

## `CommandBar`

命令栏:主命令直接显示,次要命令收纳进溢出菜单(「⋯」按钮)。

| 属性 | 类型 | 默认值 | 说明 |
|---|---|---|---|
| `PrimaryCommands` | `ObservableCollection<UIElement>` | 空集合 | 主命令(始终显示) |
| `SecondaryCommands` | `ObservableCollection<MenuFlyoutItemBase>` | 空集合 | 次要命令(溢出菜单) |
| `OverflowButtonVisibility` | `CommandBarOverflowButtonVisibility` | `Auto` | 溢出按钮显隐策略 |
| `IsOverflowOpen` | `bool`(只读) | `false` | 溢出菜单是否展开 |

### `CommandBarOverflowButtonVisibility`

| 值 | 说明 |
|---|---|
| `Auto` | 存在次要命令时显示(默认) |
| `Visible` | 始终显示 |
| `Collapsed` | 始终隐藏 |

---

## Chip 家族

Material 3 芯片。基类 `Chip` 提供全部能力,四个子类在构造函数中预设变体。`Chip` 继承 `Control` 而非 `ToggleButton`,避免鼠标捕获干扰子元素交互。

```xml
<mine:AssistChip Content="刷新" Icon="refresh"/>
<mine:FilterChip Content="已启用" IsChecked="True"/>
<mine:InputChip Content="标签一" DeleteCommand="{Binding RemoveCommand}" DeleteCommandParameter="标签一"/>
<mine:SuggestionChip Content="试用新功能"/>
```

### 子类

| 类 | 变体 | 说明 |
|---|---|---|
| `AssistChip` | `Assist` | 触发单次操作,不保留选中态 |
| `FilterChip` | `Filter` | 可切换选中/取消,选中显示勾选 |
| `InputChip` | `Input` | 已输入标签,带删除按钮 |
| `SuggestionChip` | `Suggestion` | 建议快捷入口,点击即触发 |

### `Chip` 属性

| 属性 | 类型 | 默认值 | 说明 |
|---|---|---|---|
| `Variant` | `ChipVariant` | `Assist` | 变体 |
| `Content` | `object?` | `null` | 芯片内容 |
| `IsChecked` | `bool` | `false` | 选中态(Filter 变体) |
| `Icon` | `string?` | `null` | 前置图标(Material Symbols 字符) |
| `Avatar` | `object?` | `null` | 头像内容 |
| `DeleteCommand` | `ICommand?` | `null` | 删除按钮命令 |
| `DeleteCommandParameter` | `object?` | `null` | 删除命令参数 |

### `Chip` 事件

| 事件 | 类型 | 说明 |
|---|---|---|
| `Click` | `RoutedEventHandler` | 点击芯片(Filter 变体同时切换 `IsChecked`) |
| `Deleted` | `RoutedEventHandler` | 点击删除按钮 |

---

## `SegmentedControl`

分段选择控件(继承 `ListBox`):多个互斥选项排成一行。

```xml
<mine:SegmentedControl SelectedIndex="0">
    <mine:SegmentedItem Icon="list" Content="列表"/>
    <mine:SegmentedItem Icon="grid_view" Content="网格"/>
    <mine:SegmentedItem Icon="settings" Content="设置"/>
</mine:SegmentedControl>
```

### `SegmentedItem` 属性

| 属性 | 类型 | 默认值 | 说明 |
|---|---|---|---|
| `Icon` | `string?` | `null` | 图标(Material Symbols 字符) |
| `Content` | — | — | 继承自 `ListBoxItem` |
| `Position` | `SegmentedItemPosition` | `Only`(父级维护) | 在组中的位置,控制圆角渲染 |

### `SegmentedItemPosition`

| 值 | 说明 |
|---|---|
| `Only` | 仅此一项(四角圆角) |
| `First` | 第一项(左端圆角) |
| `Middle` | 中间项(无圆角) |
| `Last` | 最后一项(右端圆角) |
