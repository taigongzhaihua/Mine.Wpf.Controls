# Mine.Wpf.Controls

一个基于 Material Design 3 (MD3) 设计规范打造的 WPF 控件库，提供主题系统、动画与丰富的现代化控件集合。

## 特性

- 遵循 Material Design 3 的色彩、形状、排版与状态层规范
- 内置浅色 / 深色主题与动态取色（Dynamic Color）支持
- 覆盖常用交互控件：按钮、命令栏（CommandBar）、菜单栏（MenuBar）、菜单弹出（MenuFlyout）、导航视图（NavigationView）、日期/时间选择器、抽屉（Drawer）、底部/侧边表单（BottomSheet / SideSheet）等
- 纯 WPF 实现，无第三方运行时依赖

## 安装

```powershell
dotnet add package Mine.Wpf.Controls
```

## 快速开始

在 `App.xaml` 中合并主题资源字典：

```xml
<Application.Resources>
	<ResourceDictionary>
		<ResourceDictionary.MergedDictionaries>
			<ResourceDictionary Source="pack://application:,,,/Mine.Wpf.Controls;component/Themes/Generic.xaml"/>
		</ResourceDictionary.MergedDictionaries>
	</ResourceDictionary>
</Application.Resources>
```

然后即可在 XAML 中使用控件：

```xml
<Window xmlns:mine="https://schemas.mine.io/wpf">
	<mine:CommandBar>
		<mine:CommandBar.PrimaryCommands>
			<mine:AppBarButton Label="新建"/>
		</mine:CommandBar.PrimaryCommands>
	</mine:CommandBar>
</Window>
```

更多示例请参考仓库中的 `samples/Mine.Wpf.Controls.Gallery` 项目。

## 许可证

本项目基于 [LICENSE](https://github.com/taigongzhaihua/Mine.Wpf.Controls/blob/main/LICENSE.txt) 开源协议发布。

## 仓库地址

https://github.com/taigongzhaihua/Mine.Wpf.Controls
