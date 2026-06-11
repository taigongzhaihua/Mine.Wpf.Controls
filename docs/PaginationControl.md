# PaginationControl - Material Design 3 分页控件

一个符合 Material Design 3 设计规范的 WPF 分页控件,提供直观的页面导航体验。

## 特性

- ✨ 符合 Material Design 3 设计规范
- 🎨 支持深色/浅色主题
- 🔢 智能页码显示(自动省略号)
- ⚡ 流畅的交互动画
- 🎯 灵活的配置选项
- 🔄 双向数据绑定支持

## 安装

控件已包含在 `Mine.Wpf.Controls` 库中,无需额外安装。

## 基本用法

```xaml
<mine:PaginationControl CurrentPage="1"
						TotalPages="10"/>
```

## 属性

### CurrentPage
- **类型**: `int`
- **默认值**: `1`
- **说明**: 当前页码(从 1 开始),支持双向绑定

### TotalPages
- **类型**: `int`
- **默认值**: `1`
- **说明**: 总页数

### MaxPageButtons
- **类型**: `int`
- **默认值**: `7`
- **说明**: 最多显示的页码按钮数量

### ShowFirstLastButtons
- **类型**: `bool`
- **默认值**: `true`
- **说明**: 是否显示首页/末页按钮

### CornerRadius
- **类型**: `CornerRadius`
- **默认值**: `20`
- **说明**: 按钮圆角半径

## 事件

### PageChanged
- **类型**: `RoutedPropertyChangedEventHandler<int>`
- **说明**: 页码改变时触发

```xaml
<mine:PaginationControl PageChanged="OnPageChanged"/>
```

```csharp
private void OnPageChanged(object sender, RoutedPropertyChangedEventArgs<int> e)
{
	var newPage = e.NewValue;
	// 处理页码变化
}
```

## 使用示例

### 基本分页

```xaml
<mine:PaginationControl CurrentPage="{Binding CurrentPage}"
						TotalPages="10"/>
```

### 大量页面(自动省略号)

```xaml
<mine:PaginationControl CurrentPage="25"
						TotalPages="100"
						MaxPageButtons="7"/>
```

### 简化样式(无首页/末页按钮)

```xaml
<mine:PaginationControl CurrentPage="5"
						TotalPages="20"
						ShowFirstLastButtons="False"/>
```

### 自定义显示更多页码

```xaml
<mine:PaginationControl CurrentPage="15"
						TotalPages="50"
						MaxPageButtons="11"/>
```

### 数据绑定示例

```xaml
<mine:PaginationControl CurrentPage="{Binding CurrentPage, Mode=TwoWay}"
						TotalPages="{Binding TotalPages}"
						PageChanged="OnPageChanged"/>
```

```csharp
public class ViewModel : INotifyPropertyChanged
{
	private int _currentPage = 1;
	private int _totalPages = 10;

	public int CurrentPage
	{
		get => _currentPage;
		set
		{
			if (_currentPage != value)
			{
				_currentPage = value;
				OnPropertyChanged();
				LoadData(); // 加载对应页的数据
			}
		}
	}

	public int TotalPages
	{
		get => _totalPages;
		set
		{
			if (_totalPages != value)
			{
				_totalPages = value;
				OnPropertyChanged();
			}
		}
	}

	private void LoadData()
	{
		// 根据 CurrentPage 加载数据
	}
}
```

## 样式定制

分页控件使用以下资源键,可以通过覆盖这些资源来自定义样式:

- `Mine.Style.PaginationButton` - 普通页码按钮样式
- `Mine.Style.PaginationButton.Selected` - 选中页码按钮样式
- `Mine.Style.PaginationNavigationButton` - 导航按钮样式

```xaml
<Style x:Key="Mine.Style.PaginationButton" TargetType="Button" BasedOn="{StaticResource {x:Type Button}}">
	<!-- 自定义样式 -->
</Style>
```

## 设计指南

### 使用场景
- 数据表格分页
- 搜索结果分页
- 文章列表分页
- 任何需要分页浏览的场景

### 最佳实践
1. **页码数量**: 对于移动端或小屏幕,建议 `MaxPageButtons="5"`
2. **首页/末页按钮**: 当总页数较少(< 10)时,可以隐藏首页/末页按钮
3. **显示信息**: 配合文本显示当前页/总页数信息,提升用户体验
4. **加载状态**: 页面切换时显示加载指示器
5. **响应式**: 在小屏幕上减少显示的页码按钮数量

### 交互说明
- 点击页码直接跳转到对应页
- 点击上一页/下一页按钮切换页面
- 点击首页/末页按钮快速跳转
- 省略号表示中间有被隐藏的页码
- 当前页高亮显示,不可点击

## Material Icons

控件使用以下 Material Symbols 图标:
- `ChevronLeft` (E5CB) - 上一页
- `ChevronRight` (E5CC) - 下一页
- `FirstPage` (E5DC) - 首页
- `LastPage` (E5DD) - 末页

## 主题适配

控件自动适配 Material Design 3 主题系统,使用以下颜色令牌:
- `Mine.Brush.OnSurface` - 文本颜色
- `Mine.Brush.OnSurfaceVariant` - 导航按钮颜色
- `Mine.Brush.Primary` - 悬停/按下颜色
- `Mine.Brush.SecondaryContainer` - 选中按钮背景
- `Mine.Brush.OnSecondaryContainer` - 选中按钮文本
- `Mine.Brush.Outline` - 导航按钮边框

## 性能建议

- 分页控件会在每次页码改变时重新生成按钮,性能开销很小
- 建议总页数不超过 1000,超过时考虑使用输入框跳转方式
- 页码按钮使用虚拟化,不会影响性能

## 浏览器兼容性

作为 WPF 控件,仅支持 Windows 平台:
- Windows 10 (1809+)
- Windows 11
- .NET 8.0+

## License

MIT License

## 相关控件

- `Button` - 按钮控件
- `NavigationView` - 导航视图
- `ListView` - 列表视图
