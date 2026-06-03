# Expander 控件

Material Design 3 风格的可展开控件，支持流畅的展开/收起动画。

## 功能特性

- ✨ **流畅动画** - 使用 MD3 动效令牌实现平滑的展开/收起动画
- 🎨 **Material Design 3** - 符合最新 MD3 设计规范
- 🎯 **高度可定制** - 支持自定义标题、圆角、动画时长等
- 📦 **灵活布局** - 标题和内容区域均支持任意 UIElement
- 🔄 **状态管理** - 提供 IsExpanded 双向绑定和状态变化事件
- 🎭 **交互反馈** - 悬停和按下状态的视觉反馈
- 🌓 **主题适配** - 自动适配浅色/深色主题

## 基础用法

```xaml
<mine:Expander Header="基本展开控件">
	<TextBlock Text="这是可展开的内容区域。点击标题栏可以展开或收起内容。"/>
</mine:Expander>
```

## 默认展开

```xaml
<mine:Expander Header="默认展开" IsExpanded="True">
	<TextBlock Text="这个 Expander 默认是展开状态。"/>
</mine:Expander>
```

## 带边框样式

```xaml
<mine:Expander Header="带边框的 Expander"
			   BorderThickness="1"
			   BorderBrush="{DynamicResource Mine.Brush.OutlineVariant}">
	<TextBlock Text="添加边框增强视觉区分。"/>
</mine:Expander>
```

## 自定义标题

Header 属性支持任何 UIElement，可以创建丰富的标题布局：

```xaml
<mine:Expander>
	<mine:Expander.Header>
		<StackPanel Orientation="Horizontal">
			<mine:MaterialIcon Kind="&#xE88E;" Size="20" Margin="0,0,12,0"
							   Foreground="{DynamicResource Mine.Brush.Primary}"/>
			<StackPanel>
				<TextBlock Text="自定义标题" 
						   Style="{DynamicResource Mine.Typography.TitleSmall}"/>
				<TextBlock Text="可以在标题中放置任何内容" 
						   Style="{DynamicResource Mine.Typography.BodySmall}"
						   Foreground="{DynamicResource Mine.Brush.OnSurfaceVariant}"/>
			</StackPanel>
		</StackPanel>
	</mine:Expander.Header>
	<TextBlock Text="Header 属性支持任何 UIElement。"/>
</mine:Expander>
```

## 嵌套使用

Expander 支持嵌套使用，可以创建多级展开结构：

```xaml
<mine:Expander Header="一级菜单">
	<StackPanel>
		<TextBlock Text="一级菜单内容" Margin="0,0,0,12"/>

		<mine:Expander Header="二级菜单 A" 
					   Background="{DynamicResource Mine.Brush.SurfaceContainerHighest}">
			<TextBlock Text="二级菜单 A 的内容。"/>
		</mine:Expander>

		<mine:Expander Header="二级菜单 B"
					   Background="{DynamicResource Mine.Brush.SurfaceContainerHighest}">
			<TextBlock Text="二级菜单 B 的内容。"/>
		</mine:Expander>
	</StackPanel>
</mine:Expander>
```

## 自定义动画速度

```xaml
<!-- 快速动画 -->
<mine:Expander Header="快速动画" AnimationDuration="0:0:0.15">
	<TextBlock Text="使用较快的动画速度（150ms）。"/>
</mine:Expander>

<!-- 慢速动画 -->
<mine:Expander Header="慢速动画" AnimationDuration="0:0:0.5">
	<TextBlock Text="使用较慢的动画速度（500ms）。"/>
</mine:Expander>
```

## 主要属性

| 属性 | 类型 | 默认值 | 说明 |
|------|------|--------|------|
| `Header` | object | null | 标题内容，支持任意对象 |
| `Content` | object | null | 展开区域的内容 |
| `IsExpanded` | bool | false | 是否展开，支持双向绑定 |
| `ExpandDirection` | ExpandDirection | Down | 展开方向（保留属性，当前仅支持 Down） |
| `CornerRadius` | CornerRadius | 12 | 圆角大小 |
| `Elevation` | int | 0 | 阴影海拔等级（0-5） |
| `AnimationDuration` | Duration | 0:0:0.3 | 展开/收起动画时长 |

## 事件

| 事件 | 说明 |
|------|------|
| `Expanded` | 展开时触发 |
| `Collapsed` | 收起时触发 |

## 使用场景

- **常见问题 (FAQ)** - 问答列表的展开/收起
- **设置面板** - 分组显示配置选项
- **详细信息** - 隐藏次要信息，节省空间
- **导航菜单** - 多级菜单的树形展开
- **表单分组** - 将复杂表单分为多个可折叠区域

## 设计原则

1. **渐进展示** - 默认收起，用户主动展开查看详情
2. **清晰状态** - 通过旋转的展开图标明确指示当前状态
3. **平滑过渡** - 使用 MD3 动效令牌确保动画流畅自然
4. **一致性** - 遵循 Material Design 3 设计规范

## 注意事项

- 避免在 Expander 内嵌套过深（建议不超过 3 层）
- 标题应简洁明了，能够概括展开内容
- 对于经常访问的内容，考虑默认展开
- 嵌套时建议使用不同的背景色来区分层级
