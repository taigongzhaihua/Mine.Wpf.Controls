# ImageViewer 控件

Material Design 3 风格的图片查看器控件，支持缩放、拖拽、旋转等交互操作。

## 功能特性

- ✨ **流畅的交互体验**：支持鼠标滚轮缩放、拖拽平移、双击重置
- 🎨 **MD3 设计规范**：完全符合 Material Design 3 设计系统
- 🛠️ **丰富的工具栏**：内置放大、缩小、旋转、适应窗口、重置等操作
- ⚙️ **高度可定制**：可配置缩放范围、步长、工具栏显示等
- 🎯 **手势优化**：自定义光标（grab/grabbing），提供直观的操作反馈

## 基本用法

```xaml
<mine:ImageViewer Source="path/to/image.jpg"
				  ShowToolbar="True"
				  EnableZoom="True"
				  EnablePan="True"/>
```

## 主要属性

| 属性 | 类型 | 默认值 | 说明 |
|------|------|--------|------|
| `Source` | `ImageSource` | `null` | 图片源（支持 URL 或本地路径） |
| `ShowToolbar` | `bool` | `true` | 是否显示工具栏 |
| `EnableZoom` | `bool` | `true` | 是否启用缩放功能 |
| `EnablePan` | `bool` | `true` | 是否启用拖拽功能 |
| `Zoom` | `double` | `1.0` | 当前缩放级别 |
| `MinZoom` | `double` | `0.1` | 最小缩放级别 |
| `MaxZoom` | `double` | `10.0` | 最大缩放级别 |
| `ZoomStep` | `double` | `0.1` | 缩放步长 |
| `Rotation` | `double` | `0.0` | 旋转角度 |

## 命令

控件提供以下命令，可用于外部按钮绑定：

- `ZoomInCommand` - 放大
- `ZoomOutCommand` - 缩小
- `ResetCommand` - 重置所有变换
- `RotateLeftCommand` - 向左旋转 90°
- `RotateRightCommand` - 向右旋转 90°
- `FitToWindowCommand` - 适应窗口大小

## 交互操作

### 鼠标操作

- **滚轮缩放**：向上滚动放大，向下滚动缩小，以鼠标位置为中心缩放
- **拖拽平移**：按住鼠标左键拖动图片
- **双击重置**：双击图片恢复到初始状态

### 工具栏操作

- **放大 / 缩小**：点击对应按钮调整缩放级别
- **旋转**：向左或向右旋转 90°
- **适应窗口**：自动调整图片大小以适应容器
- **重置**：恢复所有变换到初始状态

## 使用示例

### 隐藏工具栏

```xaml
<mine:ImageViewer Source="image.jpg"
				  ShowToolbar="False"/>
```

### 自定义缩放范围

```xaml
<mine:ImageViewer Source="image.jpg"
				  MinZoom="0.5"
				  MaxZoom="5.0"
				  ZoomStep="0.2"/>
```

### 禁用缩放或拖拽

```xaml
<!-- 只允许查看，不允许交互 -->
<mine:ImageViewer Source="image.jpg"
				  EnableZoom="False"
				  EnablePan="False"/>
```

### 外部控制

```xaml
<StackPanel>
	<mine:ImageViewer x:Name="MyImageViewer" 
					  Source="image.jpg"
					  ShowToolbar="False"/>

	<StackPanel Orientation="Horizontal">
		<Button Content="放大" 
				Command="{Binding ElementName=MyImageViewer, Path=ZoomInCommand}"/>
		<Button Content="缩小" 
				Command="{Binding ElementName=MyImageViewer, Path=ZoomOutCommand}"/>
		<Button Content="重置" 
				Command="{Binding ElementName=MyImageViewer, Path=ResetCommand}"/>
	</StackPanel>
</StackPanel>
```

### 程序化控制

```csharp
// 设置缩放级别
myImageViewer.Zoom = 2.0;

// 旋转图片
myImageViewer.Rotation = 90;

// 调用方法
myImageViewer.ZoomIn();
myImageViewer.FitToWindow();
myImageViewer.Reset();
```

## 样式定制

控件完全遵循 MD3 设计令牌系统，颜色会自动跟随主题变化：

```xaml
<mine:ImageViewer Source="image.jpg"
				  Background="{DynamicResource Mine.Brush.Surface}"
				  Foreground="{DynamicResource Mine.Brush.OnSurface}"
				  BorderBrush="{DynamicResource Mine.Brush.OutlineVariant}"
				  BorderThickness="1"/>
```

## 技术细节

### 变换组合

控件使用 `TransformGroup` 组合以下变换：

1. `ScaleTransform` - 缩放
2. `RotateTransform` - 旋转
3. `TranslateTransform` - 平移

变换按顺序应用，确保缩放和旋转以图片中心为基准，平移则作用于整体。

### 动画效果

所有变换操作都带有流畅的缓动动画（300ms，CubicEase EaseOut），符合 Material Design 动效规范。

### 光标处理

- 默认状态：张开的手掌（grab）
- 拖拽时：握拳（grabbing）
- 使用自定义 `.cur` 光标文件，提供一致的视觉反馈

## 注意事项

1. **图片加载**：支持网络图片和本地图片，网络图片需要确保可访问
2. **性能优化**：使用 `RenderOptions.BitmapScalingMode="HighQuality"` 确保缩放质量
3. **透明图片**：自动显示棋盘格背景，便于查看透明区域
4. **容器大小**：建议设置明确的 `Height` 或使用可用空间

## 浏览器兼容性

该控件为 WPF 控件，运行于 .NET 8+ 环境。

## 参与贡献

欢迎提交 Issue 和 Pull Request！
