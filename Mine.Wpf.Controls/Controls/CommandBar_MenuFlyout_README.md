# CommandBar 和 MenuFlyout 控件

## CommandBar

CommandBar 是一个用于显示一组操作按钮的工具栏容器，支持主要命令和次要命令的组织管理。

### 主要特性

- **主要命令（PrimaryCommands）**：显示在工具栏中的主要操作按钮
- **次要命令（SecondaryCommands）**：显示在"更多"菜单中的次要操作
- **紧凑模式（CompactMode）**：仅显示图标，隐藏标签
- **Material 3 设计**：符合 Material Design 3 规范的视觉样式

### 基本用法

```xaml
<mine:CommandBar>
	<mine:CommandBar.PrimaryCommands>
		<mine:Button Content="保存" Icon="Save" Variant="Text" />
		<mine:Button Content="撤销" Icon="Undo" Variant="Text" />
		<mine:Button Content="重做" Icon="Redo" Variant="Text" />
	</mine:CommandBar.PrimaryCommands>

	<mine:CommandBar.SecondaryCommands>
		<mine:MenuFlyoutItem Text="设置" Icon="Settings" />
		<mine:MenuFlyoutItem Text="帮助" Icon="Help" />
		<mine:MenuFlyoutSeparator />
		<mine:MenuFlyoutItem Text="关于" Icon="Info" />
	</mine:CommandBar.SecondaryCommands>
</mine:CommandBar>
```

### 紧凑模式

```xaml
<mine:CommandBar CompactMode="True">
	<mine:CommandBar.PrimaryCommands>
		<mine:Button Variant="Text" ToolTip="保存">
			<mine:Button.Icon>
				<mine:MaterialIcon Kind="Save" Size="20" />
			</mine:Button.Icon>
		</mine:Button>
	</mine:CommandBar.PrimaryCommands>
</mine:CommandBar>
```

## MenuFlyout

MenuFlyout 是一个轻量级上下文菜单，支持图标、快捷键、分隔符和子菜单。

### 主要特性

- **菜单项（MenuFlyoutItem）**：基本菜单项，支持图标、文本和快捷键提示
- **分隔符（MenuFlyoutSeparator）**：菜单项分组分隔线
- **子菜单（MenuFlyoutSubItem）**：支持嵌套子菜单
- **动画效果**：淡入淡出和缩放动画
- **自动关闭**：点击外部区域或选择菜单项后自动关闭

### 基本用法

```xaml
<mine:Button Content="打开菜单" Click="OnOpenMenuClick"/>

<mine:MenuFlyout x:Name="MyMenu">
	<mine:MenuFlyout.Items>
		<mine:MenuFlyoutItem Text="新建" KeyboardAcceleratorTextOverride="Ctrl+N">
			<mine:MenuFlyoutItem.Icon>
				<mine:MaterialIcon Kind="Add" Size="20" />
			</mine:MenuFlyoutItem.Icon>
		</mine:MenuFlyoutItem>
		<mine:MenuFlyoutItem Text="打开" KeyboardAcceleratorTextOverride="Ctrl+O">
			<mine:MenuFlyoutItem.Icon>
				<mine:MaterialIcon Kind="FolderOpen" Size="20" />
			</mine:MenuFlyoutItem.Icon>
		</mine:MenuFlyoutItem>
		<mine:MenuFlyoutSeparator />
		<mine:MenuFlyoutItem Text="退出" />
	</mine:MenuFlyout.Items>
</mine:MenuFlyout>
```

### 在代码中打开菜单

```csharp
private void OnOpenMenuClick(object sender, RoutedEventArgs e)
{
	if (sender is FrameworkElement element)
	{
		MyMenu.PlacementTarget = element;
		MyMenu.IsOpen = true;
	}
}
```

### 子菜单

```xaml
<mine:MenuFlyout x:Name="SubMenu">
	<mine:MenuFlyout.Items>
		<mine:MenuFlyoutItem Text="复制" />
		<mine:MenuFlyoutItem Text="粘贴" />
		<mine:MenuFlyoutSeparator />
		<mine:MenuFlyoutSubItem Text="导出">
			<mine:MenuFlyoutSubItem.Icon>
				<mine:MaterialIcon Kind="FileDownload" Size="20" />
			</mine:MenuFlyoutSubItem.Icon>
			<mine:MenuFlyoutSubItem.Items>
				<mine:MenuFlyoutItem Text="导出为 PDF" />
				<mine:MenuFlyoutItem Text="导出为 PNG" />
				<mine:MenuFlyoutItem Text="导出为 SVG" />
			</mine:MenuFlyoutSubItem.Items>
		</mine:MenuFlyoutSubItem>
	</mine:MenuFlyout.Items>
</mine:MenuFlyout>
```

### 禁用菜单项

```xaml
<mine:MenuFlyoutItem Text="禁用项" IsEnabled="False" />
```

### 菜单项命令绑定

```xaml
<mine:MenuFlyoutItem Text="保存" 
					Command="{Binding SaveCommand}"
					KeyboardAcceleratorTextOverride="Ctrl+S">
	<mine:MenuFlyoutItem.Icon>
		<mine:MaterialIcon Kind="Save" Size="20" />
	</mine:MenuFlyoutItem.Icon>
</mine:MenuFlyoutItem>
```

### 属性说明

#### CommandBar 属性

| 属性 | 类型 | 默认值 | 描述 |
|------|------|--------|------|
| PrimaryCommands | IList | null | 主要命令集合 |
| SecondaryCommands | IList | null | 次要命令集合 |
| CompactMode | bool | false | 紧凑模式（仅显示图标） |
| IsSecondaryMenuOpen | bool | false | 次要菜单是否打开 |
| DefaultLabelPosition | CommandBarLabelPosition | Right | 标签位置（Bottom/Right/Collapsed） |

#### MenuFlyout 属性

| 属性 | 类型 | 默认值 | 描述 |
|------|------|--------|------|
| Items | ObservableCollection<MenuFlyoutItemBase> | null | 菜单项集合 |
| IsOpen | bool | false | 菜单是否打开 |
| PlacementTarget | UIElement | null | 定位目标元素 |
| Placement | PlacementMode | Bottom | 放置模式 |
| MenuMinWidth | double | 112 | 菜单最小宽度 |
| MenuMaxWidth | double | 280 | 菜单最大宽度 |

#### MenuFlyoutItem 属性

| 属性 | 类型 | 默认值 | 描述 |
|------|------|--------|------|
| Text | string | "" | 菜单项文本 |
| Icon | object | null | 菜单项图标 |
| Command | ICommand | null | 命令 |
| CommandParameter | object | null | 命令参数 |
| KeyboardAcceleratorTextOverride | string | "" | 快捷键文本提示 |
| IsEnabled | bool | true | 是否启用 |

## 示例

完整示例请参考 `samples/Mine.Wpf.Controls.Gallery/Pages/CommandBarPage.xaml`。

## 设计参考

- [Material Design 3 - Top app bar](https://m3.material.io/components/top-app-bar/overview)
- [Material Design 3 - Menu](https://m3.material.io/components/menus/overview)
- [WinUI 3 - CommandBar](https://learn.microsoft.com/en-us/windows/windows-app-sdk/api/winrt/microsoft.ui.xaml.controls.commandbar)
- [WinUI 3 - MenuFlyout](https://learn.microsoft.com/en-us/windows/windows-app-sdk/api/winrt/microsoft.ui.xaml.controls.menuflyout)
