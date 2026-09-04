# 控件:设置与数据

命名空间:`Mine.Wpf.Controls.Controls`

## Settings 系列

Material 3 设置页组件:卡片 + 分组 + 可展开项,组合出标准设置界面。

```xml
<mine:SettingsGroup Header="通用">
    <mine:SettingsCard Header="主题模式"
                       Description="跟随系统或手动指定"
                       HeaderIcon="dark_mode">
        <mine:SettingsCard.ActionContent>
            <mine:ComboBox ItemsSource="{Binding Modes}" SelectedItem="{Binding Mode}"/>
        </mine:SettingsCard.ActionContent>
    </mine:SettingsCard>

    <mine:SettingsCardButton Header="通知设置"
                             Description="管理通知方式"
                             HeaderIcon="notifications"
                             Command="{Binding OpenNotifyCommand}"/>

    <mine:SettingsColorItem Header="主题色"
                            Description="自定义强调色"
                            SelectedColor="{Binding AccentColor}"
                            Click="OnPickColor"/>

    <mine:SettingsExpander Header="高级" Description="谨慎修改" IsExpanded="False">
        <mine:SettingsCard Header="调试日志" .../>
        <mine:SettingsCard Header="崩溃报告" .../>
    </mine:SettingsExpander>
</mine:SettingsGroup>
```

### `SettingsCard`

单行设置项:左图标 + 标题/描述,右侧操作区。

| 属性 | 类型 | 默认值 | 说明 |
|---|---|---|---|
| `Header` | `object` | `null` | 标题 |
| `Description` | `object` | `null` | 描述 |
| `HeaderIcon` | `object` | `null` | 头部图标 |
| `ActionContent` | `object` | `null` | 右侧操作区(Switch / ComboBox / Button 等) |
| `IsClickable` | `bool` | `false` | 点击态视觉反馈 |
| `CornerRadius` | `CornerRadius` | `12` | 圆角 |

事件:`Click`(`RoutedEventHandler`,`IsClickable` 时触发)。

### `SettingsCardButton`

整行可点击的设置项(继承 `ButtonBase`,支持 `Command` 与键盘激活);未指定 `ActionContent` 时右侧自动显示前进箭头。

| 属性 | 类型 | 默认值 | 说明 |
|---|---|---|---|
| `Header` | `object` | `null` | 标题 |
| `Description` | `object` | `null` | 描述 |
| `HeaderIcon` | `object` | `null` | 头部图标 |
| `ActionContent` | `object` | `null` | 右侧操作区(null 显示默认箭头) |
| `CornerRadius` | `CornerRadius` | `12` | 圆角 |

### `SettingsColorItem`

颜色设置项:右侧圆形色块展示颜色,整行可点击——通常配合 `FlyoutService` 弹出 `ColorPicker`。

| 属性 | 类型 | 默认值 | 说明 |
|---|---|---|---|
| `Header` / `Description` / `HeaderIcon` | `object` | `null` | 同 `SettingsCard` |
| `SelectedColor` | `Color` | `Gray` | 色块颜色(双向) |
| `ShowChevron` | `bool` | `true` | 显示前进箭头 |
| `CornerRadius` | `CornerRadius` | `12` | 圆角 |

事件:`Click`(`RoutedEventHandler`)。

典型组合:

```xml
<mine:SettingsColorItem SelectedColor="{Binding Accent}" x:Name="ColorItem">
    <mine:FlyoutService.Flyout>
        <mine:Flyout>
            <mine:ColorPicker SelectedColor="{Binding Accent}"/>
        </mine:Flyout>
    </mine:FlyoutService.Flyout>
</mine:SettingsColorItem>
```

### `SettingsExpander`

可展开设置项(继承 `HeaderedItemsControl`):头部与 `SettingsCard` 一致,`Items` 承载子设置项,带展开动画。

| 属性 | 类型 | 默认值 | 说明 |
|---|---|---|---|
| `IsExpanded` | `bool` | `false` | 是否展开 |
| `Header` | `object` | `null` | 标题(继承自 HeaderedItemsControl) |
| `Description` | `object` | `null` | 描述 |
| `HeaderIcon` | `object` | `null` | 头部图标 |
| `ActionContent` | `object` | `null` | 头部右侧操作区 |
| `CornerRadius` | `CornerRadius` | `12` | 圆角 |

### `SettingsGroup`

设置项分组容器(继承 `ItemsControl`):圆角卡片外观,项间自动分隔线。

| 属性 | 类型 | 默认值 | 说明 |
|---|---|---|---|
| `Header` | `object` | `null` | 分组标题 |
| `CornerRadius` | `CornerRadius` | `12` | 圆角 |

> 项容器 `SettingsGroupItem` 提供 `IsLast`(bool,默认 `false`)控制分隔线显隐,由父级维护。

---

## `TreeListView`

树形列表视图:结合 `TreeView` 层级与 `ListView` 多列,支持多选、列宽拖拽与表头排序。

```xml
<mine:TreeListView ItemsSource="{Binding Roots}" SelectionMode="Multiple">
    <mine:TreeListView.Columns>
        <GridViewColumn Header="名称" Width="220" DisplayMemberBinding="{Binding Name}"/>
        <GridViewColumn Header="大小" Width="100" DisplayMemberBinding="{Binding Size}"/>
        <GridViewColumn Header="修改时间" Width="160" DisplayMemberBinding="{Binding Modified}"/>
    </mine:TreeListView.Columns>
    <mine:TreeListView.ItemTemplate>
        <HierarchicalDataTemplate ItemsSource="{Binding Children}">
            <TextBlock Text="{Binding Name}"/>
        </HierarchicalDataTemplate>
    </mine:TreeListView.ItemTemplate>
</mine:TreeListView>
```

### 属性

| 属性 | 类型 | 默认值 | 说明 |
|---|---|---|---|
| `Columns` | `GridViewColumnCollection` | `null` | 列定义(表头与所有行共享,拖拽自动同步) |
| `SelectionMode` | `TreeListViewSelectionMode` | `Single` | 选择模式 |
| `SelectedItems` | `ReadOnlyObservableCollection<object>`(只读) | 空 | 选中项集合(多选由 Ctrl/Shift 维护) |
| `SortMemberPath` | `string?`(只读) | `null` | 当前排序列绑定路径 |
| `SortDirection` | `ListSortDirection?`(只读) | `null` | 当前排序方向 |

### 事件

| 事件 | 类型 | 说明 |
|---|---|---|
| `SelectionChanged` | `SelectionChangedEventHandler` | 选中集合变化 |

### 枚举与项容器

`TreeListViewSelectionMode`:`Single`(默认)/ `Multiple`(Ctrl/Shift 多选)

`TreeListViewItem`(项容器,继承 `TreeViewItem`):

| 属性 | 类型 | 默认值 | 说明 |
|---|---|---|---|
| `Level` | `int` | `0` | 层级深度(缩进计算) |
| `IsItemSelected` | `bool` | `false` | 选中状态(驱动多选视觉,独立于原生单选 `IsSelected`) |
