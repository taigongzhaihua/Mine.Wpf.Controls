# NavigationView 控件设计规范

> 版本：1.0 | 日期：2026-05-26  
> 参考：WinUI 3 `NavigationView` 源码（`NavigationView_themeresources.xaml`）

---

## 一、概述

`NavigationView` 是基于 `SplitView` 实现的左侧导航框架控件，提供三种显示模式和自动响应式切换。

---

## 二、视觉设计

### 2.1 NavigationViewItem（导航项）

| 属性 | 值 | 说明 |
|---|---|---|
| 外形 | 大圆角胶囊（CornerRadius=20+）| 无左侧竖条选中指示器 |
| 高度 | MinHeight=36 | 与 WinUI 一致，不固定高度 |
| 外边距 | `Margin=4,2` | 与 WinUI `NavigationViewItemButtonMargin` 一致 |
| 图标列宽 | 40px | 与 WinUI `NavigationViewIconBoxWidth` 一致 |
| 图标字号 | 20px | Material Symbols |
| 标签字号 | `Mine.FontSize.LabelLarge` | FontWeight=Medium |

#### 状态

```
Normal      → Background=Transparent
PointerOver → Background=OnSurface @ 8% opacity
IsSelected  → Background=SecondaryContainer, Foreground=OnSecondaryContainer
Compact     → Label.Visibility=Collapsed（图标自动居中于 40px 列）
```

#### 模板结构

```
Grid [Margin=4,2]
  ├─ Border "LayoutRoot" [CornerRadius=20, 同高于Grid, 负责背景色]
  └─ Grid "ContentGrid" [MinHeight=36]
       ├─ Col 0 [Width=40]: TextBlock "Icon" (HAlign=Center, VAlign=Center)
       └─ Col 1 [Width=*]:  ContentPresenter "Label" (Margin=4,0,8,0)
```

---

## 三、布局设计

### 3.1 整体层次结构

```
NavigationView (ContentControl)
│
├─ [z=0] SplitView "PART_SplitView"               ← 铺满全区域
│    ├─ SplitView.Pane                             ← 左侧导航抽屉
│    │    ├─ PaneTopPadding [Height=56]            ← 给汉堡按钮留空
│    │    ├─ ScrollViewer → NavigationViewList "PART_MenuList"
│    │    └─ NavigationViewList "PART_FooterList"
│    │
│    └─ SplitView.Content                         ← 右侧主内容
│         ├─ Row 0 [Height=56]: Header 区域        ← 动态左边距
│         └─ Row 1 [Height=*]:  ContentPresenter
│
└─ [z=1] Button "PART_PaneToggleButton"           ← 浮层，始终可见
          [HAlign=Left, VAlign=Top, Margin=8,8]
```

### 3.2 汉堡按钮（PaneToggleButton）

- **位置**：浮在 NavigationView **最外层**，ZIndex 高于 SplitView，始终位于左上角
- **尺寸**：40×40px，CornerRadius=20（圆形），外边距 Margin=8,8
- **样式**：完全自定义 ControlTemplate，无 WPF 默认蓝色高亮
- **状态**：PointerOver=OnSurface@8%, Pressed=OnSurface@12%
- **图标**：Material Symbols `&#xE5D2;`（menu 汉堡图标），FontSize=22

#### 为什么必须在外层？

| 放在 SplitView.Content 内 | 放在外层浮层（当前方案）|
|---|---|
| Left 模式：按钮在内容区左上，不在窗口左上 | 始终在窗口左上角 ✅ |
| Overlay 模式关闭：按钮随内容区存在，OK | 始终可见 ✅ |
| Overlay 模式关闭：按钮在 x=0 处，无 Pane 偏移 | 正确位置 ✅ |

### 3.3 Pane 顶部留白

Pane 内容区顶部固定留 **56px** 空白，与汉堡按钮行高对齐，防止列表项被按钮遮挡。

```
PaneTopPadding = 56px（汉堡按钮行高，固定值）
```

### 3.4 Header 动态左边距（核心设计）

Header 区域的左边距根据 **Pane 是否占用水平空间**动态调整：

| 场景 | SplitView 状态 | Pane 占用空间？ | Header.PaddingLeft |
|---|---|---|---|
| Left（展开并排）| Inline, Open | 是（OpenPaneLength） | `0` |
| LeftCompact（图标条）| CompactOverlay, Closed | 是（CompactPaneLength=48px） | `0` |
| LeftCompact（展开叠加）| CompactOverlay, Open | 是（CompactPaneLength） | `0` |
| **LeftMinimal（关闭）**| **Overlay, Closed** | **否（Pane=0px）** | **`CompactPaneLength`（48px）** |
| **LeftMinimal（叠加展开）**| **Overlay, Open** | **否（Pane 浮动）** | **`CompactPaneLength`（48px）** |

**规则**：`HeaderPaddingLeft = (SplitView.DisplayMode == Overlay) ? CompactPaneLength : 0`

此值作为 `Thickness` DP 由 `NavigationView.cs` 的 `ApplyDisplayMode()` 方法计算并写入 `HeaderPadding` 属性，XAML 中 Header Border 绑定该属性。

---

## 四、DisplayMode 行为规范

### 4.1 模式映射

| NavigationViewDisplayMode | SplitView.DisplayMode | 默认 IsPaneOpen | Item.LayoutMode |
|---|---|---|---|
| `Left` | `Inline` | `true`（强制） | Expanded |
| `LeftCompact` | `CompactOverlay` | 用户控制 | Compact（关闭时）/ Expanded（打开时）|
| `LeftCompactInline` | `CompactInline` | 用户控制 | Compact（关闭时）/ Expanded（打开时）|
| `LeftMinimal` | `Overlay` | 用户控制 | Expanded |
| `Auto` | 根据宽度动态切换 | 随模式 | 随模式 |

**LeftCompact vs LeftCompactInline 的区别：**

| | `LeftCompact`（CompactOverlay）| `LeftCompactInline`（CompactInline）|
|---|---|---|
| 关闭时 | 图标条（48px）| 图标条（48px）|
| 展开时 | 全宽 Pane **叠加**内容上方（浮层）| 全宽 Pane **推挤**内容向右（并排）|
| 适用场景 | 内容区不希望被遮挡时临时查看 | 长期并排使用，内容随 Pane 宽度收缩 |

### 4.2 Auto 模式宽度阈值（与 WinUI 一致）

```
控件宽度 ≥ 1008px → Left
控件宽度 ≥  641px → LeftCompact
控件宽度 <  641px → LeftMinimal
```

### 4.3 Overlay 模式行为

- 选中导航项后**自动关闭** Pane（`IsPaneOpen = false`）
- Pane 以动画形式从左侧滑入/滑出（TranslateTransform 动画）
- Pane 打开时显示半透明 Scrim 遮罩，点击 Scrim 关闭 Pane

---

## 五、依赖属性一览

| 属性 | 类型 | 默认值 | 说明 |
|---|---|---|---|
| `DisplayMode` | `NavigationViewDisplayMode` | `Auto` | 显示模式 |
| `IsPaneOpen` | `bool` | `true` | Pane 开关状态（双向绑定）|
| `MenuItemsSource` | `IEnumerable` | `null` | 主导航列表数据源 |
| `FooterMenuItemsSource` | `IEnumerable` | `null` | 底部 Footer 列表数据源 |
| `SelectedItem` | `object` | `null` | 当前选中项（双向绑定）|
| `Header` | `object` | `null` | 内容区顶部标题（null 时隐藏）|
| `PaneTitle` | `string` | `null` | Pane 顶部 App 名称（展开时显示）|
| `OpenPaneLength` | `double` | `260` | 展开状态 Pane 宽度（px）|
| `CompactPaneLength` | `double` | `48` | 紧凑状态 Pane 宽度（px）|
| `HeaderPadding` | `Thickness` | `0` | Header 区域左边距（由代码动态写入）|
| `IsPaneToggleButtonVisible` | `bool` | `true` | 汉堡按钮是否显示 ✅ |

---

## 六、模板 Part 命名

| XAML x:Name | 类型 | 说明 |
|---|---|---|
| `PART_SplitView` | `SplitView` | 核心布局容器 |
| `PART_MenuList` | `NavigationViewList` | 主导航列表 |
| `PART_FooterList` | `NavigationViewList` | Footer 列表 |
| `PART_PaneToggleButton` | `Button` | 汉堡开关按钮 |
| `PART_Header` | `Border` | 内容区标题容器 |

---

## 七、NavigationViewItem 属性

| 属性 | 类型 | 说明 |
|---|---|---|
| `Icon` | `string` | Material Symbols Unicode 字符（如 `"\uE88A"`）|
| `LayoutMode` | `NavigationViewItemLayoutMode` | `Expanded`（展开）/ `Compact`（紧凑图标条）|
| `Content` | `object` | 继承自 `ListBoxItem`，显示为标签文字 |

`LayoutMode` 由 `NavigationView` 代码根据当前 `DisplayMode` 和 `IsPaneOpen` 状态自动写入，不需要用户手动设置。

---

## 八、事件

| 事件 | 类型 | 说明 |
|---|---|---|
| `SelectionChanged` | `SelectionChangedEventHandler` | 导航项选中变化（冒泡路由事件）|

---

## 九、使用示例

```xml
<nav:NavigationView x:Name="NavView"
                    DisplayMode="Auto"
                    PaneTitle="My App"
                    Header="Home"
                    SelectionChanged="OnNavSelectionChanged">
    <Frame x:Name="ContentFrame"/>
</nav:NavigationView>
```

```csharp
private void OnLoaded(object sender, RoutedEventArgs e)
{
    NavView.MenuItemsSource = new[]
    {
        new NavigationViewItem { Icon = "\uE88A", Content = "Home" },
        new NavigationViewItem { Icon = "\uE8F4", Content = "Favorites" },
    };
    NavView.FooterMenuItemsSource = new[]
    {
        new NavigationViewItem { Icon = "\uE8B8", Content = "Settings" },
    };
}

private void OnNavSelectionChanged(object sender, SelectionChangedEventArgs e)
{
    if (e.AddedItems[0] is NavigationViewItem item)
    {
        // ContentFrame.Navigate(...)
    }
}
```

---

## 十、待实现 / 已知限制

| 项目 | 状态 | 说明 |
|---|---|---|
| `HeaderPadding` DP + 动态计算 | ⬜ 待实现 | `ApplyDisplayMode` 中根据 DisplayMode 写入 |
| `IsPaneToggleButtonVisible` DP | ✅ 已实现 | 控制按钮 Visibility |
| 汉堡按钮移至外层浮层 | ⬜ 待实现 | 当前仍在 SplitView.Content 内 |
| Pane 顶部 56px 留白 | ⬜ 待实现 | 当前实现方式需核查 |
| NavigationViewItem 圆角胶囊样式 | ✅ 已实现 | CornerRadius=4（WinUI 参考），待改大圆角 |
| 去除选中指示条 | ⬜ 待实现 | 当前有 SelectionIndicator Rectangle |
| SelectionChanged 事件 | ✅ 已实现 | |
| Auto 模式宽度响应 | ✅ 已实现 | |
| Overlay 选中后自动关闭 | ✅ 已实现 | |
| 两列表互斥选中 | ✅ 已实现 | |

