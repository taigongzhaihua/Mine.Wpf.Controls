# 控件:选择

命名空间:`Mine.Wpf.Controls.Controls`

## `CheckBox` / `RadioButton`

Material 3 复选框 / 单选按钮。均继承系统基类,支持 `IsThreeState`(CheckBox)与 `GroupName`(RadioButton)等原生能力。

```xml
<mine:CheckBox Content="同意条款" IsChecked="True" LabelPlacement="Right"/>
<mine:RadioButton Content="方案 A" GroupName="Plan" IsChecked="True"/>
```

| 属性 | 类型 | 默认值 | 说明 |
|---|---|---|---|
| `LabelPlacement` | `LabelPlacement` | `Right` | 标签位置 |

### `LabelPlacement` 枚举

| 值 | 说明 |
|---|---|
| `Left` | 标签在左 |
| `Right` | 标签在右(默认) |
| `Top` | 标签在上 |
| `Bottom` | 标签在下 |

## `Switch`

拨动开关(继承 `ToggleButton`)。`OnIcon` / `OffIcon` 可在滑块中显示 Material Symbols 图标。

```xml
<mine:Switch IsChecked="{Binding IsEnabled}" OnIcon="done" OffIcon="close"/>
```

| 属性 | 类型 | 默认值 | 说明 |
|---|---|---|---|
| `OnIcon` | `string?` | `null` | 开状态滑块图标(Material Symbols 字符) |
| `OffIcon` | `string?` | `null` | 关状态滑块图标 |
| `LabelPlacement` | `LabelPlacement` | `Right` | 标签位置 |

---

## `Slider`

Material 3 滑块,支持值气泡与刻度点。

```xml
<mine:Slider Minimum="0" Maximum="100" Value="{Binding Volume}"
             ShowValueLabel="True" ValueFormat="{}{0:0}%"/>
```

| 属性 | 类型 | 默认值 | 说明 |
|---|---|---|---|
| `ShowValueLabel` | `bool` | `false` | 显示值气泡 |
| `ShowStopIndicators` | `bool` | `false` | 显示刻度点 |
| `ValueFormat` | `string` | `"{0:0}"` | 值标签格式 |
| `FormattedValue` | `string`(只读) | `"0"` | 格式化后的显示值 |

## `RangeSlider`

双端范围滑块:拖拽两个滑块选取区间。

```xml
<mine:RangeSlider Minimum="0" Maximum="100"
                  RangeStart="{Binding Min}" RangeEnd="{Binding Max}"
                  StartValueLabelMode="Always" EndValueLabelMode="Always"/>
```

### 属性

| 属性 | 类型 | 默认值 | 说明 |
|---|---|---|---|
| `Minimum` | `double` | `0` | 最小值 |
| `Maximum` | `double` | `100` | 最大值 |
| `RangeStart` | `double` | `0` | 区间起始值(双向) |
| `RangeEnd` | `double` | `100` | 区间结束值(双向) |
| `SmallChange` | `double` | `1` | 键盘步进量 |
| `ValueFormat` | `string` | `"0"` | 值显示格式 |
| `StartValueLabelMode` | `RangeSliderValueLabelMode` | `Always` | 起始值标签显隐 |
| `EndValueLabelMode` | `RangeSliderValueLabelMode` | `Always` | 结束值标签显隐 |
| `TickFrequency` | `double` | `0` | 刻度频率 |
| `IsSnapToTickEnabled` | `bool` | `false` | 吸附刻度 |
| `ShowStopIndicators` | `bool` | `false` | 显示刻度点 |
| `FormattedStart` | `string`(只读) | `"0"` | 格式化起始值 |
| `FormattedEnd` | `string`(只读) | `"100"` | 格式化结束值 |

### 事件

| 事件 | 类型 | 说明 |
|---|---|---|
| `RangeChanged` | `RoutedEventHandler` | 区间变化 |

### `RangeSliderValueLabelMode`

| 值 | 说明 |
|---|---|
| `Hidden` | 不显示 |
| `Hover` | 悬停时显示 |
| `Drag` | 拖拽时显示 |
| `HoverOrDrag` | 悬停或拖拽时显示 |
| `Always` | 始终显示(默认) |

---

## `Rating`

星级评分。

```xml
<mine:Rating Value="{Binding Score}" Maximum="5" AllowHalfStar="True"/>
```

| 属性 | 类型 | 默认值 | 说明 |
|---|---|---|---|
| `Value` | `double` | `0` | 评分(钳制到 `[0, Maximum]`) |
| `Maximum` | `int` | `5` | 星星数量 |
| `IsReadOnly` | `bool` | `false` | 只读 |
| `AllowHalfStar` | `bool` | `false` | 允许半星 |

### 事件

| 事件 | 类型 | 说明 |
|---|---|---|
| `ValueChanged` | `RoutedPropertyChangedEventHandler<double>` | 评分变化 |

---

## `DatePicker`

日期选择器:弹出日历,支持单日期 / 日期范围。

```xml
<!-- 单日期 -->
<mine:DatePicker SelectedDate="{Binding Date}" SelectionMode="Single"/>
<!-- 范围 -->
<mine:DatePicker SelectionMode="Range"
                 StartDate="{Binding From}" EndDate="{Binding To}"/>
```

### 属性

| 属性 | 类型 | 默认值 | 说明 |
|---|---|---|---|
| `Variant` | `TextFieldVariant` | `Outlined` | 变体 |
| `SelectionMode` | `DatePickerSelectionMode` | `Single` | 选择模式 |
| `SelectedDate` | `DateTime?` | `null` | 选中日期(Single 模式) |
| `StartDate` | `DateTime?` | `null` | 范围起始(Range 模式) |
| `EndDate` | `DateTime?` | `null` | 范围结束(Range 模式) |
| `DisplayFormat` | `string` | `"yyyy-MM-dd"` | 显示格式 |
| `Hint` | `string?` | `"选择日期"` | 浮动标签 |
| `HelperText` | `string?` | `""` | 辅助文本 |
| `MinimumDate` | `DateTime?` | `null` | 最小可选日期 |
| `MaximumDate` | `DateTime?` | `null` | 最大可选日期 |
| `FirstDayOfWeek` | `DayOfWeek` | `Monday` | 每周第一天 |
| `IsOpen` | `bool` | `false` | 弹出层是否打开(双向) |
| `DisplayText` | `string`(只读) | `""` | 显示文本 |
| `HasSelectedDate` | `bool`(只读) | `false` | 是否已选日期 |
| `CalendarDays` | `IReadOnlyList<CalendarDayItem>`(只读) | 空 | 日历单元格(供自定义模板) |
| `WeekdayNames` | `IReadOnlyList<string>`(只读) | 空 | 星期标题 |

> 模板数据类 `CalendarDayItem` 公开成员:`Date`、`Label`、`IsToday`、`IsSelected`、`IsConfirmed`、`IsOutsideMonth`、`IsEnabled`、`IsInRange`、`IsRangeStart`、`IsRangeEnd`(供自定义日历模板绑定)。

### 事件

| 事件 | 类型 | 说明 |
|---|---|---|
| `SelectedDateChanged` | `RoutedPropertyChangedEventHandler<DateTime?>` | 选中日期变化 |
| `DateRangeChanged` | `RoutedEventHandler` | 范围变化 |

### `DatePickerSelectionMode`

| 值 | 说明 |
|---|---|
| `Single` | 单日期(默认) |
| `Range` | 日期范围 |

---

## `TimePicker`

时间选择器:环形时钟盘选择时/分/秒,支持 12/24 小时制与毫秒精度。

```xml
<mine:TimePicker SelectedTime="{Binding Time}" Is24Hours="True"
                 Precision="Second"/>
```

### 属性

| 属性 | 类型 | 默认值 | 说明 |
|---|---|---|---|
| `Variant` | `TextFieldVariant` | `Outlined` | 变体 |
| `SelectedTime` | `TimeSpan?` | `null` | 选中时间(按 `Precision` 截断) |
| `DisplayFormat` | `string` | `"HH:mm:ss.fff"` | 显示格式 |
| `Hint` | `string?` | `"选择时间"` | 浮动标签 |
| `HelperText` | `string?` | `""` | 辅助文本 |
| `Is24Hours` | `bool` | `true` | 24 小时制 |
| `MinuteInterval` | `int` | `1` | 分钟间隔(1–59) |
| `SecondInterval` | `int` | `1` | 秒间隔(1–59) |
| `MillisecondInterval` | `int` | `1` | 毫秒间隔(1–999) |
| `Precision` | `TimePickerPrecision` | `Millisecond` | 选择精度 |
| `ClockMode` | `TimePickerClockMode` | `Hour` | 时钟盘当前模式 |
| `IsOpen` | `bool` | `false` | 弹出层是否打开 |
| `DisplayText` | `string`(只读) | `""` | 显示文本 |
| `HasSelectedTime` | `bool`(只读) | `false` | 是否已选时间 |

> 模板数据类 `TimePickerClockItem` 公开成员:`Value`、`Label`、`IsSelected`、`IsMajorLabel`、`IsMajorTick`、`X`、`Y`、`Size`(供自定义时钟盘模板绑定)。

### 事件

| 事件 | 类型 | 说明 |
|---|---|---|
| `SelectedTimeChanged` | `RoutedPropertyChangedEventHandler<TimeSpan?>` | 选中时间变化 |

### `TimePickerPrecision`

| 值 | 说明 |
|---|---|
| `Minute` | 精确到分 |
| `Second` | 精确到秒 |
| `Millisecond` | 精确到毫秒(默认) |

---

## `ColorPicker`

颜色选择器:色相滑块、饱和度/亮度面板、透明度滑块、预设色板与屏幕取色,支持 Hex 输入。

```xml
<mine:ColorPicker SelectedColor="{Binding Color}" ShowAlpha="True"/>
```

### 属性

| 属性 | 类型 | 默认值 | 说明 |
|---|---|---|---|
| `SelectedColor` | `Color` | `Red` | 选中颜色(双向) |
| `ShowAlpha` | `bool` | `false` | 显示透明度滑块 |
| `PresetColors` | `IEnumerable<Color>` | MD3 参考色板(21 色) | 预设色集合 |
| `HueGradient` | `LinearGradientBrush`(只读) | — | 色相滑块背景 |

### 事件

| 事件 | 类型 | 说明 |
|---|---|---|
| `SelectedColorChanged` | `RoutedPropertyChangedEventHandler<Color>` | 选中颜色变化 |

> 与设置页的典型组合:`SettingsColorItem` + `FlyoutService.Flyout` 挂载 `ColorPicker`,见[设置与数据](settings-data.md)。

---

## `Stepper`

步骤条:展示有序步骤进度,由 `ActiveIndex` 控制激活步骤。

```xml
<mine:Stepper ActiveIndex="1">
    <mine:StepperItem Label="选择"   Description="选择文件"/>
    <mine:StepperItem Label="配置"   Description="设置参数"/>
    <mine:StepperItem Label="完成"   Description="执行任务"/>
</mine:Stepper>
```

### `Stepper` 属性

| 属性 | 类型 | 默认值 | 说明 |
|---|---|---|---|
| `Orientation` | `StepperOrientation` | `Horizontal` | 方向 |
| `ActiveIndex` | `int` | `0` | 激活步骤索引(0 起) |

### `StepperItem` 属性

| 属性 | 类型 | 默认值 | 说明 |
|---|---|---|---|
| `Label` | `string` | `""` | 标题 |
| `Description` | `string` | `""` | 说明(可选) |
| `Status` | `StepperItemStatus`(只读) | `Pending` | 状态,由父级自动设置 |
| `StepIndex` | `int`(只读) | `1` | 序号(1 起) |
| `IsLastStep` | `bool`(只读) | `false` | 是否末项(隐藏连接线) |

### 枚举

`StepperOrientation`:`Horizontal` / `Vertical`
`StepperItemStatus`:`Pending`(待处理)/ `Active`(进行中)/ `Completed`(已完成)
