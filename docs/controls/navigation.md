# 控件:导航与布局

命名空间:`Mine.Wpf.Controls.Controls`

## `Window`

Material 风格窗口:自定义标题栏 + 系统级背景特效(Windows 11)。

```xml
<mine:Window x:Class="MyApp.MainWindow"
             xmlns:mine="https://schemas.mine.io/wpf"
             Title="My App" Width="1200" Height="760"
             BackdropType="Mica">
    ...
</mine:Window>
```

| 属性 | 类型 | 默认值 | 说明 |
|---|---|---|---|
| `BackdropType` | `WindowBackdropType` | `Mica` | 背景特效 |
| `TitleBarHeight` | `double` | `40` | 标题栏高度(同步到 WindowChrome) |
| `ShowTitleBar` | `bool` | `true` | 显示标题栏 |
| `TitleBarContent` | `object?` | `null` | 标题栏自定义内容 |
| `CornerRadius` | `CornerRadius` | `8` | 窗口圆角 |

### `WindowBackdropType`

| 值 | 说明 |
|---|---|
| `None` | 无特效 |
| `Mica` | Windows 11 Mica(默认) |
| `MicaAlt` | Mica Alt(Tabbed) |
| `Acrylic` | Acrylic 亚克力 |

> 背景特效在窗口 `Loaded` 时应用;仅 Windows 11 生效,低版本系统自动降级为普通背景。

---

## `Page` / `Frame`

页面与导航帧。`Page` 内置 `ScrollViewer`(可关闭);`Frame` 管理导航栈与过渡动画。

```xml
<mine:Frame x:Name="MainFrame" DefaultTransition="FadeThrough"/>
```

```csharp
MainFrame.Navigate(new SettingsPage());
MainFrame.GoBack();
```

### `Page` 属性

| 属性 | 类型 | 默认值 | 说明 |
|---|---|---|---|
| `Title` | `string` | `""` | 标题 |
| `Subtitle` | `string` | `""` | 副标题 |
| `IsScrollable` | `bool` | `true` | 内容是否包裹在 `ScrollViewer` 中 |
| `Frame` | `Frame?`(只读) | — | 所在导航帧,由 `Frame` 注入 |

> `Page` 提供 `protected virtual OnNavigatedTo / OnNavigatingFrom` 供子类覆写导航生命周期。

### `Frame` 属性

| 属性 | 类型 | 默认值 | 说明 |
|---|---|---|---|
| `DefaultTransition` | `PageTransition` | `FadeThrough` | 默认过渡动画 |
| `TransitionDuration` | `Duration` | `240ms` | 动画时长 |
| `CanGoBack` | `bool`(只读) | `false` | 可返回 |
| `CanGoForward` | `bool`(只读) | `false` | 可前进 |
| `CurrentPage` | `object?`(只读) | `null` | 当前页面内容 |

### `Frame` 方法

| 签名 | 说明 |
|---|---|
| `Navigate(object content, PageTransition? transition = null)` | 导航到新页,清空前进栈 |
| `GoBack(PageTransition? transition = null)` | 返回上一页(默认 `SlideBack`) |
| `GoForward(PageTransition? transition = null)` | 前进(默认 `SlideForward`) |
| `NavigateRoot(object content, PageTransition? transition = null)` | 清空历史并导航到初始页 |

### `Frame` 事件

| 事件 | 类型 | 说明 |
|---|---|---|
| `Navigated` | `EventHandler<PageNavigatedEventArgs>` | 导航完成(动画结束后);`e.Page` 为新页面 |

### `PageTransition`

| 值 | 说明 |
|---|---|
| `None` | 直接切换 |
| `Fade` | 淡入淡出 |
| `FadeThrough` | MD3 FadeThrough(默认) |
| `SlideForward` | 新页从右滑入 |
| `SlideBack` | 新页从左滑入 |
| `SlideUp` | 新页从下方滑入 |

---

## `NavigationView`

左侧导航框架(基于 `SplitView`):菜单数据驱动,支持五种显示模式与内置搜索框。

```xml
<mine:NavigationView MenuItemsSource="{Binding NavItems}"
                     SelectedItem="{Binding Current}"
                     PaneTitle="My App">
    <Grid>...页面内容...</Grid>
</mine:NavigationView>
```

### 属性

| 属性 | 类型 | 默认值 | 说明 |
|---|---|---|---|
| `MenuItemsSource` | `IEnumerable?` | `null` | 主导航数据源 |
| `FooterMenuItemsSource` | `IEnumerable?` | `null` | 底部导航数据源 |
| `SelectedItem` | `object?` | `null` | 当前选中项(双向) |
| `DisplayMode` | `NavigationViewDisplayMode` | `Auto` | 显示模式 |
| `IsPaneOpen` | `bool` | `true` | 面板是否展开(双向) |
| `PaneTitle` | `string?` | `null` | 面板顶部应用名 |
| `Header` | `object?` | `null` | 内容区顶部标题(`null` 隐藏标题行) |
| `OpenPaneLength` | `double` | `260` | 展开宽度 |
| `CompactPaneLength` | `double` | `50` | 图标条宽度 |
| `IsPaneToggleButtonVisible` | `bool` | `true` | 显示汉堡按钮 |
| `PaneFooter` | `object?` | `null` | 面板底部自定义内容 |
| `IsSearchBoxVisible` | `bool` | `false` | 面板内显示搜索框 |
| `AutoSuggestBoxText` | `string` | `""` | 搜索框文字(双向) |
| `AutoSuggestBoxPlaceholder` | `string` | `"搜索"` | 搜索框占位符 |
| `AutoSuggestBoxSuggestionsSource` | `IEnumerable?` | `null` | 搜索建议数据源 |
| `HeaderPadding` | `Thickness`(只读) | `0` | 标题区边距(自动计算) |

### 方法

`OpenPane()` / `ClosePane()` / `TogglePane()` — 展开 / 收起 / 切换面板。

### 事件

| 事件 | 类型 | 说明 |
|---|---|---|
| `SelectionChanged` | `SelectionChangedEventHandler` | 选中变化 |
| `QuerySubmitted` | `RoutedEventHandler` | 搜索提交 |

### 枚举

`NavigationViewDisplayMode`:

| 值 | 说明 |
|---|---|
| `Auto` | 按宽度在 `Left` / `LeftCompact` / `LeftMinimal` 间自动切换(默认) |
| `Left` | 左侧展开面板(Inline) |
| `LeftCompact` | 紧凑图标条,浮层展开(CompactOverlay) |
| `LeftCompactInline` | 紧凑图标条,推挤内容展开(CompactInline) |
| `LeftMinimal` | 面板隐藏,浮层展开(Overlay) |

`NavigationViewItem`(导航项容器):

| 属性 | 类型 | 默认值 | 说明 |
|---|---|---|---|
| `Icon` | `string?` | `null` | 图标(Material Symbols 字符) |
| `LayoutMode` | `NavigationViewItemLayoutMode` | `Expanded` | 布局(父级设置) |

> `NavigationViewList`(内部列表,继承 `ListBox`,负责生成 `NavigationViewItem` 容器)无自有公开成员。

---

## `BreadcrumbBar`

面包屑导航。

```xml
<mine:BreadcrumbBar ItemsSource="{Binding Path}"
                    ItemClicked="OnBreadcrumbClicked"/>
```

| 属性 | 类型 | 默认值 | 说明 |
|---|---|---|---|
| `ItemsSource` | `IEnumerable?` | `null` | 层级数据源 |
| `ItemTemplate` | `DataTemplate?` | `null` | 项模板 |
| `MaxItems` | `int` | `0` | 最大显示项数(超出折叠为省略号;0 不限制) |
| `NavigateCommand` | `RoutedCommand`(静态字段) | — | 导航命令,参数为 `BreadcrumbItem` |

`BreadcrumbItem`(内部容器,`DependencyObject` 子类)公开成员: `Index`(int,数据源索引)、`IsEllipsis`(bool,省略号占位)、`IsLast`(bool,末项)、`Data`(object?,原始数据)。

### 事件

| 事件 | 类型 | 说明 |
|---|---|---|
| `ItemClicked` | `BreadcrumbItemClickedEventHandler` | 项被点击;`e.Item` 为原始数据,`e.Index` 为索引 |

---

## `MenuBar`

顶部菜单栏:「文件 / 编辑 / 视图」式横向菜单,悬停自动切换展开。

```xml
<mine:MenuBar>
    <mine:MenuBarItem Header="文件">
        <mine:MenuFlyoutItem Text="新建" Icon="add" Command="{Binding NewCommand}"/>
        <mine:MenuFlyoutSeparator/>
        <mine:MenuFlyoutSubItem Text="最近打开" Icon="history">
            <mine:MenuFlyoutItem Text="文档 A"/>
            <mine:MenuFlyoutItem Text="文档 B"/>
        </mine:MenuFlyoutSubItem>
    </mine:MenuBarItem>
    <mine:MenuBarItem Header="编辑">
        <mine:MenuFlyoutItem Text="撤销" InputGestureText="Ctrl+Z"/>
    </mine:MenuBarItem>
</mine:MenuBar>
```

### `MenuBar` 属性 / 方法

| 成员 | 类型 | 说明 |
|---|---|---|
| `Items` | `ObservableCollection<MenuBarItem>` | 菜单栏项 |
| `CloseAll()` | 方法 | 关闭所有展开菜单 |

### `MenuBarItem` 属性

| 属性 | 类型 | 默认值 | 说明 |
|---|---|---|---|
| `Header` | `string` | `""` | 标题 |
| `Items` | `ObservableCollection<MenuFlyoutItemBase>` | 空集合 | 下拉菜单项 |
| `IsSubMenuOpen` | `bool` | `false` | 下拉是否展开 |

---

## `MenuFlyout`

Material 3 弹出菜单,支持图标、快捷键提示、分隔符与子菜单,可独立使用或由 `MenuBar` / `CommandBar` 承载。

```xml
<mine:MenuFlyout x:Name="ContextMenu" Placement="MousePoint">
    <mine:MenuFlyoutItem Text="复制" Icon="content_copy"/>
    <mine:MenuFlyoutSeparator/>
    <mine:MenuFlyoutSubItem Text="导出" Icon="download">
        <mine:MenuFlyoutItem Text="PNG"/>
        <mine:MenuFlyoutItem Text="SVG"/>
    </mine:MenuFlyoutSubItem>
</mine:MenuFlyout>
```

### `MenuFlyout` 属性

| 属性 | 类型 | 默认值 | 说明 |
|---|---|---|---|
| `Items` | `ObservableCollection<MenuFlyoutItemBase>` | 空集合 | 菜单项 |
| `IsOpen` | `bool` | `false` | 是否打开 |
| `PlacementTarget` | `UIElement?` | `null` | 定位目标 |
| `Placement` | `PlacementMode` | `Bottom` | 放置模式 |
| `MenuMinWidth` | `double` | `112` | 最小宽度 |
| `MenuMaxWidth` | `double` | `280` | 最大宽度 |
| `StaysOpen` | `bool` | `false` | 不随失焦自动关闭 |

### `MenuFlyout` 方法

`Show()` / `CloseMenu()`(含子菜单)/ `ContainsElement(DependencyObject?)`。

### 菜单项类型

**`MenuFlyoutItem`**

| 属性 | 类型 | 默认值 | 说明 |
|---|---|---|---|
| `Text` | `string` | `""` | 文本 |
| `Icon` | `object?` | `null` | 图标(字符串或 UIElement) |
| `Command` | `ICommand?` | `null` | 命令 |
| `CommandParameter` | `object?` | `null` | 命令参数 |
| `InputGestureText` | `string` | `""` | 快捷键提示文本(仅展示) |

事件:`Click`(`RoutedEventHandler`)。

**`MenuFlyoutSubItem`**:`Text` / `Icon` / `Items` / `IsSubMenuOpen`(悬停或右方向键展开)。

**`MenuFlyoutSeparator`**:分隔线,无公开成员。

---

## `SplitView`

侧边栏布局容器:Pane + Content,四种显示模式。

```xml
<mine:SplitView IsPaneOpen="True" DisplayMode="Inline"
                OpenPaneLength="280" CompactPaneLength="50">
    <mine:SplitView.Pane>
        <Border Background="{mine:ThemeResource Brush.SurfaceContainer}">...</Border>
    </mine:SplitView.Pane>
    <Grid>...主内容...</Grid>
</mine:SplitView>
```

### 属性

| 属性 | 类型 | 默认值 | 说明 |
|---|---|---|---|
| `Pane` | `object?` | `null` | 侧边栏内容 |
| `PaneTemplate` | `DataTemplate?` | `null` | 侧边栏数据模板 |
| `IsPaneOpen` | `bool` | `false` | 是否打开 |
| `OpenPaneLength` | `double` | `320` | 打开宽度 |
| `CompactPaneLength` | `double` | `50` | 紧凑模式细条宽度 |
| `DisplayMode` | `SplitViewDisplayMode` | `Overlay` | 显示模式 |
| `PaneBackground` | `Brush?` | `null` | 侧边栏背景 |

### 方法

`OpenPane()` / `ClosePane()` / `TogglePane()`。

### `SplitViewDisplayMode`

| 值 | 说明 |
|---|---|
| `Overlay` | 叠加内容,关闭完全隐藏(默认) |
| `Inline` | 与内容并排,关闭隐藏 |
| `CompactOverlay` | 叠加,关闭保留细条 |
| `CompactInline` | 并排,关闭保留细条 |

---

## `Drawer`

Material 3 Navigation Drawer:从边缘滑入的抽屉,支持四方向。

```xml
<mine:Drawer IsOpen="{Binding IsDrawerOpen}" DrawerPlacement="Left"
             CloseOnScrimClick="True">
    <mine:Drawer.DrawerContent>
        <StackPanel>...抽屉内容...</StackPanel>
    </mine:Drawer.DrawerContent>
    <Grid>...页面主体...</Grid>
</mine:Drawer>
```

### 属性

| 属性 | 类型 | 默认值 | 说明 |
|---|---|---|---|
| `IsOpen` | `bool` | `false` | 是否打开 |
| `DrawerContent` | `object?` | `null` | 抽屉内容 |
| `DrawerContentTemplate` | `DataTemplate?` | `null` | 内容模板 |
| `DrawerPlacement` | `DrawerPlacement` | `Left` | 滑入方向 |
| `DrawerWidth` | `double` | `320` | 宽度(左右方向) |
| `DrawerHeight` | `double` | `NaN` | 高度(上下方向,NaN 自适应) |
| `ScrimOpacity` | `double` | `0.4` | 遮罩不透明度 |
| `CloseOnScrimClick` | `bool` | `true` | 点遮罩关闭 |

### 事件

| 事件 | 类型 | 说明 |
|---|---|---|
| `Opened` | `RoutedEventHandler` | 打开 |
| `Closed` | `RoutedEventHandler` | 关闭 |

### `DrawerPlacement`

`Left`(默认)/ `Right` / `Top` / `Bottom`
