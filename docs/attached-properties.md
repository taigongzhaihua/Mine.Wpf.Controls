# 附加属性与助手

命名空间:`Mine.Wpf.Controls.Attached`、`Mine.Wpf.Controls.Helpers`

附加属性统一通过 `mine:` 前缀使用,例如 `mine:ShadowAssist.Elevation="3"`。

## `ShadowAssist`

为任意 `UIElement` 提供 Material 3 海拔(0–5)阴影。

| 附加属性 | 类型 | 默认值 | 说明 |
|---|---|---|---|
| `Elevation` | `int` | `0` | 海拔等级(0–5),渲染为对应阴影 |

```xml
<mine:Card mine:ShadowAssist.Elevation="3" .../>
```

## `ShapeAssist`

为任意 `FrameworkElement` 设置形状比例令牌或显式圆角。

| 附加属性 | 类型 | 默认值 | 说明 |
|---|---|---|---|
| `ShapeScale` | `ShapeScale` | `None` | 形状比例令牌,设置后自动换算为 `CornerRadius` |
| `CornerRadius` | `CornerRadius` | `0` | 显式圆角(与 `ShapeScale` 二选一) |

| 静态方法 | 说明 |
|---|---|
| `ScaleToRadius(ShapeScale scale)` | 比例 → 圆角换算,见下表 |

### `ShapeScale` 枚举

| 值 | 圆角半径 | | 值 | 圆角半径 |
|---|---|---|---|---|
| `None` | 0 | | `Large` | 16 |
| `ExtraSmall` | 4 | | `ExtraLarge` | 28 |
| `Small` | 8 | | `Full` | 9999(胶囊形) |
| `Medium` | 12 | | | |

```xml
<Border mine:ShapeAssist.ShapeScale="Medium" Background="{mine:ThemeResource Brush.SurfaceContainer}"/>
```

## `HintAssist`

为文本输入控件提供浮动标签、辅助文本等附加属性。`TextBox` 系列控件已内置同名依赖属性,此附加属性面向标准 WPF `TextBox` 等第三方控件。

| 附加属性 | 类型 | 默认值 | 说明 |
|---|---|---|---|
| `Hint` | `string` | `null` | 浮动标签文字 |
| `HelperText` | `string` | `null` | 辅助文本 |
| `FloatingScale` | `double` | `0.75` | 标签浮动后的缩放比例 |
| `HintForeground` | `Brush` | `null` | 标签画刷(默认使用主题色) |

## `IconAssist`

为控件提供前置/后置图标附加属性。

| 附加属性 | 类型 | 默认值 | 说明 |
|---|---|---|---|
| `Icon` | `object` | `null` | 前置图标(Material Symbols 字符串或任意内容) |
| `IconSize` | `double` | `18.0` | 图标尺寸 |
| `TrailingIcon` | `object` | `null` | 后置图标 |

## `RippleAssist`

为任意控件提供水波纹效果控制。

| 附加属性 | 类型 | 默认值 | 说明 |
|---|---|---|---|
| `IsEnabled` | `bool` | `true` | 是否启用水波纹 |
| `RippleColor` | `Color` | `White` | 波纹颜色 |
| `IsCentered` | `bool` | `false` | 波纹是否从控件中心扩散 |
| `ClipToBounds` | `bool` | `true` | 是否裁剪波纹到控件边界 |

## `ToolTipAssist`

| 附加属性 | 类型 | 默认值 | 说明 |
|---|---|---|---|
| `Title` | `string` | `null` | 为 `RichToolTip` 提供可选标题 |

```xml
<mine:RichToolTip mine:ToolTipAssist.Title="快捷键">按 Ctrl+S 保存</mine:RichToolTip>
```

## `FlyoutService`

将 `Flyout` 挂载到任意元素:点击该元素自动打开/关闭弹出层。支持两种写法:

```xml
<!-- 引用已有 Flyout -->
<mine:Button Content="菜单" mine:FlyoutService.Flyout="{Binding ElementName=MyFlyout}"/>
<mine:Flyout x:Name="MyFlyout">
    <TextBlock Text="弹出内容"/>
</mine:Flyout>

<!-- 内联定义 -->
<mine:Button Content="打开">
    <mine:FlyoutService.Flyout>
        <mine:Flyout>...内容...</mine:Flyout>
    </mine:FlyoutService.Flyout>
</mine:Button>
```

| 附加属性 | 类型 | 默认值 | 说明 |
|---|---|---|---|
| `Flyout` | `Flyout` | `null` | 挂载的弹出层;点击宿主元素切换其 `IsOpen` |

`Flyout` 控件本身的 API 见[浮层](controls/overlays.md)。

## `ClipHelper`

为任意 `FrameworkElement` 应用真正的圆角裁剪(`UIElement.Clip`)。`Border.ClipToBounds` 只裁剪到矩形边界,不裁剪圆角,此属性弥补该缺口。

| 附加属性 | 类型 | 默认值 | 说明 |
|---|---|---|---|
| `CornerRadius` | `double` | `0` | 圆角裁剪半径;`> 0` 时按元素尺寸应用裁剪并随尺寸变化更新,`<= 0` 时清除 `Clip` |

```xml
<Border Background="{mine:ThemeResource Brush.PrimaryContainer}">
    <mine:ClipHelper.CornerRadius>12</mine:ClipHelper.CornerRadius>
</Border>
```

> 注意:`ClipHelper` 位于 `Mine.Wpf.Controls.Helpers` 命名空间,但同样已映射到 `mine:` 前缀。
