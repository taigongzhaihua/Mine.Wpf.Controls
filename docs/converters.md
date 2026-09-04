# 转换器

命名空间:`Mine.Wpf.Controls.Converters`

全部转换器已映射到 `mine:` 前缀,可在 XAML 中直接实例化,或经 `ThemeDictionary` 加载后按 key 引用。

## 单值转换器

### `BoolToVisibilityConverter`

| 属性 | 类型 | 默认值 | 说明 |
|---|---|---|---|
| `Invert` | `bool` | `false` | 取反 |
| `UseHidden` | `bool` | `false` | `true` 时用 `Hidden` 而非 `Collapsed` |

转换:`bool → Visibility`(反向 `Visibility → bool`)。

### `StringToVisibilityConverter`

| 属性 | 类型 | 默认值 | 说明 |
|---|---|---|---|
| `Invert` | `bool` | `false` | 取反 |

转换:字符串非空 → `Visible`,否则 `Collapsed`。

### `StringIsNotNullOrEmptyConverter`

字符串非空 → `true`。

### `NullToVisibilityConverter`

| 属性 | 类型 | 默认值 | 说明 |
|---|---|---|---|
| `Invert` | `bool` | `false` | 取反 |

转换:null → `Collapsed`,非 null → `Visible`。

### `NullToBoolConverter`

非 null → `true`,否则 `false`。

### `IsGridViewConverter`

值是否为 `GridView` 实例 → `bool`。

### `ColorToBrushConverter` / `BrushToColorConverter`

`Color ↔ SolidColorBrush` 双向转换。`BrushToColorConverter` 遇到非 `SolidColorBrush` 输入时返回 `Color.FromArgb(40, 255, 255, 255)`(供水波纹回退色)。

### `ElevationToOpacityConverter`

海拔等级(int,0–5)→ 表面着色透明度:`[0, 0.05, 0.08, 0.11, 0.12, 0.14]`。

### `HalfValueConverter`

`double → double / 2`(非 double 输入返回 `20.0`)。提供静态单例 `Instance`。

### `ZeroToNullStringConverter`

`int → string`:0 → `null`(隐藏徽标),其他 → 数字字符串。用于 `NotificationCenter` 铃铛的 `BadgeText`。

### `CornerRadiusTopOnlyConverter`

`CornerRadius → CornerRadius`(仅保留顶部圆角,底部归零)。

### `CollectionCountToVisibilityConverter`

集合元素数 > 0 → `Visible`,否则 `Collapsed`。

### `LevelToIndentConverter`

| 属性 | 类型 | 默认值 | 说明 |
|---|---|---|---|
| `IndentSize` | `double` | `20` | 每级缩进宽度 |

转换:层级(int)→ 缩进宽度(`level × IndentSize`)。同时实现 `IMultiValueConverter`。

## 多值转换器

### `RoundedRectClipConverter`

将尺寸转换为圆角 `RectangleGeometry`,用于 `UIElement.Clip` 的真正圆角裁剪。

| 属性 | 类型 | 默认值 | 说明 |
|---|---|---|---|
| `RadiusX` | `double` | `8` | 圆角水平半径 |
| `RadiusY` | `double` | `8` | 圆角垂直半径 |

输入顺序:`[0] ActualWidth`,`[1] ActualHeight`,`[2] CornerRadius(可选,以 TopLeft 覆盖半径)`;`ConverterParameter` 也可直接指定半径。

### `CornerRadiusFilterConverter`

CSS border-radius 比例压缩算法,防止超大圆角导致渲染变形。输入:`[0] CornerRadius`,`[1] ActualWidth`,`[2] ActualHeight`。

### `SelectedIndexToVisibilityConverter`

输入:`[0] selectedIndex`,`[1] itemIndex`。相等 → `Visible`,否则 `Collapsed`。

### `GridViewSortIndicatorConverter`

表头排序箭头显隐。输入:`[0] GridViewColumnHeader.Column`,`[1] SortMemberPath`,`[2] SortDirection`;`ConverterParameter` 为 `"Ascending"` 或 `"Descending"`,与当前排序状态匹配时返回 `Visible`。

### Carousel 系列

供 `Carousel` 模板内部使用,通常无需直接使用:

| 转换器 | 说明 |
|---|---|
| `CarouselItemOpacityConverter` | 输入 `[0] SelectedIndex, [1] ItemIndex, [2] ItemsCount`;距离 0→`1.0`,1→`0.9`,其余→`0.5` |
| `CarouselItemZIndexConverter` | 输入 `[0] SelectedIndex, [1] ItemIndex`;返回 `100 - |diff|` |
| `CarouselItemTransformConverter` | 输入 `[0] SelectedIndex, [1] ItemIndex, [2] ItemsCount, [3] HostWidth`;返回含缩放与位移的 `TransformGroup` |
| `CarouselItemVisibilityConverter` | 循环距离 ≤ 2 → `Visible`,否则 `Collapsed` |
